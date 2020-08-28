using DamageMeter.D3D9Render.TeraData;
using DamageMeter.Database.Structures;
using DamageMeter.UI.Windows;
using Data;
using Lang;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Tera.Game;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using Point = System.Windows.Point;

namespace DamageMeter.UI
{
    public sealed partial class MainWindow
    {
        // ========================================================================
        public MainViewModel DC { get; set; } // Temporary reference, remove after.
        // ========================================================================

        private readonly DispatcherTimer _dispatcherTimer;

        internal bool ForceHidden;
        public Dictionary<Player, PlayerStats> Controls { get; set; } = new Dictionary<Player, PlayerStats>();

        public MainWindow()
        {
            App.SplashScreen?.SetText("Initializing main window...");

            // ========================================================================
            DC = new MainViewModel(); // todo: Temporary reference, remove after (assign to DataContext directly).
            DataContext = DC;
            // ========================================================================
            App.HudContainer.MainWindow = this;

            InitializeComponent();

            if (BasicTeraData.Instance.WindowData.InvisibleUi)
                HideWindow();
            if (BasicTeraData.Instance.WindowData.ClickThrou)
                SetClickThrou();
            Topmost = BasicTeraData.Instance.WindowData.Topmost;
            ShowInTaskbar = !BasicTeraData.Instance.WindowData.Topmost;

            Console.WriteLine("SuperUniqueStringEasilyDetectableToBeAbleToRandomizeTheProgramAndBypassSignatureBasedBlock");

            PacketProcessor.Instance.TickUpdated += Update;

            _dispatcherTimer = new DispatcherTimer();
            _dispatcherTimer.Tick += UpdateKeyboard;
            _dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
            _dispatcherTimer.Start();

            //UpdateComboboxEncounter(new List<NpcEntity>(), null);

            SettingsWindowViewModel.WindowScaleChanged += OnScaleChanged;

            App.SplashScreen?.CloseWindowSafe();
        }

        private void ListEncounterOnPreviewKeyDown(object sender, KeyEventArgs keyEventArgs)
        {
            keyEventArgs.Handled = true;
        }
        public void UpdateKeyboard(object o, EventArgs args)
        {
            var teraWindowActive = TeraWindow.IsTeraActive();
            var meterWindowActive = TeraWindow.IsMeterActive();

            if (KeyboardHook.Instance.SetHotkeys(teraWindowActive))
                App.HudContainer.StayTopMost();

            if (!BasicTeraData.Instance.WindowData.AlwaysVisible)
            {
                if (!teraWindowActive && !meterWindowActive)
                {
                    HideWindow();
                    ForceHidden = true;
                }

                if ((meterWindowActive || teraWindowActive) && (BasicTeraData.Instance.WindowData.InvisibleUi && Controls.Count > 0 ||
                                                                !BasicTeraData.Instance.WindowData.InvisibleUi))
                {
                    ForceHidden = false;
                    ShowWindow();
                }
            }
            else
            {
                ForceHidden = false;
            }
        }
        public void Update(UiUpdateMessage nmessage)
        {
            void ChangeUi(UiUpdateMessage message)
            {
                //UpdateComboboxEncounter(message.Entities, message.StatsSummary.EntityInformation.Entity);

                #region PlayerStats

                var visiblePlayerStats = new HashSet<Player>();
                var statsDamage = message.StatsSummary.PlayerDamageDealt;
                var statsHeal = message.StatsSummary.PlayerHealDealt;
                foreach (var playerStats in statsDamage)
                {
                    Controls.TryGetValue(playerStats.Source, out var playerStatsControl);
                    if (playerStats.Amount == 0) { continue; }

                    visiblePlayerStats.Add(playerStats.Source);
                    if (playerStatsControl != null) { continue; }
                    playerStatsControl = new PlayerStats(playerStats, statsHeal.FirstOrDefault(x => x.Source == playerStats.Source), message.StatsSummary.EntityInformation,
                        message.Skills, message.Abnormals.Get(playerStats.Source));
                    Controls.Add(playerStats.Source, playerStatsControl);
                }


                var invisibleControls = Controls.Where(x => !visiblePlayerStats.Contains(x.Key)).ToList();
                foreach (var invisibleControl in invisibleControls)
                {
                    Controls[invisibleControl.Key].CloseSkills();
                    Controls.Remove(invisibleControl.Key);
                }

                Players.Items.Clear();

                foreach (var item in statsDamage)
                {
                    if (!Controls.ContainsKey(item.Source)) { continue; }
                    if (Players.Items.Contains(Controls[item.Source]))
                    {
                        BasicTeraData.LogError("duplicate playerinfo: \r\n" + string.Join("\r\n ", statsDamage.Select(x => x.Source.ToString() + " ->  " + x.Amount)), false, true);
                        continue;
                    }
                    Players.Items.Add(Controls[item.Source]);
                    Controls[item.Source].Repaint(item, statsHeal.FirstOrDefault(x => x.Source == item.Source), message.StatsSummary.EntityInformation, message.Skills,
                        message.Abnormals.Get(item.Source), message.TimedEncounter);
                }
                #endregion

                #region WindowVisibility
                if (BasicTeraData.Instance.WindowData.InvisibleUi && !DC.Paused)
                {
                    if (Controls.Count > 0 && !ForceHidden && Visibility != Visibility.Visible) { ShowWindow(); }
                    if (Controls.Count == 0 && Visibility != Visibility.Hidden) { HideWindow(); }
                }
                else if (!ForceHidden && Visibility != Visibility.Visible)
                {
                    ShowWindow();
                }

                #endregion

            }

            Dispatcher.Invoke((PacketProcessor.UpdateUiHandler)ChangeUi, nmessage);
        }
        //private bool NeedUpdateEncounter(IReadOnlyList<NpcEntity> entities)
        //{
        //    if (entities.Count != ListEncounter.Items.Count - 1) { return true; }
        //    for (var i = 1; i < ListEncounter.Items.Count - 1; i++)
        //    {
        //        if ((NpcEntity)((ComboBoxItem)ListEncounter.Items[i]).Content != entities[i - 1]) { return true; }
        //    }
        //    return false;
        //}
        //private bool ChangeEncounterSelection(NpcEntity entity)
        //{
        //    if (entity == null) { return false; }

