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
    private Button shuffleButton;
    private Label playingTrackLabel;
    private ProgressDisplay progressDisplay;

    private static readonly string homePath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyMusic), "tracks");

    private bool isShuffle = false;
    private readonly Random random = new Random();

    private List<string> playlistTracks = new List<string>();
    private List<string> shuffleQueue = new List<string>();
    private string selectedTrackName = "";
    private string playingTrack = "";

    public override void _Ready()
    {
        audioPlayer = GetNode<AudioStreamPlayer>("AudioStreamPlayer");
        playlist = GetNode<ItemList>("Playlist");
        playButton = GetNode<Button>("PlayerControlsContainer/PlayButton");
        nextButton = GetNode<Button>("PlayerControlsContainer/NextButton");
        prevButton = GetNode<Button>("PlayerControlsContainer/PrevButton");
        shuffleButton = GetNode<Button>("PlayerControlsContainer/ShuffleButton");
        playingTrackLabel = GetNode<Label>("PlayingTrackLabel");
        progressDisplay = GetNode<ProgressDisplay>("ProgressDisplay");

        playlist.Size = new Vector2(500, 400);
        populatePlaylistFromFile();

        playlist.ItemSelected += OnPlaylistItemSelected;
        playButton.Pressed += OnPlayPressed;
        nextButton.Pressed += OnNextPressed;
        prevButton.Pressed += OnPrevPressed;
        shuffleButton.Toggled += OnShuffleToggled;

        audioPlayer.Finished += playNext;
        progressDisplay.Setup(audioPlayer);
    }


    private void populatePlaylistFromFile()
    {
        playlist.Clear();
        FileInfo[] files = new DirectoryInfo(homePath).GetFiles();
        foreach (FileInfo file in files)
        {
            playlist.AddItem(file.Name);
            playlistTracks.Add(file.Name);
        }
    }

    private void OnPlaylistItemSelected(long index)
    {
        selectedTrackName = playlist.GetItemText((int)index);
    }

    private void OnPrevPressed()
    {
        int currentIndex = playlistTracks.IndexOf(playingTrack);
        if (currentIndex == 0)
        {
            loadTrack(playlistTracks[playlistTracks.Count - 1]);
        }
        else
        {
            loadTrack(playlistTracks[currentIndex - 1]);
        }
        audioPlayer.Play();
        playButton.Text = "Stop";
    }

    private void OnNextPressed()
    {
        playNext();
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

    private void OnShuffleToggled(bool toggledOn)
    {
        isShuffle = toggledOn;
        if (isShuffle)
        {
            InitializeShuffleQueue();
        }
    }

    private void InitializeShuffleQueue()
    {
        shuffleQueue = new List<string>(playlistTracks);
    }

    private void playNext()
    {
        if (playlistTracks.Count == 0)
        {
            return;
        }

        if (isShuffle)
        {
            // Reset shuffle queue if empty (all tracks have been played)
            if (shuffleQueue.Count == 0)
            {
                InitializeShuffleQueue();
            }

            // Pick a random track from remaining tracks
            int randomIndex = random.Next(shuffleQueue.Count);
            string nextTrack = shuffleQueue[randomIndex];
            shuffleQueue.RemoveAt(randomIndex);
            loadTrack(nextTrack);
        }
        else
        {
            int currentIndex = playlistTracks.IndexOf(playingTrack);
            if (currentIndex == playlistTracks.Count - 1)
            {
                loadTrack(playlistTracks[0]);
            }
            else
            {
                loadTrack(playlistTracks[currentIndex + 1]);
            }
        }
        audioPlayer.Play();
        playButton.Text = "Stop";
    }

    private void loadTrack(String trackName)
    {
        byte[] bytes = File.ReadAllBytes(Path.Combine(homePath, trackName));
        AudioStreamMP3 mp3 = new AudioStreamMP3();
        mp3.Data = bytes;

        audioPlayer.Stream = mp3;
        updateTrackPlayer(trackName);
        playingTrack = trackName;

        // Remove track from shuffle queue if shuffle is enabled
        if (isShuffle)
        {
            shuffleQueue.Remove(trackName);
        }
    }

    private void updateTrackPlayer(String trackName)
    {
        playingTrackLabel.Text = "Playing: " + trackName;
    }
}