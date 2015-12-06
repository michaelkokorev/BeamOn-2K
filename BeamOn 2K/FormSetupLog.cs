using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace BeamOn_2K
{
    public partial class FormSetupLog : Form
    {
        SystemData m_sysData = SystemData.MyInstance;

        public FormSetupLog()
        {
            InitializeComponent();
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveConfig = new SaveFileDialog();

            if (rbFileLog.Checked == true)
            {
                saveConfig.Filter = "Log files (*.log)|*.log";
                saveConfig.FileName = "*.log";
                saveConfig.DefaultExt = "log";
            }
            else if (rbFileExcel.Checked == true)
            {
                if (DataExcel.Version < 12)
                {
                    saveConfig.Filter = "Excel files (*.xls)|*.xls";
                    saveConfig.FileName = "*.xls";
                    saveConfig.DefaultExt = "xls";
                }
                else
                {
                    saveConfig.Filter = "Excel files (*.xlsx)|*.xlsx";
                    saveConfig.FileName = "*.xlsx";
                    saveConfig.DefaultExt = "xlsx";
                }
            }
            else
            {
                saveConfig.Filter = "XML files (*.xml)|*.xml";
                saveConfig.FileName = "*.xml";
                saveConfig.DefaultExt = "xml";
            }

            saveConfig.FilterIndex = 1;

            saveConfig.AddExtension = true;
            saveConfig.CheckPathExists = true;
            saveConfig.CheckFileExists = false;
            saveConfig.InitialDirectory = m_sysData.applicationData.m_strMyDataDir;
            saveConfig.RestoreDirectory = true;
            saveConfig.Title = "Save Log File";
            saveConfig.FileOk += new CancelEventHandler(saveConfig_FileOk);

            if (saveConfig.ShowDialog() == DialogResult.OK)
            {
                txtFileName.Text = saveConfig.FileName;
            }
        }

        private void UpDateLogMode()
        {
            NumPointsUpDown.Enabled = rbPoints.Checked;
            gbInterval.Enabled = rbTime.Checked;
            gbDuration.Enabled = rbTime.Checked;
        }

        void saveConfig_FileOk(object sender, CancelEventArgs e)
        {
            SaveFileDialog saveConfig = (SaveFileDialog)sender;

            if (rbFileLog.Checked == true)
            {
                if (Path.GetExtension(saveConfig.FileName).CompareTo(".log") != 0)
                {
                    CustomMessageBox.Show("Extension of the Log File should be 'LOG'",
                                    "Save Log File", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

                    e.Cancel = true;
                }
            }
            else if (rbFileExcel.Checked == true)
            {
                if ((DataExcel.Version < 12) && (Path.GetExtension(saveConfig.FileName).CompareTo(".xls") != 0))
                {
                    CustomMessageBox.Show("Extension of the Log File should be 'XLS'",
                                    "Save Log File", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

                    e.Cancel = true;
                }
                else if ((DataExcel.Version >= 12) && (Path.GetExtension(saveConfig.FileName).CompareTo(".xlsx") != 0))
                {
                    CustomMessageBox.Show("Extension of the Log File should be 'XLSX'",
                                    "Save Log File", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

                    e.Cancel = true;
                }
            }
            else
            {
                if (Path.GetExtension(saveConfig.FileName).CompareTo(".xml") != 0)
                {
                    CustomMessageBox.Show("Extension of the Log File should be 'XML'",
                                    "Save Log File", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

                    e.Cancel = true;
                }
            }
        }

        private void rbFile_CheckedChanged(object sender, EventArgs e)
        {
            txtFileName.Text = SetExtension(txtFileName.Text);
        }

        private String SetExtension(String strFileName)
        {
            String strExt = "log";

            if ((strFileName == null) && (strFileName == "")) return strFileName;

            if (rbFileExcel.Checked == true)
            {
                if (DataExcel.Version < 12)
                    strExt = "xls";
                else if (DataExcel.Version >= 12)
                    strExt = "xlsx";
            }
            else if (rbFileHtml.Checked == true)
            {
                strExt = "xml";
            }

            return Path.ChangeExtension(strFileName, strExt);
        }

        private void FormSetupLog_Load(object sender, EventArgs e)
        {
            //File
            if (DataExcel.CheckExcelInst() == true)
            {
                if (m_sysData.logData.ftFile == FileType.ftLog)
                    rbFileLog.Checked = true;
                else if (m_sysData.logData.ftFile == FileType.ftExcel)
                    rbFileExcel.Checked = true;
                else
                    rbFileHtml.Checked = true;
            }
            else
            {
                rbFileExcel.Enabled = false;
                if (m_sysData.logData.ftFile == FileType.ftLog)
                    rbFileLog.Checked = true;
                else
                    rbFileHtml.Checked = true;
            }

            txtFileName.Text = SetExtension(m_sysData.logData.strFileName);

            //Settings
            if (m_sysData.logData.ltMode == LogType.ltTime)
            {
                rbTime.Checked = true;
            }
            else if (m_sysData.logData.ltMode == LogType.ltPoints)
            {
                rbPoints.Checked = true;
            }
            else
            {
                rbManual.Checked = true;
            }

            UpDateLogMode();

            DurationHoursUpDown.Value = (UInt32)(m_sysData.logData.LogDuration / 3600);
            DurationMinutesUpDown.Value = (UInt32)((m_sysData.logData.LogDuration - DurationHoursUpDown.Value * 3600) / 60);
            DurationSecondsUpDown.Value = (UInt32)(m_sysData.logData.LogDuration - DurationHoursUpDown.Value * 3600 - DurationMinutesUpDown.Value * 60);

            IntervalHoursUpDown.Value = (UInt32)(m_sysData.logData.LogInterval / 3600);
            IntervalMinutesUpDown.Value = (UInt32)((m_sysData.logData.LogInterval - IntervalHoursUpDown.Value * 3600) / 60);
            IntervalSecondsUpDown.Value = (UInt32)(m_sysData.logData.LogInterval - IntervalHoursUpDown.Value * 3600 - IntervalMinutesUpDown.Value * 60);

            NumPointsUpDown.Value = m_sysData.logData.LogNumPoints;

            //Data
            chkPower.Checked = m_sysData.logData.bPower;
            chkPositionX.Checked = m_sysData.logData.bPositionX;
            chkPositionY.Checked = m_sysData.logData.bPositionY;
            chkWidthW1.Checked = m_sysData.logData.bWidthW1;
            chkWidthW2.Checked = m_sysData.logData.bWidthW2;
            chkWidthW3.Checked = m_sysData.logData.bWidthW3;
            chkWidthV1.Checked = m_sysData.logData.bWidthV1;
            chkWidthV2.Checked = m_sysData.logData.bWidthV2;
            chkWidthV3.Checked = m_sysData.logData.bWidthV3;
            chkGausfitV.Checked = m_sysData.logData.bGaussfitV;
            chkGausfitW.Checked = m_sysData.logData.bGaussfitW;
            chkGaussWidthW1.Checked = m_sysData.logData.bGaussWidthW1;
            chkGaussWidthW2.Checked = m_sysData.logData.bGaussWidthW2;
            chkGaussWidthW3.Checked = m_sysData.logData.bGaussWidthW3;
            chkGaussWidthV1.Checked = m_sysData.logData.bGaussWidthV1;
            chkGaussWidthV2.Checked = m_sysData.logData.bGaussWidthV2;
            chkGaussWidthV3.Checked = m_sysData.logData.bGaussWidthV3;
            chkMajor.Checked = m_sysData.logData.bMajor;
            chkMinor.Checked = m_sysData.logData.bMinor;
            chkOrientation.Checked = m_sysData.logData.bOrientation;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            btnOK.DialogResult = DialogResult.None;
            if (txtFileName.Text == "")
            {
                CustomMessageBox.Show("Path on the Log file not defined",
                                "Setup Log Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

                return;
            }

            String strPathName = txtFileName.Text;
            String strFileName = txtFileName.Text;
            int lastLocation = strPathName.LastIndexOf("\\");
            if (lastLocation >= 0)
            {
                strPathName = strPathName.Substring(0, lastLocation);

                if (lastLocation < strFileName.Length)
                {
                    strFileName = strFileName.Substring(lastLocation, strFileName.Length - lastLocation - 1);

                    if (strFileName != "")
                    {
                        txtFileName.Text = SetExtension(txtFileName.Text);
                    }
                    else
                    {
                        CustomMessageBox.Show("The file name is invalid",
                                        "Setup Log Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

                        return;
                    }
                }
                else
                {
                    CustomMessageBox.Show("The file name is invalid",
                                    "Setup Log Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

                    return;
                }
            }

            if (Directory.Exists(strPathName) == false)
            {
                try
                {
                    // Try to create the directory.
                    DirectoryInfo di = Directory.CreateDirectory(strPathName);

                    // Delete the directory.
                    di.Delete();
                }
                catch //(DirectoryNotFoundException ex)
                {
                    CustomMessageBox.Show("The specified path is invalid",
                                    "Setup Log Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

                    return;
                }
            }

            if ((rbTime.Checked == true) && ((Decimal.ToUInt32(DurationHoursUpDown.Value) * 3600 + Decimal.ToUInt32(DurationMinutesUpDown.Value) * 60 + Decimal.ToUInt32(DurationSecondsUpDown.Value)) == 0))
            {
                CustomMessageBox.Show("Duration time must be bigger than '0'",
                                "Setup Log Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

                return;
            }

            //File
            if (rbFileLog.Checked == true)
                m_sysData.logData.ftFile = FileType.ftLog;
            else if (rbFileExcel.Checked == true)
                m_sysData.logData.ftFile = FileType.ftExcel;
            else
                m_sysData.logData.ftFile = FileType.ftXML;

            m_sysData.logData.strFileName = txtFileName.Text;

            //Settings
            if (rbTime.Checked == true)
            {
                m_sysData.logData.ltMode = LogType.ltTime;
            }
            else if (rbPoints.Checked == true)
            {
                m_sysData.logData.ltMode = LogType.ltPoints;
            }
            else
            {
                m_sysData.logData.ltMode = LogType.ltManual;
            }

            m_sysData.logData.LogDuration = Decimal.ToUInt32(DurationHoursUpDown.Value) * 3600 + Decimal.ToUInt32(DurationMinutesUpDown.Value) * 60 + Decimal.ToUInt32(DurationSecondsUpDown.Value);

            m_sysData.logData.LogInterval = Decimal.ToUInt32(IntervalHoursUpDown.Value) * 3600 + Decimal.ToUInt32(IntervalMinutesUpDown.Value) * 60 + Decimal.ToUInt32(IntervalSecondsUpDown.Value);

            m_sysData.logData.LogNumPoints = Decimal.ToUInt32(NumPointsUpDown.Value);

            //Data
            m_sysData.logData.bPower = chkPower.Checked;
            m_sysData.logData.bPositionX = chkPositionX.Checked;
            m_sysData.logData.bPositionY = chkPositionY.Checked;
            m_sysData.logData.bWidthW1 = chkWidthW1.Checked;
            m_sysData.logData.bWidthW2 = chkWidthW2.Checked;
            m_sysData.logData.bWidthW3 = chkWidthW3.Checked;
            m_sysData.logData.bWidthV1 = chkWidthV1.Checked;
            m_sysData.logData.bWidthV2 = chkWidthV2.Checked;
            m_sysData.logData.bWidthV3 = chkWidthV3.Checked;
            m_sysData.logData.bGaussfitV = chkGausfitV.Checked;
            m_sysData.logData.bGaussfitW = chkGausfitW.Checked;
            m_sysData.logData.bGaussWidthW1 = chkGaussWidthW1.Checked;
            m_sysData.logData.bGaussWidthW2 = chkGaussWidthW2.Checked;
            m_sysData.logData.bGaussWidthW3 = chkGaussWidthW3.Checked;
            m_sysData.logData.bGaussWidthV1 = chkGaussWidthV1.Checked;
            m_sysData.logData.bGaussWidthV2 = chkGaussWidthV2.Checked;
            m_sysData.logData.bGaussWidthV3 = chkGaussWidthV3.Checked;
            m_sysData.logData.bMajor = chkMajor.Checked;
            m_sysData.logData.bMinor = chkMinor.Checked;
            m_sysData.logData.bOrientation = chkOrientation.Checked;

            this.DialogResult = DialogResult.OK;
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveConfig = new SaveFileDialog();

            if (rbFileLog.Checked == true)
            {
                saveConfig.Filter = "Log files (*.log)|*.log";
                saveConfig.FileName = "*.log";
                saveConfig.DefaultExt = "log";
            }
            else if (rbFileExcel.Checked == true)
            {
                if (DataExcel.Version < 12)
                {
                    saveConfig.Filter = "Excel files (*.xls)|*.xls";
                    saveConfig.FileName = "*.xls";
                    saveConfig.DefaultExt = "xls";
                }
                else
                {
                    saveConfig.Filter = "Excel files (*.xlsx)|*.xlsx";
                    saveConfig.FileName = "*.xlsx";
                    saveConfig.DefaultExt = "xlsx";
                }
            }
            else
            {
                saveConfig.Filter = "XML files (*.xml)|*.xml";
                saveConfig.FileName = "*.xml";
                saveConfig.DefaultExt = "xml";
            }

            saveConfig.FilterIndex = 1;

            saveConfig.AddExtension = true;
            saveConfig.CheckPathExists = true;
            saveConfig.CheckFileExists = false;
            saveConfig.InitialDirectory = m_sysData.applicationData.m_strMyDataDir;
            saveConfig.RestoreDirectory = true;
            saveConfig.Title = "Save Log File";
            saveConfig.FileOk += new CancelEventHandler(saveConfig_FileOk);

            if (saveConfig.ShowDialog() == DialogResult.OK)
            {
                txtFileName.Text = saveConfig.FileName;
            }
        }

        private void rbMode_CheckedChanged(object sender, EventArgs e)
        {
            UpDateLogMode();
        }

        private void btnSelectAll_Click(object sender, EventArgs e)
        {

        }

        private void btnClearAll_Click(object sender, EventArgs e)
        {

        }
    }
}
