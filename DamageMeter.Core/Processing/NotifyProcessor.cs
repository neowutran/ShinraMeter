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
                    foreach(var action in ev.Value)
                    {
                        if(action is NotifyAction)
                        {
                            var notifyAction = ((NotifyAction)action).Clone();
                            notifyAction.Balloon.BodyText = notifyAction.Balloon.BodyText.Replace("{afk_body}", bodyText);
                            notifyAction.Balloon.TitleText = notifyAction.Balloon.TitleText.Replace("{afk_title}", titleText);
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
            if (_lastBoss.Value != target) return;
            var teraActive = TeraWindow.IsTeraActive();

            var time = DateTime.Now;
            foreach (var e in BasicTeraData.Instance.EventsData.Events)
            {

                if(time.AddMilliseconds(2000) < e.Key.LastCheck) { continue; }
                e.Key.LastCheck = time;

                EntityId entityIdToCheck = meterUser.Id;
                UserEntity player = meterUser;
                if (e.Key.GetType() != typeof(AbnormalityEvent)) { continue; }
                var abnormalityEvent = (AbnormalityEvent)e.Key;
                if (!abnormalityEvent.InGame && teraActive) { continue; }
                if (abnormalityEvent.Trigger != AbnormalityTriggerType.MissingDuringFight) { continue; }
                if (abnormalityEvent.Target == AbnormalityTargetType.Self && ( meterUser.Id != source)) { continue; }
                if (abnormalityEvent.Target == AbnormalityTargetType.Boss){ entityIdToCheck = _lastBoss.Value; }
                if (abnormalityEvent.Target == AbnormalityTargetType.Party)
                {
                    player = skillResult.TargetPlayer?.User ?? skillResult.SourcePlayer?.User;
                    if (player == null || !NetworkController.Instance.PlayerTracker.PartyList().Contains(player)) continue;
                    entityIdToCheck = player.Id;
                }

                TimeSpan? abnormalityTimeLeft = null;
                var noAbnormalitiesMissing = false;

                foreach (var id in abnormalityEvent.Ids)
                {
         
                    if (abnormalityEvent.RemainingSecondBeforeTrigger == 0)
                    {
                        if (NetworkController.Instance.AbnormalityTracker.AbnormalityExist(entityIdToCheck, id))
                        {
                            noAbnormalitiesMissing = true;
                            break;
                        }
                        continue;
                    }
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
               
                    if (abnormalityEvent.RemainingSecondBeforeTrigger == 0)
                    {
                        if (NetworkController.Instance.AbnormalityTracker.AbnormalityExist(entityIdToCheck, type))
                        {
                            noAbnormalitiesMissing = true;
                            break;
                        }
                        continue;
                    }
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

                foreach (var a in e.Value)
                {
                    if (a.GetType() != typeof(NotifyAction)) { continue; }
                    var notifyAction = ((NotifyAction)a).Clone();
                    if (notifyAction.Balloon != null)
                    {
                      
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

        private static void AbnormalityNotifierCommon(EntityId target, int abnormalityId, AbnormalityTriggerType trigger)
        {
            if (!BasicTeraData.Instance.WindowData.EnableChat) return;

            var meterUser = NetworkController.Instance.EntityTracker.MeterUser;
            if (meterUser == null || _lastBoss == null) return;
            if (_lastBoss.Value != target) return;

            var teraActive = TeraWindow.IsTeraActive();

            foreach (var e in BasicTeraData.Instance.EventsData.Events)
            {
                UserEntity player = meterUser;
                if (e.Key.GetType() != typeof(AbnormalityEvent)){ continue; }
                var abnormalityEvent = (AbnormalityEvent)e.Key;
                if(!abnormalityEvent.Ids.Contains(abnormalityId)) { continue; }
                if (!abnormalityEvent.InGame && teraActive) { continue; }
                if (abnormalityEvent.Trigger != trigger) { continue; }
                if(abnormalityEvent.Target == AbnormalityTargetType.Boss && _lastBoss.Value != target) { continue; }
                if(abnormalityEvent.Target == AbnormalityTargetType.Self && meterUser.Id != target) { continue; }
                if(abnormalityEvent.Target == AbnormalityTargetType.Party)
                {
                    player = NetworkController.Instance.EntityTracker.GetOrNull(target) as UserEntity;
                    if (player == null || !NetworkController.Instance.PlayerTracker.PartyList().Contains(player)) continue;
                }
                foreach(var a in e.Value)
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
                    NetworkController.Instance.FlashMessage = notifyAction;
                }
            }
        }
        private static EntityId? _lastBoss;
        internal static void S_BOSS_GAGE_INFO(Tera.Game.Messages.S_BOSS_GAGE_INFO message)
        {
            NetworkController.Instance.EntityTracker.Update(message);
            _lastBoss = message.EntityId;
        }

        private static List<UserEntity> playerWithUnkownBuff = new List<UserEntity>();

        internal static void SpawnMe(Tera.Game.Messages.SpawnMeServerMessage message)
        {
            NetworkController.Instance.AbnormalityTracker.Update(message);
        }

        internal static void SpawnUser(Tera.Game.Messages.SpawnUserServerMessage message)
        {

        }
        internal static void DespawnNpc(Tera.Game.Messages.SDespawnNpc message)
        {
            _lastBoss = null;
        }

        internal static void S_BEGIN_THROUGH_ARBITER_CONTRACT(S_BEGIN_THROUGH_ARBITER_CONTRACT message)
        {
            if (message.PlayerName.StartsWith("Error")) BasicTeraData.LogError(message.PlayerName);
        }
    }
}
