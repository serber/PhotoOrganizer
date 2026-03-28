namespace MediaOrganizer.Common;

/// <inheritdoc />
public class FileProcessor : IFileProcessor
{
    private readonly IEnumerable<IDateReader> _readers;

    public FileProcessor(IEnumerable<IDateReader> readers)
    {
        _readers = readers ?? throw new ArgumentNullException(nameof(readers));
    }

    /// <inheritdoc />
    public void ProcessFiles(string inputDirectory, string outputDirectory, string extension = "", string fileNameFormat = "yyyyMMdd_HHmmss")
    {
        if (string.IsNullOrWhiteSpace(inputDirectory))
            throw new ArgumentException("Input directory cannot be null or empty", nameof(inputDirectory));
        
        if (string.IsNullOrWhiteSpace(outputDirectory))
            throw new ArgumentException("Output directory cannot be null or empty", nameof(outputDirectory));

        if (!Directory.Exists(inputDirectory))
            throw new DirectoryNotFoundException($"Input directory does not exist: {inputDirectory}");

        // Ensure output directory exists
        if (!Directory.Exists(outputDirectory))
        {
            Directory.CreateDirectory(outputDirectory);
        }

        var searchPattern = string.IsNullOrWhiteSpace(extension)
            ? "*"
            : $"*.{extension.TrimStart('.')}";

        var files = Directory.GetFiles(inputDirectory, searchPattern, SearchOption.AllDirectories);
        
        Console.WriteLine($"Found {files.Length} file(s) to process...");

        int processed = 0;
        int skipped = 0;
        int errors = 0;

        foreach (var file in files)
        {
            try
            {
                var fileInfo = new FileInfo(file);
                
                // Skip if file doesn't exist (might have been deleted)
                if (!fileInfo.Exists)
                {
                    skipped++;
                    continue;
                }

                var reader = GetReader(fileInfo.Extension);
                if (reader == null)
                {
                    Console.WriteLine($"No reader found for file: {file}");
                    skipped++;
                    continue;
                }

                var fileDate = reader.ReadDate(file);
                
                // Validate date is reasonable (not MinValue or too far in future)
                if (fileDate == DateTime.MinValue || fileDate.Year < 1900 || fileDate.Year > DateTime.Now.Year + 1)
                {
                    Console.WriteLine($"Warning: Invalid date {fileDate:yyyy-MM-dd} for file: {file}, using file system date");
                    fileDate = ReaderHelper.GetDefault(file);
                }

                var rootDirectoryPath = Path.Combine(outputDirectory, $"{fileDate.Year}", $"{fileDate.Month:00}");
                if (!Directory.Exists(rootDirectoryPath))
                {
                    Directory.CreateDirectory(rootDirectoryPath);
                }

                var outputFilePath = Path.Combine(rootDirectoryPath, Path.ChangeExtension(fileDate.ToString(fileNameFormat), fileInfo.Extension)).ToLower();

                if (File.Exists(outputFilePath))
                {
                    var existingFileInfo = new FileInfo(outputFilePath);
                    if (fileInfo.Length == existingFileInfo.Length)
                    {
                        Console.WriteLine($"Duplicate file found (same size), deleting: {file}");
                        fileInfo.Delete();
                        skipped++;
                        continue;
                    }
                    else
                    {
                        outputFilePath = GetAvailableFileName(rootDirectoryPath, fileDate, fileNameFormat, fileInfo.Extension);
                        if (string.IsNullOrEmpty(outputFilePath))
                        {
                            Console.WriteLine($"Error: Could not generate available filename for: {file}");
                            errors++;
                            continue;
                        }
                        outputFilePath = outputFilePath.ToLower();
                    }
                }

                fileInfo.MoveTo(outputFilePath);
                processed++;
                
                if (processed % 10 == 0)
                {
                    Console.WriteLine($"Processed {processed} file(s)...");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing file {file}: {ex.Message}");
                errors++;
            }
        }

        Console.WriteLine($"\nProcessing complete: {processed} processed, {skipped} skipped, {errors} errors");
    }

    private IDateReader? GetReader(string extension)
    {
        return _readers.FirstOrDefault(x => x.FileTypeSupported(extension));
    }

    private static string GetAvailableFileName(string outputDirectory, DateTime fileDate, string format, string extension)
    {
        const int maxAttempts = 500;
        var originalDate = fileDate;
        
        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            var testDate = originalDate.AddSeconds(attempt);
            var filePath = Path.Combine(outputDirectory, Path.ChangeExtension(testDate.ToString(format), extension));
            
            if (!File.Exists(filePath))
            {
                return filePath;
            }
        }

        // Fallback: add index to filename
        for (int index = 1; index <= 999; index++)
        {
            var filePath = Path.Combine(outputDirectory, Path.ChangeExtension($"{originalDate.ToString(format)}_{index:D3}", extension));
            if (!File.Exists(filePath))
            {
                return filePath;
            }
        }

        return string.Empty;
    }
}