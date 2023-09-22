using System.IO;
using System.Numerics;
using MathNet.Numerics.IntegralTransforms;

namespace LaserMod
{
    public class FftPeriodEstimator
    {
        public double ModulationFrequency => RawFrequency / InstrumentConstants.FftCalFactor;
        public double ModulationPeriod => 1 / ModulationFrequency;
        public double RawModulationPeriod => 1e6 / RawFrequency;    // in units of samples
        public int RawFrequency { get; private set; }

        public FftPeriodEstimator(TotalFitter totalFitter)
        {
            this.totalFitter = totalFitter;
            FourierTransformData();
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

        private void FourierTransformData()
        {
            Complex[] buffer = LoadCounterReadings();
            Fourier.Forward(buffer, FourierOptions.NumericalRecipes);
            int maxPosition = 0;
            double maxPower = 0;
            for (int i = 1; i < buffer.Length / 2; i++)
            {
                double power = Complex.Abs(buffer[i]);
                if (power >= maxPower)
                {
                    maxPower = power;
                    maxPosition = i;
                }
            }
            RawFrequency = maxPosition;
            _DebugLog(buffer);
        }

        private void _DebugLog(Complex[] buffer)
        {
            using (StreamWriter writer = new StreamWriter("FFT.csv", false))
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
