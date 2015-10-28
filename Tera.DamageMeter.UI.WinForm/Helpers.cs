using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tera.DamageMeter
{
    public class Helpers
    {
        public static string FormatValue(long value)
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
            const string thinspace = "\u2009";
            switch (exponent)
            {
                case 0:
                    suffix = "";
                    break;
                case 3:
                    suffix = thinspace + "k";
                    break;
                case 6:
                    suffix = thinspace + "M";
                    break;
                case 9:
                    suffix = thinspace + "B";
                    break;
                default:
                    suffix = thinspace + "E" + thinspace + exponent;
                    break;
            }
            return rounded.ToString() + suffix;
        }
    }
}
