namespace Kiyote.Geometry.DelaunayVoronoi;

using System.Linq;

// Ported from: https://github.com/d3/d3-delaunay/blob/main/src/voronoi.js
/*
Copyright 2018-2021 Observable, Inc.
Copyright 2021 Mapbox

Permission to use, copy, modify, and/or distribute this software for any purpose
with or without fee is hereby granted, provided that the above copyright notice
and this permission notice appear in all copies.

THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES WITH
REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF MERCHANTABILITY AND
FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY SPECIAL, DIRECT,
INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES WHATSOEVER RESULTING FROM LOSS
OF USE, DATA OR PROFITS, WHETHER IN AN ACTION OF CONTRACT, NEGLIGENCE OR OTHER
TORTIOUS ACTION, ARISING OUT OF OR IN CONNECTION WITH THE USE OR PERFORMANCE OF
THIS SOFTWARE.
*/

internal sealed class D3VoronoiFactory : IVoronoiFactory {

	private const int MaximumExpectedPointsPerCell = 25;
	private const int MaximumExpectedNeighbours = 10;

	IVoronoi IVoronoiFactory.Create(
		Rect bounds,
		IReadOnlyList<Point> points
	) {
		return ( this as IVoronoiFactory ).Create( bounds, points, false );
	}

	IVoronoi IVoronoiFactory.Create(
		Rect bounds,
		IReadOnlyList<Point> points,
		bool sanitizePoints
	) {
		if( sanitizePoints ) {
			points = points
				.Where( p =>
					p.X >= bounds.X1
					&& p.X <= bounds.X2
					&& p.Y >= bounds.Y1
					&& p.Y <= bounds.Y2
				)
				.Distinct()
				.ToList();
		}
		double[] coords = points.ToCoords();

		MapboxDelaunator delaunator = MapboxDelaunatorFactory.Create( coords );
		D3Delaunay delaunay = D3DelaunayFactory.Create( coords, delaunator );

		double[] circumcenters = CalculateCircumcenters( delaunator, coords );
		double[] vectors = CalculateVectors( delaunator, coords );

		double[] cellPointBuffer = new double[MaximumExpectedPointsPerCell * 2];
		double[] projectedPointBuffer = new double[cellPointBuffer.Length + 4];

		List<Cell> cells = new List<Cell>( points.Count );
		List<double> cellCoords = new List<double>( MaximumExpectedPointsPerCell * 2 );
		for( int i = 0; i < points.Count; i++ ) {
			bool isOpen = false;
			Clip(
				bounds,
				coords,
				circumcenters,
				delaunator,
				delaunay,
				vectors,
				i,
				cellCoords,
				cellPointBuffer,
				projectedPointBuffer,
				ref isOpen
			);

			if( !cellCoords.Any() ) {
				throw new InvalidOperationException( "Clipped cell contains no points." );
			}

			int minX = int.MaxValue;
			int minY = int.MaxValue;
			int maxX = int.MinValue;
			int maxY = int.MinValue;
			List<Point> cellPoints = new List<Point>();
			for( int j = 0; j < cellCoords.Count / 2; j++ ) {
				int x = (int)cellCoords[( j * 2 ) + 0];
				int y = (int)cellCoords[( j * 2 ) + 1];
				if( x < minX ) {
					minX = x;
				}
				if( x > maxX ) {
					maxX = x;
				}
				if( y < minY ) {
					minY = y;
				}
				if( y > maxY ) {
					maxY = y;
				}
				cellPoints.Add( new Point( x, y ) );
			}
			Polygon polygon = new Polygon(
				cellPoints
			);
			Cell cell = new Cell(
				points[i],
				polygon,
				isOpen,
				new Rect( minX, minY, maxX - minX + 1, maxY - minY + 1 )
			);
			cells.Add( cell );
		}

		ReadOnlySpan<int> inedges = delaunay.Inedges;
		ReadOnlySpan<int> hullIndex = delaunay.HullIndex;
		Dictionary<Cell, IReadOnlyList<Cell>> cellNeighbours = new Dictionary<Cell, IReadOnlyList<Cell>>( points.Count );
		List<Cell> neighbours = new List<Cell>( MaximumExpectedNeighbours );
		for( int i = 0; i < points.Count; i++ ) {
			int e0 = inedges[i];
			if( e0 == -1 ) {
				throw new InvalidOperationException( "Coincident point." );
			}
			int e = e0;
			int p0;
			do {
				p0 = delaunator.Triangles[e];
				neighbours.Add( cells[p0] );

				e = e % 3 == 2 ? e - 2 : e + 1;
				if( delaunator.Triangles[e] != i ) {
					throw new InvalidOperationException( "Bad triangulation." );
				}
				e = delaunator.HalfEdges[e];
				if( e == -1 ) {
					int p = delaunator.Hull[( hullIndex[i] + 1 ) % delaunator.Hull.Length];
					if( p != p0 ) {
						neighbours.Add( cells[p] );
					}
					break;
				}

			} while( e != e0 );
			cellNeighbours[cells[i]] = neighbours.ToList(); // Make a copy
			neighbours.Clear();
		}

		HashSet<Edge> edges = new HashSet<Edge>();
		for( int i = 0; i < delaunator.Triangles.Length; i++ ) {
			if( i < delaunator.HalfEdges[i] ) {
				int t1 = i / 3;
				Point c1 = new Point(
					(int)circumcenters[(t1 * 2) + 0],
					(int)circumcenters[(t1 * 2) + 1]
				);

				int t2 = delaunator.HalfEdges[i] / 3;
				Point c2 = new Point(
					(int)circumcenters[(t2 * 2) + 0],
					(int)circumcenters[(t2 * 2) + 1]
				);

				Edge edge = new Edge( c1, c2 );
				if( !edges.Contains( edge ) ) {
					_ = edges.Add( edge );
				}
			}
		}

		return new Voronoi( points, cells, cellNeighbours, edges.ToList() );
	}

