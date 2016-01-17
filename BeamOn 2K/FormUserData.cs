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
    public partial class FormUserData : Form
    {
        SystemData m_sysData = null;

        public FormUserData()
        {
            m_sysData = SystemData.MyInstance;
            InitializeComponent();
        }

        private void FormUserData_Load(object sender, EventArgs e)
        {
            if ((m_sysData.applicationData.m_strUserTitle != null) && (m_sysData.applicationData.m_strUserTitle.Equals("") == false))
                textUserData.Text = m_sysData.applicationData.m_strUserTitle;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            m_sysData.applicationData.m_strUserTitle = textUserData.Text;
            this.DialogResult = DialogResult.OK;
        }
    }
}
