using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using Data;
using Data.Actions.Notify;
using Data.Actions.Notify.SoundElements;
using Data.Events;
using Data.Events.Abnormality;
using DiscordRPC;
using Lang;
using Tera.Game;
using Tera.Game.Abnormality;
using Tera.Game.Messages;
using Action = Data.Actions.Action;
using EventType = Data.EventType;
using RichPresence = Tera.RichPresence.RichPresence;

namespace DamageMeter.Processing
{
    internal class NotifyProcessor
    {
        private static NotifyProcessor _instance;
        internal Dictionary<EntityId, long> _lastBosses = new Dictionary<EntityId, long>();
        private long _lastBossHpMeterUser;
        private EntityId? _lastBossMeterUser;

        private NotifyProcessor() { }

        public static NotifyProcessor Instance => _instance ?? (_instance = new NotifyProcessor());

        internal void InstanceMatchingSuccess(S_FIN_INTER_PARTY_MATCH message)
        {
            MatchingSuccess();
        }

        internal void InstanceMatchingSuccess(S_BATTLE_FIELD_ENTRANCE_INFO message)
        {
            MatchingSuccess();
        }

        internal NotifyFlashMessage DefaultNotifyAction(string titleText, string bodyText, EventType evType)
        {
            var ev = BasicTeraData.Instance.EventsData.AFK;

            if (!(ev?.Item1 is CommonAFKEvent)) { return null; }
            foreach (var action in ev.Item2)
            {
                var action1 = action as NotifyAction;
                if (action1 == null) { continue; }
                var notifyAction = action1.Clone();
                notifyAction.Balloon.BodyText = notifyAction.Balloon.BodyText.Replace("{afk_body}", bodyText);
                notifyAction.Balloon.TitleText = notifyAction.Balloon.TitleText.Replace("{afk_title}", titleText);
                notifyAction.Balloon.EventType = evType;

                if (notifyAction.Sound == null || notifyAction.Sound.GetType() != typeof(TextToSpeech))
                {
                    return new NotifyFlashMessage(notifyAction.Sound, notifyAction.Balloon, ev.Item1.Priority);
                }

                var tts = (TextToSpeech) notifyAction.Sound;
                tts.Text = tts.Text.Replace("{afk_title}", titleText);
                if (!BasicTeraData.Instance.WindowData.TTSSizeExceededTruncate && bodyText.Length > BasicTeraData.Instance.WindowData.MaxTTSSize) {
                    tts.Text = tts.Text.Replace("{afk_body}", "");
                }
                else
                {
                    tts.Text = tts.Text.Replace("{afk_body}", bodyText.Substring(0, Math.Min(bodyText.Length, BasicTeraData.Instance.WindowData.MaxTTSSize)));
                }

                return new NotifyFlashMessage(notifyAction.Sound, notifyAction.Balloon, ev.Item1.Priority);
            }

            return null;
        }

        private void MatchingSuccess()
        {
            if (BasicTeraData.Instance.WindowData.ShowAfkEventsIngame || !TeraWindow.IsTeraActive())
            {
                PacketProcessor.Instance.FlashMessage.Add(DefaultNotifyAction(LP.PartyMatchingSuccess, LP.PartyMatchingSuccess, EventType.MatchingSuccess));
            }
        }

        internal void S_CHECK_TO_READY_PARTY(S_CHECK_TO_READY_PARTY message)
        {
            if (message.Count == 1 && (BasicTeraData.Instance.WindowData.ShowAfkEventsIngame || !TeraWindow.IsTeraActive()))
            {
                PacketProcessor.Instance.FlashMessage.Add(DefaultNotifyAction(LP.CombatReadyCheck, LP.CombatReadyCheck, EventType.ReadyCheck));
            }
        }

