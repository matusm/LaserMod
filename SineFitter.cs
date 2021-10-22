using At.Matus.StatisticPod;
using System;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearRegression;

namespace LaserMod
{
    public class SineFitter
    {
        public double FrequencyDeviationLSQ;
        public double CarrierFrequencyLSQ;
        public double FrequencyDeviationFromStatistics;
        public double CarrierFrequencyFromStatistics;
        public double Tau; // period of modulation frequency
        public double Phase;

        public void EstimateParametersFrom(double[] data)
        {
            InvalidateParameters();
            if (data.Length < 10) return;
            var stPod = new StatisticPod();

            // Estimate from statistics
            foreach (var y in data)
                stPod.Update(y);
            FrequencyDeviationFromStatistics = stPod.StandardDeviation * Math.Sqrt(2.0) * 2; // assuming an U-distribution
            CarrierFrequencyFromStatistics = stPod.AverageValue;

            // generate x,y data array
            xData = new double[data.Length];
            yData = new double[data.Length];
            for (int i = 0; i < xData.Length; i++)
                xData[i] = i;
            for (int i = 0; i < data.Length; i++)
                yData[i] = data[i] - CarrierFrequencyFromStatistics;

            Tau = EstimatePeriod();
            LeastSquareFit();
        }

        private void LeastSquareFit()
        {
            // generate vectors and matrices (the naive way)
            double omega = 2 * Math.PI / Tau;
            double[] oneVector = new double[yData.Length];
            double[] sineVector = new double[yData.Length];
            double[] cosineVector = new double[yData.Length];
            for (int i = 0; i < xData.Length; i++)
            {
                double x = xData[i];
                oneVector[i] = 1;
                sineVector[i] = Math.Sin(omega * x);
                cosineVector[i] = Math.Cos(omega * x);
            }

            List<double[]> columns = new List<double[]>();
            columns.Add(oneVector);
            columns.Add(sineVector);
            columns.Add(cosineVector);
            try
            {
                var X = Matrix<double>.Build.DenseOfColumns(columns);
                var y = Vector<double>.Build.Dense(yData);

                Vector<double> p = MultipleRegression.NormalEquations(X, y);

                CarrierFrequencyLSQ = p[0] + CarrierFrequencyFromStatistics;
                FrequencyDeviationLSQ = 2 * Math.Sqrt((p[1] * p[1]) + (p[2] * p[2]));
                Phase = Math.Atan2(p[2], p[1]);
            }
            catch (Exception)
            {
                // NOP
            }
        }

        private double EstimatePeriod()
        {
            List<int> zeroPos = new List<int>();
            List<int> zeroNeg = new List<int>();

            for (int i = 0; i < yData.Length - 1; i++)
            {
                if (yData[i] > 0 && yData[i + 1] < 0)
                    zeroPos.Add(i);
                if (yData[i] < 0 && yData[i + 1] > 0)
                    zeroNeg.Add(i);
            }

            double posPeriod = double.NaN;
            double negPeriod = double.NaN;
            if (zeroPos.Count > 1)
                posPeriod = (zeroPos.Last() - zeroPos.First()) / (double)(zeroPos.Count - 1);
            if (zeroNeg.Count > 1)
                negPeriod = (zeroNeg.Last() - zeroNeg.First()) / (double)(zeroNeg.Count - 1);
            return 0.5 * (posPeriod + negPeriod);
        }

        private void InvalidateParameters()
        {
            FrequencyDeviationFromStatistics = double.NaN;
            Tau = double.NaN;
            CarrierFrequencyFromStatistics = double.NaN;
            Phase = double.NaN;
            FrequencyDeviationLSQ = double.NaN;
            CarrierFrequencyLSQ = double.NaN;
        }

        private double[] xData;
        private double[] yData;

    }
}
