using System.Collections.Generic;
using Tera.Data;
using Tera.Game;
using Tera.Game.Messages;
using Tera.Sniffing;

namespace Tera.DamageMeter.UI.Handler
{
    public class UiModel
    {
        public delegate void ConnectedHandler(string serverName);

        public delegate void DataUpdatedHandler(IEnumerable<PlayerInfo> data);

        private static UiModel _instance;
        private static TeraData _teraData;

        private DamageTracker _damageTracker;
        private EntityTracker _entityTracker;
        private MessageFactory _messageFactory;
        private PlayerTracker _playerTracker;

        private UiModel()
        {
            TeraSniffer.Instance.MessageReceived += HandleMessageReceived;
            TeraSniffer.Instance.NewConnection += HandleNewConnection;
        }

        public static UiModel Instance => _instance ?? (_instance = new UiModel());

        public event DataUpdatedHandler DataUpdated;
        public event ConnectedHandler Connected;


        private void HandleNewConnection(Server server)
        {
            //  Text = $"{server.Name}";
            _teraData = BasicTeraData.Instance.DataForRegion(server.Region);

            _entityTracker = new EntityTracker();
            _playerTracker = new PlayerTracker(_entityTracker);
            _damageTracker = new DamageTracker();
            _messageFactory = new MessageFactory(_teraData.OpCodeNamer);

            var handler = Connected;

            handler?.Invoke(server.Name);
        }

        public void Reset()
        {
            _damageTracker = new DamageTracker();
            var handler = DataUpdated;
            handler?.Invoke(_damageTracker);
        }

        private void HandleMessageReceived(Message obj)
        {
            var message = _messageFactory.Create(obj);
            _entityTracker.Update(message);


            //  Console.WriteLine(message.OpCodeName);
            if (message.OpCodeName == "S_ABNORMALITY_BEGIN")
            {
                // CAN SEE DOT HERE
                // target of the dot: 19eme byte, just after that: owner of the dot
                /*
                   var data = message.Data;
                    foreach (var partdata in data)
                    {
                        var str = String.Format("{0:x}", partdata);
                        Console.Write(str+"-");
                    }
                    Console.WriteLine("something");
                    */
            }
            /*  if (message.OpCodeName == "S_NPC_STATUS")
              {

                  var data = message.Data;
                  foreach (var partdata in data)
                   {
                       Console.Write(partdata+"-");
                   }
                   Console.WriteLine("some");
              }
              */
            var skillResultMessage = message as EachSkillResultServerMessage;
            if (skillResultMessage != null)
            {
                //DOT doesn't go here
                var skillResult = new SkillResult(skillResultMessage, _entityTracker, _playerTracker,
                    _teraData.SkillDatabase);
                _damageTracker.Update(skillResult);


                var handler = DataUpdated;
                handler?.Invoke(_damageTracker);
            }
        }
    }
}