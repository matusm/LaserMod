using System;
using System.Collections.Generic;
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
        public double Tau; // period of modulation frequency in units of samples
        public double Phase;

        public void EstimateParametersFrom(double[] data, double rawTau)
        {
            InvalidateParameters();
            Tau = rawTau;
            if (data.Length < 10) return;

            TotalFitter totFit = new TotalFitter(data);
            FrequencyDeviationFromStatistics = totFit.CarrierDispersion * Math.Sqrt(2.0) * 2; // assuming an U-distribution
            CarrierFrequencyFromStatistics = totFit.Carrier;

            // generate x,y data array
            xData = new double[data.Length];
            for (int i = 0; i < xData.Length; i++)
                xData[i] = i;
            yData = totFit.ReducedCounterData;

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
