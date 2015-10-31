// Copyright (c) Gothos
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Tera.Sniffer
{
    partial class SnifferForm
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
            this.MessageCount = new System.Windows.Forms.Label();
            this.ConnectionList = new System.Windows.Forms.ListBox();
            this.SuspendLayout();
            // 
            // MessageCount
            // 
            this.MessageCount.AutoSize = true;
            this.MessageCount.Location = new System.Drawing.Point(12, 9);
            this.MessageCount.Name = "MessageCount";
            this.MessageCount.Size = new System.Drawing.Size(78, 13);
            this.MessageCount.TabIndex = 3;
            this.MessageCount.Text = "MessageCount";
            // 
            // ConnectionList
            // 
            this.ConnectionList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ConnectionList.FormattingEnabled = true;
            this.ConnectionList.Location = new System.Drawing.Point(8, 25);
            this.ConnectionList.Name = "ConnectionList";
            this.ConnectionList.Size = new System.Drawing.Size(264, 225);
            this.ConnectionList.TabIndex = 4;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 262);
            this.Controls.Add(this.ConnectionList);
            this.Controls.Add(this.MessageCount);
            this.Name = "Form1";
            this.Text = "Form1";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form1_FormClosed);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label MessageCount;
        private System.Windows.Forms.ListBox ConnectionList;
    }
}

