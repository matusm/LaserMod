using System;
using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearRegression;

namespace LaserMod
{
    public class TwoSineFitter
    {
        public double Mpp1FromLSQ { get; private set; }
        public double Mpp2FromLSQ { get; private set; }
        public double CarrierFrequencyFromLSQ { get; private set; }
        public double CarrierFrequencyFromStatistics { get; private set; }

        // rawTau // period of modulation frequency in units of samples
        public void EstimateParametersFrom(double[] data, double rawTau1, double rawTau2)
        {
            InvalidateParameters();
            if (data.Length < 10) return;

            TotalFitter totFit = new TotalFitter(data);
            CarrierFrequencyFromStatistics = totFit.Carrier;

            // generate x,y data array
            xData = new double[data.Length];
            for (int i = 0; i < xData.Length; i++)
                xData[i] = i;
            yData = totFit.ReducedCounterData;

            LeastSquareFit(rawTau1, rawTau2);
        }

        private void LeastSquareFit(double rawTau1, double rawTau2)
        {
            // generate vectors and matrices (the naive way)
            double omega1 = 2 * Math.PI / rawTau1;
            double omega2 = 2 * Math.PI / rawTau2;
            double[] oneVector = new double[yData.Length];
            double[] sineVector1 = new double[yData.Length];
            double[] cosineVector1 = new double[yData.Length];
            double[] sineVector2 = new double[yData.Length];
            double[] cosineVector2 = new double[yData.Length];
            for (int i = 0; i < xData.Length; i++)
            {
                double x = xData[i];
                oneVector[i] = 1;
                sineVector1[i] = Math.Sin(omega1 * x);
                cosineVector1[i] = Math.Cos(omega1 * x);
                sineVector2[i] = Math.Sin(omega2 * x);
                cosineVector2[i] = Math.Cos(omega2 * x);
            }

            List<double[]> columns = new List<double[]>();
            columns.Add(oneVector);
            columns.Add(sineVector1);
            columns.Add(cosineVector1);
            columns.Add(sineVector2);
            columns.Add(cosineVector2);
            try
            {
                Matrix<double> X = Matrix<double>.Build.DenseOfColumns(columns);
                Vector<double> y = Vector<double>.Build.Dense(yData);

                Vector<double> p = MultipleRegression.NormalEquations(X, y);

                CarrierFrequencyFromLSQ = p[0] + CarrierFrequencyFromStatistics;
                Mpp1FromLSQ = 2 * Math.Sqrt((p[1] * p[1]) + (p[2] * p[2]));
                Mpp2FromLSQ = 2 * Math.Sqrt((p[3] * p[3]) + (p[4] * p[4]));
            }
            catch (Exception)
            {
                // NOP
            }
        }

        private void InvalidateParameters()
        {
            CarrierFrequencyFromStatistics = double.NaN;
            Mpp1FromLSQ = double.NaN;
            Mpp2FromLSQ = double.NaN;
            CarrierFrequencyFromLSQ = double.NaN;
        }

        private double[] xData;
        private double[] yData;

    }
}
