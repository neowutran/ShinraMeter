using LiveChartsCore.Drawing;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.Painting.Effects;
using SkiaSharp;
using System;

namespace DamageMeter.UI;

internal static class UiUtils
{
    internal static RectangularSection CreateEnrageSection(long startTime, long endTime)
    {
        return new RectangularSection
        {
            Xi = startTime / (double)TimeSpan.TicksPerSecond,
            Xj = endTime / (double)TimeSpan.TicksPerSecond,
            Fill = new LinearGradientPaint([new SKColor(0xff, 0x33, 0x33, 0x30), new SKColor(0xff, 0x33, 0x33, 0x00)],
                new SKPoint(0, 0), new SKPoint(0, 1), [0, 1])
        };
    }
    
    internal static Axis CreateDpsXAxis()
    {
        return CreateBaseAxis(v => FormatHelpers.Instance.FormatTimeSpan(TimeSpan.FromSeconds(v)), true);
    }
    internal static Axis CreateDpsYAxis()
    {
        var ret = CreateBaseAxis(v => FormatHelpers.Instance.FormatValue(Convert.ToInt64(v)), true);
        ret.MinLimit = 0;
        return ret;
    }
    
    static Axis CreateBaseAxis(Func<double, string> labeler, bool dashedSeparators = false)
    {
        return new Axis
        {
            LabelsPaint = new SolidColorPaint(SKColors.LightSlateGray),
            TextSize = 8,
            SeparatorsPaint = new SolidColorPaint(SKColors.LightSlateGray.WithAlpha(30))
            {
                StrokeThickness = 1,
                PathEffect = dashedSeparators ? new DashEffect([3, 3]) : null
            },
            TicksPaint = new SolidColorPaint(SKColors.DimGray.WithAlpha(20))
            {
                StrokeThickness = 1,
                PathEffect = new DashEffect([3, 3]),
            },
            Padding = new Padding(4),
            CrosshairLabelsBackground = SKColors.Transparent.AsLvcColor(),
            CrosshairLabelsPaint = new SolidColorPaint(SKColors.Transparent, 1),
            CrosshairPaint = new SolidColorPaint(SKColors.DimGray, 1),
            CrosshairSnapEnabled = true,
            Labeler = labeler
        };
    }
}