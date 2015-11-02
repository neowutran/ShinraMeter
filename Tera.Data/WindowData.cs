using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Forms;
using System.Xml.Linq;

namespace Tera.Data
{
    public class WindowData
    {

        public WindowData(BasicTeraData basicData)
        {
            // Load XML File
            _windowFile = Path.Combine(basicData.ResourceDirectory, "window.xml");
            _xml = XDocument.Load(_windowFile);
            int width, height, x, y;
            int.TryParse(_xml.Root.Element("size").Element("width").Value, out width);
            int.TryParse(_xml.Root.Element("size").Element("height").Value, out height);
            int.TryParse(_xml.Root.Element("location").Element("x").Value, out x);
            int.TryParse(_xml.Root.Element("location").Element("y").Value, out y);

            Size = new Size(width, height);
            _location = new Point(x,y);
            
        }

        private string _windowFile;
        private XDocument _xml;

        public Size Size { get; set; }

        private Point _location;
        public Point Location
        {
            get { return _location; }
            set
            {
                _location = value;
            }
        }

        public void Save()
        {
            _xml.Root.Element("location").Element("x").Value = _location.X.ToString();
            _xml.Root.Element("location").Element("y").Value = _location.Y.ToString();
            _xml.Root.Element("size").Element("height").Value = Size.Height.ToString();
            _xml.Root.Element("size").Element("width").Value = Size.Width.ToString();

            _xml.Save(_windowFile);
            Console.WriteLine("saved");
        }
    }
}
