using System;
using System.Text;
using System.Reflection;

namespace LaserMod
{
    public class ParameterContainer
    {

        public ParameterContainer(double gateTime) => GateTime = gateTime;

        public string Filename { get; set; }
        public double GateTime { get; } // in s
        public bool IsGateTimeTooLong => GateTime > InstrumentConstants.MaximumGateTime;
        public int WindowSize => movFitter.WindowSize;
        public double Resolution => 1 / GateTime; // in Hz
        public double ModulationFrequency => fftEstimator.ModulationFrequency1;
        public double ModulationFrequency1 => fftEstimator.ModulationFrequency1;
        public double ModulationFrequency2 => fftEstimator.ModulationFrequency2;
        public double Tau => fftEstimator.ModulationPeriod1;
        public double RawTau => fftEstimator.RawModulationPeriod1;
        public double Tau1 => fftEstimator.ModulationPeriod1;
        public double RawTau1 => fftEstimator.RawModulationPeriod1;
        public double Tau2 => fftEstimator.ModulationPeriod2;
        public double RawTau2 => fftEstimator.RawModulationPeriod2;
        public double SincCorrection => SincCorrFactor(GateTime, Tau);
        public double SincCorrection1 => SincCorrFactor(GateTime, Tau1);
        public double SincCorrection2 => SincCorrFactor(GateTime, Tau2);

        public double CarrierTotal => TotalizeToHz(totFitter.Carrier);
        public double CarrierDispTotal => TotalizeToHz(totFitter.CarrierDispersion);
        public double CarrierStat => TotalizeToHz(movFitter.CarrierFrequency);
        public double CarrierDispStat => TotalizeToHz(movFitter.CarrierFrequencyDispersion);
        public double CarrierLSQ => TotalizeToHz(movFitter.CarrierFrequencyLSQ);
        public double CarrierDispLSQ => TotalizeToHz(movFitter.CarrierFrequencyDispersionLSQ);
        public double MppStat => TotalizeToHz(movFitter.ModulationDepth) * SincCorrection + InstrumentConstants.EmpiricalCorrectionForMppStat(TotalizeToHz(movFitter.ModulationDepth));
        public double MppDispStat => TotalizeToHz(movFitter.ModulationDepthDispersion);
        public double MppLSQ => TotalizeToHz(movFitter.ModulationDepthLSQ) * SincCorrection + InstrumentConstants.EmpiricalCorrectionForMppLSQ(TotalizeToHz(movFitter.ModulationDepthLSQ), ModulationFrequency);
        public double MppDispLSQ => TotalizeToHz(movFitter.ModulationDepthDispersionLSQ);
        public double Mpp1LSQ => TotalizeToHz(movFitter.ModulationDepth1LSQ) * SincCorrection1 + InstrumentConstants.EmpiricalCorrectionForMppLSQ(TotalizeToHz(movFitter.ModulationDepth1LSQ), ModulationFrequency1);
        public double Mpp1DispLSQ => TotalizeToHz(movFitter.ModulationDepth1DispersionLSQ);
        public double Mpp2LSQ => (TotalizeToHz(movFitter.ModulationDepth2LSQ) * SincCorrection2) + InstrumentConstants.EmpiricalCorrectionForMppLSQ(TotalizeToHz(movFitter.ModulationDepth2LSQ), ModulationFrequency2);
        public double Mpp2DispLSQ => TotalizeToHz(movFitter.ModulationDepth2DispersionLSQ);
        public double Mpp => (MppLSQ + MppStat) / 2.0;
        public double MppUncert => EvaluateUncertainty();


        public void SetParametersFromFitter(TotalFitter totFitter) => this.totFitter = totFitter;

        public void SetParametersFromFitter(MovingFitter movFitter) => this.movFitter = movFitter;

        public void SetParametersFromFitter(FftTwoPeriodEstimator fftEstimator) => this.fftEstimator = fftEstimator;

