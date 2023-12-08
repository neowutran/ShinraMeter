using System;
using System.Collections.Generic;
using System.Linq;
using LiveChartsCore;
using LiveChartsCore.Drawing;
using LiveChartsCore.Kernel;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView.Drawing;
using LiveChartsCore.SkiaSharpView.Drawing.Geometries;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.VisualElements;
using LiveChartsCore.VisualElements;
using SkiaSharp;

namespace DamageMeter.UI;

public class RealtimeChartTooltip : IChartTooltip<SkiaSharpDrawingContext>
{
    private StackPanel<RoundedRectangleGeometry, SkiaSharpDrawingContext>? _stackPanel;
    private static readonly int s_zIndex = 10100;
    private readonly SolidColorPaint _backgroundPaint = new(new SKColor(28, 28, 30, 120)) { ZIndex = s_zIndex };
    private readonly SolidColorPaint _fontPaint = new(new SKColor(230, 230, 230)) { ZIndex = s_zIndex + 1 };

    public void Show(IEnumerable<ChartPoint> foundPoints, Chart<SkiaSharpDrawingContext> chart)
    {
        if (_stackPanel == null)
        {
            _stackPanel = new StackPanel<RoundedRectangleGeometry, SkiaSharpDrawingContext>
            {
                Padding = new Padding(10, 5),
                Orientation = ContainerOrientation.Vertical,
                HorizontalAlignment = Align.Start,
                VerticalAlignment = Align.Middle,
                BackgroundPaint = _backgroundPaint
            };

            _stackPanel
                .Animate(
                    new Animation(EasingFunctions.EaseInOut, TimeSpan.FromSeconds(.25)),
                    nameof(_stackPanel.X),
                    nameof(_stackPanel.Y));
        }

        // clear the previous elements.
        foreach (var child in _stackPanel.Children.ToArray())
        {
            _ = _stackPanel.Children.Remove(child);
            chart.RemoveVisual(child);
        }

        var points = foundPoints.OrderByDescending(p => p.Coordinate.PrimaryValue).Take(5).ToArray();

            
        foreach (var point in points)
        {
            var sketch = ((IChartSeries<SkiaSharpDrawingContext>)point.Context.Series).GetMiniaturesSketch();
            var relativePanel = sketch.AsDrawnControl(s_zIndex);

            var label = new LabelVisual
            {
                Text = point.Context.Series.Name + " " + FormatHelpers.Instance.FormatValue((long)point.Coordinate.PrimaryValue),
                Paint = _fontPaint,
                TextSize = 10,
                Padding = new Padding(8, 0, 0, 0),
                ClippingMode = ClipMode.None, // required on tooltips 
                VerticalAlignment = Align.Start,
                HorizontalAlignment = Align.Start
            };

            var sp = new StackPanel<RoundedRectangleGeometry, SkiaSharpDrawingContext>
            {
                Padding = new Padding(0, 4),
                VerticalAlignment = Align.Middle,
                HorizontalAlignment = Align.Middle,
                Children =
                {
                    relativePanel,
                    label
                }
            };

            _stackPanel?.Children.Add(sp);
        }

        var size = _stackPanel!.Measure(chart);

        var location = points.GetTooltipLocation(size, chart);

        _stackPanel.X = location.X;
        _stackPanel.Y = location.Y;

        chart.AddVisual(_stackPanel);
    }

    public void Hide(Chart<SkiaSharpDrawingContext>? chart)
    {
        if (chart is null || _stackPanel is null) return;
        chart.RemoveVisual(_stackPanel);
    }
}