namespace BeamOn_2K
{
    partial class FormSetupLog
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
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.gbFileName = new System.Windows.Forms.GroupBox();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.txtFileName = new System.Windows.Forms.TextBox();
            this.gbTypeFile = new System.Windows.Forms.GroupBox();
            this.rbFileHtml = new System.Windows.Forms.RadioButton();
            this.rbFileExcel = new System.Windows.Forms.RadioButton();
            this.rbFileLog = new System.Windows.Forms.RadioButton();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.gbDuration = new System.Windows.Forms.GroupBox();
            this.lblDuration = new System.Windows.Forms.Label();
            this.DurationSecondsUpDown = new System.Windows.Forms.NumericUpDown();
            this.DurationHoursUpDown = new System.Windows.Forms.NumericUpDown();
            this.DurationMinutesUpDown = new System.Windows.Forms.NumericUpDown();
            this.gbInterval = new System.Windows.Forms.GroupBox();
            this.lblInterval = new System.Windows.Forms.Label();
            this.IntervalSecondsUpDown = new System.Windows.Forms.NumericUpDown();
            this.IntervalHoursUpDown = new System.Windows.Forms.NumericUpDown();
            this.IntervalMinutesUpDown = new System.Windows.Forms.NumericUpDown();
            this.gbMode = new System.Windows.Forms.GroupBox();
            this.NumPointsUpDown = new System.Windows.Forms.NumericUpDown();
            this.rbManual = new System.Windows.Forms.RadioButton();
            this.rbPoints = new System.Windows.Forms.RadioButton();
            this.rbTime = new System.Windows.Forms.RadioButton();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.btnClearAll = new System.Windows.Forms.Button();
            this.btnSelectAll = new System.Windows.Forms.Button();
            this.checkedListBoxDataLog = new System.Windows.Forms.CheckedListBox();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.gbFileName.SuspendLayout();
            this.gbTypeFile.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.gbDuration.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.DurationSecondsUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.DurationHoursUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.DurationMinutesUpDown)).BeginInit();
            this.gbInterval.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.IntervalSecondsUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.IntervalHoursUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.IntervalMinutesUpDown)).BeginInit();
            this.gbMode.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.NumPointsUpDown)).BeginInit();
            this.tabPage3.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(199, 403);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(82, 25);
            this.btnCancel.TabIndex = 12;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(111, 403);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(82, 25);
            this.btnOK.TabIndex = 11;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Location = new System.Drawing.Point(12, 12);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(273, 385);
            this.tabControl1.TabIndex = 13;
            // 
            // tabPage1
            // 
            this.tabPage1.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.tabPage1.Controls.Add(this.gbFileName);
            this.tabPage1.Controls.Add(this.gbTypeFile);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(265, 359);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "File";
            // 
            // gbFileName
            // 
            this.gbFileName.Controls.Add(this.btnBrowse);
            this.gbFileName.Controls.Add(this.txtFileName);
            this.gbFileName.Location = new System.Drawing.Point(3, 151);
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
            // gbTypeFile
            // 
            this.gbTypeFile.Controls.Add(this.rbFileHtml);
            this.gbTypeFile.Controls.Add(this.rbFileExcel);
            this.gbTypeFile.Controls.Add(this.rbFileLog);
            this.gbTypeFile.Location = new System.Drawing.Point(3, 19);
            this.gbTypeFile.Name = "gbTypeFile";
            this.gbTypeFile.Size = new System.Drawing.Size(256, 126);
            this.gbTypeFile.TabIndex = 4;
            this.gbTypeFile.TabStop = false;
            this.gbTypeFile.Text = "Type";
            // 
            // rbFileHtml
            // 
            this.rbFileHtml.AutoSize = true;
            this.rbFileHtml.Location = new System.Drawing.Point(92, 80);
            this.rbFileHtml.Name = "rbFileHtml";
            this.rbFileHtml.Size = new System.Drawing.Size(55, 17);
            this.rbFileHtml.TabIndex = 2;
            this.rbFileHtml.TabStop = true;
            this.rbFileHtml.Text = "HTML";
            this.rbFileHtml.UseVisualStyleBackColor = true;
            this.rbFileHtml.CheckedChanged += new System.EventHandler(this.rbFile_CheckedChanged);
            // 
            // rbFileExcel
            // 
            this.rbFileExcel.AutoSize = true;
            this.rbFileExcel.Location = new System.Drawing.Point(92, 50);
            this.rbFileExcel.Name = "rbFileExcel";
            this.rbFileExcel.Size = new System.Drawing.Size(51, 17);
            this.rbFileExcel.TabIndex = 1;
            this.rbFileExcel.TabStop = true;
            this.rbFileExcel.Text = "Excel";
            this.rbFileExcel.UseVisualStyleBackColor = true;
            this.rbFileExcel.CheckedChanged += new System.EventHandler(this.rbFile_CheckedChanged);
            // 
            // rbFileLog
            // 
            this.rbFileLog.AutoSize = true;
            this.rbFileLog.Location = new System.Drawing.Point(92, 19);
            this.rbFileLog.Name = "rbFileLog";
            this.rbFileLog.Size = new System.Drawing.Size(43, 17);
            this.rbFileLog.TabIndex = 0;
            this.rbFileLog.TabStop = true;
            this.rbFileLog.Text = "Log";
            this.rbFileLog.UseVisualStyleBackColor = true;
            this.rbFileLog.CheckedChanged += new System.EventHandler(this.rbFile_CheckedChanged);
            // 
            // tabPage2
            // 
            this.tabPage2.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.tabPage2.Controls.Add(this.gbDuration);
            this.tabPage2.Controls.Add(this.gbInterval);
            this.tabPage2.Controls.Add(this.gbMode);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(265, 359);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Settings";
            // 
            // gbDuration
            // 
            this.gbDuration.Controls.Add(this.lblDuration);
            this.gbDuration.Controls.Add(this.DurationSecondsUpDown);
            this.gbDuration.Controls.Add(this.DurationHoursUpDown);
            this.gbDuration.Controls.Add(this.DurationMinutesUpDown);
            this.gbDuration.Location = new System.Drawing.Point(6, 203);
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
            // gbInterval
            // 
            this.gbInterval.Controls.Add(this.lblInterval);
            this.gbInterval.Controls.Add(this.IntervalSecondsUpDown);
            this.gbInterval.Controls.Add(this.IntervalHoursUpDown);
            this.gbInterval.Controls.Add(this.IntervalMinutesUpDown);
            this.gbInterval.Location = new System.Drawing.Point(6, 121);
            this.gbInterval.Name = "gbInterval";
            this.gbInterval.Size = new System.Drawing.Size(256, 76);
            this.gbInterval.TabIndex = 1;
            this.gbInterval.TabStop = false;
            this.gbInterval.Text = "Interval";
            // 
            // lblInterval
            // 
            this.lblInterval.AutoSize = true;
            this.lblInterval.Location = new System.Drawing.Point(72, 21);
            this.lblInterval.Name = "lblInterval";
            this.lblInterval.Size = new System.Drawing.Size(81, 13);
            this.lblInterval.TabIndex = 9;
            this.lblInterval.Text = "hh   :  mm  :   ss";
            // 
            // IntervalSecondsUpDown
            // 
            this.IntervalSecondsUpDown.Location = new System.Drawing.Point(135, 37);
            this.IntervalSecondsUpDown.Maximum = new decimal(new int[] {
            59,
            0,
            0,
            0});
            this.IntervalSecondsUpDown.Name = "IntervalSecondsUpDown";
            this.IntervalSecondsUpDown.Size = new System.Drawing.Size(36, 20);
            this.IntervalSecondsUpDown.TabIndex = 8;
            this.IntervalSecondsUpDown.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.IntervalSecondsUpDown.Value = new decimal(new int[] {
            59,
            0,
            0,
            0});
            // 
            // IntervalHoursUpDown
            // 
            this.IntervalHoursUpDown.Location = new System.Drawing.Point(65, 37);
            this.IntervalHoursUpDown.Maximum = new decimal(new int[] {
            999,
            0,
            0,
            0});
            this.IntervalHoursUpDown.Name = "IntervalHoursUpDown";
            this.IntervalHoursUpDown.Size = new System.Drawing.Size(40, 20);
            this.IntervalHoursUpDown.TabIndex = 7;
            this.IntervalHoursUpDown.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.IntervalHoursUpDown.Value = new decimal(new int[] {
            999,
            0,
            0,
            0});
            // 
            // IntervalMinutesUpDown
            // 
            this.IntervalMinutesUpDown.Location = new System.Drawing.Point(103, 37);
            this.IntervalMinutesUpDown.Maximum = new decimal(new int[] {
            59,
            0,
            0,
            0});
            this.IntervalMinutesUpDown.Name = "IntervalMinutesUpDown";
            this.IntervalMinutesUpDown.Size = new System.Drawing.Size(36, 20);
            this.IntervalMinutesUpDown.TabIndex = 6;
            this.IntervalMinutesUpDown.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.IntervalMinutesUpDown.Value = new decimal(new int[] {
            59,
            0,
            0,
            0});
            // 
            // gbMode
            // 
            this.gbMode.Controls.Add(this.NumPointsUpDown);
            this.gbMode.Controls.Add(this.rbManual);
            this.gbMode.Controls.Add(this.rbPoints);
            this.gbMode.Controls.Add(this.rbTime);
            this.gbMode.Location = new System.Drawing.Point(6, 6);
            this.gbMode.Name = "gbMode";
            this.gbMode.Size = new System.Drawing.Size(253, 109);
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
            // rbManual
            // 
            this.rbManual.AutoSize = true;
            this.rbManual.Location = new System.Drawing.Point(30, 75);
            this.rbManual.Name = "rbManual";
            this.rbManual.Size = new System.Drawing.Size(216, 17);
            this.rbManual.TabIndex = 2;
            this.rbManual.TabStop = true;
            this.rbManual.Text = "Manual  (press SpaceBar to save Event)";
            this.rbManual.UseVisualStyleBackColor = true;
            this.rbManual.CheckedChanged += new System.EventHandler(this.rbMode_CheckedChanged);
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
            // tabPage3
            // 
            this.tabPage3.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.tabPage3.Controls.Add(this.checkedListBoxDataLog);
            this.tabPage3.Controls.Add(this.btnClearAll);
            this.tabPage3.Controls.Add(this.btnSelectAll);
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Size = new System.Drawing.Size(265, 359);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "Data";
            // 
            // btnClearAll
            // 
            this.btnClearAll.Location = new System.Drawing.Point(179, 324);
            this.btnClearAll.Name = "btnClearAll";
            this.btnClearAll.Size = new System.Drawing.Size(70, 20);
            this.btnClearAll.TabIndex = 10;
            this.btnClearAll.Text = "Clear All";
            this.btnClearAll.UseVisualStyleBackColor = true;
            this.btnClearAll.Click += new System.EventHandler(this.btnClearAll_Click);
            // 
            // btnSelectAll
            // 
            this.btnSelectAll.Location = new System.Drawing.Point(106, 324);
            this.btnSelectAll.Name = "btnSelectAll";
            this.btnSelectAll.Size = new System.Drawing.Size(70, 20);
            this.btnSelectAll.TabIndex = 9;
            this.btnSelectAll.Text = "Select All";
            this.btnSelectAll.UseVisualStyleBackColor = true;
            this.btnSelectAll.Click += new System.EventHandler(this.btnSelectAll_Click);
            // 
            // checkedListBoxDataLog
            // 
            this.checkedListBoxDataLog.FormattingEnabled = true;
            this.checkedListBoxDataLog.Items.AddRange(new object[] {
            "Horizontal Profile Width Level 1",
            "Horizontal Profile Width Level 2",
            "Horizontal Profile Width Level 3",
            "Horizontal Gaussian Width Level 1",
            "Horizontal Gaussian Width Level 2",
            "Horizontal Gaussian Width Level 3",
            "Horizontal Gaussian fit",
            "Vertical Profile Width Level 1",
            "Vertical Profile Width Level 2",
            "Vertical Profile Width Level 3",
            "Vertical Gaussian Width Level 1",
            "Vertical Gaussian Width Level 2",
            "Vertical Gaussian Width Level 3",
            "Vertical Gaussian fit",
            "Power",
            "Position X",
            "Position Y",
            "Major",
            "Minor",
            "Orientation"});
            this.checkedListBoxDataLog.Location = new System.Drawing.Point(13, 14);
            this.checkedListBoxDataLog.Name = "checkedListBoxDataLog";
            this.checkedListBoxDataLog.Size = new System.Drawing.Size(236, 304);
            this.checkedListBoxDataLog.TabIndex = 11;
            // 
            // FormSetupLog
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(296, 436);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.HelpButton = true;
            this.Name = "FormSetupLog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Setup Log";
            this.TopMost = true;
            this.Load += new System.EventHandler(this.FormSetupLog_Load);
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.gbFileName.ResumeLayout(false);
            this.gbFileName.PerformLayout();
            this.gbTypeFile.ResumeLayout(false);
            this.gbTypeFile.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.gbDuration.ResumeLayout(false);
            this.gbDuration.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.DurationSecondsUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.DurationHoursUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.DurationMinutesUpDown)).EndInit();
            this.gbInterval.ResumeLayout(false);
            this.gbInterval.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.IntervalSecondsUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.IntervalHoursUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.IntervalMinutesUpDown)).EndInit();
            this.gbMode.ResumeLayout(false);
            this.gbMode.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.NumPointsUpDown)).EndInit();
            this.tabPage3.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.GroupBox gbFileName;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.TextBox txtFileName;
        private System.Windows.Forms.GroupBox gbTypeFile;
        private System.Windows.Forms.RadioButton rbFileHtml;
        private System.Windows.Forms.RadioButton rbFileExcel;
        private System.Windows.Forms.RadioButton rbFileLog;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.GroupBox gbDuration;
        private System.Windows.Forms.Label lblDuration;
        private System.Windows.Forms.NumericUpDown DurationSecondsUpDown;
        private System.Windows.Forms.NumericUpDown DurationHoursUpDown;
        private System.Windows.Forms.NumericUpDown DurationMinutesUpDown;
        private System.Windows.Forms.GroupBox gbInterval;
        private System.Windows.Forms.Label lblInterval;
        private System.Windows.Forms.NumericUpDown IntervalSecondsUpDown;
        private System.Windows.Forms.NumericUpDown IntervalHoursUpDown;
        private System.Windows.Forms.NumericUpDown IntervalMinutesUpDown;
        private System.Windows.Forms.GroupBox gbMode;
        private System.Windows.Forms.NumericUpDown NumPointsUpDown;
        private System.Windows.Forms.RadioButton rbManual;
        private System.Windows.Forms.RadioButton rbPoints;
        private System.Windows.Forms.RadioButton rbTime;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.CheckedListBox checkedListBoxDataLog;
        private System.Windows.Forms.Button btnClearAll;
        private System.Windows.Forms.Button btnSelectAll;
    }
}