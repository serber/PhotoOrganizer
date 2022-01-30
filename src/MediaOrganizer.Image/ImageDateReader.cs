using ImageMagick;
using MediaOrganizer.Common;
using System.Globalization;

namespace MediaOrganizer.Image;

/// <inheritdoc />
public class ImageDateReader : IDateReader
{
    private readonly string[] _imageFileExtensions = new[]
    {
        ".tif", ".tiff", ".gif", ".jpeg", ".jpg", ".jif", ".jfif", ".jp2", ".jpx", ".j2k", ".j2c", ".fpx", ".pcd", ".png", ".heic"
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
            using MagickImage image = new MagickImage(filePath);
            var profile = image.GetExifProfile();
            if (profile != null)
            {
                var dateExifValue = profile.GetValue(ExifTag.DateTimeDigitized);
                if (dateExifValue != null &&
                    DateTime.TryParseExact(dateExifValue.Value, "yyyy:MM:dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
                {
                    return date;
                }
            }
        }
        catch
        {
            //---   ignore
        }

        return ReaderHelper.GetDefault(filePath);
    }
}