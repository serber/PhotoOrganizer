using System.Runtime.InteropServices;
using MediaOrganizer.Common;
using NReco.VideoInfo;

namespace MediaOrganizer.Video;

/// <inheritdoc />
public class VideoDateReader : IDateReader
{
    private readonly string[] _videoFileExtensions = new[]
    {
        ".webm", ".mpg", ".mp2", ".mpeg", ".mpe", ".mpv", ".ogg", ".mp4", ".m4p", ".m4v", ".avi", ".wmv", ".mov", ".qt", ".flv", ".swf", ".avchd"
    };

    /// <inheritdoc />
    public bool FileTypeSupported(string extension)
    {
        return _videoFileExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase);
    }

    /// <inheritdoc />
    public DateTime ReadDate(string filePath)
    {
        try
        {
            var ffProbe = new FFProbe();
            var videoInfo = ffProbe.GetMediaInfo(filePath);

            var creationDate = GetCreationDateTime(videoInfo, new[] { "com.apple.quicktime.creationdate", "creation_time" });

            if (creationDate is not null)
            {
                return creationDate.Value;
            }
        }
        catch
        {
            //---   ignore
        }

        return ReaderHelper.GetDefault(filePath);
    }

    private static DateTime? GetCreationDateTime(MediaInfo videoInfo, IEnumerable<string> tags)
    {
        foreach (var tag in tags)
        {
            var pair = videoInfo.FormatTags.FirstOrDefault(x => x.Key.Equals(tag, StringComparison.OrdinalIgnoreCase));
            if (DateTime.TryParse(pair.Value, out var dateVideoTaken))
            {
                return dateVideoTaken;
            }
        }

        return null;
    }
}