	private static void Clip(
		Rect bounds,
		ReadOnlySpan<double> coords,
		ReadOnlySpan<double> circumcenters,
		MapboxDelaunator delaunator,
		D3Delaunay delaunay,
		ReadOnlySpan<double> vectors,
		int i,
		List<double> cellCoords,
		Span<double> cellPointBuffer,
		Span<double> projectedPointBuffer,
		ref bool isOpen
	) {
		Cell(
			i,
			circumcenters,
			delaunator,
			delaunay,
			cellPointBuffer,
			out int cellPointCount
		);
		// If it's an empty calculation, bail early
		if( cellPointCount == 0 ) {
			throw new InvalidOperationException( "Cell contains no points." );
			//return Array.Empty<double>();
		}
		int v = i * 4;
		if( vectors[v] != 0
			|| vectors[v + 1] != 0
		) {
			ClipInfinite(
				bounds,
				coords,
				delaunator,
				delaunay,
				i,
				cellPointBuffer[..cellPointCount],
				projectedPointBuffer,
				vectors[v + 0],
				vectors[v + 1],
				vectors[v + 2],
				vectors[v + 3],
				cellCoords,
				ref isOpen
			);
		} else {
			ClipFinite(
				bounds,
				coords,
				delaunator,
				delaunay,
				i,
				cellPointBuffer[..cellPointCount],
				cellCoords,
				ref isOpen
			);
		}
		Simplify( cellCoords );
	}

	private static void Simplify(
		List<double> points
	) {
		if( points.Count > 4 ) {
			for( int i = 0; i < points.Count; i += 2 ) {
				int j = ( i + 2 ) % points.Count;
				int k = ( i + 4 ) % points.Count;
				if( ( points[i] == points[j] && points[j] == points[k] )
					|| ( points[i + 1] == points[j + 1]
						&& points[j + 1] == points[k + 1] )
				) {
					points.RemoveRange( j, 2 );
					i -= 2;
				}
			}
			if( points.Count == 0 ) {
				throw new InvalidOperationException( "Cell simplified to no points." );
			}
		}
	}

