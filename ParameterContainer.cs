﻿using System;
using System.Text;
using System.Reflection;

namespace LaserMod
{
    public class ParameterContainer
    {
        private TotalFitter totFitter;
        private MovingFitter movFitter;
        private FftPeriodEstimator fft;

        public string Filename { get; set; }
        public double GateTime { get; private set; } // in s
        public bool IsGateTimeTooLong => GateTime > InstrumentConstants.MaximumGateTime;
        public double SincCorrection => SincCorrFactor(GateTime, Tau);
        public int WindowSize => movFitter.WindowSize;
        public double Resolution => 1 / GateTime; // in Hz
        public double RawTau => fft.RawModulationPeriod;

        public double ModulationFrequency => fft.ModulationFrequency;
        public double Tau => fft.ModulationPeriod;

        public double CarrierTotal => TotalizeToHz(totFitter.Carrier);
        public double CarrierDispTotal => TotalizeToHz(totFitter.CarrierDispersion);
        public double CarrierStat => TotalizeToHz(movFitter.CarrierFrequency);
        public double CarrierDispStat => TotalizeToHz(movFitter.CarrierFrequencyDispersion);
        public double CarrierLSQ => TotalizeToHz(movFitter.CarrierFrequencyLSQ);
        public double CarrierDispLSQ => TotalizeToHz(movFitter.CarrierFrequencyDispersionLSQ);
        public double MppStat => TotalizeToHz(movFitter.ModulationDepth) * SincCorrection;
        public double MppDispStat => TotalizeToHz(movFitter.ModulationDepthDispersion);
        public double MppLSQ => TotalizeToHz(movFitter.ModulationDepthLSQ) * SincCorrection;
        public double MppDispLSQ => TotalizeToHz(movFitter.ModulationDepthDispersionLSQ);
        public double MppRange => TotalizeToHz(movFitter.ModulationDepthFromRange);
        public double MppDispRange => TotalizeToHz(movFitter.ModulationDepthDispersionFromRange);
        public double Mpp => (MppLSQ + MppStat) / 2.0;
        public double MppUncert => Math.Abs(MppStat - MppLSQ);

        public ParameterContainer(double gateTime) => GateTime = gateTime;

        public void SetParametersFromFitter(TotalFitter totFitter) => this.totFitter = totFitter;

        public void SetParametersFromFitter(MovingFitter movFitter) => this.movFitter = movFitter;

        public void SetParametersFromFitter(FftPeriodEstimator fft) => this.fft = fft;

        public string ToOutputString(OutputType outputType)
        {
            switch (outputType)
            {
                case OutputType.Succinct:
                    return $"{Filename} -> Mpp = {Mpp * 1e-6:F3} ± {MppUncert * 1e-6:F3} MHz";
                case OutputType.Verbose:
                    StringBuilder sb = new StringBuilder();
                    Assembly assembly = Assembly.GetExecutingAssembly();
                    sb.AppendLine();
                    sb.AppendLine($"Program version:           {assembly.GetName().Name} {assembly.GetName().Version}");
                    sb.AppendLine($"File name:                 {Filename}");
                    sb.AppendLine($"Gate time:                 {GateTime * 1e6:F0} µs");
                    sb.AppendLine($"Frequency resolution:      {Resolution * 1e-6:F3} MHz");
                    sb.AppendLine($"Window size:               {WindowSize}");
                    sb.AppendLine($"Number of samples:         {totFitter.SampleSize}");
                    sb.AppendLine($"Minimum raw reading:       {totFitter.MinValue:F0}");
                    sb.AppendLine($"Maximum raw reading:       {totFitter.MaxValue:F0}");
                    sb.AppendLine($"Carrier frequency (tot):   {CarrierTotal * 1e-6:F3} ± {CarrierDispTotal * 1e-6:F3} MHz");
                    sb.AppendLine($"Carrier frequency (sta):   {CarrierStat * 1e-6:F3} ± {CarrierDispStat * 1e-6:F3} MHz");
                    sb.AppendLine($"Carrier frequency (LSQ):   {CarrierLSQ * 1e-6:F3} ± {CarrierDispLSQ * 1e-6:F3} MHz");
                    sb.AppendLine($"Modulation Period:         {Tau * 1e6:F1} µs");
                    sb.AppendLine($"Modulation frequency:      {ModulationFrequency * 1e-3:F3} kHz");
                    sb.AppendLine($"Correction factor:         {SincCorrection:F3}");
                    sb.AppendLine($"Modulation width (range):  {MppRange * 1e-6:F3} ± {MppDispRange * 1e-6:F3} MHz");
                    sb.AppendLine($"Modulation width (sts):    {MppStat * 1e-6:F3} ± {MppDispStat * 1e-6:F3} MHz");
                    sb.AppendLine($"Modulation width (LSQ):    {MppLSQ * 1e-6:F3} ± {MppDispLSQ * 1e-6:F3} MHz");
                    sb.AppendLine("================================================");
                    sb.AppendLine($"Modulation width:          {Mpp * 1e-6:F3} ± {MppUncert * 1e-6:F3} MHz");
                    sb.AppendLine("================================================");
                    return sb.ToString();
                case OutputType.SingleLine:
                    return $"{Filename,-20} {GateTime * 1e6,5:F0} {WindowSize,6} {CarrierTotal * 1e-6,6:F4} {CarrierDispTotal * 1e-6,6:F4} {CarrierLSQ * 1e-6,6:F4} {CarrierDispLSQ * 1e-6,6:F4} {MppStat * 1e-6,6:F4} {MppDispStat * 1e-6,6:F4} {MppLSQ * 1e-6,6:F4} {MppDispLSQ * 1e-6,6:F4} {Tau * 1e6,5:F1} {ModulationFrequency * 1e-3,6:F4} {Mpp * 1e-6,6:F4} {MppUncert * 1e-6,6:F4}";
                case OutputType.CsvLine:
                    return $"{Filename},{GateTime * 1e6:F0},{WindowSize},{CarrierTotal * 1e-6:F4},{CarrierDispTotal * 1e-6:F4},{CarrierLSQ * 1e-6:F4},{CarrierDispLSQ * 1e-6:F4},{MppStat * 1e-6:F4},{MppDispStat * 1e-6:F4},{MppLSQ * 1e-6:F4},{MppDispLSQ * 1e-6:F4},{Tau * 1e6:F1},{ModulationFrequency * 1e-3:F4},{Mpp * 1e-6:F4},{MppUncert * 1e-6:F4}";
                case OutputType.CsvHeader:
                    return "filename,gate time / µs,windows size,overall beat / MHz,overall standard deviation / MHz," +
                        "carrier from LSQ fit / MHz,standard deviation of carrier from LSQ fit / MHz,Mpp from statistics / MHz,Mpp from statistics standard deviation / MHz," +
                        "Mpp LSQ / MHz,Mpp LSQ standard deviation / MHz,tau / µs,f_mod / kHz,Mpp / MHz,U(Mpp) / MHz";
                default:
                    return string.Empty;
            }
        }

        private double TotalizeToHz(double counterReading) => counterReading / GateTime;

        private double SincCorrFactor(double gateTime, double modulationPeriod)
        {
            double relGateTime = gateTime / modulationPeriod;
            if (gateTime > InstrumentConstants.MaximumGateTime)
                return double.NaN;
            return Math.Abs((relGateTime * Math.PI) / Math.Sin(relGateTime * Math.PI));
        }

    }
}
