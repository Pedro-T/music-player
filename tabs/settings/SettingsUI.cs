using Godot;
using System;
using System.IO;

public partial class SettingsUI : VBoxContainer
{
    public override void _Ready()
    {
        Label trackDirectoryLabel = GetNode<Label>("TrackDirContainer/TrackDirPathLabel");
        Label dlpPathLabel = GetNode<Label>("DLPContainer/DLPPathLabel");
        Label ffmpegPathLabel = GetNode<Label>("FFmpegContainer/FFmpegPathLabel");
        Label ffprobePathLabel = GetNode<Label>("FFprobeContainer/FFprobePathLabel");
        Button browseButton = GetNode<Button>("TrackDirContainer/BrowseButton");
        Button dlpBrowseButton = GetNode<Button>("DLPContainer/BrowseButton");
        Button ffmpegBrowseButton = GetNode<Button>("FFmpegContainer/BrowseButton");
        Button ffprobeBrowseButton = GetNode<Button>("FFprobeContainer/BrowseButton");

        trackDirectoryLabel.Text = Settings.Instance.TrackDirectoryPath;
        dlpPathLabel.Text = Settings.Instance.DLPDirectoryPath;
        ffmpegPathLabel.Text = Settings.Instance.FFmpegPath;
        ffprobePathLabel.Text = Settings.Instance.FFprobePath;

        browseButton.Pressed += () =>
        {
            FileDialog fileDialog = new FileDialog();
            AddChild(fileDialog);
            fileDialog.FileMode = FileDialog.FileModeEnum.OpenDir;
            fileDialog.Access = FileDialog.AccessEnum.Filesystem;
            fileDialog.PopupCentered();
            fileDialog.DirSelected += (string path) =>
            {
                Settings.Instance.TrackDirectoryPath = path;
                RemoveChild(fileDialog); // Free the FileDialog after setting 
                trackDirectoryLabel.Text = path;
            };

            fileDialog.Canceled += () => fileDialog.QueueFree();
        };

        dlpBrowseButton.Pressed += () => BrowseToolPath(dlpPathLabel, path => Settings.Instance.DLPDirectoryPath = path);
        ffmpegBrowseButton.Pressed += () => BrowseToolPath(ffmpegPathLabel, path => Settings.Instance.FFmpegPath = path);
        ffprobeBrowseButton.Pressed += () => BrowseToolPath(ffprobePathLabel, path => Settings.Instance.FFprobePath = path);
    }

    private void BrowseToolPath(Label pathLabel, Action<string> onPathSelected)
    {
        FileDialog fileDialog = new FileDialog();
        AddChild(fileDialog);
        fileDialog.FileMode = FileDialog.FileModeEnum.OpenFile;
        fileDialog.Access = FileDialog.AccessEnum.Filesystem;
        fileDialog.PopupCentered();
        fileDialog.FileSelected += (string path) =>
        {
            onPathSelected(path);
            RemoveChild(fileDialog); // Free the FileDialog after setting
            pathLabel.Text = path;
        };

        fileDialog.Canceled += () => fileDialog.QueueFree();
    }
}


