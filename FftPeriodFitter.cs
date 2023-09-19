using System;
using System.Numerics;
using MathNet.Numerics.IntegralTransforms;

namespace LaserMod
{
    public class FftPeriodFitter
    {

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
            double[] power = new double[buffer.Length];
            for (int i = 0; i < buffer.Length; i++)
            {
                power[i] = Complex.Abs(buffer[i]);
            }


            Console.WriteLine("=======================================");
            for (int i = 0; i < buffer.Length; i++)
            {
                Console.WriteLine($"{i} : {power[i]}");
            }
            Console.WriteLine("=======================================");

        }

        private readonly TotalFitter totalFitter;

    }
}
