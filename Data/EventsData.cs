using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Speech.Synthesis;
using System.Xml;
using System.Xml.Linq;
using Data.Actions.Notify;
using Data.Actions.Notify.SoundElements;
using Data.Events;
using Data.Events.Abnormality;
using Lang;
using Tera.Game;
using Action = Data.Actions.Action;

namespace Data
{
    public class EventsData
    {
        private readonly BasicTeraData _basicData;

        public EventsData(BasicTeraData basicData)
        {
            _basicData = basicData;
            EventsCommon = new Dictionary<Event, List<Action>>();
            var eventsdir = Path.Combine(_basicData.ResourceDirectory, "config/events");
            try
            {
                Directory.CreateDirectory(eventsdir);
                foreach (var pclass in Enum.GetNames(typeof(PlayerClass)))
                {
                    var fname = Path.Combine(_basicData.ResourceDirectory,
                        "config/events/events-" + pclass.ToLowerInvariant() + ".xml");
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
                var filestreamCommon = new FileStream(windowFile, FileMode.Open, FileAccess.Read);
                xml = XDocument.Load(filestreamCommon);
            }
            catch (Exception ex) when (ex is XmlException || ex is InvalidOperationException)
            {
                BasicTeraData.LogError(ex.Message, true, true);
                Save();
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

        private Dictionary<Event, List<Action>> EventsCommon { get; }
        private Dictionary<Event, List<Action>> EventsClass { get; set; }
        public Dictionary<Event, List<Action>> MissingAbnormalities { get; private set; }
        public Dictionary<Event, List<Action>> AddedRemovedAbnormalities { get; private set; }
        public Dictionary<Event, List<Action>> Events { get; private set; }

        public Dictionary<Event, List<Action>> Cooldown { get; private set; }
        public Tuple<Event, List<Action>> AFK { get; private set; }


        public void Load(PlayerClass playerClass)
        {
            var windowFile = Path.Combine(_basicData.ResourceDirectory,
                "config/events/events-" + playerClass.ToString().ToLowerInvariant() + ".xml");
            XDocument xml;
            try
            {
                var filestreamClass = new FileStream(windowFile, FileMode.Open, FileAccess.Read);
                xml = XDocument.Load(filestreamClass);
            }
            catch (Exception ex) when (ex is XmlException || ex is InvalidOperationException)
            {
                BasicTeraData.LogError(ex.Message, true, true);
                Save();
                return;
            }
            catch (Exception ex)
            {
                BasicTeraData.LogError(ex.Message, true, true);
                return;
            }
            EventsClass = new Dictionary<Event, List<Action>>();
            MissingAbnormalities = new Dictionary<Event, List<Action>>();
            AddedRemovedAbnormalities = new Dictionary<Event, List<Action>>();
            Cooldown = new Dictionary<Event, List<Action>>();
            Events = new Dictionary<Event, List<Action>>();
            AFK = null;
            ParseAbnormalities(EventsClass, xml);
            ParseCooldown(EventsClass, xml);
            AssociateEvent(EventsCommon);
            AssociateEvent(EventsClass);
        }

        private void AssociateEvent(Dictionary<Event, List<Action>> rootEvents)
        {
            foreach (var e in rootEvents)
            {
                if (!e.Key.Active) continue;
                Events.Add(e.Key, e.Value);
                var evAbnormalities = e.Key as AbnormalityEvent;
                if (evAbnormalities != null)
                    if (evAbnormalities.Trigger == AbnormalityTriggerType.MissingDuringFight ||
                        evAbnormalities.Trigger == AbnormalityTriggerType.Ending)
                        MissingAbnormalities.Add(e.Key, e.Value);
                    else
                        AddedRemovedAbnormalities.Add(e.Key, e.Value);

                var evCooldown = e.Key as CooldownEvent;
                if (evCooldown != null)
                    Cooldown.Add(e.Key, e.Value);

                var evAFK = e.Key as CommonAFKEvent;
                if (evAFK != null)
                    AFK = new Tuple<Event, List<Action>>(e.Key, e.Value);
            }
        }

        public void Save()
        {
            //TODO UI, too lazy atm
        }

        private void ParseCommonAFK(Dictionary<Event, List<Action>> events, XDocument xml)
        {
            var root = xml.Root;
            var default_active = root.Attribute("active")?.Value ?? "True";
            var commonAfk = root.Element("common_afk");
            if (commonAfk == null) return;

            var active = bool.Parse(commonAfk.Attribute("active")?.Value ?? default_active);
            var ev = new CommonAFKEvent(active);
            events.Add(ev, new List<Action>());
            ParseActions(commonAfk, events, ev);
        }


        private void ParseActions(XElement root, Dictionary<Event, List<Action>> events, Event ev)
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
                if (music.Any() && beeps.Any() || music.Any() && textToSpeech.Any() ||
                    textToSpeech.Any() && beeps.Any())
                    throw new Exception("Only 1 type of sound allowed by notifyAction");
                if (music.Any())
                {
                    var musicFile = music.First().Attribute("file").Value;
                    var volume = float.Parse(music.First().Attribute("volume").Value, CultureInfo.InvariantCulture) /
                                 100;
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
                    var voiceGender = (VoiceGender) Enum.Parse(typeof(VoiceGender),
                        tts.Attribute("voice_gender")?.Value ?? "Female", true);
                    var voiceAge = (VoiceAge) Enum.Parse(typeof(VoiceAge), tts.Attribute("voice_age")?.Value ?? "Adult",
                        true);

                    var culture = tts.Attribute("culture")?.Value ?? LP.Culture.ToString();
                    var voicePosition = int.Parse(tts.Attribute("voice_position")?.Value ?? "0");
                    var volume = int.Parse(tts.Attribute("volume")?.Value ?? "30");
                    var rate = int.Parse(tts.Attribute("rate")?.Value ?? "0");
                    soundInterface = new TextToSpeech(text, voiceGender, voiceAge, voicePosition, culture, volume,
                        rate);
                }

                var notifyAction = new NotifyAction(soundInterface, ballonData);
                events[ev].Add(notifyAction);
            }
        }

        private List<BlackListItem> ParseAreaBossBlackList(XElement root)
        {
            var areaBossBlacklist = new List<BlackListItem>();
            if (root.Element("area_boss_blacklist") == null)
                return areaBossBlacklist;
            foreach (var blacklist in root.Element("area_boss_blacklist").Elements("blacklist"))
            {
                var areaId = int.Parse(blacklist.Attribute("area_id").Value);
                var bossId = int.Parse(blacklist.Attribute("boss_id")?.Value ?? "-1");
                areaBossBlacklist.Add(new BlackListItem {AreaId = areaId, BossId = bossId});
            }
            return areaBossBlacklist;
        }

        private void ParseCooldown(Dictionary<Event, List<Action>> events, XDocument xml)
        {
            var root = xml.Root;
            var default_active = root.Attribute("active")?.Value ?? "True";
            var default_priority = root.Attribute("priority")?.Value ?? "5";
            foreach (var abnormality in root.Elements("cooldown"))
            {
                var skillId = int.Parse(abnormality.Attribute("skill_id").Value);
                var onlyResetted = bool.Parse(abnormality.Attribute("only_resetted")?.Value ?? "True");
                var active = bool.Parse(abnormality.Attribute("active")?.Value ?? default_active);
                var ingame = bool.Parse(abnormality.Attribute("ingame").Value);
                var priority = int.Parse(abnormality.Attribute("priority")?.Value ?? default_priority);
                ParseAreaBossBlackList(abnormality);
                var cooldownEvent = new CooldownEvent(ingame, active, priority, skillId, onlyResetted);
                events.Add(cooldownEvent, new List<Action>());
                ParseActions(abnormality, events, cooldownEvent);
            }
        }

        private void ParseAbnormalities(Dictionary<Event, List<Action>> events, XDocument xml)
        {
            var root = xml.Root;
            var default_active = root.Attribute("active")?.Value ?? "True";
            var default_priority = root.Attribute("priority")?.Value ?? "5";
            var default_blacklist = ParseAreaBossBlackList(root);
            foreach (var abnormality in root.Elements("abnormality"))
            {
                var ids = new Dictionary<int, int>();
                var types = new List<HotDot.Types>();
                var abnormalities = abnormality.Element("abnormalities");
                foreach (var abnormalityId in abnormalities.Elements("abnormality"))
                {
                    var idElement = abnormalityId.Value;
                    int id;

                    if (int.TryParse(idElement, out id))
                    {
                        var stack = int.Parse(abnormalityId.Attribute("stack")?.Value ?? "0");
                        ids[id] = stack;
                        continue;
                    }
                    HotDot.Types type;
                    if (!Enum.TryParse(idElement, true, out type))
                        throw new Exception(idElement + " is not an acceptable value.");
                    types.Add(type);
                }
                var ingame = bool.Parse(abnormality.Attribute("ingame").Value);
                var active = bool.Parse(abnormality.Attribute("active")?.Value ?? default_active);
                var priority = int.Parse(abnormality.Attribute("priority")?.Value ?? default_priority);
                AbnormalityTargetType target;
                AbnormalityTriggerType trigger;
                Enum.TryParse(abnormality.Attribute("target").Value, true, out target);
                Enum.TryParse(abnormality.Attribute("trigger").Value, true, out trigger);
                var remainingSecondsBeforeTrigger = 0;
                var rewarnTimeoutSeconds = 0;
                if (trigger == AbnormalityTriggerType.MissingDuringFight || trigger == AbnormalityTriggerType.Ending)
                {
                    remainingSecondsBeforeTrigger =
                        int.Parse(abnormality.Attribute("remaining_seconds_before_trigger").Value);
                    rewarnTimeoutSeconds = int.Parse(abnormality.Attribute("rewarn_timeout_seconds")?.Value ?? "0");
                }
                var blacklist = ParseAreaBossBlackList(abnormality);
                var abnormalityEvent = new AbnormalityEvent(ingame, active, priority,
                    blacklist.Any() ? blacklist : default_blacklist, ids, types, target, trigger,
                    remainingSecondsBeforeTrigger, rewarnTimeoutSeconds);
                events.Add(abnormalityEvent, new List<Action>());
                ParseActions(abnormality, events, abnormalityEvent);
            }
        }
    }
}