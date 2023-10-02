namespace LaserMod
{
    public static class InstrumentConstants
    {
        // for gatetimes longer than 22 µs the modulation frequency can not be estimated
        public static readonly double MaximumGateTime = 22e-6;

        // the counter readings are smaller by this value on average
        // this is an empirical value!
        public static readonly double TotalizeCorrection = 0.286;

        // ? 1e6/ModulationFrequencyCalFactor ?
        public static readonly double FftCalFactor = 24.4798;

        // Hz/point - legacy, no longer used
        public static readonly double ModulationFrequencyCalFactor = 40848.0222;

        // target value for the number of modulation periods for the fitters
        public static readonly int EvaluationPeriods = 200;

        // FFT peak region cutoff factor (relativ to maximum power)
        public static readonly double PeakRegionCutoffFactor = 0.1;

        // For the statistical technique the estimated modulation width must be corrected by this value
        // Correction by adding this value to the raw Mpp value
        // All values in Hz!
        public static double EmpiricalCorrectionForMppStat(double mpp)
        {
            return -1300 + 0.0008 * mpp;
        }

        // For the LSQ technique the estimated modulation width must be corrected by this value
        // Correction by adding this value to the raw Mpp value
        // All values in Hz!
        public static double EmpiricalCorrectionForMppLSQ(double mpp, double fmod)
        {
            double c = 4e-5 - 0.0007 * mpp * 1e-6;
            double a = 1e-12 - 7e-12 * mpp * 1e-6;
            double corr = -1e6 * (c + a * fmod * fmod) + 290;
            return corr;
        }

    }
}
