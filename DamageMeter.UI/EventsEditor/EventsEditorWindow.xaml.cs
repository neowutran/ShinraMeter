using Nostrum.Extensions;
using System.Windows;
using System.Windows.Input;

namespace DamageMeter.UI.EventsEditor
{
    public partial class EventsEditorWindow : Window
    {
        public EventsEditorWindow()
        {
            InitializeComponent();

            DataContext = new EventsEditorViewModel();
        }

        private void Drag(object sender, MouseButtonEventArgs e)
        {
            this.TryDragMove();
        }

        private void CloseWindow(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