        internal void S_OTHER_USER_APPLY_PARTY(S_OTHER_USER_APPLY_PARTY message)
        {
            if (BasicTeraData.Instance.WindowData.ShowAfkEventsIngame || !TeraWindow.IsTeraActive())
            {
                PacketProcessor.Instance.FlashMessage.Add(DefaultNotifyAction(message.PlayerName + " " + LP.ApplyToYourParty,
                    LP.Class + ": " + LP.ResourceManager.GetString(message.PlayerClass.ToString(), LP.Culture) + Environment.NewLine + LP.Lvl + ": " + message.Lvl +
                    Environment.NewLine, EventType.OtherUserApply));
            }

            if (BasicTeraData.Instance.WindowData.CopyInspect)
            {
                var thread = new Thread(() => CopyPaste.CopyInspect(message.PlayerName));
                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();
            }
        }

        internal void S_TRADE_BROKER_DEAL_SUGGESTED(S_TRADE_BROKER_DEAL_SUGGESTED message)
        {
            if (BasicTeraData.Instance.WindowData.ShowAfkEventsIngame || !TeraWindow.IsTeraActive())
            {
                PacketProcessor.Instance.FlashMessage.Add(DefaultNotifyAction(LP.Trading + ": " + message.PlayerName,
                    LP.SellerPrice + ": " + Tera.Game.Messages.S_TRADE_BROKER_DEAL_SUGGESTED.Gold(message.SellerPrice) + Environment.NewLine + LP.OfferedPrice +
                    ": " + Tera.Game.Messages.S_TRADE_BROKER_DEAL_SUGGESTED.Gold(message.OfferedPrice), EventType.Broker));
            }
        }

        internal void S_REQUEST_CONTRACT(S_REQUEST_CONTRACT message)
        {
            if (BasicTeraData.Instance.WindowData.ShowAfkEventsIngame || !TeraWindow.IsTeraActive())
            {
                if (message.Type == Tera.Game.Messages.S_REQUEST_CONTRACT.RequestType.PartyInvite)
                {
                    PacketProcessor.Instance.FlashMessage.Add(DefaultNotifyAction(LP.PartyInvite + ": " + message.Sender, message.Sender, EventType.PartyInvite));
                }
                else if (message.Type == Tera.Game.Messages.S_REQUEST_CONTRACT.RequestType.TradeRequest)
                {
                    PacketProcessor.Instance.FlashMessage.Add(DefaultNotifyAction(LP.Trading + ": " + message.Sender, message.Sender, EventType.Trade));
                }
                else if (!Enum.IsDefined(typeof(S_REQUEST_CONTRACT.RequestType), (int) message.Type))
                {
                    PacketProcessor.Instance.FlashMessage.Add(DefaultNotifyAction(LP.ContactTry, LP.ContactTry, EventType.GenericContract));
                }
            }
        }

