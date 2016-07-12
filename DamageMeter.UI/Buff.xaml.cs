using System.Collections.Generic;
using System.Linq;
using DamageMeter.Database.Structures;
using DamageMeter.UI.EntityStats;
using Tera.Game.Abnormality;

namespace DamageMeter.UI
{
    /// <summary>
    ///     Logique d'interaction pour Buff.xaml
    /// </summary>
    public partial class Buff
    {
        private readonly List<EnduranceDebuff> _enduranceDebuffsList = new List<EnduranceDebuff>();

        private readonly EnduranceDebuffHeader _header;

        public Buff(PlayerDealt playerDealt, PlayerAbnormals buffs, EntityInformation entityInformation)
        {
            InitializeComponent();
            _header = new EnduranceDebuffHeader();
            ContentWidth = 1020;

            EnduranceAbnormality.Items.Clear();
            EnduranceAbnormality.Items.Add(_header);
            var counter = 0;
            foreach (var abnormality in buffs.Times.Where(x => x.Value.Duration(playerDealt.BeginTime, playerDealt.EndTime) > 0))
            {
                EnduranceDebuff abnormalityUi;
                if (_enduranceDebuffsList.Count > counter)
                {
                    abnormalityUi = _enduranceDebuffsList[counter];
                }
                else
                {
                    abnormalityUi = new EnduranceDebuff();
                    _enduranceDebuffsList.Add(abnormalityUi);
                }
                abnormalityUi.Update(abnormality.Key, abnormality.Value, playerDealt.BeginTime, playerDealt.EndTime);
                EnduranceAbnormality.Items.Add(abnormalityUi);

                counter++;
            }
        }

        public double ContentWidth { get; private set; }
    }
}