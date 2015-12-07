using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Timers;

namespace BeamOn_2K
{
    public partial class FormErrorMessage : Form
    {
        private MyLabel lblErrorStatus = null;

        public FormErrorMessage()
        {
            InitializeComponent();

            this.BackColor = SystemColors.Control;
            this.TransparencyKey = BackColor;
            lblErrorStatus = new MyLabel();
            // 
            // tslblErrorStatus
            // 
            lblErrorStatus.TextAlign = ContentAlignment.MiddleCenter;
            lblErrorStatus.AutoSize = false;
            lblErrorStatus.BackColor = SystemColors.AppWorkspace;
            lblErrorStatus.ErrorMessage = ErrorStatus.BA_OK;
            lblErrorStatus.Name = "lblErrorStatus";
            lblErrorStatus.Dock = DockStyle.Fill;
            lblErrorStatus.Text = " ";

            this.Controls.Add(this.lblErrorStatus);
        }

        public Boolean DemoVersion
        {
            get { return lblErrorStatus.DemoVersion; }
            set { lblErrorStatus.DemoVersion = value; }
        }

        ///
        /// Summary:
        ///     Gets or sets the number error message.
        ///
        /// Returns:
        ///     A Current error message number. 
        public ErrorStatus ErrorMessage
        {
            get { return lblErrorStatus.ErrorMessage; }

            set
            {
                lblErrorStatus.ErrorMessage = value;
                this.Invalidate();
            }
        }

        ///
        /// Summary:
        ///     Enable or Disable the beep error message.
        ///
        /// Returns:
        ///     A Current error message beep state. 
        public Boolean ErrorMessageBeep
        {
            get { return lblErrorStatus.ErrorMessageBeep; }

            set
            {
                lblErrorStatus.ErrorMessageBeep = value;
                this.Invalidate();
            }
        }

        ///
        /// Summary:
        ///     Gets or sets the port name.
        ///
        /// Returns:
        ///     A Current type port name. 
        public String PortName
        {
            get { return lblErrorStatus.PortName; }
            set { lblErrorStatus.PortName = value; }
        }

        ///
        /// Summary:
        ///     Gets or sets the number system message.
        ///
        /// Returns:
        ///     A Current system message number. 
        public UInt16 SystemMessage
        {
            get { return lblErrorStatus.SystemMessage; }

            set
            {
                lblErrorStatus.SystemMessage = value;
                this.Invalidate();
            }
        }

        ///
        /// Summary:
        ///     Gets or sets type of log file.
        ///
        /// Returns:
        ///     A Current log type file. 
        public Byte LogTypeMessage
        {
            get { return lblErrorStatus.LogTypeMessage; }

            set { lblErrorStatus.LogTypeMessage = value; }
        }
    }

    public class MyLabel : System.Windows.Forms.Label
    {
        [System.Runtime.InteropServices.DllImport("Kernel32.dll")]
        static extern bool Beep(int dwFreq, int dwDuration);

        private delegate void AsyncVisible();
        private delegate void AsyncText();
        private delegate void AddItemAsyncDelegate();

        Boolean m_bErrorMessageBeep = false;
        System.Timers.Timer m_tFlashing = null;
        ErrorStatus m_esErrorMessage = ErrorStatus.BA_OK;
        Byte m_ltMessge = 0;
        UInt16 m_esSystemMessage = 0;
        String m_strText = "";
        Boolean m_bValue = false;
        String m_strPortName = "";

        Boolean m_bDemo = false;

        String[] m_strErrorMessage = new String[] {
                                                        "Ok",
                                                    };

        String[] m_strStatusMessage = new String[] {
                                                        "",
                                                        "Snapshot ",
                                                        "Slave Mode ",
                                                        "Data Link ",
                                                        "Log "
                                                   };

        private void CreateTimer()
        {
            m_tFlashing = new System.Timers.Timer();
            m_tFlashing.Elapsed += new ElapsedEventHandler(m_tFlashing_Elapsed);
            m_tFlashing.Interval = 200;
        }

        // Summary:
        //     Initializes a new instance of the MyToolStripLabel class.
        public MyLabel()
            : base()
        {
            CreateTimer();
        }

        //
        // Summary:
        //     Enable or Disable the beep error message.
        //
        // Returns:
        //     A Current error message beep state. 
        public Boolean ErrorMessageBeep
        {
            get { return m_bErrorMessageBeep; }

            set { m_bErrorMessageBeep = value; }
        }

        public Boolean DemoVersion
        {
            get { return m_bDemo; }
            set { m_bDemo = value; }
        }

