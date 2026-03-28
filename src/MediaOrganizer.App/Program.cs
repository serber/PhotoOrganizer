using CommandLine;
using MediaOrganizer.App;
using MediaOrganizer.Common;
using MediaOrganizer.Image;
using MediaOrganizer.Video;

var exitCode = Parser.Default.ParseArguments<CommandLineArguments>(args)
    .MapResult(
        RunOptionsAndReturnExitCode,
        HandleParseError);

return exitCode;

static int RunOptionsAndReturnExitCode(CommandLineArguments arguments)
{
    try
    {
        Console.WriteLine("PhotoOrganizer - Media File Organizer");
        Console.WriteLine("=====================================\n");
        Console.WriteLine($"Input directory: {arguments.Input}");
        Console.WriteLine($"Output directory: {arguments.Output}");
        if (!string.IsNullOrWhiteSpace(arguments.Extension))
        {
            Console.WriteLine($"Extension filter: {arguments.Extension}");
        }
        Console.WriteLine($"File name format: {arguments.Format}\n");

        var readers = new List<IDateReader>
        {
            new ImageDateReader(),
            new VideoDateReader(),
            new AppleVideoDataReader()
        };

        var processor = new FileProcessor(readers);
        processor.ProcessFiles(arguments.Input, arguments.Output, arguments.Extension, arguments.Format);
        
        return 0;
    }
    catch (DirectoryNotFoundException ex)
    {
        Console.WriteLine($"Error: Directory not found - {ex.Message}");
        return 1;
    }
    catch (UnauthorizedAccessException ex)
    {
        Console.WriteLine($"Error: Access denied - {ex.Message}");
        return 1;
    }
    catch (ArgumentException ex)
    {
        Console.WriteLine($"Error: Invalid argument - {ex.Message}");
        return 1;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Unexpected error: {ex.Message}");
        Console.WriteLine(ex.StackTrace);
        return 1;
    }
}

static int HandleParseError(IEnumerable<Error> errors)
{
    Console.WriteLine("Error: Not all required arguments passed");
    Console.WriteLine(string.Join(Environment.NewLine, errors.Select(x => x.ToString())));
    return 1;
}