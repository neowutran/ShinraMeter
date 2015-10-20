using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Tera.DamageMeter
{
    public partial class PlayerStatsControl : UserControl
    {
        public PlayerStatsControl()
        {
            InitializeComponent();
        }

        private static string FormatValue(long value)
        {
            int exponent = 0;
            decimal decimalValue = value;
            decimal rounded;
            while (Math.Abs(rounded = (long)decimal.Round(decimalValue)) >= 1000)
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

        public void Fetch(PlayerStats playerStats)
        {
            PlayerNameLabel.Text = playerStats.Name;
            PlayerClassLabel.Text = playerStats.Class.ToString();
            DamageLabel.Text = FormatValue(playerStats.Damage);
            HealLabel.Text = FormatValue(playerStats.Heal);

            PlayerNameLabel.Left = PlayerClassLabel.Right;
            int pos = Width - 4;
            HealLabel.Visible = playerStats.Heal != 0;
            DamageHealSeparator.Visible = HealLabel.Visible;
            if (HealLabel.Visible)
            {
                pos -= HealLabel.Width;
                HealLabel.Left = pos;
            }
            if (DamageHealSeparator.Visible)
            {
                pos -= DamageHealSeparator.Width;
                DamageHealSeparator.Left = pos;
            }
            if (DamageLabel.Visible)
            {
                pos -= DamageLabel.Width;
                DamageLabel.Left = pos;
            }
        }
    }
}
