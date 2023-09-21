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

        static double[] data;
        static double rawPeriod; //TODO bad!!!
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
            // no argument: exit
            if (args.Length == 0)
            {
                Console.WriteLine("! no file name provided !");
                return;
            }
            if (args.Length == 2)
            {
                windowSize = int.Parse(args[1]);
            }
            string filename = args[0];
            if (Path.GetExtension(filename) == "")
                filename = Path.ChangeExtension(filename, ".csv");
            outputFilename = Path.ChangeExtension(filename, "prn");
            //ReadEvaluatePrint(filename, windowSize, outputType);

            double[] data = ReadDataFromFile(filename);
            double gateTime = EstimateGateTimeFromFileName(filename);

            container = new ParameterContainer(gateTime);

            if (container.IsGateTimeTooLong)
            {
                Console.WriteLine("! Warning: gate time too long! Some parameters may be invalid!");
            }
            TotalFitter totalFitter = new TotalFitter(data);
            FftPeriodEstimator fft = new FftPeriodEstimator(totalFitter);
            rawPeriod = fft.RawModulationPeriod;
            container.SetParametersFromFitter(totalFitter);
            container.SetParametersFromFitter(fft);
            int optimalWindowSize = EstimateOptimalWindowSize(windowSize, rawPeriod);
            MovingFitter movingFitter = new MovingFitter(data, rawPeriod);
            movingFitter.FitWithWindowSize(optimalWindowSize);
            container.SetParametersFromFitter(movingFitter);

            PrintParameters(outputType);
        }

        //*********************************************************************************************

        //private static void ReadEvaluatePrint(string filename, int windowSize, OutputType outputType)
        //{
        //    ReadData(filename);
        //    if (container.IsGateTimeTooLong)
        //    {
        //        Console.WriteLine();
        //        Console.WriteLine("Warning: gate time too long! Some parameters may be invalid!");
        //    }
        //    EvaluateAll(windowSize);
        //    int optimalWindowSize = EstimateOptimalWindowSize(windowSize, container.RawTau);
        //    EvaluatePiecewise(optimalWindowSize, rawPeriod);
        //    PrintParameters(outputType);
        //}

        private static int EstimateOptimalWindowSize(int maxWindowSize, double rawPeriod)
        {
            double minimumFringeFraction = double.PositiveInfinity;
            int optimalWindowSize = maxWindowSize;
            for (int i = maxWindowSize / 10; i <= maxWindowSize; i++)
            {
                double fringe = i / rawPeriod;
                double fringeFraction = Math.Abs(fringe - Math.Round(fringe));
                if (fringeFraction <= minimumFringeFraction)
                {
                    minimumFringeFraction = fringeFraction;
                    optimalWindowSize = i;
                }
            }
            return optimalWindowSize;
        }

        //private static void ReadData(string filename)
        //{
        //    double gateTime = EstimateGateTimeFromFileName(filename);
        //    data = ReadDataFromFile(filename);
        //    container = new ParameterContainer(gateTime);
        //    container.Filename = Path.GetFileNameWithoutExtension(filename);
        //}

        private static void PrintParameters(OutputType outputType) => Console.WriteLine(container.ToOutputString(outputType));

        //private static void EvaluateAll(int windowSize)
        //{
        //    // overall calculation
        //    TotalFitter totalFitter = new TotalFitter(data);
        //    FftPeriodEstimator fft = new FftPeriodEstimator(totalFitter);
        //    rawPeriod = fft.RawModulationPeriod;

        //    container.SetParametersFromFitter(totalFitter);
        //    container.SetParametersFromFitter(fft);
        //    // moving window calculation
        //    EvaluatePiecewise(windowSize, rawPeriod);
        //}

        //private static void EvaluatePiecewise(int windowSize, double rawTau)
        //{
        //    // moving window calculation
        //    MovingFitter movingFitter = new MovingFitter(data, rawTau);
        //    movingFitter.FitWithWindowSize(windowSize);
        //    container.SetParametersFromFitter(movingFitter);
        //}

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
                    counterReadings.Add(y + InstrumentConstants.TotalizeCorrection);
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
