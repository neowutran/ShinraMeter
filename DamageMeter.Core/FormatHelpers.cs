// Copyright (c) Gothos
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Globalization;

namespace DamageMeter
{
    public class FormatHelpers
    {
        public const string Thinspace = "\u2009";

        public static readonly FormatHelpers Pretty = new FormatHelpers {UnitSeparator = Thinspace};

        public static readonly FormatHelpers Invariant = new FormatHelpers {UnitSeparator = "", CultureInfo = CultureInfo.InvariantCulture};

        private static FormatHelpers _instance;

        private FormatHelpers()
        {
            CultureInfo = CultureInfo.CurrentUICulture;
        }

        public CultureInfo CultureInfo { get; set; }
        public string UnitSeparator { get; set; }

        public static FormatHelpers Instance => _instance ?? (_instance = new FormatHelpers());

        public string FormatTimeSpan(TimeSpan? timeSpan)
        {
            return timeSpan == null ? null : FormatTimeSpan(timeSpan.Value);
        }

        public string FormatTimeSpan(TimeSpan timeSpan)
        {
            if (timeSpan.Hours != 0 || timeSpan.Days != 0) { return timeSpan.ToString("g", CultureInfo); }
            return timeSpan.ToString(@"mm\:ss");
        }

        public string FormatValue(long? value)
        {
            return value == null ? null : FormatValue(value.Value);
        }

        public string FormatValue(long value)
        {
            var exponent = 0;
            decimal decimalValue = value;
            decimal rounded;
            string suffix;
            if (CultureInfo.Name.StartsWith("ko"))
            {
                while (Math.Abs(rounded = (long) decimal.Round(decimalValue)) >= 10000)
                {
                    decimalValue /= 10;
                    exponent++;
                }
                while (exponent % 4 != 0)
                {
                    rounded *= 0.1m;
                    exponent++;
                }

                suffix = exponent switch
                {
                    0 => "",
                    4 => UnitSeparator + "만",
                    8 => UnitSeparator + "억",
                    _ => UnitSeparator + "E" + UnitSeparator + exponent
                };
            }
            else
            {
                while (Math.Abs(rounded = (long) decimal.Round(decimalValue)) >= 1000)
                {
                    decimalValue /= 10;
                    exponent++;
                }
                while (exponent % 3 != 0)
                {
                    rounded *= 0.1m;
                    exponent++;
                }

                suffix = exponent switch
                {
                    0 => "",
                    3 => UnitSeparator + "k",
                    6 => UnitSeparator + "M",
                    9 => UnitSeparator + "B",
                    _ => UnitSeparator + "E" + UnitSeparator + exponent
                };
            }
            return string.Format(CultureInfo, "{0}{1}", rounded, suffix);
        }

        public string FormatPercent(double fraction)
        {
            return double.IsNaN(fraction) ? null : fraction.ToString("P1", CultureInfo);
        }

        public string FormatDouble(double value)
        {
            return double.IsNaN(value) ? null : value.ToString("F2", CultureInfo);
        }
    }
}