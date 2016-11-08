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
            var balloon = new Balloon(titleText, bodyText, BasicTeraData.Instance.WindowData.PopupDisplayTime);
            List<Beep> beeps = new List<Beep>();
            beeps.Add(new Beep(440, 500));
            beeps.Add(new Beep(440, 500));
            beeps.Add(new Beep(440, 500));
            var music = new Music(BasicTeraData.Instance.WindowData.NotifySound, BasicTeraData.Instance.WindowData.Volume, BasicTeraData.Instance.WindowData.SoundNotifyDuration);
            var type = SoundType.Music;
            if (BasicTeraData.Instance.WindowData.SoundConsoleBeepFallback)
            {
                type = SoundType.Beeps;
            }
            var sound = new Sound(beeps, music, type);
            return new NotifyAction(sound, balloon);
           
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


        internal static void AbnormalityNotifierMissing(EntityId target, EntityId source)
        {
            if (!BasicTeraData.Instance.WindowData.EnableChat) return;

            var meterUser = NetworkController.Instance.EntityTracker.MeterUser;
            if (meterUser == null || _lastBoss == null) return;
            if (_lastBoss != target && _lastBoss != source) return;
            UserEntity player = meterUser;
            var teraActive = TeraWindow.IsTeraActive();


            foreach (var e in BasicTeraData.Instance.EventsData.Events)
            {
                if (e.Key.GetType() != typeof(AbnormalityEvent)) { continue; }
                var abnormalityEvent = (AbnormalityEvent)e.Key;
                if (!abnormalityEvent.InGame && teraActive) { continue; }
                if (abnormalityEvent.Trigger != AbnormalityTriggerType.MissingDuringFight) { continue; }
                if (abnormalityEvent.Target == AbnormalityTargetType.Boss && ( _lastBoss.Value != target || _lastBoss.Value != source)) { continue; }
                if (abnormalityEvent.Target == AbnormalityTargetType.Self && ( meterUser.Id != target || meterUser.Id != source)) { continue; }
                if (abnormalityEvent.Target == AbnormalityTargetType.Party)
                {
                    bool found = false;
                    foreach (var partyPlayer in NetworkController.Instance.PlayerTracker.PartyList())
                    {
                        if (partyPlayer.Id == target || partyPlayer.Id == source)
                        {
                            player = partyPlayer;
                            found = true;
                            break;
                        }
                    }
                    if (!found) { continue; }
                }
                TimeSpan? abnormalityTimeLeft = null;
                var noAbnormalitiesMissing = false;

                foreach(var type in abnormalityEvent.Types)
                {
                    var timeLeft = NetworkController.Instance.AbnormalityTracker.AbnormalityTimeLeft(player.Id, type);
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
                
                foreach (var id in abnormalityEvent.Ids)
                {
                   
                    var timeLeft = NetworkController.Instance.AbnormalityTracker.AbnormalityTimeLeft(player.Id, id);
                    if (timeLeft >= abnormalityEvent.RemainingSecondBeforeTrigger * TimeSpan.TicksPerSecond)
                    {
                        noAbnormalitiesMissing = true;
                        break;
                    }
                    if ( timeLeft != -1 && ((abnormalityTimeLeft != null && timeLeft > abnormalityTimeLeft.Value.Ticks) || abnormalityTimeLeft == null))
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
            if (_lastBoss != target) return;

            var teraActive = TeraWindow.IsTeraActive();
            UserEntity player = null;

            foreach (var e in BasicTeraData.Instance.EventsData.Events)
            {
                if (e.Key.GetType() != typeof(AbnormalityEvent)){ continue; }
                var abnormalityEvent = (AbnormalityEvent)e.Key;
                if(!abnormalityEvent.Ids.Contains(abnormalityId)) { continue; }
                if (!abnormalityEvent.InGame && teraActive) { continue; }
                if (abnormalityEvent.Trigger != trigger) { continue; }
                if(abnormalityEvent.Target == AbnormalityTargetType.Boss && _lastBoss.Value != target) { continue; }
                if(abnormalityEvent.Target == AbnormalityTargetType.Self && meterUser.Id != target) { continue; }
                if(abnormalityEvent.Target == AbnormalityTargetType.Party)
                {
                    bool found = false;
                    foreach (var partyPlayer in NetworkController.Instance.PlayerTracker.PartyList())
                    {
                        if(partyPlayer.Id == target)
                        {
                            player = partyPlayer;
                            found = true;
                            break;
                        }
                    }
                    if (!found) { continue; }
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
                            notifyAction.Balloon.BodyText = notifyAction.Balloon.BodyText.Replace("{party_player}", player.Name);
                            notifyAction.Balloon.TitleText = notifyAction.Balloon.TitleText.Replace("{party_player}", player.Name);
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
            _lastBoss = null;
        }

        internal static void SpawnUser(Tera.Game.Messages.SpawnUserServerMessage message)
        {

        }
        internal static void DespawnNpc(Tera.Game.Messages.SDespawnNpc message)
        {
            if (message.Npc == _lastBoss) _lastBoss = null;
        }

        internal static void S_BEGIN_THROUGH_ARBITER_CONTRACT(S_BEGIN_THROUGH_ARBITER_CONTRACT message)
        {
            if (message.PlayerName.StartsWith("Error")) BasicTeraData.LogError(message.PlayerName);
        }
    }
}
