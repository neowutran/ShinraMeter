using Data.Actions.Notify;
using Data.Actions.Notify.SoundElements;
using Data.Events;
using Data.Events.Abnormality;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Xml;
using System.Xml.Linq;
using Lang;
using Tera.Game;
using System.Speech.Synthesis;

namespace Data
{
    public class EventsData
    {
        private FileStream _filestreamCommon;
        private FileStream _filestreamClass;

        public Dictionary<Event, List<Actions.Action>> EventsCommon { get; private set; }
        public Dictionary<Event, List<Actions.Action>> EventsClass { get; private set; }
        public Dictionary<Event, List<Actions.Action>> Events { get; private set; }
      

        public void Load(PlayerClass playerClass)
        {
            //TODO load the file depending on meter user class.
            var windowFile = Path.Combine(_basicData.ResourceDirectory, "config/events/events-"+playerClass.ToString().ToLowerInvariant()+".xml");
            XDocument xml;
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
                _filestreamClass = new FileStream(windowFile, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
                xml = XDocument.Load(_filestreamClass);
            }
            catch (Exception ex) when (ex is XmlException || ex is InvalidOperationException)
            {
                BasicTeraData.LogError(ex.Message, true, true);
                Save();
                _filestreamClass = new FileStream(windowFile, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
                return;
            }
            catch (Exception ex)
            {
                BasicTeraData.LogError(ex.Message, true, true);
                return;
            }
            EventsClass = new Dictionary<Event, List<Actions.Action>>();
            Events = new Dictionary<Event, List<Actions.Action>>();
            ParseAbnormalities(EventsClass, xml);
            foreach(var e in EventsCommon)
            {
                Events.Add(e.Key, e.Value);
            }
            foreach(var e in EventsClass)
            {
                Events.Add(e.Key, e.Value);
            }
        }

        BasicTeraData _basicData;


        public EventsData(BasicTeraData basicData)
        {
            _basicData = basicData;
            var eventsdir = Path.Combine(_basicData.ResourceDirectory, "config/events");
            try
            {
                Directory.CreateDirectory(eventsdir);
                foreach (var pclass in Enum.GetNames(typeof(PlayerClass)))
                {
                    var fname = Path.Combine(_basicData.ResourceDirectory, "config/events/events-" + pclass.ToLowerInvariant() + ".xml");
                    if (!File.Exists(fname))
                        File.WriteAllText(fname, LP.ResourceManager.GetString("events_" + pclass.ToLowerInvariant()));
                }
            }
            catch (Exception ex)
            {
                BasicTeraData.LogError(ex.Message, true);
            }
            var windowFile = Path.Combine(_basicData.ResourceDirectory, "config/events/events-common.xml");
            XDocument xml;

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
                _filestreamCommon = new FileStream(windowFile, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
                xml = XDocument.Load(_filestreamCommon);
            }
            catch (Exception ex) when (ex is XmlException || ex is InvalidOperationException)
            {
                BasicTeraData.LogError(ex.Message, true, true);
                Save();
                _filestreamCommon = new FileStream(windowFile, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
                return;
            }
            catch (Exception ex)
            {
                BasicTeraData.LogError(ex.Message, true, true);
                return;
            }
            Events = new Dictionary<Event, List<Actions.Action>>();
            EventsCommon = new Dictionary<Event, List<Actions.Action>>();
            ParseAbnormalities(EventsCommon, xml);
            ParseCommonAFK(EventsCommon, xml);
        }

        public void Save()
        {
            //TODO, too lazy atm
            /*
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
            */
        }

        private void ParseCommonAFK(Dictionary<Event, List<Actions.Action>> events, XDocument xml)
        {
            var root = xml.Root;
            var default_active = root.Element("events")?.Attribute("active")?.Value ?? "True";
            var commonAfk = root.Element("common_afk");
            if (commonAfk == null) { return; }

            var active = bool.Parse(commonAfk.Attribute("active")?.Value ?? default_active);
            var ev = new CommonAFKEvent(active);
            events.Add(ev, new List<Actions.Action>());
            ParseActions(commonAfk, events, ev);
        }


        private void ParseActions(XElement root, Dictionary<Event, List<Actions.Action>> events, Event ev)
        {
            foreach (var notify in root.Element("actions").Elements("notify"))
            {
                Balloon ballonData = null;
                var balloon = notify.Element("balloon");
                if (balloon != null)
                {
                    var titleText = balloon.Attribute("title_text").Value;
                    var bodyText = balloon.Attribute("body_text").Value;
                    var displayDuration = int.Parse(balloon.Attribute("display_time").Value);
                    ballonData = new Balloon(titleText, bodyText, displayDuration);
                }
                
                SoundInterface soundInterface = null;
                var music = notify.Element("music");
                var beeps = notify.Element("beeps");
                var textToSpeech = notify.Element("text_to_speech");
                if ((music != null && beeps != null) || (music != null && textToSpeech != null) || (textToSpeech != null && beeps != null))
                {
                    throw new Exception("Only 1 type of sound allowed by notifyAction");
                }
                if (music != null)
                {
                    var musicFile = music.Attribute("file").Value;
                    var volume = float.Parse(music.Attribute("volume").Value);
                    var duration = int.Parse(music.Attribute("duration").Value);
                    soundInterface = new Music(musicFile, volume, duration);
                }
                if (beeps != null)
                {
                    var beepsList = new List<Beep>();
                    foreach (var beep in beeps.Elements())
                    {
                        var frequency = int.Parse(beep.Attribute("frequency").Value);
                        var duration = int.Parse(beep.Attribute("duration").Value);
                        beepsList.Add(new Beep(frequency, duration));
                    }
                    soundInterface = new Beeps(beepsList);
                }

                if (textToSpeech != null)
                {
                    var text = textToSpeech.Attribute("text").Value;
                    var voiceGender = (VoiceGender)Enum.Parse(typeof(VoiceGender), textToSpeech.Attribute("voice_gender")?.Value ?? "Female", true);
                    var voiceAge = (VoiceAge)Enum.Parse(typeof(VoiceAge), textToSpeech.Attribute("voice_age")?.Value ?? "Adult", true);
                    var culture = textToSpeech.Attribute("culture")?.Value ?? "en-US";
                    var voicePosition = int.Parse(textToSpeech.Attribute("voice_position")?.Value ?? "0");
                    var volume = int.Parse(textToSpeech.Attribute("volume")?.Value ?? "30");
                    var rate = int.Parse(textToSpeech.Attribute("rate")?.Value ?? "0");
                    soundInterface = new TextToSpeech(text, voiceGender, voiceAge, voicePosition, culture, volume, rate);
                }
                
                var notifyAction = new NotifyAction(soundInterface, ballonData);
                events[ev].Add(notifyAction);
            }
        }

        private void ParseAbnormalities(Dictionary<Event, List<Actions.Action>> events, XDocument xml)
        {
            var root = xml.Root;
            var default_active = root.Element("events")?.Attribute("active")?.Value ?? "True";
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
                var active = bool.Parse(abnormality.Attribute("active")?.Value ?? default_active);
                AbnormalityTargetType target;
                AbnormalityTriggerType trigger;
                Enum.TryParse(abnormality.Attribute("target").Value, true, out target);
                Enum.TryParse(abnormality.Attribute("trigger").Value, true, out trigger);
                var remainingSecondsBeforeTrigger = 0;
                var rewarnTimeoutSeconds = 0;
                if (trigger == AbnormalityTriggerType.MissingDuringFight)
                {
                    remainingSecondsBeforeTrigger = int.Parse(abnormality.Attribute("remaining_seconds_before_trigger").Value);
                    rewarnTimeoutSeconds = int.Parse(abnormality.Attribute("rewarn_timeout_seconds")?.Value??"0");
                }
                var abnormalityEvent = new AbnormalityEvent(ingame, active, ids, types, target, trigger, remainingSecondsBeforeTrigger,rewarnTimeoutSeconds);
                events.Add(abnormalityEvent, new List<Actions.Action>());
                ParseActions(abnormality, events, abnormalityEvent);
            }
        }
    }
}