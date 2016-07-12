using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DamageMeter.UI
{
    /// <summary>
    /// Logique d'interaction pour SkillsLog.xaml
    /// </summary>
    public partial class SkillsLog
    {
    
        private readonly List<SkillLog> _skillList = new List<SkillLog>();


        public SkillsLog(IEnumerable<Database.Structures.Skill> skills, bool received)
        {
            InitializeComponent();
            ContentWidth = 1500;
            Skills.Items.Clear();

            var counter = 0;
            foreach (var skill in skills)
            {
                SkillLog log;
                if (_skillList.Count > counter)
                {
                    log = _skillList[counter];
                }
                else
                {
                    log = new SkillLog();
                    _skillList.Add(log);
                }
                log.Update(skill, received);
                Skills.Items.Add(log);
                counter++;
            }
        }

        public double ContentWidth { get; private set; }
    }
}
