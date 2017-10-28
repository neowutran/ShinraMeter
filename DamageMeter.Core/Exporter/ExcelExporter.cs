using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;
using DamageMeter.AutoUpdate;
using DamageMeter.TeraDpsApi;
using Data;
using Lang;
using OfficeOpenXml;
using OfficeOpenXml.Drawing.Chart;
using OfficeOpenXml.Style;
using Tera.Game;

namespace DamageMeter
{
    internal static class EPPExstension
    {
        private static readonly List<string> Tol2 = new List<string> {"4477AA", "CC6677"};

        private static readonly List<string> Tol7 = new List<string> {"332288", "88CCEE", "44AA99", "117733", "DDCC77", "CC6677", "AA4499"};

        private static readonly List<string> Tol9 = new List<string> {"332288", "88CCEE", "44AA99", "117733", "999933", "DDCC77", "CC6677", "882255", "AA4499"};

        private static readonly List<string> Tol21 = new List<string>
        {
            "771155",
            "AA4488",
            "CC99BB",
            "114477",
            "4477AA",
            "77AADD",
            "117777",
            "44AAAA",
            "77CCCC",
            "117744",
            "44AA77",
            "88CCAA",
            "777711",
            "AAAA44",
            "DDDD77",
            "774411",
            "AA7744",
            "DDAA77",
            "771122",
            "AA4455",
            "DD7788"
        };

        public static void SetLineChartColors(this ExcelChart chart)
        {
            var i = 0;
            var nsa = chart.WorkSheet.Drawings.NameSpaceManager.LookupNamespace("a");
            var chartXml = chart.ChartXml;
            var nsuri = chartXml.DocumentElement.NamespaceURI;
            var nsm = new XmlNamespaceManager(chartXml.NameTable);
            nsm.AddNamespace("a", nsa);
            nsm.AddNamespace("c", nsuri);
            var series = chart.ChartXml.SelectNodes(@"c:chartSpace/c:chart/c:plotArea/c:lineChart/c:ser", nsm);
            var numLines = series.Count;
            foreach (XmlNode serieNode in series)
            {
                string color;
                if (numLines <= 2)
                {
                    color = Tol2[i]; //personal
                }
                else if (numLines <= 7)
                {
                    color = Tol7[i]; //5-party + boss
                }
                else if (numLines <= 9)
                {
                    color = Tol9[i]; //7-raid  + boss
                }
                else
                {
                    color = Tol21[i % 21]; //if more than 21 lines - reuse old colors, anyway no one can see anithing at the bottom of the chart.
                }

                //var serieNode = chart.ChartXml.SelectSingleNode($@"c:chartSpace/c:chart/c:plotArea/c:lineChart/c:ser[c:idx[@val='{i}']]", nsm);

                //Add reference to the color for the legend
                var srgbClr = chartXml.CreateNode(XmlNodeType.Element, "srgbClr", nsa);
                var att = chartXml.CreateAttribute("val");
                att.Value = color;
                srgbClr.Attributes.Append(att);

                var solidFill = chartXml.CreateNode(XmlNodeType.Element, "solidFill", nsa);
                solidFill.AppendChild(srgbClr);

                var ln = chartXml.CreateNode(XmlNodeType.Element, "ln", nsa);
                ln.AppendChild(solidFill);

                var spPr = chartXml.CreateNode(XmlNodeType.Element, "spPr", nsuri);
                spPr.AppendChild(ln);

                serieNode.AppendChild(spPr);
                ++i;
            }
        }

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
            if (!found) { throw new InvalidOperationException("series not found."); }

            var nsm = chart.WorkSheet.Drawings.NameSpaceManager;
            var nschart = nsm.LookupNamespace("c");
            var nsa = nsm.LookupNamespace("a");
            var node = chart.ChartXml.SelectSingleNode(
                @"c:chartSpace/c:chart/c:plotArea/c:barChart/c:ser[c:idx[@val='" + i.ToString(CultureInfo.InvariantCulture) + "']]", nsm);
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
            child.Attributes.Append(doc.CreateAttribute("val")).Value = (1F - 1.6F / (maxVal + 2)).ToString(CultureInfo.InvariantCulture);
            layout.AppendChild(child);
            node = doc.SelectSingleNode(@"c:chartSpace/c:chart/c:plotArea/c:layout", nsm);
            node.AppendChild(layout);
        }

        public static XmlNode RenameNode(XmlNode node, string namespaceUri, string qualifiedName)
        {
            if (node.NodeType == XmlNodeType.Element)
            {
                var oldElement = (XmlElement) node;
                var newElement = node.OwnerDocument.CreateElement(qualifiedName, namespaceUri);
                while (oldElement.HasAttributes) { newElement.SetAttributeNode(oldElement.RemoveAttributeNode(oldElement.Attributes[0])); }
                while (oldElement.HasChildNodes) { newElement.AppendChild(oldElement.FirstChild); }
                oldElement.ParentNode?.ReplaceChild(newElement, oldElement);
                return newElement;
            }
            return null;
        }

