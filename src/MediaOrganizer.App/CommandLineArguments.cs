using CommandLine;

namespace MediaOrganizer.App;

/// <summary>
/// Command line arguments
/// </summary>
internal class CommandLineArguments
{
    [Option('i', "input", Required = true, HelpText = "Input directory")]
    public string Input { get; set; } = null!;

    [Option('o', "output", Required = true, HelpText = "Output directory")]
    public string Output { get; set; } = null!;

    [Option('f', "format", Required = false, HelpText = "File name format", Default = "yyyyMMdd_HHmmss")]
    public string Format { get; set; } = null!;

    [Option('e', "extension", Required = false, HelpText = "Extension")]
    public string Extension { get; set; } = null!;
}