        //    for (var i = 1; i < ListEncounter.Items.Count; i++)
        //    {
        //        if ((NpcEntity)((ComboBoxItem)ListEncounter.Items[i]).Content != entity) { continue; }
        //        ListEncounter.SelectedItem = ListEncounter.Items[i];
        //        return true;
        //    }
        //    return false;
        //}
        //private void UpdateComboboxEncounter(IReadOnlyList<NpcEntity> entities, NpcEntity currentBoss)
        //{
        //    //http://stackoverflow.com/questions/12164488/system-reflection-targetinvocationexception-occurred-in-presentationframework
        //    if (ListEncounter == null || !ListEncounter.IsLoaded) { return; }

        //    if (!NeedUpdateEncounter(entities))
        //    {
        //        ChangeEncounterSelection(currentBoss);
        //        return;
        //    }

        //    NpcEntity selectedEntity = null;
        //    if ((ComboBoxItem)ListEncounter.SelectedItem != null && !(((ComboBoxItem)ListEncounter.SelectedItem).Content is string))
        //    {
        //        selectedEntity = (NpcEntity)((ComboBoxItem)ListEncounter.SelectedItem).Content;
        //    }

        //    ListEncounter.Items.Clear();
        //    ListEncounter.Items.Add(new ComboBoxItem { Content = LP.TotalEncounter });
        //    var selected = false;
        //    foreach (var entity in entities)
        //    {
        //        var item = new ComboBoxItem { Content = entity };
        //        ListEncounter.Items.Add(item);
        //        if (entity != selectedEntity) { continue; }
        //        ListEncounter.SelectedItem = item;
        //        selected = true;
        //    }

