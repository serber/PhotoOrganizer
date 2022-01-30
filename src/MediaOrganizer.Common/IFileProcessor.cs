namespace MediaOrganizer.Common;

/// <summary>
/// Media files processor
/// </summary>
public interface IFileProcessor
{
    /// <summary>
    /// Process files from input directory
    /// </summary>
    void ProcessFiles(string inputDirectory, string outputDirectory, string extension = "", string fileNameFormat = "yyyyMMdd_HHmmss");
}