        internal void AbnormalityNotifierMissing()
        {
            if (!BasicTeraData.Instance.WindowData.EnableChat) { return; }
            var meterUser = PacketProcessor.Instance.EntityTracker.MeterUser;
            if (meterUser == null) { return; }
            if (PacketProcessor.Instance.AbnormalityStorage.DeadOrJustResurrected(PacketProcessor.Instance.PlayerTracker.Me())) { return; }
            var bossIds = _lastBosses.Where(x => x.Value > 0).Select(x => x.Key).ToList();
            var teraActive = TeraWindow.IsTeraActive();
            var time = DateTime.Now;
            var bossList = bossIds.Select(x => (NpcEntity) PacketProcessor.Instance.EntityTracker.GetOrNull(x)).ToList();

            foreach (var e in BasicTeraData.Instance.EventsData.MissingAbnormalities) {
                if (!(bossIds.Any() || e.Key.OutOfCombat)) { continue; }
                var entitiesIdToCheck = new List<EntityId>();
                var abnormalityEvent = (AbnormalityEvent) e.Key;
                if (abnormalityEvent.InGame != teraActive) { continue; }
                if (bossList.Any(
                    x => x != null && e.Key.AreaBossBlackList.Any(y => y.AreaId == x.Info.HuntingZoneId && (y.BossId == -1 || y.BossId == x.Info.TemplateId))))
                {
                    continue;
                }
                if (abnormalityEvent.Target == AbnormalityTargetType.Self) { entitiesIdToCheck.Add(meterUser.Id); }
                if (abnormalityEvent.Target == AbnormalityTargetType.Boss) { entitiesIdToCheck.AddRange(bossIds); }
                if (abnormalityEvent.Target == AbnormalityTargetType.MyBoss)
                {
                    if (_lastBossMeterUser == null || _lastBossHpMeterUser == 0) { continue; }
                    entitiesIdToCheck.Add(_lastBossMeterUser.Value);
                }
                if ((abnormalityEvent.Target == AbnormalityTargetType.Party || abnormalityEvent.Target == AbnormalityTargetType.PartySelfExcluded) &&
                    BasicTeraData.Instance.WindowData.DisablePartyEvent) { continue; }
                if (abnormalityEvent.Target == AbnormalityTargetType.Party)
                {
                    foreach (var player in PacketProcessor.Instance.PlayerTracker.PartyList())
                    {
                        if (player.OutOfRange) { continue; }
                        if (PacketProcessor.Instance.AbnormalityStorage.DeadOrJustResurrected(PacketProcessor.Instance.PlayerTracker.GetOrUpdate(player)))
                        {
                            continue;
                        }
                        entitiesIdToCheck.Add(player.Id);
                    }
                }
                if (abnormalityEvent.Target == AbnormalityTargetType.PartySelfExcluded)
                {
                    foreach (var player in PacketProcessor.Instance.PlayerTracker.PartyList())
                    {
                        if (player == meterUser) { continue; }
                        if (player.OutOfRange) { continue; }
                        if (PacketProcessor.Instance.AbnormalityStorage.DeadOrJustResurrected(PacketProcessor.Instance.PlayerTracker.GetOrUpdate(player)))
                        {
                            continue;
                        }
                        entitiesIdToCheck.Add(player.Id);
                    }
                }


                foreach (var entityIdToCheck in entitiesIdToCheck) {
                    if (!e.Key.NextChecks.ContainsKey(entityIdToCheck)) abnormalityEvent.NextChecks[entityIdToCheck] = time.AddSeconds(abnormalityEvent.RewarnTimeoutSeconds > 20 ? 0 : abnormalityEvent.RewarnTimeoutSeconds);
                    if (time <= e.Key.NextChecks[entityIdToCheck]) { continue; }

                    TimeSpan? abnormalityTimeLeft = null;
                    var noAbnormalitiesMissing = false;
                    var stack = 0;

                    foreach (var id in abnormalityEvent.Ids)
                    {
                        var timeLeft = PacketProcessor.Instance.AbnormalityTracker.AbnormalityTimeLeft(entityIdToCheck, id.Key, id.Value);
                        if (timeLeft >= abnormalityEvent.RemainingSecondBeforeTrigger * TimeSpan.TicksPerSecond)
                        {
                            noAbnormalitiesMissing = true;
                            break;
                        }
                        if (timeLeft != -1 && (abnormalityTimeLeft != null && timeLeft > abnormalityTimeLeft.Value.Ticks || abnormalityTimeLeft == null))
                        {
                            abnormalityTimeLeft = TimeSpan.FromTicks(timeLeft);
                        }
                        if (id.Value > 0) stack = Math.Max(stack, PacketProcessor.Instance.AbnormalityTracker.Stack(entityIdToCheck, id.Key));
                    }

                    if (noAbnormalitiesMissing) { continue; }

                    foreach (var type in abnormalityEvent.Types)
                    {
                        var timeLeft = PacketProcessor.Instance.AbnormalityTracker.AbnormalityTimeLeft(entityIdToCheck, type);
                        if (timeLeft >= abnormalityEvent.RemainingSecondBeforeTrigger * TimeSpan.TicksPerSecond)
                        {
                            noAbnormalitiesMissing = true;
                            break;
                        }
                        if (timeLeft != -1 && (abnormalityTimeLeft != null && timeLeft > abnormalityTimeLeft.Value.Ticks || abnormalityTimeLeft == null))
                        {
                            abnormalityTimeLeft = TimeSpan.FromTicks(timeLeft);
                        }
                    }

                    if (noAbnormalitiesMissing) { continue; }
                    if (abnormalityEvent.Trigger == AbnormalityTriggerType.Ending && (abnormalityTimeLeft == null || abnormalityTimeLeft.Value.Ticks <= 0))
                    {
                        continue;
                    }

                    abnormalityEvent.NextChecks[entityIdToCheck] = time.AddSeconds(abnormalityEvent.RewarnTimeoutSeconds);

                    foreach (var a in e.Value)
                    {
                        if (a.GetType() != typeof(NotifyAction)) { continue; }
                        var notifyAction = ((NotifyAction) a).Clone();
                        var player = PacketProcessor.Instance.EntityTracker.GetOrNull(entityIdToCheck) as UserEntity;
                        if (notifyAction.Sound != null && notifyAction.Sound.GetType() == typeof(TextToSpeech))
                        {
                            var textToSpeech = (TextToSpeech) notifyAction.Sound;
                            if (player != null) { textToSpeech.Text = textToSpeech.Text.Replace("{player_name}", player.Name); }

                            if (abnormalityEvent.Ids.Count > 0) {
                                var abName = BasicTeraData.Instance.HotDotDatabase.Get(abnormalityEvent.Ids.First().Key)?.Name ?? "Unknown abnormality missing";
                                textToSpeech.Text = textToSpeech.Text.Replace("{abnormality_name}", abName);
                            }
                            else { textToSpeech.Text = textToSpeech.Text.Replace("{abnormality_name}", LP.NoCrystalBind); }

                            textToSpeech.Text = textToSpeech.Text.Replace("{time_left}", abnormalityTimeLeft?.Seconds.ToString() ?? "0");
                            textToSpeech.Text = textToSpeech.Text.Replace("{stack}", stack.ToString());
                        }

                        if (notifyAction.Balloon != null)
                        {
                            if (abnormalityEvent.Ids.Count > 0)
                            {
                                var abName = BasicTeraData.Instance.HotDotDatabase.Get(abnormalityEvent.Ids.First().Key)?.Name ?? "Unknown abnormality missing";
                                var icon = BasicTeraData.Instance.HotDotDatabase.Get(abnormalityEvent.Ids.First().Key)?.EffectIcon;
                                notifyAction.Balloon.BodyText = notifyAction.Balloon.BodyText.Replace("{abnormality_name}", abName);
                                notifyAction.Balloon.TitleText = notifyAction.Balloon.TitleText.Replace("{abnormality_name}", abName);
                                notifyAction.Balloon.Icon = icon;
                            }
                            else
                            {
                                notifyAction.Balloon.BodyText = notifyAction.Balloon.BodyText.Replace("{abnormality_name}", LP.NoCrystalBind);
                                notifyAction.Balloon.TitleText = notifyAction.Balloon.TitleText.Replace("{abnormality_name}", LP.NoCrystalBind);
                                notifyAction.Balloon.Icon = "icon_items.q_closedspace_stone_05_tex";
                            }
                            notifyAction.Balloon.BodyText = notifyAction.Balloon.BodyText.Replace("{player_name}", player?.Name);
                            notifyAction.Balloon.BodyText = notifyAction.Balloon.BodyText.Replace("{stack}", stack.ToString());
                            notifyAction.Balloon.TitleText = notifyAction.Balloon.TitleText.Replace("{stack}", stack.ToString());
                            notifyAction.Balloon.TitleText = notifyAction.Balloon.TitleText.Replace("{player_name}", player?.Name);

                            if (abnormalityTimeLeft.HasValue)
                            {
                                notifyAction.Balloon.TitleText = notifyAction.Balloon.TitleText.Replace("{time_left}",
                                    abnormalityTimeLeft.Value.ToString(@"hh\:mm\:ss", CultureInfo.InvariantCulture));
                                notifyAction.Balloon.BodyText = notifyAction.Balloon.BodyText.Replace("{time_left}",
                                    abnormalityTimeLeft.Value.ToString(@"hh\:mm\:ss", CultureInfo.InvariantCulture));
                            }
                            else
                            {
                                notifyAction.Balloon.TitleText = notifyAction.Balloon.TitleText.Replace("{time_left}", "0");
                                notifyAction.Balloon.BodyText = notifyAction.Balloon.BodyText.Replace("{time_left}", "0");
                            }
                        }
                        PacketProcessor.Instance.FlashMessage.Add(new NotifyFlashMessage(notifyAction.Sound, notifyAction.Balloon, e.Key.Priority));
                    }
                    break;
                }
            }
        }

