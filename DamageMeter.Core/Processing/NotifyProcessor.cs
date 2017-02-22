using Lang;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Data;
using Tera.Game;
using Tera.Game.Messages;
using static Tera.Game.HotDotDatabase;
using Data.Events.Abnormality;
using Data.Events;
using Data.Actions.Notify;
using Data.Actions.Notify.SoundElements;

namespace DamageMeter.Processing
{
    internal class NotifyProcessor
    {

        private NotifyProcessor()
        {

        }
        private static NotifyProcessor _instance;

        public static NotifyProcessor Instance => _instance ?? (_instance = new NotifyProcessor());

        internal void InstanceMatchingSuccess(Tera.Game.Messages.S_FIN_INTER_PARTY_MATCH message)
        {
            MatchingSuccess();
        }

        internal void InstanceMatchingSuccess(Tera.Game.Messages.S_BATTLE_FIELD_ENTRANCE_INFO message)
        {
            MatchingSuccess();
        }

        internal NotifyFlashMessage DefaultNotifyAction(string titleText, string bodyText)
        {
            var ev = BasicTeraData.Instance.EventsData.AFK;
            
            if(ev?.Item1 is CommonAFKEvent)
            {
                foreach (var action in ev.Item2)
                {
                    if(action is NotifyAction)
                    {
                        var notifyAction = ((NotifyAction)action).Clone();
                        notifyAction.Balloon.BodyText = notifyAction.Balloon.BodyText.Replace("{afk_body}", bodyText);
                        notifyAction.Balloon.TitleText = notifyAction.Balloon.TitleText.Replace("{afk_title}", titleText);


                        if(notifyAction.Sound != null && notifyAction.Sound.GetType() == typeof(TextToSpeech))
                        {
                            var tts = (TextToSpeech)notifyAction.Sound;
                            tts.Text = tts.Text.Replace("{afk_body}", bodyText);
                            tts.Text = tts.Text.Replace("{afk_title}", titleText);
                        }

                        return new NotifyFlashMessage(notifyAction.Sound, notifyAction.Balloon, ev.Item1.Priority);
                    }
                }
            }
            
            return null;     
        }
        private void MatchingSuccess()
        {
            if (BasicTeraData.Instance.WindowData.ShowAfkEventsIngame || !TeraWindow.IsTeraActive())
            {
                NetworkController.Instance.FlashMessage = DefaultNotifyAction(
                    LP.PartyMatchingSuccess,
                    LP.PartyMatchingSuccess
                    );
            }
        }

        internal void S_CHECK_TO_READY_PARTY(Tera.Game.Messages.S_CHECK_TO_READY_PARTY message)
        {
            if (message.Count == 1 && (BasicTeraData.Instance.WindowData.ShowAfkEventsIngame || !TeraWindow.IsTeraActive()))
            {    
                NetworkController.Instance.FlashMessage = DefaultNotifyAction(
                    LP.CombatReadyCheck,
                    LP.CombatReadyCheck
                    );
            }
        }

        internal void S_OTHER_USER_APPLY_PARTY(Tera.Game.Messages.S_OTHER_USER_APPLY_PARTY message)
        {
            if (BasicTeraData.Instance.WindowData.ShowAfkEventsIngame || !TeraWindow.IsTeraActive())
            {

                NetworkController.Instance.FlashMessage = DefaultNotifyAction(
                    message.PlayerName + " " + LP.ApplyToYourParty,
                    LP.Class + ": " +
                    LP.ResourceManager.GetString(message.PlayerClass.ToString(), LP.Culture) +
                    Environment.NewLine +
                    LP.Lvl + ": " + message.Lvl + Environment.NewLine
                    );
            }

            if (BasicTeraData.Instance.WindowData.CopyInspect)
            {
                var thread = new Thread(() => CopyPaste.CopyInspect(message.PlayerName));
                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();
            }
        }

        internal void S_TRADE_BROKER_DEAL_SUGGESTED(Tera.Game.Messages.S_TRADE_BROKER_DEAL_SUGGESTED message)
        {
            if (BasicTeraData.Instance.WindowData.ShowAfkEventsIngame || !TeraWindow.IsTeraActive())
            {
                NetworkController.Instance.FlashMessage = DefaultNotifyAction(
                    LP.Trading + ": " + message.PlayerName,
                    LP.SellerPrice + ": " + Tera.Game.Messages.S_TRADE_BROKER_DEAL_SUGGESTED.Gold(message.SellerPrice) +
                    Environment.NewLine +
                    LP.OfferedPrice + ": " + Tera.Game.Messages.S_TRADE_BROKER_DEAL_SUGGESTED.Gold(message.OfferedPrice)
                    );
            }
        }

