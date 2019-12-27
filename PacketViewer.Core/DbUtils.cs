using System.Collections.Generic;
using System.Linq;
using PacketViewer.Heuristic;
using Tera.Game;
using Tera.Game.Messages;

namespace PacketViewer
{
    public static class DbUtils
    {
        public static bool IsNpcSpawned(ulong id)
        {
            if (!OpcodeFinder.Instance.KnowledgeDatabase.ContainsKey(OpcodeFinder.KnowledgeDatabaseItem.SpawnedNpcs)) { return false; }
            var res = OpcodeFinder.Instance.KnowledgeDatabase[OpcodeFinder.KnowledgeDatabaseItem.SpawnedNpcs];
            var list = (List<Npc>)res;

            return list.Any(x => x.Cid == id);
        }
        public static bool IsNpcSpawned(ulong id, uint zoneId, uint templateId)
        {
            if (!OpcodeFinder.Instance.KnowledgeDatabase.ContainsKey(OpcodeFinder.KnowledgeDatabaseItem.SpawnedNpcs)) { return false; }
            var res = OpcodeFinder.Instance.KnowledgeDatabase[OpcodeFinder.KnowledgeDatabaseItem.SpawnedNpcs];
            var list = (List<Npc>)res;

            return list.Any(x => x.Cid == id && x.ZoneId == zoneId && x.TemplateId == templateId);
        }

        public static bool IsUserSpawned(ulong id)
        {
            if (!OpcodeFinder.Instance.KnowledgeDatabase.ContainsKey(OpcodeFinder.KnowledgeDatabaseItem.SpawnedUsers)) { return false; }
            var res = OpcodeFinder.Instance.KnowledgeDatabase[OpcodeFinder.KnowledgeDatabaseItem.SpawnedUsers];
            var list = (List<ulong>)res;
            return list.Any(x => x == id);
        }

        public static Vector3f GetPlayerLocation()
        {
            if (!OpcodeFinder.Instance.KnowledgeDatabase.ContainsKey(OpcodeFinder.KnowledgeDatabaseItem.PlayerLocation)) { return new Vector3f(); }
            var res = OpcodeFinder.Instance.KnowledgeDatabase[OpcodeFinder.KnowledgeDatabaseItem.PlayerLocation];
            return (Vector3f)res;
        }

        public static ulong GetPlayercId()
        {
            if (!OpcodeFinder.Instance.KnowledgeDatabase.TryGetValue(OpcodeFinder.KnowledgeDatabaseItem.LoggedCharacter, out var res)) { return 0; }
            var character = (LoggedCharacter)res;
            return character.Cid;
        }

        public static ulong GetPlayerModel()
        {
            if (!OpcodeFinder.Instance.KnowledgeDatabase.TryGetValue(OpcodeFinder.KnowledgeDatabaseItem.LoggedCharacter, out var res)) { return 0; }
            var character = (LoggedCharacter)res;
            return character.Model;
        }

        public static bool IsPartyMember(uint playerId)
        {
            if (!OpcodeFinder.Instance.KnowledgeDatabase.TryGetValue(OpcodeFinder.KnowledgeDatabaseItem.PartyMemberList, out var res)) { return false; }
            var list = (List<Heuristic.PartyMember>)res;
            return list.Any(x => x.PlayerId == playerId);
        }
        public static bool IsPartyMember(ulong cId)
        {
            if (!OpcodeFinder.Instance.KnowledgeDatabase.TryGetValue(OpcodeFinder.KnowledgeDatabaseItem.PartyMemberList, out var res)) { return false; }
            var list = (List<Heuristic.PartyMember>)res;
            return list.Any(x => x.Cid == cId);
        }
        public static bool IsPartyMember(uint playerId, uint serverId)
        {
            if (!OpcodeFinder.Instance.KnowledgeDatabase.TryGetValue(OpcodeFinder.KnowledgeDatabaseItem.PartyMemberList, out var res)) { return false; }
            var list = (List<Heuristic.PartyMember>)res;
            return list.Any(x => x.PlayerId == playerId && x.ServerId == serverId);
        }

