using Sandbox;

public sealed class PositionMusicHere : Component
{
	[Property] MusicManager MusicComponent { get; set; }

	protected override void OnUpdate()
	{
		if ( MusicComponent.IsValid() )
		{
			MusicComponent.Position = Transform.Position;
		}
	}
}