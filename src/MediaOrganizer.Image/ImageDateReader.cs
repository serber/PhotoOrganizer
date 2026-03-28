using ImageMagick;
using MediaOrganizer.Common;
using System.Globalization;

namespace MediaOrganizer.Image;

/// <inheritdoc />
public class ImageDateReader : IDateReader
{
    private readonly string[] _imageFileExtensions = new[]
    {
        ".tif", ".tiff", ".gif", ".jpeg", ".jpg", ".jif", ".jfif", ".jp2", ".jpx", ".j2k", ".j2c", ".fpx", ".pcd", ".png", ".heic", ".dng"
    };

    /// <inheritdoc />
    public bool FileTypeSupported(string extension)
    {
        return _imageFileExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase);
    }

    /// <inheritdoc />
    public DateTime ReadDate(string filePath)
    {
        try
        {
            using var image = new MagickImage(filePath);
            var profile = image.GetExifProfile();
            if (profile is not null)
            {
                // Try DateTimeDigitized first (most reliable)
                var dateExifValue = profile.GetValue(ExifTag.DateTimeDigitized);
                if (dateExifValue != null &&
                    DateTime.TryParseExact(dateExifValue.Value, "yyyy:MM:dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
                {
                    return date;
                }

                // Fallback to DateTimeOriginal
                dateExifValue = profile.GetValue(ExifTag.DateTimeOriginal);
                if (dateExifValue != null &&
                    DateTime.TryParseExact(dateExifValue.Value, "yyyy:MM:dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
                {
                    return date;
                }

                // Fallback to DateTime
                dateExifValue = profile.GetValue(ExifTag.DateTime);
                if (dateExifValue != null &&
                    DateTime.TryParseExact(dateExifValue.Value, "yyyy:MM:dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
                {
                    return date;
                }
            }
        }
        catch (Exception ex)
        {
            // Log but don't fail - fall back to file system date
            Console.WriteLine($"Warning: Could not read EXIF data from {filePath}: {ex.Message}");
        }

        return ReaderHelper.GetDefault(filePath);
    }
}