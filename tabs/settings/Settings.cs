using Godot;
using System;
using System.IO;

public partial class Settings : VBoxContainer
{
    public string TrackDirectoryPath {get; private set;} = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyMusic), "tracks");

    public override void _Ready()
    {
        Label trackDirectoryLabel = GetNode<Label>("TrackDirContainer/TrackDirPathLabel");
        Button browseButton = GetNode<Button>("TrackDirContainer/BrowseButton");

        trackDirectoryLabel.Text = TrackDirectoryPath;

        browseButton.Pressed += () =>
        {
            FileDialog fileDialog = new FileDialog();
            AddChild(fileDialog);
            fileDialog.FileMode = FileDialog.FileModeEnum.OpenDir;
            fileDialog.Access = FileDialog.AccessEnum.Filesystem;
            fileDialog.PopupCentered();
            fileDialog.DirSelected += (string path) =>
            {
                TrackDirectoryPath = path;
                trackDirectoryLabel.Text = TrackDirectoryPath;
            };

            fileDialog.Canceled += () => fileDialog.QueueFree();
        };
    }

}
