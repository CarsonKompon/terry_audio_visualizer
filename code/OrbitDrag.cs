using Sandbox;

public sealed class OrbitDrag : Component
{
	[Property] public float RotationSpeed { get; set; } = 0f;

	Vector2 lastMousePosition = -Vector2.One;

	protected override void OnUpdate()
	{
		var mousePos = Mouse.Position;
		if ( Input.Down( "attack1" ) )
		{
			if ( lastMousePosition == -Vector2.One )
			{
				lastMousePosition = mousePos;
			}

			var delta = mousePos - lastMousePosition;

			RotationSpeed = -delta.x * 10f;
		}
		lastMousePosition = mousePos;

		RotationSpeed = RotationSpeed.LerpTo( 0, Time.Delta * 5.0f );

		if ( RotationSpeed != 0f )
		{
			Transform.LocalRotation *= Rotation.FromYaw( RotationSpeed * Time.Delta );
		}
	}
}