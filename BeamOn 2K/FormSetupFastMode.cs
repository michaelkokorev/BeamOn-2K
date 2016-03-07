using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace BeamOn_U3
{
    public partial class FormSetupFastMode : Form
    {
        SystemData m_sysData = SystemData.MyInstance;

        public FormSetupFastMode()
        {
            InitializeComponent();
        }

        private void FormSetupFastMode_Load(object sender, EventArgs e)
        {
            //File
            if (DataExcel.CheckExcelInst() == true)
            {
                if (m_sysData.fastModeData.ftFile == FileType.ftLog)
                    rbFileLog.Checked = true;
                else if (m_sysData.fastModeData.ftFile == FileType.ftExcel)
                    rbFileExcel.Checked = true;
                else
                    rbFileHtml.Checked = true;
            }
            else
            {
                rbFileExcel.Enabled = false;
                if (m_sysData.fastModeData.ftFile == FileType.ftLog)
                    rbFileLog.Checked = true;
                else
                    rbFileHtml.Checked = true;
            }

            txtFileName.Text = SetExtension(m_sysData.fastModeData.strFileName);

            //Settings
            rbTime.Checked = (m_sysData.fastModeData.ltMode == LogType.ltTime);
            rbPoints.Checked = (m_sysData.logData.ltMode == LogType.ltPoints);

            UpDateLogMode();

            DurationHoursUpDown.Value = (UInt32)(m_sysData.fastModeData.LogDuration / 3600);
            DurationMinutesUpDown.Value = (UInt32)((m_sysData.fastModeData.LogDuration - DurationHoursUpDown.Value * 3600) / 60);
            DurationSecondsUpDown.Value = (UInt32)(m_sysData.fastModeData.LogDuration - DurationHoursUpDown.Value * 3600 - DurationMinutesUpDown.Value * 60);

            NumPointsUpDown.Value = m_sysData.logData.LogNumPoints;
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveConfig = new SaveFileDialog();

            if (rbFileLog.Checked == true)
            {
                saveConfig.Filter = "Fast Log files (*.fst)|*.fst";
                saveConfig.FileName = "*.fst";
                saveConfig.DefaultExt = "fst";
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
            saveConfig.Title = "Save Fast Mode Log File";
            saveConfig.FileOk += new CancelEventHandler(saveConfig_FileOk);

            if (saveConfig.ShowDialog() == DialogResult.OK)
            {
                txtFileName.Text = saveConfig.FileName;
            }
        }

        void saveConfig_FileOk(object sender, CancelEventArgs e)
        {
            SaveFileDialog saveConfig = (SaveFileDialog)sender;

            if (rbFileLog.Checked == true)
            {
                if (Path.GetExtension(saveConfig.FileName).CompareTo(".fst") != 0)
                {
                    CustomMessageBox.Show("Extension of the Log File should be 'FST'",
                                    "Save Fast Mode Log File", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

                    e.Cancel = true;
                }
            }
            else if (rbFileExcel.Checked == true)
            {
                if ((DataExcel.Version < 12) && (Path.GetExtension(saveConfig.FileName).CompareTo(".xls") != 0))
                {
                    CustomMessageBox.Show("Extension of the Fast Mode Log File should be 'XLS'",
                                    "Save Fast Mode Log File", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

                    e.Cancel = true;
                }
                else if ((DataExcel.Version >= 12) && (Path.GetExtension(saveConfig.FileName).CompareTo(".xlsx") != 0))
                {
                    CustomMessageBox.Show("Extension of the Fast Mode Log File should be 'XLSX'",
                                    "Save Fast Mode Log File", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

                    e.Cancel = true;
                }
            }
            else
            {
                if (Path.GetExtension(saveConfig.FileName).CompareTo(".xml") != 0)
                {
                    CustomMessageBox.Show("Extension of the Fast Mode Log File should be 'XML'",
                                    "Save Fast Mode Log File", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

                    e.Cancel = true;
                }
            }
        }

        private void UpDateLogMode()
        {
            NumPointsUpDown.Enabled = rbPoints.Checked;
            gbDuration.Enabled = rbTime.Checked;
        }

        private String SetExtension(String strFileName)
        {
            String strExt = "fst";

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

        private void rbMode_CheckedChanged(object sender, EventArgs e)
        {
            UpDateLogMode();
        }

        private void rbFile_CheckedChanged(object sender, EventArgs e)
        {
            txtFileName.Text = SetExtension(txtFileName.Text);
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
                                        "Setup Fast Mode Log Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

                        return;
                    }
                }
                else
                {
                    CustomMessageBox.Show("The file name is invalid",
                                    "Setup Fast Mode Log Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

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
                                    "Setup Fast Mode Log Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

                    return;
                }
            }

            if ((rbTime.Checked == true) && ((Decimal.ToUInt32(DurationHoursUpDown.Value) * 3600 + Decimal.ToUInt32(DurationMinutesUpDown.Value) * 60 + Decimal.ToUInt32(DurationSecondsUpDown.Value)) == 0))
            {
                CustomMessageBox.Show("Duration time must be bigger than '0'",
                                "Setup Fast Mode Log Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

                return;
            }

            //File
            if (rbFileLog.Checked == true)
                m_sysData.fastModeData.ftFile = FileType.ftLog;
            else if (rbFileExcel.Checked == true)
                m_sysData.fastModeData.ftFile = FileType.ftExcel;
            else
                m_sysData.fastModeData.ftFile = FileType.ftXML;

            m_sysData.fastModeData.strFileName = txtFileName.Text;

            //Settings
            m_sysData.logData.ltMode = (rbTime.Checked == true) ? LogType.ltTime : LogType.ltPoints;

            m_sysData.fastModeData.LogDuration = Decimal.ToUInt32(DurationHoursUpDown.Value) * 3600 + Decimal.ToUInt32(DurationMinutesUpDown.Value) * 60 + Decimal.ToUInt32(DurationSecondsUpDown.Value);

            m_sysData.fastModeData.LogNumPoints = Decimal.ToUInt32(NumPointsUpDown.Value);

            this.DialogResult = DialogResult.OK;
        }
    }
}
