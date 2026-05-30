using Godot;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

public partial class Download : VBoxContainer
{
    private Button downloadButton;
    private LineEdit urlField;
    private Label statusLabel;
    private bool isDownloading;

    public override void _Ready()
    {
        downloadButton = GetNode<Button>("DownloadButton");
        urlField = GetNode<LineEdit>("UrlField");
        statusLabel = GetNode<Label>("StatusLabel");

        downloadButton.Pressed += OnDownloadPressed;
    }

    private async void OnDownloadPressed()
    {
        if (isDownloading)
        {
            return;
        }

        string url = urlField.Text.Trim();
        if (string.IsNullOrWhiteSpace(url))
        {
            SetStatus("Enter a YouTube URL first.", true);
            return;
        }

        string dlpPath = Settings.Instance.DLPDirectoryPath;
        if (string.IsNullOrWhiteSpace(dlpPath) || !File.Exists(dlpPath))
        {
            SetStatus("Set a valid youtube-dlp executable path in settings.", true);
            return;
        }

        string trackDirectoryPath = Settings.Instance.TrackDirectoryPath;
        if (string.IsNullOrWhiteSpace(trackDirectoryPath))
        {
            SetStatus("Set a track directory in settings.", true);
            return;
        }

        string ffmpegPath = Settings.Instance.FFmpegPath;
        if (string.IsNullOrWhiteSpace(ffmpegPath) || !File.Exists(ffmpegPath))
        {
            SetStatus("Set a valid ffmpeg executable path in settings.", true);
            return;
        }

        string ffprobePath = Settings.Instance.FFprobePath;
        if (string.IsNullOrWhiteSpace(ffprobePath) || !File.Exists(ffprobePath))
        {
            SetStatus("Set a valid ffprobe executable path in settings.", true);
            return;
        }

        Directory.CreateDirectory(trackDirectoryPath);

        isDownloading = true;
        downloadButton.Disabled = true;
        downloadButton.Text = "Downloading...";
        SetStatus("Starting download...", false);

        try
        {
            DownloadResult result = await DownloadAudioAsync(dlpPath, ffmpegPath, ffprobePath, trackDirectoryPath, url);
            if (result.ExitCode == 0)
            {
                urlField.Clear();
                Settings.Instance.NotifyTrackDirectoryContentsChanged();
                SetStatus("Download complete.", false);
                return;
            }

            string error = string.IsNullOrWhiteSpace(result.ErrorOutput)
                ? result.StandardOutput
                : result.ErrorOutput;
            SetStatus($"Download failed: {TrimOutput(error)}", true);
        }
        catch (Exception ex)
        {
            SetStatus($"Download failed: {ex.Message}", true);
        }
        finally
        {
            isDownloading = false;
            downloadButton.Disabled = false;
            downloadButton.Text = "Download";
        }
    }

    private static async Task<DownloadResult> DownloadAudioAsync(string dlpPath, string ffmpegPath, string ffprobePath, string trackDirectoryPath, string url)
    {
        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = dlpPath,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
        };

        string ffmpegDirectory = Path.GetDirectoryName(ffmpegPath);
        string ffprobeDirectory = Path.GetDirectoryName(ffprobePath);
        AddToolDirectoryToPath(startInfo, ffmpegDirectory);
        AddToolDirectoryToPath(startInfo, ffprobeDirectory);

        startInfo.ArgumentList.Add("--no-playlist");
        startInfo.ArgumentList.Add("--extract-audio");
        startInfo.ArgumentList.Add("--audio-format");
        startInfo.ArgumentList.Add("vorbis");
        startInfo.ArgumentList.Add("--ffmpeg-location");
        startInfo.ArgumentList.Add(ffmpegDirectory == ffprobeDirectory ? ffmpegDirectory : ffmpegPath);
        startInfo.ArgumentList.Add("--output");
        startInfo.ArgumentList.Add(Path.Combine(trackDirectoryPath, "%(title)s.%(ext)s"));
        startInfo.ArgumentList.Add(url);

        using Process process = new Process { StartInfo = startInfo };
        process.Start();

        Task<string> standardOutput = process.StandardOutput.ReadToEndAsync();
        Task<string> errorOutput = process.StandardError.ReadToEndAsync();

        await process.WaitForExitAsync();

        return new DownloadResult(
            process.ExitCode,
            await standardOutput,
            await errorOutput
        );
    }

    private static void AddToolDirectoryToPath(ProcessStartInfo startInfo, string directoryPath)
    {
        if (string.IsNullOrWhiteSpace(directoryPath))
        {
            return;
        }

        string existingPath = startInfo.Environment.TryGetValue("PATH", out string path) ? path : string.Empty;
        if (existingPath.Contains(directoryPath))
        {
            return;
        }

        startInfo.Environment["PATH"] = string.IsNullOrWhiteSpace(existingPath)
            ? directoryPath
            : directoryPath + Path.PathSeparator + existingPath;
    }

    private void SetStatus(string message, bool isError)
    {
        statusLabel.Text = message;
        statusLabel.Modulate = isError ? Colors.IndianRed : Colors.LightGreen;
    }

    private static string TrimOutput(string output)
    {
        if (string.IsNullOrWhiteSpace(output))
        {
            return "Unknown error";
        }

        output = output.Trim();
        return output.Length <= 220 ? output : output[^220..];
    }

    private readonly record struct DownloadResult(int ExitCode, string StandardOutput, string ErrorOutput);
}