        internal void UpdateMeterBoss(EachSkillResultServerMessage message)
        {
            var source = PacketProcessor.Instance.EntityTracker.GetOrNull(message.Source) as UserEntity;
            if (PacketProcessor.Instance.EntityTracker.MeterUser != source) { return; }
            UpdateMeterBoss(message.Target);
        }

        internal void UpdateMeterBoss(EntityId entityId)
        {
            var npc = PacketProcessor.Instance.EntityTracker.GetOrNull(entityId) as NpcEntity;
            if (npc == null) { return; }
            if (npc.Info.Boss) { _lastBossMeterUser = npc.Id; }
        }

        

        internal void AbnormalityNotifierAdded(Abnormality ab, bool newStack)
        {
            if (newStack) { AbnormalityNotifierCommon(ab.Target, ab.HotDot.Id, AbnormalityTriggerType.Added, ab.Stack); }
            var boss = HudManager.Instance.CurrentBosses.FirstOrDefault(x => x.EntityId == ab.Target);
            boss?.AddOrRefresh(ab);
        }

        internal void AbnormalityNotifierRemoved(EntityId target, int abnormalityId)
        {
            AbnormalityNotifierCommon(target, abnormalityId, AbnormalityTriggerType.Removed, 0);
            var boss = HudManager.Instance.CurrentBosses.FirstOrDefault(x => x.EntityId == target);
            boss?.EndBuff(abnormalityId);
        }

