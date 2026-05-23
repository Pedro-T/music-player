using Godot;
using System;

public partial class ProgressDisplay : HBoxContainer
{
    private Label currentTimeLabel;
    private Label totalDurationLabel;
    private AudioStreamPlayer audioPlayer;

    private Timer timer;
    public override void _Ready()
    {
        currentTimeLabel = GetNode<Label>("CurrentTimeLabel");
        totalDurationLabel = GetNode<Label>("TrackLengthLabel");
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
        currentTimeLabel.Text = TimeSpan.FromSeconds(audioPlayer.GetPlaybackPosition()).ToString(@"mm\:ss");
        totalDurationLabel.Text = TimeSpan.FromSeconds(audioPlayer.Stream.GetLength()).ToString(@"mm\:ss");
    }
}
