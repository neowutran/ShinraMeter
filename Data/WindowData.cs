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
            DefaultValue();
            // Load XML File
            _windowFile = Path.Combine(basicData.ResourceDirectory, "config/window.xml");
            try
            {
                _xml = XDocument.Load(_windowFile);
            }
            catch (FileNotFoundException)
            {
                return;
            }

            ParseLocation();
            ParseLanguage();
            ParseMainWindowOpacity();
            ParseSkillWindowOpacity();
            ParseAutoUpdate();
            ParseRememberPosition();
            ParseWinpcap();
        }


        public Point Location { get; set; }

        public double MainWindowOpacity { get; private set; }
        public double SkillWindowOpacity { get; private set; }

        public bool RememberPosition { get; private set; }
        public string Language { get; private set; }

        public bool AutoUpdate { get; private set; }

        public bool Winpcap { get; private set; }

        private void DefaultValue()
        {
            Location = new Point(0, 0);
            Language = "EU-EN";
            MainWindowOpacity = 0.5;
            SkillWindowOpacity = 0.7;
            AutoUpdate = true;
            RememberPosition = true;
            Winpcap = false;
        }

        public void ParseRememberPosition()
        {
            var root = _xml.Root;
            var rememberPosition = root?.Element("remember_position");
            if (rememberPosition == null) return;
            bool remember;
            var parseSuccess = bool.TryParse(rememberPosition.Value, out remember);
            if (parseSuccess)
            {
                RememberPosition = remember;
            }
        }

        public void ParseWinpcap()
        {
            var root = _xml.Root;
            var winpcapElement = root?.Element("winpcap");
            if (winpcapElement == null) return;
            bool winpcap;
            var parseSuccess = bool.TryParse(winpcapElement.Value, out winpcap);
            if (parseSuccess)
            {
                Winpcap = winpcap;
            }
        }




        public void ParseAutoUpdate()
        {
            var root = _xml.Root;
            var autoupdate = root?.Element("autoupdate");
            if (autoupdate == null) return;
            bool auto;
            var parseSuccess = bool.TryParse(autoupdate.Value, out auto);
            if (parseSuccess)
            {
                AutoUpdate = auto;
            }
        }

        private void ParseLocation()
        {
            int x, y;
            var root = _xml.Root;

            var location = root?.Element("location");
            if (location == null) return;
            var xElement = location.Element("x");
            var yElement = location.Element("y");
            if (xElement == null || yElement == null) return;

            var xParsed = int.TryParse(xElement.Value, out x);
            var yParsed = int.TryParse(yElement.Value, out y);
            if (xParsed && yParsed)
            {
                Location = new Point(x, y);
            }
        }

        private void ParseLanguage()
        {
            var root = _xml.Root;
            var languageElement = root?.Element("language");
            if (languageElement == null) return;
            Language = languageElement.Value;
        }

        private void ParseMainWindowOpacity()
        {
            int mainWindowOpacity;
            var root = _xml.Root;
            var opacity = root?.Element("opacity");
            var mainWindowElement = opacity?.Element("mainWindow");
            if (mainWindowElement == null) return;

            if (int.TryParse(mainWindowElement.Value, out mainWindowOpacity))
            {
                MainWindowOpacity = (double) mainWindowOpacity/100;
            }
        }

        private void ParseSkillWindowOpacity()
        {
            int skillWindowOpacity;
            var root = _xml.Root;
            var opacity = root?.Element("opacity");
            var mainWindowElement = opacity?.Element("skillWindow");
            if (mainWindowElement == null) return;

            if (int.TryParse(mainWindowElement.Value, out skillWindowOpacity))
            {
                SkillWindowOpacity = (double) skillWindowOpacity/100;
            }
        }

        public void Save()
        {
            var xml = new XDocument(new XElement("window"));
            xml.Root.Add(new XElement("location"));
            xml.Root.Element("location").Add(new XElement("x", Location.X.ToString(CultureInfo.InvariantCulture)));
            xml.Root.Element("location").Add(new XElement("y", Location.Y.ToString(CultureInfo.InvariantCulture)));
            xml.Root.Add(new XElement("language", Language));
            xml.Root.Add(new XElement("opacity"));
            xml.Root.Element("opacity").Add(new XElement("mainWindow", MainWindowOpacity*100));
            xml.Root.Element("opacity").Add(new XElement("skillWindow", SkillWindowOpacity*100));
            xml.Root.Add(new XElement("autoupdate", AutoUpdate));
            xml.Root.Add(new XElement("remember_position", RememberPosition));
            xml.Root.Add(new XElement("winpcap", Winpcap));
            xml.Save(_windowFile);
        }
    }
}