        internal void SkillReset(int skillId, CrestType type)
        {
            if (type != CrestType.Reset) { return; }
            var meterUser = PacketProcessor.Instance.EntityTracker.MeterUser;
            //var bossIds = _lastBosses.Where(x => x.Value > 0).Select(x => x.Key).ToList();
            //if (meterUser == null || !bossIds.Any()) { return; }
            if (meterUser == null) { return; }
            var teraActive = TeraWindow.IsTeraActive();

            foreach (var e in BasicTeraData.Instance.EventsData.Cooldown)
            {
                if (e.Key.InGame != teraActive) { continue; }
                var cooldownEvent = (CooldownEvent) e.Key;
                if (cooldownEvent.SkillId != 0 && cooldownEvent.SkillId != skillId) { continue; }

                foreach (var a in e.Value)
                {
                    if (a.GetType() != typeof(NotifyAction)) { continue; }
                    var notifyAction = ((NotifyAction) a).Clone();
                    var skill = BasicTeraData.Instance.SkillDatabase.GetOrNull(meterUser, skillId);
                    if (notifyAction.Balloon != null)
                    {
                        notifyAction.Balloon.BodyText = notifyAction.Balloon.BodyText.Replace("{skill_name}", skill?.Name ?? skillId.ToString()).Replace("{skill_id}",skillId.ToString());
                        notifyAction.Balloon.TitleText = notifyAction.Balloon.TitleText.Replace("{skill_name}", skill?.Name ?? skillId.ToString()).Replace("{skill_id}", skillId.ToString());
                        notifyAction.Balloon.Icon = skill?.IconName??"";
                    }
                    if (notifyAction.Sound != null && notifyAction.Sound.GetType() == typeof(TextToSpeech))
                    {
                        var textToSpeech = (TextToSpeech) notifyAction.Sound;
                        textToSpeech.Text = textToSpeech.Text.Replace("{skill_name}", skill?.Name ?? skillId.ToString());
                    }
                    PacketProcessor.Instance.FlashMessage.Add(new NotifyFlashMessage(notifyAction.Sound, notifyAction.Balloon, e.Key.Priority));
                    PacketProcessor.Instance.ForceUiUpdate = true;
                }
            }
        }


