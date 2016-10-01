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
            {typeof(Tera.Game.Messages.S_GET_USER_LIST), Helpers.Contructor<Func<Tera.Game.Messages.S_GET_USER_LIST, DamageMeter.Processing.S_GET_USER_LIST>>()},
            {typeof(Tera.Game.Messages.S_GET_USER_GUILD_LOGO), Helpers.Contructor<Func<Tera.Game.Messages.S_GET_USER_GUILD_LOGO, DamageMeter.Processing.S_GET_USER_GUILD_LOGO>>()},
            {typeof(Tera.Game.Messages.C_CHECK_VERSION) , Helpers.Contructor<Func<Tera.Game.Messages.C_CHECK_VERSION, DamageMeter.Processing.C_CHECK_VERSION>>()},
            {typeof(Tera.Game.Messages.LoginServerMessage), Helpers.Contructor<Func<Tera.Game.Messages.LoginServerMessage, DamageMeter.Processing.S_LOGIN>>()}
        };

        private static readonly Dictionary<Type, Delegate> MessageToProcessingOptionnal = new Dictionary<Type, Delegate>
        {
            {typeof(Tera.Game.Messages.S_CHAT), Helpers.InstanceMethod<Action<Tera.Game.Messages.S_CHAT>>(DamageMeter.Chat.Instance,"Add") },
            {typeof(Tera.Game.Messages.S_WHISPER), Helpers.InstanceMethod<Action<Tera.Game.Messages.S_WHISPER>>(DamageMeter.Chat.Instance,"Add") },
            {typeof(Tera.Game.Messages.S_TRADE_BROKER_DEAL_SUGGESTED), Helpers.Contructor<Func<Tera.Game.Messages.S_TRADE_BROKER_DEAL_SUGGESTED , DamageMeter.Processing.S_TRADE_BROKER_DEAL_SUGGESTED>>()},
            {typeof(Tera.Game.Messages.S_OTHER_USER_APPLY_PARTY) , Helpers.Contructor<Func<Tera.Game.Messages.S_OTHER_USER_APPLY_PARTY , DamageMeter.Processing.S_OTHER_USER_APPLY_PARTY>>()},
            {typeof(Tera.Game.Messages.S_PRIVATE_CHAT) , Helpers.InstanceMethod<Action<Tera.Game.Messages.S_PRIVATE_CHAT>>(DamageMeter.Chat.Instance,"Add") },
            {typeof(Tera.Game.Messages.S_FIN_INTER_PARTY_MATCH), Helpers.Contructor<Func<Tera.Game.Messages.S_FIN_INTER_PARTY_MATCH , InstanceMatchingSuccess>>() },
            {typeof(Tera.Game.Messages.S_BATTLE_FIELD_ENTRANCE_INFO), Helpers.Contructor<Func<Tera.Game.Messages.S_BATTLE_FIELD_ENTRANCE_INFO , InstanceMatchingSuccess>>() },
            {typeof(Tera.Game.Messages.S_REQUEST_CONTRACT), Helpers.Contructor<Func<Tera.Game.Messages.S_REQUEST_CONTRACT , DamageMeter.Processing.S_REQUEST_CONTRACT>>() },
            {typeof(Tera.Game.Messages.S_CHECK_TO_READY_PARTY), Helpers.Contructor<Func<Tera.Game.Messages.S_CHECK_TO_READY_PARTY , DamageMeter.Processing.S_CHECK_TO_READY_PARTY>>() },
            {typeof(Tera.Game.Messages.S_GUILD_QUEST_LIST), Helpers.Contructor<Func<Tera.Game.Messages.S_GUILD_QUEST_LIST , DamageMeter.Processing.S_GUILD_QUEST_LIST>>() }
        };

        private static readonly Dictionary<Type, Delegate> MessageToProcessing = new Dictionary<Type, Delegate>
        {
            {typeof(Tera.Game.Messages.EachSkillResultServerMessage), Helpers.Contructor<Func<Tera.Game.Messages.EachSkillResultServerMessage , S_EACH_SKILL_RESULT>>()},
            {typeof(Tera.Game.Messages.SpawnUserServerMessage), Helpers.Contructor<Func<Tera.Game.Messages.SpawnUserServerMessage , S_SPAWN_USER>>()},
            {typeof(Tera.Game.Messages.SpawnMeServerMessage), Helpers.Contructor<Func<Tera.Game.Messages.SpawnMeServerMessage , S_SPAWN_ME>>()},
            {typeof(Tera.Game.Messages.SCreatureChangeHp) , Helpers.Contructor<Func<Tera.Game.Messages.SCreatureChangeHp , S_CREATURE_CHANGE_HP>>()},
            {typeof(Tera.Game.Messages.SNpcOccupierInfo), Helpers.Contructor<Func<Tera.Game.Messages.SNpcOccupierInfo , S_NPC_OCCUPIER_INFO>>()},
            {typeof(Tera.Game.Messages.SAbnormalityBegin), Helpers.Contructor<Func<Tera.Game.Messages.SAbnormalityBegin , Abnormality>>()},
            {typeof(Tera.Game.Messages.SAbnormalityEnd), Helpers.Contructor<Func<Tera.Game.Messages.SAbnormalityEnd , Abnormality>>()},
            {typeof(Tera.Game.Messages.SAbnormalityRefresh), Helpers.Contructor<Func<Tera.Game.Messages.SAbnormalityRefresh , Abnormality>>()},
            {typeof(Tera.Game.Messages.SDespawnNpc), Helpers.Contructor<Func<Tera.Game.Messages.SDespawnNpc , S_DESPAWN_NPC>>()},
            {typeof(Tera.Game.Messages.SPlayerChangeMp), Helpers.Contructor<Func<Tera.Game.Messages.SPlayerChangeMp , S_PLAYER_CHANGE_MP>>()},
            {typeof(Tera.Game.Messages.SPartyMemberChangeHp), Helpers.Contructor<Func<Tera.Game.Messages.SPartyMemberChangeHp , S_PARTY_MEMBER_CHANGE_HP>>()},
            {typeof(Tera.Game.Messages.SDespawnUser), Helpers.Contructor<Func<Tera.Game.Messages.SDespawnUser , S_DESPAWN_USER>>()},
            {typeof(Tera.Game.Messages.SCreatureLife), Helpers.Contructor<Func<Tera.Game.Messages.SCreatureLife , S_CREATURE_LIFE>>()},
            {typeof(Tera.Game.Messages.SNpcStatus), Helpers.Contructor<Func<Tera.Game.Messages.SNpcStatus , S_NPC_STATUS>>()},
            {typeof(Tera.Game.Messages.SAddCharmStatus), Helpers.Contructor<Func<Tera.Game.Messages.SAddCharmStatus , Charm>>()},
            {typeof(Tera.Game.Messages.SEnableCharmStatus), Helpers.Contructor<Func<Tera.Game.Messages.SEnableCharmStatus , Charm>>()},
            {typeof(Tera.Game.Messages.SRemoveCharmStatus), Helpers.Contructor<Func<Tera.Game.Messages.SRemoveCharmStatus , Charm>>()},
            {typeof(Tera.Game.Messages.SResetCharmStatus), Helpers.Contructor<Func<Tera.Game.Messages.SResetCharmStatus , Charm>>()},
            {typeof(Tera.Game.Messages.SPartyMemberCharmAdd), Helpers.Contructor<Func<Tera.Game.Messages.SPartyMemberCharmAdd , Charm>>()},
            {typeof(Tera.Game.Messages.SPartyMemberCharmDel), Helpers.Contructor<Func<Tera.Game.Messages.SPartyMemberCharmDel , Charm>>()},
            {typeof(Tera.Game.Messages.SPartyMemberCharmEnable), Helpers.Contructor<Func<Tera.Game.Messages.SPartyMemberCharmEnable , Charm>>()},
            {typeof(Tera.Game.Messages.SPartyMemberCharmReset), Helpers.Contructor<Func<Tera.Game.Messages.SPartyMemberCharmReset , Charm>>()},
            {typeof(Tera.Game.Messages.S_PARTY_MEMBER_STAT_UPDATE), Helpers.Contructor<Func<Tera.Game.Messages.S_PARTY_MEMBER_STAT_UPDATE , DamageMeter.Processing.S_PARTY_MEMBER_STAT_UPDATE>>()},
            {typeof(Tera.Game.Messages.S_PLAYER_STAT_UPDATE), Helpers.Contructor<Func<Tera.Game.Messages.S_PLAYER_STAT_UPDATE , DamageMeter.Processing.S_PLAYER_STAT_UPDATE>>()},        
            {typeof(Tera.Game.Messages.S_CREST_INFO), Helpers.Contructor<Func<Tera.Game.Messages.S_CREST_INFO , DamageMeter.Processing.S_CREST_INFO>>() },
        };

        private static Dictionary<Type, Delegate> MessageToProcessingInitAndCommon = new Dictionary<Type, Delegate>();

        public PacketProcessingFactory()
        {
            MessageToProcessingInitAndCommon = MessageToProcessing.Union(MessageToProcessingInit).ToDictionary(x => x.Key, y => y.Value);
        }

        public bool Process(Tera.Game.Messages.ParsedMessage message)
        {
            Delegate type = null;
            if (NetworkController.Instance.NeedInit)
            {
                MessageToProcessingInit.TryGetValue(message.GetType(), out type);
            }
            else
            {
                MessageToProcessingInitAndCommon.TryGetValue(message.GetType(), out type);
                if (BasicTeraData.Instance.WindowData.EnableChat && type == null)
                {
                    MessageToProcessingOptionnal.TryGetValue(message.GetType(), out type);
                }
            }

            if (type == null) return false;
            type.DynamicInvoke(message);
            return true;
        }

    }
}