        protected String GetTextStatusMessage()
        {
            String strText = "";

            if (m_esSystemMessage > 0)
            {
                if ((m_esSystemMessage & (UInt16)(SystemStatus.M_SS_DATALINK)) > 0)
                    strText = "Data Link ";

                if ((m_esSystemMessage & (UInt16)(SystemStatus.M_SS_LOG)) > 0)
                {
                    if (strText != "") strText += "&& ";

                    if (m_ltMessge == (Byte)0)
                        strText += "Log ";
                    else if (m_ltMessge == (Byte)1)
                        strText += "Excel ";
                    else
                        strText += "HTML ";
                }

                if ((m_esSystemMessage & (UInt16)(SystemStatus.M_SS_SLAVEMODE)) > 0)
                {
                    if (strText != "") strText += "&& ";
                    strText += "Slave Mode ";
                    if (m_strPortName != "") strText += "(" + m_strPortName + ") ";
                }

                if ((m_esSystemMessage & (UInt16)(SystemStatus.M_SS_CLIENT)) > 0)
                {
                    if (strText != "") strText += "&& ";
                    strText += "Client Mode ";
                    if (m_strPortName != "") strText += "(" + m_strPortName + ") ";
                }

                if ((m_esSystemMessage & (UInt16)(SystemStatus.M_SS_SNAPSHOT)) > 0)
                {
                    if (strText != "") strText += "&& ";
                    strText += "Snapshot ";
                }

                if ((m_esSystemMessage & (UInt16)(SystemStatus.M_SS_SERVERMODE)) > 0)
                {
                    if (strText != "") strText += "&& ";
                    strText += "Server Mode ";
                    if (m_strPortName != "") strText += "(" + m_strPortName + ") ";
                }

                if ((m_esSystemMessage & (UInt16)(SystemStatus.M_SS_STEP)) > 0)
                {
                    if (strText != "") strText += "&& ";
                    strText += "Step Mode ";
                }

                if ((m_esSystemMessage & (UInt16)(SystemStatus.M_SS_SAVEDATA)) > 0)
                {
                    if (strText != "") strText += "&& ";
                    strText += "Saving Data ";
                }

                strText += "in progress";
            }

            return strText;
        }

        ///
        /// Summary:
        ///     Gets or sets the type of log file.
        ///
        /// Returns:
        ///     A Current type log file. 
        public Byte LogTypeMessage
        {
            get { return m_ltMessge; }

            set { m_ltMessge = value; }
        }

        ///
        /// Summary:
        ///     Gets or sets the number system message.
        ///
        /// Returns:
        ///     A Current system message number. 
        public UInt16 SystemMessage
        {
            get { return m_esSystemMessage; }

            set
            {
                if (m_esSystemMessage != value)
                {
                    m_esSystemMessage = value;

                    if (m_esSystemMessage > 0)
                    {
                        m_strText = GetTextStatusMessage();

                        //this.Text = m_strStatusMessage[(UInt16)m_esSystemMessage];
                        //strText = m_strStatusMessage[(UInt16)m_esSystemMessage] + "in progress";

                        this.m_tFlashing.Stop();
                        m_bValue = true;

                        AddItemAsyncDelegate asyncDel = new AddItemAsyncDelegate(UpdateVisibleAsync);
                        asyncDel.BeginInvoke(null, null);
                    }
                    else if (m_esErrorMessage != ErrorStatus.BA_OK)
                    {
                        m_strText = m_strErrorMessage[(UInt16)m_esErrorMessage];

                        this.m_tFlashing.Start();
                    }
                    else
                    {
                        m_strText = (m_bDemo ? "Demo Version" : "");

                        m_tFlashing.Stop();

                        m_bValue = true;

                        AddItemAsyncDelegate asyncDel = new AddItemAsyncDelegate(UpdateVisibleAsync);
                        asyncDel.BeginInvoke(null, null);
                    }

                    AddItemAsyncDelegate asyncUpdate = new AddItemAsyncDelegate(UpdateAsync);
                    asyncUpdate.BeginInvoke(null, null);
                }
                else if (value == 0)
                {
                    if (m_esErrorMessage != ErrorStatus.BA_OK)
                        m_strText = m_strErrorMessage[(UInt16)m_esErrorMessage];
                    else
                        m_strText = (m_bDemo ? "Demo Version" : "");

                    AddItemAsyncDelegate asyncDel = new AddItemAsyncDelegate(UpdateAsync);
                    asyncDel.BeginInvoke(null, null);
                }
            }
        }

