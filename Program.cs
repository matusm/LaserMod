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
        // the following constants are valid for the setup at BEV
        const double modulationFrequencyCalFactor = 40848.0222; // Hz/point
        const double maxGateTime = 22e-6;   // 22 µs, for longer gatetimes the modulation frequency can not be estimated
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
            // EvaluateAndPrint(filename, windowSize, outputType);

            //process a single file with different window sizes
            for (int i = 450; i < 550; i += 1)
                ReadEvaluatePrint(filename, i, OutputType.SingleLine);
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

        

        private static void ReadEvaluatePrint(string filename, int windowSize, OutputType outputType)
        {
            double gateTime = EstimateGateTimeFromFileName(filename);
            data = ReadDataFromFile(filename);

            // overall calculation
            TotalFitter totalFitter = new TotalFitter(data);

            // moving window calculation
            MovingFitter movingFitter = new MovingFitter(data);
            movingFitter.FitWithWindowSize(windowSize);

            // collate all parameters of interest
            double modulationFrequency = modulationFrequencyCalFactor * movingFitter.ModulationFrequency;
            double modulationFrequencyDisp = modulationFrequencyCalFactor * movingFitter.ModulationFrequencyDispersion;
            double tau = movingFitter.ModulationPeriod / modulationFrequencyCalFactor;
            double tauDisp = movingFitter.ModulationPeriodDispersion / modulationFrequencyCalFactor;
            double modTau = 1 / modulationFrequency; // in s

            double beatTotal = TotalizeToMHz(totalFitter.Carrier);
            double beatDispTotal = TotalizeToMHz(totalFitter.CarrierDispersion);
            double beatStat = TotalizeToMHz(movingFitter.BeatFrequency); //TotalizeToMHzAndCorrect(movingFitter.BeatFrequency);
            double beatDispStat = TotalizeToMHz(movingFitter.BeatFrequencyDispersion);
            double beatLSQ = TotalizeToMHz(movingFitter.BeatFrequencyLSQ); //TotalizeToMHzAndCorrect(movingFitter.BeatFrequencyLSQ);
            double beatDispLSQ = TotalizeToMHz(movingFitter.BeatFrequencyDispersionLSQ);
            double mppStat = TotalizeToMHz(movingFitter.ModulationDepth) * SincCorrFactor(gateTime, modTau);
            double mppDispStat = TotalizeToMHz(movingFitter.ModulationDepthDispersion);
            double mppLSQ = TotalizeToMHz(movingFitter.ModulationDepthLSQ) * SincCorrFactor(gateTime, modTau);
            double mppDispLSQ = TotalizeToMHz(movingFitter.ModulationDepthDispersionLSQ);

            //string warningSign = gateTime > maxGateTime ? "*" : " ";
            string warningSign = "";

            switch (outputType)
            {
                case OutputType.Verbose:
                    Console.WriteLine();
                    Console.WriteLine($"File name:            {Path.GetFileNameWithoutExtension(filename)}");
                    Console.WriteLine($"Gate time:            {gateTime * 1e6:F0} µs");
                    Console.WriteLine($"Window size:          {windowSize}");
                    Console.WriteLine($"Number of samples:    {data.Length}");
                    Console.WriteLine($"Minimum raw reading:  {data.Min()}");
                    Console.WriteLine($"Maximum raw reading:  {data.Max()}");
                    Console.WriteLine($"Beat frequency (tot): {beatTotal:F3} ± {beatDispTotal:F3} MHz");
                    Console.WriteLine($"Beat frequency (sta): {beatStat:F3} ± {beatDispStat:F3} MHz");
                    Console.WriteLine($"Beat frequency (LSQ): {beatLSQ:F3} ± {beatDispLSQ:F3} MHz");
                    Console.WriteLine($"Modulation Period:    {tau * 1e6:F1} ± {tauDisp*1e6:F1} µs");
                    Console.WriteLine($"Modulation frequency: {modulationFrequency*1e-3:F3} ± {modulationFrequencyDisp*1e-3:F3} kHz");
                    Console.WriteLine("===========================================");
                    Console.WriteLine($"Modulation width (s): {mppStat:F3} ± {mppDispStat:F3} MHz");
                    Console.WriteLine($"Modulation width (f): {mppLSQ:F3} ± {mppDispLSQ:F3} MHz");
                    Console.WriteLine("===========================================");
                    Console.WriteLine();
                    break;
                case OutputType.Succinct:
                    Console.WriteLine($"{Path.GetFileNameWithoutExtension(filename)} -> Mpp = {(mppLSQ+mppStat)/2:F3} MHz");
                    break;
                case OutputType.SingleLine:
                    double beatRepro = Math.Max(Math.Max(beatTotal, beatStat), beatLSQ) - Math.Min(Math.Min(beatTotal, beatStat), beatLSQ);
                    double mppDiff = mppStat - mppLSQ;
                    Console.WriteLine($"{Path.GetFileNameWithoutExtension(filename),-20} {gateTime * 1e6,5:F0} {windowSize,6} {beatTotal,6:F4} {beatDispTotal,6:F4} {beatLSQ,6:F4} {beatDispLSQ,6:F4} {mppStat,6:F4} {mppDispStat,6:F4} {mppLSQ,6:F4} {mppDispLSQ,6:F4} {tau*1e6,6:F1}{warningSign} {tauDisp*1e6,6:F1} {modulationFrequency*1e-3,6:F4}{warningSign} {modulationFrequencyDisp*1e-3,6:F4} {mppDiff:F4}");
                    break;
                case OutputType.CsvLine:
                    Console.WriteLine($"{Path.GetFileNameWithoutExtension(filename)} , {gateTime * 1e6:F0} , {windowSize} , {beatTotal:F3} , {beatDispTotal:F3} , {mppStat:F3} , {mppDispStat:F3} , {mppLSQ:F3} , {mppDispLSQ:F3} , {tau:F3} , {tauDisp:F3} , {modulationFrequency:F3} , {modulationFrequencyDisp:F3}");
                    break;
                default:
                    break;
            }

            double TotalizeToHz(double counterReading)
            {
                return counterReading / gateTime;
            }

            double TotalizeToMHz(double counterReading)
            {
                return TotalizeToHz(counterReading) * 1e-6;
            }

        }

        private static double SincCorrFactor(double gateTime, double modulationPeriod)
        {
            double relGateTime = gateTime / modulationPeriod;
            if (gateTime > maxGateTime)
                return double.NaN;
            return Math.Abs((relGateTime * Math.PI)/Math.Sin(relGateTime * Math.PI));
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

    enum OutputType
    {
        None,
        SingleLine,
        CsvLine,
        Verbose,
        Succinct
    }
}
