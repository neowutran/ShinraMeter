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
using System.Threading;

namespace Data
{
    public class EventsData
    {
       
        public Dictionary<Event, List<Actions.Action>> EventsCommon { get; private set; }
        public Dictionary<Event, List<Actions.Action>> EventsClass { get; private set; }
        public Dictionary<Event, List<Actions.Action>> Events { get; private set; }
      

        public void Load(PlayerClass playerClass)
        {

            var windowFile = Path.Combine(_basicData.ResourceDirectory, "config/events/events-"+playerClass.ToString().ToLowerInvariant()+".xml");
            XDocument xml;
            FileStream filestreamClass;
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
                filestreamClass = new FileStream(windowFile, FileMode.Open, FileAccess.ReadWrite);
                xml = XDocument.Load(filestreamClass);
            }
            catch (Exception ex) when (ex is XmlException || ex is InvalidOperationException)
            {
                BasicTeraData.LogError(ex.Message, true, true);
                Save();
                filestreamClass = new FileStream(windowFile, FileMode.Open, FileAccess.ReadWrite);
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
            ParseCooldown(EventsClass, xml);

            foreach (var e in EventsCommon)
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
            Events = new Dictionary<Event, List<Actions.Action>>();
            EventsCommon = new Dictionary<Event, List<Actions.Action>>();
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
            FileStream filestreamCommon;
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
                filestreamCommon = new FileStream(windowFile, FileMode.Open, FileAccess.ReadWrite);
                xml = XDocument.Load(filestreamCommon);
            }
            catch (Exception ex) when (ex is XmlException || ex is InvalidOperationException)
            {
                BasicTeraData.LogError(ex.Message, true, true);
                Save();
                filestreamCommon = new FileStream(windowFile, FileMode.Open, FileAccess.ReadWrite);
                return;
            }
            catch (Exception ex)
            {
                BasicTeraData.LogError(ex.Message, true, true);
                return;
            }
            ParseAbnormalities(EventsCommon, xml);
            ParseCooldown(EventsCommon, xml);
            ParseCommonAFK(EventsCommon, xml);
        }

        public void Save()
        {
            //TODO UI, too lazy atm
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
                var music = notify.Descendants("music");
                var beeps = notify.Descendants("beeps");
                var textToSpeech = notify.Descendants("text_to_speech");
                if ((music.Any() && beeps.Any()) || ((music.Any() && textToSpeech.Any()) || textToSpeech.Any() && beeps.Any()))
                {
                    throw new Exception("Only 1 type of sound allowed by notifyAction");
                }
                if (music.Any())
                {
                    var musicFile = music.First().Attribute("file").Value;
                    var volume = float.Parse(music.First().Attribute("volume").Value);
                    var duration = int.Parse(music.First().Attribute("duration").Value);
                    soundInterface = new Music(musicFile, volume, duration);
                }
                if (beeps.Any())
                {
                    var beepsList = new List<Beep>();
                    foreach (var beep in beeps.First().Elements())
                    {
                        var frequency = int.Parse(beep.Attribute("frequency").Value);
                        var duration = int.Parse(beep.Attribute("duration").Value);
                        beepsList.Add(new Beep(frequency, duration));
                    }
                    soundInterface = new Beeps(beepsList);
                }

                if (textToSpeech.Any())
                {
                    var tts = textToSpeech.First();
                    var text = tts.Attribute("text").Value;
                    var voiceGender = (VoiceGender)Enum.Parse(typeof(VoiceGender), tts.Attribute("voice_gender")?.Value ?? "Female", true);
                    var voiceAge = (VoiceAge)Enum.Parse(typeof(VoiceAge), tts.Attribute("voice_age")?.Value ?? "Adult", true);
                    
                    var culture = tts.Attribute("culture")?.Value ?? CultureInfo.CurrentUICulture.ToString();
                    var voicePosition = int.Parse(tts.Attribute("voice_position")?.Value ?? "0");
                    var volume = int.Parse(tts.Attribute("volume")?.Value ?? "30");
                    var rate = int.Parse(tts.Attribute("rate")?.Value ?? "0");
                    soundInterface = new TextToSpeech(text, voiceGender, voiceAge, voicePosition, culture, volume, rate);
                }
                
                var notifyAction = new NotifyAction(soundInterface, ballonData);
                events[ev].Add(notifyAction);
            }
        }

        private void ParseCooldown(Dictionary<Event, List<Actions.Action>> events, XDocument xml)
        {
            var root = xml.Root;
            var default_active = root.Element("events")?.Attribute("active")?.Value ?? "True";
            foreach (var abnormality in root.Elements("cooldown"))
            {
                var skillId = int.Parse(abnormality.Attribute("skill_id").Value);
                var onlyResetted = bool.Parse(abnormality.Attribute("only_resetted")?.Value??"True");
                var active = bool.Parse(abnormality.Attribute("active")?.Value ?? default_active);
                var ingame = bool.Parse(abnormality.Attribute("ingame").Value);
                var cooldownEvent = new CooldownEvent(ingame, active, skillId, onlyResetted);
                events.Add(cooldownEvent, new List<Actions.Action>());
                ParseActions(abnormality, events, cooldownEvent);
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
                var abnormalityEvent = new AbnormalityEvent(ingame,active, ids, types, target, trigger, remainingSecondsBeforeTrigger,rewarnTimeoutSeconds);
                events.Add(abnormalityEvent, new List<Actions.Action>());
                ParseActions(abnormality, events, abnormalityEvent);
            }
        }
    }
}