using Data;
using System;
using System.Collections.Generic;
using System.Linq;
using DamageMeter.Processing;
using Tera.Game;

namespace DamageMeter
{
    // Creates a ParsedMessage from a Message
    // Contains a mapping from OpCodeNames to message types and knows how to instantiate those
    // Since it works with OpCodeNames not numeric OpCodes, it needs an OpCodeNamer
    public class PacketProcessingFactory
    {
        private static readonly Dictionary<Type, Delegate> MessageToProcessingInit = new Dictionary<Type, Delegate>
        {
            {typeof(Tera.Game.Messages.S_GET_USER_LIST), new Action<Tera.Game.Messages.S_GET_USER_LIST>(x=>NetworkController.Instance.UserLogoTracker.SetUserList(x))},
            {typeof(Tera.Game.Messages.S_GET_USER_GUILD_LOGO), new Action<Tera.Game.Messages.S_GET_USER_GUILD_LOGO>(x=>NetworkController.Instance.UserLogoTracker.AddLogo(x))},
            {typeof(Tera.Game.Messages.C_CHECK_VERSION) , Helpers.Contructor<Func<Tera.Game.Messages.C_CHECK_VERSION, DamageMeter.Processing.C_CHECK_VERSION>>()},
            {typeof(Tera.Game.Messages.LoginServerMessage), Helpers.Contructor<Func<Tera.Game.Messages.LoginServerMessage, DamageMeter.Processing.S_LOGIN>>()}
        };
        private static readonly Dictionary<Type, Delegate> MessageToProcessingOptionnal = new Dictionary<Type, Delegate>
        {
            {typeof(Tera.Game.Messages.S_AVAILABLE_EVENT_MATCHING_LIST), new Action<Tera.Game.Messages.S_AVAILABLE_EVENT_MATCHING_LIST> (x=>NotifyProcessor.Instance.UpdateCredits(x)) },
            {typeof(Tera.Game.Messages.S_UPDATE_NPCGUILD), new Action<Tera.Game.Messages.S_UPDATE_NPCGUILD> (x=>NotifyProcessor.Instance.UpdateCredits(x)) },
            {typeof(Tera.Game.Messages.S_BOSS_GAGE_INFO) , new Action<Tera.Game.Messages.S_BOSS_GAGE_INFO>((x)=>NotifyProcessor.Instance.S_BOSS_GAGE_INFO(x)) },//override with optional processing
            {typeof(Tera.Game.Messages.S_LOAD_TOPO) , new Action<Tera.Game.Messages.S_LOAD_TOPO>((x)=>NotifyProcessor.Instance.S_LOAD_TOPO(x)) },
            {typeof(Tera.Game.Messages.S_CHAT), new Action<Tera.Game.Messages.S_CHAT>(x=> DamageMeter.Chat.Instance.Add(x))},
            {typeof(Tera.Game.Messages.S_WHISPER), new Action<Tera.Game.Messages.S_WHISPER>(x=>DamageMeter.Chat.Instance.Add(x))},
            {typeof(Tera.Game.Messages.S_TRADE_BROKER_DEAL_SUGGESTED), new Action<Tera.Game.Messages.S_TRADE_BROKER_DEAL_SUGGESTED> (x=>NotifyProcessor.Instance.S_TRADE_BROKER_DEAL_SUGGESTED(x)) },
            {typeof(Tera.Game.Messages.S_OTHER_USER_APPLY_PARTY) , new Action<Tera.Game.Messages.S_OTHER_USER_APPLY_PARTY>(x=> NotifyProcessor.Instance.S_OTHER_USER_APPLY_PARTY(x)) },
            {typeof(Tera.Game.Messages.S_PRIVATE_CHAT) , new Action<Tera.Game.Messages.S_PRIVATE_CHAT>(x=>DamageMeter.Chat.Instance.Add(x))},
            {typeof(Tera.Game.Messages.S_FIN_INTER_PARTY_MATCH), new Action<Tera.Game.Messages.S_FIN_INTER_PARTY_MATCH>(x=> NotifyProcessor.Instance.InstanceMatchingSuccess(x)) },
            {typeof(Tera.Game.Messages.S_BATTLE_FIELD_ENTRANCE_INFO), new Action<Tera.Game.Messages.S_BATTLE_FIELD_ENTRANCE_INFO>(x=>NotifyProcessor.Instance.InstanceMatchingSuccess(x)) },
            {typeof(Tera.Game.Messages.S_REQUEST_CONTRACT), new Action<Tera.Game.Messages.S_REQUEST_CONTRACT>(x=>NotifyProcessor.Instance.S_REQUEST_CONTRACT(x)) },
            {typeof(Tera.Game.Messages.S_CHECK_TO_READY_PARTY), new Action<Tera.Game.Messages.S_CHECK_TO_READY_PARTY>(x=>NotifyProcessor.Instance.S_CHECK_TO_READY_PARTY(x)) },
            {typeof(Tera.Game.Messages.S_GUILD_QUEST_LIST), Helpers.Contructor<Func<Tera.Game.Messages.S_GUILD_QUEST_LIST , DamageMeter.Processing.S_GUILD_QUEST_LIST>>() },
            {typeof(Tera.Game.Messages.S_CREST_MESSAGE), new Action<Tera.Game.Messages.S_CREST_MESSAGE>((x)=>NotifyProcessor.Instance.SkillReset(x.SkillId, x.Type)) },

        };
        private static readonly Dictionary<Type, Delegate> MessageToProcessing = new Dictionary<Type, Delegate>
        {
            {typeof(Tera.Game.Messages.EachSkillResultServerMessage), Helpers.Contructor<Func<Tera.Game.Messages.EachSkillResultServerMessage , S_EACH_SKILL_RESULT>>()},
            {typeof(Tera.Game.Messages.SpawnUserServerMessage), Helpers.Contructor<Func<Tera.Game.Messages.SpawnUserServerMessage , S_SPAWN_USER>>()},
            {typeof(Tera.Game.Messages.SNpcOccupierInfo), new Action<Tera.Game.Messages.SNpcOccupierInfo>(x=>DamageTracker.Instance.UpdateEntities(new NpcOccupierResult(x), x.Time.Ticks))},
            {typeof(Tera.Game.Messages.SDespawnNpc), Helpers.Contructor<Func<Tera.Game.Messages.SDespawnNpc , S_DESPAWN_NPC>>()},
            {typeof(Tera.Game.Messages.SCreatureLife), Helpers.Contructor<Func<Tera.Game.Messages.SCreatureLife , S_CREATURE_LIFE>>()},
            {typeof(Tera.Game.Messages.S_CREST_INFO), Helpers.Contructor<Func<Tera.Game.Messages.S_CREST_INFO , S_CREST_INFO>>() },
            {typeof(Tera.Game.Messages.SUserStatus), new Action<Tera.Game.Messages.SUserStatus>(x=>S_USER_STATUS.Process(x))}
//            {typeof(Tera.Game.Messages.S_BEGIN_THROUGH_ARBITER_CONTRACT), new Action<Tera.Game.Messages.S_BEGIN_THROUGH_ARBITER_CONTRACT>(x=>NotifyProcessor.S_BEGIN_THROUGH_ARBITER_CONTRACT(x))}
        };
        private static Dictionary<Type, Delegate> MainProcessor = new Dictionary<Type, Delegate>();

