using CommandLine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PhotoOrganizer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Parser.Default.ParseArguments<CommandLineArguments>(args)
                          .WithParsed(arguments => RunOptionsAndReturnExitCode(arguments))
                          .WithNotParsed((errors) => HandleParseError(errors));
        }

        /// <summary>
        /// Handle command line argument parse error
        /// </summary>
        /// <param name="errors">Errors</param>
        private static void HandleParseError(IEnumerable<Error> errors)
        {
            Console.WriteLine($"Not all required argumets passed");
            Console.WriteLine(string.Join(Environment.NewLine, errors.Select(x => x.ToString())));
            Console.ReadKey();
        }

        /// <summary>
        /// Execute program
        /// </summary>
        /// <param name="arguments">Parsed arguments</param>
        private static void RunOptionsAndReturnExitCode(CommandLineArguments arguments)
        {
            var files = string.IsNullOrEmpty(arguments.Extension)
                ? Directory.GetFiles(arguments.Input, string.Empty, SearchOption.AllDirectories)
                : Directory.GetFiles(arguments.Input, $"*.{arguments.Extension}", SearchOption.AllDirectories);
            
            var fileProcessor = new FileProcessor();

            try
            {
                for (int i = 0; i < files.Length; i++)
                {
                    if (i % 100 == 0)
                    {
                        Console.WriteLine($"Processed {i} of {files.Length}");
                    }

                    fileProcessor.ProcessFile(files[i], arguments.Output, arguments.Format);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);

                Console.ReadKey();
            }
        }
    }
}