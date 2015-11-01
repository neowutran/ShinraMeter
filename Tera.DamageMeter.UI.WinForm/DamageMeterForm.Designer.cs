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
            this.ListPanel = new System.Windows.Forms.Panel();
            this.SuspendLayout();
            // 
            // RefershTimer
            // 
            this.RefershTimer.Enabled = true;
            this.RefershTimer.Interval = 250;
            this.RefershTimer.Tick += new System.EventHandler(this.RefershTimer_Tick);

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
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Timer RefershTimer;
        private System.Windows.Forms.Panel ListPanel;
    }
}
