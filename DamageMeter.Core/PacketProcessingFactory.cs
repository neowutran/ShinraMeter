using System;
using System.Collections.Generic;
using System.Linq;
using DamageMeter.Processing;
using Data;
using Tera.Game;
using Tera.Game.Messages;
using Tera.RichPresence;
using C_CHECK_VERSION = Tera.Game.Messages.C_CHECK_VERSION;
using S_CREST_INFO = Tera.Game.Messages.S_CREST_INFO;
using C_LOGIN_ARBITER = Tera.Game.Messages.C_LOGIN_ARBITER;

namespace DamageMeter
{
    // Creates a ParsedMessage from a Message
    // Contains a mapping from OpCodeNames to message types and knows how to instantiate those
    // Since it works with OpCodeNames not numeric OpCodes, it needs an OpCodeNamer
    public class PacketProcessingFactory
    {
        public bool Paused = false;
        private static readonly Dictionary<Type, Delegate> MessageToProcessingPaused = new Dictionary<Type, Delegate>
        {
            {typeof(C_LOGIN_ARBITER), Helpers.Contructor<Func<C_LOGIN_ARBITER, Processing.C_LOGIN_ARBITER>>()},
            {typeof(S_GET_USER_LIST), new Action<S_GET_USER_LIST>(x => PacketProcessor.Instance.UserLogoTracker.SetUserList(x))},
            {typeof(S_GET_USER_GUILD_LOGO), new Action<S_GET_USER_GUILD_LOGO>(x => PacketProcessor.Instance.UserLogoTracker.AddLogo(x))},
            {typeof(C_CHECK_VERSION), Helpers.Contructor<Func<C_CHECK_VERSION, Processing.C_CHECK_VERSION>>()},
            {typeof(S_LOAD_TOPO), new Action<S_LOAD_TOPO>(x => NotifyProcessor.Instance.Resume(x))},
            {typeof(LoginServerMessage), Helpers.Contructor<Func<LoginServerMessage, S_LOGIN>>()}
        };

        private static readonly Dictionary<Type, Delegate> MessageToProcessingInit = new Dictionary<Type, Delegate>
        {
            {typeof(C_LOGIN_ARBITER), Helpers.Contructor<Func<C_LOGIN_ARBITER, Processing.C_LOGIN_ARBITER>>()},
            {typeof(S_GET_USER_LIST), new Action<S_GET_USER_LIST>(x => PacketProcessor.Instance.UserLogoTracker.SetUserList(x))},
            {typeof(S_GET_USER_GUILD_LOGO), new Action<S_GET_USER_GUILD_LOGO>(x => PacketProcessor.Instance.UserLogoTracker.AddLogo(x))},
            {typeof(C_CHECK_VERSION), Helpers.Contructor<Func<C_CHECK_VERSION, Processing.C_CHECK_VERSION>>()},
            {typeof(LoginServerMessage), Helpers.Contructor<Func<LoginServerMessage, S_LOGIN>>()}
        };

        private static readonly Dictionary<Type, Delegate> MessageToProcessingOptionnal = new Dictionary<Type, Delegate>
        {
            {typeof(S_WEAK_POINT), new Action<S_WEAK_POINT>(x => HudManager.Instance.UpdateRunemarks(x))},
            {typeof(S_AVAILABLE_EVENT_MATCHING_LIST), new Action<S_AVAILABLE_EVENT_MATCHING_LIST>(x => NotifyProcessor.Instance.UpdateCredits(x))},
            {typeof(S_UPDATE_NPCGUILD), new Action<S_UPDATE_NPCGUILD>(x => NotifyProcessor.Instance.UpdateCredits(x))},
            {typeof(SpawnMeServerMessage), new Action<SpawnMeServerMessage>(x => NotifyProcessor.Instance.SpawnMe(x))}, //override with optional processing
            {typeof(S_BOSS_GAGE_INFO), new Action<S_BOSS_GAGE_INFO>(x => NotifyProcessor.Instance.S_BOSS_GAGE_INFO(x))}, //override with optional processing
            {typeof(S_RETURN_TO_LOBBY), new Action<S_RETURN_TO_LOBBY>(x => NotifyProcessor.Instance.S_LOAD_TOPO(null))},
            {typeof(S_LOAD_TOPO), new Action<S_LOAD_TOPO>(x => NotifyProcessor.Instance.S_LOAD_TOPO(x))},
            {typeof(S_CHAT), new Action<S_CHAT>(x => Chat.Instance.Add(x))},
            {typeof(S_WHISPER), new Action<S_WHISPER>(x => Chat.Instance.Add(x))},
            {typeof(S_TRADE_BROKER_DEAL_SUGGESTED), new Action<S_TRADE_BROKER_DEAL_SUGGESTED>(x => NotifyProcessor.Instance.S_TRADE_BROKER_DEAL_SUGGESTED(x))},
            {typeof(S_OTHER_USER_APPLY_PARTY), new Action<S_OTHER_USER_APPLY_PARTY>(x => NotifyProcessor.Instance.S_OTHER_USER_APPLY_PARTY(x))},
            {typeof(S_PRIVATE_CHAT), new Action<S_PRIVATE_CHAT>(x => Chat.Instance.Add(x))},
            {typeof(S_FIN_INTER_PARTY_MATCH), new Action<S_FIN_INTER_PARTY_MATCH>(x => NotifyProcessor.Instance.InstanceMatchingSuccess(x))},
            {typeof(S_BATTLE_FIELD_ENTRANCE_INFO), new Action<S_BATTLE_FIELD_ENTRANCE_INFO>(x => NotifyProcessor.Instance.InstanceMatchingSuccess(x))},
            {typeof(S_REQUEST_CONTRACT), new Action<S_REQUEST_CONTRACT>(x => NotifyProcessor.Instance.S_REQUEST_CONTRACT(x))},
            {typeof(S_CHECK_TO_READY_PARTY), new Action<S_CHECK_TO_READY_PARTY>(x => NotifyProcessor.Instance.S_CHECK_TO_READY_PARTY(x))},
            {typeof(S_CREST_MESSAGE), new Action<S_CREST_MESSAGE>(x => NotifyProcessor.Instance.SkillReset(x.SkillId, x.Type))},
            {typeof(S_VISIT_NEW_SECTION), new Action<S_VISIT_NEW_SECTION>(x => RichPresence.Instance.VisitNewSection(x))},
            {typeof(S_SHOW_PARTY_MATCH_INFO), new Action<S_SHOW_PARTY_MATCH_INFO>(x => RichPresence.Instance.HandleLfg(x))},
            {typeof(C_REGISTER_PARTY_INFO), new Action<C_REGISTER_PARTY_INFO>(x => RichPresence.Instance.HandleLfg(x))},
            {typeof(S_CHANGE_EVENT_MATCHING_STATE), new Action<S_CHANGE_EVENT_MATCHING_STATE>(x => RichPresence.Instance.HandleIms(x))}
        };

