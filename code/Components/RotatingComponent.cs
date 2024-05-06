using Sandbox;

namespace CUtils.Components;

[Category("CUtils")]
[Title("Rotator Component")]
[Icon("360")]
public class RotatingComponent : Component
{
    [Property] public Angles RotateSpeed { get; set; }

    protected override void OnUpdate()
    {
        base.OnUpdate();

        var angles = Transform.Rotation.Angles();
        angles.pitch += Time.Delta * RotateSpeed.pitch;
        angles.yaw += Time.Delta * RotateSpeed.yaw;
        angles.roll += Time.Delta * RotateSpeed.roll;
        Transform.Rotation = Rotation.From(angles);
    }
}