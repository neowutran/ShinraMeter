using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace DamageMeter.UI
{
    public class CustomScrollViewer : ScrollViewer
    {
        public static readonly DependencyProperty CustomOffsetProperty = DependencyProperty.Register("CustomOffset", typeof(double), typeof(CustomScrollViewer), new PropertyMetadata(new PropertyChangedCallback(onChanged)));

        public double CustomOffset
        {
            get { return (double)this.GetValue(ScrollViewer.VerticalOffsetProperty); }
            set { this.ScrollToVerticalOffset(value); }
        }
        private static void onChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((CustomScrollViewer)d).CustomOffset = (double)e.NewValue;
        }
    }
}
