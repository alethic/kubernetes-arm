using System;

namespace ArmKubeOper.Extensions
{

    public static class TimeSpanExtensions
    {

        /// <summary>
        /// Parses a Kubernetes timespan string.
        /// </summary>
        /// <param name="pTime"></param>
        /// <returns></returns>
        /// <exception cref="FormatException"></exception>
        public static TimeSpan ParseKubeString(string pTime)
        {
            if (pTime.EndsWith("ms"))
            {
                return double.TryParse(pTime.Trim().Replace("ms", ""), out var dTime) ? TimeSpan.FromMilliseconds(dTime) : throw new FormatException("Unable to convert " + pTime);
            }
            else if (pTime.EndsWith("s"))
            {
                return double.TryParse(pTime.Trim().Replace("s", ""), out var dTime) ? TimeSpan.FromSeconds(dTime) : throw new FormatException("Unable to convert " + pTime);
            }
            else if (pTime.EndsWith("m"))
            {
                return double.TryParse(pTime.Trim().Replace("m", ""), out var dTime) ? TimeSpan.FromMinutes(dTime) : throw new FormatException("Unable to convert " + pTime);
            }
            else if (pTime.EndsWith("h"))
            {
                return double.TryParse(pTime.Trim().Replace("h", ""), out var dTime) ? TimeSpan.FromHours(dTime) : throw new FormatException("Unable to convert " + pTime);
            }
            else if (pTime.EndsWith("d"))
            {
                return double.TryParse(pTime.Trim().Replace("d", ""), out var dTime) ? TimeSpan.FromDays(dTime) : throw new FormatException("Unable to convert " + pTime);
            }

            throw new FormatException(pTime + " is not a valid timeformat");
        }

    }

}
