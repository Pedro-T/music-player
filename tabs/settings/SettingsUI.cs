using Godot;
using System;
using System.IO;

public partial class SettingsUI : VBoxContainer
{
    public override void _Ready()
    {
        Label trackDirectoryLabel = GetNode<Label>("TrackDirContainer/TrackDirPathLabel");
        Button browseButton = GetNode<Button>("TrackDirContainer/BrowseButton");

        trackDirectoryLabel.Text = Settings.Instance.TrackDirectoryPath;

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
    }

}



