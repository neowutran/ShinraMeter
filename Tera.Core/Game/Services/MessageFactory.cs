using System;
using System.Collections.Generic;
using System.Reflection;
using Tera.Game.Messages;

namespace Tera.Game
{
    // Creates a ParsedMessage from a Message
    // Contains a mapping from OpCodeNames to message types and knows how to instantiate those
    // Since it works with OpCodeNames not numeric OpCodes, it needs an OpCodeNamer
    public class MessageFactory
    {
        private static readonly Dictionary<string, Type> OpcodeNameToType = new Dictionary<string, Type>
        {
            {"S_EACH_SKILL_RESULT", typeof (EachSkillResultServerMessage)},
            {"S_SPAWN_USER", typeof (SpawnUserServerMessage)},
            {"S_SPAWN_ME", typeof (SpawnMeServerMessage)},
            {"S_SPAWN_NPC", typeof (SpawnNpcServerMessage)},
            {"S_SPAWN_PROJECTILE", typeof (SpawnProjectileServerMessage)},
            {"S_LOGIN", typeof (LoginServerMessage)},
            {"S_START_USER_PROJECTILE", typeof (StartUserProjectileServerMessage)}
        };

        private readonly OpCodeNamer _opCodeNamer;

        public MessageFactory(OpCodeNamer opCodeNamer)
        {
            _opCodeNamer = opCodeNamer;

            foreach (var name in OpcodeNameToType.Keys)
            {
                opCodeNamer.GetCode(name);
            }
        }

        private ParsedMessage Instantiate(string opCodeName, TeraMessageReader reader)
        {
            Type type;
            if (!OpcodeNameToType.TryGetValue(opCodeName, out type))
                type = typeof (UnknownMessage);

            var constructor = type.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null,
                CallingConventions.Any, new[] {typeof (TeraMessageReader)}, null);
            if (constructor == null)
                throw new Exception("Constructor not found");
            return (ParsedMessage) constructor.Invoke(new object[] {reader});
        }

        public ParsedMessage Create(Message message)
        {
            var reader = new TeraMessageReader(message, _opCodeNamer);
            var opCodeName = _opCodeNamer.GetName(message.OpCode);
            return Instantiate(opCodeName, reader);
        }
    }
}