namespace Tera.DamageMeter
{
    partial class PlayerStatsControl
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.PlayerNameLabel = new System.Windows.Forms.Label();
            this.PlayerClassLabel = new System.Windows.Forms.Label();
            this.DamageLabel = new System.Windows.Forms.Label();
            this.HealLabel = new System.Windows.Forms.Label();
            this.DamageHealSeparator = new System.Windows.Forms.Label();
            this.InfoLabel = new System.Windows.Forms.Label();
            this.DamagePercentLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // PlayerNameLabel
            // 
            this.PlayerNameLabel.AutoSize = true;
            this.PlayerNameLabel.Location = new System.Drawing.Point(70, 4);
            this.PlayerNameLabel.Name = "PlayerNameLabel";
            this.PlayerNameLabel.Size = new System.Drawing.Size(64, 13);
            this.PlayerNameLabel.TabIndex = 0;
            this.PlayerNameLabel.Text = "PlayerName";
            // 
            // PlayerClassLabel
            // 
            this.PlayerClassLabel.AutoSize = true;
            this.PlayerClassLabel.Location = new System.Drawing.Point(3, 4);
            this.PlayerClassLabel.Name = "PlayerClassLabel";
            this.PlayerClassLabel.Size = new System.Drawing.Size(61, 13);
            this.PlayerClassLabel.TabIndex = 1;
            this.PlayerClassLabel.Text = "PlayerClass";
            // 
            // DamageLabel
            // 
            this.DamageLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.DamageLabel.AutoSize = true;
            this.DamageLabel.ForeColor = System.Drawing.Color.Red;
            this.DamageLabel.Location = new System.Drawing.Point(206, 4);
            this.DamageLabel.Name = "DamageLabel";
            this.DamageLabel.Size = new System.Drawing.Size(47, 13);
            this.DamageLabel.TabIndex = 2;
            this.DamageLabel.Text = "Damage";
            // 
            // HealLabel
            // 
            this.HealLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.HealLabel.AutoSize = true;
            this.HealLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(192)))), ((int)(((byte)(0)))));
            this.HealLabel.Location = new System.Drawing.Point(162, 4);
            this.HealLabel.Name = "HealLabel";
            this.HealLabel.Size = new System.Drawing.Size(29, 13);
            this.HealLabel.TabIndex = 3;
            this.HealLabel.Text = "Heal";
            // 
            // DamageHealSeparator
            // 
            this.DamageHealSeparator.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.DamageHealSeparator.AutoSize = true;
            this.DamageHealSeparator.Location = new System.Drawing.Point(197, 4);
            this.DamageHealSeparator.Name = "DamageHealSeparator";
            this.DamageHealSeparator.Size = new System.Drawing.Size(13, 13);
            this.DamageHealSeparator.TabIndex = 4;
            this.DamageHealSeparator.Text = "+";
            // 
            // InfoLabel
            // 
            this.InfoLabel.AutoSize = true;
            this.InfoLabel.Location = new System.Drawing.Point(3, 24);
            this.InfoLabel.Name = "InfoLabel";
            this.InfoLabel.Size = new System.Drawing.Size(90, 13);
            this.InfoLabel.TabIndex = 5;
            this.InfoLabel.Text = "Critrate xx% Hits x";
            // 
            // DamagePercentLabel
            // 
            this.DamagePercentLabel.AutoSize = true;
            this.DamagePercentLabel.Location = new System.Drawing.Point(225, 24);
            this.DamagePercentLabel.Name = "DamagePercentLabel";
            this.DamagePercentLabel.Size = new System.Drawing.Size(25, 13);
            this.DamagePercentLabel.TabIndex = 6;
            this.DamagePercentLabel.Text = "xx%";
            // 
            // PlayerStatsControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.DamagePercentLabel);
            this.Controls.Add(this.InfoLabel);
            this.Controls.Add(this.DamageHealSeparator);
            this.Controls.Add(this.HealLabel);
            this.Controls.Add(this.DamageLabel);
            this.Controls.Add(this.PlayerClassLabel);
            this.Controls.Add(this.PlayerNameLabel);
            this.Name = "PlayerStatsControl";
            this.Size = new System.Drawing.Size(253, 40);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label PlayerNameLabel;
        private System.Windows.Forms.Label PlayerClassLabel;
        private System.Windows.Forms.Label DamageLabel;
        private System.Windows.Forms.Label HealLabel;
        private System.Windows.Forms.Label DamageHealSeparator;
        private System.Windows.Forms.Label InfoLabel;
        private System.Windows.Forms.Label DamagePercentLabel;
    }
}
