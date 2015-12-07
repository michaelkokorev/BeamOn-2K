using System;
using System.Timers;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms.Design;
using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;

namespace BeamOn_2K
{
    public class MyToolStripLabel : System.Windows.Forms.ToolStripLabel
    {
        [System.Runtime.InteropServices.DllImport("Kernel32.dll")]
        static extern bool Beep(int dwFreq, int dwDuration);

        private delegate void AsyncVisible();
        private delegate void AsyncText();
        private delegate void AddItemAsyncDelegate();

        Boolean m_bErrorMessageBeep = false;
        System.Timers.Timer m_tFlashing = null;
        ErrorStatus m_esErrorMessage = ErrorStatus.BA_OK;
        UInt16 m_esSystemMessage = 0;
        Boolean m_bDemo = false;
        Byte m_ltMessge = 0;
        String m_strText = "";
        String m_strPortName = "";
        Boolean m_bValue = false;

        String[] m_strErrorMessage = new String[] {
                                                        "Ok",
                                                    };

        String[] m_strStatusMessage = new String[] {
                                                        "",
                                                        "Snapshot ",
                                                        "Slave Mode ",
                                                        "Data Link ",
                                                        "Log ",
                                                        "Server Mode ",
                                                        "Step Mode "                                                        
                                                    };

        private void CreateTimer()
        {
            m_tFlashing = new System.Timers.Timer();
            m_tFlashing.Elapsed += new ElapsedEventHandler(m_tFlashing_Elapsed);
            m_tFlashing.Interval = 200;
        }

        /// Summary:
        ///     Initializes a new instance of the MyToolStripLabel class.
        public MyToolStripLabel()
            : base()
        {
            CreateTimer();
        }
        //
        // Summary:
        //     Initializes a new instance of the MyToolStripLabel class,
        //     specifying the image to display.
        //
        // Parameters:
        //   image:
        //     The System.Drawing.Image to display on the MyToolStripLabel.
        public MyToolStripLabel(Image image)
            : base(image)
        {
            CreateTimer();
        }
        //
        // Summary:
        //     Initializes a new instance of the MyToolStripLabel class,
        //     specifying the text to display.
        //
        // Parameters:
        //   text:
        //     The text to display on the MyToolStripLabel.
        public MyToolStripLabel(string text)
            : base(text)
        {
            CreateTimer();
        }
        //
        // Summary:
        //     Initializes a new instance of the MyToolStripLabel class,
        //     specifying the text and image to display.
        //
        // Parameters:
        //   text:
        //     The text to display on the MyToolStripLabel.
        //
        //   image:
        //     The System.Drawing.Image to display on the MyToolStripLabel.
        public MyToolStripLabel(string text, Image image)
            : base(text, image)
        {
            CreateTimer();
        }
        //
        // Summary:
        //     Initializes a new instance of the MyToolStripLabel class,
        //     specifying the text and image to display and whether the MyToolStripLabel
        //     acts as a link.
        //
        // Parameters:
        //   text:
        //     The text to display on the MyToolStripLabel.
        //
        //   image:
        //     The System.Drawing.Image to display on the MyToolStripLabel.
        //
        //   isLink:
        //     true if the MyToolStripLabel acts as a link; otherwise,
        //     false.
        public MyToolStripLabel(string text, Image image, bool isLink)
            : base(text, image, isLink)
        {
            CreateTimer();
        }
        //
        // Summary:
        //     Initializes a new instance of the MyToolStripLabel class,
        //     specifying the text and image to display, whether the MyToolStripLabel
        //     acts as a link, and providing a System.Windows.Forms.ToolStripItem.Click
        //     event handler.
        //
        // Parameters:
        //   text:
        //     The text to display on the MyToolStripLabel.
        //
        //   image:
        //     The System.Drawing.Image to display on the MyToolStripLabel.
        //
        //   isLink:
        //     true if the MyToolStripLabel acts as a link; otherwise,
        //     false.
        //
        //   onClick:
        //     A System.Windows.Forms.ToolStripItem.Click event handler.
        public MyToolStripLabel(string text, Image image, bool isLink, EventHandler onClick)
            : base(text, image, isLink, onClick)
        {
            CreateTimer();
        }
        //
        // Summary:
        //     Initializes a new instance of the MyToolStripLabel class,
        //     specifying the text and image to display, whether the MyToolStripLabel
        //     acts as a link, and providing a System.Windows.Forms.ToolStripItem.Click
        //     event handler and name for the MyToolStripLabel.
        //
        // Parameters:
        //   text:
        //     The text to display on the MyToolStripLabel.
        //
        //   image:
        //     The System.Drawing.Image to display on the MyToolStripLabel.
        //
        //   isLink:
        //     true if the MyToolStripLabel acts as a link; otherwise,
        //     false.
        //
        //   onClick:
        //     A MyToolStripLabel event handler.
        //
        //   name:
        //     The name of the MyToolStripLabel.
        public MyToolStripLabel(string text, Image image, bool isLink, EventHandler onClick, string name)
            : base(text, image, isLink, onClick, name)
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
                if ((m_esSystemMessage & (UInt16)(SystemStatus.M_SS_DATALINK)) > 0) strText = "Data Link ";

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
        ///     Gets or sets the port name.
        ///
        /// Returns:
        ///     A Current type port name. 
        public String PortName
        {
            get { return m_strPortName; }
            set { m_strPortName = value; }
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

        //
        // Summary:
        //     Gets or sets the number system message.
        //
        // Returns:
        //     A Current system message number. 
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

                        this.m_tFlashing.Stop();
                        this.Visible = true;
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
                        this.Visible = true;
                    }

