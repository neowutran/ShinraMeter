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
    

        public SkillsLog(IEnumerable<Database.Structures.Skill> skills, bool received)
        {
            InitializeComponent();
            ContentWidth = 900;
            foreach (var skill in skills.OrderByDescending(x => x.Time))
            {   
                var log = new SkillLog();                
                log.Update(skill, received);
                Skills.Items.Add(log);
            }
        }

        public double ContentWidth { get; private set; }
    }
}
