using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace BeamOn_2K
{
    static class Program
    {
        static DialogResult m_dr;
        static Boolean m_bSimulation = false;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            FormWellcome formWellcome = new FormWellcome();
            m_dr = formWellcome.ShowDialog();

            if (m_dr == DialogResult.Cancel)
            {
                m_bSimulation = (CustomMessageBox.Show("Would you like start this program in viewer/client mode?",
                                                        "Hardware Error:",
                                                        MessageBoxButtons.YesNo,
                                                        MessageBoxIcon.Exclamation) == DialogResult.Yes);
            }

            if ((((m_dr == DialogResult.OK) || (m_dr == DialogResult.Yes))) || (m_bSimulation == true))
            {
                String strArg = (args.Length > 0) ? args[0] : null;

                FormMain formMain = new FormMain(strArg);
                Application.Run(formMain);
            }
        }
    }
}
