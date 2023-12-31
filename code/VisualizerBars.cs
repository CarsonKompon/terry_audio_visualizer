using System;
using System.Collections.Generic;
using Sandbox;

public sealed class VisualizerBarManager : Component
{
	[Property] MusicManager MusicComponent { get; set; }
	[Property] GameObject Prefab { get; set; }
	[Property] public float BarWidth { get; set; } = 0.1f;

	List<GameObject> Bars = new();

	float wiggleSpeed = 1f;
	float time = 0f;

	protected override void OnStart()
	{
		base.OnStart();

		float amount = 512f;

		for ( int i = 0; i < 512; i++ )
		{
			var bar = SceneUtility.Instantiate( Prefab );
			bar.Parent = GameObject;
			bar.Transform.LocalPosition = new Vector3( MathF.Sin( i / amount * (2 * MathF.PI) ) * 400, MathF.Cos( i / amount * (2 * MathF.PI) ) * 400, 0 );
			bar.Enabled = true;
			Bars.Add( bar );
		}

		MusicComponent.OnBeat += OnBeat;
	}

	protected override void OnUpdate()
	{
		if ( MusicComponent == null ) return;
		if ( !MusicComponent.IsPlaying ) return;

		time += Time.Delta * wiggleSpeed;
		wiggleSpeed = wiggleSpeed.LerpTo( 1f, Time.Delta * 10f );

		var spectrum = MusicComponent.Spectrum;

		for ( int i = 0; i < Bars.Count; i++ )
		{
			var index = i;
			if ( i > Bars.Count / 2 )
			{
				index = Bars.Count - i;
			}
			var value = (spectrum[index] + spectrum[index + 1] + spectrum[index + 2] + spectrum[index + 3]) / 4f;
			var bar = Bars[i];
			var targetScale = new Vector3( BarWidth, BarWidth, value / 10f );
			bar.Transform.LocalScale = bar.Transform.LocalScale.LerpTo( targetScale, Time.Delta * 10f );
			bar.Transform.LocalPosition = new Vector3( bar.Transform.LocalPosition.x, bar.Transform.LocalPosition.y, value + (MathF.Sin( time + (i / 128f) * (2 * MathF.PI) ) * 32f) );
		}
	}

	void OnBeat()
	{
		wiggleSpeed = 10f;
	}
}