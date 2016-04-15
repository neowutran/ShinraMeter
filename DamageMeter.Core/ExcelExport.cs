using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OfficeOpenXml;
using OfficeOpenXml.Drawing;
using DamageMeter.TeraDpsApi;
using Data;
using Tera.Game;
using System.IO;
using System.Drawing;
using OfficeOpenXml.Style;

namespace DamageMeter
{
    class ExcelExport
    {
    public static void ExcelSave(EncounterBase data)
        {
            BasicTeraData BTD = BasicTeraData.Instance;
            if (!BTD.WindowData.Excel) return;
            NpcInfo Boss = BTD.MonsterDatabase.GetOrPlaceholder(ushort.Parse(data.areaId), uint.Parse(data.bossId));
            var dir = Path.Combine(BTD.ResourceDirectory, $"logs/{Boss.Area}");
            Directory.CreateDirectory(dir);
            var fname = Path.Combine(dir, $"{Boss.Name} {DateTime.Now.ToString("yyyy-MM-dd hh-mm-ss")}.xlsx");
            FileInfo file = new FileInfo(fname);
            if (file.Exists) return; //the only case this can happen is BAM mobtraining, that's not so interesting statistic to deal with more complex file names.
            using (ExcelPackage package = new ExcelPackage(file))
            {
                ExcelWorksheet ws = package.Workbook.Worksheets.Add("Boss");
                ws.DefaultRowHeight = 30;
                ws.Cells.Style.Font.Size = 14;
                ws.Cells.Style.Font.Name = "Arial";
                ws.Cells.Style.Font.Color.SetColor(Color.White);
                ws.Cells.Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells.Style.Fill.BackgroundColor.SetColor(Color.Black); ;
                ws.Cells[1, 1].Value = $"{Boss.Area}: {Boss.Name}";
                ws.Cells[1, 1, 1, 8].Merge = true;
                ws.Cells[1, 1, 1, 8].Style.Font.Bold = true;
                ws.Cells[1, 1, 1, 8].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                ws.Cells[2, 2].Value = "Name";
                ws.Cells[2, 3].Value = "Damage %";
                ws.Cells[2, 4].Value = "Damage";
                ws.Cells[2, 5].Value = "DPS";
                ws.Cells[2, 6].Value = "Crit %";
                ws.Cells[2, 7].Value = "Deaths";
                ws.Cells[2, 8].Value = "Death time";
                int i = 2;
                foreach (var user in data.members)
                {
                    i++;
                    AddImage(ws, i, 1, ClassIcons.Instance.GetBitmap((PlayerClass)Enum.Parse(typeof(PlayerClass),user.playerClass)));
                    ws.Cells[i, 2].Value = $"{user.playerServer}: {user.playerName}";
                    ws.Cells[i, 3].Value = double.Parse(user.playerTotalDamagePercentage)/100;
                    ws.Cells[i, 3].Style.Numberformat.Format = "0.0%";
                    ws.Cells[i, 4].Value = long.Parse(user.playerTotalDamage);
                    ws.Cells[i, 4].Style.Numberformat.Format = @"#,#00,\k";
                    ws.Cells[i, 5].Value = long.Parse(user.playerDps);
                    ws.Cells[i, 5].Style.Numberformat.Format = @"#,#00,\k\/\s";
                    ws.Cells[i, 6].Value = double.Parse(user.playerAverageCritRate)/100;
                    ws.Cells[i, 6].Style.Numberformat.Format = "0.0%";
                    ws.Cells[i, 7].Value = long.Parse(user.playerDeaths);
                    ws.Cells[i, 8].Value = long.Parse(user.playerDeathDuration);
                    ws.Cells[i, 8].Style.Numberformat.Format = @"0\s";
                }
                ws.Column(1).Width=5.5;
                ws.Column(2).AutoFit();
                ws.Column(3).AutoFit();
                ws.Column(4).AutoFit();
                ws.Column(5).AutoFit();
                ws.Column(6).AutoFit();
                ws.Column(7).AutoFit();
                ws.Column(8).AutoFit();
                ws.Cells[1, 1, i, 8].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                ws.Cells[1, 1, i, 8].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                var border = ws.Cells[1, 1, i, 8].Style.Border;
                border.Bottom.Style = border.Top.Style = border.Left.Style = border.Right.Style = ExcelBorderStyle.Thick;
                border.Bottom.Color.SetColor(Color.White);
                border.Top.Color.SetColor(Color.White);
                border.Left.Color.SetColor(Color.White);
                border.Right.Color.SetColor(Color.White);
                package.Workbook.Properties.Title = Boss.Name;
                package.Workbook.Properties.Author = "ShinraMeter "+AutoUpdate.UpdateManager.Version;
                package.Workbook.Properties.Company = "github.com/neowutran & github.com/Gl0";
                package.Save();
            }
        }
        private static void AddImage(ExcelWorksheet ws, int rowIndex, int columnIndex, Bitmap image)
        {
            //How to Add a Image using EP Plus
            ExcelPicture picture = null;
            if (image != null)
            {
                picture = ws.Drawings.AddPicture("pic" + rowIndex.ToString() + columnIndex.ToString(), image);
                picture.From.Column = columnIndex-1;
                picture.From.Row = rowIndex-1;
                picture.SetSize(40, 40);
            }
        }

    }
}