	private static void ClipInfinite(
		Rect bounds,
		ReadOnlySpan<double> coords,
		MapboxDelaunator delaunator,
		D3Delaunay delaunay,
		int i,
		ReadOnlySpan<double> points,
		Span<double> projectedPointBuffer,
		double vx0,
		double vy0,
		double vxn,
		double vyn,
		List<double> cellCoords, // Re-used list of doubles for cell vertices
		ref bool isOpen
	) {
		// Duplicate the points, but make spaces for potential points
		// at the start and end so we don't have to re-allocate
		points.CopyTo( projectedPointBuffer[2..] );
		int startIndex = 2;
		int endIndex = points.Length + 2;

		if( Project( bounds, points[0], points[1], vx0, vy0, out double px, out double py ) ) {
			projectedPointBuffer[0] = px;
			projectedPointBuffer[1] = py;
			startIndex = 0;
		}
		if( Project( bounds, points[^2], points[^1], vxn, vyn, out px, out py ) ) {
			projectedPointBuffer[endIndex + 0] = px;
			projectedPointBuffer[endIndex + 1] = py;
			endIndex += 2;
		}

		cellCoords.Clear();
		ClipFinite(
				bounds,
				coords,
				delaunator,
				delaunay,
				i,
				projectedPointBuffer[startIndex..endIndex],
				cellCoords,
				ref isOpen
			);
		if( cellCoords.Any() ) {
			int n = cellCoords.Count;
			int c0;
			int c1 = EdgeCode( bounds, cellCoords[^2], cellCoords[^1] );
			for( int j = 0; j < n; j += 2 ) {
				c0 = c1;
				c1 = EdgeCode( bounds, cellCoords[j], cellCoords[j + 1] );
				if( c0 != 0
					&& c1 != 0
				) {
					isOpen = true;
					j = Edge(
						bounds,
						coords,
						delaunator,
						delaunay,
						i,
						c0,
						c1,
						cellCoords,
						j
					);
					n = cellCoords.Count;
				}
			}
		} else if(
			Contains(
				coords,
				delaunator,
				delaunay,
				i,
				( bounds.X1 + bounds.X2 ) / 2,
				( bounds.Y1 + bounds.Y2 ) / 2
		) ) {
			cellCoords.Clear();
			cellCoords.AddRange( new double[] {
				bounds.X1,
				bounds.Y1,
				bounds.X2,
				bounds.Y1,
				bounds.X2,
				bounds.Y2,
				bounds.X1,
				bounds.Y2
			} );
		}
	}

	private static void ClipFinite(
		Rect bounds,
		ReadOnlySpan<double> coords,
		MapboxDelaunator delaunator,
		D3Delaunay delaunay,
		int i,
		ReadOnlySpan<double> points,
		List<double> cellCoords,
		ref bool isOpen
	) {
		int n = points.Length;
		cellCoords.Clear();
		double x0;
		double y0;
		double x1 = points[^2];
		double y1 = points[^1];
		int c0;
		int c1 = RegionCode( bounds, x1, y1 );
		int e0;
		int e1 = 0;

		for( int j = 0; j < n; j += 2 ) {
			x0 = x1;
			y0 = y1;
			x1 = points[j + 0];
			y1 = points[j + 1];
			c0 = c1;
			c1 = RegionCode( bounds, x1, y1 );
			if( c0 == 0 && c1 == 0 ) {
				//e0 = e1;
				e1 = 0;
				cellCoords.Add( x1 );
				cellCoords.Add( y1 );
			} else {
				double sx0;
				double sy0;
				double sx1;
				double sy1;
				if( c0 == 0 ) {
					if( !ClipSegment( bounds, x0, y0, x1, y1, c0, c1, out double _, out double _, out double cx1, out double cy1 ) ) {
						continue;
					}
					sx1 = cx1;
					sy1 = cy1;
					isOpen = true;
				} else {
					if( !ClipSegment( bounds, x1, y1, x0, y0, c1, c0, out double cx0, out double cy0, out double cx1, out double cy1 ) ) {
						continue;
					}
					sx1 = cx0;
					sy1 = cy0;
					sx0 = cx1;
					sy0 = cy1;
					isOpen = true;
					e0 = e1;
					e1 = EdgeCode( bounds, sx0, sy0 );
					if( e0 != 0
						&& e1 != 0
					) {
						_ = Edge(
							bounds,
							coords,
							delaunator,
							delaunay,
							i,
							e0,
							e1,
							cellCoords,
							cellCoords.Count
						);
					}
					cellCoords.Add( sx0 );
					cellCoords.Add( sy0 );
				}
				e0 = e1;
				e1 = EdgeCode( bounds, sx1, sy1 );
				if( e0 != 0
					&& e1 != 0
				) {
					isOpen = true;
					_ = Edge(
						bounds,
						coords,
						delaunator,
						delaunay,
						i,
						e0,
						e1,
						cellCoords,
						cellCoords.Count
					);
				}
				cellCoords.Add( sx1 );
				cellCoords.Add( sy1 );
			}
		}
		if( cellCoords.Any() ) {
			e0 = e1;
			e1 = EdgeCode( bounds, cellCoords[0], cellCoords[1] );
			if( e0 != 0
				&& e1 != 0
			) {
				isOpen = true;
				_ = Edge(
					bounds,
					coords,
					delaunator,
					delaunay,
					i,
					e0,
					e1,
					cellCoords,
					cellCoords.Count
				);
			}
		} else if(
			Contains(
				coords,
				delaunator,
				delaunay,
				i,
				( bounds.X1 + bounds.X2 ) / 2,
				( bounds.Y1 + bounds.Y2 ) / 2
		) ) {
			cellCoords.Clear();
			cellCoords.AddRange( new double[] {
				bounds.X2,
				bounds.Y1,
				bounds.X2,
				bounds.Y2,
				bounds.X1,
				bounds.Y2,
				bounds.X1,
				bounds.Y1
			} );
		}
	}

