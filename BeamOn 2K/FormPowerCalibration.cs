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
    public partial class FormPowerCalibration : Form
    {
        private delegate void AsyncMeasureCount();
        private int m_iMesureCount = 0;
        private float m_sPowerCalibration = 0f;

        public FormPowerCalibration()
        {
            InitializeComponent();
        }

        private void FormPowerCalibration_Load(object sender, EventArgs e)
        {
            progressBarCalibration.Maximum = FormMain.POWER_CALIBRATION_NUM;
        }

        public int MeasureCount
        {
            get { return m_iMesureCount; }
            set
            {
                m_iMesureCount = value;
                AsyncMeasureCount asyncTimeStamp = new AsyncMeasureCount(UpdateMeasureCountAsync);
                asyncTimeStamp.BeginInvoke(null, null);
            }
        }

        private void UpdateMeasureCountAsync()
        {
            try
            {
                this.Invoke((MethodInvoker)delegate
                {
                    if ((m_iMesureCount <= progressBarCalibration.Maximum) && (m_iMesureCount >= progressBarCalibration.Minimum))
                    {
                        progressBarCalibration.Value = m_iMesureCount;
                        if (progressBarCalibration.Value == progressBarCalibration.Maximum)
                        {
                            panelStart.Left = -1000;
                            panelPower.Left = 3;
                        }
                    }
                    this.Invalidate();
                });
            }
            catch
            {
            }
        }

        private void textBoxPowerValue_TextChanged(object sender, EventArgs e)
        {
            buttonOk.Enabled = (Convert.ToSingle(textBoxPowerValue.Text) != 0);
        }

        private void buttonOk_Click(object sender, EventArgs e)
        {
            m_sPowerCalibration = Convert.ToSingle(textBoxPowerValue.Text);
            this.DialogResult = DialogResult.OK;
        }

        public float PowerCalibration { get { return m_sPowerCalibration; } }
    }
}
