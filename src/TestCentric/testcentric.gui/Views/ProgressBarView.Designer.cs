﻿namespace TestCentric.Gui.Views
{
    partial class ProgressBarView
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
            this.testProgressBar = new TestCentric.Gui.Controls.TestCentricProgressBar();
            this.SuspendLayout();
            // 
            // testProgressBar
            // 
            this.testProgressBar.Display = TestCentric.Gui.Controls.ProgressBarDisplay.Success;
            this.testProgressBar.Dock = System.Windows.Forms.DockStyle.Fill;
            this.testProgressBar.Location = new System.Drawing.Point(0, 0);
            this.testProgressBar.Name = "testProgressBar";
            this.testProgressBar.Size = new System.Drawing.Size(239, 19);
            this.testProgressBar.TabIndex = 0;
            // 
            // ProgressBarView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.testProgressBar);
            this.Name = "ProgressBarView";
            this.Size = new System.Drawing.Size(239, 19);
            this.ResumeLayout(false);

        }

        #endregion

        private TestCentric.Gui.Controls.TestCentricProgressBar testProgressBar;
    }
}