	private static int Edge(
		Rect bounds,
		ReadOnlySpan<double> coords,
		MapboxDelaunator delaunator,
		D3Delaunay delaunay,
		int i,
		int e0,
		int e1,
		List<double> points,
		int j
	) {
		while( e0 != e1 ) {
			double x;
			double y;
			switch( e0 ) {
				case 0b0101:
					e0 = 0b0100;
					continue;
				case 0b0100:
					e0 = 0b0110;
					x = bounds.X2;
					y = bounds.Y1;
					break;
				case 0b0110:
					e0 = 0b0010;
					continue;
				case 0b0010:
					e0 = 0b1010;
					x = bounds.X2;
					y = bounds.Y2;
					break;
				case 0b1010:
					e0 = 0b1000;
					continue;
				case 0b1000:
					e0 = 0b1001;
					x = bounds.X1;
					y = bounds.Y2;
					break;
				case 0b1001:
					e0 = 0b0001;
					continue;
				case 0b0001:
					e0 = 0b0101;
					x = bounds.X1;
					y = bounds.Y1;
					break;
				default:
					throw new InvalidOperationException( "Unable to calculate edge." );
			}
			if( j + 1 >= points.Count
				|| ( ( points[j] != x
				|| points[j + 1] != y )
				&& Contains(
					coords,
					delaunator,
					delaunay,
					i,
					x,
					y
			) ) ) {
				points.Insert( j, x );
				points.Insert( j + 1, y );
				j += 2;
			}
		}
		return j;
	}

	private static bool Contains(
		ReadOnlySpan<double> coords,
		MapboxDelaunator delaunator,
		D3Delaunay delaunay,
		int i,
		double x,
		double y
	) {
		return ( Step( coords, delaunator, delaunay, i, x, y ) == i );
	}

	private static int Step(
		ReadOnlySpan<double> coords,
		MapboxDelaunator delaunator,
		D3Delaunay delaunay,
		int i,
		double x,
		double y
	) {
		ReadOnlySpan<int> inedges = delaunay.Inedges;
		if( inedges[i] == -1
			|| coords.Length == 0
		) {
			return ( i + 1 ) % ( coords.Length / 2 );
		}
		ReadOnlySpan<int> triangles = delaunator.Triangles;
		ReadOnlySpan<int> halfEdges = delaunator.HalfEdges;
		ReadOnlySpan<int> hull = delaunator.Hull;
		ReadOnlySpan<int> hullIndex = delaunay.HullIndex;
		int c = i;
		double dc = Math.Pow( x - coords[i * 2], 2 ) + Math.Pow( y - coords[( i * 2 ) + 1], 2 );
		int e0 = inedges[i];
		int e = e0;
		do {
			int t = triangles[e];
			double dt = Math.Pow( x - coords[t * 2], 2 ) + Math.Pow( y - coords[( t * 2 ) + 1], 2 );
			if( dt < dc ) {
				dc = dt;
				c = t;
			}
			e = ( e % 3 == 2 ) ? e - 2 : e + 1;
			if( triangles[e] != i ) {
				throw new InvalidOperationException( "Bad triangulation." );
			}
			e = halfEdges[e];
			if( e == -1 ) {
				e = hull[( hullIndex[i] + 1 ) % hull.Length];
				if( e != t ) {
					if( ( Math.Pow( x - coords[e * 2], 2 )
						+ Math.Pow( y - coords[( e * 2 ) + 1], 2 ) ) < dc
					) {
						return e;
					}
				}
				break;
			}

		} while( e != e0 );
		return c;
	}

