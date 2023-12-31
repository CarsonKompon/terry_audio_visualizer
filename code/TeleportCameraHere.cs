using Sandbox;

public sealed class TeleportCameraHere : Component
{
	[Property] CameraComponent CameraComponent { get; set; }

	protected override void OnUpdate()
	{
		CameraComponent.Transform.Position = Transform.Position;
		CameraComponent.Transform.Rotation = Transform.Rotation;
	}
}