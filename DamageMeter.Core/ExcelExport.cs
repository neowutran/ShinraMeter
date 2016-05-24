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
using System.Globalization;
using System.Runtime.InteropServices;
using System.Xml;
using System.Xml.Serialization;
using OfficeOpenXml.Drawing.Chart;

namespace DamageMeter
{
    static class EPPExstension
    {
        public static void InvisibleSerie(this ExcelBarChart chart, ExcelChartSerie series)
        {
            var i = 0;
            var found = false;
            foreach (var s in chart.Series)
            {
                if (s == series)
                {
                    found = true;
                    break;
                }
                ++i;
            }
            if (!found) throw new InvalidOperationException("series not found.");

            var nsm = chart.WorkSheet.Drawings.NameSpaceManager;
            var nschart = nsm.LookupNamespace("c");
            var nsa = nsm.LookupNamespace("a");
            var node = chart.ChartXml.SelectSingleNode(@"c:chartSpace/c:chart/c:plotArea/c:barChart/c:ser[c:idx[@val='" + i.ToString(CultureInfo.InvariantCulture) + "']]", nsm);
            var doc = chart.ChartXml;

            var spPr = doc.CreateElement("c:spPr", nschart);
            spPr.AppendChild(doc.CreateElement("a:noFill", nsa));
            var ln = spPr.AppendChild(doc.CreateElement("a:ln", nsa));
            ln.Attributes.Append(doc.CreateAttribute("w")).Value = "25400";
            ln.AppendChild(doc.CreateElement("a:noFill", nsa));

            node.AppendChild(spPr);
        }
        public static void FirstAxisDate(this ExcelBarChart chart, int maxVal)
        {
            var nsm = chart.WorkSheet.Drawings.NameSpaceManager;
            var nschart = nsm.LookupNamespace("c");
            var node = chart.ChartXml.SelectSingleNode(@"c:chartSpace/c:chart/c:plotArea/c:catAx[c:axId[@val='1']]", nsm);
            var doc = chart.ChartXml;
            node = RenameNode(node, nschart, "c:dateAx");
            node.SelectSingleNode("c:auto/@val", nsm).Value = "0";
            node.SelectSingleNode("c:tickLblPos/@val", nsm).Value = "none";
            node.SelectSingleNode("c:crosses/@val", nsm).Value = "max";
            var scale = node.SelectSingleNode("c:scaling", nsm);
            var max = doc.CreateElement("c:max", nschart);
            max.Attributes.Append(doc.CreateAttribute("val")).Value = maxVal.ToString();
            scale.AppendChild(max);

            var layout = doc.CreateElement("c:manualLayout", nschart);
            var child = doc.CreateElement("c:layoutTarget", nschart);
            child.Attributes.Append(doc.CreateAttribute("val")).Value = "inner";
            layout.AppendChild(child);
            child = doc.CreateElement("c:xMode", nschart);
            child.Attributes.Append(doc.CreateAttribute("val")).Value = "edge";
            layout.AppendChild(child);
            child = doc.CreateElement("c:yMode", nschart);
            child.Attributes.Append(doc.CreateAttribute("val")).Value = "edge";
            layout.AppendChild(child);
            child = doc.CreateElement("c:x", nschart);
            child.Attributes.Append(doc.CreateAttribute("val")).Value = "0.005";
            layout.AppendChild(child);
            child = doc.CreateElement("c:h", nschart);
            child.Attributes.Append(doc.CreateAttribute("val")).Value = (1F-1.6F/(maxVal+2)).ToString(CultureInfo.InvariantCulture);
            layout.AppendChild(child);
            node = doc.SelectSingleNode(@"c:chartSpace/c:chart/c:plotArea/c:layout", nsm);
            node.AppendChild(layout);
        }
        public static XmlNode RenameNode(XmlNode node, string namespaceUri, string qualifiedName)
        {
            if (node.NodeType == XmlNodeType.Element)
            {
                XmlElement oldElement = (XmlElement)node;
                XmlElement newElement =
                node.OwnerDocument.CreateElement(qualifiedName, namespaceUri);
                while (oldElement.HasAttributes)
                {
                    newElement.SetAttributeNode(oldElement.RemoveAttributeNode(oldElement.Attributes[0]));
                }
                while (oldElement.HasChildNodes)
                {
                    newElement.AppendChild(oldElement.FirstChild);
                }
                oldElement.ParentNode?.ReplaceChild(newElement, oldElement);
                return newElement;
            }
            return null;
        }
        public static void FixEppPlusBug(this ExcelChart chart) //epplus do not renumber series in the chart correctly if inserted not in sequence.
        {
            var xml = chart.ChartXml;
            var nsm = chart.WorkSheet.Drawings.NameSpaceManager;
            var badattr = xml.SelectNodes("//c:idx", nsm);
            int idx = 0;
            foreach (XmlNode attr in badattr)
            {
                attr.Attributes["val"].Value = idx.ToString();
                idx++;
            }
            badattr = xml.SelectNodes("//c:order", nsm);
            idx = 0;
            foreach (XmlNode attr in badattr)
            {
                attr.Attributes["val"].Value = idx.ToString();
                idx++;
            }
        }

    }

