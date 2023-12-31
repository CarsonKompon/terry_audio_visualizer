using System;
using System.Collections.Generic;
using Sandbox;


public class MusicManager : Component
{
    [Property] public string Url { get; set; }
    [Property] public bool Loop { get; set; } = false;

    public bool IsPlaying => MusicPlayer != null;
    public ReadOnlySpan<float> Spectrum => IsPlaying ? MusicPlayer.Spectrum : null;
    public Vector3 Position
    {
        get => MusicPlayer.Position;
        set => MusicPlayer.Position = value;
    }

    public bool IsPeaking { get; private set; } = false;
    public Action OnBeat { get; set; }


    public float Energy { get; private set; } = 0f;
    public float EnergyHistoryAverage { get; private set; } = 0f;
    public float PeakKickVolume { get; private set; } = 0f;
    List<float> EnergyHistory = new();

    MusicPlayer MusicPlayer;

    protected override void OnUpdate()
    {
        if ( MusicPlayer == null ) return;

        var spectrum = MusicPlayer.Spectrum;


        // Energy Calculations
        var energy = 0f;
        for ( int i = 0; i < 512; i++ )
        {
            energy += spectrum[i];
        }
        energy /= 512f;
        Energy = Energy.LerpTo( energy, Time.Delta * 30f );

        EnergyHistory.Add( energy );
        if ( EnergyHistory.Count > 64 ) EnergyHistory.RemoveAt( 0 );

        // Energy History Average
        EnergyHistoryAverage = 0f;
        for ( int i = 0; i < EnergyHistory.Count; i++ )
        {
            EnergyHistoryAverage += EnergyHistory[i];
        }
        EnergyHistoryAverage /= EnergyHistory.Count;

        // Beat Detection
        float energySum = 0f;
        foreach ( var energyValue in EnergyHistory )
        {
            energySum += energyValue;
        }
        float energyMean = energySum / EnergyHistory.Count;

        float variance = 0f;
        foreach ( var energyValue in EnergyHistory )
        {
            variance += (energyValue - energyMean) * (energyValue - energyMean);
        }
        float energyStdDev = (float)Math.Sqrt( variance / EnergyHistory.Count );

        // Adjusted Peak Threshold Calculation
        float peakThresholdMultiplier = 1.08f; // Adjust this multiplier as needed
        float adjustedPeakThreshold = peakThresholdMultiplier * energyStdDev;

        if ( EnergyHistoryAverage > 0.05f && Energy > EnergyHistoryAverage + adjustedPeakThreshold )
        {
            if ( !IsPeaking )
            {
                OnBeat?.Invoke();
            }
            IsPeaking = true;
        }
        else
        {
            IsPeaking = false;
        }


        Gizmo.Draw.LineThickness = 8f;
        Gizmo.Draw.Color = Color.Red;
        Gizmo.Draw.Line( new Vector3( 0, 100, 0 ), new Vector3( 0, 100, Energy * 100f ) );
        Gizmo.Draw.Line( new Vector3( 0, -100, 0 ), new Vector3( 0, -100, Energy * 100f ) );
        Gizmo.Draw.Color = Color.Green;
        Gizmo.Draw.Line( new Vector3( 0, 90, 0 ), new Vector3( 0, 90, EnergyHistoryAverage * 100f ) );
        Gizmo.Draw.Line( new Vector3( 0, -90, 0 ), new Vector3( 0, -90, EnergyHistoryAverage * 100f ) );
        Gizmo.Draw.Color = Color.Blue;
        var height = EnergyHistoryAverage + adjustedPeakThreshold;
        Gizmo.Draw.Line( new Vector3( 0, 95, 0 ), new Vector3( 0, 95, height * 100f ) );
        Gizmo.Draw.Line( new Vector3( 0, -95, 0 ), new Vector3( 0, -95, height * 100f ) );


    }

    protected override void OnEnabled()
    {
        if ( string.IsNullOrEmpty( Url ) ) return;

        MusicPlayer = MusicPlayer.PlayUrl( Url );
        MusicPlayer.Repeat = Loop;
        MusicPlayer.OnFinished += () =>
        {
            OnDisabled();
        };
    }

    protected override void OnDisabled()
    {
        if ( MusicPlayer == null ) return;

        MusicPlayer.Stop();
        MusicPlayer.Dispose();
        MusicPlayer = null;
        PeakKickVolume = 0f;
    }

    public void Play()
    {
        if ( MusicPlayer == null )
        {
            OnEnabled();
        }
    }
}