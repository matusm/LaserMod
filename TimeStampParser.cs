using System;
using System.Globalization;
using System.IO;

namespace LaserMod
{
    public class TimeStampParser
    {
        public TimeStampParser(string filename)
        {
            FullName = filename;
            BaseName = Path.ChangeExtension(FullName, null);
            SplitBaseName();
            TimeStamp = GetTimeStampFromPostfix();
        }

        public double TimeStampMjd => HasTimeStamp ? MjdFromDateTime(TimeStamp) : double.NaN;
        public DateTime TimeStamp { get; }
        public bool HasTimeStamp { get; private set; } = false;
        public string FullName { get; }
        public string BaseName { get; }
        public string Prefix { get; private set; } = string.Empty;
        public string Postfix { get; private set; } = string.Empty;

        private void SplitBaseName()
        {
            if (BaseName.Length < postfixLength + 1)
            {
                Prefix = BaseName;
                return;
            }
            Postfix = BaseName.Remove(0, BaseName.Length - postfixLength);
            Prefix = BaseName.Substring(0, BaseName.Length - postfixLength);
        }

        private DateTime GetTimeStampFromPostfix()
        {
            DateTime returnValue = unixEpoche;
            try
            {
                returnValue = DateTime.ParseExact(Postfix, "_yyyyMMdd_HHmmss", CultureInfo.InvariantCulture);
            }
            catch (Exception)
            {
                return returnValue;
            }
            HasTimeStamp = true;
            return returnValue;
        }

        private double MjdFromDateTime(DateTime date) => MjdFromUnixTime(UnixTimeFromDateTime(date));

        private long UnixTimeFromDateTime(DateTime date) => (long)date.Subtract(unixEpoche).TotalSeconds;

        private double MjdFromUnixTime(long unixSeconds) => (unixSeconds + 2209161600) / 86400.0 + 15018.0;

        private readonly DateTime unixEpoche = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private readonly int postfixLength = 16;
    }

}
