using CommandLine;
using MediaOrganizer.App;
using MediaOrganizer.Common;
using MediaOrganizer.Image;
using MediaOrganizer.Video;

Parser.Default.ParseArguments<CommandLineArguments>(args)
    .WithParsed(RunOptionsAndReturnExitCode)
    .WithNotParsed(HandleParseError);

static void RunOptionsAndReturnExitCode(CommandLineArguments arguments)
{
    try
    {
        new FileProcessor(new List<IDateReader> { new ImageDateReader(), new VideoDateReader() })
            .ProcessFiles(arguments.Input, arguments.Output, arguments.Extension, arguments.Format);
    }
    catch (Exception e)
    {
        Console.WriteLine(e);

        Console.ReadKey();
    }
}

static void HandleParseError(IEnumerable<Error> errors)
{
    Console.WriteLine("Not all required arguments passed");
    Console.WriteLine(string.Join(Environment.NewLine, errors.Select(x => x.ToString())));
    Console.ReadKey();
}