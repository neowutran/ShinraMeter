using System;
using System.Collections.Generic;
using DamageMeter.UI.EntityStats;
using Tera.Game;

namespace DamageMeter.UI
{
    /// <summary>
    ///     Logique d'interaction pour Buff.xaml
    /// </summary>
    public partial class Buff
    {
        private readonly List<EnduranceDebuff> _enduranceDebuffsList = new List<EnduranceDebuff>();

        private readonly EnduranceDebuffHeader _header;

        public Buff(PlayerInfo playerInfo, PlayerAbnormals buffs, Entity currentBoss)
        {
            InitializeComponent();
            _header = new EnduranceDebuffHeader();
            ContentWidth = 1020;
            Update(playerInfo, buffs, currentBoss);
        }

        public double ContentWidth { get; private set; }

        public void Update(PlayerInfo playerInfo, PlayerAbnormals buffs, Entity currentBoss)
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
                abnormalityUi.Update(abnormality.Key, abnormality.Value, playerInfo.Dealt.FirstHit(currentBoss)*TimeSpan.TicksPerSecond,
                    (playerInfo.Dealt.LastHit(currentBoss)+1)*TimeSpan.TicksPerSecond-1);
                EnduranceAbnormality.Items.Add(abnormalityUi);

                counter++;
            }
        }
    }
}