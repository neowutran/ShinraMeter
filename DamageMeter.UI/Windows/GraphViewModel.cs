using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Media;
using DamageMeter.Database.Structures;
using Data;
using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
using Tera.Game;
using Color = System.Windows.Media.Color;
using Point = System.Windows.Point;

namespace DamageMeter.UI
{
    public class GraphViewModel : TSPropertyChanged
    {
        private class DpsSource
        {
            // fields
            private long _prevAmount;
            private double _damageSum;
            private long _interval = TimeSpan.TicksPerSecond * Samples;

            // properties
            public ulong Id { get; }
            public Queue<Tuple<double, long>> Values { get; }
            public double Avg => Values.Count == 0 ? _damageSum : _damageSum * TimeSpan.TicksPerSecond / _interval;

            public double Dps { get; private set; }
            // ctor
            public DpsSource(ulong id)
            {
                Id = id;
                Values = new Queue<Tuple<double, long>>();
            }

            // methods
            public void Update(PlayerDamageDealt newValue, EntityInformation entityInfo)
            {
                switch (BasicTeraData.Instance.WindowData.GraphMode)
                {
                    case GraphMode.CMA:
                        var newAmount = newValue.Amount;
                        // get the amount of damage done between this and previous time instants
                        var dmgDiff = Convert.ToDouble(newAmount - _prevAmount);

                        // update values
                        var now = DateTime.Now.Ticks;
                        _damageSum += dmgDiff;

                        // queue damage and time deltas
                        Values.Enqueue(new Tuple<double, long>(dmgDiff, now));

                        _prevAmount = newAmount;

                        if (Values.Count < Samples) return;

                        // remove first value pair in the queue and subtract it from total damage and total time
                        while (Values.Peek().Item2 < now - _interval)
                        {
                            var val = Values.Dequeue();
                            _damageSum -= val.Item1;
                        }
                        break;
                    case GraphMode.DPS:
                        Dps = entityInfo.Interval == 0 ? newValue.Amount : newValue.Amount * TimeSpan.TicksPerSecond / entityInfo.Interval;
                        break;
                }
            }
        }

        // fields
        private readonly List<DpsSource> Sources;
        private int _currSample = 0;
        private int _values = 0;
        private bool _onlyMe;
        private long _currTime = -1;
        private ulong _currEntity = 0;
        private bool _enraged;
        private bool _chartVisibility;
        internal static int Samples => BasicTeraData.Instance.WindowData.RealtimeGraphCMAseconds;
        internal static int ShowedSamples => BasicTeraData.Instance.WindowData.RealtimeGraphDisplayedInterval == 0
                                           ? int.MaxValue
                                           : BasicTeraData.Instance.WindowData.RealtimeGraphDisplayedInterval;
        private bool _notAnimated;

        public bool NotAnimated
        {
            get => _notAnimated;
            set
            {
                if (_notAnimated == value) return;
                _notAnimated = value;
                NotifyPropertyChanged();
            }
        }


        // properties
        //private StepLineSeries Enrage { get; set; }
        public Func<double, string> DpsFormatter { get; set; }
        public Func<double, string> TimeFormatter { get; set; }
        public SeriesCollection Series { get; set; }
        //public SeriesCollection EnrageSeries { get; set; }
        public SectionsCollection EnrageSections { get; }

        public bool ChartVisibility
        {
            get => _chartVisibility;
            set
            {
                if (_chartVisibility == value) return;
                _chartVisibility = value;
                NotifyPropertyChanged();
            }
        }

        private AxisSection CreateSection()
        {
            return new AxisSection
            {
                StrokeThickness = 0,
                Fill = new LinearGradientBrush
                {
                    Opacity = .3,
                    StartPoint = new Point(0, 0),
                    EndPoint = new Point(0, 1),
                    GradientStops = new GradientStopCollection
                    {
                        new GradientStop(Color.FromArgb(0xff, 0xff, 0x33, 0x33), 0),
                        new GradientStop(Color.FromArgb(0x00, 0xff, 0x33, 0x33), 1),
                    }
                },
                Value = _currTime / (double)TimeSpan.TicksPerSecond,
                SectionWidth = 1
            };
        }
        private bool Enraged
        {
            set
            {
                if (_enraged != value && value)
                {
                    // just enraged, add new
                    EnrageSections.Add(CreateSection());
                }
                else if (value)
                {
                    // enrage still going
                    if (EnrageSections.Count > 0)
                    {
                        // update duration
                        var last = EnrageSections.Last();
                        last.SectionWidth = _currTime / (double)TimeSpan.TicksPerSecond - last.Value;
                    }
                    else
                    {
                        // add new
                        EnrageSections.Add(CreateSection());
                    }
                }
                _enraged = value;
            }
        }

