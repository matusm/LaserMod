using At.Matus.StatisticPod;

namespace LaserMod
{
    public class MovingFitter
    {
        

        public double ModulationDepthLSQ => spMppLSQ.AverageValue;
        public double ModulationDepthDispersionLSQ => spMppLSQ.StandardDeviation;
        public double ModulationDepth => spMppFromStat.AverageValue;
        public double ModulationDepthDispersion => spMppFromStat.StandardDeviation;
        public double ModulationFrequency => spModulationFrequency.AverageValue;
        public double ModulationFrequencyDispersion => spModulationFrequency.StandardDeviation;
        public double ModulationPeriod => spTau.AverageValue;
        public double ModulationPeriodDispersion => spTau.StandardDeviation;
        public double BeatFrequency => spCarrierFromStat.AverageValue;
        public double BeatFrequencyDispersion => spCarrierFromStat.StandardDeviation;
        public double BeatFrequencyLSQ => spCarrierLSQ.AverageValue;
        public double BeatFrequencyDispersionLSQ => spCarrierLSQ.StandardDeviation;
        public int NumberOfWindows => (int)spCarrierLSQ.SampleSize;
        public int WindowSize => windowSize;


        public MovingFitter(double[] counterData)
        {
            this.counterData = counterData;
            spMppFromStat = new StatisticPod("Mpp statistic");
            spMppLSQ = new StatisticPod("Mpp LSQ");   
            spCarrierLSQ = new StatisticPod("carrier LSQ");
            spCarrierFromStat = new StatisticPod("carrier statistic");
            spTau = new StatisticPod("mdulation period");
            spModulationFrequency = new StatisticPod("modulation frequency");
        }

        public void FitWithWindowSize(int windowSize)
        {
            this.windowSize = windowSize;
            var sineFitter = new SineFitter();
            spMppFromStat.Restart();
            spCarrierFromStat.Restart();
            spMppLSQ.Restart();
            spCarrierLSQ.Restart();
            spTau.Restart();
            spModulationFrequency.Restart();

            double[] window = new double[windowSize];
            int runningIndex = 0;
            int indexIncrement = windowSize;
            //int indexIncrement = 1; // actual moving average, slow!
            while (runningIndex + windowSize < counterData.Length)
            {
                for (int i = 0; i < windowSize; i++)
                    window[i] = counterData[runningIndex + i];
                sineFitter.EstimateParametersFrom(window);
                spMppFromStat.Update(sineFitter.FrequencyDeviationFromStatistics);
                spCarrierFromStat.Update(sineFitter.CarrierFrequencyFromStatistics);
                spMppLSQ.Update(sineFitter.FrequencyDeviationLSQ);
                spCarrierLSQ.Update(sineFitter.CarrierFrequencyLSQ);   
                spTau.Update(sineFitter.Tau);
                spModulationFrequency.Update(1.0 / sineFitter.Tau);
                runningIndex += indexIncrement;
            }
        }

        private readonly StatisticPod spMppFromStat;
        private readonly StatisticPod spMppLSQ;        
        private readonly StatisticPod spCarrierFromStat;
        private readonly StatisticPod spCarrierLSQ;
        private readonly StatisticPod spTau;
        private readonly StatisticPod spModulationFrequency;

        private readonly double[] counterData;
        private int windowSize;

    }
}
