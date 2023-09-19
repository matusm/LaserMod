using At.Matus.StatisticPod;

namespace LaserMod
{
    public class TotalFitter
    {
        public double Carrier => statisticPod.AverageValue;
        public double CarrierDispersion => statisticPod.StandardDeviation;
        public int SampleSize => (int)statisticPod.SampleSize;
        public int MinValue => (int)statisticPod.MinimumValue;
        public int MaxValue => (int)statisticPod.MaximumValue;
        public double[] ReducedCounterData { get; }

        public TotalFitter(double[] counterData)
        {
            statisticPod = new StatisticPod();
            statisticPod.Update(counterData);
            ReducedCounterData = GetReducedCounterData(counterData);
        }

        private double[] GetReducedCounterData(double[] data)
        {
            double[] reducedData = new double[data.Length];
            for (int i = 0; i < reducedData.Length; i++)
            {
                reducedData[i] = data[i] - Carrier;
            }
            return reducedData;
        }

        private readonly StatisticPod statisticPod;

    }
}