                    AddItemAsyncDelegate asyncDel = new AddItemAsyncDelegate(UpdateAsync);
                    asyncDel.BeginInvoke(null, null);
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

        //
        // Summary:
        //     Gets or sets the number error message.
        //
        // Returns:
        //     A Current error message number. 
        public ErrorStatus ErrorMessage
        {
            get
            {
                return m_esErrorMessage;
            }

            set
            {
                if (m_esErrorMessage != value)
                {
                    m_esErrorMessage = value;

                    if (m_esErrorMessage != ErrorStatus.BA_OK)
                    {
                        m_strText = m_strErrorMessage[(UInt16)m_esErrorMessage];

                        this.m_tFlashing.Start();
                    }
                    else if (m_esSystemMessage > 0)
                    {
                        m_strText = GetTextStatusMessage();

                        this.m_tFlashing.Stop();
                        this.Visible = true;
                    }
                    else
                    {
                        m_strText = (m_bDemo ? "Demo Version" : "");

                        this.m_tFlashing.Stop();
                        this.Visible = true;
                    }

                    AddItemAsyncDelegate asyncDel = new AddItemAsyncDelegate(UpdateAsync);
                    asyncDel.BeginInvoke(null, null);
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
                MethodInvoker methodinvoker = new MethodInvoker(LabelText);
                Parent.BeginInvoke(methodinvoker);
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
                e.Graphics.FillRectangle(new SolidBrush(BackColor), rc);
            }
            else if (m_esSystemMessage > 0)
            {
                base.ForeColor = Color.Yellow;
                e.Graphics.FillRectangle(new SolidBrush(Color.Red), rc);
            }
            else if (m_bDemo == true)
            {
                base.ForeColor = Color.Red;
            }
            else
            {
                base.ForeColor = Color.Black;
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
            MethodInvoker methodinvoker = new MethodInvoker(ChangeVisible);
            try
            {
                Parent.BeginInvoke(methodinvoker);// this.BeginInvoke(methodinvoker);
            }
            catch
            {
            }
        }

        private void ChangeVisible()
        {
            this.Visible = m_bValue;
        }
    }
}
