using System.Collections.Generic;
using Sandbox;
using System;

public sealed class DancePadGridManager : Component
{
	const int SIZE = 5;
	List<ModelRenderer> GridObjects = new();

	[Property] MusicManager MusicComponent { get; set; }
	[Property] public float Amplitude { get; set; } = 8f;
	[Property] public float Reactivity { get; set; } = 0.7f;
	[Property]
	public float Wavyness { get; set; } = 1f;

	float wiggleSpeed = 1f;
	float time = 0f;

	protected override void OnStart()
	{
		foreach ( var child in GameObject.Children )
		{
			var mr = child.Components.Get<ModelRenderer>();
			if ( mr != null )
				GridObjects.Add( mr );
		}

		MusicComponent.OnBeat += OnBeat;
	}

	void OnBeat()
	{
		wiggleSpeed = 6f;

		foreach ( var obj in GridObjects )
		{
			obj.Tint = Color.Random;
		}
	}

	protected override void OnUpdate()
	{
		time += Time.Delta * wiggleSpeed;
		wiggleSpeed = wiggleSpeed.LerpTo( 1f, Time.Delta * 6f );

		var energy = MusicComponent.Energy;
		var combined = MusicComponent.EnergyHistoryAverage + MusicComponent.AdjustedPeakThreshold;
		var average = MusicComponent.EnergyHistoryAverage;

		var min = MathF.Min( MathF.Min( energy, combined ), average );
		var max = MathF.Max( MathF.Max( energy, combined ), average );

		energy = MathX.Remap( energy, min, max, 0f, 1f );
		combined = MathX.Remap( combined, min, max, 0f, 1f );
		average = MathX.Remap( average, min, max, 0f, 1f );

		foreach ( var obj in GridObjects )
		{
			var x = obj.Transform.LocalPosition.x;
			var y = obj.Transform.LocalPosition.y;
			var z = MathF.Sin( time * 3f + (MathF.Abs( x ) + MathF.Abs( y )) * Wavyness ) * Amplitude;
			var targetPos = new Vector3( x, y, z );
			if ( MathF.Abs( x ) == 64f || MathF.Abs( y ) == 64f )
			{
				targetPos.z += energy * Amplitude * Reactivity;
			}
			else if ( MathF.Abs( x ) == 32f || MathF.Abs( y ) == 32f )
			{
				targetPos.z += combined * Amplitude * Reactivity;
			}
			else
			{
				targetPos.z += average * Amplitude * Reactivity;
			}

			obj.Transform.LocalPosition = obj.Transform.LocalPosition.LerpTo( targetPos, Time.Delta * 10f );
			obj.Tint = Color.Lerp( obj.Tint, Color.White, Time.Delta * 1f );
		}
	}
}