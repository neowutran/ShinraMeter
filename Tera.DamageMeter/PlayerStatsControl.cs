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

        public void Fetch(PlayerStats playerStats, long totalDamage)
        {
            PlayerNameLabel.Text = playerStats.Name;
            PlayerClassLabel.Text = playerStats.Class.ToString();
            DamageLabel.Text = Helpers.FormatValue(playerStats.Dealt.Damage);
            HealLabel.Text = Helpers.FormatValue(playerStats.Dealt.Heal);
            InfoLabel.Text = string.Format("Critrate {0:f1}% Hits {1} Received {2}",
                                           (playerStats.Dealt.Crits * 100.0 / playerStats.Dealt.Hits),
                                           playerStats.Dealt.Hits,
                                           Helpers.FormatValue(playerStats.Received.Damage));
            DamagePercentLabel.Text = (playerStats.Dealt.Damage * 100.0 / totalDamage).ToString("f1") + "%";


            HealLabel.Visible = playerStats.Dealt.Heal != 0;
            DamageHealSeparator.Visible = HealLabel.Visible;

            // Positioning
            PlayerNameLabel.Left = PlayerClassLabel.Right;
            DamagePercentLabel.Left = Width - 4 - DamagePercentLabel.Width;

            int pos = Width - 4;
            if (DamageLabel.Visible)
            {
                pos -= DamageLabel.Width;
                DamageLabel.Left = pos;
            }
            if (DamageHealSeparator.Visible)
            {
                pos -= DamageHealSeparator.Width;
                DamageHealSeparator.Left = pos;
            }
            if (HealLabel.Visible)
            {
                pos -= HealLabel.Width;
                HealLabel.Left = pos;
            }
        }
    }
}