        internal void S_REQUEST_CONTRACT(Tera.Game.Messages.S_REQUEST_CONTRACT message)
        {
            if (BasicTeraData.Instance.WindowData.ShowAfkEventsIngame || !TeraWindow.IsTeraActive())
            {
                if (message.Type == Tera.Game.Messages.S_REQUEST_CONTRACT.RequestType.PartyInvite)
                {
                    NetworkController.Instance.FlashMessage = DefaultNotifyAction(
                        LP.PartyInvite + ": " + message.Sender,
                        message.Sender
                        );
                }
                else if (message.Type == Tera.Game.Messages.S_REQUEST_CONTRACT.RequestType.TradeRequest)
                {
                    NetworkController.Instance.FlashMessage = DefaultNotifyAction(
                        LP.Trading + ": " + message.Sender,
                        message.Sender
                        );
                }
                else if (!Enum.IsDefined(typeof(Tera.Game.Messages.S_REQUEST_CONTRACT.RequestType),(int)message.Type))
                
                {
                    NetworkController.Instance.FlashMessage = DefaultNotifyAction(
                        LP.ContactTry,
                        LP.ContactTry
                        );
                }
            }
        }

        internal void AbnormalityNotifierMissing()
        {
            if (!BasicTeraData.Instance.WindowData.EnableChat) return;
            var meterUser = NetworkController.Instance.EntityTracker.MeterUser;
            var bossIds = _lastBosses.Where(x => x.Value > 0).Select(x => x.Key).ToList();
            if (meterUser == null || !bossIds.Any()) return;
            if (NetworkController.Instance.AbnormalityStorage.DeadOrJustResurrected(NetworkController.Instance.PlayerTracker.Me())) return;
            var teraActive = TeraWindow.IsTeraActive();
            var time = DateTime.Now;
            var bossList = bossIds.Select(x=>(NpcEntity)NetworkController.Instance.EntityTracker.GetOrNull(x)).ToList();

            foreach (var e in BasicTeraData.Instance.EventsData.MissingAbnormalities)
            {
                List<EntityId> entitiesIdToCheck = new List<EntityId>();
                if (NetworkController.Instance.FlashMessage != null && NetworkController.Instance.FlashMessage.Priority >= e.Key.Priority) { continue; }
                var abnormalityEvent = (AbnormalityEvent)e.Key;
                if (abnormalityEvent.InGame != teraActive) continue;
                if (bossList.Any(x=>
                    x!=null && e.Key.AreaBossBlackList.ContainsKey(x.Info.HuntingZoneId)&&(e.Key.AreaBossBlackList[x.Info.HuntingZoneId]==-1 || e.Key.AreaBossBlackList[x.Info.HuntingZoneId]==x.Info.TemplateId)
                    )) continue;
                if (abnormalityEvent.Target == AbnormalityTargetType.Self) { entitiesIdToCheck.Add(meterUser.Id); }
                if (abnormalityEvent.Target == AbnormalityTargetType.Boss) { entitiesIdToCheck.AddRange(bossIds); }
                if (abnormalityEvent.Target == AbnormalityTargetType.MyBoss)
                {
                    if (_lastBossMeterUser == null || _lastBossHpMeterUser == 0) { continue; }
                    entitiesIdToCheck.Add(_lastBossMeterUser.Value);
                }
                if ((abnormalityEvent.Target == AbnormalityTargetType.Party || abnormalityEvent.Target == AbnormalityTargetType.PartySelfExcluded) && BasicTeraData.Instance.WindowData.DisablePartyEvent) continue;
                if (abnormalityEvent.Target == AbnormalityTargetType.Party)
                {
                    foreach (var player in NetworkController.Instance.PlayerTracker.PartyList())
                    {
                        if (player.OutOfRange) { continue; }
                        if (NetworkController.Instance.AbnormalityStorage.DeadOrJustResurrected(NetworkController.Instance.PlayerTracker.GetOrUpdate(player))) { continue; }
                        entitiesIdToCheck.Add(player.Id);
                    }
                }
                if (abnormalityEvent.Target == AbnormalityTargetType.PartySelfExcluded)
                {
                    foreach (var player in NetworkController.Instance.PlayerTracker.PartyList())
                    {
                        if (player == meterUser) { continue; }
                        if (player.OutOfRange) { continue; }
                        if (NetworkController.Instance.AbnormalityStorage.DeadOrJustResurrected(NetworkController.Instance.PlayerTracker.GetOrUpdate(player))) { continue; }
                        entitiesIdToCheck.Add(player.Id);
                    }
                }


                foreach (var entityIdToCheck in entitiesIdToCheck)
                {
                    if (e.Key.NextChecks.ContainsKey(entityIdToCheck) && time < e.Key.NextChecks[entityIdToCheck]) continue;

                    TimeSpan? abnormalityTimeLeft = null;
                    var noAbnormalitiesMissing = false;

                    foreach (var id in abnormalityEvent.Ids)
                    {

                        var timeLeft = NetworkController.Instance.AbnormalityTracker.AbnormalityTimeLeft(entityIdToCheck, id.Key, id.Value);
                        if (timeLeft >= abnormalityEvent.RemainingSecondBeforeTrigger * TimeSpan.TicksPerSecond)
                        {
                            noAbnormalitiesMissing = true;
                            break;
                        }
                        if (timeLeft != -1 && ((abnormalityTimeLeft != null && timeLeft > abnormalityTimeLeft.Value.Ticks) || abnormalityTimeLeft == null))
                        {
                            abnormalityTimeLeft = TimeSpan.FromTicks(timeLeft);
                        }
                    }

                    if (noAbnormalitiesMissing) continue;

                    foreach (var type in abnormalityEvent.Types)
                    {
                        var timeLeft = NetworkController.Instance.AbnormalityTracker.AbnormalityTimeLeft(entityIdToCheck, type);
                        if (timeLeft >= abnormalityEvent.RemainingSecondBeforeTrigger * TimeSpan.TicksPerSecond)
                        {
                            noAbnormalitiesMissing = true;
                            break;
                        }
                        if (timeLeft != -1 && ((abnormalityTimeLeft != null && timeLeft > abnormalityTimeLeft.Value.Ticks) || abnormalityTimeLeft == null))
                        {
                            abnormalityTimeLeft = TimeSpan.FromTicks(timeLeft);
                        }
                    }

                    if (noAbnormalitiesMissing) continue;
                    if (abnormalityEvent.Trigger == AbnormalityTriggerType.Ending && (abnormalityTimeLeft == null || abnormalityTimeLeft.Value.Ticks <= 0)) continue;
                        
                    abnormalityEvent.NextChecks[entityIdToCheck] = time.AddSeconds(abnormalityEvent.RewarnTimeoutSeconds);

                    foreach (var a in e.Value)
                    {
                        if (a.GetType() != typeof(NotifyAction)) { continue; }
                        var notifyAction = ((NotifyAction)a).Clone();
                        var player = NetworkController.Instance.EntityTracker.GetOrNull(entityIdToCheck) as UserEntity;
                        if (notifyAction.Sound != null && notifyAction.Sound.GetType() == typeof(TextToSpeech))
                        {
                            var textToSpeech = (TextToSpeech)notifyAction.Sound;
                            if (player != null) { textToSpeech.Text = textToSpeech.Text.Replace("{player_name}", player.Name); }

                            if (abnormalityEvent.Ids.Count > 0)
                            {
                                var abName = BasicTeraData.Instance.HotDotDatabase.Get(abnormalityEvent.Ids.First().Key).Name;
                                textToSpeech.Text = textToSpeech.Text.Replace("{abnormality_name}", abName);
                            }
                            else
                            {
                                textToSpeech.Text = textToSpeech.Text.Replace("{abnormality_name}", LP.NoCrystalBind);
                            }

                            if (abnormalityTimeLeft.HasValue) 
                                textToSpeech.Text = textToSpeech.Text.Replace("{time_left}", abnormalityTimeLeft.Value.Seconds.ToString());
                            else
                                textToSpeech.Text = textToSpeech.Text.Replace("{time_left}", "");
                        }

                        if (notifyAction.Balloon != null)
                        {
                            if (abnormalityEvent.Ids.Count > 0)
                            {
                                var abName = BasicTeraData.Instance.HotDotDatabase.Get(abnormalityEvent.Ids.First().Key).Name;
                                notifyAction.Balloon.BodyText = notifyAction.Balloon.BodyText.Replace("{abnormality_name}", abName);
                                notifyAction.Balloon.TitleText = notifyAction.Balloon.TitleText.Replace("{abnormality_name}", abName);
                            }
                            else
                            {
                                notifyAction.Balloon.BodyText = notifyAction.Balloon.BodyText.Replace("{abnormality_name}", LP.NoCrystalBind);
                                notifyAction.Balloon.TitleText = notifyAction.Balloon.TitleText.Replace("{abnormality_name}", LP.NoCrystalBind);
                            }
                            notifyAction.Balloon.BodyText = notifyAction.Balloon.BodyText.Replace("{player_name}", player?.Name);
                            notifyAction.Balloon.TitleText = notifyAction.Balloon.TitleText.Replace("{player_name}", player?.Name);

                            if (abnormalityTimeLeft.HasValue)
                            {
                                notifyAction.Balloon.TitleText = notifyAction.Balloon.TitleText.Replace("{time_left}", abnormalityTimeLeft.Value.ToString(@"hh\:mm\:ss", CultureInfo.InvariantCulture));
                                notifyAction.Balloon.BodyText = notifyAction.Balloon.BodyText.Replace("{time_left}", abnormalityTimeLeft.Value.ToString(@"hh\:mm\:ss", CultureInfo.InvariantCulture));
                            }
                            else
                            {
                                notifyAction.Balloon.TitleText = notifyAction.Balloon.TitleText.Replace("{time_left}", "");
                                notifyAction.Balloon.BodyText = notifyAction.Balloon.BodyText.Replace("{time_left}", "");
                            }

                        }
                        NetworkController.Instance.FlashMessage = new NotifyFlashMessage(notifyAction.Sound, notifyAction.Balloon, e.Key.Priority);
                    }
                    break;
                }
            }
        }

