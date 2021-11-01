using At.Matus.StatisticPod;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;

namespace LaserMod
{
    class Program
    {

        const double totalizeError = 0.286; // the counter readings are smaller by this value on average
        static double[] data;
        static ParameterContainer container;

        static void Main(string[] args)
        {
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

            // default parameters
            //string filename = @"E:\LaserModData\BEV2\T010BEV2_A.csv";
            string filename = @"/Volumes/NO NAME/LaserModData/S01/T020S01.csv";
            int windowSize = 1000;
            OutputType outputType = OutputType.Verbose;

            // command line logic
            if (args.Length == 2)
                windowSize = int.Parse(args[1]);
            if (args.Length == 1)
            {
                filename = args[0];
                if (Path.GetExtension(filename) == "")
                    filename = Path.ChangeExtension(filename, ".csv");
                ReadEvaluatePrint(filename, windowSize, outputType);
            }
            if (args.Length == 0)
            {
                // process a whole directory of files
                StatisticPod spMpp = new StatisticPod("Mpp from all files");
                StatisticPod spFc = new StatisticPod("fc from all files");
                //string workingDirectory = @"/Volumes/NO NAME/LaserModData/S01/";
                string workingDirectory = Directory.GetCurrentDirectory();
                string[] filenames = Directory.GetFiles(workingDirectory, @"*.csv");
                Array.Sort(filenames);
                foreach (string fn in filenames)
                {
                    ReadEvaluatePrint(fn, windowSize, OutputType.SingleLine);
                    spMpp.Update(container.Mpp);
                    spFc.Update(container.BeatStat);
                }
                Console.WriteLine();
                Console.WriteLine($"{spMpp.SampleSize,4} files -> Mpp = {spMpp.AverageValue * 1e-6:F3} ± {spMpp.StandardDeviation * 1e-6:F3} MHz");
                Console.WriteLine($"           ->  fc = {spFc.AverageValue * 1e-6:F3} ± {spFc.StandardDeviation * 1e-6:F3} MHz");
                Console.WriteLine();
            }

        }


        private static void ReadEvaluatePrint(string filename, int windowSize, OutputType outputType)
        {
            ReadData(filename);
            Evaluate(windowSize);
            PrintParameters(outputType);
        }

        private static void ReadData(string filename)
        {
            double gateTime = EstimateGateTimeFromFileName(filename);
            data = ReadDataFromFile(filename);
            container = new ParameterContainer(gateTime);
            container.Filename = Path.GetFileNameWithoutExtension(filename);
        }

        private static void PrintParameters(OutputType outputType)
        {
            Console.WriteLine(container.ToOutputString(outputType));
        }

        private static void Evaluate(int windowSize)
        {
            // overall calculation
            TotalFitter totalFitter = new TotalFitter(data);
            container.SetParametersFromFitter(totalFitter);
            // moving window calculation
            MovingFitter movingFitter = new MovingFitter(data);
            movingFitter.FitWithWindowSize(windowSize);
            container.SetParametersFromFitter(movingFitter);
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