    class ExcelExport
    {
        private static BasicTeraData BTD = BasicTeraData.Instance;
        private static object savelock = new object();

      
        public static void ExcelSave(ExtendedStats exdata)
        {
            lock (savelock) //can't save 2 excel files at one time
            {
                if (!BTD.WindowData.Excel) return;
                var data = exdata.BaseStats;
                NpcInfo Boss = exdata.Entity.Info;
                
                /*
                Select save directory
                */
                string dir = "";
                if (BTD.WindowData.ExcelSaveDirectory == "")
                {
                    dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), $"ShinraMeter/{Boss.Area.Replace(":", "-")}");
                }
                else
                {
                    dir = BTD.WindowData.ExcelSaveDirectory;
                }

                /*
                Test if you have access to the user choice directory, if not, switch back to the default save directory
                */
                try
                {
                    Directory.CreateDirectory(dir);
                }
                catch
                {
                    dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), $"ShinraMeter/{Boss.Area.Replace(":", "-")}");
                    Directory.CreateDirectory(dir);
                }

                var fname = Path.Combine(dir, $"{Boss.Name.Replace(":", "-")} {DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss", CultureInfo.InvariantCulture)}.xlsx");
                FileInfo file = new FileInfo(fname);
                if (file.Exists) return; //the only case this can happen is BAM mobtraining, that's not so interesting statistic to deal with more complex file names.
                using (ExcelPackage package = new ExcelPackage(file))
                {
                    ExcelWorksheet details = CreateDetailsSheet(package, exdata);
                    ExcelWorksheet ws = package.Workbook.Worksheets.Add("Boss");
                    var linkStyle = package.Workbook.Styles.CreateNamedStyle("HyperLink");   //This one is language dependent
                    linkStyle.Style.Font.UnderLine = true;
                    linkStyle.Style.Font.Color.SetColor(Color.Blue);
                    ws.DefaultRowHeight = 30;
                    ws.Cells.Style.Font.Size = 14;
                    ws.Cells.Style.Font.Name = "Arial";
                    ws.Cells[1, 1].Value = $"{Boss.Area}: {Boss.Name} {TimeSpan.FromSeconds(double.Parse(data.fightDuration)).ToString(@"mm\:ss")}";
                    ws.Cells[1, 1, 1, 6].Merge = true;
                    ws.Cells[1, 1, 1, 8].Style.Font.Bold = true;
                    ws.Cells[1, 7].Value = long.Parse(data.partyDps);
                    ws.Cells[1, 7].Style.Numberformat.Format = @"#,#00,\k\/\s";
                    ws.Cells[2, 1].Value = "Ic";
                    ws.Cells[2, 1].Style.Font.Color.SetColor(Color.Transparent);
                    ws.Cells[2, 2].Value = "Name";
                    ws.Cells[2, 3].Value = "Deaths";
                    ws.Cells[2, 4].Value = "Death time";
                    ws.Cells[2, 5].Value = "Damage %";
                    ws.Cells[2, 6].Value = "Crit %";
                    ws.Cells[2, 7].Value = "DPS";
                    ws.Cells[2, 8].Value = "Damage";
                    int i = 2;
                    foreach (var user in data.members.OrderByDescending(x => long.Parse(x.playerTotalDamage)))
                    {
                        i++;
                        ws.Cells[i, 1].Value = i - 2;
                        AddImage(ws, i, 1, ClassIcons.Instance.GetBitmap((PlayerClass)Enum.Parse(typeof(PlayerClass), user.playerClass)));
                        ws.Cells[i, 2].Value = $"{user.playerServer}: {user.playerName}";
                        ws.Cells[i, 2].Hyperlink = CreateUserSheet(package.Workbook, user, exdata, details);
                        ws.Cells[i, 3].Value = long.Parse(user.playerDeaths);
                        ws.Cells[i, 4].Value = long.Parse(user.playerDeathDuration);
                        ws.Cells[i, 4].Style.Numberformat.Format = @"0\s";
                        ws.Cells[i, 5].Value = double.Parse(user.playerTotalDamagePercentage) / 100;
                        ws.Cells[i, 5].Style.Numberformat.Format = "0.0%";
                        ws.Cells[i, 6].Value = double.Parse(user.playerAverageCritRate) / 100;
                        ws.Cells[i, 6].Style.Numberformat.Format = "0.0%";
                        ws.Cells[i, 7].Value = long.Parse(user.playerDps);
                        ws.Cells[i, 7].Style.Numberformat.Format = @"#,#0,\k\/\s";
                        ws.Cells[i, 8].Value = long.Parse(user.playerTotalDamage);
                        ws.Cells[i, 8].Style.Numberformat.Format = @"#,#0,\k";
                    }
                    ws.Cells[1, 8].Formula = $"SUM(H3:H{i})";
                    ws.Cells[1, 8].Style.Numberformat.Format = @"#,#0,\k";
                    var border = ws.Cells[1, 1, i, 8].Style.Border;
                    border.Bottom.Style = border.Top.Style = border.Left.Style = border.Right.Style = ExcelBorderStyle.Thick;
                    ws.Cells[2, 1, i, 8].AutoFilter = true;

                    int j = i + 3;
                    ws.Cells[j, 1].Value = "Ic";
                    ws.Cells[j, 1].Style.Font.Color.SetColor(Color.Transparent);
                    ws.Cells[j, 2].Value = "Debuff name";
                    ws.Cells[j, 2, j, 7].Merge = true;
                    ws.Cells[j, 8].Value = "%";
                    ws.Cells[j, 2, j, 8].Style.Font.Bold = true;
                    foreach (var buf in data.debuffUptime)
                    {
                        j++;
                        var hotdot = BTD.HotDotDatabase.Get(int.Parse(buf.Key));
                        ws.Cells[j, 1].Value = j - i - 3;
                        AddImage(ws, j, 1, BTD.Icons.GetBitmap(hotdot.IconName));
                        ws.Cells[j, 2].Value = hotdot.Name;
                        if (!string.IsNullOrEmpty(hotdot.Tooltip)) ws.Cells[j, 2].AddComment("" + hotdot.Tooltip, "info");
                        ws.Cells[j, 2, j, 7].Merge = true;
                        ws.Cells[j, 8].Value = double.Parse(buf.Value) / 100;
                        ws.Cells[j, 8].Style.Numberformat.Format = "0%";
                    }
                    border = ws.Cells[i + 3, 1, j, 8].Style.Border;
                    border.Bottom.Style = border.Top.Style = border.Left.Style = border.Right.Style = ExcelBorderStyle.Thick;

                    AddCharts(ws, exdata, details, j);

                    ws.Column(1).Width = 5.6;
                    ws.Column(2).AutoFit();
                    ws.Column(3).AutoFit();
                    ws.Column(4).AutoFit();
                    ws.Column(5).AutoFit();
                    ws.Column(6).AutoFit();
                    ws.Column(7).AutoFit();
                    ws.Column(8).Width = 17;
                    ws.Column(2).Width = GetTrueColumnWidth(ws.Column(2).Width);
                    ws.Column(3).Width = GetTrueColumnWidth(ws.Column(3).Width);
                    ws.Column(4).Width = GetTrueColumnWidth(ws.Column(4).Width);
                    ws.Column(5).Width = GetTrueColumnWidth(ws.Column(5).Width);
                    ws.Column(6).Width = GetTrueColumnWidth(ws.Column(6).Width);
                    ws.Column(7).Width = GetTrueColumnWidth(ws.Column(7).Width);
                    ws.Cells[1, 1, j, 8].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    ws.Cells[1, 1, j, 8].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ws.PrinterSettings.FitToPage = true;

                    // I don't know why, but sometimes column height setting is lost.
                    for (int x = 1; x < j; ++x)
                    {
                        ws.Row(x).CustomHeight = true;
                        ws.Row(x).Height = 30;
                    }

                    ws.View.TabSelected = true;
                    details.Hidden = eWorkSheetHidden.Hidden;
                    package.Workbook.Properties.Title = Boss.Name;
                    package.Workbook.Properties.Author = "ShinraMeter " + AutoUpdate.UpdateManager.Version;
                    package.Workbook.Properties.Company = "github.com/neowutran & github.com/Gl0";
                    package.Save();
                }
            }
        }

        private static void AddCharts(ExcelWorksheet ws, ExtendedStats exdata, ExcelWorksheet details, int startrow)
        {
            int time = (int) (exdata.LastTick/TimeSpan.TicksPerSecond - exdata.FirstTick/TimeSpan.TicksPerSecond);
            int offset = exdata.PlayerBuffs.Keys.ToList().IndexOf(ws.Name) + 1;
            bool bossSheet = ws.Name == "Boss";
            if (!bossSheet && offset <= 0) return; //no buff data for user -> no graphs.
            offset = bossSheet ? 3 : 3 + offset*7;
            ExcelChart dps = ws.Drawings.AddChart(ws.Name + "DPS", eChartType.Line);
            dps.SetPosition(startrow + 1, 5, 0, 5);
            dps.SetSize(1200, 300);
            dps.Legend.Position = eLegendPosition.Top;
            if (time > 40) dps.XAxis.MajorUnit = time/20;
            ExcelChart typeDmg;
            ExcelChartSerie serieDmg;
            if (!bossSheet)
            {
                typeDmg = dps.PlotArea.ChartTypes[0];
                typeDmg.YAxis.Title.Text = "Damage";
                typeDmg.YAxis.Title.Rotation = 90;
                serieDmg = typeDmg.Series.Add(details.Cells[3, offset + 5, time + 3, offset + 5], details.Cells[3, 2, time + 3, 2]);
                serieDmg.Header = ws.Name+" Dmg";
            }
            var typeDps = dps.PlotArea.ChartTypes.Add(eChartType.Line);
            if (!bossSheet)
            {
                typeDps.UseSecondaryAxis = true;
                typeDps.YAxis.Title.Text = "Avg DPS";
            }
            typeDps.YAxis.Title.Rotation = 90;
            typeDps.YAxis.SourceLinked = false;
            typeDps.YAxis.Format = @"#,#0\k\/\s";//not sure why, but it loss sourcelink itself if we show only dps.
            var serieDps = typeDps.Series.Add(details.Cells[3, offset + 6, time + 3, offset + 6], details.Cells[3, 2, time + 3, 2]);
            serieDps.Header = ws.Name+" Avg DPS";
            if (bossSheet)
            {
                int col = 3;
                foreach (var user in exdata.PlayerBuffs)
                {
                    col += 7;
                    //var userDmg = typeDmg.Series.Add(details.Cells[3, col + 5, time + 3, col + 5], details.Cells[3, 2, time + 3, 2]);
                    //userDmg.Header = user.Key + " Dmg";
                    var userDps = typeDps.Series.Add(details.Cells[3, col + 6, time + 3, col + 6],
                        details.Cells[3, 2, time + 3, 2]);
                    userDps.Header = user.Key + " Avg DPS";
                }
            }
            //dps.FixEppPlusBug();// needed only if adding users dmg to main boss chart

            var numInt = bossSheet
                ? exdata.Debuffs.Sum(x => x.Value.Count()) - 1
                : exdata.PlayerBuffs[ws.Name].Times.Sum(x => x.Value.Count()) 
                    + exdata.PlayerBuffs[ws.Name].Death.Count() 
                    + exdata.PlayerBuffs[ws.Name].Aggro(exdata.Entity).Count() 
                    - 1;
            var numBuff = bossSheet
                ? exdata.Debuffs.Count
                : exdata.PlayerBuffs[ws.Name].Times.Count(x => x.Value.Count() > 0)
                  + (exdata.PlayerBuffs[ws.Name].Death.Count() > 0 ? 1 : 0)
                  + (exdata.PlayerBuffs[ws.Name].Aggro(exdata.Entity).Count() > 0 ? 1 : 0);
            if (numInt >= 0 && numBuff > 0)
            {
                ExcelChart buff = ws.Drawings.AddChart(ws.Name+"Buff", eChartType.BarStacked);
                var typeBuff = buff.PlotArea.ChartTypes[0];
                buff.SetPosition(startrow + 9, 5, 0, 5);
                buff.SetSize(1200, numBuff*25+38);
                buff.Legend.Remove();
                var serieStart = typeBuff.Series.Add(details.Cells[3, offset + 1, numInt + 3, offset + 1],
                    details.Cells[3, offset, numInt + 3, offset]);
                serieStart.Header = "Start";
                (buff as ExcelBarChart).InvisibleSerie(serieStart);
                var serieTime = typeBuff.Series.Add(details.Cells[3, offset + 2, numInt + 3, offset + 2],
                    details.Cells[3, offset, numInt + 3, offset]);
                serieTime.Header = "Time";
                typeBuff.YAxis.MajorUnit = (time >= 40) ? (double)(time / 20) / 86400F : 1F / 86400F;
                typeBuff.YAxis.MinValue = 0F;
                typeBuff.YAxis.MaxValue = (double)time / 86400F;
                typeBuff.XAxis.Orientation = eAxisOrientation.MaxMin;
                typeBuff.XAxis.MinorTickMark = eAxisTickMark.None;
                typeBuff.YAxis.Crosses = eCrosses.Max;
                var typeAxis = buff.PlotArea.ChartTypes.Add(eChartType.BarStacked);
                typeAxis.UseSecondaryAxis = true;
                var serieAxis = typeAxis.Series.Add(details.Cells[3, offset + 4, numBuff - 1 + 3, offset + 4],
                    details.Cells[3, offset + 3, numBuff - 1 + 3, offset + 3]);
                serieAxis.Header = "Names";
                typeAxis.XAxis.Orientation = eAxisOrientation.MaxMin;
                typeAxis.XAxis.TickLabelPosition = eTickLabelPosition.NextTo;
                typeAxis.XAxis.MinorTickMark = eAxisTickMark.None;
                typeAxis.YAxis.Deleted = true;
                typeAxis.XAxis.Deleted = false;
                (buff as ExcelBarChart).FirstAxisDate(numBuff);
                //buff.FixEppPlusBug();
            }

        }
        private static ExcelWorksheet CreateDetailsSheet(ExcelPackage package, ExtendedStats exdata)
        {
            string timeFormat = (exdata.LastTick - exdata.FirstTick)/TimeSpan.TicksPerSecond < 40 ? "ss" : "mm:ss";
            ExcelWorksheet details = package.Workbook.Worksheets.Add("Details");
            details.Cells[1, 1].Value = "Boss";
            details.Cells[2, 1].Value = "Seconds";
            details.Cells[2, 2].Value = "Time";
            details.Column(2).Style.Numberformat.Format = timeFormat;
            details.Cells[2, 3].Value = "BuffNum";
            details.Cells[2, 4].Value = "Start";
            details.Column(4).Style.Numberformat.Format = timeFormat;
            details.Cells[2, 5].Value = "Duration";
            details.Column(5).Style.Numberformat.Format = timeFormat;
            details.Cells[2, 6].Value = "BuffName";
            details.Cells[2, 7].Value = "Axis";
            details.Cells[2, 8].Value = "Damage";
            details.Column(8).Style.Numberformat.Format = @"#,#0\k";
            details.Cells[2, 9].Value = "AvgDPS";
            details.Column(9).Style.Numberformat.Format = @"#,#0\k\/\s";
            details.Cells[1, 1, 1, 9].Merge = true;
            for (int t = 0; t <= exdata.LastTick / TimeSpan.TicksPerSecond - exdata.FirstTick / TimeSpan.TicksPerSecond; t++)
            {
                details.Cells[t + 3, 1].Value = t;
                details.Cells[t + 3, 2].Value = (double)t/86400;
            }
            int buffnum = 0;
            int j = 0;
            foreach (var buffPair in exdata.Debuffs)
            {
                if (buffPair.Value.Count()==0) continue;
                buffnum++;
                details.Cells[2 + buffnum, 6].Value = buffPair.Key.Name;
                details.Cells[2 + buffnum, 7].Value = 0;
                foreach (var buff in buffPair.Value.AllDurations())
                {
                    j++;
                    details.Cells[2 + j, 3].Value = buffnum;
                    details.Cells[2 + j, 4].Value = (double)(buff.Begin - exdata.FirstTick) / TimeSpan.TicksPerDay;
                    details.Cells[2 + j, 5].Value = (double)(buff.End - buff.Begin) / TimeSpan.TicksPerDay;
                }
            }
            long totalDamage = 0;
            j = 0;
            long delta = exdata.FirstTick - (exdata.FirstTick / TimeSpan.TicksPerSecond) * TimeSpan.TicksPerSecond;
            for (long curTick = exdata.FirstTick/TimeSpan.TicksPerSecond;
                      curTick <= exdata.LastTick/TimeSpan.TicksPerSecond;
                      curTick++)
            {
                j++;
                var damage =
                    exdata.PlayerSkills.Sum(
                        x => x.Value.Where(time => time.Key == curTick)
                             .Sum(skill => skill.Value.Sum(stat => stat.Value.Damage)));
                totalDamage += damage;
                details.Cells[j+2, 8].Value = damage/1000;
                if (curTick == exdata.LastTick / TimeSpan.TicksPerSecond)
                    details.Cells[j + 2, 9].Value = totalDamage * TimeSpan.TicksPerSecond / (exdata.LastTick - exdata.FirstTick)/1000;
                else if (j != 1)
                    details.Cells[j + 2, 9].Value = totalDamage * TimeSpan.TicksPerSecond / ((j - 1) * TimeSpan.TicksPerSecond + delta)/1000;
            }
            int i = 3;
            foreach (var user in exdata.PlayerBuffs)
            {
                i += 7;
                details.Cells[1, i].Value = user.Key;
                details.Cells[2, i].Value = "BuffNum";
                details.Cells[2, i+1].Value = "Start";
                details.Column(i+1).Style.Numberformat.Format = timeFormat;
                details.Cells[2, i+2].Value = "Duration";
                details.Column(i+2).Style.Numberformat.Format = timeFormat;
                details.Cells[2, i+3].Value = "BuffName";
                details.Cells[2, i+4].Value = "Axis";
                details.Cells[2, i+5].Value = "Damage";
                details.Column(i + 5).Style.Numberformat.Format = @"#,#0\k";
                details.Cells[2, i+6].Value = "AvgDPS";
                details.Column(i + 6).Style.Numberformat.Format = @"#,#0\k\/\s";
                details.Cells[1, i, 1, i+6].Merge = true;
                buffnum = 0;
                j = 0;
                foreach (var buffPair in user.Value.Times)
                {
                    if (buffPair.Value.Count() == 0) continue;
                    buffnum++;
                    details.Cells[2 + buffnum, i+3].Value = buffPair.Key.Name;
                    details.Cells[2 + buffnum, i+4].Value = 0;
                    foreach (var buff in buffPair.Value.AllDurations())
                    {
                        j++;
                        details.Cells[2 + j, i].Value = buffnum;
                        details.Cells[2 + j, i + 1].Value = (double)(buff.Begin - exdata.FirstTick) / TimeSpan.TicksPerDay;
                        details.Cells[2 + j, i + 2].Value = (double)(buff.End - buff.Begin) / TimeSpan.TicksPerDay;
                    }
                }
                if (user.Value.Death.Count() > 0)
                {
                    buffnum++;
                    details.Cells[2 + buffnum, i + 3].Value = "Death";
                    details.Cells[2 + buffnum, i + 4].Value = 0;
                    foreach (var buff in user.Value.Death.AllDurations())
                    {
                        j++;
                        details.Cells[2 + j, i].Value = buffnum;
                        details.Cells[2 + j, i + 1].Value = (double)(buff.Begin - exdata.FirstTick) / TimeSpan.TicksPerDay;
                        details.Cells[2 + j, i + 2].Value = (double)(buff.End - buff.Begin) / TimeSpan.TicksPerDay;
                    }
                }
                if (user.Value.Aggro(exdata.Entity).Count() > 0)
                {
                    buffnum++;
                    details.Cells[2 + buffnum, i + 3].Value = "Aggro";
                    details.Cells[2 + buffnum, i + 4].Value = 0;
                    foreach (var buff in user.Value.Aggro(exdata.Entity).AllDurations())
                    {
                        j++;
                        details.Cells[2 + j, i].Value = buffnum;
                        details.Cells[2 + j, i + 1].Value = (double)(buff.Begin - exdata.FirstTick) / TimeSpan.TicksPerDay;
                        details.Cells[2 + j, i + 2].Value = (double)(buff.End - buff.Begin) / TimeSpan.TicksPerDay;
                    }
                }
                totalDamage = 0;
                j = 0;
                for (long curTick = exdata.FirstTick / TimeSpan.TicksPerSecond;
                          curTick <= exdata.LastTick / TimeSpan.TicksPerSecond;
                          curTick++)
                {
                    j++;
                    var damage =
                        exdata.PlayerSkills.Where(all => all.Key==user.Key).Sum(
                            x => x.Value.Where(time => time.Key == curTick)
                                 .Sum(skill => skill.Value.Sum(stat => stat.Value.Damage)));
                    totalDamage += damage;
                    details.Cells[j + 2, i + 5].Value = damage/1000;
                    if (curTick == exdata.LastTick / TimeSpan.TicksPerSecond)
                        details.Cells[j + 2, i + 6].Value = totalDamage * TimeSpan.TicksPerSecond / (exdata.LastTick - exdata.FirstTick)/1000;
                    else if (j != 1)
                        details.Cells[j + 2, i + 6].Value = totalDamage * TimeSpan.TicksPerSecond / ((j - 1) * TimeSpan.TicksPerSecond + delta)/1000;
                }
            }

            return details;
        }

        private static double GetTrueColumnWidth(double width)
        {
            //DEDUCE WHAT THE COLUMN WIDTH WOULD REALLY GET SET TO
            double z = 1d;
            if (width >= (1 + 2F / 3))
                z = Math.Round((Math.Round(7 * (width - 1F / 256), 0) - 5) / 7, 2);
            else
                z = Math.Round((Math.Round(12 * (width - 1F / 256), 0) - Math.Round(5 * width, 0)) / 12, 2);

            //HOW FAR OFF? (WILL BE LESS THAN 1)
            double errorAmt = width - z;

            //CALCULATE WHAT AMOUNT TO TACK ONTO THE ORIGINAL AMOUNT TO RESULT IN THE CLOSEST POSSIBLE SETTING 
            double adj = 0d;
            if (width >= (1 + 2 / 3))
                adj = (Math.Round(7 * errorAmt - 7F / 256, 0)) / 7;
            else
                adj = ((Math.Round(12 * errorAmt - 12F / 256, 0)) / 12) + (2F / 12);

            //RETURN A SCALED-VALUE THAT SHOULD RESULT IN THE NEAREST POSSIBLE VALUE TO THE TRUE DESIRED SETTING
            if (z > 0)
                return width + adj;

            return 0d;
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
                picture.From.ColumnOff = 12000;
                picture.From.RowOff = 12000;
                picture.SetSize(38, 38);
            }
        }
        private static ExcelHyperLink CreateUserSheet(ExcelWorkbook wb, Members user, ExtendedStats exdata, ExcelWorksheet details)
        {
            ExcelWorksheet ws = wb.Worksheets.Add($"{user.playerServer}_{user.playerName}");
            var rgc = new RaceGenderClass("Common", "Common", user.playerClass);
            ws.DefaultRowHeight = 30;
            ws.Cells.Style.Font.Size = 14;
            ws.Cells.Style.Font.Name = "Arial";
            AddImage(ws, 1, 1, ClassIcons.Instance.GetBitmap((PlayerClass)Enum.Parse(typeof(PlayerClass), user.playerClass)));
            ws.Cells[1, 2].Value = $"{user.playerServer}: {user.playerName}";
            ws.Cells[1, 2, 1, 10].Merge = true;
            ws.Cells[1, 2, 1, 10].Style.Font.Bold = true;
            ws.Cells[2, 2].Value = "Skill";
            ws.Cells[2, 3].Value = "Damage %";
            ws.Cells[2, 4].Value = "Damage";
            ws.Cells[2, 5].Value = "Crit %";
            ws.Cells[2, 6].Value = "Hits";
            ws.Cells[2, 7].Value = "Max Crit";
            ws.Cells[2, 8].Value = "Min Crit";
            ws.Cells[2, 9].Value = "Avg Crit";
            ws.Cells[2, 10].Value = "Avg White";
            int i = 2;
            foreach (var stat in user.skillLog.OrderByDescending(x => long.Parse(x.skillTotalDamage)))
            {
                i++;
                var skill = BTD.SkillDatabase.GetOrNull(rgc, int.Parse(stat.skillId));
                if (skill == null)
                {
                    ws.Cells[i, 1].Value = i-2;
                    ws.Cells[i, 2].Value = "Some pet skill";
                }
                else
                {
                    ws.Cells[i, 1].Value = i - 2;
                    AddImage(ws, i, 1, BTD.Icons.GetBitmap(skill.IconName));
                    ws.Cells[i, 2].Value = skill.Name;
                }
                ws.Cells[i, 3].Value = double.Parse(stat.skillDamagePercent) / 100;
                ws.Cells[i, 3].Style.Numberformat.Format = "0.0%";
                ws.Cells[i, 4].Value = long.Parse(stat.skillTotalDamage);
                ws.Cells[i, 4].Style.Numberformat.Format = @"#,#0,\k";
                ws.Cells[i, 5].Value = double.Parse(stat.skillCritRate) / 100;
                ws.Cells[i, 5].Style.Numberformat.Format = "0.0%";
                ws.Cells[i, 6].Value = long.Parse(stat.skillHits);
                ws.Cells[i, 7].Value = long.Parse(stat.skillHighestCrit);
                ws.Cells[i, 7].Style.Numberformat.Format = @"#,#0,\k";
                ws.Cells[i, 8].Value = long.Parse(stat.skillLowestCrit);
                ws.Cells[i, 8].Style.Numberformat.Format = @"#,#0,\k";
                ws.Cells[i, 9].Value = long.Parse(stat.skillAverageCrit);
                ws.Cells[i, 9].Style.Numberformat.Format = @"#,#0,\k";
                ws.Cells[i, 10].Value = long.Parse(stat.skillAverageWhite);
                ws.Cells[i, 10].Style.Numberformat.Format = @"#,#0,\k";
            }
            var border = ws.Cells[1, 1, i, 10].Style.Border;
            border.Bottom.Style = border.Top.Style = border.Left.Style = border.Right.Style = ExcelBorderStyle.Thick;
            ws.Cells[2, 1, i, 10].AutoFilter = true;

            int j = i + 3;
            ws.Cells[j, 2].Value = "Buff name";
            ws.Cells[j, 2, j, 9].Merge = true;
            ws.Cells[j, 10].Value = "%";
            ws.Cells[j, 2, j, 10].Style.Font.Bold = true;
            foreach (var buf in user.buffUptime)
            {
                j++;
                var hotdot = BTD.HotDotDatabase.Get(int.Parse(buf.Key));
                ws.Cells[j, 1].Value = j - i - 3;
                AddImage(ws, j, 1, BTD.Icons.GetBitmap(hotdot.IconName));
                ws.Cells[j, 2].Value = hotdot.Name;
                if (!string.IsNullOrEmpty(hotdot.Tooltip)) ws.Cells[j, 2].AddComment("" + hotdot.Tooltip, "info");
                ws.Cells[j, 2, j, 9].Merge = true;
                ws.Cells[j, 10].Value = double.Parse(buf.Value) / 100;
                ws.Cells[j, 10].Style.Numberformat.Format = "0%";
                if (!string.IsNullOrEmpty(hotdot.ItemName)) ws.Cells[j, 10].AddComment("" + hotdot.ItemName, "info");
            }
            border = ws.Cells[i + 3, 1, j, 10].Style.Border;
            border.Bottom.Style = border.Top.Style = border.Left.Style = border.Right.Style = ExcelBorderStyle.Thick;

            AddCharts(ws, exdata, details, j);

            ws.Column(1).Width = 5.6;
            ws.Column(2).AutoFit();
            ws.Column(3).AutoFit();
            ws.Column(4).AutoFit();
            ws.Column(5).AutoFit();
            ws.Column(6).AutoFit();
            ws.Column(7).AutoFit();
            ws.Column(8).AutoFit();
            ws.Column(9).AutoFit();
            ws.Column(10).AutoFit();
            ws.Column(2).Width = GetTrueColumnWidth(ws.Column(2).Width);
            ws.Column(3).Width = GetTrueColumnWidth(ws.Column(3).Width);
            ws.Column(4).Width = GetTrueColumnWidth(ws.Column(4).Width);
            ws.Column(5).Width = GetTrueColumnWidth(ws.Column(5).Width);
            ws.Column(6).Width = GetTrueColumnWidth(ws.Column(6).Width);
            ws.Column(7).Width = GetTrueColumnWidth(ws.Column(7).Width);
            ws.Column(8).Width = GetTrueColumnWidth(ws.Column(8).Width);
            ws.Column(9).Width = GetTrueColumnWidth(ws.Column(9).Width);
            ws.Column(10).Width = GetTrueColumnWidth(ws.Column(10).Width);
            ws.Cells[1, 1, j, 10].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            ws.Cells[1, 1, j, 10].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            ws.PrinterSettings.FitToPage = true;

            // I don't know why, but sometimes column height setting is lost.
            for (int x = 1; x<j; ++x)
            {
                ws.Row(x).CustomHeight = true;
                ws.Row(x).Height = 30;
            }

            // If sheet name contains space character, name should be enclosed in single quotes.
            return new ExcelHyperLink($"'{user.playerServer}_{user.playerName}'!A1", $"{user.playerServer}: {user.playerName}");
        }

    }
}
