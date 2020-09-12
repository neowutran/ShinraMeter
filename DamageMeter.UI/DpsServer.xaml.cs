using System.Windows.Input;

namespace DamageMeter.UI
{
    public partial class DpsServer
    {

        public DpsServer()
        {
            InitializeComponent();
        }

        private void OnTbKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;
            Keyboard.ClearFocus();
        }
    }
}
