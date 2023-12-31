using Sandbox;

public sealed class VisualizerReactor : Component
{
	[Property] public MusicManager MusicComponent { get; set; }

	[Property] public Vector3 PositionFactor { get; set; } = Vector3.Zero;
	[Property] public Vector3 ScaleFactor { get; set; } = Vector3.Zero;
	[Property] public Angles RotationFactor { get; set; } = Angles.Zero;
	[Property] ReactorMode Mode { get; set; } = ReactorMode.Energy;

	public Vector3 StartingPosition;
	public Vector3 StartingScale;
	public Angles StartingRotation;

	protected override void OnStart()
	{
		StartingPosition = Transform.LocalPosition;
		StartingScale = Transform.LocalScale;
		StartingRotation = Transform.LocalRotation;
	}

	protected override void OnUpdate()
	{
		if ( !MusicComponent.IsValid() ) return;

		float factor = GetFactor();
		Transform.LocalPosition = StartingPosition + PositionFactor * factor;
		Transform.LocalScale = StartingScale + ScaleFactor * factor;
		Transform.LocalRotation = StartingRotation + RotationFactor * factor;
	}

	float GetFactor()
	{
		switch ( Mode )
		{
			case ReactorMode.Energy:
				return MusicComponent.Energy;
			case ReactorMode.HistoryAverage:
				return MusicComponent.EnergyHistoryAverage;
			case ReactorMode.TargetEnergy:
				return MusicComponent.EnergyHistoryAverage + MusicComponent.AdjustedPeakThreshold;
			default:
				return 0f;
		}
	}

	enum ReactorMode
	{
		Energy,
		HistoryAverage,
		TargetEnergy
	}
}