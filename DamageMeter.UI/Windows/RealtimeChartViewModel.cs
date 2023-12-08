using DamageMeter.Database.Structures;
using Data;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.Drawing;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.Painting.Effects;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using LiveChartsCore.Measure;
using Tera.Game;
using Color = System.Windows.Media.Color;

namespace DamageMeter.UI;

public class RealtimeChartViewModel : TSPropertyChanged
{

    readonly List<DpsSource> _sources = [];
    long _currTime = -1;
    ulong _currEntity = 0;

    public ObservableCollection<ISeries> DpsSeries { get; } = [];
    public ObservableCollection<RectangularSection> EnrageSections { get; } = [];

    public Axis[] XAxes { get; }
    public Axis[] YAxes { get; }

    int _currSample;
    int _values;
    bool _onlyMe;
    bool _enraged;

    //

    bool _isAnimated;
    public bool IsAnimated
    {
        get => _isAnimated;
        set
        {
            if (_isAnimated == value) return;
            _isAnimated = value;
            NotifyPropertyChanged();
        }
    }

    bool _isChartVisible;
    readonly Axis _xAxis;
    readonly Axis _yAxis;

    public bool IsChartVisible
    {
        get => _isChartVisible;
        set
        {
            if (_isChartVisible == value) return;
            _isChartVisible = value;
            NotifyPropertyChanged();
        }
    }

    public RealtimeChartViewModel()
    {
        _xAxis = new Axis
        {
            LabelsPaint = new SolidColorPaint(SKColors.DimGray/*.WithAlpha(100)*/),
            TextSize = 8,
            //MinStep = 10,
            SeparatorsPaint = new SolidColorPaint(SKColors.LightSlateGray/*.WithAlpha(30)*/)
            {
                StrokeThickness = 1,
                PathEffect = new DashEffect([3, 3]),
            },
            TicksPaint = new SolidColorPaint(SKColors.DimGray/*.WithAlpha(20)*/)
            {
                StrokeThickness = 1,
                PathEffect = new DashEffect([3, 3]),
            },
            //ForceStepToMin = true,
            Labeler = v => FormatHelpers.Instance.FormatTimeSpan(TimeSpan.FromSeconds(v)),
            Padding = new Padding(4),
            CrosshairLabelsBackground = SKColors.Transparent.AsLvcColor(),
            CrosshairLabelsPaint = new SolidColorPaint(SKColors.Transparent, 1),
            CrosshairPaint = new SolidColorPaint(SKColors.DimGray, 1),
            CrosshairSnapEnabled = true

        };
        XAxes = [_xAxis];
        _yAxis = new Axis
        {
            LabelsPaint = new SolidColorPaint(SKColors.DimGray/*.WithAlpha(100)*/),
            MinLimit = 0,
            TextSize = 8,
            Position = AxisPosition.End,
            SeparatorsPaint = new SolidColorPaint(SKColors.LightSlateGray/*.WithAlpha(30)*/)
            {
                StrokeThickness = 1,
                PathEffect = new DashEffect([3, 3]),
            },
            TicksPaint = new SolidColorPaint(SKColors.DimGray/*.WithAlpha(20)*/)
            {
                StrokeThickness = 1,
                PathEffect = new DashEffect([3, 3]),
            },
            CrosshairLabelsBackground = SKColors.Transparent.AsLvcColor(),
            CrosshairLabelsPaint = new SolidColorPaint(SKColors.Transparent, 1),
            CrosshairPaint = new SolidColorPaint(SKColors.SlateGray, 1),
            CrosshairSnapEnabled = true,
            //ForceStepToMin = false,
            //MinStep = 10,
            Labeler = v => FormatHelpers.Instance.FormatValue(Convert.ToInt64(v)),
            Padding = new Padding(4)
        };
        YAxes = [_yAxis];
    }

