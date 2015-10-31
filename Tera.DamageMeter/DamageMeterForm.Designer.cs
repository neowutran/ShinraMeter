// Copyright (c) Gothos
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Tera.DamageMeter
{
    partial class DamageMeterForm
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
            this.components = new System.ComponentModel.Container();
            this.RefershTimer = new System.Windows.Forms.Timer(this.components);
            this.HeaderPanel = new System.Windows.Forms.Panel();
            this.MenuButton = new System.Windows.Forms.Button();
            this.ResetButton = new System.Windows.Forms.Button();
            this.ListPanel = new System.Windows.Forms.Panel();
            this.MainMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.OpenPacketLogMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.CaptureMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.OpenFileMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.PasteStatsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ResetMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.optionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.alwaysOnTopToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.SettingsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ExitMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.OpenPacketLogFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.FooterPanel = new System.Windows.Forms.Panel();
            this.TotalTimeLabel = new System.Windows.Forms.Label();
            this.TotalDpsLabel = new System.Windows.Forms.Label();
            this.TotalDamageLabel = new System.Windows.Forms.Label();
            this.HeaderPanel.SuspendLayout();
            this.MainMenu.SuspendLayout();
            this.FooterPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // RefershTimer
            // 
            this.RefershTimer.Enabled = true;
            this.RefershTimer.Interval = 500;
            this.RefershTimer.Tick += new System.EventHandler(this.RefershTimer_Tick);
            // 
            // HeaderPanel
            // 
            this.HeaderPanel.Controls.Add(this.MenuButton);
            this.HeaderPanel.Controls.Add(this.ResetButton);
            this.HeaderPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.HeaderPanel.Location = new System.Drawing.Point(0, 0);
            this.HeaderPanel.Name = "HeaderPanel";
            this.HeaderPanel.Size = new System.Drawing.Size(284, 33);
            this.HeaderPanel.TabIndex = 1;
            // 
            // MenuButton
            // 
            this.MenuButton.Location = new System.Drawing.Point(4, 4);
            this.MenuButton.Name = "MenuButton";
            this.MenuButton.Size = new System.Drawing.Size(51, 23);
            this.MenuButton.TabIndex = 2;
            this.MenuButton.Text = "Menu";
            this.MenuButton.UseVisualStyleBackColor = true;
            this.MenuButton.Click += new System.EventHandler(this.MenuButton_Click);
            // 
            // ResetButton
            // 
            this.ResetButton.Location = new System.Drawing.Point(61, 4);
            this.ResetButton.Name = "ResetButton";
            this.ResetButton.Size = new System.Drawing.Size(51, 23);
            this.ResetButton.TabIndex = 0;
            this.ResetButton.Text = "Reset";
            this.ResetButton.UseVisualStyleBackColor = true;
            this.ResetButton.Click += new System.EventHandler(this.ResetButton_Click);
            // 
            // ListPanel
            // 
            this.ListPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ListPanel.Location = new System.Drawing.Point(0, 33);
            this.ListPanel.Name = "ListPanel";
            this.ListPanel.Size = new System.Drawing.Size(284, 556);
            this.ListPanel.TabIndex = 2;
            // 
            // MainMenu
            // 
            this.MainMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.OpenPacketLogMenuItem,
            this.PasteStatsMenuItem,
            this.ResetMenuItem,
            this.optionsToolStripMenuItem,
            this.SettingsMenuItem,
            this.ExitMenuItem});
            this.MainMenu.Name = "MainMenu";
            this.MainMenu.Size = new System.Drawing.Size(149, 136);
            // 
            // OpenPacketLogMenuItem
            // 
            this.OpenPacketLogMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.CaptureMenuItem,
            this.OpenFileMenuItem});
            this.OpenPacketLogMenuItem.Name = "OpenPacketLogMenuItem";
            this.OpenPacketLogMenuItem.Size = new System.Drawing.Size(148, 22);
            this.OpenPacketLogMenuItem.Text = "Open";
            // 
            // CaptureMenuItem
            // 
            this.CaptureMenuItem.Name = "CaptureMenuItem";
            this.CaptureMenuItem.Size = new System.Drawing.Size(116, 22);
            this.CaptureMenuItem.Text = "Capture";
            this.CaptureMenuItem.Click += new System.EventHandler(this.CaptureMenuItem_Click);
            // 
            // OpenFileMenuItem
            // 
            this.OpenFileMenuItem.Name = "OpenFileMenuItem";
            this.OpenFileMenuItem.Size = new System.Drawing.Size(116, 22);
            this.OpenFileMenuItem.Text = "File...";
            this.OpenFileMenuItem.Click += new System.EventHandler(this.OpenPacketLogMenuItem_Click);
            // 
            // PasteStatsMenuItem
            // 
            this.PasteStatsMenuItem.Name = "PasteStatsMenuItem";
            this.PasteStatsMenuItem.Size = new System.Drawing.Size(148, 22);
            this.PasteStatsMenuItem.Text = "Paste damage";
            this.PasteStatsMenuItem.Click += new System.EventHandler(this.PasteStatsMenuItem_Click);
            // 
            // ResetMenuItem
            // 
            this.ResetMenuItem.Name = "ResetMenuItem";
            this.ResetMenuItem.Size = new System.Drawing.Size(148, 22);
            this.ResetMenuItem.Text = "Reset";
            this.ResetMenuItem.Click += new System.EventHandler(this.ResetButton_Click);
            // 
            // optionsToolStripMenuItem
            // 
            this.optionsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.alwaysOnTopToolStripMenuItem});
            this.optionsToolStripMenuItem.Name = "optionsToolStripMenuItem";
            this.optionsToolStripMenuItem.Size = new System.Drawing.Size(148, 22);
            this.optionsToolStripMenuItem.Text = "Options";
            // 
            // alwaysOnTopToolStripMenuItem
            // 
            this.alwaysOnTopToolStripMenuItem.Name = "alwaysOnTopToolStripMenuItem";
            this.alwaysOnTopToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.alwaysOnTopToolStripMenuItem.Text = "Always on Top";
            this.alwaysOnTopToolStripMenuItem.Click += new System.EventHandler(this.alwaysOnTopToolStripMenuItem_Click);
            // 
            // SettingsMenuItem
            // 
            this.SettingsMenuItem.Name = "SettingsMenuItem";
            this.SettingsMenuItem.Size = new System.Drawing.Size(148, 22);
            this.SettingsMenuItem.Text = "Settings...";
            this.SettingsMenuItem.Click += new System.EventHandler(this.SettingsMenuItem_Click);
            // 
            // ExitMenuItem
            // 
            this.ExitMenuItem.Name = "ExitMenuItem";
            this.ExitMenuItem.Size = new System.Drawing.Size(148, 22);
            this.ExitMenuItem.Text = "E&xit";
            this.ExitMenuItem.Click += new System.EventHandler(this.ExitMenuItem_Click);
            // 
            // OpenPacketLogFileDialog
            // 
            this.OpenPacketLogFileDialog.Filter = "Tera Packet Logs|*.TeraLog|All files|*.*";
            this.OpenPacketLogFileDialog.Title = "Open Tera Packet Log";
            // 
            // FooterPanel
            // 
            this.FooterPanel.Controls.Add(this.TotalTimeLabel);
            this.FooterPanel.Controls.Add(this.TotalDpsLabel);
            this.FooterPanel.Controls.Add(this.TotalDamageLabel);
            this.FooterPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.FooterPanel.Location = new System.Drawing.Point(0, 551);
            this.FooterPanel.Name = "FooterPanel";
            this.FooterPanel.Size = new System.Drawing.Size(284, 38);
            this.FooterPanel.TabIndex = 3;
            // 
            // TotalTimeLabel
            // 
            this.TotalTimeLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.TotalTimeLabel.AutoSize = true;
            this.TotalTimeLabel.Location = new System.Drawing.Point(224, 22);
            this.TotalTimeLabel.Name = "TotalTimeLabel";
            this.TotalTimeLabel.Size = new System.Drawing.Size(57, 13);
            this.TotalTimeLabel.TabIndex = 4;
            this.TotalTimeLabel.Text = "Total Time";
            // 
            // TotalDpsLabel
            // 
            this.TotalDpsLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.TotalDpsLabel.AutoSize = true;
            this.TotalDpsLabel.Location = new System.Drawing.Point(225, 4);
            this.TotalDpsLabel.Name = "TotalDpsLabel";
            this.TotalDpsLabel.Size = new System.Drawing.Size(56, 13);
            this.TotalDpsLabel.TabIndex = 3;
            this.TotalDpsLabel.Text = "Total DPS";
            // 
            // TotalDamageLabel
            // 
            this.TotalDamageLabel.AutoSize = true;
            this.TotalDamageLabel.Location = new System.Drawing.Point(4, 4);
            this.TotalDamageLabel.Name = "TotalDamageLabel";
            this.TotalDamageLabel.Size = new System.Drawing.Size(71, 13);
            this.TotalDamageLabel.TabIndex = 2;
            this.TotalDamageLabel.Text = "TotalDamage";
            // 
            // DamageMeterForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 589);
            this.Controls.Add(this.FooterPanel);
            this.Controls.Add(this.ListPanel);
            this.Controls.Add(this.HeaderPanel);
            this.Name = "DamageMeterForm";
            this.Text = "Damage Meter";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.DamageMeterForm_FormClosed);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.HeaderPanel.ResumeLayout(false);
            this.MainMenu.ResumeLayout(false);
            this.FooterPanel.ResumeLayout(false);
            this.FooterPanel.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Timer RefershTimer;
        private System.Windows.Forms.Panel HeaderPanel;
        private System.Windows.Forms.Button ResetButton;
        private System.Windows.Forms.Panel ListPanel;
        private System.Windows.Forms.Button MenuButton;
        private System.Windows.Forms.ContextMenuStrip MainMenu;
        private System.Windows.Forms.ToolStripMenuItem ExitMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ResetMenuItem;
        private System.Windows.Forms.ToolStripMenuItem OpenPacketLogMenuItem;
        private System.Windows.Forms.OpenFileDialog OpenPacketLogFileDialog;
        private System.Windows.Forms.ToolStripMenuItem optionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem alwaysOnTopToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem OpenFileMenuItem;
        private System.Windows.Forms.ToolStripMenuItem CaptureMenuItem;
        private System.Windows.Forms.ToolStripMenuItem SettingsMenuItem;
        private System.Windows.Forms.ToolStripMenuItem PasteStatsMenuItem;
        private System.Windows.Forms.Panel FooterPanel;
        private System.Windows.Forms.Label TotalTimeLabel;
        private System.Windows.Forms.Label TotalDpsLabel;
        private System.Windows.Forms.Label TotalDamageLabel;
    }
}

