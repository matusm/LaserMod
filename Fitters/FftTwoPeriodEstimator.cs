using System.Collections.Generic;
using System.IO;
using System.Numerics;
using MathNet.Numerics.IntegralTransforms;

namespace LaserMod
{
    public class FftTwoPeriodEstimator
    {

        public double ModulationFrequency1 => RawFrequency1 / InstrumentConstants.FftCalFactor;
        public double ModulationPeriod1 => 1 / ModulationFrequency1;
        public double RawModulationPeriod1 => 1e6 / RawFrequency1;    // in units of samples
        public int RawFrequency1 { get; private set; }

        public double ModulationFrequency2 => RawFrequency2 / InstrumentConstants.FftCalFactor;
        public double ModulationPeriod2 => 1 / ModulationFrequency2;
        public double RawModulationPeriod2 => 1e6 / RawFrequency2;    // in units of samples
        public int RawFrequency2 { get; private set; }


        public FftTwoPeriodEstimator(TotalFitter totalFitter)
        {
            this.totalFitter = totalFitter;
            Complex[] buffer = FourierTransformData();
            _DebugLog(buffer);
            double cutOff = MaximumPower(buffer)/10.0;
            AnalyzeSpectrum(buffer, cutOff);
        }

        private Complex[] LoadCounterReadings()
        {
            Complex[] data = new Complex[totalFitter.ReducedCounterData.Length];
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = totalFitter.ReducedCounterData[i];
            }
            return data;
        }

        private Complex[] FourierTransformData()
        {
            Complex[] buffer = LoadCounterReadings();
            Fourier.Forward(buffer, FourierOptions.NumericalRecipes);
            return buffer;
        }

        private double MaximumPower(Complex[] buffer)
        {
            double maxPower = 0;
            for (int i = 0; i < buffer.Length/1; i++)
            {
                double power = Complex.Abs(buffer[i]);
                if (power >= maxPower)
                {
                    maxPower = power;
                }
            }
            return maxPower;
        }

        private void AnalyzeSpectrum(Complex[] buffer, double cutOffPower)
        {
            List<int> lowPart = new List<int>();
            List<int> highPart = new List<int>();
            bool flag = false;
            for (int i = 1; i < buffer.Length / 2; i++)
            {
                double power = Complex.Abs(buffer[i]);
                if (power > cutOffPower)
                {
                    flag = true;
                    lowPart.Add(i);
                }
                else
                {
                    if (flag == true) break;
                }
            }
            flag = false;
            for (int i = buffer.Length / 2; i>1 ; i--)
            {
                double power = Complex.Abs(buffer[i]);
                if (power > cutOffPower)
                {
                    flag = true;
                    highPart.Add(i);
                }
                else
                {
                    if (flag == true) break;
                }
            }

            int maxPosition = 0;
            double maxPower = 0;
            foreach (int index in lowPart)
            {
                double power = Complex.Abs(buffer[index]);
                if (power >= maxPower)
                {
                    maxPower = power;
                    maxPosition = index;
                }
            }
            RawFrequency1 = maxPosition;

            maxPosition = 0;
            maxPower = 0;
            foreach (int index in highPart)
            {
                double power = Complex.Abs(buffer[index]);
                if (power >= maxPower)
                {
                    maxPower = power;
                    maxPosition = index;
                }
            }
            RawFrequency2 = maxPosition;
        }

        private void _DebugLog(Complex[] buffer)
        {
            using (StreamWriter writer = new StreamWriter("FFT2.csv", false))
            {
                for (int i = 1; i < buffer.Length / 2; i++)
                {
                    double power = Complex.Abs(buffer[i]);
                    writer.WriteLine($"{i}, {power}");
                }
            }
        }

        private readonly TotalFitter totalFitter;

    }
}
