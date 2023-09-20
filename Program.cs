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
        static string outputFilename;

        static void Main(string[] args)
        {
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

            // default parameters
            // @"E:\LaserModData\BEV2\T010BEV2_A.csv";
            // @"/Volumes/NO NAME/LaserModData/S01/T020S01.csv";
            int windowSize = 1000;
            OutputType outputType = OutputType.Verbose;

            // command line logic
            // two arguments: first file name, second window size
            // one argument: file name
            // no argument: process all files in working directory
            if (args.Length == 2)
            {
                windowSize = int.Parse(args[1]);
            }
            if (args.Length >= 1)
            {
                string filename = args[0];
                if (Path.GetExtension(filename) == "")
                    filename = Path.ChangeExtension(filename, ".csv");
                outputFilename = Path.ChangeExtension(filename, "prn");
                ReadEvaluatePrint(filename, windowSize, outputType);
            }
            if (args.Length == 0)
            {
                // process a whole directory of files and make simple statistic
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
                    spFc.Update(container.CarrierStat);
                }
                Console.WriteLine();
                Console.WriteLine($"{spMpp.SampleSize,4} files -> Mpp =  {spMpp.AverageValue * 1e-6:F3} ± {spMpp.StandardDeviation * 1e-6:F3} MHz");
                Console.WriteLine($"           ->  fc = {spFc.AverageValue * 1e-6:F3} ± {spFc.StandardDeviation * 1e-6:F3} MHz");
                Console.WriteLine();
            }

        }

        //*********************************************************************************************

        private static void ReadEvaluatePrint(string filename, int windowSize, OutputType outputType)
        {
            ReadData(filename);
            if (container.IsGateTimeTooLong)
            {
                Console.WriteLine();
                Console.WriteLine("Warning: gate time to long! Some parameters may be invalid!");
            }
            EvaluateAll(windowSize);
            int optimalWindowSize = EstimateOptimalWindowSize(windowSize, container.RawTau);
            EvaluatePiecewise(optimalWindowSize);
            PrintParameters(outputType);
        }

        private static int EstimateOptimalWindowSize(int maxWindowSize, double tau)
        {
            double minimumFringeFraction = double.PositiveInfinity;
            int optimalWindowSize = maxWindowSize;
            for (int i = maxWindowSize/10; i <= maxWindowSize; i++)
            {
                double fringe = i / tau;
                double fringeFraction = Math.Abs(fringe - Math.Round(fringe));
                if (fringeFraction<=minimumFringeFraction)
                {
                    minimumFringeFraction = fringeFraction;
                    optimalWindowSize = i;
                }
            }
            return optimalWindowSize;
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

        private static void EvaluateAll(int windowSize)
        {
            // overall calculation
            TotalFitter totalFitter = new TotalFitter(data);
            FftPeriodFitter fft = new FftPeriodFitter(totalFitter);

            Console.WriteLine($"FFT:  f={fft.RawFrequency}   P={fft.RawAmplitude}");

            container.SetParametersFromFitter(totalFitter);
            // moving window calculation
            EvaluatePiecewise(windowSize);
        }

        private static void EvaluatePiecewise(int windowSize)
        {
            // moving window calculation
            MovingFitter movingFitter = new MovingFitter(data);
            movingFitter.FitWithWindowSize(windowSize);
            container.SetParametersFromFitter(movingFitter);
        }

        // the raw readings are corrected by the totalize error!
        private static double[] ReadDataFromFile(string filename)
        {
            List<double> counterReadings = new List<double>();
            StreamReader reader = new StreamReader(File.OpenRead(filename));
            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();
                double y = MyParseDouble(line);
                if (!double.IsNaN(y))
                {
                    counterReadings.Add(y + totalizeError);
                }
            }
            reader.Close();
            return counterReadings.ToArray();
        }

        // the first valid integer in the file name is interpreted as the gate time in µs
        // the gate time is returned in s
        private static double EstimateGateTimeFromFileName(string filename)
        {
            string baseFilename = Path.GetFileNameWithoutExtension(filename);
            string token = Regex.Match(baseFilename, @"\d+").Value;
            return int.Parse(token) * 1e-6;
        }

        private static double MyParseDouble(string line)
        {
            return double.TryParse(line, out double value) ? value : double.NaN;
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