        ///
        /// Summary:
        ///     Gets or sets the number error message.
        ///
        /// Returns:
        ///     A Current error message number. 
        public ErrorStatus ErrorMessage
        {
            get { return m_esErrorMessage; }

            set
            {
                if (m_esErrorMessage != value)
                {
                    m_esErrorMessage = value;

                    if (m_esErrorMessage != ErrorStatus.BA_OK)
                    {
                        //this.Text = m_strErrorMessage[(UInt16)m_esErrorMessage];
                        m_strText = m_strErrorMessage[(UInt16)m_esErrorMessage];

                        this.m_tFlashing.Start();
                    }
                    else if (m_esSystemMessage > 0)
                    {
                        //this.Text = m_strStatusMessage[(UInt16)m_esSystemMessage];
                        //strText = m_strStatusMessage[(UInt16)m_esSystemMessage];

                        m_strText = GetTextStatusMessage();

                        this.m_tFlashing.Stop();

                        m_bValue = true;

                        AddItemAsyncDelegate asyncDel = new AddItemAsyncDelegate(UpdateVisibleAsync);
                        asyncDel.BeginInvoke(null, null);
                    }
                    else
                    {
                        m_strText = (m_bDemo ? "Demo Version" : "");

                        this.m_tFlashing.Stop();

                        m_bValue = true;

                        AddItemAsyncDelegate asyncDel = new AddItemAsyncDelegate(UpdateVisibleAsync);
                        asyncDel.BeginInvoke(null, null);
                    }

                    AddItemAsyncDelegate asyncUpdate = new AddItemAsyncDelegate(UpdateAsync);
                    asyncUpdate.BeginInvoke(null, null);
                }
                else if (value == ErrorStatus.BA_OK)
                {
                    if (m_esSystemMessage > 0)
                        m_strText = GetTextStatusMessage();
                    else
                        m_strText = (m_bDemo ? "Demo Version" : "");

                    AddItemAsyncDelegate asyncDel = new AddItemAsyncDelegate(UpdateAsync);
                    asyncDel.BeginInvoke(null, null);
                }
            }
        }

        private void UpdateAsync()
        {
            try
            {
                // Асинхронный вызов метода UpdateUI
                MethodInvoker methodinvoker = new MethodInvoker(LabelText);
                Parent.BeginInvoke(methodinvoker);// this.BeginInvoke(methodinvoker);
            }
            catch
            {
            }
        }

        private void LabelText()
        {
            this.Text = m_strText;
        }

        //
        // Summary:
        //     Raises the System.Windows.Forms.ToolStripItem.Paint event.
        //
        // Parameters:
        //   e:
        //     A System.Windows.Forms.PaintEventArgs that contains the event data.
        protected override void OnPaint(System.Windows.Forms.PaintEventArgs e)
        {
            Rectangle rc = e.ClipRectangle;
            rc.Size = new Size(rc.Width - 1, rc.Height - 1);

            if (m_esErrorMessage != ErrorStatus.BA_OK)
            {
                base.ForeColor = Color.White;
                e.Graphics.FillRectangle(new SolidBrush(Color.Black), rc);
            }
            else if (m_esSystemMessage > 0)
            {
                base.ForeColor = Color.Yellow;
                e.Graphics.FillRectangle(new SolidBrush(Color.Red), rc);
            }
            else if (m_bDemo == true)
            {
                base.ForeColor = Color.Red;
                e.Graphics.FillRectangle(new SolidBrush(BackColor), rc);
            }
            else
            {
                base.ForeColor = Color.Black;
                e.Graphics.FillRectangle(new SolidBrush(BackColor), rc); //SystemColors.AppWorkspace
            }

            base.OnPaint(e);
        }

        void m_tFlashing_Elapsed(object sender, ElapsedEventArgs e)
        {
            m_bValue = !this.Visible;

            AddItemAsyncDelegate asyncDel = new AddItemAsyncDelegate(UpdateVisibleAsync);
            asyncDel.BeginInvoke(null, null);

            if ((m_esErrorMessage != ErrorStatus.BA_OK) && (m_bErrorMessageBeep == true))
            {
                Beep(600, 150);
            }
        }

        private void UpdateVisibleAsync()
        {
            try
            {
                // Асинхронный вызов метода ChangeVisible
                MethodInvoker methodinvoker = new MethodInvoker(ChangeVisible);
                Parent.BeginInvoke(methodinvoker);
                //this.BeginInvoke(methodinvoker);
            }
            catch
            {
            }
        }

        ///
        /// Summary:
        ///     Gets or sets the port name.
        ///
        /// Returns:
        ///     A Current type port name. 
        public String PortName
        {
            get { return m_strPortName; }
            set { m_strPortName = value; }
        }

        private void ChangeVisible()
        {
            this.Visible = m_bValue;
        }
    }
}
