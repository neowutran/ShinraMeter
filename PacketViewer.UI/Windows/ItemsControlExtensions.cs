using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PacketViewer.UI
{
    public static class ItemsControlExtensions
    {
        public static void VirtualizedScrollIntoView(this ItemsControl control, object item)
        {
            try
            {
                // this is basically getting a reference to the ScrollViewer defined in the ItemsControl's style (identified above).
                // you *could* enumerate over the ItemsControl's children until you hit a scroll viewer, but this is quick and
                // dirty!
                // First 0 in the GetChild returns the Border from the ControlTemplate, and the second 0 gets the ScrollViewer from
                // the Border.
                ScrollViewer sv = VisualTreeHelper.GetChild(VisualTreeHelper.GetChild((DependencyObject)control, 0), 0) as ScrollViewer;
                // now get the index of the item your passing in
                int index = control.Items.IndexOf(item);
                if (index != -1)
                {
                    // since the scroll viewer is using content scrolling not pixel based scrolling we just tell it to scroll to the index of the item
                    // and viola!  we scroll there!
                    sv.ScrollToVerticalOffset(index);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("What the..." + ex.Message);
            }
        }
    }
}