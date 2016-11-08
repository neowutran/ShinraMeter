using Data.Actions.Notify;
using Data.Actions.Notify.SoundElements;
using Data.Events;
using Data.Events.Abnormality;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Xml;
using System.Xml.Linq;
using Tera.Game;

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

            //TODO load the file depending on meter user class.
            var windowFile = Path.Combine(basicData.ResourceDirectory, "config/events/events-mystic.xml");

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
                BasicTeraData.LogError(ex.Message, true, true);
                Save();
                _filestream = new FileStream(windowFile, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
                return;
            }
            catch(Exception ex)
            {
                BasicTeraData.LogError(ex.Message, true, true);
                return;
            }
       
            ParseAbnormalities();
        }

        public void Save()
        {
            if (_filestream == null)
            {
                return;
            }


            var xml = new XDocument(new XDeclaration("1.0", "utf-8", "yes"), new XElement("events"));
            xml.Root.Add(Events.Serialize());
            _filestream.SetLength(0); 
            using (var sw = new StreamWriter(_filestream, new UTF8Encoding(true)))
            {
                sw.Write(xml.Declaration + Environment.NewLine + xml);
            }
            _filestream.Close();
        }

        private void ParseAbnormalities()
        {
            var root = _xml.Root;
            foreach (var abnormality in root.Elements("abnormality"))
            {
                List<int> ids = new List<int>();
                List<HotDot.Types> types = new List<HotDot.Types>();
                var abnormalities = abnormality.Element("abnormalities");
                foreach (var abnormalityId in abnormalities.Elements("abnormality"))
                {
                    var idElement = abnormalityId.Value;
                    int id;

                    if(int.TryParse(idElement, out id))
                    {
                        ids.Add(id);
                        continue;
                    }
                    HotDot.Types type;
                    if(!Enum.TryParse(idElement, true, out type))
                    {
                        throw new Exception(idElement + " is not an acceptable value.");
                    }
                    types.Add(type);
                }


                var ingame = bool.Parse(abnormality.Attribute("ingame").Value);
                AbnormalityTargetType target;
                AbnormalityTriggerType trigger;
                Enum.TryParse(abnormality.Attribute("target").Value, true, out target);
                Enum.TryParse(abnormality.Attribute("trigger").Value, true, out trigger);
                var remainingSecondsBeforeTrigger = 0;
                if (trigger == AbnormalityTriggerType.MissingDuringFight)
                {
                    remainingSecondsBeforeTrigger = int.Parse(abnormality.Attribute("remaining_seconds_before_trigger").Value);
                }
                var abnormalityEvent = new AbnormalityEvent(ingame, ids, types, target, trigger, remainingSecondsBeforeTrigger);
                Events.Add(abnormalityEvent, new List<Actions.Action>());
                foreach(var notify in abnormality.Element("actions").Elements("notify"))
                {
                    Balloon ballonData = null;
                    Sound soundData = null;
                    var balloon = notify.Element("balloon");
                    if (balloon != null) {
                        var titleText = balloon.Attribute("title_text").Value;
                        var bodyText = balloon.Attribute("body_text").Value;
                        var displayDuration = int.Parse(balloon.Attribute("display_time").Value);
                        ballonData = new Balloon(titleText, bodyText, displayDuration);
                    }
                    var sound = notify.Element("sound");
                    if(sound != null)
                    {
                        Music musicData = null;
                        List<Beep> beepsData = null;
                        var music = sound.Element("music");
                        if(music != null)
                        {
                            var musicFile = music.Attribute("file").Value;
                            var volume = float.Parse(music.Attribute("volume").Value);
                            var duration = int.Parse(music.Attribute("duration").Value);
                            musicData = new Music(musicFile, volume, duration);
                        }
                        var beeps = sound.Element("beeps");
                        if(beeps != null)
                        {
                            beepsData = new List<Beep>();
                            foreach(var beep in beeps.Elements())
                            {
                                var frequency = int.Parse(beep.Attribute("frequency").Value);
                                var duration = int.Parse(beep.Attribute("duration").Value);
                                beepsData.Add(new Beep(frequency, duration));
                            }
                        }

                        SoundType soundType;
                        Enum.TryParse(sound.Attribute("type").Value, true, out soundType);
                        soundData = new Sound(beepsData, musicData, soundType);
                    }

                    var notifyAction = new NotifyAction(soundData, ballonData);
                    Events[abnormalityEvent].Add(notifyAction);
                }
            }
        }
    }
}