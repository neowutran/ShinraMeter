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
            this.MainMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.OpenPacketLogMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.CaptureMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.OpenFileMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.optionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.alwaysOnTopToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ResetMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ExitMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.OpenPacketLogFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.overlay_timer = new System.Windows.Forms.Timer(this.components);
            this.ListPanel = new System.Windows.Forms.Panel();
            this.MainMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // RefershTimer
            // 
            this.RefershTimer.Enabled = true;
            this.RefershTimer.Interval = 250;
            this.RefershTimer.Tick += new System.EventHandler(this.RefershTimer_Tick);
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
            // overlay_timer
            // 
            this.overlay_timer.Enabled = true;
            this.overlay_timer.Interval = 250;
            // 
            // ListPanel
            // 
            this.ListPanel.BackColor = System.Drawing.Color.Transparent;
            this.ListPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ListPanel.Location = new System.Drawing.Point(0, 0);
            this.ListPanel.Name = "ListPanel";
            this.ListPanel.Size = new System.Drawing.Size(284, 589);
            this.ListPanel.TabIndex = 2;
            this.ListPanel.Paint += new System.Windows.Forms.PaintEventHandler(this.ListPanel_Paint);
            // 
            // DamageMeterForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(284, 589);
            this.Controls.Add(this.ListPanel);
            this.Name = "DamageMeterForm";
            this.Text = "Damage Meter";
            this.TopMost = true;
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.DamageMeterForm_FormClosed);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.MainMenu.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Timer RefershTimer;
        private System.Windows.Forms.ContextMenuStrip MainMenu;
        private System.Windows.Forms.ToolStripMenuItem ExitMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ResetMenuItem;
        private System.Windows.Forms.ToolStripMenuItem OpenPacketLogMenuItem;
        private System.Windows.Forms.OpenFileDialog OpenPacketLogFileDialog;
        private System.Windows.Forms.ToolStripMenuItem optionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem alwaysOnTopToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem OpenFileMenuItem;
        private System.Windows.Forms.ToolStripMenuItem CaptureMenuItem;
        private System.Windows.Forms.Timer overlay_timer;
        private System.Windows.Forms.Panel ListPanel;
    }
}