        private static readonly Dictionary<Type, Delegate> MessageToProcessing = new Dictionary<Type, Delegate>
        {
            {typeof(EachSkillResultServerMessage), Helpers.Contructor<Func<EachSkillResultServerMessage, S_EACH_SKILL_RESULT>>()},
            {typeof(SpawnUserServerMessage), Helpers.Contructor<Func<SpawnUserServerMessage, S_SPAWN_USER>>()},
            {typeof(SNpcOccupierInfo), new Action<SNpcOccupierInfo>(x => DamageTracker.Instance.UpdateEntities(x))},
            {typeof(SDespawnNpc), Helpers.Contructor<Func<SDespawnNpc, S_DESPAWN_NPC>>()},
            {typeof(SCreatureLife), Helpers.Contructor<Func<SCreatureLife, S_CREATURE_LIFE>>()},
            {typeof(S_CREST_INFO), Helpers.Contructor<Func<S_CREST_INFO, Processing.S_CREST_INFO>>()},
            {typeof(SUserStatus), new Action<SUserStatus>(S_USER_STATUS.Process)}
//            {typeof(Tera.Game.Messages.S_BEGIN_THROUGH_ARBITER_CONTRACT), new Action<Tera.Game.Messages.S_BEGIN_THROUGH_ARBITER_CONTRACT>(x=>NotifyProcessor.S_BEGIN_THROUGH_ARBITER_CONTRACT(x))}
        };
        
        private static readonly Dictionary<Type, Delegate> MainProcessor = new Dictionary<Type, Delegate>();

        public PacketProcessingFactory()
        {
            MessageToProcessingInit.ToList().ForEach(x => MainProcessor[x.Key] = x.Value);
        }

        public void Update()
        {
            MainProcessor.Clear();
            MessageToProcessingInit.ToList().ForEach(x => MainProcessor[x.Key] = x.Value);
            MessageToProcessing.ToList().ForEach(x => MainProcessor[x.Key] = x.Value);
            UpdateEntityTracker();
            UpdatePlayerTracker();
            UpdateAbnormalityTracker();
            if (BasicTeraData.Instance.WindowData.EnableChat) { MessageToProcessingOptionnal.ToList().ForEach(x => MainProcessor[x.Key] = x.Value); }
            Paused = false;
        }

        public void Pause()
        {
            MainProcessor.Clear();
            MessageToProcessingPaused.ToList().ForEach(x => MainProcessor[x.Key] = x.Value);
            UpdatePlayerTracker();
            Paused = true;
        }

