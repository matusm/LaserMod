using At.Matus.StatisticPod;

namespace LaserMod
{
    internal class TotalFitter
    {
        public double Carrier => statisticPod.AverageValue;
        public double CarrierDispersion => statisticPod.StandardDeviation;
        public int SampleSize => (int)statisticPod.SampleSize;

        public TotalFitter(int[] counterData)
        {
            statisticPod = new StatisticPod("total");
            foreach (int data in counterData)
                statisticPod.Update(data);
        }

        private readonly StatisticPod statisticPod;

    }
}
