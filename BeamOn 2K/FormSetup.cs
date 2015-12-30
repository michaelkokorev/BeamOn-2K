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

            this.DialogResult = DialogResult.OK;
        }
    }
}
