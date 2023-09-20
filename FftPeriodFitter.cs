using System.Numerics;
using MathNet.Numerics.IntegralTransforms;

namespace LaserMod
{
    public class FftPeriodFitter
    {
        public double RawFrequency { get; private set; }
        public double RawAmplitude { get; private set; }

        public FftPeriodFitter(TotalFitter totalFitter)
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
            var buffer = LoadCounterReadings();
            Fourier.Forward(buffer, FourierOptions.Default);
            int maxPosition = 0;
            double maxPower = 0;
            for (int i = 1; i < buffer.Length/2; i++)
            {
                double power = Complex.Abs(buffer[i]);
                if(power>= maxPower)
                {
                    maxPower = power;
                    maxPosition = i;
                }
            }
            RawFrequency = maxPosition;
            RawAmplitude = maxPower;
        }

        private readonly TotalFitter totalFitter;

    }
}
