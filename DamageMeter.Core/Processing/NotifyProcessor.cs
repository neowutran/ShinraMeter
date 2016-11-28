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
    class NotifyProcessor
    {
        internal static void InstanceMatchingSuccess(Tera.Game.Messages.S_FIN_INTER_PARTY_MATCH message)
        {
            Process();
        }

        internal static void InstanceMatchingSuccess(Tera.Game.Messages.S_BATTLE_FIELD_ENTRANCE_INFO message)
        {
            Process();
        }

        internal static NotifyAction DefaultNotifyAction(string titleText, string bodyText)
        {
            foreach(var ev in BasicTeraData.Instance.EventsData.Events)
            {
                if(ev.Key is CommonAFKEvent)
                {
                    if (!ev.Key.Active) continue;
                    foreach (var action in ev.Value)
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

                            return notifyAction;
                        }
                    }
                }
            }
            return null;     
        }
        private static void Process()
        {
            if (!TeraWindow.IsTeraActive())
            {
                NetworkController.Instance.FlashMessage = DefaultNotifyAction(
                    LP.PartyMatchingSuccess,
                    LP.PartyMatchingSuccess
                    );
            }
        }

        internal static void S_CHECK_TO_READY_PARTY(Tera.Game.Messages.S_CHECK_TO_READY_PARTY message)
        {
            if (message.Count == 1)
            {
                if (!TeraWindow.IsTeraActive())
                {
                    NetworkController.Instance.FlashMessage = DefaultNotifyAction(
                        LP.CombatReadyCheck,
                        LP.CombatReadyCheck
                        );
                }
            }
        }

        internal static void S_OTHER_USER_APPLY_PARTY(Tera.Game.Messages.S_OTHER_USER_APPLY_PARTY message)
        {
            if (!TeraWindow.IsTeraActive())
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

        internal static void S_TRADE_BROKER_DEAL_SUGGESTED(Tera.Game.Messages.S_TRADE_BROKER_DEAL_SUGGESTED message)
        {
            if (!TeraWindow.IsTeraActive())
            {
                NetworkController.Instance.FlashMessage = DefaultNotifyAction(
                    LP.Trading + ": " + message.PlayerName,
                    LP.SellerPrice + ": " + Tera.Game.Messages.S_TRADE_BROKER_DEAL_SUGGESTED.Gold(message.SellerPrice) +
                    Environment.NewLine +
                    LP.OfferedPrice + ": " + Tera.Game.Messages.S_TRADE_BROKER_DEAL_SUGGESTED.Gold(message.OfferedPrice)
                    );
            }
        }

        internal static void S_REQUEST_CONTRACT(Tera.Game.Messages.S_REQUEST_CONTRACT message)
        {
            if (!TeraWindow.IsTeraActive())
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
                else if (message.Type != Tera.Game.Messages.S_REQUEST_CONTRACT.RequestType.Craft)
                {
                    NetworkController.Instance.FlashMessage = DefaultNotifyAction(
                        LP.ContactTry,
                        LP.ContactTry
                        );
                }
            }
        }


        internal static void AbnormalityNotifierMissing(EntityId target, EntityId source, SkillResult skillResult)
        {
            if (!BasicTeraData.Instance.WindowData.EnableChat) return;

            var meterUser = NetworkController.Instance.EntityTracker.MeterUser;
            if (meterUser == null || _lastBoss == null) return;
            if (_lastBossHP==0 || target != _lastBoss.Value) return;
            if (NetworkController.Instance.AbnormalityStorage.DeadOrJustResurrected(NetworkController.Instance.PlayerTracker.Me())) return;
            var teraActive = TeraWindow.IsTeraActive();
            var time = DateTime.Now;
            foreach (var e in BasicTeraData.Instance.EventsData.Events)
            {
                if (!e.Key.Active) continue;
                EntityId entityIdToCheck = meterUser.Id;
                UserEntity player = meterUser;
                if (e.Key.GetType() != typeof(AbnormalityEvent)) continue;
                var abnormalityEvent = (AbnormalityEvent)e.Key;
                if (abnormalityEvent.InGame != teraActive) continue;
                if (abnormalityEvent.Trigger != AbnormalityTriggerType.MissingDuringFight) continue; 
                if (abnormalityEvent.Target == AbnormalityTargetType.Self && ( meterUser.Id != source)) continue;
                if (abnormalityEvent.Target == AbnormalityTargetType.Boss) entityIdToCheck = _lastBoss.Value;
                if ((abnormalityEvent.Target == AbnormalityTargetType.Party || abnormalityEvent.Target == AbnormalityTargetType.PartySelfExcluded) && BasicTeraData.Instance.WindowData.DisablePartyEvent) continue;
                if (abnormalityEvent.Target == AbnormalityTargetType.Party)
                {
                    player = skillResult.SourcePlayer?.User;
                    if (player == null || !NetworkController.Instance.PlayerTracker.PartyList().Contains(player)) continue;
                    if (NetworkController.Instance.AbnormalityStorage.DeadOrJustResurrected(skillResult.SourcePlayer)) return;
                    entityIdToCheck = player.Id;
                }
                if(abnormalityEvent.Target == AbnormalityTargetType.PartySelfExcluded)
                {
                    player = skillResult.SourcePlayer?.User;
                    if (player == null || !NetworkController.Instance.PlayerTracker.PartyList().Contains(player) || meterUser.Id == player.Id) continue;
                    if (NetworkController.Instance.AbnormalityStorage.DeadOrJustResurrected(skillResult.SourcePlayer)) return;
                    entityIdToCheck = player.Id;
                }

                if (player.OutOfRange) { continue; }
                //if (!NetworkController.Instance.AbnormalityTracker.HaveAbnormalities(entityIdToCheck)){ continue; }

                if (!e.Key.NextChecks.ContainsKey(entityIdToCheck))
                {
                    e.Key.NextChecks.Add(entityIdToCheck, time.AddMilliseconds(1000));
                }
                else
                {
                    if (time < e.Key.NextChecks[entityIdToCheck]) { continue; }
                    e.Key.NextChecks[entityIdToCheck] = time.AddMilliseconds(1000);
                }

                TimeSpan? abnormalityTimeLeft = null;
                var noAbnormalitiesMissing = false;

                foreach (var id in abnormalityEvent.Ids)
                {

                    var timeLeft = NetworkController.Instance.AbnormalityTracker.AbnormalityTimeLeft(entityIdToCheck, id);
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
                abnormalityEvent.NextChecks[entityIdToCheck] += TimeSpan.FromSeconds(abnormalityEvent.RewarnTimeoutSeconds);

                foreach (var a in e.Value)
                {
                    if (a.GetType() != typeof(NotifyAction)) { continue; }
                    var notifyAction = ((NotifyAction)a).Clone();
                    if (notifyAction.Sound != null && notifyAction.Sound.GetType() == typeof(TextToSpeech))
                    {
                        var textToSpeech = (TextToSpeech)notifyAction.Sound;
                        if (player != null){ textToSpeech.Text = textToSpeech.Text.Replace("{player_name}", player.Name); }

                        if (abnormalityEvent.Ids.Count > 0)
                        {
                            var abName = BasicTeraData.Instance.HotDotDatabase.Get(abnormalityEvent.Ids[0]).Name;
                            textToSpeech.Text = textToSpeech.Text.Replace("{abnormality_name}", abName);
                        }
                        else {
                            textToSpeech.Text = textToSpeech.Text.Replace("{abnormality_name}", LP.NoCrystalBind);
                        }

                    }

                    if (notifyAction.Balloon != null)
                    {
                        if (abnormalityEvent.Ids.Count > 0)
                        {
                            var abName = BasicTeraData.Instance.HotDotDatabase.Get(abnormalityEvent.Ids[0]).Name;
                            notifyAction.Balloon.BodyText = notifyAction.Balloon.BodyText.Replace("{abnormality_name}", abName);
                            notifyAction.Balloon.TitleText = notifyAction.Balloon.TitleText.Replace("{abnormality_name}", abName);
                        }
                        else
                        {
                            notifyAction.Balloon.BodyText = notifyAction.Balloon.BodyText.Replace("{abnormality_name}", LP.NoCrystalBind);
                            notifyAction.Balloon.TitleText = notifyAction.Balloon.TitleText.Replace("{abnormality_name}", LP.NoCrystalBind);
                        }
                        notifyAction.Balloon.BodyText = notifyAction.Balloon.BodyText.Replace("{player_name}", player.Name);
                        notifyAction.Balloon.TitleText = notifyAction.Balloon.TitleText.Replace("{player_name}", player.Name);
                     
                        if (abnormalityTimeLeft.HasValue) {
                            notifyAction.Balloon.TitleText = notifyAction.Balloon.TitleText.Replace("{time_left}", abnormalityTimeLeft.Value.ToString(@"hh\:mm\:ss", CultureInfo.InvariantCulture));
                            notifyAction.Balloon.BodyText = notifyAction.Balloon.BodyText.Replace("{time_left}", abnormalityTimeLeft.Value.ToString(@"hh\:mm\:ss", CultureInfo.InvariantCulture));
                        }else
                        {
                            notifyAction.Balloon.TitleText = notifyAction.Balloon.TitleText.Replace("{time_left}", "");
                            notifyAction.Balloon.BodyText = notifyAction.Balloon.BodyText.Replace("{time_left}", "");
                        }

                    }
                    NetworkController.Instance.FlashMessage = notifyAction;
                }
            }
        }

        internal static void AbnormalityNotifierAdded(EntityId target, int abnormalityId)
        {
            AbnormalityNotifierCommon(target, abnormalityId, AbnormalityTriggerType.Added);
        }

        internal static void AbnormalityNotifierRemoved(EntityId target, int abnormalityId)
        {
            AbnormalityNotifierCommon(target, abnormalityId, AbnormalityTriggerType.Removed);
        }

        internal static void SkillReset(int skillId, CrestType type)
        {
            if (type != CrestType.Reset) return;
            if (!BasicTeraData.Instance.WindowData.EnableChat) return;
            var meterUser = NetworkController.Instance.EntityTracker.MeterUser;
            if (meterUser == null || _lastBoss == null) return;
            if (_lastBossHP == 0) return;
            var teraActive = TeraWindow.IsTeraActive();

            foreach (var e in BasicTeraData.Instance.EventsData.Events)
            {
                if (!e.Key.Active) continue;
                UserEntity player = meterUser;
                if (e.Key.InGame != teraActive) continue;
                if (e.Key.GetType() != typeof(CooldownEvent)) { continue; }
                var cooldownEvent = (CooldownEvent)e.Key;
                if(cooldownEvent.SkillId != skillId) { continue; }

                foreach (var a in e.Value)
                {
                    if (a.GetType() != typeof(NotifyAction)) { continue; }
                    var notifyAction = ((NotifyAction)a).Clone();
                    var skill = BasicTeraData.Instance.SkillDatabase.GetOrNull(meterUser, skillId);
                    if (notifyAction.Balloon != null)
                    {
                        notifyAction.Balloon.BodyText = notifyAction.Balloon.BodyText.Replace("{skill_name}", skill.Name);
                        notifyAction.Balloon.TitleText = notifyAction.Balloon.TitleText.Replace("{skill_name}", skill.Name);
                    }
                    if (notifyAction.Sound != null && notifyAction.Sound.GetType() == typeof(TextToSpeech))
                    {
                        var textToSpeech = (TextToSpeech)notifyAction.Sound;
                        textToSpeech.Text = textToSpeech.Text.Replace("{skill_name}", skill.Name);
                    }
                    NetworkController.Instance.FlashMessage = notifyAction;
                }
            }
        }
        

        private static void AbnormalityNotifierCommon(EntityId target, int abnormalityId, AbnormalityTriggerType trigger)
        {
            if (!BasicTeraData.Instance.WindowData.EnableChat) return;

            var meterUser = NetworkController.Instance.EntityTracker.MeterUser;
            if (meterUser == null || _lastBoss == null) return;
            if (_lastBossHP==0) return;
            var teraActive = TeraWindow.IsTeraActive();

            foreach (var e in BasicTeraData.Instance.EventsData.Events)
            {
                if (!e.Key.Active) continue;
                UserEntity player = meterUser;
                if (e.Key.GetType() != typeof(AbnormalityEvent)){ continue; }
                var abnormalityEvent = (AbnormalityEvent)e.Key;
                if (abnormalityEvent.InGame != teraActive) continue;
                if (!abnormalityEvent.Ids.Contains(abnormalityId)) { continue; }
                if (abnormalityEvent.Trigger != trigger) { continue; }
                if (abnormalityEvent.Target == AbnormalityTargetType.Boss && _lastBoss.Value != target)  continue;
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
                        if (player != null)
                        {
                            notifyAction.Balloon.BodyText = notifyAction.Balloon.BodyText.Replace("{player_name}", player.Name);
                            notifyAction.Balloon.TitleText = notifyAction.Balloon.TitleText.Replace("{player_name}", player.Name);
                        }
                        notifyAction.Balloon.TitleText=  notifyAction.Balloon.TitleText.Replace("{abnormality_name}", abnormality.Name);
                    }
                    if(notifyAction.Sound != null && notifyAction.Sound.GetType() == typeof(TextToSpeech))
                    {
                        var textToSpeech = (TextToSpeech)notifyAction.Sound;
                        if (player != null)
                        {
                            textToSpeech.Text = textToSpeech.Text.Replace("{player_name}", player.Name);
                        }
                        textToSpeech.Text = textToSpeech.Text.Replace("{abnormality_name}", abnormality.Name);
                    }
                    NetworkController.Instance.FlashMessage = notifyAction;
                }
            }
        }
        private static EntityId? _lastBoss;
        private static long _lastBossHP;
        internal static void S_BOSS_GAGE_INFO(Tera.Game.Messages.S_BOSS_GAGE_INFO message)
        {
            NetworkController.Instance.EntityTracker.Update(message);
            _lastBoss = message.EntityId;
            if (message.TotalHp != message.HpRemaining) _lastBossHP = (long) message.HpRemaining;
        }

        internal static void SpawnMe(Tera.Game.Messages.SpawnMeServerMessage message)
        {
            NetworkController.Instance.AbnormalityTracker.Update(message);
            _lastBoss = null;
            _lastBossHP = 0;
            foreach (var e in BasicTeraData.Instance.EventsData.Events.Keys)
            {
                e.NextChecks=new Dictionary<EntityId, DateTime>();
            }
        }

        internal static void SpawnUser(Tera.Game.Messages.SpawnUserServerMessage message)
        {

        }
        internal static void DespawnNpc(Tera.Game.Messages.SDespawnNpc message)
        {
            _lastBoss = null;
            _lastBossHP = 0;
        }

        internal static void S_BEGIN_THROUGH_ARBITER_CONTRACT(S_BEGIN_THROUGH_ARBITER_CONTRACT message)
        {
            if (message.PlayerName.StartsWith("Error")) BasicTeraData.LogError(message.PlayerName);
        }
    }
}