        // ctor
        internal GraphViewModel()
        {
            Series = new SeriesCollection();
            DpsFormatter = (v) => FormatHelpers.Instance.FormatValue(Convert.ToInt64(v));
            TimeFormatter = (v) => FormatHelpers.Instance.FormatTimeSpan(TimeSpan.FromSeconds(v));
            Sources = new List<DpsSource>();
            EnrageSections = new SectionsCollection();
        }

        // methods
        Stopwatch sw = new Stopwatch();
        internal void Update(UiUpdateMessage message)
        {
            sw.Stop();
            Debug.WriteLine(sw.ElapsedMilliseconds);

            sw.Restart();
            NotAnimated = !BasicTeraData.Instance.WindowData.RealtimeGraphAnimated;
            // skip update if timer is the same
            if (_currTime == message.StatsSummary.EntityInformation.Interval)
            {
                ChartVisibility = Series.Count > 0 && _currSample != 0;
                return;
            }
            _currSample++;

            CheckReset(message);

            // store the amount of samples we received until now
            if (_values >= ShowedSamples)
                _values = ShowedSamples;
            else _values++;

            // store current enemy
            _currEntity = message.StatsSummary.EntityInformation.Entity != null ? message.StatsSummary.EntityInformation.Entity.Id.Id : 0;

            // show only current player if there are more than 5 players
            var onlyMeChanged = false;
            if (!_onlyMe && message.StatsSummary.PlayerDamageDealt.Count > 35)
            {
                onlyMeChanged = true;
                _onlyMe = true;
            }

            // adds enrage stat
            if (BasicTeraData.Instance.HotDotDatabase?.Get((int)HotDotDatabase.StaticallyUsedBuff.Enraged) == null)
                Enraged = false;
            else
            {
                var currTime = message.StatsSummary.EntityInformation.EndTime;
                message.Abnormals.Get(message.StatsSummary.EntityInformation.Entity)
                                 .TryGetValue(BasicTeraData.Instance.HotDotDatabase?
                                 .Get((int)HotDotDatabase.StaticallyUsedBuff.Enraged), out var enrageHotDot);
                var lastEnd = enrageHotDot?.LastEnd() ?? long.MaxValue;
                Enraged = lastEnd == currTime;
            }

            // iterate player stats and update line series
            foreach (var playerDamage in message.StatsSummary.PlayerDamageDealt)
            {
                var playerId = playerDamage.Source.User.Id;
                var name = playerDamage.Source.User.Name;
                var pClass = playerDamage.Source.Class;
                var isMe = playerId.Id == PacketProcessor.Instance.PlayerTracker.Me().User.Id.Id;

                // skip or remove this player if we have too many entries
                if (_onlyMe && !isMe)
                {
                    if (onlyMeChanged)
                    {
                        var rem = Series.FirstOrDefault(x => x.Title == name);
                        if (rem != null) Series.Remove(rem);
                        var srcRem = Sources.FirstOrDefault(x => x.Id == playerId.Id);
                        if (srcRem != null) Sources.Remove(srcRem);
                    }
                    continue;
                }

                // add this player to sources if it's not already there
                var src = Sources.FirstOrDefault(x => x.Id == playerId.Id);
                if (src == null)
                {
                    src = new DpsSource(playerId.Id);
                    Sources.Add(src);
                }
                // update it
                src.Update(playerDamage, message.StatsSummary.EntityInformation);

                // check if we already have a line series for this player
                if (Series.FirstOrDefault(x => x.Title == name) is LineSeries existing)
                {
                    // series exists, add and remove points

                    // get player line series
                    var seriesVals = existing.Values as ChartValues<ObservablePoint>;
                    // remove first point if total sample count is above the maximum
                    //if (_values >= ShowedSamples) seriesVals.RemoveAt(0);

                    // get average for current instant and add it as a line point
                    double val = 0;
                    switch (BasicTeraData.Instance.WindowData.GraphMode)
                    {
                        case GraphMode.CMA:
                            val = src.Avg;
                            break;
                        case GraphMode.DPS:
                            val = src.Dps;
                            break;
                    }

                    seriesVals.Add(new ObservablePoint(_currTime / (double)TimeSpan.TicksPerSecond, val));
                    // remove first point if the series is longer than it should be (shouldn't happen anyway)
                    while (seriesVals.Count >= _values) seriesVals.RemoveAt(0);
                }
                else
                {
                    // series doesen't exist, create it

                    // get line color (red = dps, blue = tank, green = healer, orange = currPlayer)
                    var color = GetColor(pClass);
                    if (isMe) color = BasicTeraData.Instance.WindowData.PlayerColor; //((Color)App.Current.FindResource("MeColor"));
                    var colorTrans = Color.FromArgb(0, color.R, color.G, color.B);
                    var ls = 0;
                    if (BasicTeraData.Instance.WindowData.GraphMode == GraphMode.DPS) { ls = 1; }

                    // create the series
                    var newSeries = new LineSeries()
                    {
                        Title = name,
                        Stroke = new LinearGradientBrush()
                        {
                            StartPoint = new Point(0, 0),
                            EndPoint = new Point(1, 0),
                            GradientStops = new GradientStopCollection
                            {
                                new GradientStop(colorTrans, 0),
                                new GradientStop(color, 1),
                            },
                            Opacity = isMe ? 1 : .7
                        },
                        Fill = new LinearGradientBrush()
                        {
                            StartPoint = new Point(0, 0),
                            EndPoint = new Point(0, 1),
                            GradientStops = new GradientStopCollection
                            {
                                new GradientStop(color, 0),
                                new GradientStop(colorTrans, 1),
                            },
                            Opacity = .2
                        },
                        PointGeometrySize = 0,
                        StrokeThickness = 2,
                        LineSmoothness = ls
                    };
                    newSeries.Values = new ChartValues<ObservablePoint>();

                    // fill the series with zeros if this player joined after the start of the fight
                    //while (newSeries.Values.Count < _values - 1) newSeries.Values.Add(0D);
                    // add current sample too
                    double val = 0;
                    switch (BasicTeraData.Instance.WindowData.GraphMode)
                    {
                        case GraphMode.CMA:
                            val = src.Avg;
                            break;
                        case GraphMode.DPS:
                            val = src.Dps;
                            break;
                    }

                    newSeries.Values.Add(new ObservablePoint(_currTime / (double)TimeSpan.TicksPerSecond, val));
                    // add new series to Series collection
                    Series.Add(newSeries);
                }
            }
            _currTime = message.StatsSummary.EntityInformation.Interval;

            ChartVisibility = Series.Count > 0 && _currSample != 0;
            var points = 0;
            foreach (var serie in Series)
            {
                points += serie.Values.Count;
            }
            Debug.WriteLine("Samples: " + points);
        }
        internal void Reset()
        {
            Series.Clear();
            Sources.Clear();
            try
            {
                // causes NullReference for some reason
                EnrageSections.Clear();
            }
            catch { }
            _enraged = false;
            _values = 0;
            _onlyMe = false;
            _currEntity = 0;
            _currTime = -1;
            _currSample = 0;
        }