        internal void UpdateMeterBoss(Tera.Game.Messages.EachSkillResultServerMessage message)
        {
            var source = NetworkController.Instance.EntityTracker.GetOrNull(message.Source) as UserEntity;
            if (NetworkController.Instance.EntityTracker.MeterUser != source) { return; }
            var target = NetworkController.Instance.EntityTracker.GetOrNull(message.Target) as NpcEntity;
            if(target == null) { return; }
            if (target.Info.Boss)
            {
                _lastBossMeterUser = target.Id;
            }
        }

        internal void AbnormalityNotifierAdded(EntityId target, int abnormalityId, int stack)
        {
            AbnormalityNotifierCommon(target, abnormalityId, AbnormalityTriggerType.Added, stack);
        }

        internal void AbnormalityNotifierRemoved(EntityId target, int abnormalityId, int stack)
        {
            AbnormalityNotifierCommon(target, abnormalityId, AbnormalityTriggerType.Removed, stack);
        }

        internal void SkillReset(int skillId, CrestType type)
        {
            if (type != CrestType.Reset) return;
            var meterUser = NetworkController.Instance.EntityTracker.MeterUser;
            var bossIds = _lastBosses.Where(x => x.Value > 0).Select(x => x.Key).ToList();
            if (meterUser == null || !bossIds.Any()) return;
            var teraActive = TeraWindow.IsTeraActive();
            var bossList = bossIds.Select(x => (NpcEntity)NetworkController.Instance.EntityTracker.GetOrNull(x)).ToList();

            foreach (var e in BasicTeraData.Instance.EventsData.Cooldown)
            {
                if (NetworkController.Instance.FlashMessage != null && NetworkController.Instance.FlashMessage.Priority >= e.Key.Priority) { continue; }
                if (e.Key.InGame != teraActive) continue;
                if (bossList.Any(x =>
                    x != null && e.Key.AreaBossBlackList.ContainsKey(x.Info.HuntingZoneId) && (e.Key.AreaBossBlackList[x.Info.HuntingZoneId] == -1 || e.Key.AreaBossBlackList[x.Info.HuntingZoneId] == x.Info.TemplateId)
                    )) continue; //not sure, whether blacklist check is needed for skillreset event at all
                var cooldownEvent = (CooldownEvent)e.Key;
                if(cooldownEvent.SkillId != skillId) { continue; }

                foreach (var a in e.Value)
                {
                    if (a.GetType() != typeof(NotifyAction)) { continue; }
                    var notifyAction = ((NotifyAction)a).Clone();
                    var skill = BasicTeraData.Instance.SkillDatabase.GetOrNull(meterUser, skillId);
                    if (notifyAction.Balloon != null)
                    {
                        notifyAction.Balloon.BodyText = notifyAction.Balloon.BodyText.Replace("{skill_name}", skill?.Name ?? skillId.ToString());
                        notifyAction.Balloon.TitleText = notifyAction.Balloon.TitleText.Replace("{skill_name}", skill?.Name ?? skillId.ToString());
                    }
                    if (notifyAction.Sound != null && notifyAction.Sound.GetType() == typeof(TextToSpeech))
                    {
                        var textToSpeech = (TextToSpeech)notifyAction.Sound;
                        textToSpeech.Text = textToSpeech.Text.Replace("{skill_name}", skill?.Name ?? skillId.ToString());
                    }
                    NetworkController.Instance.FlashMessage = new NotifyFlashMessage(notifyAction.Sound, notifyAction.Balloon, e.Key.Priority);
                }
            }
        }
        

