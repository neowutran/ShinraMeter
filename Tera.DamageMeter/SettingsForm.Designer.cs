// Copyright (c) Gothos
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Tera.DamageMeter
{
    partial class SettingsForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.Label label1;
            System.Windows.Forms.Label label3;
            this.panel1 = new System.Windows.Forms.Panel();
            this.ApplyButton = new System.Windows.Forms.Button();
            this.CancelButton = new System.Windows.Forms.Button();
            this.OkButton = new System.Windows.Forms.Button();
            this.SettingsTabs = new System.Windows.Forms.TabControl();
            this.GeneralTab = new System.Windows.Forms.TabPage();
            this.label4 = new System.Windows.Forms.Label();
            this.BufferSizeComboBox = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.OpacityTrackBar = new System.Windows.Forms.TrackBar();
            this.AlwaysOnTopCheckbox = new System.Windows.Forms.CheckBox();
            this.HotkeysTab = new System.Windows.Forms.TabPage();
            this.ResetHotKeyBox = new Tera.DamageMeter.HotKeyControl();
            this.PasteStatsHotKeyBox = new Tera.DamageMeter.HotKeyControl();
            label1 = new System.Windows.Forms.Label();
            label3 = new System.Windows.Forms.Label();
            this.panel1.SuspendLayout();
            this.SettingsTabs.SuspendLayout();
            this.GeneralTab.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.OpacityTrackBar)).BeginInit();
            this.HotkeysTab.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(8, 21);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(61, 13);
            label1.TabIndex = 1;
            label1.Text = "Paste Stats";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(8, 55);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(35, 13);
            label3.TabIndex = 3;
            label3.Text = "Reset";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.ApplyButton);
            this.panel1.Controls.Add(this.CancelButton);
            this.panel1.Controls.Add(this.OkButton);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 369);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(470, 48);
            this.panel1.TabIndex = 0;
            // 
            // ApplyButton
            // 
            this.ApplyButton.Location = new System.Drawing.Point(177, 13);
            this.ApplyButton.Name = "ApplyButton";
            this.ApplyButton.Size = new System.Drawing.Size(75, 23);
            this.ApplyButton.TabIndex = 2;
            this.ApplyButton.Text = "Apply";
            this.ApplyButton.UseVisualStyleBackColor = true;
            this.ApplyButton.Click += new System.EventHandler(this.ApplyButton_Click);
            // 
            // CancelButton
            // 
            this.CancelButton.Location = new System.Drawing.Point(96, 13);
            this.CancelButton.Name = "CancelButton";
            this.CancelButton.Size = new System.Drawing.Size(75, 23);
            this.CancelButton.TabIndex = 1;
            this.CancelButton.Text = "Cancel";
            this.CancelButton.UseVisualStyleBackColor = true;
            this.CancelButton.Click += new System.EventHandler(this.CancelButton_Click);
            // 
            // OkButton
            // 
            this.OkButton.Location = new System.Drawing.Point(12, 13);
            this.OkButton.Name = "OkButton";
            this.OkButton.Size = new System.Drawing.Size(75, 23);
            this.OkButton.TabIndex = 0;
            this.OkButton.Text = "OK";
            this.OkButton.UseVisualStyleBackColor = true;
            this.OkButton.Click += new System.EventHandler(this.OkButton_Click);
            // 
            // SettingsTabs
            // 
            this.SettingsTabs.Controls.Add(this.GeneralTab);
            this.SettingsTabs.Controls.Add(this.HotkeysTab);
            this.SettingsTabs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.SettingsTabs.Location = new System.Drawing.Point(0, 0);
            this.SettingsTabs.Name = "SettingsTabs";
            this.SettingsTabs.SelectedIndex = 0;
            this.SettingsTabs.Size = new System.Drawing.Size(470, 369);
            this.SettingsTabs.TabIndex = 0;
            // 
            // GeneralTab
            // 
            this.GeneralTab.Controls.Add(this.label4);
            this.GeneralTab.Controls.Add(this.BufferSizeComboBox);
            this.GeneralTab.Controls.Add(this.label2);
            this.GeneralTab.Controls.Add(this.OpacityTrackBar);
            this.GeneralTab.Controls.Add(this.AlwaysOnTopCheckbox);
            this.GeneralTab.Location = new System.Drawing.Point(4, 22);
            this.GeneralTab.Name = "GeneralTab";
            this.GeneralTab.Padding = new System.Windows.Forms.Padding(3);
            this.GeneralTab.Size = new System.Drawing.Size(462, 343);
            this.GeneralTab.TabIndex = 1;
            this.GeneralTab.Text = "General";
            this.GeneralTab.UseVisualStyleBackColor = true;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(24, 127);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(59, 13);
            this.label4.TabIndex = 4;
            this.label4.Text = "Buffer size:";
            // 
            // BufferSizeComboBox
            // 
            this.BufferSizeComboBox.DisplayMember = "Value";
            this.BufferSizeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.BufferSizeComboBox.FormattingEnabled = true;
            this.BufferSizeComboBox.Location = new System.Drawing.Point(92, 124);
            this.BufferSizeComboBox.Name = "BufferSizeComboBox";
            this.BufferSizeComboBox.Size = new System.Drawing.Size(177, 21);
            this.BufferSizeComboBox.TabIndex = 3;
            this.BufferSizeComboBox.ValueMember = "Text";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(16, 47);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(43, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Opacity";
            // 
            // OpacityTrackBar
            // 
            this.OpacityTrackBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.OpacityTrackBar.Location = new System.Drawing.Point(19, 73);
            this.OpacityTrackBar.Maximum = 100;
            this.OpacityTrackBar.Minimum = 20;
            this.OpacityTrackBar.Name = "OpacityTrackBar";
            this.OpacityTrackBar.Size = new System.Drawing.Size(424, 45);
            this.OpacityTrackBar.SmallChange = 10;
            this.OpacityTrackBar.TabIndex = 1;
            this.OpacityTrackBar.Value = 20;
            // 
            // AlwaysOnTopCheckbox
            // 
            this.AlwaysOnTopCheckbox.AutoSize = true;
            this.AlwaysOnTopCheckbox.Location = new System.Drawing.Point(19, 16);
            this.AlwaysOnTopCheckbox.Name = "AlwaysOnTopCheckbox";
            this.AlwaysOnTopCheckbox.Size = new System.Drawing.Size(92, 17);
            this.AlwaysOnTopCheckbox.TabIndex = 0;
            this.AlwaysOnTopCheckbox.Text = "Always on top";
            this.AlwaysOnTopCheckbox.UseVisualStyleBackColor = true;
            // 
            // HotkeysTab
            // 
            this.HotkeysTab.Controls.Add(label3);
            this.HotkeysTab.Controls.Add(label1);
            this.HotkeysTab.Controls.Add(this.ResetHotKeyBox);
            this.HotkeysTab.Controls.Add(this.PasteStatsHotKeyBox);
            this.HotkeysTab.Location = new System.Drawing.Point(4, 22);
            this.HotkeysTab.Name = "HotkeysTab";
            this.HotkeysTab.Padding = new System.Windows.Forms.Padding(3);
            this.HotkeysTab.Size = new System.Drawing.Size(462, 343);
            this.HotkeysTab.TabIndex = 0;
            this.HotkeysTab.Text = "Hotkeys";
            this.HotkeysTab.UseVisualStyleBackColor = true;
            // 
            // ResetHotKeyBox
            // 
            this.ResetHotKeyBox.CancelKey = System.Windows.Forms.Keys.Escape;
            this.ResetHotKeyBox.Key = System.Windows.Forms.Keys.None;
            this.ResetHotKeyBox.Location = new System.Drawing.Point(92, 52);
            this.ResetHotKeyBox.Name = "ResetHotKeyBox";
            this.ResetHotKeyBox.ShortcutsEnabled = false;
            this.ResetHotKeyBox.Size = new System.Drawing.Size(100, 20);
            this.ResetHotKeyBox.TabIndex = 2;
            this.ResetHotKeyBox.Text = "None";
            // 
            // PasteStatsHotKeyBox
            // 
            this.PasteStatsHotKeyBox.CancelKey = System.Windows.Forms.Keys.Escape;
            this.PasteStatsHotKeyBox.Key = System.Windows.Forms.Keys.None;
            this.PasteStatsHotKeyBox.Location = new System.Drawing.Point(92, 18);
            this.PasteStatsHotKeyBox.Name = "PasteStatsHotKeyBox";
            this.PasteStatsHotKeyBox.ShortcutsEnabled = false;
            this.PasteStatsHotKeyBox.Size = new System.Drawing.Size(100, 20);
            this.PasteStatsHotKeyBox.TabIndex = 0;
            this.PasteStatsHotKeyBox.Text = "None";
            // 
            // SettingsForm
            // 
            this.AcceptButton = this.OkButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(470, 417);
            this.Controls.Add(this.SettingsTabs);
            this.Controls.Add(this.panel1);
            this.Name = "SettingsForm";
            this.Text = "SettingsForm";
            this.Load += new System.EventHandler(this.SettingsForm_Load);
            this.panel1.ResumeLayout(false);
            this.SettingsTabs.ResumeLayout(false);
            this.GeneralTab.ResumeLayout(false);
            this.GeneralTab.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.OpacityTrackBar)).EndInit();
            this.HotkeysTab.ResumeLayout(false);
            this.HotkeysTab.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.TabControl SettingsTabs;
        private System.Windows.Forms.TabPage HotkeysTab;
        private HotKeyControl PasteStatsHotKeyBox;
        private System.Windows.Forms.Button ApplyButton;
        private System.Windows.Forms.Button CancelButton;
        private System.Windows.Forms.Button OkButton;
        private System.Windows.Forms.TabPage GeneralTab;
        private System.Windows.Forms.CheckBox AlwaysOnTopCheckbox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TrackBar OpacityTrackBar;
        private HotKeyControl ResetHotKeyBox;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox BufferSizeComboBox;
    }
}