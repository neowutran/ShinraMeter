using System.Collections.Generic;
using DamageMeter.UI.EntityStats;

namespace DamageMeter.UI
{
    /// <summary>
    ///     Logique d'interaction pour Buff.xaml
    /// </summary>
    public partial class Buff
    {
        private readonly List<EnduranceDebuff> _enduranceDebuffsList = new List<EnduranceDebuff>();

        private readonly EnduranceDebuffHeader _header;

        public Buff(PlayerInfo playerInfo, Entity currentBoss)
        {
            InitializeComponent();
            _header = new EnduranceDebuffHeader();
            ContentWidth = 1020;
            Update(playerInfo, currentBoss);
        }

        public double ContentWidth { get; private set; }

        public void Update(PlayerInfo playerInfo, Entity currentBoss)
        {
            EnduranceAbnormality.Items.Clear();

            EnduranceAbnormality.Items.Add(_header);
            var counter = 0;
            foreach (var abnormality in playerInfo.AbnormalityTime)
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
                abnormalityUi.Update(abnormality.Key, abnormality.Value, playerInfo.Dealt.FirstHit(currentBoss),
                    playerInfo.Dealt.LastHit(currentBoss));
                EnduranceAbnormality.Items.Add(abnormalityUi);

                counter++;
            }
        }
    }
}