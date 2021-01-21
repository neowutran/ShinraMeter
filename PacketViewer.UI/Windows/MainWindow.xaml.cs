using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using DamageMeter.Sniffing;
using Microsoft.Win32;
using Tera;
using Tera.Game.Messages;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using OpcodeId = System.UInt16;
using Point = System.Windows.Point;

namespace PacketViewer.UI
{
    /// <summary>
    ///     Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : INotifyPropertyChanged
    {
        private bool _topMost = true;
        private int _count;
        private int _queued;
        private bool _bottom;
        private bool _newMessagesBelow;
        private PacketViewModel _packetDetails;
        private int _sizeFilter = -1;
        private int _currentSelectedItemIndex = 0;
        private string _currentFile;
        private DispatcherTimer _dbUpdateTimer;
        public int Queued
        {
            get => _queued;
            set
            {
                _queued = value;
                OnPropertyChanged(nameof(Queued));
            }
        }
        public PacketViewModel PacketDetails
        {
            get => _packetDetails;
            set
            {
                if (_packetDetails == value) return;
                _packetDetails = value;
                OnPropertyChanged(nameof(PacketDetails));
            }
        }
        public bool NewMessagesBelow
        {
            get => _newMessagesBelow;
            set
            {
                if (_newMessagesBelow == value) return;
                _newMessagesBelow = value;
                OnPropertyChanged(nameof(NewMessagesBelow));
            }
        }
        public ObservableDictionary<ushort, OpcodeEnum> Known { get; set; } = new ObservableDictionary<ushort, OpcodeEnum>();
        public ObservableCollection<PacketViewModel> All { get; set; } = new ObservableCollection<PacketViewModel>();
        public ObservableCollection<OpcodeToFindVm> OpcodesToFind { get; set; } = new ObservableCollection<OpcodeToFindVm>();
        public ObservableCollection<FilteredOpcodeVm> FilteredOpcodes { get; set; } = new ObservableCollection<FilteredOpcodeVm>();
        public List<PacketViewModel> SearchList { get; set; } = new List<PacketViewModel>();
        public DatabaseVm Database { get; } = new DatabaseVm();
        public MainWindow()
        {
            InitializeComponent();
            // Handler for exceptions in threads behind forms.
            NetworkController.Instance.Sniffer.Enabled = true;
            //TeraSniffer.Instance.Warning += PcapWarning;
            NetworkController.Instance.Connected += HandleConnected;
            NetworkController.Instance.GuildIconAction += InstanceOnGuildIconAction;
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.AboveNormal;
            Title = "Opcode Searcher V0";
            SystemEvents.SessionEnding += new SessionEndingEventHandler(SystemEvents_SessionEnding);
            NetworkController.Instance.TickUpdated += (msg) => Dispatcher.BeginInvoke(new Action(() => HandleNewMessage(msg)), DispatcherPriority.Background);
            NetworkController.Instance.ResetUi += () => Dispatcher.Invoke(() =>
            {
                All.Clear();
                Known.Clear();
                OpcodeNameConv.Instance.Clear();
            });
            All.CollectionChanged += All_CollectionChanged;
            DataContext = this;
            //((ItemsControl)KnownSw.Content).ItemsSource = Known;
            _dbUpdateTimer = new DispatcherTimer();
            _dbUpdateTimer.Interval = TimeSpan.FromSeconds(1);
            _dbUpdateTimer.Tick += (sender, args) => Dispatcher.Invoke(()=>
            {
                Database.RefreshDatabase();
                OnPropertyChanged(nameof(Database));
            });

        }

        private void CheckFindList(ushort opcode, OpcodeEnum opname)
        {
            var opc = OpcodesToFind.FirstOrDefault(x => x.OpcodeName == opname.ToString());
            if (opc == null) { return; }
            if (opc.Opcode == 0)
            {
                opc.Opcode = opcode;
                opc.Confirmed = true;
            }
            else
            {
                if (opc.Opcode == opcode) { opc.Confirmed = true; }
                else
                {
                    opc.Mismatching = opcode;
                }
            }

        }
        private void HandleNewMessage(Tuple<List<ParsedMessage>, Dictionary<OpcodeId, OpcodeEnum>, int> update)
        {
            Queued = update.Item3;
            if (update.Item2.Count != 0)
            {
                foreach (var opcode in update.Item2)
                {
                    Dispatcher.Invoke(() =>
                    {
                        Known.Add(opcode.Key, opcode.Value);
                        CheckFindList(opcode.Key, opcode.Value);
                    });

                    OpcodeNameConv.Instance.Known.Add(opcode.Key, opcode.Value);
                    foreach (var packetViewModel in All.Where(x => x.Message.OpCode == opcode.Key))
                    {
                        packetViewModel.RefreshName();
                    }
                }
                KnownSw.ScrollToBottom();
            }

            foreach (var msg in update.Item1)
            {
                _count++;
                if (msg.Direction == MessageDirection.ServerToClient && ServerCb.IsChecked == false) return;
                if (msg.Direction == MessageDirection.ClientToServer && ClientCb.IsChecked == false) return;
                if (FilteredOpcodes.Count(x => x.Mode == FilterMode.ShowOnly) > 0 && FilteredOpcodes.Where(x => x.Mode == FilterMode.ShowOnly).All(x => x.Opcode != msg.OpCode)) return;
                if (FilteredOpcodes.Any(x => x.Opcode == msg.OpCode && x.Mode == FilterMode.Exclude)) return;
                if (SpamCb.IsChecked == true && All.Count > 0 && All.Last().Message.OpCode == msg.OpCode) return;
                if (_sizeFilter != -1)
                {
                    if (msg.Payload.Count != _sizeFilter) return;
                }
                var vm = new PacketViewModel(msg, _count);
                All.Add(vm);
                if (SearchList.Count > 0)
                {
                    if (SearchList[0].Message.OpCode == msg.OpCode) UpdateSearch(msg.OpCode.ToString(), false); //could be performance intensive
                }
            }
            //Dispatcher.Invoke(() =>
            //{
            //    Database.RefreshDatabase();
            //    OnPropertyChanged(nameof(Database));
            //});
        }

        private void AllSw_ScrollChanged(object sender, System.Windows.Controls.ScrollChangedEventArgs e)
        {
            var b = VisualTreeHelper.GetChild(AllItemsControl, 0) as Border;
            var sw = VisualTreeHelper.GetChild(b, 0) as ScrollViewer;

            if (sw.VerticalOffset == sw.ScrollableHeight)
            {
                _bottom = true;
                NewMessagesBelow = false;
            }
            else _bottom = false;
        }
        private void All_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            var b = VisualTreeHelper.GetChild(AllItemsControl, 0) as Border;
            var sw = VisualTreeHelper.GetChild(b, 0) as ScrollViewer;

            if (_bottom) Dispatcher.Invoke(() => sw.ScrollToBottom());
            else
            {
                NewMessagesBelow = true;
            }
        }
        private void SystemEvents_SessionEnding(object sender, SessionEndingEventArgs e)
        {
            Exit();
        }

