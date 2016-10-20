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
        private static void Process()
        {
            if (!TeraWindow.IsTeraActive())
            {
                NetworkController.Instance.FlashMessage = new NotifyMessage(
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
                    NetworkController.Instance.FlashMessage = new NotifyMessage(
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

                NetworkController.Instance.FlashMessage = new NotifyMessage(
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
                NetworkController.Instance.FlashMessage = new NotifyMessage(
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
                    NetworkController.Instance.FlashMessage = new NotifyMessage(
                        LP.PartyInvite + ": " + message.Sender,
                        message.Sender
                        );
                }
                else if (message.Type == Tera.Game.Messages.S_REQUEST_CONTRACT.RequestType.TradeRequest)
                {
                    NetworkController.Instance.FlashMessage = new NotifyMessage(
                        LP.Trading + ": " + message.Sender,
                        message.Sender
                        );
                }
                else if (message.Type != Tera.Game.Messages.S_REQUEST_CONTRACT.RequestType.Craft)
                {
                    NetworkController.Instance.FlashMessage = new NotifyMessage(
                        LP.ContactTry,
                        LP.ContactTry
                        );
                }
            }
        }

        private static long _nextCBNotifyCheck;
        private static EntityId? _lastBoss;
        internal static void S_BOSS_GAGE_INFO(Tera.Game.Messages.S_BOSS_GAGE_INFO message)
        {
            NetworkController.Instance.EntityTracker.Update(message);
            _lastBoss = message.EntityId;
            CheckCB(message);
        }

        private static List<UserEntity> playerWithUnkownBuff = new List<UserEntity>();

        internal static void CheckCB(ParsedMessage message)
        {
            if (BasicTeraData.Instance.WindowData.DoNotWarnOnCB) return;
            if (_lastBoss == null) return;
            if (message.Time.Ticks < _nextCBNotifyCheck) return;
            _nextCBNotifyCheck = message.Time.Ticks + 15 * TimeSpan.TicksPerSecond;// check no more than once per 15s
            var party = NetworkController.Instance.PlayerTracker.PartyList();
            string notify = "";
            foreach (var player in party)
            {

                if (NetworkController.Instance.AbnormalityTracker.AbnormalityCount(player.Id)==0) continue;
                var cbleft = Math.Max(
                    NetworkController.Instance.AbnormalityTracker.AbnormalityTimeLeft(player.Id, HotDot.Types.CCrystalBind),
                    NetworkController.Instance.AbnormalityTracker.AbnormalityTimeLeft(player.Id, HotDot.Types.CrystalBind));
                if (cbleft != 0 && cbleft < 15 * TimeSpan.TicksPerMinute)
                {
                    var timeLeft = TimeSpan.FromTicks(cbleft);
                    if (cbleft < 0)
                    {
                        notify += player.Name + LP.NoCrystalBind;
                    }
                    else
                    {
                        notify += player.Name + " "+LP.Time+": " + timeLeft.ToString(@"hh\:mm\:ss", CultureInfo.InvariantCulture);
                    }
                    notify += Environment.NewLine;
                }

            }
            if (!string.IsNullOrEmpty(notify))
            {
                _nextCBNotifyCheck = message.Time.Ticks + 15 * TimeSpan.TicksPerMinute;// no more than 1 notify in 10 minutes
                NetworkController.Instance.FlashMessage = new NotifyMessage(LP.NotifyCB, notify, false);
            }
        }

        internal static void SpawnMe(Tera.Game.Messages.SpawnMeServerMessage message)
        {
            NetworkController.Instance.AbnormalityTracker.Update(message);
            _nextCBNotifyCheck = message.Time.Ticks + 15 * TimeSpan.TicksPerSecond;// delay check after respawn
            _lastBoss = null;
        }

        internal static void SpawnUser(Tera.Game.Messages.SpawnUserServerMessage message)
        {
            var check= message.Time.Ticks + 15 * TimeSpan.TicksPerSecond;// delay check after respawn
            _nextCBNotifyCheck = check > _nextCBNotifyCheck ? check : _nextCBNotifyCheck;
        }
        internal static void DespawnNpc(Tera.Game.Messages.SDespawnNpc message)
        {
            if (message.Npc == _lastBoss) _lastBoss = null;
        }

        private static bool _joyOfPartyingIs100 = false;

        internal static void CheckJoyOfPartying()
        {
            if (!BasicTeraData.Instance.WindowData.EnableChat) return;

            var player = NetworkController.Instance.EntityTracker.MeterUser;
            if (player == null) return;


            var joyOfPartying20 = BasicTeraData.Instance.HotDotDatabase.JoyOfPartying20;
            var joyOfPartying50 = BasicTeraData.Instance.HotDotDatabase.JoyOfPartying50;
            var joyOfPartying0 = BasicTeraData.Instance.HotDotDatabase.JoyOfPartying0;
            var joyOfPartying100 = BasicTeraData.Instance.HotDotDatabase.JoyOfPartying100;

            if (joyOfPartying0==null||joyOfPartying20==null||joyOfPartying50==null||joyOfPartying100==null) return;

            if (NetworkController.Instance.AbnormalityTracker.AbnormalityExist(player.Id, joyOfPartying0) ||
                NetworkController.Instance.AbnormalityTracker.AbnormalityExist(player.Id, joyOfPartying20) ||
                NetworkController.Instance.AbnormalityTracker.AbnormalityExist(player.Id, joyOfPartying50))
            {
                _joyOfPartyingIs100 = false;
                return;
            }

            if (_joyOfPartyingIs100) return;
            if (NetworkController.Instance.AbnormalityTracker.AbnormalityExist(player.Id, joyOfPartying100))
            {
                if (!TeraWindow.IsTeraActive())
                {
                    NetworkController.Instance.FlashMessage = new NotifyMessage(LP.JoyOfPartying, LP.JoyOfPartying, false);
                }
                _joyOfPartyingIs100 = true;
            }
        }

        internal static void S_BEGIN_THROUGH_ARBITER_CONTRACT(S_BEGIN_THROUGH_ARBITER_CONTRACT message)
        {
            if (message.PlayerName.StartsWith("Error")) BasicTeraData.LogError(message.PlayerName);
        }
    }
}