	private static bool ClipSegment(
		Rect bounds,
		double x0,
		double y0,
		double x1,
		double y1,
		int c0,
		int c1,
		out double cx0,
		out double cy0,
		out double cx1,
		out double cy1
	) {
		bool flip = c0 < c1;
		if( flip ) {
			double x = x0;
			double y = y0;
			int c = c0;

			x0 = x1;
			y0 = y1;
			c0 = c1;
			x1 = x;
			y1 = y;
			c1 = c;
		}

		while( true ) {
			if( c0 == 0
				&& c1 == 0
			) {
				if( flip ) {
					cx0 = x1;
					cy0 = y1;
					cx1 = x0;
					cy1 = y0;
					return true;
				} else {
					cx0 = x0;
					cy0 = y0;
					cx1 = x1;
					cy1 = y1;
					return true;
				}
			}
			if( ( c0 & c1 ) != 0 ) {
				cx0 = 0;
				cy0 = 0;
				cx1 = 0;
				cy1 = 0;
				return false;
			}
			double x;
			double y;
			int c = c0 != 0 ? c0 : c1;
			if( ( c & 0b1000 ) != 0 ) {
				x = x0 + ( ( x1 - x0 ) * ( bounds.Y2 - y0 ) / ( y1 - y0 ) );
				y = bounds.Y2;
			} else if( ( c & 0b0100 ) != 0 ) {
				x = x0 + ( ( x1 - x0 ) * ( bounds.Y1 - y0 ) / ( y1 - y0 ) );
				y = bounds.Y1;
			} else if( ( c & 0b0010 ) != 0 ) {
				y = y0 + ( ( y1 - y0 ) * ( bounds.X2 - x0 ) / ( x1 - x0 ) );
				x = bounds.X2;
			} else {
				y = y0 + ( ( y1 - y0 ) * ( bounds.X1 - x0 ) / ( x1 - x0 ) );
				x = bounds.X1;
			}
			if( c0 != 0 ) {
				x0 = x;
				y0 = y;
				c0 = RegionCode( bounds, x0, y0 );
			} else {
				x1 = x;
				y1 = y;
				c1 = RegionCode( bounds, x1, y1 );
			}
		}
	}

	private static int RegionCode(
		Rect bounds,
		double x,
		double y
	) {
		int code = 0b0000;

		if( x < bounds.X1 ) {
			code |= 0b0001;
		} else if( x > bounds.X2 ) {
			code |= 0b0010;
		}

		if( y < bounds.Y1 ) {
			code |= 0b0100;
		} else if( y > bounds.Y2 ) {
			code |= 0b1000;
		}

		return code;
	}

	private static int EdgeCode(
		Rect bounds,
		double x,
		double y
	) {
		int code = 0b0000;

		if( x == bounds.X1 ) {
			code |= 0b0001;
		} else if( x == bounds.X2 ) {
			code |= 0b0010;
		}

		if( y == bounds.Y1 ) {
			code |= 0b0100;
		} else if( y == bounds.Y2 ) {
			code |= 0b1000;
		}

		return code;
	}

	private static bool Project(
		Rect bounds,
		double x0,
		double y0,
		double vx,
		double vy,
		out double x,
		out double y
	) {
		double t = double.PositiveInfinity;
		double c;
		x = 0;
		y = 0;
		if( vy < 0 ) { // top
			if( y0 <= bounds.Y1 ) {
				return false;
			}
			c = ( bounds.Y1 - y0 ) / vy;
			if( c < t ) {
				y = bounds.Y1;
				t = c;
				x = x0 + ( t * vx );
			}
		} else if( vy > 0 ) {
			if( y0 >= bounds.Y2 ) {
				return false;
			}
			c = ( bounds.Y2 - y0 ) / vy;
			if( c < t ) {
				y = bounds.Y2;
				t = c;
				x = x0 + ( t * vx );
			}
		}

		if( vx > 0 ) {
			if( x0 >= bounds.X2 ) {
				return false;
			}
			c = ( bounds.X2 - x0 ) / vx;
			if( c < t ) {
				x = bounds.X2;
				t = c;
				y = y0 + ( t * vy );
			}
		} else if( vx < 0 ) {
			if( x0 <= bounds.X1 ) {
				return false;
			}
			c = ( bounds.X1 - x0 ) / vx;
			if( c < t ) {
				x = bounds.X1;
				t = c;
				y = y0 + ( t * vy );
			}
		}
		return true;
	}

