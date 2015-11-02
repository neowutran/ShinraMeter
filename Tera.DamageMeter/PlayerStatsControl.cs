// Copyright (c) Gothos
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Tera.DamageMeter
{
    public class PlayerStatsControl : Control
    {
        public PlayerInfo PlayerInfo { get; set; }
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
            var formatHelpers = FormatHelpers.Pretty;
            var placeHolder = DamagePlaceHolders.FromPlayerInfo(PlayerInfo, formatHelpers);

            var graphics = e.Graphics;
            var rect = ClientRectangle;

            using (var backBrush = new SolidBrush(BackColor))
            {
                graphics.FillRectangle(backBrush, rect);
            }

            if (PlayerInfo == null)
                return;

            var highlightRect = new Rectangle(rect.Left, rect.Top, (int)Math.Round(rect.Width * PlayerInfo.DamageFraction), rect.Height);

            using (var highlightBrush = new SolidBrush(HighlightColor))
            {
                graphics.FillRectangle(highlightBrush, highlightRect);
            }

            var iconWidth = (int)(rect.Height * 0.8);
            graphics.DrawImage(ClassIcons.GetImage(PlayerInfo.Class), (iconWidth - ClassIcons.Size) / 2, (rect.Height - ClassIcons.Size) / 2);
            var textRect = Rectangle.FromLTRB(rect.Left + iconWidth, rect.Top, rect.Right, rect.Bottom);

            using (var bigFont = new Font(Font.FontFamily, (int)Math.Round(rect.Height * 0.34), GraphicsUnit.Pixel))
            using (var smallFont = new Font(Font.FontFamily, (int)Math.Round(rect.Height * 0.25), GraphicsUnit.Pixel))
            using (var textBrush = new SolidBrush(ForeColor))
            {
                var row1LeftText = placeHolder.Replace(Strings.PlayerBoxLeftLine1);
                var row2RightText = placeHolder.Replace(Strings.PlayerBoxRightLine1);
                var row2LeftText = placeHolder.Replace(Strings.PlayerBoxLeftLine2);
                var row3LeftText = placeHolder.Replace(Strings.PlayerBoxLeftLine3);
                var row3RightText = placeHolder.Replace(Strings.PlayerBoxRightLine3);

                var row1Y = (float)Math.Round(textRect.Top + 0.00 * textRect.Height);
                var row2Y = (float)Math.Round(textRect.Top + 0.36 * textRect.Height);
                var row3Y = (float)Math.Round(textRect.Top + 0.64 * textRect.Height);

                graphics.DrawString(row1LeftText, bigFont, textBrush, textRect.Left, row1Y);
                graphics.DrawString(row2LeftText, smallFont, textBrush, textRect.Left, row2Y);
                graphics.DrawString(row3LeftText, smallFont, textBrush, textRect.Left, row3Y);

                float x = textRect.Right;
                x = DrawStringRightToLeft(graphics, row2RightText, bigFont, textBrush, x, textRect.Top);

                x = textRect.Right;
                x = DrawStringRightToLeft(graphics, formatHelpers.FormatValue(PlayerInfo.Dealt.Damage), smallFont, Brushes.Red, x, row2Y);
                if (PlayerInfo.Dealt.Heal != 0)
                {
                    x = DrawStringRightToLeft(graphics, "+", smallFont, textBrush, x, row2Y);
                    x = DrawStringRightToLeft(graphics, formatHelpers.FormatValue(PlayerInfo.Dealt.Heal), smallFont, Brushes.LawnGreen, x, row2Y);
                }

                x = textRect.Right;
                x = DrawStringRightToLeft(graphics, row3RightText, smallFont, textBrush, x, row3Y);
            }
        }

        public PlayerStatsControl()
        {
            DoubleBuffered = true;

            HighlightColor = Color.RoyalBlue;
            BackColor = Color.Navy;
            ForeColor = Color.White;
        }
    }
}
