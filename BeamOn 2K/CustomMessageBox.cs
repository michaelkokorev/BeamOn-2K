using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace BeamOn_2K
{
    class CustomMessageBox
    {
        public static DialogResult Show(string Text, string Title, MessageBoxButtons Buttons, MessageBoxIcon Icon)
        {
#if SA
            if (m_sysData.iMonitorSA != -1)
            {
                MessageForm message = new MessageForm(Text, Title, Buttons, Icon);
                return (message.ShowDialog(m_sysData.sa_Data.m_FormViewSA_Start));
            }
            else
                return (MessageBox.Show(Text, Title, Buttons, Icon));
#else
            return (MessageBox.Show(Text, Title, Buttons, Icon));
#endif
        }

        public static DialogResult Show(string Text, string Title, MessageBoxButtons Buttons, MessageBoxIcon Icon, Form frm = null)
        {
#if SA
            if (MessageForm.GetMonitorSA() != -1)
            {
                MessageForm message = new MessageForm(Text, Title, Buttons, Icon);
                return (message.ShowDialog(frm));
            }
            else
                return (MessageBox.Show(Text, Title, Buttons, Icon));
#else
            return (MessageBox.Show(Text, Title, Buttons, Icon));
#endif
        }

        public static DialogResult Show(string Text)
        {
#if SA
            if (m_sysData.iMonitorSA != -1)
            {
                MessageForm message = new MessageForm(Text, "", MessageBoxButtons.OK, MessageBoxIcon.None);
                return (message.ShowDialog(m_sysData.sa_Data.m_FormViewSA_Start));
            }
            else
                return (MessageBox.Show(Text));
#else
            return (MessageBox.Show(Text));
#endif
        }
    }
}
