using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Windows.Media;
using Data;
using LiveCharts;
using LiveCharts.Wpf;
using Tera.Game;
using Color = System.Windows.Media.Color;

namespace DamageMeter.UI
{
    public class GraphViewModel : TSPropertyChanged
    {
        private class DpsSource
        {
            // fields
            private long _prevAmount;
            private double _damageSum;
            private long _timeSum;
            private long _prevTick;

            // properties
            public ulong Id { get; }
            public Queue<Tuple<double, long>> Values { get; }
            public double Avg => Values.Count == 0 || _timeSum == 0 ? _damageSum : TimeSpan.TicksPerSecond * _damageSum / _timeSum;

            // ctor
            public DpsSource(ulong id)
            {
                Id = id;
                Values = new Queue<Tuple<double, long>>();
                _prevTick = DateTime.Now.Ticks;
            }

            // methods
            public void Update(long newAmount)
            {
                // get the amount of damage done between this and previous time instants
                var dmgDiff = Convert.ToDouble(newAmount - _prevAmount);
                // get the amount of ticks between this and previous update
                var timeDiff = Values.Count != 0 ? DateTime.Now.Ticks - _prevTick : 0L;
                // get a factor indicating how many seconds last interval was
                var factor = timeDiff != 0 ? timeDiff / (double)TimeSpan.TicksPerSecond : 1;

                // update values
                _prevAmount = newAmount;
                _prevTick = DateTime.Now.Ticks;
                _timeSum += timeDiff;
                dmgDiff = dmgDiff / factor;
                _damageSum += dmgDiff;

                // queue damage and time deltas
                Values.Enqueue(new Tuple<double, long>(dmgDiff, timeDiff));

                if (Values.Count < Samples) return;

                // remove first value pair in the queue and subtract it from total damage and total time
                var val = Values.Dequeue();
                _damageSum -= val.Item1;
                _timeSum -= val.Item2;
            }
        }

        // fields
        private List<DpsSource> Sources;
        private int _currSample = 0;
        private int _values = 0;
        private bool _onlyMe;
        private long _currTime = -1;
        private ulong _currEntity = 0;
        private bool _enraged;

        internal static int Samples = 10;
        internal static int ShowedSamples = int.MaxValue;

        // properties
        //private StepLineSeries Enrage { get; set; }
        public Func<double, string> DpsFormatter { get; set; }
        public SeriesCollection Series { get; set; }
        //public SeriesCollection EnrageSeries { get; set; }
        public SectionsCollection EnrageSections { get; }
        public bool ChartVisibility => Series.Count > 0;
        private bool Enraged
        {
            get => _enraged;
            set
            {
                if (_enraged != value && value)
                {
                    // just enraged, add new
                    EnrageSections.Add(new AxisSection
                    {
                        StrokeThickness = 0,
                        Fill = new SolidColorBrush(Colors.Red) { Opacity = .2 },
                        Value = _currSample,
                        SectionWidth = 1
                    });
                }
                else if (value)
                {
                    // enrage still going
                    if (EnrageSections.Count > 0)
                    {
                        // update duration
                        var last = EnrageSections.Last();
                        last.SectionWidth = _currSample - last.Value;
                    }
                    else
                    {
                        // add new
                        EnrageSections.Add(new AxisSection
                        {
                            StrokeThickness = 0,
                            Fill = new SolidColorBrush(Colors.Red) { Opacity = .2 },
                            Value = _currSample,
                            SectionWidth = 1
                        });
                    }
                }
                _enraged = value;
            }
        }

        // ctor
        internal GraphViewModel()
        {
            Series = new SeriesCollection();
            DpsFormatter = new Func<double, string>((v) => FormatHelpers.Instance.FormatValue(Convert.ToInt64(v)));
            Sources = new List<DpsSource>();
            EnrageSections = new SectionsCollection();
        }

        // methods
        internal void Update(UiUpdateMessage message)
        {
            NotifyPropertyChanged(nameof(ChartVisibility));
            // skip update if timer is the same
            if (_currTime == message.StatsSummary.EntityInformation.Interval) return;
            _currSample++;

            CheckReset(message);

            // store the amount of samples we received until now
            if (_values >= ShowedSamples) _values = ShowedSamples;
            else _values++;

            // store current enemy
            _currEntity = message.StatsSummary.EntityInformation.Entity != null ? message.StatsSummary.EntityInformation.Entity.Id.Id : 0;

            // show only current player if there are more than 5 players
            var onlyMeChanged = false;
            if (!_onlyMe && message.StatsSummary.PlayerDamageDealt.Count > 5)
            {
                onlyMeChanged = true;
                _onlyMe = true;
            }

            // adds enrage stat
            if (BasicTeraData.Instance.HotDotDatabase?.Get((int)HotDotDatabase.StaticallyUsedBuff.Enraged) == null) Enraged = false; 
            else
            {
                var currTime = message.StatsSummary.EntityInformation.EndTime;
                message.Abnormals.Get(message.StatsSummary.EntityInformation.Entity)
                                 .TryGetValue(BasicTeraData.Instance.HotDotDatabase?
                                 .Get((int)HotDotDatabase.StaticallyUsedBuff.Enraged), out var enrageHotDot);

                Enraged = (enrageHotDot?.LastEnd() ?? long.MaxValue) == currTime;
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
                src.Update(playerDamage.Amount);

                // check if we already have a line series for this player
                if (Series.FirstOrDefault(x => x.Title == name) is LineSeries existing)
                {
                    // series exists, add and remove points

                    // get player line series
                    var seriesVals = existing.Values as ChartValues<double>;
                    // remove first point if total sample count is above the maximum
                    if (_values >= ShowedSamples) seriesVals.RemoveAt(0);
                    // get average for current instant and add it as a line point
                    seriesVals.Add(src.Avg);
                    // remove first point if the series is longer than it should be (shouldn't happen anyway)
                    while (seriesVals.Count >= _values) seriesVals.RemoveAt(0);
                }
                else
                {
                    // series doesen't exist, create it

                    // get line color (red = dps, blue = tank, green = healer, orange = currPlayer)
                    var color = GetColor(pClass);
                    if (isMe) color = ((Color)App.Current.FindResource("MeColor"));

                    // create the series
                    var newSeries = new LineSeries()
                    {
                        Title = name,
                        Stroke = new SolidColorBrush(color) { Opacity = isMe ? 1 : .7 },
                        Fill = new SolidColorBrush(Colors.Transparent),
                        PointGeometrySize = 0,
                        StrokeThickness = 2,
                        LineSmoothness = 0
                    };
                    newSeries.Values = new ChartValues<double>();

                    // fill the series with zeros if this player joined after the start of the fight
                    while (newSeries.Values.Count < _values - 1) newSeries.Values.Add(0D);
                    // add current sample too
                    newSeries.Values.Add(src.Avg);
                    // add new series to Series collection
                    Series.Add(newSeries);
                }
            }
            _currTime = message.StatsSummary.EntityInformation.Interval;
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
            catch  { }
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
                case PlayerClass.Valkyrie:
                    return ((Color)App.Current.FindResource("DpsColor"));
                case PlayerClass.Priest:
                case PlayerClass.Mystic:
                    return ((Color)App.Current.FindResource("HealColor"));
                case PlayerClass.Lancer:
                case PlayerClass.Brawler:
                    return ((Color)App.Current.FindResource("TankColor"));
                default:
                    return ((Color)App.Current.FindResource("UnkColor"));
            }
        }
    }
}