        public string ToOutputString(OutputType outputType)
        {
            switch (outputType)
            {
                case OutputType.Succinct:
                    if (movFitter.Modulation == ModulationType.Single)
                        return SuccinctOutputSingleMod();
                    if (movFitter.Modulation == ModulationType.Double)
                        return SuccinctOutputDoubleMod();
                    return string.Empty;
                case OutputType.Verbose:
                    if (movFitter.Modulation == ModulationType.Single)
                        return VerboseOutputSingleMod();
                    if (movFitter.Modulation == ModulationType.Double)
                        return VerboseOutputDoubleMod();
                    return string.Empty;
                case OutputType.TestCase:
                    if (movFitter.Modulation == ModulationType.Single)
                        return TestOutputSingleModCsv();
                    if (movFitter.Modulation == ModulationType.Double)
                        return TestOutputDoubleModCsv();
                    return string.Empty;
                default:
                    return string.Empty;
            }
        }

        private string SuccinctOutputSingleMod() => $"{Filename} -> Mpp = {Mpp * 1e-6:F3} ± {MppUncert * 1e-6:F3} MHz";

        private string SuccinctOutputDoubleMod() => $"{Filename}  ->  Mpp(1) = {Mpp1LSQ * 1e-6:F3} ± {Mpp1DispLSQ * 1e-6:F3} MHz   Mpp(2) = {Mpp2LSQ * 1e-6:F3} ± {Mpp2DispLSQ * 1e-6:F3} MHz";

        private string VerboseOutputCommon()
        {
            TimeStampParser timeStampParser = new TimeStampParser(Filename);
            StringBuilder sb = new StringBuilder();
            Assembly assembly = Assembly.GetExecutingAssembly();
            sb.AppendLine($"Program version:           {assembly.GetName().Name} {assembly.GetName().Version.ToString(3)}");
            sb.AppendLine($"File name:                 {Filename}");
            if(timeStampParser.HasTimeStamp)
            {
                sb.AppendLine($"Timestamp (MJD):           {timeStampParser.TimeStampMjd:F4} d");
            }
            sb.AppendLine($"Gate time:                 {GateTime * 1e6:F0} µs");
            sb.AppendLine($"Frequency resolution:      {Resolution * 1e-6:F3} MHz");
            sb.AppendLine($"Window size:               {WindowSize} (in counter samples)");
            sb.AppendLine($"Number of counter samples: {totFitter.SampleSize}");
            sb.AppendLine($"Minimum raw reading:       {totFitter.MinValue:F0}");
            sb.AppendLine($"Maximum raw reading:       {totFitter.MaxValue:F0}");
            sb.AppendLine($"Carrier frequency (tot):   {CarrierTotal * 1e-6:F3} ± {CarrierDispTotal * 1e-6:F3} MHz");
            sb.AppendLine($"Carrier frequency (stat):  {CarrierStat * 1e-6:F3} ± {CarrierDispStat * 1e-6:F3} MHz");
            sb.Append($"Carrier frequency (LSQ):   {CarrierLSQ * 1e-6:F3} ± {CarrierDispLSQ * 1e-6:F3} MHz");
            return sb.ToString();
        }

        private string VerboseOutputDoubleMod()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(VerboseOutputCommon());
            sb.AppendLine("===============================================================");
            sb.AppendLine("Laser 1");
            sb.AppendLine($"  Modulation Period:       {Tau1 * 1e6:F1} µs");
            sb.AppendLine($"  Modulation Period:       {RawTau1:F3} (in counter samples)");
            sb.AppendLine($"  Modulation frequency:    {ModulationFrequency1 * 1e-3:F3} kHz");
            sb.AppendLine($"  sinc correction factor:  {SincCorrection1:F3}");
            sb.AppendLine($"  Modulation width (LSQ):  {Mpp1LSQ * 1e-6:F3} ± {Mpp1DispLSQ * 1e-6:F3} MHz");
            sb.AppendLine("===============================================================");
            sb.AppendLine("Laser 2");
            sb.AppendLine($"  Modulation Period:       {Tau2 * 1e6:F1} µs");
            sb.AppendLine($"  Modulation Period:       {RawTau2:F3} (in counter samples)");
            sb.AppendLine($"  Modulation frequency:    {ModulationFrequency2 * 1e-3:F3} kHz");
            sb.AppendLine($"  sinc correction factor:  {SincCorrection2:F3}");
            sb.AppendLine($"  Modulation width (LSQ):  {Mpp2LSQ * 1e-6:F3} ± {Mpp2DispLSQ * 1e-6:F3} MHz");
            sb.AppendLine("===============================================================");
            return sb.ToString();
        }

