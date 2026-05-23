using Godot;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.IO;

public partial class Player : GridContainer
{
    private AudioStreamPlayer audioPlayer;
    private ItemList playlist;
    private Button playButton;
    private Button nextButton;
    private Button prevButton;

    private static readonly String homePath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyMusic), "tracks");
    private String selectedTrackName = "";
    private String playingTrack = "";

    public override void _Ready()
    {
        audioPlayer = GetNode<AudioStreamPlayer>("AudioStreamPlayer");
        playlist = GetNode<ItemList>("Playlist");
        playButton = GetNode<Button>("PlayerControlsContainer/PlayButton");
        nextButton = GetNode<Button>("PlayerControlsContainer/NextButton");
        prevButton = GetNode<Button>("PlayerControlsContainer/PrevButton");
        playlist.Size = new Vector2(500, 400);
        populatePlaylistFromFile();

        playlist.ItemSelected += OnPlaylistItemSelected;
        playButton.Pressed += OnPlayPressed;
    }


    private void populatePlaylistFromFile()
    {
        playlist.Clear();
        FileInfo[] files = new DirectoryInfo(homePath).GetFiles();
        foreach (FileInfo file in files)
        {
            playlist.AddItem(file.Name);
        }
    }

    private void OnPlaylistItemSelected(long index)
    {
        selectedTrackName = playlist.GetItemText((int)index);
    }

    private void OnPlayPressed()
    {
        if (audioPlayer.Playing)
        {
            playButton.Text = "Play";
            audioPlayer.StreamPaused = true;
            return;
        }
        if (selectedTrackName == "")
        {
            return;
        }
        if (playingTrack != selectedTrackName) { // not playing and different track selected
            loadTrack(selectedTrackName);
            audioPlayer.Play();
            playButton.Text = "Stop";
            return;
        }
        playButton.Text = "Stop";
        audioPlayer.StreamPaused = false;
    }

    private void loadTrack(String trackName)
    {
        byte[] bytes = File.ReadAllBytes(Path.Combine(homePath, trackName));
        AudioStreamMP3 mp3 = new AudioStreamMP3();
        mp3.Data = bytes;

        audioPlayer.Stream = mp3;
        playingTrack = trackName;
    }
}