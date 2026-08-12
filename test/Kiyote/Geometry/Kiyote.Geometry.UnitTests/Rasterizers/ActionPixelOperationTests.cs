namespace Kiyote.Geometry.Rasterizers.Tests;

[TestFixture]
public sealed class ActionPixelOperationTests {

	[Test]
	public void Pixel_Invoked_ForwardsToDelegate() {
		int capturedX = -1;
		int capturedY = -1;
		ActionPixelOperation action = new ActionPixelOperation( ( x, y ) => {
			capturedX = x;
			capturedY = y;
		} );

		action.Pixel( 3, 7 );

		Assert.Multiple( () => {
			Assert.That( capturedX, Is.EqualTo( 3 ) );
			Assert.That( capturedY, Is.EqualTo( 7 ) );
		} );
	}

	[Test]
	public void Equals_SameDelegate_ReturnsTrue() {
		Action<int, int> pixelAction = ( _, _ ) => { };
		ActionPixelOperation left = new ActionPixelOperation( pixelAction );
		ActionPixelOperation right = new ActionPixelOperation( pixelAction );

		Assert.Multiple( () => {
			Assert.That( left.Equals( right ), Is.True );
			Assert.That( left == right, Is.True );
			Assert.That( left != right, Is.False );
			Assert.That( left.GetHashCode(), Is.EqualTo( right.GetHashCode() ) );
		} );
	}

	[Test]
	public void Equals_DifferentDelegates_ReturnsFalse() {
		ActionPixelOperation left = new ActionPixelOperation( ( _, _ ) => { } );
		ActionPixelOperation right = new ActionPixelOperation( ( _, _ ) => { } );

		Assert.Multiple( () => {
			Assert.That( left.Equals( right ), Is.False );
			Assert.That( left == right, Is.False );
			Assert.That( left != right, Is.True );
		} );
	}

	[Test]
	public void Equals_OtherType_ReturnsFalse() {
		ActionPixelOperation action = new ActionPixelOperation( ( _, _ ) => { } );

		Assert.That( action.Equals( "not a pixel action" ), Is.False );
	}

	[Test]
	public void Equals_BoxedSameDelegate_ReturnsTrue() {
		Action<int, int> pixelAction = ( _, _ ) => { };
		ActionPixelOperation left = new ActionPixelOperation( pixelAction );
		object right = new ActionPixelOperation( pixelAction );

		Assert.That( left.Equals( right ), Is.True );
	}

	[Test]
	public void GetHashCode_Default_DoesNotThrow() {
		// The parameterless default leaves the delegate null, so the hash has to
		// tolerate that rather than dereferencing it.
		ActionPixelOperation action = default;

		Assert.That( action.GetHashCode(), Is.EqualTo( 0 ) );
	}
}
