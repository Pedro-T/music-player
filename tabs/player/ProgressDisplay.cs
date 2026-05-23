using Godot;
using System;

public partial class ProgressDisplay : HBoxContainer
{
    private Label currentTimeLabel;
    private Label totalDurationLabel;

    private ProgressBar progressBar;
    private AudioStreamPlayer audioPlayer;

    private Timer timer;
    public override void _Ready()
    {
        currentTimeLabel = GetNode<Label>("CurrentTimeLabel");
        totalDurationLabel = GetNode<Label>("TrackLengthLabel");
        progressBar = GetNode<ProgressBar>("TrackProgressBar");

        timer = GetNode<Timer>("ProgressUpdateTimer");
        timer.Connect("timeout", Callable.From(() => UpdateProgress()));
        timer.Start();
    }

    public void Setup(AudioStreamPlayer player)
    {
        audioPlayer = player;
    }

    private void UpdateProgress()
    {
        if (audioPlayer == null || audioPlayer.Playing == false)
        {
            return;
        }
        TimeSpan currentTime = TimeSpan.FromSeconds(audioPlayer.GetPlaybackPosition());
        TimeSpan totalDuration = TimeSpan.FromSeconds(audioPlayer.Stream.GetLength());
        currentTimeLabel.Text = currentTime.ToString(@"mm\:ss");
        totalDurationLabel.Text = totalDuration.ToString(@"mm\:ss");
        float progress = (float)currentTime.TotalSeconds / (float)totalDuration.TotalSeconds;
        progressBar.Value = progress;
    }
}
