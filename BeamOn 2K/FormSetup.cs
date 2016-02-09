using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace BeamOn_2K
{
    public partial class FormSetup : Form
    {
        SystemData m_sysData = null;

        public FormSetup()
        {
            m_sysData = SystemData.MyInstance;

            InitializeComponent();
        }

        private void FormSetup_Load(object sender, EventArgs e)
        {

            gbWaveLength.Enabled = !m_sysData.Simulation && !m_sysData.SnapshotView;
            gbAverage.Enabled = !m_sysData.Simulation && !m_sysData.SnapshotView;
            gbMode.Enabled = !m_sysData.Simulation && !m_sysData.SnapshotView;
            gbUnits.Enabled = !m_sysData.Simulation && !m_sysData.SnapshotView;

            //Binning
            switch (m_sysData.videoDeviceData.uiBinning)
            {
                case 1:
                    radioButtonBinningOff.Checked = true;
                    break;
                case 2:
                    radioButtonBinning2.Checked = true;
                    break;
                case 3:
                    radioButtonBinning4.Checked = true;
                    break;
                case 4:
                    radioButtonBinning8.Checked = true;
                    break;
            }

            //Pixel Format
            if (m_sysData.videoDeviceData.pixelFormat == System.Drawing.Imaging.PixelFormat.Format8bppIndexed)
                radioButton8bpp.Checked = true;
            else if (m_sysData.videoDeviceData.pixelFormat == System.Drawing.Imaging.PixelFormat.Format24bppRgb)
                radioButton12bpp.Checked = true;

            //Step Mode
            rbRun.Checked = m_sysData.RunOn;
            rbStep.Checked = !rbRun.Checked;

            //Profile
            checkGaussian.Checked = m_sysData.ViewGaussian;
            checkAutoscale.Checked = m_sysData.ScaleProfile;

            Level2UpDown.Value = m_sysData.ClipLevels.Level(2);
            Level1UpDown.Value = m_sysData.ClipLevels.Level(1);
            Level0UpDown.Value = m_sysData.ClipLevels.Level(0);

            //Wavelenght
            WaveUpDown.Minimum = m_sysData.powerData.uiWavelenghtMin;
            WaveUpDown.Maximum = m_sysData.powerData.uiWavelenghtMax;
            WaveUpDown.Value = m_sysData.powerData.uiWavelenght;

            //Unit
            rbUnitsmrad.Checked = (m_sysData.UnitMeasure == MeasureUnits.muMiliRad);
            rbUnitsum.Checked = !rbUnitsmrad.Checked;

            //Power Units
            switch (m_sysData.powerData.PowerUnits)
            {
                case 0:
                    rbUnitsmW.Checked = true;
                    break;
                case 1:
                    rbUnitsuW.Checked = true;
                    break;
                case 3:
                    rbUnitsdB.Checked = true;
                    break;
                case 4:
                    rbUnitsW.Checked = true;
                    break;
                case 5:
                    rbUnitskW.Checked = true;
                    break;
            }

        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            //Binning
            if (radioButtonBinningOff.Checked == true)
                m_sysData.videoDeviceData.uiBinning = 1;
            else if (radioButtonBinning2.Checked == true)
                m_sysData.videoDeviceData.uiBinning = 2;
            else if (radioButtonBinning4.Checked == true)
                m_sysData.videoDeviceData.uiBinning = 3;
            else if (radioButtonBinning8.Checked == true)
                m_sysData.videoDeviceData.uiBinning = 4;

            //Pixel Format
            if (radioButton12bpp.Checked == true)
                m_sysData.videoDeviceData.pixelFormat = System.Drawing.Imaging.PixelFormat.Format24bppRgb;
            else if (radioButton8bpp.Checked == true)
                m_sysData.videoDeviceData.pixelFormat = System.Drawing.Imaging.PixelFormat.Format8bppIndexed;

            //Step Mode
            m_sysData.RunOn = rbRun.Checked;

            //Profile
            m_sysData.ViewGaussian = checkGaussian.Checked;
            m_sysData.ScaleProfile = checkAutoscale.Checked;

            m_sysData.ClipLevels.SetLevel(0, Level0UpDown.Value);
            m_sysData.ClipLevels.SetLevel(1, Level1UpDown.Value);
            m_sysData.ClipLevels.SetLevel(2, Level2UpDown.Value);

            //Wavelenght
            m_sysData.powerData.uiWavelenght = Decimal.ToUInt16(WaveUpDown.Value);
            m_sysData.powerData.SetSensitivity();
            m_sysData.powerData.SetFilterFactor();
            m_sysData.powerData.SetSAMFactor();

            //Unit
            m_sysData.UnitMeasure = (rbUnitsmrad.Checked == true) ? MeasureUnits.muMiliRad : MeasureUnits.muMicro;
            m_sysData.FocalLens = Decimal.ToSingle(FocalLensUpDown.Value);

            //Power Units
            if (rbUnitsmW.Checked == true)
                m_sysData.powerData.PowerUnits = 0;
            else if (rbUnitsuW.Checked == true)
                m_sysData.powerData.PowerUnits = 1;
            else if(rbUnitsdB.Checked == true)
                m_sysData.powerData.PowerUnits = 3;
            else if(rbUnitsW.Checked == true)
                m_sysData.powerData.PowerUnits = 4;
             else if(rbUnitskW.Checked == true)
                m_sysData.powerData.PowerUnits = 5;

            this.DialogResult = DialogResult.OK;
        }

        private void Level0UpDown_ValueChanged(object sender, EventArgs e)
        {
            Level1UpDown.Minimum = Level0UpDown.Value + 0.1M;
        }

        private void Level2UpDown_ValueChanged(object sender, EventArgs e)
        {
            Level1UpDown.Maximum = Level2UpDown.Value - 0.1M;
        }

        private void Level1UpDown_ValueChanged(object sender, EventArgs e)
        {
            Level2UpDown.Minimum = Level1UpDown.Value + 0.1M;
            Level0UpDown.Maximum = Level1UpDown.Value - 0.1M;
        }
    }
}
