// Copyright (c) Gothos
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Globalization;

namespace Tera.DamageMeter
{
    public class FormatHelpers
    {
        public CultureInfo CultureInfo { get; set; }
        public string UnitSeparator { get; set; }

        public const string Thinspace = "\u2009";

        public static readonly FormatHelpers Pretty = new FormatHelpers { UnitSeparator = Thinspace };
        public static readonly FormatHelpers Invariant = new FormatHelpers { UnitSeparator = "", CultureInfo = CultureInfo.InvariantCulture };

        public string FormatTimeSpan(TimeSpan? timeSpan)
        {
            if (timeSpan == null)
                return null;

            return FormatTimeSpan(timeSpan.Value);
        }

        public string FormatTimeSpan(TimeSpan timeSpan)
        {
            if (timeSpan.Hours != 0 || timeSpan.Days != 0)
                return timeSpan.ToString("g", CultureInfo);
            else
                return timeSpan.ToString(@"mm\:ss");
        }

        public string FormatValue(long? value)
        {
            if (value == null)
                return null;

            return FormatValue(value.Value);
        }

        public string FormatValue(long value)
        {
            int exponent = 0;
            decimal decimalValue = value;
            decimal rounded;
            while (Math.Abs(rounded = (long)Decimal.Round(decimalValue)) >= 1000)
            {
                decimalValue /= 10;
                exponent++;
            }
            while (exponent % 3 != 0)
            {
                rounded *= 0.1m;
                exponent++;
            }
            string suffix;

            switch (exponent)
            {
                case 0:
                    suffix = "";
                    break;
                case 3:
                    suffix = UnitSeparator + "k";
                    break;
                case 6:
                    suffix = UnitSeparator + "M";
                    break;
                case 9:
                    suffix = UnitSeparator + "B";
                    break;
                default:
                    suffix = UnitSeparator + "E" + UnitSeparator + exponent;
                    break;
            }
            return string.Format(CultureInfo, "{0}{1}", rounded, suffix);
        }

        public string FormatPercent(double fraction)
        {
            if (double.IsNaN(fraction))
                return null;

            return fraction.ToString("P1", CultureInfo);
        }
    }
}