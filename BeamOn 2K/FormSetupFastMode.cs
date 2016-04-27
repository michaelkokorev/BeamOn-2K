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
        VideoControl.VideoControl m_vc = null;

        public FormSetupFastMode(VideoControl.VideoControl vc)
        {
            InitializeComponent();

            m_vc = vc;
        }

        private void FormSetupFastMode_Load(object sender, EventArgs e)
        {
            txtFileName.Text = m_vc.strFileName;

            //Settings
            rbTime.Checked = (m_vc.Type == VideoControl.VideoControl.RecordType.rtTime);
            rbPoints.Checked = (m_vc.Type == VideoControl.VideoControl.RecordType.rtPoints);

            UpDateLogMode();

            DurationHoursUpDown.Value = (UInt32)(m_vc.RecordDuration / 3600);
            DurationMinutesUpDown.Value = (UInt32)((m_vc.RecordDuration - DurationHoursUpDown.Value * 3600) / 60);
            DurationSecondsUpDown.Value = (UInt32)(m_vc.RecordDuration - DurationHoursUpDown.Value * 3600 - DurationMinutesUpDown.Value * 60);

            NumPointsUpDown.Value = m_vc.RecordNumPoints;
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveConfig = new SaveFileDialog();

            saveConfig.Filter = "Fast Mode Data files (*.fst)|*.fst";
            saveConfig.FileName = "*.fst";
            saveConfig.DefaultExt = "fst";

            saveConfig.FilterIndex = 1;

            saveConfig.AddExtension = true;
            saveConfig.CheckPathExists = true;
            saveConfig.CheckFileExists = false;
            saveConfig.InitialDirectory = m_sysData.applicationData.m_strMyDataDir;
            saveConfig.RestoreDirectory = true;
            saveConfig.Title = "Save Fast Mode Data File";
            saveConfig.FileOk += new CancelEventHandler(saveConfig_FileOk);

            if (saveConfig.ShowDialog() == DialogResult.OK)
            {
                txtFileName.Text = saveConfig.FileName;
            }
        }

        void saveConfig_FileOk(object sender, CancelEventArgs e)
        {
            SaveFileDialog saveConfig = (SaveFileDialog)sender;

            if (Path.GetExtension(saveConfig.FileName).CompareTo(".fst") != 0)
            {
                CustomMessageBox.Show("Extension of the Fast Mode Data File should be 'FST'",
                                "Save Fast Mode Data File", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

                e.Cancel = true;
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
                CustomMessageBox.Show("Path on the Fast Mode Data file not defined",
                                "Setup Fast Mode Data Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

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
                                        "Setup Fast Mode Data Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

                        return;
                    }
                }
                else
                {
                    CustomMessageBox.Show("The file name is invalid",
                                    "Setup Fast Mode Data Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

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
                                    "Setup Fast Mode Data Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

                    return;
                }
            }

            if ((rbTime.Checked == true) && ((Decimal.ToUInt32(DurationHoursUpDown.Value) * 3600 + Decimal.ToUInt32(DurationMinutesUpDown.Value) * 60 + Decimal.ToUInt32(DurationSecondsUpDown.Value)) == 0))
            {
                CustomMessageBox.Show("Duration time must be bigger than '0'",
                                "Setup Fast Mode Data Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

                return;
            }

            m_vc.strFileName = txtFileName.Text;

            //Settings
            m_vc.Type = (rbTime.Checked == true) ? VideoControl.VideoControl.RecordType.rtTime : VideoControl.VideoControl.RecordType.rtPoints;

            m_vc.RecordDuration = Decimal.ToUInt32(DurationHoursUpDown.Value) * 3600 + Decimal.ToUInt32(DurationMinutesUpDown.Value) * 60 + Decimal.ToUInt32(DurationSecondsUpDown.Value);

            m_vc.RecordNumPoints = Decimal.ToUInt32(NumPointsUpDown.Value);

            this.DialogResult = DialogResult.OK;
        }
    }
}
