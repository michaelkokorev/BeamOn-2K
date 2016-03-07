namespace BeamOn_U3
{
    partial class FormPowerCalibration
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
            this.buttonOk = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.panelStart = new System.Windows.Forms.Panel();
            this.progressBarCalibration = new System.Windows.Forms.ProgressBar();
            this.labelStart = new System.Windows.Forms.Label();
            this.panelPower = new System.Windows.Forms.Panel();
            this.labelUnits = new System.Windows.Forms.Label();
            this.textBoxPowerValue = new System.Windows.Forms.TextBox();
            this.labelPower = new System.Windows.Forms.Label();
            this.panelStart.SuspendLayout();
            this.panelPower.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonOk
            // 
            this.buttonOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOk.Enabled = false;
            this.buttonOk.Location = new System.Drawing.Point(142, 76);
            this.buttonOk.Name = "buttonOk";
            this.buttonOk.Size = new System.Drawing.Size(96, 26);
            this.buttonOk.TabIndex = 0;
            this.buttonOk.Text = "&Ok";
            this.buttonOk.UseVisualStyleBackColor = true;
            this.buttonOk.Click += new System.EventHandler(this.buttonOk_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(244, 76);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(96, 26);
            this.buttonCancel.TabIndex = 1;
            this.buttonCancel.Text = "&Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // panelStart
            // 
            this.panelStart.Controls.Add(this.progressBarCalibration);
            this.panelStart.Controls.Add(this.labelStart);
            this.panelStart.Location = new System.Drawing.Point(3, 3);
            this.panelStart.Name = "panelStart";
            this.panelStart.Size = new System.Drawing.Size(337, 64);
            this.panelStart.TabIndex = 2;
            // 
            // progressBarCalibration
            // 
            this.progressBarCalibration.Location = new System.Drawing.Point(18, 33);
            this.progressBarCalibration.Name = "progressBarCalibration";
            this.progressBarCalibration.Size = new System.Drawing.Size(301, 10);
            this.progressBarCalibration.Step = 1;
            this.progressBarCalibration.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.progressBarCalibration.TabIndex = 2;
            // 
            // labelStart
            // 
            this.labelStart.Location = new System.Drawing.Point(18, 6);
            this.labelStart.Name = "labelStart";
            this.labelStart.Size = new System.Drawing.Size(301, 18);
            this.labelStart.TabIndex = 1;
            this.labelStart.Text = "Please wait...";
            this.labelStart.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // panelPower
            // 
            this.panelPower.Controls.Add(this.labelUnits);
            this.panelPower.Controls.Add(this.textBoxPowerValue);
            this.panelPower.Controls.Add(this.labelPower);
            this.panelPower.Location = new System.Drawing.Point(-1000, 3);
            this.panelPower.Name = "panelPower";
            this.panelPower.Size = new System.Drawing.Size(337, 64);
            this.panelPower.TabIndex = 3;
            // 
            // labelUnits
            // 
            this.labelUnits.AutoSize = true;
            this.labelUnits.Location = new System.Drawing.Point(234, 30);
            this.labelUnits.Name = "labelUnits";
            this.labelUnits.Size = new System.Drawing.Size(32, 13);
            this.labelUnits.TabIndex = 3;
            this.labelUnits.Text = "(mW)";
            // 
            // textBoxPowerValue
            // 
            this.textBoxPowerValue.Location = new System.Drawing.Point(108, 27);
            this.textBoxPowerValue.Name = "textBoxPowerValue";
            this.textBoxPowerValue.Size = new System.Drawing.Size(120, 20);
            this.textBoxPowerValue.TabIndex = 2;
            this.textBoxPowerValue.TextChanged += new System.EventHandler(this.textBoxPowerValue_TextChanged);
            // 
            // labelPower
            // 
            this.labelPower.Location = new System.Drawing.Point(18, 6);
            this.labelPower.Name = "labelPower";
            this.labelPower.Size = new System.Drawing.Size(301, 18);
            this.labelPower.TabIndex = 1;
            this.labelPower.Text = "Please enter the value of the measured power";
            this.labelPower.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // FormPowerCalibration
            // 
            this.AcceptButton = this.buttonOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(346, 111);
            this.Controls.Add(this.panelPower);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOk);
            this.Controls.Add(this.panelStart);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.HelpButton = true;
            this.Name = "FormPowerCalibration";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Power Calibration";
            this.TopMost = true;
            this.Load += new System.EventHandler(this.FormPowerCalibration_Load);
            this.panelStart.ResumeLayout(false);
            this.panelPower.ResumeLayout(false);
            this.panelPower.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button buttonOk;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Panel panelStart;
        private System.Windows.Forms.Panel panelPower;
        private System.Windows.Forms.Label labelUnits;
        private System.Windows.Forms.TextBox textBoxPowerValue;
        private System.Windows.Forms.Label labelPower;
        private System.Windows.Forms.Label labelStart;
        public System.Windows.Forms.ProgressBar progressBarCalibration;
    }
}