        public static void UpdateEntityTracker()
        {
            var entityTrackerProcessing = new Dictionary<Type, Delegate>
            {
                {typeof(S_BOSS_GAGE_INFO), new Action<S_BOSS_GAGE_INFO>(x => PacketProcessor.Instance.EntityTracker.Update(x))},
                {typeof(S_USER_LOCATION), new Action<S_USER_LOCATION>(x => PacketProcessor.Instance.EntityTracker.Update(x))},
                {typeof(SNpcLocation), new Action<SNpcLocation>(x => PacketProcessor.Instance.EntityTracker.Update(x))},
                {typeof(S_CREATURE_ROTATE), new Action<S_CREATURE_ROTATE>(x => PacketProcessor.Instance.EntityTracker.Update(x))},
                {typeof(S_INSTANT_MOVE), new Action<S_INSTANT_MOVE>(x => PacketProcessor.Instance.EntityTracker.Update(x))},
                {typeof(S_INSTANT_DASH), new Action<S_INSTANT_DASH>(x => PacketProcessor.Instance.EntityTracker.Update(x))},
                {typeof(S_ACTION_END), new Action<S_ACTION_END>(x => PacketProcessor.Instance.EntityTracker.Update(x))},
                {typeof(S_ACTION_STAGE), new Action<S_ACTION_STAGE>(x => PacketProcessor.Instance.EntityTracker.Update(x))},
                {typeof(S_CHANGE_DESTPOS_PROJECTILE), new Action<S_CHANGE_DESTPOS_PROJECTILE>(x => PacketProcessor.Instance.EntityTracker.Update(x))},
                {typeof(C_PLAYER_LOCATION), new Action<C_PLAYER_LOCATION>(x => PacketProcessor.Instance.EntityTracker.Update(x))},
                {typeof(S_MOUNT_VEHICLE_EX), new Action<S_MOUNT_VEHICLE_EX>(x => PacketProcessor.Instance.EntityTracker.Update(x))},
                {typeof(StartUserProjectileServerMessage), new Action<StartUserProjectileServerMessage>(x => PacketProcessor.Instance.EntityTracker.Update(x))},
                {typeof(SpawnProjectileServerMessage), new Action<SpawnProjectileServerMessage>(x => PacketProcessor.Instance.EntityTracker.Update(x))},
                {typeof(SpawnNpcServerMessage), new Action<SpawnNpcServerMessage>(S_SPAWN_NPC.Process)}
            };
            entityTrackerProcessing.ToList().ForEach(x => MainProcessor[x.Key] = x.Value);
        }

        public static void UpdatePlayerTracker()
        {
            var playerTrackerProcessing = new Dictionary<Type, Delegate>
            {
                {typeof(S_PARTY_MEMBER_LIST), new Action<S_PARTY_MEMBER_LIST>(x => PacketProcessor.Instance.PlayerTracker.UpdateParty(x))},
                {typeof(S_BAN_PARTY_MEMBER), new Action<S_BAN_PARTY_MEMBER>(x => PacketProcessor.Instance.PlayerTracker.UpdateParty(x))},
                {typeof(S_LEAVE_PARTY_MEMBER), new Action<S_LEAVE_PARTY_MEMBER>(x => PacketProcessor.Instance.PlayerTracker.UpdateParty(x))},
                {typeof(S_LEAVE_PARTY), new Action<S_LEAVE_PARTY>(x => PacketProcessor.Instance.PlayerTracker.UpdateParty(x))},
                {typeof(S_BAN_PARTY), new Action<S_BAN_PARTY>(x => PacketProcessor.Instance.PlayerTracker.UpdateParty(x))}
            };
            playerTrackerProcessing.ToList().ForEach(x => MainProcessor[x.Key] = x.Value);
        }

        public static void UpdateAbnormalityTracker()
        {
            var abnormalityTrackerProcessing = new Dictionary<Type, Delegate>
            {
                {typeof(SAbnormalityBegin), new Action<SAbnormalityBegin>(Abnormalities.Update)},
                {typeof(SAbnormalityEnd), new Action<SAbnormalityEnd>(Abnormalities.Update)},
                {typeof(SAbnormalityRefresh), new Action<SAbnormalityRefresh>(Abnormalities.Update)},
                {typeof(SpawnMeServerMessage), new Action<SpawnMeServerMessage>(x => PacketProcessor.Instance.AbnormalityTracker.Update(x))},
                {typeof(SCreatureChangeHp), Helpers.Contructor<Func<SCreatureChangeHp, S_CREATURE_CHANGE_HP>>()},
                {typeof(SPlayerChangeMp), new Action<SPlayerChangeMp>(x => PacketProcessor.Instance.AbnormalityTracker.Update(x))},
                {typeof(SPartyMemberChangeHp), new Action<SPartyMemberChangeHp>(x => PacketProcessor.Instance.AbnormalityTracker.Update(x))},
                {typeof(SDespawnUser), Helpers.Contructor<Func<SDespawnUser, S_DESPAWN_USER>>()},
                {typeof(SNpcStatus), new Action<SNpcStatus>(x => PacketProcessor.Instance.AbnormalityTracker.Update(x))},
                {typeof(S_PARTY_MEMBER_STAT_UPDATE), new Action<S_PARTY_MEMBER_STAT_UPDATE>(x => PacketProcessor.Instance.AbnormalityTracker.Update(x))},
                {typeof(S_PLAYER_STAT_UPDATE), new Action<S_PLAYER_STAT_UPDATE>(x => PacketProcessor.Instance.AbnormalityTracker.Update(x))}
            };
            abnormalityTrackerProcessing.ToList().ForEach(x => MainProcessor[x.Key] = x.Value);
        }

        public bool Process(ParsedMessage message)
        {
            MainProcessor.TryGetValue(message.GetType(), out Delegate type);
            if (type == null) { return false; }
            type.DynamicInvoke(message);
            return true;
        }
    }
}