        public void Update()
        {
            MainProcessor.Clear();
            MessageToProcessingInit.ToList().ForEach(x => MainProcessor[x.Key] = x.Value);
            MessageToProcessing.ToList().ForEach(x => MainProcessor[x.Key] = x.Value);
            UpdateEntityTracker();
            UpdatePlayerTracker();
            UpdateAbnormalityTracker();
            if (BasicTeraData.Instance.WindowData.EnableChat) MessageToProcessingOptionnal.ToList().ForEach(x => MainProcessor[x.Key] = x.Value);
        }

        public PacketProcessingFactory()
        {
            MessageToProcessingInit.ToList().ForEach(x => MainProcessor[x.Key] = x.Value); ;
        }

        public static void UpdateEntityTracker()
        {
            var entityTrackerProcessing = new Dictionary<Type, Delegate>
            {
            { typeof(Tera.Game.Messages.S_GUILD_INFO) , new Action<Tera.Game.Messages.S_GUILD_INFO>((x)=>NetworkController.Instance.EntityTracker.Update(x)) },
            { typeof(Tera.Game.Messages.S_BOSS_GAGE_INFO) , new Action<Tera.Game.Messages.S_BOSS_GAGE_INFO>((x)=>NetworkController.Instance.EntityTracker.Update(x)) },
            { typeof(Tera.Game.Messages.S_USER_LOCATION) , new Action<Tera.Game.Messages.S_USER_LOCATION>((x)=>NetworkController.Instance.EntityTracker.Update(x)) },
            { typeof(Tera.Game.Messages.SNpcLocation) , new Action<Tera.Game.Messages.SNpcLocation>((x)=>NetworkController.Instance.EntityTracker.Update(x)) },
            { typeof(Tera.Game.Messages.S_CREATURE_ROTATE) , new Action<Tera.Game.Messages.S_CREATURE_ROTATE>((x)=>NetworkController.Instance.EntityTracker.Update(x)) },
            { typeof(Tera.Game.Messages.S_INSTANT_MOVE) , new Action<Tera.Game.Messages.S_INSTANT_MOVE>((x)=>NetworkController.Instance.EntityTracker.Update(x)) },
            { typeof(Tera.Game.Messages.S_INSTANT_DASH) , new Action<Tera.Game.Messages.S_INSTANT_DASH>((x)=>NetworkController.Instance.EntityTracker.Update(x)) },
            { typeof(Tera.Game.Messages.S_ACTION_END) , new Action<Tera.Game.Messages.S_ACTION_END>((x)=>NetworkController.Instance.EntityTracker.Update(x)) },
            { typeof(Tera.Game.Messages.S_ACTION_STAGE) , new Action<Tera.Game.Messages.S_ACTION_STAGE>((x)=>NetworkController.Instance.EntityTracker.Update(x)) },
            { typeof(Tera.Game.Messages.S_CHANGE_DESTPOS_PROJECTILE) , new Action<Tera.Game.Messages.S_CHANGE_DESTPOS_PROJECTILE>((x)=>NetworkController.Instance.EntityTracker.Update(x)) },
            { typeof(Tera.Game.Messages.C_PLAYER_LOCATION) , new Action<Tera.Game.Messages.C_PLAYER_LOCATION>((x)=>NetworkController.Instance.EntityTracker.Update(x)) },
            { typeof(Tera.Game.Messages.S_MOUNT_VEHICLE_EX) , new Action<Tera.Game.Messages.S_MOUNT_VEHICLE_EX>((x)=>NetworkController.Instance.EntityTracker.Update(x)) },
            { typeof(Tera.Game.Messages.StartUserProjectileServerMessage) , new Action<Tera.Game.Messages.StartUserProjectileServerMessage>((x)=>NetworkController.Instance.EntityTracker.Update(x)) },
            { typeof(Tera.Game.Messages.SpawnProjectileServerMessage) , new Action<Tera.Game.Messages.SpawnProjectileServerMessage>((x)=>NetworkController.Instance.EntityTracker.Update(x)) },
            { typeof(Tera.Game.Messages.SpawnNpcServerMessage) , new Action<Tera.Game.Messages.SpawnNpcServerMessage>((x)=>S_SPAWN_NPC.Process(x)) },
            };
            entityTrackerProcessing.ToList().ForEach(x => MainProcessor[x.Key] = x.Value);
        }
        public static void UpdatePlayerTracker()
        {
            var playerTrackerProcessing = new Dictionary<Type, Delegate>
            {
            {typeof(Tera.Game.Messages.S_PARTY_MEMBER_LIST) , new Action<Tera.Game.Messages.S_PARTY_MEMBER_LIST>((x)=>NetworkController.Instance.PlayerTracker.UpdateParty(x)) },
            {typeof(Tera.Game.Messages.S_BAN_PARTY_MEMBER) , new Action<Tera.Game.Messages.S_BAN_PARTY_MEMBER>((x)=>NetworkController.Instance.PlayerTracker.UpdateParty(x)) },
            {typeof(Tera.Game.Messages.S_LEAVE_PARTY_MEMBER) , new Action<Tera.Game.Messages.S_LEAVE_PARTY_MEMBER>((x)=>NetworkController.Instance.PlayerTracker.UpdateParty(x)) },
            {typeof(Tera.Game.Messages.S_LEAVE_PARTY) , new Action<Tera.Game.Messages.S_LEAVE_PARTY>((x)=>NetworkController.Instance.PlayerTracker.UpdateParty(x)) },
            {typeof(Tera.Game.Messages.S_BAN_PARTY) , new Action<Tera.Game.Messages.S_BAN_PARTY>((x)=>NetworkController.Instance.PlayerTracker.UpdateParty(x)) },
                };
            playerTrackerProcessing.ToList().ForEach(x => MainProcessor[x.Key] = x.Value);
        }
        public static void UpdateAbnormalityTracker()
        {
            var abnormalityTrackerProcessing = new Dictionary<Type, Delegate>
            {
            { typeof(Tera.Game.Messages.SAbnormalityBegin) , new Action<Tera.Game.Messages.SAbnormalityBegin>((x)=> Abnormalities.Update(x)) },
            { typeof(Tera.Game.Messages.SAbnormalityEnd) , new Action<Tera.Game.Messages.SAbnormalityEnd>((x)=>Abnormalities.Update(x)) },
            { typeof(Tera.Game.Messages.SAbnormalityRefresh) , new Action<Tera.Game.Messages.SAbnormalityRefresh>((x)=>Abnormalities.Update(x)) },
            { typeof(Tera.Game.Messages.SpawnMeServerMessage) , new Action<Tera.Game.Messages.SpawnMeServerMessage>((x)=>NetworkController.Instance.AbnormalityTracker.Update(x)) },
            { typeof(Tera.Game.Messages.SCreatureChangeHp) , new Action<Tera.Game.Messages.SCreatureChangeHp>((x)=>NetworkController.Instance.AbnormalityTracker.Update(x)) },
            { typeof(Tera.Game.Messages.SPlayerChangeMp) , new Action<Tera.Game.Messages.SPlayerChangeMp>((x)=>NetworkController.Instance.AbnormalityTracker.Update(x)) },
            { typeof(Tera.Game.Messages.SPartyMemberChangeHp) , new Action<Tera.Game.Messages.SPartyMemberChangeHp>((x)=>NetworkController.Instance.AbnormalityTracker.Update(x)) },
            { typeof(Tera.Game.Messages.SDespawnUser) , Helpers.Contructor<Func<Tera.Game.Messages.SDespawnUser , S_DESPAWN_USER>>()},
            { typeof(Tera.Game.Messages.SNpcStatus) , new Action<Tera.Game.Messages.SNpcStatus>((x)=>NetworkController.Instance.AbnormalityTracker.Update(x)) },
            { typeof(Tera.Game.Messages.S_PARTY_MEMBER_STAT_UPDATE) , new Action<Tera.Game.Messages.S_PARTY_MEMBER_STAT_UPDATE>((x)=>NetworkController.Instance.AbnormalityTracker.Update(x)) },
            { typeof(Tera.Game.Messages.S_PLAYER_STAT_UPDATE) , new Action<Tera.Game.Messages.S_PLAYER_STAT_UPDATE>((x)=>NetworkController.Instance.AbnormalityTracker.Update(x)) },
            };
            abnormalityTrackerProcessing.ToList().ForEach(x => MainProcessor[x.Key] = x.Value);
        }

        public bool Process(Tera.Game.Messages.ParsedMessage message)
        {
            Delegate type;
            MainProcessor.TryGetValue(message.GetType(), out type);
            if (type == null) return false;
            type.DynamicInvoke(message);
            return true;
        }
    }
}