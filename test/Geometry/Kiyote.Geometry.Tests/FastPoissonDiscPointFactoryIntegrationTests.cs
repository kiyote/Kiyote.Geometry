﻿using Kiyote.Geometry.Randomization;

namespace Kiyote.Geometry.Tests;

[TestFixture]
public sealed class FastPoissonDiscPointFactoryIntegrationTests
{
	private IRandom _random;
	private IPointFactory _pointFactory;

	[SetUp]
	public void SetUp() {
		_random = new FastRandom();

		_pointFactory = new FastPoissonDiscPointFactory(
			_random
		);
	}

	[Test]
	public void Fill_ValidBounds_AreaFilled() {
		Bounds bounds = new Bounds( 100, 100 );
		IReadOnlyList<IPoint> points = _pointFactory.Fill( bounds, 5 );

		Assert.IsNotEmpty( points );
	}
}