	private static void Cell(
		int i,
		ReadOnlySpan<double> circumcenters,
		MapboxDelaunator delaunator,
		D3Delaunay delaunay,
		Span<double> cellPointBuffer,
		out int cellPointCount
	) {
		ReadOnlySpan<int> triangles = delaunator.Triangles;
		ReadOnlySpan<int> halfEdges = delaunator.HalfEdges;
		ReadOnlySpan<int> inedges = delaunay.Inedges;
		int e0 = inedges[i];
		if( e0 == -1 ) {
			throw new InvalidOperationException( "Coincident points." );
			// return Array.Empty<double>();
		}
		int e = e0;
		cellPointCount = 0;
		do {
			int t = e / 3;
			cellPointBuffer[cellPointCount + 0] = circumcenters[( t * 2 ) + 0];
			cellPointBuffer[cellPointCount + 1] = circumcenters[( t * 2 ) + 1];
			cellPointCount += 2;
			e = ( e % 3 ) == 2 ? e - 2 : e + 1;
			if( triangles[e] != i ) {
				throw new InvalidOperationException( "Bad triangulation." );
			}
			e = halfEdges[e];
		} while( e != e0 && e != -1 );
	}

	private static double[] CalculateVectors(
		MapboxDelaunator delaunator,
		ReadOnlySpan<double> coords
	) {
		ReadOnlySpan<int> hull = delaunator.Hull;
		// Two vectors for each point
		double[] vectors = new double[coords.Length * 2];
		int h = hull[^1];
		int p0;
		int p1 = h * 4;
		double x0;
		double x1 = coords[( 2 * h ) + 0];
		double y0;
		double y1 = coords[( 2 * h ) + 1];

		for( int i = 0; i < hull.Length; i++ ) {
			h = hull[i];
			p0 = p1;
			x0 = x1;
			y0 = y1;
			p1 = h * 4;
			x1 = coords[( 2 * h ) + 0];
			y1 = coords[( 2 * h ) + 1];
			double v0 = y0 - y1;
			double v1 = x1 - x0;
			vectors[p0 + 2] = vectors[p1 + 0] = v0;
			vectors[p0 + 3] = vectors[p1 + 1] = v1;
		}
		return vectors;
	}

	private static double[] CalculateCircumcenters(
		MapboxDelaunator delaunator,
		ReadOnlySpan<double> coords
	) {
		ReadOnlySpan<int> triangles = delaunator.Triangles;
		// Two coordinates for every triangle
		double[] circumcenters = new double[triangles.Length / 3 * 2];
		int j = 0;
		int n = triangles.Length;
		for( int i = 0; i < n; i += 3 ) {
			int t1 = triangles[i + 0] * 2;
			int t2 = triangles[i + 1] * 2;
			int t3 = triangles[i + 2] * 2;
			double x1 = coords[t1 + 0];
			double y1 = coords[t1 + 1];
			double x2 = coords[t2 + 0];
			double y2 = coords[t2 + 1];
			double x3 = coords[t3 + 0];
			double y3 = coords[t3 + 1];

			double dx = x2 - x1;
			double dy = y2 - y1;
			double ex = x3 - x1;
			double ey = y3 - y1;
			double ab = ( ( dx * ey ) - ( dy * ex ) ) * 2;

			if( Math.Abs( ab ) < 0.0000000001D ) {
				throw new InvalidOperationException( "Degenerate triangle" );
			}
			double d = 1D / ab;
			double b1 = ( dx * dx ) + ( dy * dy );
			double c1 = ( ex * ex ) + ( ey * ey );
			double x = x1 + ( ( ( ey * b1 ) - ( dy * c1 ) ) * d );
			double y = y1 + ( ( ( dx * c1 ) - ( ex * b1 ) ) * d );
			circumcenters[j + 0] = x;
			circumcenters[j + 1] = y;
			j += 2;
		}
		return circumcenters;
	}
}
