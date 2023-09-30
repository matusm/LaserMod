using At.Matus.StatisticPod;

namespace LaserMod
{
    public class MovingFitter
    {
        public ModulationType Modulation { get; }
        
        public double ModulationDepthLSQ => spMppLSQ.AverageValue;
        public double ModulationDepthDispersionLSQ => spMppLSQ.StandardDeviation;
        public double ModulationDepth1LSQ => spMpp1LSQ.AverageValue;
        public double ModulationDepth1DispersionLSQ => spMpp1LSQ.StandardDeviation; 
        public double ModulationDepth2LSQ => spMpp2LSQ.AverageValue;
        public double ModulationDepth2DispersionLSQ => spMpp2LSQ.StandardDeviation;
        public double ModulationDepth => spMppFromStat.AverageValue;
        public double ModulationDepthDispersion => spMppFromStat.StandardDeviation;
        public double CarrierFrequency => spCarrierFromStat.AverageValue;
        public double CarrierFrequencyDispersion => spCarrierFromStat.StandardDeviation;
        public double CarrierFrequencyLSQ => spCarrierLSQ.AverageValue;
        public double CarrierFrequencyDispersionLSQ => spCarrierLSQ.StandardDeviation;
        public int NumberOfWindows => (int)spCarrierLSQ.SampleSize;
        public int WindowSize => windowSize;
        public double RawTau { get; }
        public double RawTau1 { get; }
        public double RawTau2 { get; }

        public MovingFitter(double[] counterData, double rawTau)
        {
            Modulation = ModulationType.Single;
            this.counterData = counterData;
            RawTau = rawTau;
            RawTau1 = rawTau;
            RawTau2 = rawTau;
        }

        public MovingFitter(double[] counterData, double rawTau1, double rawTau2)
        {
            Modulation = ModulationType.Double;
            this.counterData = counterData;
            RawTau1 = rawTau1;
            RawTau2 = rawTau2;
            RawTau = rawTau1;
        }

        public void FitWithWindowSize(int windowSize)
        {
            this.windowSize = windowSize;
            SineFitter sineFitter = new SineFitter();
            TwoSineFitter twoSineFitter = new TwoSineFitter();
            spMppFromStat.Restart();
            spCarrierFromStat.Restart();
            spMppLSQ.Restart();
            spMpp1LSQ.Restart();
            spMpp2LSQ.Restart();
            spCarrierLSQ.Restart();

            double[] window = new double[windowSize];
            int runningIndex = 0;
            int indexIncrement = windowSize;
            //int indexIncrement = 1; // actual moving average, slow!
            while (runningIndex + windowSize < counterData.Length)
            {
                for (int i = 0; i < windowSize; i++)
                    window[i] = counterData[runningIndex + i];
                if (Modulation == ModulationType.Single)
                {
                    sineFitter.EstimateParametersFrom(window, RawTau);
                    spMppFromStat.Update(sineFitter.MppFromStatistics);
                    spCarrierFromStat.Update(sineFitter.CarrierFrequencyFromStatistics);
                    spMppLSQ.Update(sineFitter.MppFromLSQ);
                    spCarrierLSQ.Update(sineFitter.CarrierFrequencyFromLSQ);
                }
                if(Modulation==ModulationType.Double)
                {
                    twoSineFitter.EstimateParametersFrom(window, RawTau1, RawTau2);
                    spCarrierFromStat.Update(twoSineFitter.CarrierFrequencyFromStatistics);
                    spMpp1LSQ.Update(twoSineFitter.Mpp1FromLSQ);
                    spMpp2LSQ.Update(twoSineFitter.Mpp2FromLSQ);
                    spCarrierLSQ.Update(twoSineFitter.CarrierFrequencyFromLSQ);
                }
                runningIndex += indexIncrement;
            }
        }

        private readonly StatisticPod spMppFromStat = new StatisticPod();
        private readonly StatisticPod spMppLSQ = new StatisticPod();
        private readonly StatisticPod spMpp1LSQ = new StatisticPod();
        private readonly StatisticPod spMpp2LSQ = new StatisticPod();
        private readonly StatisticPod spCarrierFromStat = new StatisticPod();
        private readonly StatisticPod spCarrierLSQ = new StatisticPod();

        private readonly double[] counterData;
        private int windowSize;

    }
}