        private string VerboseOutputSingleMod()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(VerboseOutputCommon());
            sb.AppendLine($"Modulation Period:         {Tau * 1e6:F1} µs");
            sb.AppendLine($"Modulation Period:         {RawTau:F3} (in counter samples)");
            sb.AppendLine($"Modulation frequency:      {ModulationFrequency * 1e-3:F3} kHz");
            sb.AppendLine($"sinc correction factor:    {SincCorrection:F3}");
            sb.AppendLine($"Modulation width (stat):   {MppStat * 1e-6:F3} ± {MppDispStat * 1e-6:F3} MHz");
            sb.AppendLine($"Modulation width (LSQ):    {MppLSQ * 1e-6:F3} ± {MppDispLSQ * 1e-6:F3} MHz");
            sb.AppendLine("===============================================================");
            sb.AppendLine($"Modulation width:          {Mpp * 1e-6:F3} ± {MppUncert * 1e-6:F3} MHz");
            sb.AppendLine("===============================================================");
            return sb.ToString();
        }

        private double TotalizeToHz(double counterReading) => counterReading / GateTime;

        private double SincCorrFactor(double gateTime, double modulationPeriod)
        {
            double relGateTime = gateTime / modulationPeriod;
            if (gateTime > InstrumentConstants.MaximumGateTime)
                return double.NaN;
            return Math.Abs((relGateTime * Math.PI) / Math.Sin(relGateTime * Math.PI));
        }

        // standard uncertainty of the mean (of MppStat and MppLSQ) combined with a dark uncertainty
        // the dark uncertainty ensures the compatibility of both Mpp
        private double EvaluateUncertainty()
        {
            double dx = Math.Abs(MppStat - MppLSQ);
            double vdx = MppDispStat * MppDispStat + MppDispLSQ * MppDispLSQ;
            double dc = dx * dx / 4 - vdx;
            double udark = 0.0;
            if (dc > 0)
                udark = Math.Sqrt(dc);
            double uMpp = Math.Sqrt(vdx / 2 + udark * udark);
            if (uMpp < 1000.0) uMpp = 1000.0; // uncertainty must be at least 1 kHz
            return uMpp;
        }

        private string TestOutputSingleModCsv()
        {
            int gate, fmod, mpp;
            string[] tokens = Filename.Split(new char[] { 'T', '_', '.' }, StringSplitOptions.RemoveEmptyEntries);
            try
            {
                gate = int.Parse(tokens[0]);
                fmod = int.Parse(tokens[1]);
                mpp = int.Parse(tokens[2]);
            }
            catch (Exception)
            {
                return $"Invalid filename syntax {Filename}";
            }
            string line = $"{Filename}, {gate,2}, {fmod,4}, {mpp}, {ModulationFrequency:F1}, {MppStat/1e6:F4}, {MppLSQ / 1e6:F4}, {ModulationFrequency-fmod:F1}, {MppStat/1e6-mpp:F4}, {MppLSQ/ 1e6-mpp:F4}";
            return line;
        }

        private string TestOutputDoubleModCsv()
        {
            return "Test case not implemented yet for double modulated data!";
        }

        private TotalFitter totFitter;
        private MovingFitter movFitter;
        private FftTwoPeriodEstimator fftEstimator;

    }

    public enum OutputType
    {
        None,
        Verbose,
        TestCase,
        Succinct
    }
}
