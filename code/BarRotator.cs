using Sandbox;

public sealed class BarRotator : Component
{
	[Property] public float RotateSpeed { get; set; } = 0f;
	protected override void OnUpdate()
	{
		if ( RotateSpeed == 0f ) return;
		Transform.LocalRotation *= Rotation.FromYaw( RotateSpeed * Time.Delta );
	}
}