        private void AbnormalityNotifierCommon(EntityId target, int abnormalityId, AbnormalityTriggerType trigger, int stack)
        {
            var meterUser = PacketProcessor.Instance.EntityTracker.MeterUser;
            if (meterUser == null) { return; }
            if (PacketProcessor.Instance.AbnormalityStorage.DeadOrJustResurrected(PacketProcessor.Instance.PlayerTracker.Me())) { return; }
            var bossIds = _lastBosses.Where(x => x.Value > 0).Select(x => x.Key).ToList();
            var teraActive = TeraWindow.IsTeraActive();
            var bossList = bossIds.Select(x => (NpcEntity) PacketProcessor.Instance.EntityTracker.GetOrNull(x)).ToList();

            foreach (var e in BasicTeraData.Instance.EventsData.AddedRemovedAbnormalities)
            {
                if (!(bossIds.Any() || e.Key.OutOfCombat)) { continue; }
                var player = meterUser;
                var abnormalityEvent = (AbnormalityEvent) e.Key;
                if (abnormalityEvent.InGame != teraActive) { continue; }
                if (abnormalityEvent.Trigger != trigger) { continue; }
                if (!abnormalityEvent.Ids.ContainsKey(abnormalityId)) { continue; }
                if (abnormalityEvent.Ids[abnormalityId] > stack) { continue; }
                if (bossList.Any(
                    x => x != null && e.Key.AreaBossBlackList.Any(y => y.AreaId == x.Info.HuntingZoneId && (y.BossId == -1 || y.BossId == x.Info.TemplateId))))
                {
                    continue;
                }
                if (abnormalityEvent.Target == AbnormalityTargetType.Boss && !bossIds.Contains(target)) { continue; }
                if (abnormalityEvent.Target == AbnormalityTargetType.MyBoss && _lastBossMeterUser != target) { continue; }
                if (abnormalityEvent.Target == AbnormalityTargetType.Self && meterUser.Id != target) { continue; }
                if ((abnormalityEvent.Target == AbnormalityTargetType.Party || abnormalityEvent.Target == AbnormalityTargetType.PartySelfExcluded) &&
                    BasicTeraData.Instance.WindowData.DisablePartyEvent) { continue; }
                if (abnormalityEvent.Target == AbnormalityTargetType.Party)
                {
                    player = PacketProcessor.Instance.EntityTracker.GetOrNull(target) as UserEntity;
                    if (player == null || !PacketProcessor.Instance.PlayerTracker.PartyList().Contains(player)) { continue; }
                }
                if (abnormalityEvent.Target == AbnormalityTargetType.PartySelfExcluded)
                {
                    player = PacketProcessor.Instance.EntityTracker.GetOrNull(target) as UserEntity;
                    if (player == null || !PacketProcessor.Instance.PlayerTracker.PartyList().Contains(player) || meterUser.Id == player.Id) { continue; }
                }

                if (player.OutOfRange) { continue; }
                var maxHp = bossList.FirstOrDefault(x => x.Id == target)?.Info.HP??0;
                _lastBosses.TryGetValue(target, out long curHp);
                var percHp = maxHp == 0 ? 0 : curHp * 100 / maxHp;
                var nextHp = percHp <= 10 ? 0 : percHp - 10;
                foreach (var a in e.Value)
                {
                    if (a.GetType() != typeof(NotifyAction)) { continue; }
                    var notifyAction = ((NotifyAction) a).Clone();
                    var abnormality = BasicTeraData.Instance.HotDotDatabase.Get(abnormalityId);
                    if (notifyAction.Balloon != null)
                    {
                        notifyAction.Balloon.BodyText = notifyAction.Balloon.BodyText.Replace("{boss_hp}", percHp.ToString());
                        notifyAction.Balloon.BodyText = notifyAction.Balloon.BodyText.Replace("{next_hp}", nextHp.ToString());
                        notifyAction.Balloon.BodyText = notifyAction.Balloon.BodyText.Replace("{abnormality_name}", abnormality.Name);
                        notifyAction.Balloon.BodyText = notifyAction.Balloon.BodyText.Replace("{stack}", stack.ToString());
                        if (player != null)
                        {
                            notifyAction.Balloon.BodyText = notifyAction.Balloon.BodyText.Replace("{player_name}", player.Name);
                            notifyAction.Balloon.TitleText = notifyAction.Balloon.TitleText.Replace("{player_name}", player.Name);
                        }
                        notifyAction.Balloon.TitleText = notifyAction.Balloon.TitleText.Replace("{boss_hp}", percHp.ToString());
                        notifyAction.Balloon.TitleText = notifyAction.Balloon.TitleText.Replace("{next_hp}", nextHp.ToString());
                        notifyAction.Balloon.TitleText = notifyAction.Balloon.TitleText.Replace("{abnormality_name}", abnormality.Name);
                        notifyAction.Balloon.TitleText = notifyAction.Balloon.TitleText.Replace("{stack}", stack.ToString());

                        notifyAction.Balloon.EventType = EventType.AddRemoveAb;
                        notifyAction.Balloon.Icon = abnormality.EffectIcon;
                    }
                    if (notifyAction.Sound != null && notifyAction.Sound.GetType() == typeof(TextToSpeech))
                    {
                        var textToSpeech = (TextToSpeech) notifyAction.Sound;
                        if (player != null) { textToSpeech.Text = textToSpeech.Text.Replace("{player_name}", player.Name); }
                        textToSpeech.Text = textToSpeech.Text.Replace("{boss_hp}", percHp.ToString());
                        textToSpeech.Text = textToSpeech.Text.Replace("{next_hp}", nextHp.ToString());
                        textToSpeech.Text = textToSpeech.Text.Replace("{abnormality_name}", abnormality.Name);
                        textToSpeech.Text = textToSpeech.Text.Replace("{stack}", stack.ToString());
                    }
                    PacketProcessor.Instance.FlashMessage.Add(new NotifyFlashMessage(notifyAction.Sound, notifyAction.Balloon, e.Key.Priority));
                }
            }
        }

