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
        get => IsPlaying ? MusicPlayer.Position : Vector3.Zero;
        set
        {
            if ( MusicPlayer != null )
            {
                MusicPlayer.Position = value;
            }
        }
    }
    public string CurrentlyPlaying => IsPlaying ? (string.IsNullOrEmpty( MusicPlayer.Title ) ? _currentlyPlaying : MusicPlayer.Title) : null;
    string _currentlyPlaying = null;

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
        float length = spectrum.Length;
        for ( int i = 0; i < length; i++ )
        {
            energy += spectrum[i];
        }
        energy /= length;
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
        // string[] songs = new string[]
        // {
        //     "https://www.youtube.com/watch?v=aTMhTKISKeI",
        //     "https://www.youtube.com/watch?v=hhMZ5mpOTGk",
        //     "https://cdn.discordapp.com/attachments/894656054074437702/1191158838079012895/1139563_Im-Human.mp3?ex=65a46c1d&is=6591f71d&hm=08fe9cc0eb427ad23a8ecbd9421b410a21f6a72341a029587880e2d6de48c3d0&"
        // };
        // Url = songs[Random.Shared.Next( 0, songs.Length )];

        Play( Url );
    }

    protected override void OnDisabled()
    {
        Stop();
    }

    public void Play( string url )
    {
        if ( IsPlaying )
        {
            Stop();
        }

        Url = url;
        Play();
    }

    public async void Play()
    {
        if ( string.IsNullOrEmpty( Url ) ) return;
        if ( MusicPlayer != null )
        {
            if ( IsPaused )
            {
                IsPaused = false;
                return;
            }
            Stop();
        }

        string url = Url;
        if ( MediaHelper.IsYoutubeUrl( url ) )
        {
            var response = await MediaHelper.GetYoutubePlayerResponseFromUrl( url );
            url = response.GetStreamUrl();
            _currentlyPlaying = response.Author + " - " + response.Title;
        }

        MusicPlayer = MusicPlayer.PlayUrl( url );
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
        _currentlyPlaying = null;
    }

    public void Pause()
    {
        if ( MusicPlayer != null )
        {
            IsPaused = true;
        }
    }
}