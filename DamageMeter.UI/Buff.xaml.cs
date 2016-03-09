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

        public Buff(PlayerInfo playerInfo)
        {
            InitializeComponent();
            _header = new EnduranceDebuffHeader();
            ContentWidth = 980;
            Update(playerInfo);
        }

        public double ContentWidth { get; private set; }

        public void Update(PlayerInfo playerInfo)
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
                abnormalityUi.Update(abnormality.Key, abnormality.Value, playerInfo.Dealt.FirstHit,
                    playerInfo.Dealt.LastHit);
                EnduranceAbnormality.Items.Add(abnormalityUi);

                counter++;
            }
        }
    }
}