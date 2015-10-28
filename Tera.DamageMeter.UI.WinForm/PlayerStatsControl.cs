using System;
using System.Drawing;
using System.Windows.Forms;

namespace Tera.DamageMeter
{
    public class PlayerStatsControl : Control
    {
        public PlayerStatsControl()
        {
            DoubleBuffered = true;

            HighlightColor = Color.DodgerBlue;
            BackColor = Color.DimGray;
            ForeColor = Color.White;
        }

        public PlayerInfo PlayerInfo { get; set; }
        public long TotalDamage { get; set; }
        public Color HighlightColor { get; set; }
        public ClassIcons ClassIcons { get; set; }

        private static float DrawStringRightToLeft(Graphics graphics, string s, Font font, Brush brush, float x, float y)
        {
            x -= graphics.MeasureString(s, font).Width;
            graphics.DrawString(s, font, brush, x, y);
            return x;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var graphics = e.Graphics;
            var rect = ClientRectangle;

            using (var backBrush = new SolidBrush(BackColor))
            {
                graphics.FillRectangle(backBrush, rect);
            }

            if (PlayerInfo == null)
                return;

            var damageFraction = (double) PlayerInfo.Dealt.Damage/TotalDamage;
            var highlightRect = new Rectangle(rect.Left, rect.Top, (int) Math.Round(rect.Width*damageFraction),
                rect.Height);

            using (var highlightBrush = new SolidBrush(HighlightColor))
            {
                graphics.FillRectangle(highlightBrush, highlightRect);
            }

            var iconWidth = rect.Height;
            graphics.DrawImage(ClassIcons.GetImage(PlayerInfo.Class), (iconWidth - ClassIcons.Size)/2,
                (rect.Height - ClassIcons.Size)/2);
            var textRect = Rectangle.FromLTRB(rect.Left + iconWidth, rect.Top, rect.Right, rect.Bottom);

            using (var bigFont = new Font(Font.FontFamily, (int) Math.Round(rect.Height*0.45), GraphicsUnit.Pixel))
            using (var smallFont = new Font(Font.FontFamily, (int) Math.Round(rect.Height*0.35), GraphicsUnit.Pixel))
            using (var textBrush = new SolidBrush(ForeColor))
            {
                graphics.DrawString(PlayerInfo.Name, bigFont, textBrush, textRect.Left, textRect.Top);
                var row2Y = (float) Math.Round(textRect.Top + 0.55*textRect.Height);
                var infoText = string.Format("crit {0} hits {1} hurt {2}",
                    Helpers.FormatPercent((double) PlayerInfo.Dealt.Crits/PlayerInfo.Dealt.Hits),
                    PlayerInfo.Dealt.Hits,
                    Helpers.FormatValue(PlayerInfo.Received.Damage));
                graphics.DrawString(infoText, smallFont, textBrush, textRect.Left, row2Y);

                float x = textRect.Right;
                x = DrawStringRightToLeft(graphics, Helpers.FormatPercent(damageFraction), bigFont, textBrush, x,
                    textRect.Top);

                x = textRect.Right;
                x = DrawStringRightToLeft(graphics, Helpers.FormatValue(PlayerInfo.Dealt.Damage), smallFont, Brushes.Red,
                    x, row2Y);
                if (PlayerInfo.Dealt.Heal != 0)
                {
                    x = DrawStringRightToLeft(graphics, "+", smallFont, textBrush, x, row2Y);
                    x = DrawStringRightToLeft(graphics, Helpers.FormatValue(PlayerInfo.Dealt.Heal), smallFont,
                        Brushes.LawnGreen, x, row2Y);
                }
            }
        }
    }
}