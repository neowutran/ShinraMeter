using System;
using System.Drawing;
using System.Windows.Forms;
using Tera.DamageMeter.UI.Handler;

namespace Tera.DamageMeter
{
    public class PlayerStatsControl : Control
    {
        public PlayerStatsControl(PlayerInfo playerInfo)
        {
            PlayerData = new PlayerData(playerInfo);
            DoubleBuffered = true;
            HighlightColor = Color.DodgerBlue;
            BackColor = Color.DimGray;
            ForeColor = Color.White;
        }

        public PlayerData PlayerData { get; }

        public Color HighlightColor { get; set; }
        public ClassIcons ClassIcons { get; set; }


        protected override void OnPaint(PaintEventArgs e)
        {
            var graphics = e.Graphics;
            var rect = ClientRectangle;

            using (var backBrush = new SolidBrush(BackColor))
            {
                graphics.FillRectangle(backBrush, rect);
            }

            var damageFraction = (double) PlayerData.PlayerInfo.Dealt.Damage/PlayerData.TotalDamage;
            var highlightRect = new Rectangle(rect.Left, rect.Top, (int) Math.Round(rect.Width*damageFraction),
                rect.Height);

            using (var highlightBrush = new SolidBrush(HighlightColor))
            {
                graphics.FillRectangle(highlightBrush, highlightRect);
            }

            var iconWidth = rect.Height;
            graphics.DrawImage(ClassIcons.GetImage(PlayerData.PlayerInfo.Class), (iconWidth - ClassIcons.Size)/2,
                (rect.Height - ClassIcons.Size)/2);
            var textRect = Rectangle.FromLTRB(rect.Left + iconWidth, rect.Top, rect.Right, rect.Bottom);

            using (var bigFont = new Font(Font.FontFamily, (int) Math.Round(rect.Height*0.45), GraphicsUnit.Pixel))
            using (var smallFont = new Font(Font.FontFamily, (int) Math.Round(rect.Height*0.35), GraphicsUnit.Pixel))
            using (var textBrush = new SolidBrush(ForeColor))
            {
                graphics.DrawString(PlayerData.PlayerInfo.Name, bigFont, textBrush, textRect.Left, textRect.Top);
                var row2Y = (float) Math.Round(textRect.Top + 0.55*textRect.Height);
                long dps = 0;
                long interval = 0;
                if (PlayerData.PlayerInfo.LastHit != 0 && PlayerData.PlayerInfo.FirstHit != 0)
                {
                    interval =
                        PlayerData.PlayerInfo.LastHit - PlayerData.PlayerInfo.FirstHit;
                    if (interval != 0)
                    {
                        dps = PlayerData.PlayerInfo.Dealt.Damage/interval;
                    }
                }

                var infoText = string.Format("{0} {1}/s {2} {3}",
                    Helpers.FormatPercent(damageFraction),
                    Helpers.FormatValue(dps),
                    Helpers.FormatValue(PlayerData.PlayerInfo.Dealt.Damage),
                    Helpers.FormatPercent((double) PlayerData.PlayerInfo.Dealt.Crits/PlayerData.PlayerInfo.Dealt.Hits)
                    );
                graphics.DrawString(infoText, smallFont, textBrush, textRect.Left, row2Y);
            }
        }
    }
}