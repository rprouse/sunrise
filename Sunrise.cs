using System;
using Microsoft.Extensions.Logging;

namespace SunriseCalculator
{
    public class Sunrise
    {
        private readonly ILogger _logger;

        public Sunrise(ILogger logger)
        {
            _logger = logger;
        }

        private string Ts2Human(double ts, TimeZoneInfo debugTz)
        {
            DateTimeOffset dateTime = DateTimeOffset.FromUnixTimeSeconds((long)ts);
            if (debugTz != null)
            {
                return TimeZoneInfo.ConvertTime(dateTime, debugTz).ToString();
            }
            return dateTime.ToString();
        }

        public static double J2Ts(double j)
        {
            return (j - 2440587.5) * 86400;
        }

        public static double Ts2J(double ts)
        {
            return ts / 86400.0 + 2440587.5;
        }

        private string J2Human(double j, TimeZoneInfo debugTz)
        {
            double ts = J2Ts(j);
            return $"{ts} = {Ts2Human(ts, debugTz)}";
        }

        private string Deg2Human(double deg)
        {
            int x = (int)(deg * 3600.0);
            string num = $"∠{deg:F3}°";
            string rad = $"∠{deg.Radians():F3}rad";
            string human = $"∠{x / 3600}°{x / 60 % 60}′{x % 60}″";
            return $"{rad} = {human} = {num}";
        }

        public Tuple<double, double, bool?> Calc(
            double currentTimestamp,
            double latitude,
            double longitude,
            double elevation = 0.0,
            TimeZoneInfo debugTz = null)
        {
            _logger.LogDebug($"Latitude               f       = {Deg2Human(latitude)}");
            _logger.LogDebug($"Longitude              l_w     = {Deg2Human(longitude)}");
            _logger.LogDebug($"Now                    ts      = {Ts2Human(currentTimestamp, debugTz)}");

            double jDate = Ts2J(currentTimestamp);
            _logger.LogDebug($"Julian date            j_date  = {jDate:F3} days");

            // Julian day
            double n = Math.Ceiling(jDate - (2451545.0 + 0.0009) + 69.184 / 86400.0);
            _logger.LogDebug($"Julian day             n       = {n:F3} days");

            // Mean solar time
            double j_ = n + 0.0009 - longitude / 360.0;
            _logger.LogDebug($"Mean solar time        J_      = {j_:F9} days");

            // Solar mean anomaly
            double mDegrees = (357.5291 + 0.98560028 * j_) % 360;
            double mRadians = mDegrees.Radians();
            _logger.LogDebug($"Solar mean anomaly     M       = {Deg2Human(mDegrees)}");

            // Equation of the center
            double cDegrees = 1.9148 * Math.Sin(mRadians) + 0.02 * Math.Sin(2 * mRadians) + 0.0003 * Math.Sin(3 * mRadians);
            _logger.LogDebug($"Equation of the center C       = {Deg2Human(cDegrees)}");

            // Ecliptic longitude
            double lDegrees = (mDegrees + cDegrees + 180.0 + 102.9372) % 360;
            _logger.LogDebug($"Ecliptic longitude     L       = {Deg2Human(lDegrees)}");

            double lambdaRadians = lDegrees.Radians();
            _logger.LogDebug($"Ecliptic longitude     Lambda  = {lDegrees}°");
            _logger.LogDebug($"Ecliptic longitude     Lambda  = {lambdaRadians}rad");

            // Solar transit (Julian date)
            double jTransit = (2451545.0 + j_ + 0.0053 * Math.Sin(mRadians) - 0.0069 * Math.Sin(2 * lambdaRadians));
            _logger.LogDebug($"Solar transit time     J_trans = {J2Human(jTransit, debugTz)}");

            // Declination of the Sun
            double sinD = Math.Sin(lambdaRadians) * Math.Sin(23.4397.Radians());
            double cosD = Math.Cos(Math.Asin(sinD));

            // Hour angle
            double someCos = (Math.Sin((-0.833 - 2.076 * Math.Sqrt(elevation) / 60.0).Radians()) - Math.Sin(latitude.Radians()) * sinD)
                           / (Math.Cos(latitude.Radians()) * cosD);

            try
            {
                double w0Radians = Math.Acos(someCos);
                double w0Degrees = w0Radians.Degrees(); // 0...180

                _logger.LogDebug($"Hour angle             w0      = {Deg2Human(w0Degrees)}");

                double jRise = jTransit - w0Degrees / 360;
                double jSet = jTransit + w0Degrees / 360;

                _logger.LogDebug($"Sunrise                j_rise  = {J2Human(jRise, debugTz)}");
                _logger.LogDebug($"Sunset                 j_set   = {J2Human(jSet, debugTz)}");
                _logger.LogDebug($"Day length                       {w0Degrees / (180 / 24):F3} hours");

                return Tuple.Create(J2Ts(jRise), J2Ts(jSet), (bool?)null);
            }
            catch (ArgumentException)
            {
                return Tuple.Create<double, double, bool?>(0, 0, someCos > 0.0);
            }
        }
    }

    // Extension methods for Math since C# doesn't have Radians/Degrees conversion built-in
    public static class MathExtensions
    {
        public static double Radians(this double degrees)
        {
            return degrees * Math.PI / 180.0;
        }

        public static double Degrees(this double radians)
        {
            return radians * 180.0 / Math.PI;
        }
    }
}
