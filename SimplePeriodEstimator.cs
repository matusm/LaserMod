using System.Collections.Generic;
using System.Linq;

namespace LaserMod
{
    public class SimplePeriodEstimator
    {
        public double Tau { get; } // period of modulation frequency in units of samples

        public SimplePeriodEstimator(TotalFitter totalFitter)
        {
            this.totalFitter = totalFitter;
            Tau = EstimatePeriod();
        }

        private double EstimatePeriod()
        {
            List<int> zeroPos = new List<int>();
            List<int> zeroNeg = new List<int>();

            for (int i = 0; i < totalFitter.ReducedCounterData.Length - 1; i++)
            {
                if (totalFitter.ReducedCounterData[i] > 0 && totalFitter.ReducedCounterData[i + 1] < 0)
                    zeroPos.Add(i);
                if (totalFitter.ReducedCounterData[i] < 0 && totalFitter.ReducedCounterData[i + 1] > 0)
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

        private readonly TotalFitter totalFitter;

    }
}
