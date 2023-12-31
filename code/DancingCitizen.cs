using System;
using Sandbox;
using Sandbox.Citizen;

public class DancingCitizen : Component
{
    [Property] MusicManager MusicComponent { get; set; }
    [Property] CitizenAnimationHelper AnimationHelper { get; set; }

    [Property] GameObject LeftHandObject { get; set; }
    [Property] GameObject RightHandObject { get; set; }
    [Property] GameObject LeftFootObject { get; set; }
    [Property] GameObject RightFootObject { get; set; }
    Transform LeftHandTarget;
    Transform RightHandTarget;
    Transform LeftFootTarget;
    Transform RightFootTarget;

    int Mode = 0;
    float RotateSpeed = 0f;

    Vector3 Velocity = Vector3.Zero;
    Angles LookAngles = Angles.Zero;
    bool HasIK = false;

    protected override void OnStart()
    {
        MusicComponent.OnBeat += OnBeat;
        LookAngles = AnimationHelper.GameObject.Transform.Rotation.Angles();
    }

    protected override void OnUpdate()
    {
        AnimationHelper.GameObject.Transform.LocalRotation *= Rotation.FromYaw( RotateSpeed * Time.Delta );
        AnimationHelper.WithVelocity( Velocity );
        AnimationHelper.WithLook( LookAngles.Forward );
        Velocity = Velocity.LerpTo( Vector3.Zero, Time.Delta * 1f );
        LookAngles = LookAngles.LerpTo( AnimationHelper.GameObject.Transform.Rotation.Angles(), Time.Delta * 1f );

        if ( HasIK )
        {
            AnimationHelper.IkLeftHand = LeftHandObject;
            AnimationHelper.IkRightHand = RightHandObject;
            AnimationHelper.IkLeftFoot = LeftFootObject;
            AnimationHelper.IkRightFoot = RightFootObject;
            LeftHandObject.Transform.LocalPosition = LeftHandObject.Transform.LocalPosition.LerpTo( LeftHandTarget.Position, Time.Delta * 10f );
            LeftHandObject.Transform.LocalRotation = Rotation.Lerp( LeftHandObject.Transform.LocalRotation, LeftHandTarget.Rotation, Time.Delta * 10f );
            RightHandObject.Transform.LocalPosition = RightHandObject.Transform.LocalPosition.LerpTo( RightHandTarget.Position, Time.Delta * 10f );
            RightHandObject.Transform.LocalRotation = Rotation.Lerp( RightHandObject.Transform.LocalRotation, RightHandTarget.Rotation, Time.Delta * 10f );
            LeftFootObject.Transform.LocalPosition = LeftFootObject.Transform.LocalPosition.LerpTo( LeftFootTarget.Position, Time.Delta * 10f );
            LeftFootObject.Transform.LocalRotation = Rotation.Lerp( LeftFootObject.Transform.LocalRotation, LeftFootTarget.Rotation, Time.Delta * 10f );
            RightFootObject.Transform.LocalPosition = RightFootObject.Transform.LocalPosition.LerpTo( RightFootTarget.Position, Time.Delta * 10f );
            RightFootObject.Transform.LocalRotation = Rotation.Lerp( RightFootObject.Transform.LocalRotation, RightFootTarget.Rotation, Time.Delta * 10f );
        }
        else
        {
            AnimationHelper.IkLeftHand = null;
            AnimationHelper.IkRightHand = null;
            AnimationHelper.IkLeftFoot = null;
            AnimationHelper.IkRightFoot = null;
        }
    }

    void OnBeat()
    {
        AnimationHelper.DuckLevel = (AnimationHelper.DuckLevel + 1) % 2;

        if ( Mode == 0 )
        {
            HasIK = false;
            if ( Random.Shared.Float( 0f, 1f ) < 0.5f )
                LookAngles = Angles.Random;
        }
        else if ( Mode == 1 )
        {
            HasIK = false;
            Velocity = Vector3.Random.Normal * 200f;
        }
        else if ( Mode == 2 )
        {
            HasIK = false;
            LookAngles = Angles.Random;
        }
        else if ( Mode == 3 )
        {
            AnimationHelper.DuckLevel = 0;
            var footRotation = Rotation.LookAt( AnimationHelper.GameObject.Transform.Rotation.Backward.WithZ( 0.0f ) );
            LeftHandTarget = new Transform( Vector3.Random.Normal * 100f, Rotation.Random );
            RightHandTarget = new Transform( Vector3.Random.Normal * 100f, Rotation.Random );
            LeftFootTarget = new Transform( Vector3.Random.Normal.WithZ( 0 ) * 15f, footRotation );
            RightFootTarget = new Transform( Vector3.Random.Normal.WithZ( 0 ) * 15f, footRotation );
            HasIK = true;
            if ( Random.Shared.Float( 0f, 1f ) < 0.5f )
                LookAngles = Angles.Random;
        }

        if ( Random.Shared.Float( 0f, 1f ) < 0.1f )
        {
            Mode = (Mode + 1) % 3;
        }

        if ( Random.Shared.Float( 0f, 1f ) < 0.3f )
        {
            if ( Random.Shared.Float( 0f, 1f ) < 0.4f )
                RotateSpeed = Random.Shared.Float( -70f, 70f );
            else
                RotateSpeed = 0f;
        }
    }
}