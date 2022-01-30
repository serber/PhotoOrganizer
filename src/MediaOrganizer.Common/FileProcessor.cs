namespace MediaOrganizer.Common;

/// <inheritdoc />
public class FileProcessor : IFileProcessor
{
    private readonly IEnumerable<IDateReader> _readers;

    public FileProcessor(IEnumerable<IDateReader> readers)
    {
        _readers = readers;
    }

    /// <inheritdoc />
    public void ProcessFiles(string inputDirectory, string outputDirectory, string extension = "", string fileNameFormat = "yyyyMMdd_HHmmss")
    {
        var files = string.IsNullOrEmpty(extension)
                ? Directory.GetFiles(inputDirectory, string.Empty, SearchOption.AllDirectories)
                : Directory.GetFiles(inputDirectory, $"*.{extension}", SearchOption.AllDirectories);

        foreach (var file in files)
        {
            var fileInfo = new FileInfo(file);

            var reader = GetReader(fileInfo.Extension);
            if (reader == null)
            {
                continue;
            }

            var fileDate = reader.ReadDate(file);

            var rootDirectoryPath = Path.Combine(outputDirectory, $"{fileDate.Year}", $"{fileDate.Month:00}");
            if (!Directory.Exists(rootDirectoryPath))
            {
                Directory.CreateDirectory(rootDirectoryPath);
            }

            var outputFilePath = Path.Combine(rootDirectoryPath, Path.ChangeExtension(fileDate.ToString(fileNameFormat), fileInfo.Extension)).ToLower();

            if (File.Exists(outputFilePath))
            {
                if (fileInfo.Length == new FileInfo(outputFilePath).Length)
                {
                    fileInfo.Delete();
                }
                else
                {
                    outputFilePath = GetAvailableFileName(rootDirectoryPath, fileDate, fileNameFormat, fileInfo.Extension).ToLower();
                    if (!string.IsNullOrEmpty(outputFilePath))
                    {
                        fileInfo.MoveTo(outputFilePath);
                    }
                }
            }
            else
            {
                fileInfo.MoveTo(outputFilePath);
            }
        }
    }

    private IDateReader? GetReader(string extension)
    {
        return _readers.FirstOrDefault(x => x.FileTypeSupported(extension));
    }

    private static string GetAvailableFileName(string outputDirectory, DateTime fileDate, string format, string extension)
    {
        int index = 1;
        while (index < 500)
        {
            fileDate = fileDate.AddSeconds(1);
            string filePath = Path.Combine(outputDirectory, Path.ChangeExtension(fileDate.ToString(format), extension));
            if (File.Exists(filePath))
            {
                index++;
                continue;
            }

            return filePath;
        }

        return string.Empty;
    }
}