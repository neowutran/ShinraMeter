// Copyright (c) Gothos
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using Newtonsoft.Json;

namespace Tera.DamageMeter
{
    public class HotKeySettings
    {
        public string PasteStats { get; set; }
        public string Reset { get; set; }
    }

    public class Settings
    {
        public static readonly string SettingsDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "GothosTeraDamageMeter");
        public static readonly string SettingsFile = Path.Combine(SettingsDirectory, "settings.json");

        public HotKeySettings HotKeys { get; private set; }
        public bool AlwaysOnTop { get; set; }
        public double Opacity { get; set; }
        public int? BufferSize { get; set; }

        public Settings()
        {
            HotKeys = new HotKeySettings();
            HotKeys.PasteStats = "Control+Shift+V";
            Opacity = 1.0;
        }

        public static Settings Load()
        {
            if (File.Exists(SettingsFile))
                return JsonConvert.DeserializeObject<Settings>(File.ReadAllText(SettingsFile));
            else
                return new Settings();
        }

        public void Save()
        {
            Directory.CreateDirectory(SettingsDirectory);
            File.WriteAllText(SettingsFile, JsonConvert.SerializeObject(this, Formatting.Indented));
        }

        public event EventHandler SettingsChanged;

        public void OnSettingsChanged()
        {
            var handler = SettingsChanged;
            if (handler != null) handler(this, new EventArgs());
        }
    }
}
