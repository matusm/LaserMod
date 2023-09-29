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
            MyCommandLine options = new MyCommandLine(args);

            // @"E:\LaserModData\BEV2\T010BEV2_A.csv";
            // @"/Volumes/NO NAME/LaserModData/S01/T020S01.csv";

            double[] data = ReadDataFromFile(options.InputFilename);
            double gateTime = EstimateGateTimeFromFileName(options.InputFilename);

            ParameterContainer container = new ParameterContainer(gateTime);
            container.Filename = Path.GetFileName(options.InputFilename);
            if (container.IsGateTimeTooLong)
            {
                Console.WriteLine("! Warning: gate time too long! Some parameters may be invalid!");
            }

            TotalFitter totalFitter = new TotalFitter(data);
            container.SetParametersFromFitter(totalFitter);

            //Test suite
            FftTwoPeriodEstimator fft2 = new FftTwoPeriodEstimator(totalFitter);
            if (!fft2.SingleModulation)
            {
                Console.WriteLine($"Frequency 1: {fft2.ModulationFrequency1:F1} Hz");
                Console.WriteLine($"Frequency 2: {fft2.ModulationFrequency2:F1} Hz");
                return;
            }
            double rawPeriod = fft2.RawModulationPeriod1;
            container.SetParametersFromFitter(fft2);

            int windowSize = (int)(rawPeriod * options.EvaluationPeriods);
            int optimalWindowSize = EstimateOptimalWindowSize(windowSize, rawPeriod);

            MovingFitter movingFitter = new MovingFitter(data, rawPeriod);
            movingFitter.FitWithWindowSize(optimalWindowSize);
            container.SetParametersFromFitter(movingFitter);

            DisplayResults(container, options.Verbosity);
            WriteResults(container, OutputType.Verbose, options.OutputFilename);

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

        private static void DisplayResults(ParameterContainer container, OutputType outputType)
        {
            if (outputType == OutputType.Verbose) Console.WriteLine();
            Console.WriteLine(container.ToOutputString(outputType));
        }

        private static void WriteResults(ParameterContainer container, OutputType outputType, string outputFilename)
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
            try
            {
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
            }
            catch (Exception)
            {
                Console.WriteLine($"! can not read {filename} !");
                Environment.Exit(1);
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

}
