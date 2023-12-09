using System.Collections.ObjectModel;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;

namespace DamageMeter.UI;

public interface IChartViewModel
{
    Axis[] XAxes { get; }
    Axis[] YAxes { get; }
    ObservableCollection<ISeries> DpsSeries { get; }
}