using MediaOrganizer.Common;
using MetadataExtractor;
using MetadataExtractor.Formats.QuickTime;

namespace MediaOrganizer.Video;

public sealed class AppleVideoDataReader : IDateReader
{
    private readonly string[] _videoFileExtensions = new[]
    {
        ".mov"
    };
    
    public bool FileTypeSupported(string extension)
    {
        return _videoFileExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase);
    }

    public DateTime ReadDate(string filePath)
    {
        try
        {
            var directories = ImageMetadataReader.ReadMetadata(filePath);
            
            var metadataHeaderDirectory = directories.OfType<QuickTimeMovieHeaderDirectory>().FirstOrDefault();
            if (metadataHeaderDirectory != null)
            {
                var date = metadataHeaderDirectory.GetDateTime(QuickTimeMovieHeaderDirectory.TagCreated);
                if (date != DateTime.MinValue && date.Year >= 1900)
                {
                    return date;
                }
            }
        }
        catch (Exception ex)
        {
            // Log but don't fail - fall back to file system date
            Console.WriteLine($"Warning: Could not read QuickTime metadata from {filePath}: {ex.Message}");
        }

        return ReaderHelper.GetDefault(filePath);
    }
}