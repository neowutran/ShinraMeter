// Copyright (c) Gothos
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Tera.DamageMeter
{
    public partial class SettingsForm : Form
    {
        public Settings Settings { get; set; }

        public SettingsForm()
        {
            InitializeComponent();
        }

        private void OkButton_Click(object sender, EventArgs e)
        {
            ApplyButton_Click(sender, e);
            CancelButton_Click(sender, e);
        }

        private void ApplyButton_Click(object sender, EventArgs e)
        {
            Settings.AlwaysOnTop = AlwaysOnTopCheckbox.Checked;
            Settings.Opacity = (double)OpacityTrackBar.Value / OpacityTrackBar.Maximum;
            Settings.HotKeys.PasteStats = HotKeyHelpers.ToString(PasteStatsHotKeyBox.Key);
            Settings.HotKeys.Reset = HotKeyHelpers.ToString(ResetHotKeyBox.Key);
            Settings.BufferSize = ((KeyValuePair<int?, string>)BufferSizeComboBox.SelectedItem).Key;
            Settings.OnSettingsChanged();
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void SettingsForm_Load(object sender, EventArgs e)
        {
            var bufferSizes = new[] { null, Settings.BufferSize }.Concat(new int?[] { 1, 2, 5, 10, 20, 50 }.Select(x => x * 1000000)).Distinct();
            var items = bufferSizes.Select(bufferSize => new KeyValuePair<int?, string>(bufferSize, bufferSize != null ? FormatHelpers.Pretty.FormatValue(bufferSize) + "B" : "Default")).ToList();
            BufferSizeComboBox.Items.AddRange(items.Cast<object>().ToArray());

            AlwaysOnTopCheckbox.Checked = Settings.AlwaysOnTop;
            OpacityTrackBar.Value = (int)Math.Round(Settings.Opacity * OpacityTrackBar.Maximum);
            PasteStatsHotKeyBox.Key = HotKeyHelpers.FromString(Settings.HotKeys.PasteStats);
            ResetHotKeyBox.Key = HotKeyHelpers.FromString(Settings.HotKeys.Reset);
            BufferSizeComboBox.SelectedItem = items.Single(x => x.Key == Settings.BufferSize);
        }
    }
}
