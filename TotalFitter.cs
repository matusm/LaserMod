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
        
        public TotalFitter(double[] counterData)
        {
            statisticPod = new StatisticPod("total");
            foreach (double data in counterData)
                statisticPod.Update(data);
        }

        private readonly StatisticPod statisticPod;

    }
}
