using System.Globalization;
using System.IO;
using System.Windows;
using System.Xml.Linq;

namespace Data
{
    public class WindowData
    {
        private readonly string _windowFile;
        private readonly XDocument _xml;

        public WindowData(BasicTeraData basicData)
        {
            // Load XML File
            _windowFile = Path.Combine(basicData.ResourceDirectory, "window.xml");
            _xml = XDocument.Load(_windowFile);
            int x, y;
            int.TryParse(_xml.Root.Element("location").Element("x").Value, out x);
            int.TryParse(_xml.Root.Element("location").Element("y").Value, out y);
            Location = new Point(x, y);
            Language = _xml.Root.Element("language").Value;
        }

        public Point Location { get; set; }

        public double MainWindowOpacity { get; private set; }
        public double SkillWindowOpacity { get; private set; }

        public string Language { get; private set; }

        public void Save()
        {
            _xml.Root.Element("location").Element("x").Value = Location.X.ToString(CultureInfo.InvariantCulture);
            _xml.Root.Element("location").Element("y").Value = Location.Y.ToString(CultureInfo.InvariantCulture);
            _xml.Save(_windowFile);
        }
    }
}