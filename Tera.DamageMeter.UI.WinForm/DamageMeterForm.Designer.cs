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
            this.PasteButton = new System.Windows.Forms.Button();
            this.MenuButton = new System.Windows.Forms.Button();
            this.TotalDamageLabel = new System.Windows.Forms.Label();
            this.ResetButton = new System.Windows.Forms.Button();
            this.ListPanel = new System.Windows.Forms.Panel();
            this.MainMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.OpenPacketLogMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.CaptureMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.OpenFileMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.optionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.alwaysOnTopToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ResetMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ExitMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.OpenPacketLogFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.copy = new System.Windows.Forms.Button();
            this.HeaderPanel.SuspendLayout();
            this.MainMenu.SuspendLayout();
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
            this.HeaderPanel.Controls.Add(this.copy);
            this.HeaderPanel.Controls.Add(this.PasteButton);
            this.HeaderPanel.Controls.Add(this.MenuButton);
            this.HeaderPanel.Controls.Add(this.TotalDamageLabel);
            this.HeaderPanel.Controls.Add(this.ResetButton);
            this.HeaderPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.HeaderPanel.Location = new System.Drawing.Point(0, 0);
            this.HeaderPanel.Name = "HeaderPanel";
            this.HeaderPanel.Size = new System.Drawing.Size(284, 33);
            this.HeaderPanel.TabIndex = 1;
            // 
            // PasteButton
            // 
            this.PasteButton.Location = new System.Drawing.Point(118, 4);
            this.PasteButton.Name = "PasteButton";
            this.PasteButton.Size = new System.Drawing.Size(51, 23);
            this.PasteButton.TabIndex = 3;
            this.PasteButton.Text = "Paste";
            this.PasteButton.UseVisualStyleBackColor = true;
            this.PasteButton.Click += new System.EventHandler(this.PasteButton_Click);
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
            // TotalDamageLabel
            // 
            this.TotalDamageLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.TotalDamageLabel.AutoSize = true;
            this.TotalDamageLabel.Location = new System.Drawing.Point(210, 9);
            this.TotalDamageLabel.Name = "TotalDamageLabel";
            this.TotalDamageLabel.Size = new System.Drawing.Size(71, 13);
            this.TotalDamageLabel.TabIndex = 1;
            this.TotalDamageLabel.Text = "TotalDamage";
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
            this.optionsToolStripMenuItem,
            this.ResetMenuItem,
            this.ExitMenuItem});
            this.MainMenu.Name = "MainMenu";
            this.MainMenu.Size = new System.Drawing.Size(112, 92);
            // 
            // OpenPacketLogMenuItem
            // 
            this.OpenPacketLogMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.CaptureMenuItem,
            this.OpenFileMenuItem});
            this.OpenPacketLogMenuItem.Name = "OpenPacketLogMenuItem";
            this.OpenPacketLogMenuItem.Size = new System.Drawing.Size(111, 22);
            this.OpenPacketLogMenuItem.Text = "Open";
            // 
            // CaptureMenuItem
            // 
            this.CaptureMenuItem.Name = "CaptureMenuItem";
            this.CaptureMenuItem.Size = new System.Drawing.Size(113, 22);
            this.CaptureMenuItem.Text = "Capture";
            this.CaptureMenuItem.Click += new System.EventHandler(this.CaptureMenuItem_Click);
            // 
            // OpenFileMenuItem
            // 
            this.OpenFileMenuItem.Name = "OpenFileMenuItem";
            this.OpenFileMenuItem.Size = new System.Drawing.Size(113, 22);
            this.OpenFileMenuItem.Text = "File...";
            this.OpenFileMenuItem.Click += new System.EventHandler(this.OpenPacketLogMenuItem_Click);
            // 
            // optionsToolStripMenuItem
            // 
            this.optionsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.alwaysOnTopToolStripMenuItem});
            this.optionsToolStripMenuItem.Name = "optionsToolStripMenuItem";
            this.optionsToolStripMenuItem.Size = new System.Drawing.Size(111, 22);
            this.optionsToolStripMenuItem.Text = "Options";
            // 
            // alwaysOnTopToolStripMenuItem
            // 
            this.alwaysOnTopToolStripMenuItem.Name = "alwaysOnTopToolStripMenuItem";
            this.alwaysOnTopToolStripMenuItem.Size = new System.Drawing.Size(144, 22);
            this.alwaysOnTopToolStripMenuItem.Text = "Always on Top";
            this.alwaysOnTopToolStripMenuItem.Click += new System.EventHandler(this.alwaysOnTopToolStripMenuItem_Click);
            // 
            // ResetMenuItem
            // 
            this.ResetMenuItem.Name = "ResetMenuItem";
            this.ResetMenuItem.Size = new System.Drawing.Size(111, 22);
            this.ResetMenuItem.Text = "Reset";
            this.ResetMenuItem.Click += new System.EventHandler(this.ResetButton_Click);
            // 
            // ExitMenuItem
            // 
            this.ExitMenuItem.Name = "ExitMenuItem";
            this.ExitMenuItem.Size = new System.Drawing.Size(111, 22);
            this.ExitMenuItem.Text = "E&xit";
            this.ExitMenuItem.Click += new System.EventHandler(this.ExitMenuItem_Click);
            // 
            // OpenPacketLogFileDialog
            // 
            this.OpenPacketLogFileDialog.Filter = "Tera Packet Logs|*.TeraLog|All files|*.*";
            this.OpenPacketLogFileDialog.Title = "Open Tera Packet Log";
            // 
            // copy
            // 
            this.copy.Location = new System.Drawing.Point(175, 3);
            this.copy.Name = "copy";
            this.copy.Size = new System.Drawing.Size(43, 23);
            this.copy.TabIndex = 4;
            this.copy.Text = "copy";
            this.copy.UseVisualStyleBackColor = true;
            this.copy.Click += new System.EventHandler(this.button1_Click);
            // 
            // DamageMeterForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 589);
            this.Controls.Add(this.ListPanel);
            this.Controls.Add(this.HeaderPanel);
            this.Name = "DamageMeterForm";
            this.Text = "Damage Meter";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.DamageMeterForm_FormClosed);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.HeaderPanel.ResumeLayout(false);
            this.HeaderPanel.PerformLayout();
            this.MainMenu.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Timer RefershTimer;
        private System.Windows.Forms.Panel HeaderPanel;
        private System.Windows.Forms.Button ResetButton;
        private System.Windows.Forms.Label TotalDamageLabel;
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
        private System.Windows.Forms.Button PasteButton;
        private System.Windows.Forms.Button copy;
    }
}