        private void AbnormalityNotifierCommon(EntityId target, int abnormalityId, AbnormalityTriggerType trigger,int stack)
        {
            var meterUser = NetworkController.Instance.EntityTracker.MeterUser;
            var bossIds = _lastBosses.Where(x => x.Value > 0).Select(x => x.Key).ToList();
            if (meterUser == null || !bossIds.Any()) return;
            var teraActive = TeraWindow.IsTeraActive();
            var bossList = bossIds.Select(x => (NpcEntity)NetworkController.Instance.EntityTracker.GetOrNull(x)).ToList();

            foreach (var e in BasicTeraData.Instance.EventsData.AddedRemovedAbnormalities)
            {
                UserEntity player = meterUser;
                if (NetworkController.Instance.FlashMessage != null && NetworkController.Instance.FlashMessage.Priority > e.Key.Priority) { continue; }
                var abnormalityEvent = (AbnormalityEvent)e.Key;
                if (abnormalityEvent.InGame != teraActive) continue;
                if (abnormalityEvent.Trigger != trigger) continue;
                if (!abnormalityEvent.Ids.ContainsKey(abnormalityId)) continue;
                if (abnormalityEvent.Ids[abnormalityId] > stack) continue;
                if (bossList.Any(x =>
                    x != null && e.Key.AreaBossBlackList.ContainsKey(x.Info.HuntingZoneId) && (e.Key.AreaBossBlackList[x.Info.HuntingZoneId] == -1 || e.Key.AreaBossBlackList[x.Info.HuntingZoneId] == x.Info.TemplateId)
                    )) continue; 
                if (abnormalityEvent.Target == AbnormalityTargetType.Boss && !bossIds.Contains(target))  continue;
                if(abnormalityEvent.Target == AbnormalityTargetType.MyBoss && _lastBossMeterUser != target) continue;
                if(abnormalityEvent.Target == AbnormalityTargetType.Self && meterUser.Id != target) continue;
                if ((abnormalityEvent.Target == AbnormalityTargetType.Party || abnormalityEvent.Target == AbnormalityTargetType.PartySelfExcluded) && BasicTeraData.Instance.WindowData.DisablePartyEvent) continue;
                if(abnormalityEvent.Target == AbnormalityTargetType.Party)
                {
                    player = NetworkController.Instance.EntityTracker.GetOrNull(target) as UserEntity;
                    if (player == null || !NetworkController.Instance.PlayerTracker.PartyList().Contains(player)) continue;
                }
                if (abnormalityEvent.Target == AbnormalityTargetType.PartySelfExcluded)
                {
                    player = NetworkController.Instance.EntityTracker.GetOrNull(target) as UserEntity;
                    if (player == null || !NetworkController.Instance.PlayerTracker.PartyList().Contains(player) || meterUser.Id == player.Id) continue;
                }

                if (player.OutOfRange) { continue; }

                foreach (var a in e.Value)
                {
                    if(a.GetType() != typeof(NotifyAction)) { continue; }
                    var notifyAction = ((NotifyAction)a).Clone();
                    var abnormality = BasicTeraData.Instance.HotDotDatabase.Get(abnormalityId);
                    if(notifyAction.Balloon != null)
                    {
                        notifyAction.Balloon.BodyText = notifyAction.Balloon.BodyText.Replace("{abnormality_name}", abnormality.Name);
                        notifyAction.Balloon.BodyText = notifyAction.Balloon.BodyText.Replace("{stack}", stack.ToString());
                        if (player != null)
                        {
                            notifyAction.Balloon.BodyText = notifyAction.Balloon.BodyText.Replace("{player_name}", player.Name);
                            notifyAction.Balloon.TitleText = notifyAction.Balloon.TitleText.Replace("{player_name}", player.Name);
                        }
                        notifyAction.Balloon.TitleText=  notifyAction.Balloon.TitleText.Replace("{abnormality_name}", abnormality.Name);
                        notifyAction.Balloon.TitleText = notifyAction.Balloon.TitleText.Replace("{stack}", stack.ToString());
                    }
                    if (notifyAction.Sound != null && notifyAction.Sound.GetType() == typeof(TextToSpeech))
                    {
                        var textToSpeech = (TextToSpeech)notifyAction.Sound;
                        if (player != null)
                        {
                            textToSpeech.Text = textToSpeech.Text.Replace("{player_name}", player.Name);
                        }
                        textToSpeech.Text = textToSpeech.Text.Replace("{abnormality_name}", abnormality.Name);
                        textToSpeech.Text = textToSpeech.Text.Replace("{stack}", stack.ToString());
                    }
                    NetworkController.Instance.FlashMessage = new NotifyFlashMessage(notifyAction.Sound, notifyAction.Balloon, e.Key.Priority);
                }
            }
        }
        private Dictionary<EntityId,long> _lastBosses = new Dictionary<EntityId, long>();
        private EntityId? _lastBossMeterUser;
        private long _lastBossHpMeterUser;
        internal void S_BOSS_GAGE_INFO(Tera.Game.Messages.S_BOSS_GAGE_INFO message)
        {
            NetworkController.Instance.EntityTracker.Update(message);
            long newHp = 0;
            if (message.TotalHp != message.HpRemaining)
            {
                newHp = (long)message.HpRemaining;
                if(message.EntityId == _lastBossMeterUser)
                {
                    _lastBossHpMeterUser = newHp;
                }
            }
            _lastBosses[message.EntityId] = newHp;
        }

