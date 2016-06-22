using System;
using System.Collections.Generic;
using DamageMeter.UI.EntityStats;
using Tera.Game;
using DamageMeter.Database.Structures;

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
            Update(playerDealt, buffs, entityInformation);
        }

        public double ContentWidth { get; private set; }

        public void Update(PlayerDealt playerDealt, PlayerAbnormals buffs, EntityInformation entityInformation)
        {
            EnduranceAbnormality.Items.Clear();

            EnduranceAbnormality.Items.Add(_header);
            var counter = 0;
            foreach (var abnormality in buffs.Times)
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
                abnormalityUi.Update(abnormality.Key, abnormality.Value, playerDealt.BeginTime,playerDealt.EndTime);
                EnduranceAbnormality.Items.Add(abnormalityUi);

                counter++;
            }
        }
    }
}