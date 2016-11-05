using Data.Events;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Xml;
using System.Xml.Linq;

namespace Data
{
    public class EventsData
    {
        private readonly FileStream _filestream;
        private readonly XDocument _xml;

        public Dictionary<Event, List<Actions.Action>> Events = new Dictionary<Event, List<Actions.Action>>();
      
        private void DefaultValue()
        {
          
        }


        public EventsData(BasicTeraData basicData)
        {
            DefaultValue();
            // Load XML File
            var windowFile = Path.Combine(basicData.ResourceDirectory, "config/events.xml");

            try
            {
                var attrs = File.GetAttributes(windowFile);
                File.SetAttributes(windowFile, attrs & ~FileAttributes.ReadOnly);
            }
            catch
            {
                //ignore
            }

            try
            {
                _filestream = new FileStream(windowFile, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
                _xml = XDocument.Load(_filestream);
            }
            catch (Exception ex) when (ex is XmlException || ex is InvalidOperationException)
            {
                Save();
                _filestream = new FileStream(windowFile, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
                return;
            }
            catch
            {
                return;
            }
       
            Parse("do_not_warn_on_crystalbind", "DoNotWarnOnCB");
        }

        public void Save()
        {
            if (_filestream == null)
            {
                return;
            }


            var xml = new XDocument(new XDeclaration("1.0", "utf-8", "yes"), new XElement("events"));
            xml.Root.Add(new XElement("location"));
            xml.Root.Element("location").Add(new XElement("x", ""));
           

            _filestream.SetLength(0); 
            using (var sw = new StreamWriter(_filestream, new UTF8Encoding(true)))
            {
                sw.Write(xml.Declaration + Environment.NewLine + xml);
            }
            _filestream.Close();
        }

        private void Parse(string xmlName, string settingName)
        {
            var root = _xml.Root;
            var xml = root?.Element(xmlName);
            if (xml == null) return;
            var setting = this.GetType().GetProperty(settingName);
            if (setting.PropertyType == typeof(int))
            {
                int value;
                var parseSuccess = int.TryParse(xml.Value, out value);
                if (parseSuccess)
                {
                    setting.SetValue(this, value, null);
                }
            }
            if (setting.PropertyType == typeof(double))
            {
                double value;
                var parseSuccess = double.TryParse(xml.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
                if (parseSuccess)
                {
                    setting.SetValue(this, value, null);
                }
            }
            if (setting.PropertyType == typeof(float))
            {
                float value;
                var parseSuccess = float.TryParse(xml.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
                if (parseSuccess)
                {
                    setting.SetValue(this, value, null);
                }
            }
            if (setting.PropertyType == typeof(bool))
            {
                bool value;
                var parseSuccess = bool.TryParse(xml.Value, out value);
                if (parseSuccess)
                {
                    setting.SetValue(this, value, null);
                }
            }
            if (setting.PropertyType == typeof(string))
            {
                setting.SetValue(this, xml.Value, null);
            }
        }
    }
}