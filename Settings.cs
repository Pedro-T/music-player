using Godot;
using System;
using System.IO;

public partial class Settings : Node
{

    private static readonly string DefaultTrackDirectoryPath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyMusic), "tracks");

    private string _trackDirectoryPath;
    public string TrackDirectoryPath {
        get => _trackDirectoryPath;
        set
        {
            if (_trackDirectoryPath == value)
                return;

            _trackDirectoryPath = value;
            SaveSettings();
            TrackDirectoryPathChanged?.Invoke(value);
        }
    }

    public string _dlpDirectoryPath;

    public string DLPDirectoryPath
    {
        get => _dlpDirectoryPath;
        set
        {
            if (_dlpDirectoryPath == value)
            {
                return;
            }
            _dlpDirectoryPath = value;
            SaveSettings();
        }
    }

    private string _ffmpegPath;
    public string FFmpegPath
    {
        get => _ffmpegPath;
        set
        {
            if (_ffmpegPath == value)
            {
                return;
            }

            _ffmpegPath = value;
            SaveSettings();
        }
    }

    private string _ffprobePath;
    public string FFprobePath
    {
        get => _ffprobePath;
        set
        {
            if (_ffprobePath == value)
            {
                return;
            }

            _ffprobePath = value;
            SaveSettings();
        }
    }

    private ConfigFile configFile = new ConfigFile();

    public static Settings Instance { get; private set; }

    public event Action<string> TrackDirectoryPathChanged;

    public override void _Ready()
    {
        Instance = this;
        LoadSettings();
    }

    private void LoadSettings()
    {
        if (Godot.FileAccess.FileExists("user://settings.cfg"))
        {
            if (configFile.Load("user://settings.cfg") == Error.Ok)
            {
                _trackDirectoryPath = (string)configFile.GetValue("Settings", "TrackDirectoryPath", DefaultTrackDirectoryPath);
                _dlpDirectoryPath = (string)configFile.GetValue("Settings", "DLPDirectoryPath", "");
                _ffmpegPath = (string)configFile.GetValue("Settings", "FFmpegPath", "");
                _ffprobePath = (string)configFile.GetValue("Settings", "FFprobePath", "");
            }
        }
        else // create new file
        {
            configFile.SetValue("Settings", "TrackDirectoryPath", DefaultTrackDirectoryPath);
            configFile.SetValue("Settings", "DLPDirectoryPath", "");
            configFile.SetValue("Settings", "FFmpegPath", "");
            configFile.SetValue("Settings", "FFprobePath", "");
            configFile.Save("user://settings.cfg");
            LoadSettings(); // Reload to ensure the values are correctly set
        }
        
    }

    public void SaveSettings()
    {
        configFile.SetValue("Settings", "TrackDirectoryPath", TrackDirectoryPath);
        configFile.SetValue("Settings", "DLPDirectoryPath", DLPDirectoryPath);
        configFile.SetValue("Settings", "FFmpegPath", FFmpegPath);
        configFile.SetValue("Settings", "FFprobePath", FFprobePath);
        configFile.Save("user://settings.cfg");
    }

    public void NotifyTrackDirectoryContentsChanged()
    {
        TrackDirectoryPathChanged?.Invoke(TrackDirectoryPath);
    }
}
