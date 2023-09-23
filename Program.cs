using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;

namespace LaserMod
{
    class Program
    {
        static void Main(string[] args)
        {
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

            // default parameters
            // @"E:\LaserModData\BEV2\T010BEV2_A.csv";
            // @"/Volumes/NO NAME/LaserModData/S01/T020S01.csv";
            int evaluationPeriods = 100;
            OutputType outputType = OutputType.Succinct;

            // command line logic
            // two arguments: first (file name), second (window size / mod period)
            // one argument: file name
            // no argument: exit
            if (args.Length == 0)
            {
                Console.WriteLine("! no file name provided !");
                return;
            }
            if (args.Length == 2)
            {
                evaluationPeriods = int.Parse(args[1]);
            }
            string filename = args[0];
            if (Path.GetExtension(filename) == "")
                filename = Path.ChangeExtension(filename, ".csv");
            string outputFilename = Path.ChangeExtension(filename, ".prn");

            double[] data = ReadDataFromFile(filename);
            double gateTime = EstimateGateTimeFromFileName(filename);

            ParameterContainer container = new ParameterContainer(gateTime);
            container.Filename = Path.GetFileName(filename);
            if (container.IsGateTimeTooLong)
            {
                Console.WriteLine("! Warning: gate time too long! Some parameters may be invalid!");
            }

            TotalFitter totalFitter = new TotalFitter(data);
            container.SetParametersFromFitter(totalFitter);

            FftPeriodEstimator fftEstimator = new FftPeriodEstimator(totalFitter);
            double rawPeriod = fftEstimator.RawModulationPeriod;
            container.SetParametersFromFitter(fftEstimator);

            int windowSize = (int)(rawPeriod * evaluationPeriods);
            int optimalWindowSize = EstimateOptimalWindowSize(windowSize, rawPeriod);

            MovingFitter movingFitter = new MovingFitter(data, rawPeriod);
            movingFitter.FitWithWindowSize(optimalWindowSize);
            container.SetParametersFromFitter(movingFitter);

            PrintParameters(container, outputType);
            WriteParameters(container, OutputType.Verbose, outputFilename);

        }

        //*********************************************************************************************

        private static int EstimateOptimalWindowSize(int maxWindowSize, double rawPeriod)
        {
            double minimumFringeFraction = double.PositiveInfinity;
            int optimalWindowSize = maxWindowSize;
            for (int testSize = maxWindowSize / 2; testSize <= maxWindowSize * 2; testSize++)
            {
                double fringe = testSize / rawPeriod;
                double fringeFraction = Math.Abs(fringe - Math.Round(fringe));
                if (fringeFraction <= minimumFringeFraction)
                {
                    minimumFringeFraction = fringeFraction;
                    optimalWindowSize = testSize;
                }
            }
            //Console.WriteLine($"debug - tau:{rawPeriod:F4}  optSize:{optimalWindowSize}  minff:{minimumFringeFraction:F4}");
            return optimalWindowSize;
        }

        private static void PrintParameters(ParameterContainer container, OutputType outputType)
        {
            Console.WriteLine();
            Console.WriteLine(container.ToOutputString(outputType));
        }

        private static void WriteParameters(ParameterContainer container, OutputType outputType, string outputFilename)
        {
            using(StreamWriter writer = new StreamWriter(outputFilename, false))
            {
                writer.Write(container.ToOutputString(outputType));
            }
        }

        // the raw readings are corrected by the totalize error!
        // the first and the last line are discarded
        private static double[] ReadDataFromFile(string filename)
        {
            List<double> counterReadings = new List<double>();
            using (StreamReader reader = new StreamReader(File.OpenRead(filename)))
            {
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    double y = MyParseDouble(line);
                    if (!double.IsNaN(y))
                    {
                        counterReadings.Add(y + InstrumentConstants.TotalizeCorrection);
                    }
                }
            }
            counterReadings.RemoveAt(counterReadings.Count-1);
            counterReadings.RemoveAt(0);
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

        private static double MyParseDouble(string line) => double.TryParse(line, out double value) ? value : double.NaN;
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
