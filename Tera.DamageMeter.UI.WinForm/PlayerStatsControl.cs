using System;
using System.Drawing;
using System.Runtime.InteropServices;
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

            var highlightRect = new Rectangle(rect.Left, rect.Top,
                (int) Math.Round(rect.Width*(PlayerData.DamageFraction/100)),
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
                var format  = new FormatHelpers();
                var infoText = string.Format("{0} {1}/s {2} {3}",
                    PlayerData.DamageFraction+"%",
                    format.FormatValue(PlayerData.PlayerInfo.Dps),
                    format.FormatValue(PlayerData.PlayerInfo.Dealt.Damage),
                    PlayerData.PlayerInfo.Dealt.CritRate+"%"
                    );
                graphics.DrawString(infoText, smallFont, textBrush, textRect.Left, row2Y);
            }
        }
    }
}