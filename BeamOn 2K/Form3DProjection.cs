using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace BeamOn_U3
{
    public partial class Form3DProjection : Form
    {
        SystemData m_sysData = null;

        public Form3DProjection()
        {
            InitializeComponent();

            m_sysData = SystemData.MyInstance;
        }

        public void SetColorPalette(Color[] colorArray)
        {
            image3D.colorArray = colorArray;
        }

        public Point SensorCenterPosition
        {
            get { return image3D.SensorCenterPosition; }
            set { image3D.SensorCenterPosition = value; }
        }

        public void SetScaleGridX(short Step, float Value)
        {
            image3D.SetScaleGridX(Step, Value);
        }

        public void SetScaleGridY(short Step, float Value)
        {
            image3D.SetScaleGridY(Step, Value);
        }

        private void Form3DProjection_Load(object sender, EventArgs e)
        {
            checkProjectionX.Checked = ((m_sysData.projectionData.Projection == OpenGLControl.TypeProjection.XZProjection) || (m_sysData.projectionData.Projection == OpenGLControl.TypeProjection.XZ_YZProjection));
            checkProjectionY.Checked = ((m_sysData.projectionData.Projection == OpenGLControl.TypeProjection.YZProjection) || (m_sysData.projectionData.Projection == OpenGLControl.TypeProjection.XZ_YZProjection));

            if (m_sysData.projectionData.m_eResolution3D == OpenGLControl.TypeGrid.Low)
                rbDensityLow.Checked = true;
            else if (m_sysData.projectionData.m_eResolution3D == OpenGLControl.TypeGrid.Midle)
                rbDensityMedium.Checked = true;
            else if (m_sysData.projectionData.m_eResolution3D == OpenGLControl.TypeGrid.Hight)
                rbDensityHigh.Checked = true;

            checkAutoRotate.Checked = m_sysData.projectionData.bAutoRot;

            checkBoxGrid.Checked = m_sysData.projectionData.bViewGrid;

            StepRotateUpDown.Value = m_sysData.projectionData.StepRotation;
            StepRotateUpDown.Enabled = checkAutoRotate.Checked;
            lblAuto.Enabled = m_sysData.projectionData.bAutoRot;

            RotateUpDown.Enabled = !StepRotateUpDown.Enabled;
            lblRotate.Enabled = !StepRotateUpDown.Enabled;
        }

        public BeamOnCL.SnapshotBase ImageData
        {
            get { return image3D.ImageData; }
            set 
            {
                image3D.ImageData = value; 
            }
        }

        private void image3D_OnChangeAngle(object sender, OpenGLControl.Image3D.ChangeAngleEventArgs e)
        {
            m_sysData.projectionData.AngleTilt = e.AngleY;
            m_sysData.projectionData.AngleRotation = e.AngleX;

            splitContainer3D.Panel2.Invalidate();
        }

        private void splitContainer3D_Panel2_Paint(object sender, PaintEventArgs e)
        {
            RotateUpDown.Value = Convert.ToDecimal(Math.Abs(m_sysData.projectionData.AngleRotation));
            TiltUpDown.Value = Convert.ToDecimal(Math.Abs(m_sysData.projectionData.AngleTilt));
        }

        private void checkBoxGrid_CheckedChanged(object sender, EventArgs e)
        {
            m_sysData.projectionData.bViewGrid = checkBoxGrid.Checked;
            image3D.Grid = m_sysData.projectionData.bViewGrid;
        }

        private void checkProjection_CheckedChanged(object sender, EventArgs e)
        {
            if ((checkProjectionY.Checked == true) && (checkProjectionX.Checked == true))
                m_sysData.projectionData.Projection = OpenGLControl.TypeProjection.XZ_YZProjection;
            else if (checkProjectionY.Checked == true)
                m_sysData.projectionData.Projection = OpenGLControl.TypeProjection.YZProjection;
            else if (checkProjectionX.Checked == true)
                m_sysData.projectionData.Projection = OpenGLControl.TypeProjection.XZProjection;
            else
                m_sysData.projectionData.Projection = OpenGLControl.TypeProjection.NoneProjection;

            image3D.Projection = m_sysData.projectionData.Projection;
        }

        private void rbDensity_CheckedChanged(object sender, EventArgs e)
        {
            if (rbDensityLow.Checked == true)
                m_sysData.projectionData.m_eResolution3D = OpenGLControl.TypeGrid.Low;
            else if (rbDensityMedium.Checked == true)
                m_sysData.projectionData.m_eResolution3D = OpenGLControl.TypeGrid.Midle;
            else if (rbDensityHigh.Checked == true)
                m_sysData.projectionData.m_eResolution3D = OpenGLControl.TypeGrid.Hight;

            image3D.Resolution = m_sysData.projectionData.m_eResolution3D;
        }

        private void checkAutoRotate_CheckedChanged(object sender, EventArgs e)
        {
            m_sysData.projectionData.bAutoRot = checkAutoRotate.Checked;

            image3D.AutoRotateX = m_sysData.projectionData.bAutoRot;

            StepRotateUpDown.Enabled = checkAutoRotate.Checked;
            lblAuto.Enabled = m_sysData.projectionData.bAutoRot;
        }

        private void StepRotateUpDown_ValueChanged(object sender, EventArgs e)
        {
            m_sysData.projectionData.StepRotation = Decimal.ToInt16(StepRotateUpDown.Value);

            image3D.StepAutoRotateX = m_sysData.projectionData.StepRotation;
        }

        private void RotateUpDown_ValueChanged(object sender, EventArgs e)
        {
            m_sysData.projectionData.AngleRotation = Decimal.ToInt16(RotateUpDown.Value);

            image3D.AngleX = m_sysData.projectionData.AngleRotation;
        }

        private void TiltUpDown_ValueChanged(object sender, EventArgs e)
        {
            m_sysData.projectionData.AngleTilt = Decimal.ToInt16(TiltUpDown.Value);

            image3D.AngleY = m_sysData.projectionData.AngleTilt;
        }

        private void buttonProperty_Click(object sender, EventArgs e)
        {
            splitContainer3D.Panel2Collapsed = !splitContainer3D.Panel2Collapsed;
            splitContainer3D.SplitterDistance = splitContainer3D.Width - splitContainer3D.Panel2MinSize- splitContainer3D.SplitterWidth;
        }
    }
}
