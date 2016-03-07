namespace BeamOn_U3
{
    partial class FormWellcome
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
            this.tableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.lblMeasurementSystem = new System.Windows.Forms.Label();
            this.logoPictureBox = new System.Windows.Forms.PictureBox();
            this.labelProductName = new System.Windows.Forms.Label();
            this.labelVersion = new System.Windows.Forms.Label();
            this.labelSerialNumber = new System.Windows.Forms.Label();
            this.labelCopyright = new System.Windows.Forms.Label();
            this.labelCompanyName = new System.Windows.Forms.Label();
            this.lblCheckLevel = new System.Windows.Forms.Label();
            this.checkProgress = new System.Windows.Forms.ProgressBar();
            this.timerSplash = new System.Windows.Forms.Timer(this.components);
            this.tableLayoutPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.logoPictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // tableLayoutPanel
            // 
            this.tableLayoutPanel.BackColor = System.Drawing.Color.Black;
            this.tableLayoutPanel.ColumnCount = 4;
            this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20.91388F));
            this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 21.26538F));
            this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 38.31283F));
            this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 19.22862F));
            this.tableLayoutPanel.Controls.Add(this.lblMeasurementSystem, 2, 2);
            this.tableLayoutPanel.Controls.Add(this.logoPictureBox, 0, 0);
            this.tableLayoutPanel.Controls.Add(this.labelProductName, 2, 0);
            this.tableLayoutPanel.Controls.Add(this.labelVersion, 2, 3);
            this.tableLayoutPanel.Controls.Add(this.labelSerialNumber, 2, 4);
            this.tableLayoutPanel.Controls.Add(this.labelCopyright, 1, 8);
            this.tableLayoutPanel.Controls.Add(this.labelCompanyName, 2, 5);
            this.tableLayoutPanel.Controls.Add(this.lblCheckLevel, 2, 6);
            this.tableLayoutPanel.Controls.Add(this.checkProgress, 2, 7);
            this.tableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel.Margin = new System.Windows.Forms.Padding(1);
            this.tableLayoutPanel.Name = "tableLayoutPanel";
            this.tableLayoutPanel.RowCount = 9;
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 21.36873F));
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 9.941521F));
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25.1462F));
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10.55276F));
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 13.45029F));
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 19.29825F));
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 31F));
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 42F));
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel.Size = new System.Drawing.Size(569, 260);
            this.tableLayoutPanel.TabIndex = 1;
            // 
            // lblMeasurementSystem
            // 
            this.lblMeasurementSystem.BackColor = System.Drawing.Color.Transparent;
            this.tableLayoutPanel.SetColumnSpan(this.lblMeasurementSystem, 2);
            this.lblMeasurementSystem.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblMeasurementSystem.Font = new System.Drawing.Font("Arial", 12F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lblMeasurementSystem.ForeColor = System.Drawing.Color.White;
            this.lblMeasurementSystem.Location = new System.Drawing.Point(243, 48);
            this.lblMeasurementSystem.Name = "lblMeasurementSystem";
            this.lblMeasurementSystem.Size = new System.Drawing.Size(323, 39);
            this.lblMeasurementSystem.TabIndex = 34;
            this.lblMeasurementSystem.Text = "Measurement System";
            this.lblMeasurementSystem.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // logoPictureBox
            // 
            this.tableLayoutPanel.SetColumnSpan(this.logoPictureBox, 2);
            this.logoPictureBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.logoPictureBox.Image = global::BeamOn_U3.Properties.Resources.WELCOME;
            this.logoPictureBox.Location = new System.Drawing.Point(3, 3);
            this.logoPictureBox.Name = "logoPictureBox";
            this.tableLayoutPanel.SetRowSpan(this.logoPictureBox, 8);
            this.logoPictureBox.Size = new System.Drawing.Size(234, 219);
            this.logoPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.logoPictureBox.TabIndex = 12;
            this.logoPictureBox.TabStop = false;
            // 
            // labelProductName
            // 
            this.labelProductName.AutoSize = true;
            this.tableLayoutPanel.SetColumnSpan(this.labelProductName, 2);
            this.labelProductName.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelProductName.Font = new System.Drawing.Font("Arial Rounded MT Bold", 24F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelProductName.ForeColor = System.Drawing.Color.White;
            this.labelProductName.Location = new System.Drawing.Point(246, 0);
            this.labelProductName.Margin = new System.Windows.Forms.Padding(6, 0, 3, 0);
            this.labelProductName.Name = "labelProductName";
            this.tableLayoutPanel.SetRowSpan(this.labelProductName, 2);
            this.labelProductName.Size = new System.Drawing.Size(320, 48);
            this.labelProductName.TabIndex = 19;
            this.labelProductName.Text = "BeamOn Series 2K";
            this.labelProductName.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            // 
            // labelVersion
            // 
            this.tableLayoutPanel.SetColumnSpan(this.labelVersion, 2);
            this.labelVersion.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelVersion.ForeColor = System.Drawing.Color.White;
            this.labelVersion.Location = new System.Drawing.Point(246, 87);
            this.labelVersion.Margin = new System.Windows.Forms.Padding(6, 0, 3, 0);
            this.labelVersion.Name = "labelVersion";
            this.labelVersion.Size = new System.Drawing.Size(320, 16);
            this.labelVersion.TabIndex = 0;
            this.labelVersion.Text = "Version 2.0 USB";
            this.labelVersion.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // labelSerialNumber
            // 
            this.tableLayoutPanel.SetColumnSpan(this.labelSerialNumber, 2);
            this.labelSerialNumber.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelSerialNumber.ForeColor = System.Drawing.Color.White;
            this.labelSerialNumber.Location = new System.Drawing.Point(246, 103);
            this.labelSerialNumber.Margin = new System.Windows.Forms.Padding(6, 0, 3, 0);
            this.labelSerialNumber.Name = "labelSerialNumber";
            this.labelSerialNumber.Size = new System.Drawing.Size(320, 20);
            this.labelSerialNumber.TabIndex = 35;
            this.labelSerialNumber.Text = "Serial Number: ";
            this.labelSerialNumber.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // labelCopyright
            // 
            this.tableLayoutPanel.SetColumnSpan(this.labelCopyright, 2);
            this.labelCopyright.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelCopyright.ForeColor = System.Drawing.Color.White;
            this.labelCopyright.Location = new System.Drawing.Point(125, 225);
            this.labelCopyright.Margin = new System.Windows.Forms.Padding(6, 0, 3, 0);
            this.labelCopyright.Name = "labelCopyright";
            this.labelCopyright.Size = new System.Drawing.Size(330, 35);
            this.labelCopyright.TabIndex = 21;
            this.labelCopyright.Text = "Copyright ©  2009  Duma Optronics Ltd., Israel";
            this.labelCopyright.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // labelCompanyName
            // 
            this.tableLayoutPanel.SetColumnSpan(this.labelCompanyName, 2);
            this.labelCompanyName.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelCompanyName.ForeColor = System.Drawing.Color.White;
            this.labelCompanyName.Location = new System.Drawing.Point(246, 123);
            this.labelCompanyName.Margin = new System.Windows.Forms.Padding(6, 0, 3, 0);
            this.labelCompanyName.Name = "labelCompanyName";
            this.labelCompanyName.Size = new System.Drawing.Size(320, 29);
            this.labelCompanyName.TabIndex = 22;
            this.labelCompanyName.Text = "Licensed to: Noname";
            this.labelCompanyName.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            // 
            // lblCheckLevel
            // 
            this.lblCheckLevel.BackColor = System.Drawing.Color.Transparent;
            this.tableLayoutPanel.SetColumnSpan(this.lblCheckLevel, 2);
            this.lblCheckLevel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblCheckLevel.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lblCheckLevel.ForeColor = System.Drawing.SystemColors.ControlLight;
            this.lblCheckLevel.Location = new System.Drawing.Point(243, 152);
            this.lblCheckLevel.Name = "lblCheckLevel";
            this.lblCheckLevel.Size = new System.Drawing.Size(323, 31);
            this.lblCheckLevel.TabIndex = 36;
            this.lblCheckLevel.Text = "Checking hardware ...";
            this.lblCheckLevel.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            // 
            // checkProgress
            // 
            this.tableLayoutPanel.SetColumnSpan(this.checkProgress, 2);
            this.checkProgress.Location = new System.Drawing.Point(265, 186);
            this.checkProgress.Margin = new System.Windows.Forms.Padding(25, 3, 3, 3);
            this.checkProgress.Name = "checkProgress";
            this.checkProgress.Size = new System.Drawing.Size(282, 10);
            this.checkProgress.TabIndex = 37;
            // 
            // timerSplash
            // 
            this.timerSplash.Interval = 25;
            this.timerSplash.Tick += new System.EventHandler(this.timerSplash_Tick);
            // 
            // FormWellcome
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(569, 260);
            this.Controls.Add(this.tableLayoutPanel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "FormWellcome";
            this.Opacity = 0.01D;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.TopMost = true;
            this.Load += new System.EventHandler(this.FormWellcome_Load);
            this.tableLayoutPanel.ResumeLayout(false);
            this.tableLayoutPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.logoPictureBox)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel;
        private System.Windows.Forms.Label lblMeasurementSystem;
        private System.Windows.Forms.PictureBox logoPictureBox;
        private System.Windows.Forms.Label labelProductName;
        private System.Windows.Forms.Label labelVersion;
        private System.Windows.Forms.Label labelSerialNumber;
        private System.Windows.Forms.Label labelCopyright;
        private System.Windows.Forms.Label labelCompanyName;
        private System.Windows.Forms.Label lblCheckLevel;
        private System.Windows.Forms.ProgressBar checkProgress;
        private System.Windows.Forms.Timer timerSplash;
    }
}