        private void CheckReset(UiUpdateMessage message)
        {
            if (message.StatsSummary.PlayerDamageDealt.Count == 0 ||
                message.StatsSummary.EntityInformation.Interval < _currTime ||
                message.StatsSummary.EntityInformation.Entity == null ||
                message.StatsSummary.EntityInformation.Entity.Id.Id != _currEntity) Reset();
        }
        private Color GetColor(PlayerClass c)
        {
            switch (c)
            {
                case PlayerClass.Warrior:
                case PlayerClass.Slayer:
                case PlayerClass.Berserker:
                case PlayerClass.Sorcerer:
                case PlayerClass.Archer:
                case PlayerClass.Reaper:
                case PlayerClass.Gunner:
                case PlayerClass.Ninja:
                case PlayerClass.Valkyrie: return BasicTeraData.Instance.WindowData.DpsColor; //((Color)App.Current.FindResource("DpsColor"));
                case PlayerClass.Priest:
                case PlayerClass.Mystic: return BasicTeraData.Instance.WindowData.HealerColor; //return ((Color)App.Current.FindResource("HealColor"));
                case PlayerClass.Lancer:
                case PlayerClass.Brawler: return BasicTeraData.Instance.WindowData.TankColor;//return ((Color)App.Current.FindResource("TankColor"));
                default: return ((Color)App.Current.FindResource("UnkColor"));
            }
        }
    }
}