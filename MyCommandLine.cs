using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace LaserMod
{
    public class MyCommandLine
    {
        public MyCommandLine(string[] args)
        {
            ParseCommandLine(args);
            GenerateFileNames();
        }

        public OutputType Verbosity { get; private set; } = OutputType.Succinct;
        public int EvaluationPeriods { get; private set; } = InstrumentConstants.EvaluationPeriods;
        public string UserComment { get; private set; } = string.Empty;
        public string InputFilename { get; private set; }
        public string OutputFilename { get; private set; }

        private void ParseCommandLine(string[] args)
        {
            if (args.Length == 0) return;
            foreach (var arg in args)
            {
                switch (arg)
                {
                    case "-v":
                    case "-v+":
                        Verbosity = OutputType.Verbose;
                        break;
                    case "-v-":
                        Verbosity = OutputType.Succinct;
                        break;
                    case "--test":
                        Verbosity = OutputType.TestCase;
                        break;
                    case "--csv":
                        Verbosity = OutputType.Csv;
                        break;
                    case "--help":
                        PrintHelpAndExit(0);
                        break;
                    default:
                        ParseCommand(arg);
                        break;
                }
            }
        }

        private void GenerateFileNames()
        {
            switch (fileNames.Count)
            {
                case 0:
                    Console.WriteLine("no file name given!");
                    PrintHelpAndExit(1);
                    break;
                case 1:
                    InputFilename = fileNames[0];
                    if (Path.GetExtension(InputFilename) == "")
                        InputFilename = Path.ChangeExtension(InputFilename, ".csv");
                    OutputFilename = Path.ChangeExtension(InputFilename, ".prn");
                    break;
                case 2:
                    InputFilename = fileNames[0];
                    if (Path.GetExtension(InputFilename) == "")
                        InputFilename = Path.ChangeExtension(InputFilename, ".csv");
                    OutputFilename = fileNames[1];
                    if (Path.GetExtension(OutputFilename) == "")
                        OutputFilename = Path.ChangeExtension(OutputFilename, ".prn");
                    break;
                default:
                    Console.WriteLine("too many file names given!");
                    PrintHelpAndExit(2);
                    break;
            }
        }

        private void ParseCommand(string arg)
        {
            if(arg.StartsWith("-"))
            {
                if(arg.StartsWith("-n"))
                {
                    EvaluationPeriods = int.Parse(arg.Substring(2));
                    return;
                }
                if(arg.StartsWith("--comment="))
                {
                    UserComment = arg.Substring(10).Replace("\"", "");
                    return;
                }
                Console.WriteLine($"unknown option: {arg}");
                PrintHelpAndExit(3);
            }
            else 
            { fileNames.Add(arg); }
        }

        private void PrintHelpAndExit(int exitcode)
        {
            Console.WriteLine();
            Console.WriteLine($"Usage: {Assembly.GetExecutingAssembly().GetName().Name} filename [filename] [options]");
            Console.WriteLine();
            Console.WriteLine($"options:");
            Console.WriteLine($"   -v     : verbosity for console output");
            Console.WriteLine($"   -n     : number of periods to evaluate ({EvaluationPeriods})");
            Console.WriteLine($"   --csv  : output for csv file");
            Console.WriteLine($"   --test : mode for validation use");
            Console.WriteLine($"   --help : this help screen");
            Console.WriteLine();
            Environment.Exit(exitcode);
        }

        private readonly List<string> fileNames = new List<string>();

    }
}