        public static List<Npc> GetNpcList()
        {
            if (!OpcodeFinder.Instance.KnowledgeDatabase.ContainsKey(OpcodeFinder.KnowledgeDatabaseItem.SpawnedNpcs)) { return new List<Npc>(); }
            var res = OpcodeFinder.Instance.KnowledgeDatabase[OpcodeFinder.KnowledgeDatabaseItem.SpawnedNpcs];
            return (List<Npc>)res;

        }
        public static List<ulong> GetUserList()
        {
            if (!OpcodeFinder.Instance.KnowledgeDatabase.ContainsKey(OpcodeFinder.KnowledgeDatabaseItem.SpawnedUsers)) { return new List<ulong>(); }
            var res = OpcodeFinder.Instance.KnowledgeDatabase[OpcodeFinder.KnowledgeDatabaseItem.SpawnedUsers];
            return (List<ulong>)res;

        }
        public static List<Heuristic.PartyMember> GetPartyMembersList()
        {
            if (!OpcodeFinder.Instance.KnowledgeDatabase.ContainsKey(OpcodeFinder.KnowledgeDatabaseItem.PartyMemberList)) { return new List<Heuristic.PartyMember>(); }
            var res = OpcodeFinder.Instance.KnowledgeDatabase[OpcodeFinder.KnowledgeDatabaseItem.PartyMemberList];
            return (List<Heuristic.PartyMember>)res;

        }
        public static void AddPartyMemberAbnormal(uint playerId, uint serverId, uint abnormId)
        {
            if (!OpcodeFinder.Instance.KnowledgeDatabase.TryGetValue(OpcodeFinder.KnowledgeDatabaseItem.PartyMemberList, out var res)) { return; }
            var list = (List<Heuristic.PartyMember>)res;
            if (!list.Any(x => x.ServerId == serverId && x.PlayerId == playerId)) { return; }

            var member = list.FirstOrDefault(x => x.ServerId == serverId && x.PlayerId == playerId);
            if (member.Abnormals.Contains(abnormId)) return;
            list.Remove(member);
            member.Abnormals.Add(abnormId);
            list.Add(member);
            OpcodeFinder.Instance.KnowledgeDatabase[OpcodeFinder.KnowledgeDatabaseItem.PartyMemberList] = list;
        }
        public static void RemovePartyMemberAbnormal(uint playerId, uint serverId, uint abnormId)
        {
            if (!OpcodeFinder.Instance.KnowledgeDatabase.TryGetValue(OpcodeFinder.KnowledgeDatabaseItem.PartyMemberList, out var res)) { return; }
            var list = (List<Heuristic.PartyMember>)res;
            if (!list.Any(x => x.ServerId == serverId && x.PlayerId == playerId)) { return; }

            var member = list.FirstOrDefault(x => x.ServerId == serverId && x.PlayerId == playerId);
            if (!member.Abnormals.Contains(abnormId)) return;
            list.Remove(member);
            member.Abnormals.Remove(abnormId);
            list.Add(member);
            OpcodeFinder.Instance.KnowledgeDatabase[OpcodeFinder.KnowledgeDatabaseItem.PartyMemberList] = list;
        }

        public static bool PartyMemberHasAbnorm(uint playerId, uint serverId, uint abnormId)
        {
            if (!OpcodeFinder.Instance.KnowledgeDatabase.TryGetValue(OpcodeFinder.KnowledgeDatabaseItem.PartyMemberList, out var res)) { return false; }
            var list = (List<Heuristic.PartyMember>)res;
            if (!list.Any(x => x.ServerId == serverId && x.PlayerId == playerId)) { return false; }

            var member = list.FirstOrDefault(x => x.ServerId == serverId && x.PlayerId == playerId);
            return member.Abnormals.Contains(abnormId);

        }

        public static void UpdatePartyMemberMaxHp(uint playerId, uint serverId, uint maxHp)
        {
            if (!OpcodeFinder.Instance.KnowledgeDatabase.TryGetValue(OpcodeFinder.KnowledgeDatabaseItem.PartyMemberList, out var res)) { return; }
            var list = (List<Heuristic.PartyMember>)res;
            if (!list.Any(x => x.ServerId == serverId && x.PlayerId == playerId)) { return; }

            var member = list.FirstOrDefault(x => x.ServerId == serverId && x.PlayerId == playerId);
            list.Remove(member);
            member.MaxHp = maxHp;
            list.Add(member);
            OpcodeFinder.Instance.KnowledgeDatabase[OpcodeFinder.KnowledgeDatabaseItem.PartyMemberList] = list;
        }
        public static void UpdatePartyMemberMaxMp(uint playerId, uint serverId, uint maxMp)
        {
            if (!OpcodeFinder.Instance.KnowledgeDatabase.TryGetValue(OpcodeFinder.KnowledgeDatabaseItem.PartyMemberList, out var res)) { return; }
            var list = (List<Heuristic.PartyMember>)res;
            if (!list.Any(x => x.ServerId == serverId && x.PlayerId == playerId)) { return; }

            var member = list.FirstOrDefault(x => x.ServerId == serverId && x.PlayerId == playerId);
            list.Remove(member);
            member.MaxMp = maxMp;
            list.Add(member);
            OpcodeFinder.Instance.KnowledgeDatabase[OpcodeFinder.KnowledgeDatabaseItem.PartyMemberList] = list; 
        }
        public static void UpdatePartyMemberMaxRe(uint playerId, uint serverId, uint maxRe)
        {
            if (!OpcodeFinder.Instance.KnowledgeDatabase.TryGetValue(OpcodeFinder.KnowledgeDatabaseItem.PartyMemberList, out var res)) { return; }
            var list = (List<Heuristic.PartyMember>)res;
            if (!list.Any(x => x.ServerId == serverId && x.PlayerId == playerId)) { return; }

            var member = list.FirstOrDefault(x => x.ServerId == serverId && x.PlayerId == playerId);
            list.Remove(member);
            member.MaxRe = maxRe;
            list.Add(member);
            OpcodeFinder.Instance.KnowledgeDatabase[OpcodeFinder.KnowledgeDatabaseItem.PartyMemberList] = list;
        }

        public static Heuristic.PartyMember GetPartyMember(uint playerId, uint serverId)
        {
            if (!OpcodeFinder.Instance.KnowledgeDatabase.TryGetValue(OpcodeFinder.KnowledgeDatabaseItem.PartyMemberList, out var res)) { return new Heuristic.PartyMember(0,0,"",0); }
            var list = (List<Heuristic.PartyMember>)res;
            if (!list.Any(x => x.ServerId == serverId && x.PlayerId == playerId)) { return new Heuristic.PartyMember(0,0,"",0); }
            return  list.FirstOrDefault(x => x.ServerId == serverId && x.PlayerId == playerId);
        }

        public static bool IsPartyFormed()
        {
            return OpcodeFinder.Instance.KnowledgeDatabase.ContainsKey(OpcodeFinder.KnowledgeDatabaseItem.PartyMemberList);
        }

        public static bool IsFriend(uint playerId)
        {
            return S_FRIEND_LIST.Friends.ContainsKey(playerId);
        }
    }

}