        internal void S_BOSS_GAGE_INFO(S_BOSS_GAGE_INFO message)
        {
            PacketProcessor.Instance.EntityTracker.Update(message);
            RichPresence.Instance.HandleBossHp(message);
            HudManager.Instance.AddOrUpdateBoss(message);
            long newHp = 0;
            if (message.TotalHp != message.HpRemaining)
            {
                newHp = (long) message.HpRemaining;
            }
            if (message.EntityId == _lastBossMeterUser) { _lastBossHpMeterUser = newHp; }
            _lastBosses[message.EntityId] = newHp;
        }

        internal void SpawnMe(SpawnMeServerMessage message)
        {
            HudManager.Instance.CurrentBosses.DisposeAll();
            S_SPAWN_ME.Process(message);
            _lastBosses = new Dictionary<EntityId, long>();
            _lastBossMeterUser = null;
            _lastBossHpMeterUser = 0;
            if (BasicTeraData.Instance.EventsData.MissingAbnormalities?.Keys != null) {
                foreach (var e in BasicTeraData.Instance.EventsData.MissingAbnormalities.Keys) e.NextChecks = new Dictionary<EntityId, DateTime>();
            }
        }

        internal void S_LOAD_TOPO(S_LOAD_TOPO message)
        {
            HudManager.Instance.CurrentBosses.DisposeAll();
            PacketProcessor.Instance.RaiseMapChanged();
            RichPresence.Instance.S_LOAD_TOPO(message);
            _lastBosses = new Dictionary<EntityId, long>();
            _lastBossMeterUser = null;
            _lastBossHpMeterUser = 0;
            if (BasicTeraData.Instance.EventsData.MissingAbnormalities?.Keys != null) {
                foreach (var e in BasicTeraData.Instance.EventsData.MissingAbnormalities.Keys) e.NextChecks = new Dictionary<EntityId, DateTime>();
            }
        }