        //    if (ChangeEncounterSelection(currentBoss)) { return; }
        //    if (selected) { return; }
        //    ListEncounter.SelectedItem = ListEncounter.Items[0];
        //}
        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (BasicTeraData.Instance.WindowData.RememberPosition)
            {
                LastSnappedPoint = BasicTeraData.Instance.WindowData.Location;
                Left = LastSnappedPoint?.X ?? 0;
                Top = LastSnappedPoint?.Y ?? 0;
                _dragged = true;
                SnapToScreen();
                return;
            }
            Top = 0;
            Left = 0;

        }
        private void MainWindow_OnClosed(object sender, EventArgs e)
        {
            foreach (Window win in App.Current.Windows)
            {
                if (!(win is ClickThrouWindow ctw)) continue;

                ctw.DontClose = false;
                ctw.Close();
            }
        }
        private void ListEncounter_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count != 1) { return; }

            NpcEntity encounter = null;
            if (e.AddedItems[0] is NpcEntity en && en != MainViewModel.TotalEncounter)
            {
                encounter = en;
            }

            if (encounter != PacketProcessor.Instance.Encounter)
            {
                PacketProcessor.Instance.NewEncounter = encounter;
            }
        }
        private void ListEncounter_OnDropDownOpened(object sender, EventArgs e)
        {
            App.HudContainer.TopMostOverride = false;
        }
        private void ListEncounter_OnDropDownClosed(object sender, EventArgs e)
        {
            App.HudContainer.TopMostOverride = true;
        }

        public void Dispose()
        {
            ForceHidden = true;
            PacketProcessor.Instance.TickUpdated -= Update;
            _dispatcherTimer.Stop();
        }

        #region Done

        private readonly DoubleAnimation _expandFooterAnim = new DoubleAnimation(31, TimeSpan.FromMilliseconds(250)) { EasingFunction = new QuadraticEase() };
        private readonly DoubleAnimation _shrinkFooterAnim = new DoubleAnimation(0, TimeSpan.FromMilliseconds(250)) { EasingFunction = new QuadraticEase() };
        private void OnGraphMouseLeave(object sender, MouseEventArgs e)
        {
            App.HudContainer.TopMostOverride = true;
        }
        private void OnGraphMouseEnter(object sender, MouseEventArgs e)
        {
            App.HudContainer.TopMostOverride = false;
        }
        private void MainWindow_OnMouseEnter(object sender, MouseEventArgs e)
        {
            Footer.BeginAnimation(HeightProperty, _expandFooterAnim);
        }
        private void MainWindow_OnMouseLeave(object sender, MouseEventArgs e)
        {
            Footer.BeginAnimation(HeightProperty, _shrinkFooterAnim);
        }
        public override void SetClickThrou()
        {
            base.SetClickThrou();
            foreach (var players in Controls) { players.Value.SetClickThrou(); }
            // todo: EntityStatsImage.Source = BasicTeraData.Instance.ImageDatabase.EntityStatsClickThrou.Source;
        }
        public override void UnsetClickThrou()
        {
            base.UnsetClickThrou();
            foreach (var players in Controls) { players.Value.UnsetClickThrou(); }
            //todo: EntityStatsImage.Source = BasicTeraData.Instance.ImageDatabase.EntityStats.Source;
        }
        private void OnScaleChanged(double val)
        {
            Scale = val;
        }

        public override void SaveWindowPos()
        {
            BasicTeraData.Instance.WindowData.Location = LastSnappedPoint ?? new Point(Left, Top);
        }

        #endregion
    }

    public static class Extensions
    {
        public static List<ClassInfo> ToClassInfo(this IEnumerable<PlayerDamageDealt> data, long sum, long interval)
        {
            // return linq expression method
            return data.Select(dealt => new ClassInfo
            {
                PName = $"{dealt.Source.Name}",
                PDmg = $"{FormatHelpers.Instance.FormatPercent((double)dealt.Amount / sum)}",
                PDsp =
                    $"{FormatHelpers.Instance.FormatValue(interval == 0 ? dealt.Amount : dealt.Amount * TimeSpan.TicksPerSecond / interval)}{LP.PerSecond}",
                PCrit = $"{Math.Round(dealt.CritRate)}%",
                PId = dealt.Source.PlayerId
            }).ToList();
        }
    }

    public static class ComboBoxBehavior
    {
        public static bool GetFixWin10HilightBug(ComboBox cb)
        {
            return (bool)cb.GetValue(FixWin10HilightBugProperty);
        }

        public static void SetFixWin10HilightBug(ComboBox cb, bool value)
        {
            cb.SetValue(FixWin10HilightBugProperty, value);
        }


        public static readonly DependencyProperty FixWin10HilightBugProperty =
            DependencyProperty.RegisterAttached(
                "FixWin10HilightBug",
                typeof(bool),
                typeof(ComboBoxBehavior),
                new UIPropertyMetadata(false, OnFixWin10HilightBugChanged));

        private static void OnFixWin10HilightBugChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            //.NET 4.6 bug: https://connect.microsoft.com/VisualStudio/feedback/details/1660886/system-windows-controls-combobox-coerceisselectionboxhighlighted-bug
            if (Environment.OSVersion.Version.Major < 10) return;
            if (!(d is ComboBox cb)) return;
            if (!(e.NewValue is bool b)) return;

            if (b)
                cb.DropDownClosed += OnDropDownClosed;
            else
                cb.DropDownClosed -= OnDropDownClosed;
        }

        private static void OnDropDownClosed(object sender, EventArgs e)
        {
            typeof(ComboBox).GetField("_highlightedInfo", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(sender, null);
        }
    }
}