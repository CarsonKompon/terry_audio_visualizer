using System;
using System.Collections.Generic;
using Sandbox;

public sealed class RandomCameraSwitcher : Component
{
	[Property] MusicManager MusicComponent { get; set; }
	[Property] float Chance { get; set; } = 0.1f;
	[Property] float MinimumTimeBetweenSwitches { get; set; } = 15f;
	RealTimeSince TimeSinceLastSwitch = 0f;

	protected override void OnStart()
	{
		MusicComponent.OnBeat += OnBeat;
	}

	void OnBeat()
	{
		if ( Random.Shared.Float( 0f, 1f ) < Chance || TimeSinceLastSwitch > MinimumTimeBetweenSwitches )
		{
			foreach ( var child in GameObject.Children )
			{
				child.Enabled = false;
			}
			int index = Random.Shared.Next( 0, GameObject.Children.Count );
			GameObject.Children[index].Enabled = true;
			TimeSinceLastSwitch = 0f;
		}
	}
}