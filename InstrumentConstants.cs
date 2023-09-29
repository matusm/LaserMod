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

        // FFT peak region cutoff factor (relativ to maximum power)
        public static readonly double PeakRegionCutoffFactor = 0.1;

        // the estimated modulation width is of by this value in Hz (5100)
        public static readonly double MppStatCorrection = 0;

        // the estimated modulation width is of by this value in Hz (7000 + 6e-5*f_^2=
        public static readonly double MppLsqCorrection = 0;

    }
}
