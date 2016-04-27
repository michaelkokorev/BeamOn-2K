namespace BeamOn_U3
{
    partial class FormSetupFastMode
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
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.gbDuration = new System.Windows.Forms.GroupBox();
            this.lblDuration = new System.Windows.Forms.Label();
            this.DurationSecondsUpDown = new System.Windows.Forms.NumericUpDown();
            this.DurationHoursUpDown = new System.Windows.Forms.NumericUpDown();
            this.DurationMinutesUpDown = new System.Windows.Forms.NumericUpDown();
            this.gbMode = new System.Windows.Forms.GroupBox();
            this.NumPointsUpDown = new System.Windows.Forms.NumericUpDown();
            this.rbPoints = new System.Windows.Forms.RadioButton();
            this.rbTime = new System.Windows.Forms.RadioButton();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.gbFileName = new System.Windows.Forms.GroupBox();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.txtFileName = new System.Windows.Forms.TextBox();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage2.SuspendLayout();
            this.gbDuration.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.DurationSecondsUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.DurationHoursUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.DurationMinutesUpDown)).BeginInit();
            this.gbMode.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.NumPointsUpDown)).BeginInit();
            this.tabPage1.SuspendLayout();
            this.gbFileName.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(190, 240);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(82, 25);
            this.btnCancel.TabIndex = 16;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOK.Location = new System.Drawing.Point(102, 240);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(82, 25);
            this.btnOK.TabIndex = 15;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // tabPage2
            // 
            this.tabPage2.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.tabPage2.Controls.Add(this.gbDuration);
            this.tabPage2.Controls.Add(this.gbMode);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(265, 208);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Settings";
            // 
            // gbDuration
            // 
            this.gbDuration.Controls.Add(this.lblDuration);
            this.gbDuration.Controls.Add(this.DurationSecondsUpDown);
            this.gbDuration.Controls.Add(this.DurationHoursUpDown);
            this.gbDuration.Controls.Add(this.DurationMinutesUpDown);
            this.gbDuration.Location = new System.Drawing.Point(3, 96);
            this.gbDuration.Name = "gbDuration";
            this.gbDuration.Size = new System.Drawing.Size(253, 73);
            this.gbDuration.TabIndex = 2;
            this.gbDuration.TabStop = false;
            this.gbDuration.Text = "Duration";
            // 
            // lblDuration
            // 
            this.lblDuration.AutoSize = true;
            this.lblDuration.Location = new System.Drawing.Point(72, 22);
            this.lblDuration.Name = "lblDuration";
            this.lblDuration.Size = new System.Drawing.Size(81, 13);
            this.lblDuration.TabIndex = 13;
            this.lblDuration.Text = "hh   :  mm  :   ss";
            // 
            // DurationSecondsUpDown
            // 
            this.DurationSecondsUpDown.Location = new System.Drawing.Point(135, 38);
            this.DurationSecondsUpDown.Maximum = new decimal(new int[] {
            59,
            0,
            0,
            0});
            this.DurationSecondsUpDown.Name = "DurationSecondsUpDown";
            this.DurationSecondsUpDown.Size = new System.Drawing.Size(36, 20);
            this.DurationSecondsUpDown.TabIndex = 12;
            this.DurationSecondsUpDown.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.DurationSecondsUpDown.Value = new decimal(new int[] {
            59,
            0,
            0,
            0});
            // 
            // DurationHoursUpDown
            // 
            this.DurationHoursUpDown.Location = new System.Drawing.Point(65, 38);
            this.DurationHoursUpDown.Maximum = new decimal(new int[] {
            999,
            0,
            0,
            0});
            this.DurationHoursUpDown.Name = "DurationHoursUpDown";
            this.DurationHoursUpDown.Size = new System.Drawing.Size(40, 20);
            this.DurationHoursUpDown.TabIndex = 11;
            this.DurationHoursUpDown.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.DurationHoursUpDown.Value = new decimal(new int[] {
            999,
            0,
            0,
            0});
            // 
            // DurationMinutesUpDown
            // 
            this.DurationMinutesUpDown.Location = new System.Drawing.Point(103, 38);
            this.DurationMinutesUpDown.Maximum = new decimal(new int[] {
            59,
            0,
            0,
            0});
            this.DurationMinutesUpDown.Name = "DurationMinutesUpDown";
            this.DurationMinutesUpDown.Size = new System.Drawing.Size(36, 20);
            this.DurationMinutesUpDown.TabIndex = 10;
            this.DurationMinutesUpDown.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.DurationMinutesUpDown.Value = new decimal(new int[] {
            59,
            0,
            0,
            0});
            // 
            // gbMode
            // 
            this.gbMode.Controls.Add(this.NumPointsUpDown);
            this.gbMode.Controls.Add(this.rbPoints);
            this.gbMode.Controls.Add(this.rbTime);
            this.gbMode.Location = new System.Drawing.Point(6, 6);
            this.gbMode.Name = "gbMode";
            this.gbMode.Size = new System.Drawing.Size(253, 84);
            this.gbMode.TabIndex = 0;
            this.gbMode.TabStop = false;
            this.gbMode.Text = "Mode";
            // 
            // NumPointsUpDown
            // 
            this.NumPointsUpDown.Location = new System.Drawing.Point(90, 47);
            this.NumPointsUpDown.Maximum = new decimal(new int[] {
            999,
            0,
            0,
            0});
            this.NumPointsUpDown.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.NumPointsUpDown.Name = "NumPointsUpDown";
            this.NumPointsUpDown.Size = new System.Drawing.Size(52, 20);
            this.NumPointsUpDown.TabIndex = 5;
            this.NumPointsUpDown.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.NumPointsUpDown.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // rbPoints
            // 
            this.rbPoints.AutoSize = true;
            this.rbPoints.Location = new System.Drawing.Point(30, 47);
            this.rbPoints.Name = "rbPoints";
            this.rbPoints.Size = new System.Drawing.Size(54, 17);
            this.rbPoints.TabIndex = 1;
            this.rbPoints.TabStop = true;
            this.rbPoints.Text = "Points";
            this.rbPoints.UseVisualStyleBackColor = true;
            this.rbPoints.CheckedChanged += new System.EventHandler(this.rbMode_CheckedChanged);
            // 
            // rbTime
            // 
            this.rbTime.AutoSize = true;
            this.rbTime.Location = new System.Drawing.Point(30, 19);
            this.rbTime.Name = "rbTime";
            this.rbTime.Size = new System.Drawing.Size(48, 17);
            this.rbTime.TabIndex = 0;
            this.rbTime.TabStop = true;
            this.rbTime.Text = "Time";
            this.rbTime.UseVisualStyleBackColor = true;
            this.rbTime.CheckedChanged += new System.EventHandler(this.rbMode_CheckedChanged);
            // 
            // tabPage1
            // 
            this.tabPage1.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.tabPage1.Controls.Add(this.gbFileName);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(265, 208);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "File";
            // 
            // gbFileName
            // 
            this.gbFileName.Controls.Add(this.btnBrowse);
            this.gbFileName.Controls.Add(this.txtFileName);
            this.gbFileName.Location = new System.Drawing.Point(5, 46);
            this.gbFileName.Name = "gbFileName";
            this.gbFileName.Size = new System.Drawing.Size(256, 101);
            this.gbFileName.TabIndex = 5;
            this.gbFileName.TabStop = false;
            this.gbFileName.Text = "Name";
            // 
            // btnBrowse
            // 
            this.btnBrowse.Location = new System.Drawing.Point(165, 60);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(82, 25);
            this.btnBrowse.TabIndex = 1;
            this.btnBrowse.Text = "&Browse ...";
            this.btnBrowse.UseVisualStyleBackColor = true;
            this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
            // 
            // txtFileName
            // 
            this.txtFileName.Location = new System.Drawing.Point(6, 34);
            this.txtFileName.Name = "txtFileName";
            this.txtFileName.Size = new System.Drawing.Size(241, 20);
            this.txtFileName.TabIndex = 0;
            this.txtFileName.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Location = new System.Drawing.Point(3, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(273, 234);
            this.tabControl1.TabIndex = 14;
            // 
            // FormSetupFastMode
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(279, 276);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.tabControl1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "FormSetupFastMode";
            this.Text = "Setup Fast Mode";
            this.Load += new System.EventHandler(this.FormSetupFastMode_Load);
            this.tabPage2.ResumeLayout(false);
            this.gbDuration.ResumeLayout(false);
            this.gbDuration.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.DurationSecondsUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.DurationHoursUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.DurationMinutesUpDown)).EndInit();
            this.gbMode.ResumeLayout(false);
            this.gbMode.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.NumPointsUpDown)).EndInit();
            this.tabPage1.ResumeLayout(false);
            this.gbFileName.ResumeLayout(false);
            this.gbFileName.PerformLayout();
            this.tabControl1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.GroupBox gbDuration;
        private System.Windows.Forms.Label lblDuration;
        private System.Windows.Forms.NumericUpDown DurationSecondsUpDown;
        private System.Windows.Forms.NumericUpDown DurationHoursUpDown;
        private System.Windows.Forms.NumericUpDown DurationMinutesUpDown;
        private System.Windows.Forms.GroupBox gbMode;
        private System.Windows.Forms.NumericUpDown NumPointsUpDown;
        private System.Windows.Forms.RadioButton rbPoints;
        private System.Windows.Forms.RadioButton rbTime;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.GroupBox gbFileName;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.TextBox txtFileName;
        private System.Windows.Forms.TabControl tabControl1;
    }
}