        internal void SpawnUser(SpawnUserServerMessage message)
        {
            if (_lastBosses.Any(x => x.Value > 0))
                if (BasicTeraData.Instance.EventsData.MissingAbnormalities?.Keys != null) {
                    foreach (var e in BasicTeraData.Instance.EventsData.MissingAbnormalities.Keys) e.NextChecks[message.Id] = DateTime.UtcNow.AddSeconds(5);
                }
        }

        internal void DespawnNpc(SDespawnNpc message)
        {
            HudManager.Instance.RemoveBoss(message);
            if (_lastBosses.ContainsKey(message.Npc)) { _lastBosses.Remove(message.Npc); }
            if (message.Npc == _lastBossMeterUser)
            {
                _lastBossMeterUser = null;
                _lastBossHpMeterUser = 0;
            }
            if (!_lastBosses.Any(x => x.Value > 0))
                if (BasicTeraData.Instance.EventsData.MissingAbnormalities?.Keys != null) {
                    foreach (var e in BasicTeraData.Instance.EventsData.MissingAbnormalities.Keys) e.NextChecks = new Dictionary<EntityId, DateTime>();
                }
        }

        internal void S_BEGIN_THROUGH_ARBITER_CONTRACT(S_BEGIN_THROUGH_ARBITER_CONTRACT message)
        {
            if (message.PlayerName.StartsWith("Error")) { BasicTeraData.LogError(message.PlayerName); }
        }

        internal void UpdateCredits(S_UPDATE_NPCGUILD.NpcGuildType type, int credits)
        {
            if (type == S_UPDATE_NPCGUILD.NpcGuildType.Vanguard && credits >= (PacketProcessor.Instance.MessageFactory.Version == 351828 ? 8500 : 19500))// hope there wouldn't be any other versions until v86
            {
                PacketProcessor.Instance.FlashMessage.Add(DefaultNotifyAction(LP.VanguardCredits + credits, LP.VanguardCredits + credits, EventType.VanguardCredits));
            }
        }

        internal void UpdateCredits(S_AVAILABLE_EVENT_MATCHING_LIST message)
        {
            UpdateCredits(S_UPDATE_NPCGUILD.NpcGuildType.Vanguard, message.Credits);
        }

        internal void UpdateCredits(S_UPDATE_NPCGUILD message)
        {
            UpdateCredits(message.Type, message.Credits);
        }

        internal void Resume(S_LOAD_TOPO sLoadTopo)
        {
            if (BasicTeraData.Instance.WindowData.UserPaused) return;
            PacketProcessor.Instance.PacketProcessing.Update();
            PacketProcessor.Instance.RaisePause(false);
            PacketProcessor.Instance.RaiseMapChanged();
            PacketProcessor.Instance.PacketProcessing.Process(sLoadTopo);
        }
    }
}