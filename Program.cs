using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace LaserMod
{
    class Program
    {

        const double totalizeError = 0.286; // the counter readings are smaller by this value on average
        static double[] data;

        static void Main(string[] args)
        {
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

            // default parameters
            //string filename = @"E:\LaserModData\BEV2\T010BEV2_A.csv";
            string filename = @"/Volumes/NO NAME/LaserModData/S01/T010S01.csv";
            int windowSize = 1000;
            OutputType outputType = OutputType.Verbose;

            // command line logic
            if (args.Length > 0)
                filename = args[0];
            if (args.Length == 2)
                windowSize = int.Parse(args[1]);
            if (Path.GetExtension(filename) == "")
                filename = Path.ChangeExtension(filename, ".csv");

            // process a single file
            ReadEvaluatePrint(filename, windowSize, outputType);
            ReadEvaluatePrint(filename, windowSize, OutputType.Succinct);
            Console.WriteLine();

            //process a single file with different window sizes
            for (int i = 450; i < 550; i += 1)
                ReadEvaluatePrint(filename, i, OutputType.CsvLine);
            for (int i = 100; i < 1000; i += 7)
                ReadEvaluatePrint(filename, i, OutputType.SingleLine);
            for (int i = 1000; i < 5000; i += 19)
                ReadEvaluatePrint(filename, i, OutputType.SingleLine);
            for (int i = 5000; i < 10001; i += 523)
                ReadEvaluatePrint(filename, i, OutputType.SingleLine);

            // process a whole directory of files
            // string workingDirectory = @"/Volumes/NO NAME/LaserModData/S01/";     // Directory.GetCurrentDirectory();
            string workingDirectory = Directory.GetCurrentDirectory();

            string[] filenames = Directory.GetFiles(workingDirectory, @"*.csv");
            Array.Sort(filenames);
            foreach (string fn in filenames)
                ReadEvaluatePrint(fn, windowSize, OutputType.SingleLine);

        }



        private static ParameterContainer ReadEvaluatePrint(string filename, int windowSize, OutputType outputType)
        {
            double gateTime = EstimateGateTimeFromFileName(filename);
            data = ReadDataFromFile(filename);
            // prepare the container for the data
            ParameterContainer container = new ParameterContainer(gateTime);
            container.Filename = Path.GetFileNameWithoutExtension(filename);
            // overall calculation
            TotalFitter totalFitter = new TotalFitter(data);
            container.SetParametersFromFitter(totalFitter);
            // moving window calculation
            MovingFitter movingFitter = new MovingFitter(data);
            movingFitter.FitWithWindowSize(windowSize);
            container.SetParametersFromFitter(movingFitter);
            Console.WriteLine(container.ToOutputString(outputType));
            return container;
        }
    
        private static double[] ReadDataFromFile(string filename)
        {
            List<double> counterReadings = new List<double>();
            var reader = new StreamReader(File.OpenRead(filename));
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                double y = MyParse(line);
                if (!double.IsNaN(y))
                {
                    counterReadings.Add(y + totalizeError);
                }
            }
            reader.Close();
            return counterReadings.ToArray();
        }

        private static double EstimateGateTimeFromFileName(string filename)
        {
            string baseFilename = Path.GetFileNameWithoutExtension(filename);
            string token = Regex.Match(baseFilename, @"\d+").Value;
            int microSeconds = int.Parse(token);
            return (double)microSeconds * 1e-6;
        }

        private static double MyParse(string line)
        {
            if (double.TryParse(line, out double value))
                return value;
            else
                return double.NaN;
        }
    }

    public enum OutputType
    {
        None,
        SingleLine,
        CsvLine,
        CsvHeader,
        Verbose,
        Succinct
    }
}
