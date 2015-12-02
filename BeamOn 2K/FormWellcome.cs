using System;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace BeamOn_2K
{
    public partial class FormWellcome : Form
    {
        float m_fStep = 0.01f;

        public FormWellcome()
        {
            InitializeComponent();
            this.labelProductName.Text = AssemblyProduct;
            this.labelVersion.Text = String.Format("Version {0}", AssemblyVersion);
            this.labelCopyright.Text = AssemblyCopyright;
            this.labelCompanyName.Text = AssemblyCompany;
        }

        #region Assembly Attribute Accessors

        public string AssemblyTitle
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
                if (attributes.Length > 0)
                {
                    AssemblyTitleAttribute titleAttribute = (AssemblyTitleAttribute)attributes[0];
                    if (titleAttribute.Title != "")
                    {
                        return titleAttribute.Title;
                    }
                }
                return System.IO.Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().CodeBase);
            }
        }

        public string AssemblyVersion
        {
            get
            {
                return Assembly.GetExecutingAssembly().GetName().Version.ToString();
            }
        }

        public string AssemblyDescription
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyDescriptionAttribute)attributes[0]).Description;
            }
        }

        public string AssemblyProduct
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyProductAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyProductAttribute)attributes[0]).Product;
            }
        }

        public string AssemblyCopyright
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyCopyrightAttribute)attributes[0]).Copyright;
            }
        }

        public string AssemblyCompany
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCompanyAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyCompanyAttribute)attributes[0]).Company;
            }
        }
        #endregion

        private void timerSplash_Tick(object sender, EventArgs e)
        {
            try
            {
                Opacity += m_fStep;
            }
            catch { }

            if (Opacity >= 0.99)
            {
                m_fStep = -0.01f;
                CheckHardware();
            }

            if (Opacity <= 0)
            {
                try
                {
                    Close();
                }
                catch { }
            }
        }


        private void CheckHardware()
        {
            this.DialogResult = DialogResult.OK;
            timerSplash.Enabled = false;
            lblCheckLevel.Text = "Checking hardware ...";

            BeamOnCL.CheckHardware beamOnCheck = new BeamOnCL.CheckHardware();
            beamOnCheck.OnGetCheckError +=new BeamOnCL.CheckHardware.GetCheckError(beamOnCheck_OnGetCheckError);
            beamOnCheck.OnGetCheckLevel +=new BeamOnCL.CheckHardware.GetCheckLevel(beamOnCheck_OnGetCheckLevel);
            beamOnCheck.StartCheck();

            timerSplash.Enabled = true;
        }

        void beamOnCheck_OnGetCheckLevel(object sender, BeamOnCL.CheckHardware.NewCheckLevelEventArgs e)
        {
            switch (e.Status)
            {
                case BeamOnCL.CheckHardware.CheckStatus.csHardware:
                    lblCheckLevel.Text = "Checking hardware ...";
                    checkProgress.Value = 25;
                    break;
                case BeamOnCL.CheckHardware.CheckStatus.csHead:
                    lblCheckLevel.Text = "Checking head ...";
                    checkProgress.Value = 50;
                    break;
                case BeamOnCL.CheckHardware.CheckStatus.csTypeHead:
                    lblCheckLevel.Text = "Checking head type ...";
                    checkProgress.Value = 75;
                    break;
                case BeamOnCL.CheckHardware.CheckStatus.csOk:
                    lblCheckLevel.Text = "Complete hardware test ...";
                    checkProgress.Value = 100;
                    timerSplash.Enabled = true;
                    break;
            }

            Application.DoEvents();
        }

        void beamOnCheck_OnGetCheckError(object sender, BeamOnCL.CheckHardware.NewCheckLevelEventArgs e)
        {
            switch (e.Status)
            {
                case BeamOnCL.CheckHardware.CheckStatus.csHardware:
                    //26.06.12
                    this.Hide();
                    CustomMessageBox.Show("Software cannot find camera. " +
                                    "Please check connection between camera and computer USB port.",
                                    "Hardware Error #1:",
                                     MessageBoxButtons.OK,
                                     MessageBoxIcon.Stop);
                    break;
                case BeamOnCL.CheckHardware.CheckStatus.csHead:
                    //26.06.12
                    this.Hide();
                    CustomMessageBox.Show("Software cannot find detector head. " +
                                    "Please check connection between camera and detector head.",
                                    "Hardware Error #2:",
                                     MessageBoxButtons.OK,
                                     MessageBoxIcon.Stop);
                    break;
            }

            timerSplash.Enabled = true;
            this.DialogResult = DialogResult.Cancel;
        }

        private void FormWellcome_Load(object sender, EventArgs e)
        {
            timerSplash.Enabled = true;
        }
    }
}
