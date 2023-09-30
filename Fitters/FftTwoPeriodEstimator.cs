using System.Collections.Generic;
using System.Numerics;
using MathNet.Numerics.IntegralTransforms;

namespace LaserMod
{
    public class FftTwoPeriodEstimator
    {
        public bool SingleModulation => RawFrequency1 == RawFrequency2;

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
            Complex[] spectrum = FourierTransformData();
            double cutOff = GetMaximumPower(spectrum) * InstrumentConstants.PeakRegionCutoffFactor;
            AnalyzeSpectrum(spectrum, cutOff);
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

        private double GetMaximumPower(Complex[] spectrum)
        {
            double maxPower = 0;
            for (int i = 0; i < spectrum.Length/1; i++)
            {
                double power = Complex.Abs(spectrum[i]);
                if (power >= maxPower)
                {
                    maxPower = power;
                }
            }
            return maxPower;
        }

        private void AnalyzeSpectrum(Complex[] spectrum, double cutOffPower)
        {
            List<int> lowPart = SegmentSpectrumFromBelow(spectrum, cutOffPower);
            RawFrequency1 = GetRawPeakFrequency(spectrum, lowPart);
            
            List<int> highPart = SegmentSpectrumFromAbove(spectrum, cutOffPower);
            RawFrequency2 = GetRawPeakFrequency(spectrum, highPart);
        }

        private List<int> SegmentSpectrumFromBelow(Complex[] spectrum, double cutOff)
        {
            List<int> region = new List<int>();
            bool flag = false;
            for (int i = spectrum.Length / 2; i > 1; i--)
            {
                double power = Complex.Abs(spectrum[i]);
                if (power > cutOff)
                {
                    flag = true;
                    region.Add(i);
                }
                else
                {
                    if (flag == true) break;
                }
            }
            return region;
        }

        private List<int> SegmentSpectrumFromAbove(Complex[] spectrum, double cutOff)
        {
            List<int> region = new List<int>();
            bool flag = false;
            for (int i = 1; i < spectrum.Length / 2; i++)
            {
                double power = Complex.Abs(spectrum[i]);
                if (power > cutOff)
                {
                    flag = true;
                    region.Add(i);
                }
                else
                {
                    if (flag == true) break;
                }
            }
            return region;
        }

        private int GetRawPeakFrequency(Complex[] spectrum, List<int> region)
        {
            int maxPosition = 0;
            double maxPower = 0;
            foreach (int index in region)
            {
                double power = Complex.Abs(spectrum[index]);
                if (power >= maxPower)
                {
                    maxPower = power;
                    maxPosition = index;
                }
            }
            return maxPosition;
        }

        private readonly TotalFitter totalFitter;

    }
}
