using Sandbox;

namespace CUtils.Components;

[Category("CUtils")]
[Title("MusicPlayer Component")]
[Icon("music_note")]
public class MusicPlayerComponent : Component
{
    [Property] public string Url { get; set; }
    [Property] public bool Loop { get; set; } = false;

    public MusicPlayer MusicPlayer;

    protected override void OnEnabled()
    {
        if (string.IsNullOrEmpty(Url)) return;

        MusicPlayer = MusicPlayer.PlayUrl(Url);
        MusicPlayer.Repeat = Loop;
        MusicPlayer.OnFinished += () =>
        {
            MusicPlayer.Stop();
            MusicPlayer.Dispose();
            MusicPlayer = null;
        };
    }

    protected override void OnDisabled()
    {
        MusicPlayer.Stop();
        MusicPlayer.Dispose();
        MusicPlayer = null;
    }

    public void Play()
    {
        if (MusicPlayer == null)
        {
            OnEnabled();
        }
    }
}