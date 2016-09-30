using Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Tera.Game;

namespace DamageMeter
{
    // Creates a ParsedMessage from a Message
    // Contains a mapping from OpCodeNames to message types and knows how to instantiate those
    // Since it works with OpCodeNames not numeric OpCodes, it needs an OpCodeNamer
    public class PacketProcessingFactory
    {
        private static readonly Dictionary<Type, Type> MessageToProcessingInit = new Dictionary<Type, Type>
        {
            {typeof(Tera.Game.Messages.S_GET_USER_LIST), typeof(Processing.S_GET_USER_LIST)},
            {typeof(Tera.Game.Messages.S_GET_USER_GUILD_LOGO), typeof(Processing.S_GET_USER_GUILD_LOGO)},
            {typeof(Tera.Game.Messages.C_CHECK_VERSION) , typeof(Processing.C_CHECK_VERSION)},
            {typeof(Tera.Game.Messages.LoginServerMessage), typeof(Processing.S_LOGIN)},
        };

        private static readonly Dictionary<Type, Type> MessageToProcessingOptionnal = new Dictionary<Type, Type>
        {
            {typeof(Tera.Game.Messages.S_CHAT), typeof(Processing.Chat)},
            {typeof(Tera.Game.Messages.S_WHISPER), typeof(Processing.Chat)},
            {typeof(Tera.Game.Messages.S_TRADE_BROKER_DEAL_SUGGESTED), typeof(Processing.S_TRADE_BROKER_DEAL_SUGGESTED)},
            {typeof(Tera.Game.Messages.S_OTHER_USER_APPLY_PARTY) , typeof(Processing.S_OTHER_USER_APPLY_PARTY)},
            {typeof(Tera.Game.Messages.S_PRIVATE_CHAT) , typeof(Processing.Chat)},
            {typeof(Tera.Game.Messages.S_FIN_INTER_PARTY_MATCH), typeof(Processing.InstanceMatchingSuccess) },
            {typeof(Tera.Game.Messages.S_BATTLE_FIELD_ENTRANCE_INFO), typeof(Processing.InstanceMatchingSuccess) },
            {typeof(Tera.Game.Messages.S_REQUEST_CONTRACT), typeof(Processing.S_REQUEST_CONTRACT) },
            {typeof(Tera.Game.Messages.S_CHECK_TO_READY_PARTY), typeof(Processing.S_CHECK_TO_READY_PARTY) },
            {typeof(Tera.Game.Messages.S_GUILD_QUEST_LIST), typeof(Processing.S_GUILD_QUEST_LIST) }
        };

        private static readonly Dictionary<Type, Type> MessageToProcessing = new Dictionary<Type, Type>
        {
            {typeof(Tera.Game.Messages.EachSkillResultServerMessage), typeof(Processing.S_EACH_SKILL_RESULT)},
            {typeof(Tera.Game.Messages.SpawnUserServerMessage), typeof(Processing.S_SPAWN_USER)},
            {typeof(Tera.Game.Messages.SpawnMeServerMessage), typeof(Processing.S_SPAWN_ME)},
            {typeof(Tera.Game.Messages.SCreatureChangeHp) , typeof(Processing.S_CREATURE_CHANGE_HP)},
            {typeof(Tera.Game.Messages.SNpcOccupierInfo), typeof(Processing.S_NPC_OCCUPIER_INFO)},
            {typeof(Tera.Game.Messages.SAbnormalityBegin), typeof(Processing.Abnormality)},
            {typeof(Tera.Game.Messages.SAbnormalityEnd), typeof(Processing.Abnormality)},
            {typeof(Tera.Game.Messages.SAbnormalityRefresh), typeof(Processing.Abnormality)},
            {typeof(Tera.Game.Messages.SDespawnNpc), typeof(Processing.S_DESPAWN_NPC)},
            {typeof(Tera.Game.Messages.SPlayerChangeMp), typeof(Processing.S_PLAYER_CHANGE_MP)},
            {typeof(Tera.Game.Messages.SPartyMemberChangeHp), typeof(Processing.S_PARTY_MEMBER_CHANGE_HP)},
            {typeof(Tera.Game.Messages.SDespawnUser), typeof(Processing.S_DESPAWN_USER)},
            {typeof(Tera.Game.Messages.SCreatureLife), typeof(Processing.S_CREATURE_LIFE)},
            {typeof(Tera.Game.Messages.SNpcStatus), typeof(Processing.S_NPC_STATUS)},
            {typeof(Tera.Game.Messages.SAddCharmStatus), typeof(Processing.Charm)},
            {typeof(Tera.Game.Messages.SEnableCharmStatus), typeof(Processing.Charm)},
            {typeof(Tera.Game.Messages.SRemoveCharmStatus), typeof(Processing.S_REMOVE_CHARM_STATUS)},
            {typeof(Tera.Game.Messages.SResetCharmStatus), typeof(Processing.Charm)},
            {typeof(Tera.Game.Messages.SPartyMemberCharmAdd), typeof(Processing.Charm)},
            {typeof(Tera.Game.Messages.SPartyMemberCharmDel), typeof(Processing.Charm)},
            {typeof(Tera.Game.Messages.SPartyMemberCharmEnable), typeof(Processing.Charm)},
            {typeof(Tera.Game.Messages.SPartyMemberCharmReset), typeof(Processing.Charm)},
            {typeof(Tera.Game.Messages.S_PARTY_MEMBER_STAT_UPDATE), typeof(Processing.S_PARTY_MEMBER_STAT_UPDATE)},
            {typeof(Tera.Game.Messages.S_PLAYER_STAT_UPDATE), typeof(Processing.S_PLAYER_STAT_UPDATE)},        
            {typeof(Tera.Game.Messages.S_CREST_INFO), typeof(Processing.S_CREST_INFO) },
        };

        private Dictionary<Type, Type> MessageToProcessingInitAndCommon = new Dictionary<Type, Type>();

        public PacketProcessingFactory()
        {
            MessageToProcessingInitAndCommon = MessageToProcessing.Union(MessageToProcessingInit).ToDictionary(x => x.Key, y => y.Value);
        }

        public bool Process(Tera.Game.Messages.ParsedMessage message)
        {
            Type type = null;
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

            var constructor = type.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null,
                CallingConventions.Any, new[] { message.GetType() }, null);
            if (constructor == null)
                throw new Exception("Constructor not found");
            constructor.Invoke(new object[] { message });
            return true;
        }

    }
}