using CommandLine;

namespace PhotoOrganizer
{
    /// <summary>
    /// Command line arguments
    /// </summary>
    internal class CommandLineArguments
    {
        [Option('i', "input", Required = true, HelpText = "Input directory")]
        public string Input { get; set; }

        [Option('o', "output", Required = true, HelpText = "Output directory")]
        public string Output { get; set; }

        [Option('f', "format", Required = false, HelpText = "File name format", Default = "yyyyMMdd_HHmmss")]
        public string Format { get; set; }

        [Option('e', "extension", Required = false, HelpText = "Extension", Default = "jpg")]
        public string Extension { get; set; }
    }
}