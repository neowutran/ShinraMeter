using System;
using System.IO;
using System.Windows;
using System.Xml.Linq;

namespace Tera.Data
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
            int height, x, y;
            int.TryParse(_xml.Root.Element("size").Element("height").Value, out height);
            int.TryParse(_xml.Root.Element("location").Element("x").Value, out x);
            int.TryParse(_xml.Root.Element("location").Element("y").Value, out y);

            Height =  height;
            Location = new Point(x, y);
        }

        public int Height { get; set; }

        public Point Location { get; set; }

        public void Save()
        {
            _xml.Root.Element("location").Element("x").Value = Location.X.ToString();
            _xml.Root.Element("location").Element("y").Value = Location.Y.ToString();
            _xml.Root.Element("size").Element("height").Value = Height.ToString();
            _xml.Save(_windowFile);
        }
    }
}