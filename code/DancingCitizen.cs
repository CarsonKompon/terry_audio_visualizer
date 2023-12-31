using System;
using Sandbox;
using Sandbox.Citizen;

public class DancingCitizen : Component
{
    [Property] MusicManager MusicComponent { get; set; }
    [Property] CitizenAnimationHelper AnimationHelper { get; set; }

    int Mode = 0;
    float RotateSpeed = 0f;
    float TargetPose = 0f;
    float CurrentPose = 0f;

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

        CurrentPose = CurrentPose.LerpTo( TargetPose, Time.Delta * 10f );
        AnimationHelper.Target.Set( "holdtype_pose", CurrentPose );
    }

    void OnBeat()
    {
        // AnimationHelper.DuckLevel = (AnimationHelper.DuckLevel + 1) % 2;

        // if ( Mode == 0 )
        // {
        //     HasIK = false;
        //     if ( Random.Shared.Float( 0f, 1f ) < 0.5f )
        //         LookAngles = Angles.Random;
        // }
        // else if ( Mode == 1 )
        // {
        //     HasIK = false;
        //     Velocity = Vector3.Random.Normal * 200f;
        // }
        // else if ( Mode == 2 )
        // {
        //     HasIK = false;
        //     LookAngles = Angles.Random;
        // }
        // else if ( Mode == 3 )
        // {
        //     AnimationHelper.DuckLevel = 0;
        //     var footRotation = Rotation.LookAt( AnimationHelper.GameObject.Transform.Rotation.Backward.WithZ( 0.0f ) );
        //     LeftHandTarget = new Transform( Vector3.Random.Normal * 100f, Rotation.Random );
        //     RightHandTarget = new Transform( Vector3.Random.Normal * 100f, Rotation.Random );
        //     LeftFootTarget = new Transform( Vector3.Random.Normal.WithZ( 0 ) * 15f, footRotation );
        //     RightFootTarget = new Transform( Vector3.Random.Normal.WithZ( 0 ) * 15f, footRotation );
        //     HasIK = true;
        //     if ( Random.Shared.Float( 0f, 1f ) < 0.5f )
        //         LookAngles = Angles.Random;
        // }

        if ( Mode == 0 )
        {
            AnimationHelper.Target.Set( "skid", 0 );
            AnimationHelper.IsNoclipping = false;
            CurrentPose = 0f;
            TargetPose = 0f;
            AnimationHelper.DuckLevel = (AnimationHelper.DuckLevel + 1) % 2;
        }
        else if ( Mode == 1 )
        {
            AnimationHelper.Target.Set( "skid", 0 );
            AnimationHelper.IsNoclipping = false;
            AnimationHelper.DuckLevel = 0;
            CurrentPose = Random.Shared.Float( -1f, 7f );
            TargetPose = CurrentPose + Random.Shared.Float( -1f, 1f );
        }
        else if ( Mode == 2 )
        {
            if ( AnimationHelper.Target.GetFloat( "skid" ) == 0 )
            {
                AnimationHelper.Target.Set( "skid", 1 );
            }
            else
            {
                AnimationHelper.Target.Set( "skid", 0 );
            }
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

    async void NoclipBump()
    {
        AnimationHelper.IsNoclipping = true;
        await GameTask.DelaySeconds( 0.02f );
        AnimationHelper.IsNoclipping = false;
    }
}