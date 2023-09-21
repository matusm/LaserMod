using At.Matus.StatisticPod;

namespace LaserMod
{
    public class MovingFitter
    {
        
        public double ModulationDepthLSQ => spMppLSQ.AverageValue;
        public double ModulationDepthDispersionLSQ => spMppLSQ.StandardDeviation;
        public double ModulationDepth => spMppFromStat.AverageValue;
        public double ModulationDepthDispersion => spMppFromStat.StandardDeviation;
        public double ModulationDepthFromRange => spMppFromRange.AverageValue;
        public double ModulationDepthDispersionFromRange => spMppFromRange.StandardDeviation;
        public double CarrierFrequency => spCarrierFromStat.AverageValue;
        public double CarrierFrequencyDispersion => spCarrierFromStat.StandardDeviation;
        public double CarrierFrequencyLSQ => spCarrierLSQ.AverageValue;
        public double CarrierFrequencyDispersionLSQ => spCarrierLSQ.StandardDeviation;
        public int NumberOfWindows => (int)spCarrierLSQ.SampleSize;
        public int WindowSize => windowSize;
        public double RawTau { get; }

        public MovingFitter(double[] counterData, double rawTau)
        {
            this.counterData = counterData;
            RawTau = rawTau;
            spMppFromStat = new StatisticPod();
            spMppFromRange = new StatisticPod();
            spMppLSQ = new StatisticPod();   
            spCarrierLSQ = new StatisticPod();
            spCarrierFromStat = new StatisticPod();
        }

        public void FitWithWindowSize(int windowSize)
        {
            this.windowSize = windowSize;
            SineFitter sineFitter = new SineFitter();
            spMppFromStat.Restart();
            spCarrierFromStat.Restart();
            spMppLSQ.Restart();
            spCarrierLSQ.Restart();
            spMppFromRange.Restart();

            double[] window = new double[windowSize];
            int runningIndex = 0;
            int indexIncrement = windowSize;
            //int indexIncrement = 1; // actual moving average, slow!
            while (runningIndex + windowSize < counterData.Length)
            {
                for (int i = 0; i < windowSize; i++)
                    window[i] = counterData[runningIndex + i];
                sineFitter.EstimateParametersFrom(window, RawTau);
                spMppFromStat.Update(sineFitter.FrequencyDispersionFromStatistics);
                spCarrierFromStat.Update(sineFitter.CarrierFrequencyFromStatistics);
                spMppLSQ.Update(sineFitter.FrequencyDispersionFromLSQ);
                spCarrierLSQ.Update(sineFitter.CarrierFrequencyFromLSQ);
                spMppFromRange.Update(sineFitter.FrequencyRangeFromStatistics);
                runningIndex += indexIncrement;
            }
        }

        private readonly StatisticPod spMppFromStat;
        private readonly StatisticPod spMppLSQ;        
        private readonly StatisticPod spCarrierFromStat;
        private readonly StatisticPod spCarrierLSQ;
        private readonly StatisticPod spMppFromRange;

        private readonly double[] counterData;
        private int windowSize;

    }
}
