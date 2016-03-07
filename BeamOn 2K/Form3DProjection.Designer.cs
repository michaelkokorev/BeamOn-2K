namespace BeamOn_U3
{
    partial class Form3DProjection
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
            this.splitContainer3D = new System.Windows.Forms.SplitContainer();
            this.buttonProperty = new System.Windows.Forms.Button();
            this.image3D = new OpenGLControl.Image3D();
            this.groupBoxGrid = new System.Windows.Forms.GroupBox();
            this.checkBoxGrid = new System.Windows.Forms.CheckBox();
            this.gbProjection = new System.Windows.Forms.GroupBox();
            this.checkProjectionY = new System.Windows.Forms.CheckBox();
            this.checkProjectionX = new System.Windows.Forms.CheckBox();
            this.gbDensity = new System.Windows.Forms.GroupBox();
            this.rbDensityHigh = new System.Windows.Forms.RadioButton();
            this.rbDensityMedium = new System.Windows.Forms.RadioButton();
            this.rbDensityLow = new System.Windows.Forms.RadioButton();
            this.gbTilt = new System.Windows.Forms.GroupBox();
            this.TiltUpDown = new System.Windows.Forms.NumericUpDown();
            this.lblTilt = new System.Windows.Forms.Label();
            this.gbRotation = new System.Windows.Forms.GroupBox();
            this.RotateUpDown = new System.Windows.Forms.NumericUpDown();
            this.StepRotateUpDown = new System.Windows.Forms.NumericUpDown();
            this.lblRotate = new System.Windows.Forms.Label();
            this.lblAuto = new System.Windows.Forms.Label();
            this.checkAutoRotate = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3D)).BeginInit();
            this.splitContainer3D.Panel1.SuspendLayout();
            this.splitContainer3D.Panel2.SuspendLayout();
            this.splitContainer3D.SuspendLayout();
            this.groupBoxGrid.SuspendLayout();
            this.gbProjection.SuspendLayout();
            this.gbDensity.SuspendLayout();
            this.gbTilt.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.TiltUpDown)).BeginInit();
            this.gbRotation.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.RotateUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.StepRotateUpDown)).BeginInit();
            this.SuspendLayout();
            // 
            // splitContainer3D
            // 
            this.splitContainer3D.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.splitContainer3D.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer3D.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer3D.IsSplitterFixed = true;
            this.splitContainer3D.Location = new System.Drawing.Point(0, 0);
            this.splitContainer3D.Name = "splitContainer3D";
            // 
            // splitContainer3D.Panel1
            // 
            this.splitContainer3D.Panel1.Controls.Add(this.buttonProperty);
            this.splitContainer3D.Panel1.Controls.Add(this.image3D);
            this.splitContainer3D.Panel1MinSize = 420;
            // 
            // splitContainer3D.Panel2
            // 
            this.splitContainer3D.Panel2.Controls.Add(this.groupBoxGrid);
            this.splitContainer3D.Panel2.Controls.Add(this.gbProjection);
            this.splitContainer3D.Panel2.Controls.Add(this.gbDensity);
            this.splitContainer3D.Panel2.Controls.Add(this.gbTilt);
            this.splitContainer3D.Panel2.Controls.Add(this.gbRotation);
            this.splitContainer3D.Panel2.Paint += new System.Windows.Forms.PaintEventHandler(this.splitContainer3D_Panel2_Paint);
            this.splitContainer3D.Panel2Collapsed = true;
            this.splitContainer3D.Panel2MinSize = 165;
            this.splitContainer3D.Size = new System.Drawing.Size(605, 422);
            this.splitContainer3D.SplitterDistance = 424;
            this.splitContainer3D.SplitterWidth = 2;
            this.splitContainer3D.TabIndex = 1;
            // 
            // buttonProperty
            // 
            this.buttonProperty.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonProperty.AutoEllipsis = true;
            this.buttonProperty.FlatAppearance.BorderSize = 0;
            this.buttonProperty.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonProperty.Image = global::BeamOn_U3.Properties.Resources.PropertyS11;
            this.buttonProperty.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.buttonProperty.Location = new System.Drawing.Point(570, 0);
            this.buttonProperty.Name = "buttonProperty";
            this.buttonProperty.Size = new System.Drawing.Size(33, 88);
            this.buttonProperty.TabIndex = 3;
            this.buttonProperty.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.buttonProperty.UseCompatibleTextRendering = true;
            this.buttonProperty.UseVisualStyleBackColor = false;
            this.buttonProperty.Click += new System.EventHandler(this.buttonProperty_Click);
            // 
            // image3D
            // 
            this.image3D.AngleX = 30F;
            this.image3D.AngleY = 30F;
            this.image3D.AngleZ = 30F;
            this.image3D.AutoRotateX = false;
            this.image3D.AutoRotateY = false;
            this.image3D.AutoRotateZ = false;
            this.image3D.colorArray = null;
            this.image3D.Dock = System.Windows.Forms.DockStyle.Fill;
            this.image3D.FocalLens = 1F;
            this.image3D.Grid = true;
            this.image3D.ImageData = null;
            this.image3D.Location = new System.Drawing.Point(0, 0);
            this.image3D.Name = "image3D";
            this.image3D.OpticalFactor = 1F;
            this.image3D.Projection = OpenGLControl.TypeProjection.NoneProjection;
            this.image3D.Resolution = OpenGLControl.TypeGrid.Low;
            this.image3D.SensorCenterPosition = new System.Drawing.Point(0, 0);
            this.image3D.Size = new System.Drawing.Size(603, 420);
            this.image3D.StepAutoRotateX = 5F;
            this.image3D.StepAutoRotateY = 5F;
            this.image3D.StepAutoRotateZ = 5F;
            this.image3D.TabIndex = 1;
            this.image3D.UnitMeasure = OpenGLControl.MeasureUnits.muMicro;
            this.image3D.ViewingRect = new System.Drawing.Rectangle(0, 0, 0, 0);
            this.image3D.OnChangeAngle += new OpenGLControl.Image3D.ChangeAngle(this.image3D_OnChangeAngle);
            // 
            // groupBoxGrid
            // 
            this.groupBoxGrid.Controls.Add(this.checkBoxGrid);
            this.groupBoxGrid.Location = new System.Drawing.Point(2, 355);
            this.groupBoxGrid.Name = "groupBoxGrid";
            this.groupBoxGrid.Size = new System.Drawing.Size(159, 54);
            this.groupBoxGrid.TabIndex = 28;
            this.groupBoxGrid.TabStop = false;
            this.groupBoxGrid.Text = "Grid";
            // 
            // checkBoxGrid
            // 
            this.checkBoxGrid.AutoSize = true;
            this.checkBoxGrid.Location = new System.Drawing.Point(55, 19);
            this.checkBoxGrid.Name = "checkBoxGrid";
            this.checkBoxGrid.Size = new System.Drawing.Size(40, 17);
            this.checkBoxGrid.TabIndex = 1;
            this.checkBoxGrid.Text = "On";
            this.checkBoxGrid.UseVisualStyleBackColor = true;
            this.checkBoxGrid.CheckedChanged += new System.EventHandler(this.checkBoxGrid_CheckedChanged);
            // 
            // gbProjection
            // 
            this.gbProjection.Controls.Add(this.checkProjectionY);
            this.gbProjection.Controls.Add(this.checkProjectionX);
            this.gbProjection.Location = new System.Drawing.Point(3, 273);
            this.gbProjection.Name = "gbProjection";
            this.gbProjection.Size = new System.Drawing.Size(159, 76);
            this.gbProjection.TabIndex = 27;
            this.gbProjection.TabStop = false;
            this.gbProjection.Text = "Projection";
            // 
            // checkProjectionY
            // 
            this.checkProjectionY.AutoSize = true;
            this.checkProjectionY.Location = new System.Drawing.Point(55, 42);
            this.checkProjectionY.Name = "checkProjectionY";
            this.checkProjectionY.Size = new System.Drawing.Size(40, 17);
            this.checkProjectionY.TabIndex = 2;
            this.checkProjectionY.Text = "YZ";
            this.checkProjectionY.UseVisualStyleBackColor = true;
            this.checkProjectionY.CheckedChanged += new System.EventHandler(this.checkProjection_CheckedChanged);
            // 
            // checkProjectionX
            // 
            this.checkProjectionX.AutoSize = true;
            this.checkProjectionX.Location = new System.Drawing.Point(55, 19);
            this.checkProjectionX.Name = "checkProjectionX";
            this.checkProjectionX.Size = new System.Drawing.Size(40, 17);
            this.checkProjectionX.TabIndex = 1;
            this.checkProjectionX.Text = "XZ";
            this.checkProjectionX.UseVisualStyleBackColor = true;
            this.checkProjectionX.CheckedChanged += new System.EventHandler(this.checkProjection_CheckedChanged);
            // 
            // gbDensity
            // 
            this.gbDensity.Controls.Add(this.rbDensityHigh);
            this.gbDensity.Controls.Add(this.rbDensityMedium);
            this.gbDensity.Controls.Add(this.rbDensityLow);
            this.gbDensity.Location = new System.Drawing.Point(3, 164);
            this.gbDensity.Name = "gbDensity";
            this.gbDensity.Size = new System.Drawing.Size(159, 103);
            this.gbDensity.TabIndex = 26;
            this.gbDensity.TabStop = false;
            this.gbDensity.Text = "Wide Density";
            // 
            // rbDensityHigh
            // 
            this.rbDensityHigh.AutoSize = true;
            this.rbDensityHigh.Location = new System.Drawing.Point(55, 74);
            this.rbDensityHigh.Name = "rbDensityHigh";
            this.rbDensityHigh.Size = new System.Drawing.Size(47, 17);
            this.rbDensityHigh.TabIndex = 5;
            this.rbDensityHigh.TabStop = true;
            this.rbDensityHigh.Text = "High";
            this.rbDensityHigh.UseVisualStyleBackColor = true;
            this.rbDensityHigh.CheckedChanged += new System.EventHandler(this.rbDensity_CheckedChanged);
            // 
            // rbDensityMedium
            // 
            this.rbDensityMedium.AutoSize = true;
            this.rbDensityMedium.Location = new System.Drawing.Point(55, 51);
            this.rbDensityMedium.Name = "rbDensityMedium";
            this.rbDensityMedium.Size = new System.Drawing.Size(62, 17);
            this.rbDensityMedium.TabIndex = 4;
            this.rbDensityMedium.TabStop = true;
            this.rbDensityMedium.Text = "Medium";
            this.rbDensityMedium.UseVisualStyleBackColor = true;
            this.rbDensityMedium.CheckedChanged += new System.EventHandler(this.rbDensity_CheckedChanged);
            // 
            // rbDensityLow
            // 
            this.rbDensityLow.AutoSize = true;
            this.rbDensityLow.Location = new System.Drawing.Point(55, 28);
            this.rbDensityLow.Name = "rbDensityLow";
            this.rbDensityLow.Size = new System.Drawing.Size(45, 17);
            this.rbDensityLow.TabIndex = 3;
            this.rbDensityLow.TabStop = true;
            this.rbDensityLow.Text = "Low";
            this.rbDensityLow.UseVisualStyleBackColor = true;
            this.rbDensityLow.CheckedChanged += new System.EventHandler(this.rbDensity_CheckedChanged);
            // 
            // gbTilt
            // 
            this.gbTilt.Controls.Add(this.TiltUpDown);
            this.gbTilt.Controls.Add(this.lblTilt);
            this.gbTilt.Location = new System.Drawing.Point(3, 107);
            this.gbTilt.Name = "gbTilt";
            this.gbTilt.Size = new System.Drawing.Size(159, 51);
            this.gbTilt.TabIndex = 25;
            this.gbTilt.TabStop = false;
            this.gbTilt.Text = "Tilt";
            // 
            // TiltUpDown
            // 
            this.TiltUpDown.Location = new System.Drawing.Point(100, 19);
            this.TiltUpDown.Maximum = new decimal(new int[] {
            359,
            0,
            0,
            0});
            this.TiltUpDown.Name = "TiltUpDown";
            this.TiltUpDown.Size = new System.Drawing.Size(55, 20);
            this.TiltUpDown.TabIndex = 7;
            this.TiltUpDown.ValueChanged += new System.EventHandler(this.TiltUpDown_ValueChanged);
            // 
            // lblTilt
            // 
            this.lblTilt.AutoSize = true;
            this.lblTilt.Location = new System.Drawing.Point(13, 22);
            this.lblTilt.Name = "lblTilt";
            this.lblTilt.Size = new System.Drawing.Size(84, 13);
            this.lblTilt.TabIndex = 6;
            this.lblTilt.Text = "Horizontal Tilt (°)\r\n";
            // 
            // gbRotation
            // 
            this.gbRotation.Controls.Add(this.RotateUpDown);
            this.gbRotation.Controls.Add(this.StepRotateUpDown);
            this.gbRotation.Controls.Add(this.lblRotate);
            this.gbRotation.Controls.Add(this.lblAuto);
            this.gbRotation.Controls.Add(this.checkAutoRotate);
            this.gbRotation.Location = new System.Drawing.Point(3, 3);
            this.gbRotation.Name = "gbRotation";
            this.gbRotation.Size = new System.Drawing.Size(159, 98);
            this.gbRotation.TabIndex = 24;
            this.gbRotation.TabStop = false;
            this.gbRotation.Text = "Rotation";
            // 
            // RotateUpDown
            // 
            this.RotateUpDown.Location = new System.Drawing.Point(100, 70);
            this.RotateUpDown.Maximum = new decimal(new int[] {
            359,
            0,
            0,
            0});
            this.RotateUpDown.Name = "RotateUpDown";
            this.RotateUpDown.Size = new System.Drawing.Size(55, 20);
            this.RotateUpDown.TabIndex = 5;
            this.RotateUpDown.ValueChanged += new System.EventHandler(this.RotateUpDown_ValueChanged);
            // 
            // StepRotateUpDown
            // 
            this.StepRotateUpDown.Location = new System.Drawing.Point(100, 44);
            this.StepRotateUpDown.Maximum = new decimal(new int[] {
            30,
            0,
            0,
            0});
            this.StepRotateUpDown.Name = "StepRotateUpDown";
            this.StepRotateUpDown.Size = new System.Drawing.Size(55, 20);
            this.StepRotateUpDown.TabIndex = 4;
            this.StepRotateUpDown.ValueChanged += new System.EventHandler(this.StepRotateUpDown_ValueChanged);
            // 
            // lblRotate
            // 
            this.lblRotate.AutoSize = true;
            this.lblRotate.Location = new System.Drawing.Point(12, 72);
            this.lblRotate.Name = "lblRotate";
            this.lblRotate.Size = new System.Drawing.Size(82, 13);
            this.lblRotate.TabIndex = 3;
            this.lblRotate.Text = "Rotate Angle (°)";
            // 
            // lblAuto
            // 
            this.lblAuto.AutoSize = true;
            this.lblAuto.Location = new System.Drawing.Point(52, 47);
            this.lblAuto.Name = "lblAuto";
            this.lblAuto.Size = new System.Drawing.Size(42, 13);
            this.lblAuto.TabIndex = 2;
            this.lblAuto.Text = "Step (°)";
            // 
            // checkAutoRotate
            // 
            this.checkAutoRotate.AutoSize = true;
            this.checkAutoRotate.Location = new System.Drawing.Point(15, 21);
            this.checkAutoRotate.Name = "checkAutoRotate";
            this.checkAutoRotate.Size = new System.Drawing.Size(119, 17);
            this.checkAutoRotate.TabIndex = 0;
            this.checkAutoRotate.Text = "Auto-Rotate On/Off";
            this.checkAutoRotate.UseVisualStyleBackColor = true;
            this.checkAutoRotate.CheckedChanged += new System.EventHandler(this.checkAutoRotate_CheckedChanged);
            // 
            // Form3DProjection
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(605, 422);
            this.Controls.Add(this.splitContainer3D);
            this.DoubleBuffered = true;
            this.HelpButton = true;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(600, 420);
            this.Name = "Form3DProjection";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "3D Projection";
            this.Load += new System.EventHandler(this.Form3DProjection_Load);
            this.splitContainer3D.Panel1.ResumeLayout(false);
            this.splitContainer3D.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3D)).EndInit();
            this.splitContainer3D.ResumeLayout(false);
            this.groupBoxGrid.ResumeLayout(false);
            this.groupBoxGrid.PerformLayout();
            this.gbProjection.ResumeLayout(false);
            this.gbProjection.PerformLayout();
            this.gbDensity.ResumeLayout(false);
            this.gbDensity.PerformLayout();
            this.gbTilt.ResumeLayout(false);
            this.gbTilt.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.TiltUpDown)).EndInit();
            this.gbRotation.ResumeLayout(false);
            this.gbRotation.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.RotateUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.StepRotateUpDown)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer3D;
        private OpenGLControl.Image3D image3D;
        private System.Windows.Forms.GroupBox gbRotation;
        private System.Windows.Forms.NumericUpDown RotateUpDown;
        private System.Windows.Forms.NumericUpDown StepRotateUpDown;
        private System.Windows.Forms.Label lblRotate;
        private System.Windows.Forms.Label lblAuto;
        private System.Windows.Forms.CheckBox checkAutoRotate;
        private System.Windows.Forms.GroupBox gbTilt;
        private System.Windows.Forms.NumericUpDown TiltUpDown;
        private System.Windows.Forms.Label lblTilt;
        private System.Windows.Forms.GroupBox gbDensity;
        private System.Windows.Forms.RadioButton rbDensityHigh;
        private System.Windows.Forms.RadioButton rbDensityMedium;
        private System.Windows.Forms.RadioButton rbDensityLow;
        private System.Windows.Forms.GroupBox gbProjection;
        private System.Windows.Forms.CheckBox checkProjectionY;
        private System.Windows.Forms.CheckBox checkProjectionX;
        private System.Windows.Forms.GroupBox groupBoxGrid;
        private System.Windows.Forms.CheckBox checkBoxGrid;
        private System.Windows.Forms.Button buttonProperty;

    }
}