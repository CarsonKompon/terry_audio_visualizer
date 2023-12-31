using System;
using System.Collections.Generic;
using Sandbox;
using MediaHelpers;

public class MusicManager : Component
{
    [Property] public string Url { get; set; }
    [Property] public bool Loop { get; set; } = false;
    [Property] public float PeakThreshold { get; set; } = 1.08f;
    public float AdjustedPeakThreshold { get; private set; } = 0f;

    public bool IsPaused
    {
        get => MusicPlayer != null && MusicPlayer.Paused;
        set
        {
            if ( MusicPlayer != null )
            {
                MusicPlayer.Paused = value;
            }
        }
    }

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
        AdjustedPeakThreshold = PeakThreshold * energyStdDev;

        if ( EnergyHistoryAverage > 0.05f && Energy > EnergyHistoryAverage + AdjustedPeakThreshold )
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
    }

    protected override void OnEnabled()
    {
        Play();
    }

    protected override void OnDisabled()
    {
        Stop();
    }

    public async void Play( string url )
    {
        if ( IsPlaying )
        {
            Stop();
        }

        if ( MediaHelper.IsYoutubeUrl( url ) )
        {
            url = await MediaHelper.GetUrlFromYoutubeUrl( url );
        }

        Url = url;
        Play();
    }

    public void Play()
    {
        if ( string.IsNullOrEmpty( Url ) ) return;
        if ( IsPlaying ) Stop();

        MusicPlayer = MusicPlayer.PlayUrl( Url );
        MusicPlayer.Repeat = Loop;
        MusicPlayer.OnFinished += () =>
        {
            OnDisabled();
        };

        IsPaused = false;
    }

    public void Stop()
    {
        if ( MusicPlayer == null ) return;

        MusicPlayer.Stop();
        MusicPlayer.Dispose();
        MusicPlayer = null;
        PeakKickVolume = 0f;
    }

    public void Pause()
    {
        if ( MusicPlayer != null )
        {
            IsPaused = true;
        }
    }
}