        public static void FixEppPlusBug(this ExcelChart chart)
            //epplus do not renumber series in the chart correctly if inserted not in sequence.
        {
            var xml = chart.ChartXml;
            var nsm = chart.WorkSheet.Drawings.NameSpaceManager;
            var badattr = xml.SelectNodes("//c:idx", nsm);
            var idx = 0;
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

    internal class ExcelExporter
    {
        private static readonly BasicTeraData BTD = BasicTeraData.Instance;
        private static readonly object savelock = new object();
        private static double scale;

        public static void ExcelSave(ExtendedStats exdata, string userName = "", bool manual = false)
        {
            lock (savelock) //can't save 2 excel files at one time
            {
                if (!BTD.WindowData.Excel && !manual) { return; }
                var data = exdata.BaseStats;
                var Boss = exdata.Entity.Info;
                scale = 96 / Graphics.FromImage(new Bitmap(1, 1))
                            .DpiX; //needed if user have not standart DPI setting in control panel, workaround EPPlus autofit bug
                /*
                Select save directory
                */
                var fileName = BTD.WindowData.ExcelPathTemplate.Replace("{Area}", Boss.Area.Replace(":", "-")).Replace("{Boss}", Boss.Name.Replace(":", "-"))
                                   .Replace("{Date}", DateTime.Now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture))
                                   .Replace("{Time}", DateTime.Now.ToString("HH-mm-ss", CultureInfo.InvariantCulture))
                                   .Replace("{User}", string.IsNullOrEmpty(userName) ? "_____" : userName) + ".xlsx";

                var fname = Path.Combine(BTD.WindowData.ExcelSaveDirectory, fileName);


                /*
                Test if you have access to the user choice directory, if not, switch back to the default save directory
                */
                try { Directory.CreateDirectory(Path.GetDirectoryName(fname)); }
                catch
                {
                    fname = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), fileName);
                    Directory.CreateDirectory(Path.GetDirectoryName(fname));
                }

                var file = new FileInfo(fname);
                if (file.Exists) { return; }
                //the only case this can happen is BAM mobtraining, that's not so interesting statistic to deal with more complex file names.
                using (var package = new ExcelPackage(file))
                {
                    var details = CreateDetailsSheet(package, exdata);
                    var ws = package.Workbook.Worksheets.Add(LP.Boss);
                    var linkStyle = package.Workbook.Styles.CreateNamedStyle("HyperLink");
                    //This one is language dependent
                    linkStyle.Style.Font.UnderLine = true;
                    linkStyle.Style.Font.Color.SetColor(Color.Blue);
                    ws.DefaultRowHeight = 30;
                    ws.Cells.Style.Font.Size = 12;
                    ws.Cells.Style.Font.Name = "Arial";
                    ws.Cells[1, 1].Value = $"{Boss.Area}: {Boss.Name} {TimeSpan.FromSeconds(double.Parse(data.fightDuration)).ToString(@"mm\:ss")}";
                    ws.Cells[1, 1, 1, 10].Merge = true;
                    ws.Cells[1, 1, 1, 12].Style.Font.Bold = true;
                    ws.Cells[1, 11].Value = long.Parse(data.partyDps);
                    ws.Cells[1, 11].Style.Numberformat.Format = @"#,#00,\k\/\s";
                    ws.Cells[2, 1].Value = "Ic";
                    ws.Cells[2, 1].Style.Font.Color.SetColor(Color.Transparent);
                    ws.Cells[2, 2].Value = LP.Name;
                    ws.Cells[2, 3].Value = LP.Deaths;
                    ws.Cells[2, 4].Value = LP.Death_Time;
                    ws.Cells[2, 5].Value = LP.DamagePercent;
                    ws.Cells[2, 6].Value = LP.CritPercent;
                    ws.Cells[2, 7].Value = LP.CritDamagePercent;
                    ws.Cells[2, 8].Value = LP.HealCritPercent;
                    ws.Cells[2, 9].Value = LP.HitsTaken;
                    ws.Cells[2, 10].Value = LP.DamgeTaken;
                    ws.Cells[2, 11].Value = LP.Dps;
                    ws.Cells[2, 12].Value = LP.Damage;
                    var i = 2;
                    foreach (var user in data.members.Where(x=>x.playerTotalDamage!="0").OrderByDescending(x => long.Parse(x.playerTotalDamage)))
                    {
                        i++;
                        ws.Cells[i, 1].Value = i - 2;
                        AddImage(ws, i, 1, ClassIcons.Instance.GetBitmap((PlayerClass) Enum.Parse(typeof(PlayerClass), user.playerClass)));
                        ws.Cells[i, 2].Value = $"{user.playerServer}: {user.playerName}";
                        ws.Cells[i, 2].Hyperlink = CreateUserSheet(package.Workbook, user, exdata, details);
                        ws.Cells[i, 3].Value = long.Parse(user.playerDeaths);
                        ws.Cells[i, 4].Value = long.Parse(user.playerDeathDuration);
                        ws.Cells[i, 4].Style.Numberformat.Format = @"0\s";
                        ws.Cells[i, 5].Value = double.Parse(user.playerTotalDamagePercentage) / 100;
                        ws.Cells[i, 5].Style.Numberformat.Format = "0.0%";
                        ws.Cells[i, 6].Value = double.Parse(user.playerAverageCritRate) / 100;
                        ws.Cells[i, 6].Style.Numberformat.Format = "0.0%";
                        ws.Cells[i, 7].Value = exdata.PlayerCritDamageRate[user.playerName] / 100;
                        ws.Cells[i, 7].Style.Numberformat.Format = "0.0%";
                        ws.Cells[i, 8].Value = string.IsNullOrEmpty(user.healCrit) ? 0 : double.Parse(user.healCrit) / 100;
                        ws.Cells[i, 8].Style.Numberformat.Format = "0.0%";
                        ws.Cells[i, 9].Value = exdata.PlayerReceived[user.playerName].Item1;
                        ws.Cells[i, 10].Value = exdata.PlayerReceived[user.playerName].Item2;
                        ws.Cells[i, 10].Style.Numberformat.Format = @"#,#0,\k";
                        ws.Cells[i, 11].Value = long.Parse(user.playerDps);
                        ws.Cells[i, 11].Style.Numberformat.Format = @"#,#0,\k\/\s";
                        ws.Cells[i, 12].Value = long.Parse(user.playerTotalDamage);
                        ws.Cells[i, 12].Style.Numberformat.Format = @"#,#0,\k";
                    }
                    ws.Cells[1, 12].Formula = $"SUM(L3:L{i})";
                    ws.Cells[1, 12].Style.Numberformat.Format = @"#,#0,\k";
                    var border = ws.Cells[1, 1, i, 12].Style.Border;
                    border.Bottom.Style = border.Top.Style = border.Left.Style = border.Right.Style = ExcelBorderStyle.Thick;
                    ws.Cells[2, 1, i, 12].AutoFilter = true;

                    var j = i + 3;
                    ws.Cells[j, 1].Value = "Ic";
                    ws.Cells[j, 1].Style.Font.Color.SetColor(Color.Transparent);
                    ws.Cells[j, 2].Value = LP.Name;
                    ws.Cells[j, 2, j, 11].Merge = true;
                    ws.Cells[j, 12].Value = "%";
                    ws.Cells[j, 2, j, 12].Style.Font.Bold = true;
                    foreach (var buf in data.debuffDetail)
                    {
                        j++;
                        var hotdot = BTD.HotDotDatabase.Get((int)buf[0]);
                        ws.Cells[j, 1].Value = j - i - 3;
                        AddImage(ws, j, 1, BTD.Icons.GetBitmap(hotdot.IconName));
                        ws.Cells[j, 2].Value = hotdot.Name;
                        if (!string.IsNullOrEmpty(hotdot.Tooltip)) { ws.Cells[j, 2].AddComment("" + hotdot.Tooltip, "info"); }
                        ws.Cells[j, 2, j, 11].Merge = true;
                        ws.Cells[j, 12].Value = (double) ((List<List<int>>)buf[1])[0][1] / 100;
                        ws.Cells[j, 12].Style.Numberformat.Format = "0%";
                    }
                    border = ws.Cells[i + 3, 1, j, 12].Style.Border;
                    border.Bottom.Style = border.Top.Style = border.Left.Style = border.Right.Style = ExcelBorderStyle.Thick;

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
                    ws.Column(11).AutoFit();
                    ws.Column(12).Width = 20;
                    ws.Column(2).Width = GetTrueColumnWidth(ws.Column(2).Width * scale);
                    ws.Column(3).Width = GetTrueColumnWidth(ws.Column(3).Width * scale);
                    ws.Column(4).Width = GetTrueColumnWidth(ws.Column(4).Width * scale);
                    ws.Column(5).Width = GetTrueColumnWidth(ws.Column(5).Width * scale);
                    ws.Column(6).Width = GetTrueColumnWidth(ws.Column(6).Width * scale);
                    ws.Column(7).Width = GetTrueColumnWidth(ws.Column(7).Width * scale);
                    ws.Column(8).Width = GetTrueColumnWidth(ws.Column(8).Width * scale);
                    ws.Column(9).Width = GetTrueColumnWidth(ws.Column(9).Width * scale);
                    ws.Column(10).Width = GetTrueColumnWidth(ws.Column(10).Width * scale);
                    ws.Column(11).Width = GetTrueColumnWidth(ws.Column(11).Width * scale);
                    ws.Cells[1, 1, j, 12].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    ws.Cells[1, 1, j, 12].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ws.PrinterSettings.FitToPage = true;

                    AddCharts(ws, exdata, details, j, LP.Boss);

                    // I don't know why, but sometimes column height setting is lost.
                    for (var x = 1; x <= j; ++x)
                    {
                        ws.Row(x).CustomHeight = true;
                        ws.Row(x).Height = 30;
                    }

                    ws.View.TabSelected = true;
                    details.Hidden = eWorkSheetHidden.Hidden;
                    package.Workbook.Properties.Title = Boss.Name;
                    package.Workbook.Properties.Author = "ShinraMeter " + UpdateManager.Version;
                    package.Workbook.Properties.Company = "github.com/neowutran & github.com/Gl0";
                    package.Save();
                }
            }
        }

        private static void AddCharts(ExcelWorksheet ws, ExtendedStats exdata, ExcelWorksheet details, int startrow, string name)
        {
            var time = (int) (exdata.LastTick / TimeSpan.TicksPerSecond - exdata.FirstTick / TimeSpan.TicksPerSecond);
            var offset = exdata.PlayerBuffs.Keys.ToList().IndexOf(name) + 1;
            var bossSheet = name == LP.Boss;
            if (!bossSheet && offset <= 0)
            {
                return; //no buff data for user -> no graphs.
            }
            offset = bossSheet ? 3 : 4 + offset * 7;
            var dps = ws.Drawings.AddChart(name + LP.Dps, eChartType.Line);
            dps.SetPosition(startrow + 1, 5, 0, 5);
            dps.SetSize(1200, 300);
            dps.Legend.Position = eLegendPosition.Top;
            if (time > 40) { dps.XAxis.MajorUnit = time / 20; }
            ExcelChart typeDmg;
            ExcelChartSerie serieDmg;
            if (!bossSheet)
            {
                typeDmg = dps.PlotArea.ChartTypes[0];
                typeDmg.YAxis.Title.Text = BasicTeraData.Instance.WindowData.ExcelCMADPSSeconds + LP.CMADPS;
                typeDmg.YAxis.Title.Rotation = 90;

                serieDmg = typeDmg.Series.Add(details.Cells[3, offset + 5, time + 3, offset + 5], details.Cells[3, 2, time + 3, 2]);
                serieDmg.Header = name + " " + BasicTeraData.Instance.WindowData.ExcelCMADPSSeconds + LP.CMADPS;
            }
            else
            {
                typeDmg = dps.PlotArea.ChartTypes[0];
                typeDmg.YAxis.Title.Text = LP.BossHP;
                typeDmg.YAxis.Title.Rotation = 90;
                typeDmg.YAxis.MaxValue = 1;
                serieDmg = typeDmg.Series.Add(details.Cells[3, offset + 7, time + 3, offset + 7], details.Cells[3, 2, time + 3, 2]);
                serieDmg.Header = LP.BossHP + " %";
            }
            typeDmg.YAxis.MinValue = 0;
            var typeDps = dps.PlotArea.ChartTypes.Add(eChartType.Line);
            typeDps.UseSecondaryAxis = true;
            typeDps.YAxis.Title.Text = LP.AvgDPS;
            typeDps.YAxis.Title.Rotation = 90;
            typeDps.YAxis.SourceLinked = false;
            typeDps.YAxis.Format = @"#,#0\k\/\s"; //not sure why, but it loss sourcelink itself if we show only dps.
            var serieDps = typeDps.Series.Add(details.Cells[3, offset + 6, time + 3, offset + 6], details.Cells[3, 2, time + 3, 2]);
            serieDps.Header = name + " " + LP.AvgDPS;
            if (bossSheet)
            {
                typeDps.YAxis.MaxValue = details.Cells[3, offset + 6, time + 3, offset + 6].Max(x => (long) x.Value);
                typeDps.YAxis.MinValue = 0;
                var col = 4;
                foreach (var user in exdata.PlayerBuffs)
                {
                    col += 7;
                    //var userDmg = typeDmg.Series.Add(details.Cells[3, col + 5, time + 3, col + 5], details.Cells[3, 2, time + 3, 2]);
                    //userDmg.Header = user.Key + " Dmg";
                    var userDps = typeDps.Series.Add(details.Cells[3, col + 6, time + 3, col + 6], details.Cells[3, 2, time + 3, 2]);
                    userDps.Header = user.Key + " " + LP.AvgDPS;
                }
            }
            //dps.FixEppPlusBug();// needed only if adding users dmg to main boss chart
            dps.SetLineChartColors();

            var numInt = bossSheet
                ? exdata.Debuffs.Sum(x => x.Value.Count()) - 1
                : exdata.PlayerBuffs[name].Times.Sum(x => x.Value.Count()) + exdata.PlayerBuffs[name].Death.Count() +
                  exdata.PlayerBuffs[name].Aggro(exdata.Entity).Count() - 1;
            var numBuff = bossSheet
                ? exdata.Debuffs.Count
                : exdata.PlayerBuffs[name].Times.Count(x => x.Value.Count() > 0) + (exdata.PlayerBuffs[name].Death.Count() > 0 ? 1 : 0) +
                  (exdata.PlayerBuffs[name].Aggro(exdata.Entity).Count() > 0 ? 1 : 0);
            if (numInt >= 0 && numBuff > 0)
            {
                var buff = ws.Drawings.AddChart(name + LP.Buff, eChartType.BarStacked);
                var typeBuff = buff.PlotArea.ChartTypes[0];
                buff.SetPosition(startrow + 9, 5, 0, 5);
                buff.SetSize(1200, numBuff * 25 + 38);
                buff.Legend.Remove();
                var serieStart = typeBuff.Series.Add(details.Cells[3, offset + 1, numInt + 3, offset + 1], details.Cells[3, offset, numInt + 3, offset]);
                serieStart.Header = LP.Start;
                (buff as ExcelBarChart).InvisibleSerie(serieStart);
                var serieTime = typeBuff.Series.Add(details.Cells[3, offset + 2, numInt + 3, offset + 2], details.Cells[3, offset, numInt + 3, offset]);
                serieTime.Header = LP.Time;
                typeBuff.YAxis.MajorUnit = time >= 40 ? (double) (time / 20) / 86400F : 1F / 86400F;
                typeBuff.YAxis.MinValue = 0F;
                typeBuff.YAxis.MaxValue = (double) time / 86400F;
                typeBuff.XAxis.Orientation = eAxisOrientation.MaxMin;
                typeBuff.XAxis.MinorTickMark = eAxisTickMark.None;
                typeBuff.YAxis.Crosses = eCrosses.Max;
                var typeAxis = buff.PlotArea.ChartTypes.Add(eChartType.BarStacked);
                typeAxis.UseSecondaryAxis = true;
                var serieAxis = typeAxis.Series.Add(details.Cells[3, offset + 4, numBuff - 1 + 3, offset + 4],
                    details.Cells[3, offset + 3, numBuff - 1 + 3, offset + 3]);
                serieAxis.Header = LP.Name;
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
            var timeFormat = (exdata.LastTick - exdata.FirstTick) / TimeSpan.TicksPerSecond < 40 ? "ss" : "mm:ss";
            var details = package.Workbook.Worksheets.Add("Details");
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
            details.Cells[2, 8].Value = "SMADPS";
            details.Column(8).Style.Numberformat.Format = @"#,#0\k\/\s";
            details.Cells[2, 9].Value = "AvgDPS";
            details.Column(9).Style.Numberformat.Format = @"#,#0\k\/\s";
            details.Cells[2, 10].Value = "BossHP";
            details.Column(10).Style.Numberformat.Format = "0%";
            details.Cells[1, 1, 1, 10].Merge = true;

            for (var t = 0; t <= exdata.LastTick / TimeSpan.TicksPerSecond - exdata.FirstTick / TimeSpan.TicksPerSecond; t++)
            {
                details.Cells[t + 3, 1].Value = t;
                details.Cells[t + 3, 2].Value = (double) t / 86400;
            }
            var buffnum = 0;
            var j = 0;
            foreach (var buffPair in exdata.Debuffs)
            {
                if (buffPair.Value.Count() == 0) { continue; }
                buffnum++;
                details.Cells[2 + buffnum, 6].Value = buffPair.Key.Name;
                details.Cells[2 + buffnum, 7].Value = 0;
                foreach (var buff in buffPair.Value.AllDurations())
                {
                    j++;
                    details.Cells[2 + j, 3].Value = buffnum;
                    details.Cells[2 + j, 4].Value = (double) (buff.Begin - exdata.FirstTick) / TimeSpan.TicksPerDay;
                    details.Cells[2 + j, 5].Value = (double) (buff.End - buff.Begin) / TimeSpan.TicksPerDay;
                }
            }
            long dealtDamage = 0;
            var hp = exdata.Entity.Info.HP;
            var totalDamage = exdata.PlayerSkills.Sum(x => x.Value.Where(time => time.Time >= exdata.FirstTick && time.Time <= exdata.LastTick).Sum(y => y.Amount));
            if (totalDamage < hp) { totalDamage = hp; }
            j = 0;
            var xCMA = BasicTeraData.Instance.WindowData.ExcelCMADPSSeconds <= 0 ? 1 : BasicTeraData.Instance.WindowData.ExcelCMADPSSeconds;
            var last = new Queue<long>();
            for (var curTick = exdata.FirstTick; curTick <= exdata.LastTick; curTick += TimeSpan.TicksPerSecond)
            {
                j++;
                var damage = exdata.PlayerSkills.Sum(x => x.Value.Where(time => time.Time >= curTick && time.Time <= curTick + TimeSpan.TicksPerSecond)
                    .Sum(skill => skill.Amount));
                dealtDamage += damage;
                last.Enqueue(damage);
                if (last.Count > xCMA) { last.Dequeue(); }
                if (curTick >= exdata.LastTick - TimeSpan.TicksPerSecond)
                {
                    if (j > xCMA)
                    {
                        details.Cells[j + 2 - xCMA / 2, 8].Value = last.ToArray().Sum(x => x) /
                                                                   (xCMA > 1 ? xCMA - 1 + TimeSpan.TicksPerSecond / (exdata.LastTick - curTick) : 1) / 1000;
                    }
                    details.Cells[j + 2, 9].Value = dealtDamage * TimeSpan.TicksPerSecond / (exdata.LastTick - exdata.FirstTick) / 1000;
                }
                else
                {
                    if (j >= xCMA) { details.Cells[j + 2 - xCMA / 2, 8].Value = last.ToArray().Sum(x => x) / xCMA / 1000; }
                    if (j != 1) { details.Cells[j + 2, 9].Value = dealtDamage / (j - 1) / 1000; }
                }
                details.Cells[j + 2, 10].Value = totalDamage == 0 ? 0 : (double) (totalDamage - dealtDamage) / totalDamage;
            }
            var i = 4;
            foreach (var user in exdata.PlayerBuffs)
            {
                i += 7;
                details.Cells[1, i].Value = user.Key;
                details.Cells[2, i].Value = "BuffNum";
                details.Cells[2, i + 1].Value = "Start";
                details.Column(i + 1).Style.Numberformat.Format = timeFormat;
                details.Cells[2, i + 2].Value = "Duration";
                details.Column(i + 2).Style.Numberformat.Format = timeFormat;
                details.Cells[2, i + 3].Value = "BuffName";
                details.Cells[2, i + 4].Value = "Axis";
                details.Cells[2, i + 5].Value = "SMADPS";
                details.Column(i + 5).Style.Numberformat.Format = @"#,#0\k\/\s";
                details.Cells[2, i + 6].Value = "AvgDPS";
                details.Column(i + 6).Style.Numberformat.Format = @"#,#0\k\/\s";
                details.Cells[1, i, 1, i + 6].Merge = true;
                buffnum = 0;
                j = 0;
                foreach (var buffPair in user.Value.Times)
                {
                    if (buffPair.Value.Count() == 0) { continue; }
                    buffnum++;
                    details.Cells[2 + buffnum, i + 3].Value = buffPair.Key.Name;
                    details.Cells[2 + buffnum, i + 4].Value = 0;
                    foreach (var buff in buffPair.Value.AllDurations())
                    {
                        j++;
                        details.Cells[2 + j, i].Value = buffnum;
                        details.Cells[2 + j, i + 1].Value = (double) (buff.Begin - exdata.FirstTick) / TimeSpan.TicksPerDay;
                        details.Cells[2 + j, i + 2].Value = (double) (buff.End - buff.Begin) / TimeSpan.TicksPerDay;
                    }
                }
                if (user.Value.Death.Count() > 0)
                {
                    buffnum++;
                    details.Cells[2 + buffnum, i + 3].Value = LP.Deaths;
                    details.Cells[2 + buffnum, i + 4].Value = 0;
                    foreach (var buff in user.Value.Death.AllDurations())
                    {
                        j++;
                        details.Cells[2 + j, i].Value = buffnum;
                        details.Cells[2 + j, i + 1].Value = (double) (buff.Begin - exdata.FirstTick) / TimeSpan.TicksPerDay;
                        details.Cells[2 + j, i + 2].Value = (double) (buff.End - buff.Begin) / TimeSpan.TicksPerDay;
                    }
                }
                if (user.Value.Aggro(exdata.Entity).Count() > 0)
                {
                    buffnum++;
                    details.Cells[2 + buffnum, i + 3].Value = LP.Aggro;
                    details.Cells[2 + buffnum, i + 4].Value = 0;
                    foreach (var buff in user.Value.Aggro(exdata.Entity).AllDurations())
                    {
                        j++;
                        details.Cells[2 + j, i].Value = buffnum;
                        details.Cells[2 + j, i + 1].Value = (double) (buff.Begin - exdata.FirstTick) / TimeSpan.TicksPerDay;
                        details.Cells[2 + j, i + 2].Value = (double) (buff.End - buff.Begin) / TimeSpan.TicksPerDay;
                    }
                }
                dealtDamage = 0;
                j = 0;
                last = new Queue<long>();
                for (var curTick = exdata.FirstTick; curTick <= exdata.LastTick; curTick += TimeSpan.TicksPerSecond)
                {
                    j++;
                    var damage = exdata.PlayerSkills.Where(all => all.Key == user.Key).Sum(x => x.Value
                        .Where(time => time.Time >= curTick && time.Time <= curTick + TimeSpan.TicksPerSecond).Sum(skill => skill.Amount));
                    dealtDamage += damage;
                    last.Enqueue(damage);
                    if (last.Count > xCMA) { last.Dequeue(); }
                    if (curTick >= exdata.LastTick - TimeSpan.TicksPerSecond)
                    {
                        if (j > xCMA)
                        {
                            details.Cells[j + 2 - xCMA / 2, i + 5].Value = last.ToArray().Sum(x => x) /
                                                                           (xCMA > 1 ? xCMA - 1 + TimeSpan.TicksPerSecond / (exdata.LastTick - curTick) : 1) / 1000;
                        }
                        details.Cells[j + 2, i + 6].Value = dealtDamage * TimeSpan.TicksPerSecond / (exdata.LastTick - exdata.FirstTick) / 1000;
                    }
                    else
                    {
                        if (j >= xCMA) { details.Cells[j + 2 - xCMA / 2, i + 5].Value = last.ToArray().Sum(x => x) / xCMA / 1000; }
                        if (j != 1) { details.Cells[j + 2, i + 6].Value = dealtDamage / (j - 1) / 1000; }
                    }
                }
            }

            return details;
        }

        private static double GetTrueColumnWidth(double width)
        {
            //DEDUCE WHAT THE COLUMN WIDTH WOULD REALLY GET SET TO
            var z = width >= 1 + 2F / 3
                ? Math.Round((Math.Round(7 * (width - 1F / 256), 0) - 5) / 7, 2)
                : Math.Round((Math.Round(12 * (width - 1F / 256), 0) - Math.Round(5 * width, 0)) / 12, 2);

            //HOW FAR OFF? (WILL BE LESS THAN 1)
            var errorAmt = width - z;

            //CALCULATE WHAT AMOUNT TO TACK ONTO THE ORIGINAL AMOUNT TO RESULT IN THE CLOSEST POSSIBLE SETTING 
            double adj;
            if (width >= 1 + 2 / 3) { adj = Math.Round(7 * errorAmt - 7F / 256, 0) / 7; }
            else { adj = Math.Round(12 * errorAmt - 12F / 256, 0) / 12 + 2F / 12; }

            //RETURN A SCALED-VALUE THAT SHOULD RESULT IN THE NEAREST POSSIBLE VALUE TO THE TRUE DESIRED SETTING
            if (z > 0) { return width + adj; }

            return 0d;
        }

        private static void AddImage(ExcelWorksheet ws, int rowIndex, int columnIndex, Bitmap image)
        {
            //How to Add a Image using EP Plus
            if (image == null) { return; }
            var picture = ws.Drawings.AddPicture("pic" + rowIndex + columnIndex, image);
            picture.From.Column = columnIndex - 1;
            picture.From.Row = rowIndex - 1;
            picture.From.ColumnOff = 12000;
            picture.From.RowOff = 12000;
            picture.SetSize(38, 38);
        }

        private static ExcelHyperLink CreateUserSheet(ExcelWorkbook wb, Members user, ExtendedStats exdata, ExcelWorksheet details)
        {
            var name = $"{user.playerServer}_{user.playerName}";
            var ws = wb.Worksheets.Add(name);
            ws.DefaultRowHeight = 30;
            ws.Cells.Style.Font.Size = 12;
            ws.Cells.Style.Font.Name = "Arial";
            AddImage(ws, 1, 1, ClassIcons.Instance.GetBitmap((PlayerClass) Enum.Parse(typeof(PlayerClass), user.playerClass)));
            ws.Cells[1, 2].Value = $"{user.playerServer}: {user.playerName}";
            ws.Cells[1, 2, 1, 11].Merge = true;
            ws.Cells[1, 2, 1, 11].Style.Font.Bold = true;
            ws.Cells[2, 2].Value = LP.SkillName;
            ws.Cells[2, 3].Value = LP.DamagePercent;
            ws.Cells[2, 4].Value = LP.Damage;
            ws.Cells[2, 5].Value = LP.CritPercent;
            ws.Cells[2, 6].Value = LP.Hits;
            ws.Cells[2, 7].Value = LP.Crits;
            ws.Cells[2, 8].Value = LP.MaxCrit;
            ws.Cells[2, 9].Value = LP.MinCrit;
            ws.Cells[2, 10].Value = LP.AverageCrit;
            ws.Cells[2, 11].Value = LP.AvgWhite;
            var i = 2;

            if(user.playerTotalDamage!="0")
                foreach (var stat in exdata.PlayerSkillsAggregated[user.playerServer + "/" + user.playerName].OrderByDescending(x => x.Amount()))
                {
                    i++;
                    ws.Cells[i, 1].Value = i - 2;
                    foreach (var skillInfo in stat.Skills)
                    {
                        if (string.IsNullOrEmpty(skillInfo.Key.IconName)) { continue; }
                        AddImage(ws, i, 1, BTD.Icons.GetBitmap(skillInfo.Key.IconName));
                        break;
                    }

                    ws.Cells[i, 2].Value = stat.Name;
                    ws.Cells[i, 3].Value = stat.DamagePercent() / 100;
                    ws.Cells[i, 3].Style.Numberformat.Format = "0.0%";
                    ws.Cells[i, 4].Value = stat.Amount();
                    ws.Cells[i, 4].Style.Numberformat.Format = @"#,#0,\k";
                    ws.Cells[i, 5].Value = stat.CritRate() / 100;
                    ws.Cells[i, 5].Style.Numberformat.Format = "0.0%";
                    ws.Cells[i, 6].Value = stat.Hits();
                    ws.Cells[i, 7].Value = stat.Crits();
                    ws.Cells[i, 8].Value = stat.BiggestCrit();
                    ws.Cells[i, 8].Style.Numberformat.Format = @"#,#0,\k";
                    ws.Cells[i, 9].Value = stat.LowestCrit();
                    ws.Cells[i, 9].Style.Numberformat.Format = @"#,#0,\k";
                    ws.Cells[i, 10].Value = stat.AvgCrit();
                    ws.Cells[i, 10].Style.Numberformat.Format = @"#,#0,\k";
                    ws.Cells[i, 11].Value = stat.AvgWhite();
                    ws.Cells[i, 11].Style.Numberformat.Format = @"#,#0,\k";
                }
            var border = ws.Cells[1, 1, i, 11].Style.Border;
            border.Bottom.Style = border.Top.Style = border.Left.Style = border.Right.Style = ExcelBorderStyle.Thick;
            ws.Cells[2, 1, i, 11].AutoFilter = true;

            var j = i + 3;
            ws.Cells[j, 2].Value = LP.Name;
            ws.Cells[j, 2, j, 10].Merge = true;
            ws.Cells[j, 11].Value = "%";
            ws.Cells[j, 2, j, 11].Style.Font.Bold = true;
            foreach (var buf in user.buffDetail)
            {
                j++;
                var hotdot = BTD.HotDotDatabase.Get((int)buf[0]);
                ws.Cells[j, 1].Value = j - i - 3;
                AddImage(ws, j, 1, BTD.Icons.GetBitmap(hotdot.IconName));
                ws.Cells[j, 2].Value = hotdot.Name;
                if (!string.IsNullOrEmpty(hotdot.Tooltip)) { ws.Cells[j, 2].AddComment("" + hotdot.Tooltip, "info"); }
                ws.Cells[j, 2, j, 10].Merge = true;
                ws.Cells[j, 11].Value = (double)((List<List<int>>)buf[1])[0][1] / 100;
                ws.Cells[j, 11].Style.Numberformat.Format = "0%";
                if (!string.IsNullOrEmpty(hotdot.ItemName)) { ws.Cells[j, 10].AddComment("" + hotdot.ItemName, "info"); }
            }
            border = ws.Cells[i + 3, 1, j, 11].Style.Border;
            border.Bottom.Style = border.Top.Style = border.Left.Style = border.Right.Style = ExcelBorderStyle.Thick;

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
            ws.Column(11).AutoFit();
            ws.Column(2).Width = GetTrueColumnWidth(ws.Column(2).Width * scale);
            ws.Column(3).Width = GetTrueColumnWidth(ws.Column(3).Width * scale);
            ws.Column(4).Width = GetTrueColumnWidth(ws.Column(4).Width * scale);
            ws.Column(5).Width = GetTrueColumnWidth(ws.Column(5).Width * scale);
            ws.Column(6).Width = GetTrueColumnWidth(ws.Column(6).Width * scale);
            ws.Column(7).Width = GetTrueColumnWidth(ws.Column(7).Width * scale);
            ws.Column(8).Width = GetTrueColumnWidth(ws.Column(8).Width * scale);
            ws.Column(9).Width = GetTrueColumnWidth(ws.Column(9).Width * scale);
            ws.Column(10).Width = GetTrueColumnWidth(ws.Column(10).Width * scale);
            ws.Column(11).Width = GetTrueColumnWidth(ws.Column(11).Width * scale);
            ws.Cells[1, 1, j, 11].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            ws.Cells[1, 1, j, 11].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            ws.PrinterSettings.FitToPage = true;

            AddCharts(ws, exdata, details, j, name);

            // I don't know why, but sometimes column height setting is lost.
            for (var x = 1; x <= j; ++x)
            {
                ws.Row(x).CustomHeight = true;
                ws.Row(x).Height = 30;
            }

            // If sheet name contains space character, name should be enclosed in single quotes.
            return new ExcelHyperLink($"'{user.playerServer}_{user.playerName}'!A1", $"{user.playerServer}: {user.playerName}");
        }
    }
}