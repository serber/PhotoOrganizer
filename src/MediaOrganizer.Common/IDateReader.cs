namespace MediaOrganizer.Common;

/// <summary>
/// Date time reader
/// </summary>
public interface IDateReader
{
    /// <summary>
    /// Checks is file type supported by reader
    /// </summary>
    bool FileTypeSupported(string extension);

    /// <summary>
    /// Reads date and time from file metadata
    /// </summary>
    DateTime ReadDate(string filePath);
}