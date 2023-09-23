namespace LaserMod
{
    public static class InstrumentConstants
    {
        // for gatetimes longer than 22 µs the modulation frequency can not be estimated
        public static readonly double MaximumGateTime = 22e-6;

        // the counter readings are smaller by this value on average
        public static readonly double TotalizeCorrection = 0.286;

        // ? 1e6/ModulationFrequencyCalFactor ?
        public static readonly double FftCalFactor = 24.4798;

        // Hz/point - legacy, no longer used
        public static readonly double ModulationFrequencyCalFactor = 40848.0222;
    }
}
