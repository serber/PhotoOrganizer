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
            var fileInfo = new FileInfo(filePath);

            var ffProbe = new FFProbe();
            var videoInfo = ffProbe.GetMediaInfo(fileInfo.FullName);

            var creationTimePair = videoInfo.FormatTags.FirstOrDefault(x => x.Key.Equals("creation_time"));
            if (DateTime.TryParse(creationTimePair.Value, out var dateVideoTaken))
            {
                return dateVideoTaken;
            }
        }
        catch
        {
            //---   ignore
        }

        return ReaderHelper.GetDefault(filePath);
    }
}