    public void Update(UiUpdateMessage message)
    {
        IsAnimated = BasicTeraData.Instance.WindowData.RealtimeGraphAnimated;

        // skip update if timer is the same
        if (_currTime == message.StatsSummary.EntityInformation.Interval)
        {
            IsChartVisible = DpsSeries.Count > 0 && _currSample != 0;
            return;
        }
        _currSample++;

        CheckReset(message);

        // store the amount of samples we received until now
        var maxDisplayedSamples = BasicTeraData.Instance.WindowData.RealtimeGraphDisplayedInterval == 0
            ? int.MaxValue
            : BasicTeraData.Instance.WindowData.RealtimeGraphDisplayedInterval;
        _values = _values >= maxDisplayedSamples ? maxDisplayedSamples : _values + 1;

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
        {
            SetEnrage(false);
        }
        else
        {
            var currTime = message.StatsSummary.EntityInformation.EndTime;
            message.Abnormals.Get(message.StatsSummary.EntityInformation.Entity)
                .TryGetValue(BasicTeraData.Instance.HotDotDatabase?
                .Get((int)HotDotDatabase.StaticallyUsedBuff.Enraged), out var enrageHotDot);
            var lastEnd = enrageHotDot?.LastEnd() ?? long.MaxValue;
            SetEnrage(lastEnd == currTime);
        }

        // iterate player stats and update line series
        double max = 0;
        foreach (var playerDamage in message.StatsSummary.PlayerDamageDealt)
        {
            UpdateDpsSerie(playerDamage);
        }

        //if(max != 0)
        //    _yAxis.SetLimits(0, max * 1.1);


        _currTime = message.StatsSummary.EntityInformation.Interval;
        IsChartVisible = DpsSeries.Count > 0 && _currSample != 0;

        //if(_currTime / (double)TimeSpan.TicksPerSecond != 0)
        //    _xAxis.MaxLimit = _currTime / (double)TimeSpan.TicksPerSecond;
        return;

        void UpdateDpsSerie(PlayerDamageDealt playerDamage)
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
                    var rem = DpsSeries.FirstOrDefault(x => x.Name == name);
                    if (rem != null) DpsSeries.Remove(rem);
                    var srcRem = _sources.FirstOrDefault(x => x.Id == playerId.Id);
                    if (srcRem != null) _sources.Remove(srcRem);
                }
                return;
            }

            // add this player to sources if it's not already there
            var src = _sources.FirstOrDefault(x => x.Id == playerId.Id);
            if (src == null)
            {
                src = new DpsSource(playerId.Id);
                _sources.Add(src);
            }
            // update it
            src.Update(playerDamage, message.StatsSummary.EntityInformation);

            // check if we already have a line series for this player
            if (DpsSeries.FirstOrDefault(x => x.Name == name) is LineSeries<ObservablePoint> { Values: Collection<ObservablePoint> seriesValues })
            {
                // series exists, add and remove points
                // get average for current instant and add it as a line point
                var val = BasicTeraData.Instance.WindowData.GraphMode switch
                {
                    GraphMode.CMA => src.Avg,
                    GraphMode.DPS => src.Dps,
                    _ => 0
                };

                if (val > max) max = val;

                seriesValues.Add(new ObservablePoint(_currTime / (double)TimeSpan.TicksPerSecond, val));
                // remove first point if the series is longer than it should be (shouldn't happen anyway)
                while (seriesValues.Count >= _values) seriesValues.RemoveAt(0);
            }
            else
            {
                // series doesen't exist, create it

                // get line color (red = dps, blue = tank, green = healer, orange = currPlayer)
                var color = pClass switch
                {
                    PlayerClass.Warrior or
                    PlayerClass.Slayer or
                    PlayerClass.Berserker or
                    PlayerClass.Sorcerer or
                    PlayerClass.Archer or
                    PlayerClass.Reaper or
                    PlayerClass.Gunner or
                    PlayerClass.Ninja or
                    PlayerClass.Valkyrie => BasicTeraData.Instance.WindowData.DpsColor,
                    PlayerClass.Priest or
                    PlayerClass.Mystic => BasicTeraData.Instance.WindowData.HealerColor,
                    PlayerClass.Lancer or
                    PlayerClass.Brawler => BasicTeraData.Instance.WindowData.TankColor,
                    _ => ((Color)App.Current.FindResource("UnkColor"))
                };

                if (isMe) color = BasicTeraData.Instance.WindowData.PlayerColor;
                var colorTrans = Color.FromArgb(0, color.R, color.G, color.B);
                var ls = 0;
                if (BasicTeraData.Instance.WindowData.GraphMode == GraphMode.DPS) { ls = 1; }

                // create the series
                var newValues = new ObservableCollection<ObservablePoint>();
                var newSeries = new LineSeries<ObservablePoint>
                {
                    Name = name,
                    Stroke
                    //= new SolidColorPaint(SKColors.Red, 2),
                    = new LinearGradientPaint([
                        colorTrans.ToSKColor(),
                        color.ToSKColor().WithAlpha(isMe ? (byte)255 : (byte)200)
                    ], new SKPoint(0, 0), new SKPoint(1, 0), [0, 1])
                    {
                        StrokeThickness = 1
                    },
                    GeometryStroke = new SolidColorPaint(color.ToSKColor(), 2),
                    GeometryFill = new SolidColorPaint(color.ToSKColor().WithAlpha(100)),

                    Fill
                    //= new SolidColorPaint(SKColors.Red, 2),
                    = new LinearGradientPaint([
                        color.ToSKColor().WithAlpha(51),
                        colorTrans.ToSKColor(),
                    ],
                    new SKPoint(0, 0), new SKPoint(0, 1), [0, 1]),
                    GeometrySize = 0,
                    LineSmoothness = ls,
                    Values = newValues,
                    DataPadding = new LvcPoint(0, 0),
                };

                // fill the series with zeros if this player joined after tshe start of the fight
                //while (newSeries.Values.Count < _values - 1) newSeries.Values.Add(0D);
                // add current sample too
                var val = BasicTeraData.Instance.WindowData.GraphMode switch
                {
                    GraphMode.CMA => src.Avg,
                    GraphMode.DPS => src.Dps,
                    _ => 0
                };
                newValues.Add(new ObservablePoint(_currTime / (double)TimeSpan.TicksPerSecond, val));

                // add new series to Series collection
                DpsSeries.Add(newSeries);
            }
        }
    }

    void SetEnrage(bool value)
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
                last.Xj = _currTime / (double)TimeSpan.TicksPerSecond;//  - last.Value;
            }
            else
            {
                // add new
                EnrageSections.Add(CreateSection());
            }
        }
        _enraged = value;

        return;

        RectangularSection CreateSection()
        {
            return new RectangularSection()
            {
                Xi = _currTime / (double)TimeSpan.TicksPerSecond,
                Xj = _currTime / (double)TimeSpan.TicksPerSecond,
                Fill = new LinearGradientPaint([new SKColor(0xff, 0x33, 0x33, 0x30), new SKColor(0xff, 0x33, 0x33, 0x00)],
                new SKPoint(0, 0), new SKPoint(0, 1), [0, 1])
            };
        }

    }

    void CheckReset(UiUpdateMessage message)
    {
        if (message.StatsSummary.PlayerDamageDealt.Count == 0 ||
            message.StatsSummary.EntityInformation.Interval < _currTime ||
            message.StatsSummary.EntityInformation.Entity == null ||
            message.StatsSummary.EntityInformation.Entity.Id.Id != _currEntity) Reset();
    }


    public void Reset()
    {
        DpsSeries.Clear();
        _sources.Clear();
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
}

public static class ColorExtensions
{
    public static SKColor ToSKColor(this Color color)
    {
        return new SKColor(color.R, color.G, color.B, color.A);
    }
}