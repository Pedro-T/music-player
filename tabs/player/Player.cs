using Godot;
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
    private HSlider volumeSlider;

    private string trackDirectoryPath = "";

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
        volumeSlider = GetNode<HSlider>("PlayerControlsContainer/VolumeSlider");

        trackDirectoryPath = Settings.Instance.TrackDirectoryPath;
        Settings.Instance.TrackDirectoryPathChanged += OnTrackDirectoryPathChanged;
        PopulatePlaylistFromFile();

        playlist.ItemSelected += OnPlaylistItemSelected;
        playButton.Pressed += OnPlayPressed;
        nextButton.Pressed += OnNextPressed;
        prevButton.Pressed += OnPrevPressed;
        shuffleButton.Toggled += OnShuffleToggled;
        volumeSlider.ValueChanged += OnVolumeChanged;

        audioPlayer.Finished += PlayNext;
        progressDisplay.Setup(audioPlayer);

        // Set initial volume to 50%
        SetVolume(50);
    }


    private void PopulatePlaylistFromFile()
    {
        playlist.Clear();
        playlistTracks.Clear();

        if (!Directory.Exists(trackDirectoryPath))
        {
            GD.PrintErr($"Track directory does not exist: {trackDirectoryPath}");
            return;
        }

        FileInfo[] files = new DirectoryInfo(trackDirectoryPath).GetFiles();
        foreach (FileInfo file in files)
        {
            playlist.AddItem(file.Name);
            playlistTracks.Add(file.Name);
        }
    }

    private void OnTrackDirectoryPathChanged(string newPath)
    {
        trackDirectoryPath = newPath;
        ReloadPlaylist();
    }

    private void ReloadPlaylist()
    {
        selectedTrackName = string.Empty;
        playingTrack = string.Empty;
        audioPlayer.Stop();
        PopulatePlaylistFromFile();
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
            LoadTrack(playlistTracks[playlistTracks.Count - 1]);
        }
        else
        {
            LoadTrack(playlistTracks[currentIndex - 1]);
        }
        audioPlayer.Play();
        playButton.Text = "Stop";
    }

    private void OnNextPressed()
    {
        PlayNext();
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
            LoadTrack(selectedTrackName);
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

    private void PlayNext()
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
            LoadTrack(nextTrack);
        }
        else
        {
            int currentIndex = playlistTracks.IndexOf(playingTrack);
            if (currentIndex == playlistTracks.Count - 1)
            {
                LoadTrack(playlistTracks[0]);
            }
            else
            {
                LoadTrack(playlistTracks[currentIndex + 1]);
            }
        }
        audioPlayer.Play();
        playButton.Text = "Stop";
    }

    private void LoadTrack(string trackName)
    {
        
        string fileExtension = Path.GetExtension(trackName).ToLower();
        AudioStream stream = null;
        switch (fileExtension)
        {
            case ".mp3":
                byte[] bytes = File.ReadAllBytes(Path.Combine(trackDirectoryPath, trackName));
                stream = new AudioStreamMP3();
                ((AudioStreamMP3)stream).Data = bytes;
                break;
            case ".ogg":
                stream = AudioStreamOggVorbis.LoadFromFile(Path.Combine(trackDirectoryPath, trackName));
                break;
            default:
                GD.PrintErr("Unsupported file format: " + fileExtension);
                return;
        }

        if (stream == null)
        {
            return;
        }
        
        audioPlayer.Stream = stream;
        UpdateTrackPlayer(trackName);
        playingTrack = trackName;

        // Remove track from shuffle queue if shuffle is enabled
        if (isShuffle)
        {
            shuffleQueue.Remove(trackName);
        }
    }

    private void UpdateTrackPlayer(string trackName)
    {
        playingTrackLabel.Text = "Playing: " + trackName;
    }

    private void OnVolumeChanged(double value)
    {
        SetVolume((float)value);
    }

    private void SetVolume(float percentage)
    {
        // Convert percentage (0-100) to linear (0-1) then to decibels
        float linearVolume = percentage / 100f;
        audioPlayer.VolumeDb = Mathf.LinearToDb(linearVolume);
    }
}