        internal void SpawnMe(Tera.Game.Messages.SpawnMeServerMessage message)
        {
            S_SPAWN_ME.Process(message);
            _lastBosses=new Dictionary<EntityId, long>();
            _lastBossMeterUser = null;
            _lastBossHpMeterUser = 0;
            foreach (var e in BasicTeraData.Instance.EventsData.MissingAbnormalities.Keys)
            {
                e.NextChecks=new Dictionary<EntityId, DateTime>();
            }
        }

        internal void S_LOAD_TOPO(Tera.Game.Messages.S_LOAD_TOPO message)
        {
            _lastBosses = new Dictionary<EntityId, long>();
            _lastBossMeterUser = null;
            _lastBossHpMeterUser = 0;
            foreach (var e in BasicTeraData.Instance.EventsData.MissingAbnormalities.Keys)
            {
                e.NextChecks = new Dictionary<EntityId, DateTime>();
            }
        }

        internal void SpawnUser(Tera.Game.Messages.SpawnUserServerMessage message)
        {
            foreach (var e in BasicTeraData.Instance.EventsData.MissingAbnormalities.Keys)
            {
                e.NextChecks[message.Id] = DateTime.UtcNow.AddSeconds(5);
            }
        }

        internal void DespawnNpc(Tera.Game.Messages.SDespawnNpc message)
        {
            if (_lastBosses.ContainsKey(message.Npc)) _lastBosses.Remove(message.Npc);
            if(message.Npc == _lastBossMeterUser)
            {
                _lastBossMeterUser = null;
                _lastBossHpMeterUser = 0;
            }
        }

        internal void S_BEGIN_THROUGH_ARBITER_CONTRACT(S_BEGIN_THROUGH_ARBITER_CONTRACT message)
        {
            if (message.PlayerName.StartsWith("Error")) BasicTeraData.LogError(message.PlayerName);
        }

        internal void UpdateCredits(S_UPDATE_NPCGUILD.NpcGuildType type, int credits)
        {
            if (type == S_UPDATE_NPCGUILD.NpcGuildType.Vanguard && credits >= 8500)
            {
                NetworkController.Instance.FlashMessage = DefaultNotifyAction(
                    LP.VanguardCredits+credits,
                    LP.VanguardCredits+credits
                    );
            } //todo: not sure, may be it worth adding StrSheet_NPCGuild.xml parser and use region-specific names instead of langpack
        }

        internal void UpdateCredits(Tera.Game.Messages.S_AVAILABLE_EVENT_MATCHING_LIST message)
        {
            UpdateCredits(S_UPDATE_NPCGUILD.NpcGuildType.Vanguard, message.Credits);
        }

        internal void UpdateCredits(Tera.Game.Messages.S_UPDATE_NPCGUILD message)
        {
            UpdateCredits(message.Type, message.Credits);
        }
    }
}
