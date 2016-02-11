using System.Collections.Generic;
using System.Linq;
using DamageMeter.UI.EntityStats;

namespace DamageMeter.UI
{
    /// <summary>
    /// Logique d'interaction pour Buff.xaml
    /// </summary>
    public partial class Buff
    {
     
        private readonly List<EnduranceDebuff> _enduranceDebuffsList = new List<EnduranceDebuff>();

        private readonly EnduranceDebuffHeader _header;

        public Buff(PlayerInfo playerInfo)
        {
            InitializeComponent();
            _header = new EnduranceDebuffHeader();
            ContentWidth = 880;
            Update(playerInfo);
        }

        public double ContentWidth { get; private set; }

        public void Update(PlayerInfo playerInfo)
        {
            EnduranceAbnormality.Items.Clear();
            
           EnduranceAbnormality.Items.Add(_header);

            for (var i = 0; i < playerInfo.AbnormalityTime.Count; i++)
            {
                EnduranceDebuff abnormality;
                if (_enduranceDebuffsList.Count > i)
                {
                    abnormality = _enduranceDebuffsList[i];
                    abnormality.Update(playerInfo.AbnormalityTime.Keys.ElementAt(i),
                        playerInfo.AbnormalityTime.Values.ElementAt(i), playerInfo.Dealt.FirstHit, playerInfo.Dealt.LastHit);
                }
                else
                {
                    abnormality = new EnduranceDebuff();
                    abnormality.Update(playerInfo.AbnormalityTime.Keys.ElementAt(i),
                        playerInfo.AbnormalityTime.Values.ElementAt(i), playerInfo.Dealt.FirstHit, playerInfo.Dealt.LastHit);
                    _enduranceDebuffsList.Add(abnormality);
                }

                EnduranceAbnormality.Items.Add(abnormality);
            }
        }

    }
}
