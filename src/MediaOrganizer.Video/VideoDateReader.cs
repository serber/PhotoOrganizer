using System.Runtime.InteropServices;
using MediaOrganizer.Common;
using NReco.VideoInfo;

namespace MediaOrganizer.Video;

/// <inheritdoc />
public class VideoDateReader : IDateReader
{
    private readonly string[] _videoFileExtensions = new[]
    {
        ".webm", ".mpg", ".mp2", ".mpeg", ".mpe", ".mpv", ".ogg", ".mp4", ".m4p", ".m4v", ".avi", ".wmv", ".qt", ".flv", ".swf", ".avchd"
    };

    private readonly string? _ffProbePath;

    public VideoDateReader(string? ffProbePath = null)
    {
        _ffProbePath = ffProbePath ?? FindFFProbePath();
    }

    /// <inheritdoc />
    public bool FileTypeSupported(string extension)
    {
        return _videoFileExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase);
    }

    /// <inheritdoc />
    public DateTime ReadDate(string filePath)
    {
        if (string.IsNullOrWhiteSpace(_ffProbePath))
        {
            return ReaderHelper.GetDefault(filePath);
        }

        try
        {
            var ffProbe = new FFProbe
            {
                ToolPath = _ffProbePath
            };
            
            var videoInfo = ffProbe.GetMediaInfo(filePath);

            var creationDate = GetCreationDateTime(videoInfo, new[] { "com.apple.quicktime.creationdate", "creation_time" });

            if (creationDate is not null)
            {
                return creationDate.Value;
            }
        }
        catch (Exception ex)
        {
            // Log but don't fail - fall back to file system date
            Console.WriteLine($"Warning: Could not read video metadata from {filePath}: {ex.Message}");
        }

        return ReaderHelper.GetDefault(filePath);
    }

    private static DateTime? GetCreationDateTime(MediaInfo videoInfo, IEnumerable<string> tags)
    {
        if (videoInfo?.FormatTags == null)
            return null;

        foreach (var tag in tags)
        {
            var pair = videoInfo.FormatTags.FirstOrDefault(x => x.Key.Equals(tag, StringComparison.OrdinalIgnoreCase));
            if (!string.IsNullOrWhiteSpace(pair.Value) && DateTime.TryParse(pair.Value, out var dateVideoTaken))
            {
                return dateVideoTaken;
            }
        }

        return null;
    }

    private static string? FindFFProbePath()
    {
        // Common paths for ffprobe
        var possiblePaths = new List<string>();

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            possiblePaths.AddRange(new[]
            {
                "/opt/homebrew/bin/ffprobe",
                "/usr/local/bin/ffprobe",
                "/opt/homebrew/bin/",
                "/usr/local/bin/"
            });
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            possiblePaths.AddRange(new[]
            {
                "/usr/bin/ffprobe",
                "/usr/local/bin/ffprobe"
            });
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            // On Windows, ffprobe might be in PATH or in a specific directory
            var pathEnv = Environment.GetEnvironmentVariable("PATH");
            if (!string.IsNullOrEmpty(pathEnv))
            {
                var paths = pathEnv.Split(Path.PathSeparator);
                foreach (var path in paths)
                {
                    var ffProbePath = Path.Combine(path, "ffprobe.exe");
                    if (File.Exists(ffProbePath))
                    {
                        return Path.GetDirectoryName(ffProbePath);
                    }
                }
            }
        }

        // Check if ffprobe is in PATH
        try
        {
            var processStartInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "ffprobe.exe" : "ffprobe",
                Arguments = "-version",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = System.Diagnostics.Process.Start(processStartInfo);
            if (process != null)
            {
                process.WaitForExit(1000);
                if (process.ExitCode == 0)
                {
                    return null; // ffprobe is in PATH, NReco will find it
                }
            }
        }
        catch
        {
            // Ignore
        }

        // Check specific paths
        foreach (var path in possiblePaths)
        {
            var fullPath = Path.IsPathRooted(path) && !path.EndsWith(Path.DirectorySeparatorChar.ToString())
                ? path
                : Path.Combine(path, RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "ffprobe.exe" : "ffprobe");
            
            if (File.Exists(fullPath))
            {
                return Path.GetDirectoryName(fullPath);
            }
            
            // If it's a directory, check if ffprobe exists there
            if (Directory.Exists(path))
            {
                var ffProbeInDir = Path.Combine(path, RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "ffprobe.exe" : "ffprobe");
                if (File.Exists(ffProbeInDir))
                {
                    return path;
                }
            }
        }

        return null;
    }
}