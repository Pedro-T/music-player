using Godot;
using System;
using System.IO;

public partial class Settings : Node
{
    private static readonly string SettingsFilePath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile), "Settings.cfg");
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
    private ConfigFile configFile = new ConfigFile();

    public static Settings Instance { get; private set; }

    public event Action<string> TrackDirectoryPathChanged;

    public override void _Ready()
    {
        Instance = this;
        GD.Print("load settings");
        LoadSettings();
    }

    private void LoadSettings()
    {
        if (File.Exists(SettingsFilePath))
        {
            if (configFile.Load(SettingsFilePath) == Error.Ok)
            {
                _trackDirectoryPath = (string)configFile.GetValue("Settings", "TrackDirectoryPath", DefaultTrackDirectoryPath);
                GD.Print(_trackDirectoryPath);
            }
        }
        else
        {
            configFile.SetValue("Settings", "TrackDirectoryPath", DefaultTrackDirectoryPath);
            configFile.Save(SettingsFilePath);
            LoadSettings(); // Reload to ensure the values are correctly set
        }
        
    }

    public void SaveSettings()
    {
        configFile.SetValue("Settings", "TrackDirectoryPath", TrackDirectoryPath);
        configFile.Save(SettingsFilePath);
    }
}