using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using DamageMeter.Skills.Skill;
using Data;

namespace DamageMeter.UI
{
    /// <summary>
    ///     Logique d'interaction pour Skills.xaml
    /// </summary>
    public partial class Skills
    {
        private readonly Buff _buff;
        private readonly PlayerStats _parent;
        private readonly SkillsDetail _skillDps;
        private readonly SkillsDetail _skillHeal;
        private readonly SkillsDetail _skillMana;


        public Skills(Dictionary<long, Dictionary<DamageMeter.Skills.Skill.Skill, SkillStats>> timedSkills,
            Dictionary<long, Dictionary<DamageMeter.Skills.Skill.Skill, SkillStats>> timedAllSkills, PlayerStats parent,
            PlayerInfo playerInfo)
        {
            InitializeComponent();

            var skills = NoTimedSkills(timedSkills);
            var allSkills = NoTimedSkills(timedAllSkills);


            _skillDps = new SkillsDetail(skills, SkillsDetail.Type.Dps);
            _skillHeal = new SkillsDetail(allSkills, SkillsDetail.Type.Heal);
            _skillMana = new SkillsDetail(allSkills, SkillsDetail.Type.Mana);
            _buff = new Buff(playerInfo);
            HealPanel.Content = _skillHeal;
            DpsPanel.Content = _skillDps;
            ManaPanel.Content = _skillMana;
            BuffPanel.Content = _buff;
            TabControl.SelectionChanged += TabControlOnSelectionChanged;
            _parent = parent;
            BackgroundColor.Opacity = BasicTeraData.Instance.WindowData.SkillWindowOpacity;
        }

        private Dictionary<DamageMeter.Skills.Skill.Skill, SkillStats> NoTimedSkills(
            Dictionary<long, Dictionary<DamageMeter.Skills.Skill.Skill, SkillStats>> dictionary)
        {
            var result = new Dictionary<DamageMeter.Skills.Skill.Skill, SkillStats>();
            foreach (var timedStats in dictionary)
            {
                foreach (var stats in timedStats.Value)
                {
                    if (result.ContainsKey(stats.Key))
                    {
                        result[stats.Key] += stats.Value;
                        continue;
                    }
                    result.Add(stats.Key, stats.Value);
                }
            }
            return result;
        }

        public void SetClickThrou()
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            WindowsServices.SetWindowExTransparent(hwnd);
        }

        public void UnsetClickThrou()
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            WindowsServices.SetWindowExVisible(hwnd);
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);

            //Set the window style to noactivate.
            var helper = new WindowInteropHelper(this);
            SetWindowLong(helper.Handle, GWL_EXSTYLE,
                GetWindowLong(helper.Handle, GWL_EXSTYLE) | WS_EX_NOACTIVATE);
        }

        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_NOACTIVATE = 0x08000000;

        [DllImport("user32.dll")]
        public static extern IntPtr SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll")]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        private void TabControlOnSelectionChanged(object sender, SelectionChangedEventArgs selectionChangedEventArgs)
        {
            var tabitem = (TabItem) ((TabControl) selectionChangedEventArgs.Source).SelectedItem;

            double width = 0;
            if (tabitem.Content is SkillsDetail)
            {
                width = ((SkillsDetail) tabitem.Content).ContentWidth;
            }
            else
            {
                width = ((Buff) tabitem.Content).ContentWidth;
            }

            MaxWidth = width + 50;
            MinWidth = width - 300;
            Width = width - 300;
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            var source = HwndSource.FromHwnd(new WindowInteropHelper(this).Handle);
            source.AddHook(new HwndSourceHook(WindowsServices.ClickNoFocus));
        }


        public void Update(Dictionary<long, Dictionary<DamageMeter.Skills.Skill.Skill, SkillStats>> timedSkills,
            Dictionary<long, Dictionary<DamageMeter.Skills.Skill.Skill, SkillStats>> timedAllSkills,
            PlayerInfo playerinfo)
        {
            // Console.WriteLine("thread id:"+Thread.CurrentThread.ManagedThreadId);
            var skills = NoTimedSkills(timedSkills);
            var allSkills = NoTimedSkills(timedAllSkills);
            _buff.Update(playerinfo);
            _skillDps.Update(skills);
            _skillHeal.Update(new Dictionary<DamageMeter.Skills.Skill.Skill, SkillStats>(allSkills));
            _skillMana.Update(new Dictionary<DamageMeter.Skills.Skill.Skill, SkillStats>(allSkills));
            HealPanel.Content = _skillHeal;
            DpsPanel.Content = _skillDps;
            ManaPanel.Content = _skillMana;
            BuffPanel.Content = _buff;
        }

        private void Button_OnClick(object sender, RoutedEventArgs e)
        {
            _parent.CloseSkills();
        }

        private void Skills_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                DragMove();
            }
            catch
            {
                Console.WriteLine(@"Exception move");
            }
        }
    }
}