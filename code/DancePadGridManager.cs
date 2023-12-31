using System.Collections.Generic;
using Sandbox;
using System;

public sealed class DancePadGridManager : Component
{
	const int SIZE = 5;
	List<ModelRenderer> GridObjects = new();

	[Property] MusicManager MusicComponent { get; set; }

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

		foreach ( var obj in GridObjects )
		{
			var x = obj.Transform.LocalPosition.x;
			var y = obj.Transform.LocalPosition.y;
			var z = MathF.Sin( time * 3f + MathF.Abs( x ) + MathF.Abs( y ) ) * 8f;
			var targetPos = new Vector3( x, y, z );
			obj.Transform.LocalPosition = obj.Transform.LocalPosition.LerpTo( targetPos, Time.Delta * 10f );
			obj.Tint = Color.Lerp( obj.Tint, Color.White, Time.Delta * 1f );
		}
	}
}