        private void InstanceOnGuildIconAction(Bitmap icon)
        {
            void ChangeUi(Bitmap bitmap)
            {
                //Icon = bitmap?.ToImageSource() ?? BasicTeraData.Instance.ImageDatabase.Icon;
                //NotifyIcon.Tray.Icon = bitmap?.GetIcon() ?? BasicTeraData.Instance.ImageDatabase.Tray;
            }

            Dispatcher.Invoke((NetworkController.GuildIconEvent)ChangeUi, icon);
        }

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            Top = 0;
            Left = 0;
            _dbUpdateTimer.Stop();
            _dbUpdateTimer.Start();

        }
        private void MainWindow_OnClosed(object sender, EventArgs e)
        {
            Exit();
        }

        public void VerifyClose()
        {
            Close();
        }

        public void Exit()
        {
            _topMost = false;
            NetworkController.Instance.Exit();
        }

        public void HandleConnected(string serverName)
        {
            void ChangeTitle(string newServerName)
            {
                Title = newServerName;
            }

            Dispatcher.Invoke((ChangeTitle)ChangeTitle, serverName);
        }

        internal void StayTopMost()
        {
            if (!_topMost || !Topmost)
            {
                Debug.WriteLine("Not topmost");
                return;
            }
            foreach (Window window in System.Windows.Application.Current.Windows)
            {
                window.Topmost = false;
                window.Topmost = true;
            }
        }

        private delegate void ChangeTitle(string servername);

        private void MessageClick(object sender, MouseButtonEventArgs e)
        {
            var s = ((Grid)sender);
            PacketDetails = s.DataContext as PacketViewModel;
            foreach (var packetViewModel in All) { packetViewModel.IsSelected = packetViewModel.Message == PacketDetails.Message; }
            OpcodeToFilter.Text = PacketDetails.Message.OpCode.ToString();

        }
        private void HexSwChanged(object sender, ScrollChangedEventArgs e)
        {
            var s = sender as ScrollViewer;
            if (s.Name == nameof(HexSw)) TextSw.ScrollToVerticalOffset(HexSw.VerticalOffset);
            else HexSw.ScrollToVerticalOffset(TextSw.VerticalOffset);
        }

        private void ClearAll(object sender, RoutedEventArgs e)
        {
            All.Clear();
        }

        private void Dump(object sender, RoutedEventArgs e)
        {
            var lines = new List<string>();
            foreach (KeyValuePair<ushort, OpcodeEnum> keyVal in Known)
            {
                var s = $"{keyVal.Value} = {keyVal.Key}";
                lines.Add(s);
            }
            File.WriteAllLines($"{Environment.CurrentDirectory}/opcodes {DateTime.Now.ToString().Replace('/', '-').Replace(':', '-')}.txt", lines);
        }

        private void LoadLog(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog { Filter = "Supported Formats (*.TeraLog)|*.TeraLog" };
            if (openFileDialog.ShowDialog() == false) return;
            NetworkController.Instance.LoadFileName = openFileDialog.FileName;
        }
        private void SaveLog(object sender, RoutedEventArgs e)
        {
            NetworkController.Instance.NeedToSave = true;
        }

        private void RemoveFilteredOpcode(object sender, RoutedEventArgs e)
        {
            var s = (System.Windows.Controls.Button)sender;
            FilteredOpcodes.Remove((FilteredOpcodeVm)s.DataContext);
        }
        private void AddBlackListedOpcode(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(OpcodeToFilter.Text)) return;
            if (!ushort.TryParse(OpcodeToFilter.Text, out ushort result)) return;
            if (FilteredOpcodes.FirstOrDefault(x => x.Opcode == result) != null) return;
            FilteredOpcodes.Add(new FilteredOpcodeVm(result, FilterMode.Exclude));
            OpcodeToFilter.Text = "";

        }
        private void AddWhiteListedOpcode(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(OpcodeToFilter.Text)) return;
            if (!ushort.TryParse(OpcodeToFilter.Text, out ushort result)) return;
            if (FilteredOpcodes.FirstOrDefault(x => x.Opcode == result) != null) return;
            FilteredOpcodes.Add(new FilteredOpcodeVm(result, FilterMode.ShowOnly));
            OpcodeToFilter.Text = "";
        }

        private void CopyPacketDetailsHex(object sender, RoutedEventArgs e)
        {
          
            StringBuilder bldr = new StringBuilder();
            foreach (var a in PacketDetails.Data)
            {
                bldr.Append(a.Hex);
            }
            System.Windows.Clipboard.Clear();
            System.Windows.Clipboard.SetDataObject(bldr.ToString(),false);
          
        }

        private void DataChunkMouseEnter(object sender, MouseEventArgs e)
        {
            var s = sender as FrameworkElement;
            var bvm = s.DataContext as ByteViewModel;
            bvm.IsHovered = true;
            int i = 0;
            i = HexIc.Items.IndexOf(bvm);
            if (i == -1) i = TextIc.Items.IndexOf(bvm);
            PacketDetails.RefreshData(i);
        }
        private void DataChunkMouseLeave(object sender, MouseEventArgs e)
        {
            var s = sender as FrameworkElement;
            var bvm = s.DataContext as ByteViewModel;
            bvm.IsHovered = false;

        }

        private void NewMessagesButtonClick(object sender, MouseButtonEventArgs e)
        {
            var b = VisualTreeHelper.GetChild(AllItemsControl, 0) as Border;
            var sw = VisualTreeHelper.GetChild(b, 0) as ScrollViewer;
            sw.ScrollToBottom();
        }

        private void LoadOpcode(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog { Filter = "Supported Formats, with / without '=' separator (*.txt)|*.txt" };
            if (openFileDialog.ShowDialog() == false) return;
            NetworkController.Instance.StrictCheck = false;
            NetworkController.Instance.LoadOpcodeCheck = openFileDialog.FileName;
        }


        private void LoadOpcodeForViewing(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog { Filter = "Supported Formats, with / without '=' separator (*.txt)|*.txt" };
            if (openFileDialog.ShowDialog() == false) return;
            NetworkController.Instance.LoadOpcodeForViewing = openFileDialog.FileName;
        }

        private void SizeFilterTextChanged(object sender, TextChangedEventArgs e)
        {
            var s = sender as System.Windows.Controls.TextBox;
            if (string.IsNullOrEmpty(s.Text))
            {
                _sizeFilter = -1;
                return;
            }
            try
            {
                _sizeFilter = Convert.ToInt32(s.Text);
            }
            catch
            {
                _sizeFilter = -1;
            }
        }

        private void SearchBoxChanged(object sender, TextChangedEventArgs e)
        {
            var s = sender as System.Windows.Controls.TextBox;
            UpdateSearch(s.Text, true);
        }
        private void UpdateSearch(string q, bool bringIntoView)
        {
            SearchList.Clear();
            OnPropertyChanged(nameof(SearchList));
            foreach (var packetViewModel in All)
            {
                //packetViewModel.IsSearched = true;
                packetViewModel.IsSearched = false;
            }
            if (string.IsNullOrEmpty(q))
            {
                foreach (var packetViewModel in All)
                {
                    //packetViewModel.IsSearched = true;
                    packetViewModel.IsSearched = false;
                }

                return;
            }
            try
            {
                var query = Convert.ToUInt16(q);
                //search by opcode
                foreach (var packetViewModel in All.Where(x => x.Message.OpCode == query))
                {
                    packetViewModel.IsSearched = true;
                    SearchList.Add(packetViewModel);
                }
                if (SearchList.Count != 0)
                {
                    var i = All.IndexOf(SearchList[0]);
                    if (bringIntoView)
                    {
                        //var container = AllItemsControl.ItemContainerGenerator.ContainerFromItem(All[i]) as FrameworkElement;
                        //container.BringIntoView();
                        AllItemsControl.VirtualizedScrollIntoView(All[i]);
                        PacketDetails = All[i];
                    }
                    foreach (var packetViewModel in All) { packetViewModel.IsSelected = packetViewModel == All[i]; }

                }
            }
            catch (Exception exception)
            {
                //search by opcodename

                OpcodeEnum opEnum;
                try
                {
                    opEnum = (OpcodeEnum)Enum.Parse(typeof(OpcodeEnum), q);
                }
                catch (Exception e1) { return; }


                foreach (var packetViewModel in All.Where(x => x.Message.OpCode == OpcodeFinder.Instance.GetOpcode(opEnum)))
                {
                    packetViewModel.IsSearched = true;
                    SearchList.Add(packetViewModel);
                }
                if (SearchList.Count != 0)
                {
                    var i = All.IndexOf(SearchList[0]);
                    if (bringIntoView)
                    {

                        //var container = AllItemsControl.ItemContainerGenerator.ContainerFromItem(All[i]) as FrameworkElement;
                        //container.BringIntoView();
                        AllItemsControl.VirtualizedScrollIntoView(All[i]);

                        PacketDetails = All[i];
                    }
                    foreach (var packetViewModel in All) { packetViewModel.IsSelected = packetViewModel == All[i]; }

                }
            }
            OnPropertyChanged(nameof(SearchList));

        }
        private void PreviousResult(object sender, RoutedEventArgs e)
        {
            if (SearchList.Count <2) return;
            if (_currentSelectedItemIndex == 0) _currentSelectedItemIndex = SearchList.Count - 1;
            else _currentSelectedItemIndex--;
            var i = All.IndexOf(SearchList[_currentSelectedItemIndex]);
            //var container = AllItemsControl.ItemContainerGenerator.ContainerFromItem(All[i]) as FrameworkElement;
            //container.BringIntoView();
            AllItemsControl.VirtualizedScrollIntoView(All[i]);

            PacketDetails = All[i];
            foreach (var packetViewModel in All) { packetViewModel.IsSelected = packetViewModel == All[i]; }

        }
        private void NextResult(object sender, RoutedEventArgs e)
        {
            if (SearchList.Count <2) return;
            if (_currentSelectedItemIndex == SearchList.Count - 1) _currentSelectedItemIndex = 0;
            else _currentSelectedItemIndex++;
            var i = All.IndexOf(SearchList[_currentSelectedItemIndex]);
            //var container = AllItemsControl.ItemContainerGenerator.ContainerFromItem(All[i]) as FrameworkElement;
            //container.BringIntoView();
            AllItemsControl.VirtualizedScrollIntoView(All[i]);

            PacketDetails = All[i];
            foreach (var packetViewModel in All) { packetViewModel.IsSelected = packetViewModel == All[i]; }

        }

        private void LoadOpcodeStrict(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog { Filter = "Supported Formats, with / without '=' separator (*.txt)|*.txt" };
            if (openFileDialog.ShowDialog() == false) return;
            NetworkController.Instance.StrictCheck = true;
            NetworkController.Instance.LoadOpcodeCheck = openFileDialog.FileName;
        }

        private void LoadList(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog { Filter = "Opcode names list (*.txt, *map)|*.txt;*.map" };
            if (openFileDialog.ShowDialog() == false) return;
            _currentFile = openFileDialog.FileName;
            var f = File.OpenText(_currentFile);
            OpcodesToFind.Clear();
            while (true)
            {
                uint opc = 0;
                var l = f.ReadLine();
                if (string.IsNullOrEmpty(l)) break;
                string opn = String.Empty;
                if (l.Contains("#"))
                {
                    var symbolIndex = l.IndexOf("#");
                    if (symbolIndex == 0) continue;
                    else
                    {
                        opn = l.Substring(0, symbolIndex - 1);
                    }
                }
                else
                {
                    opn = l;
                }

                var split = opn.Split(new string[] { " = " }, StringSplitOptions.RemoveEmptyEntries);
                if (split.Length > 1)
                {
                    opn = split[0];
                    opc = Convert.ToUInt32(split[1]);
                    OpcodesToFind.Add(new OpcodeToFindVm(opn, opc));
                    continue;
                }
                split = l.Split(' ');
                if (split.Length == 2)
                {
                    opn = split[0];
                    opc = Convert.ToUInt32(split[1]);
                    OpcodesToFind.Add(new OpcodeToFindVm(opn, opc));
                    continue;
                }
                OpcodesToFind.Add(new OpcodeToFindVm(opn, opc));

            }
            foreach (KeyValuePair<ushort, OpcodeEnum> o in Known)
            {
                CheckFindList(o.Key, o.Value);
            }

        }
        private void SaveList(object sender, RoutedEventArgs e)
        {
            var lines = new List<string>();
            foreach (var opcodeToFindVm in OpcodesToFind)
            {
                var line = $"{opcodeToFindVm.OpcodeName} = {opcodeToFindVm.Opcode}";
                lines.Add(line);
            }
            File.WriteAllLines(_currentFile, lines);
        }

        private void TabClicked(object sender, MouseButtonEventArgs e)
        {
            var s = sender as FrameworkElement;
            var w = s.ActualWidth;
            var tp = s.TemplatedParent as FrameworkElement;
            var p = tp.Parent as UIElement;
            var r = s.TranslatePoint(new Point(0, 0), p);
            var sizeAn = new DoubleAnimation(w, TimeSpan.FromMilliseconds(250)) { EasingFunction = new QuadraticEase() };
            var posAn = new DoubleAnimation(r.X, TimeSpan.FromMilliseconds(250)) { EasingFunction = new QuadraticEase() };

            TabSelectionRect.BeginAnimation(WidthProperty, sizeAn);
            TabSelectionRect.RenderTransform.BeginAnimation(TranslateTransform.XProperty, posAn);
        }

        private void ClearAllFilters(object sender, RoutedEventArgs e)
        {
            FilteredOpcodes.Clear();
        }

        private void HideLeftSlide(object sender, MouseButtonEventArgs e)
        {
            LeftSlide.RenderTransform.BeginAnimation(TranslateTransform.XProperty, new DoubleAnimation(-230,TimeSpan.FromMilliseconds(150)) {EasingFunction = new QuadraticEase()});
        }
        private void OpenLeftSlide(object sender, RoutedEventArgs e)
        {
            LeftSlide.RenderTransform.BeginAnimation(TranslateTransform.XProperty, new DoubleAnimation(0, TimeSpan.FromMilliseconds(150)) { EasingFunction = new QuadraticEase() });
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void ListEncounter_OnDropDownOpened(object sender, EventArgs e)
        {
            _topMost = false;
        }
        private void Close_MouseLeftButtonDown(object sender, RoutedEventArgs e)
        {
            VerifyClose();
        }

    }
}