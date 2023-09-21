using System;
using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearRegression;

namespace LaserMod
{
    public class SineFitter
    {
        public double FrequencyDispersionFromLSQ { get; private set; }
        public double CarrierFrequencyFromLSQ { get; private set; }
        public double FrequencyDispersionFromStatistics { get; private set; }
        public double CarrierFrequencyFromStatistics { get; private set; }
        public double FrequencyRangeFromStatistics { get; private set; }

        // rawTau // period of modulation frequency in units of samples
        public void EstimateParametersFrom(double[] data, double rawTau)
        {
            InvalidateParameters();
            if (data.Length < 10) return;

            TotalFitter totFit = new TotalFitter(data);
            FrequencyDispersionFromStatistics = totFit.CarrierDispersion * Math.Sqrt(2.0) * 2; // assuming an U-distribution
            CarrierFrequencyFromStatistics = totFit.Carrier;
            FrequencyRangeFromStatistics = totFit.Range;

            // generate x,y data array
            xData = new double[data.Length];
            for (int i = 0; i < xData.Length; i++)
                xData[i] = i;
            yData = totFit.ReducedCounterData;

            LeastSquareFit(rawTau);
        }

        private void LeastSquareFit(double rawTau)
        {
            // generate vectors and matrices (the naive way)
            double omega = 2 * Math.PI / rawTau;
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

                CarrierFrequencyFromLSQ = p[0] + CarrierFrequencyFromStatistics;
                FrequencyDispersionFromLSQ = 2 * Math.Sqrt((p[1] * p[1]) + (p[2] * p[2]));
            }
            catch (Exception)
            {
                // NOP
            }
        }

        private void InvalidateParameters()
        {
            FrequencyDispersionFromStatistics = double.NaN;
            CarrierFrequencyFromStatistics = double.NaN;
            FrequencyDispersionFromLSQ = double.NaN;
            CarrierFrequencyFromLSQ = double.NaN;
            FrequencyRangeFromStatistics = double.NaN;
        }

        private double[] xData;
        private double[] yData;

    }
}
