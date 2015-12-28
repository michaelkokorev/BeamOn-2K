using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.Diagnostics;
using System.Threading;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Xml;

namespace BeamOn_2K
{
    public enum ErrorStatus
    {
        BA_OK = 0,              // Normal termination
    };

    public enum SystemStatus
    {
        M_SS_EMPTY = 0,
        M_SS_SNAPSHOT = 1,
        M_SS_SLAVEMODE = 2,
        M_SS_DATALINK = 4,
        M_SS_LOG = 8,
        M_SS_CLIENT = 16,
        M_SS_SERVERMODE = 32,
        M_SS_STEP = 64,
        M_SS_SAVEDATA = 128,
    };

    public partial class FormMain : Form
    {
        private delegate void AsyncChangeAngle(Double dAngle);
        private delegate void AsyncChangePalette(ColorPalette cpValue, PixelFormat pixelFormat, System.Drawing.Color[] color);
        private delegate void AsyncTimeStamp(Int64 iValue);
        private delegate void AddItemAsyncDelegate();
        private delegate void CloseLogAsyncDelegate();

        private delegate void SetEndLogCallback();

        Object m_lLockBMP = new Object();
        Bitmap m_bmp = null;
        System.Drawing.Color[] m_colorArray = null;

        Stopwatch m_sw = null;

        private BeamOnCL.BeamOnCL bm = null;

        Pen m_PenSensor = new Pen(System.Drawing.Color.Yellow, 1.2f);
        Pen m_PenCentroid = new Pen(System.Drawing.Color.Blue, 0.2f);
        Pen m_PenEllipse = new Pen(System.Drawing.Color.Green, 2f);
        Pen m_PenGaussian = new Pen(System.Drawing.Color.Red, 0.2f);
        Font myFont = new Font("Arial", 10, FontStyle.Bold);
        Brush PaletteBrush = new SolidBrush(System.Drawing.Color.DarkGray);
        StringFormat m_strfrm = new StringFormat();
        Pen m_PenGrid = new Pen(System.Drawing.Color.DarkGray, 0.1f);

        public enum DrawOrientation { doHorizontal, doVertical };

        public const UInt16 DELTA = 1;
        private int iLevelSelected = -1;
        private int iLevelCurrent = -1;

        BeamOnCL.Profile m_profileHorizontal = null;
        BeamOnCL.Profile m_profileVertical = null;
        Point[] plArea = null;

        Image printImage = null;
        Font printFont = new Font("Arial", 10);

        public enum ePrinterType { ePrintText, ePrintImage };
        public struct PrinterData
        {
            public ePrinterType printerType;
            public int iPagePrint;
            public String strFileName;
        }

        PrinterData m_printerData;
        StreamReader streamToPrint = null;

        SystemData m_sysData = SystemData.MyInstance;
        FileLogData m_fldLog = null;

        Byte m_SystemMessage = 0;
        Form3DProjection m_frm3D = null;

        public FormMain(String strArgument)
        {
            InitializeComponent();

            if (strArgument != null)
            {
                if (strArgument.Length >= 6)
                {
                    m_sysData.Calibr = ((strArgument.Substring(0, 6)).ToUpper() == "CALIBR");
                }
                else if (strArgument.Length >= 5)
                {
                    m_sysData.Debug = ((strArgument.Substring(0, 5)).ToUpper() == "DEBUG");
                }
            }

            printDialog.PrinterSettings.FromPage = 1;
            printDialog.PrinterSettings.ToPage = printDialog.PrinterSettings.MaximumPage;

            bm = new BeamOnCL.BeamOnCL();
            bm.OnImageReceved += new BeamOnCL.BeamOnCL.ImageReceved(bm_OnImageReceved);
        }

        void bm_OnImageReceved(object sender, BeamOnCL.MeasureCamera.NewDataRecevedEventArgs e)
        {
            //if (InvokeRequired)
            //{
            //    // If called from a different thread, we must use the Invoke method to marshal the call to the proper GUI thread.
            //    // The grab result will be disposed after the event call. Clone the event arguments for marshaling to the GUI thread. 
            //    BeginInvoke(new EventHandler<BeamOnCL.MeasureCamera.NewDataRecevedEventArgs>(bm_OnImageReceved), sender, e.Clone());
            //    return;
            //}
            //BeamOnCL.MeasureCamera.NewDataRecevedEventArgs e = ee.Clone();

            BitmapData bmpData = null;

            if (m_bmp != null)
            {
                lock (m_lLockBMP)
                {
                    try
                    {
                        bmpData = m_bmp.LockBits(
                                                    new Rectangle(new Point(0, 0), e.Snapshot.ImageRectangle.Size),
                                                    System.Drawing.Imaging.ImageLockMode.WriteOnly,
                                                    m_bmp.PixelFormat
                                                 );

                        e.Snapshot.SetImageDataArray(bmpData.Scan0, m_colorArray);

                        m_bmp.UnlockBits(bmpData);

                        if (m_frm3D != null)
                        {
                            m_frm3D.ImageData = e.Snapshot;
                        }

                        if (m_sysData.Measure == true)
                        {
                            bm.GetMeasure(e.Snapshot);

                            plArea = bm.CreateEllipse();

                            m_sysData.positionData.RealPosition = bm.Centroid;
                            m_sysData.positionData.Ellipse.Major = bm.MajorRadius;
                            m_sysData.positionData.Ellipse.Minor = bm.MinorRadius;
                            m_sysData.positionData.Ellipse.Orientation = bm.Angle;

                            m_profileHorizontal = new BeamOnCL.Profile(bm.profileHorizontal);
                            m_profileVertical = new BeamOnCL.Profile(bm.profileVertical);

                            for (int i = 0; i < m_sysData.ClipLevels.NumberLevels; i++)
                            {
                                m_sysData.HorizontalProfile.SetWidthProfile(i, m_profileHorizontal.GetWidth(Decimal.ToSingle(m_sysData.ClipLevels.Level(i))));
                                m_sysData.VerticalProfile.SetWidthProfile(i, m_profileVertical.GetWidth(Decimal.ToSingle(m_sysData.ClipLevels.Level(i))));
                            }

                            if (m_sysData.ViewGaussian == true)
                            {
                                for (int i = 0; i < m_sysData.ClipLevels.NumberLevels; i++)
                                {
                                    m_sysData.HorizontalProfile.SetWidthGauss(i, m_profileHorizontal.GaussianData.GetWidth(Decimal.ToSingle(m_sysData.ClipLevels.Level(i))));
                                    m_sysData.VerticalProfile.SetWidthGauss(i, m_profileVertical.GaussianData.GetWidth(Decimal.ToSingle(m_sysData.ClipLevels.Level(i))));
                                }

                                m_sysData.HorizontalProfile.m_fCorrelation = m_profileHorizontal.GaussianData.Correlation;
                                m_sysData.VerticalProfile.m_fCorrelation = m_profileVertical.GaussianData.Correlation;
                            }

                            if ((m_fldLog != null) && (m_fldLog.IsOpen() == true))
                            {
                                m_sysData.logData.LastMeasureTime = e.Timestamp;
                                m_fldLog.AddData();
                            }
                        }
                    }
                    catch { }
                    finally
                    {
                        // Dispose the DataReceved result if needed for returning it to the grab loop.
                        e.DisposeDataRecevedIfClone();
                    }
                }
            }

            //Thread.Sleep(0);

            m_sw.Stop();

            AsyncTimeStamp asyncTimeStamp = new AsyncTimeStamp(UpdateVisibleAsync);
            asyncTimeStamp.BeginInvoke(m_sw.ElapsedMilliseconds, null, null);

            System.Threading.Thread.Sleep(100);

            m_sw = Stopwatch.StartNew();
            m_sw.Start();
            /*
                        this.Invalidate();
                        dataSplitContainer.Panel2.Invalidate();
                        //            pictureBoxImage.Invalidate();
                        pictureBoxData.Invalidate();
            */
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void propertyBoxToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            dataSplitContainer.Panel1Collapsed = !propertyBoxToolStripMenuItem.Checked;
        }

        private void dataPanelToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            mainSplitContainer.Panel2Collapsed = !dataPanelToolStripMenuItem.Checked;
        }

        private void toolBarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            m_sysData.applicationData.bViewToolbar = toolBarToolStripMenuItem.Checked;

            ViewToolStrip(m_sysData.applicationData.bViewToolbar);
        }

        private void ViewToolStrip(Boolean bValue)
        {
            mainToolStrip.Visible = bValue;
            toolBarToolStripMenuItem.Checked = bValue;

            if (m_sysData.applicationData.m_FormErrorMessage != null)
            {
                if (bValue == true)
                    m_sysData.applicationData.m_FormErrorMessage.Hide();
                else
                    m_sysData.applicationData.m_FormErrorMessage.Show();
            }
        }

        private void statusBarToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            mainStatusStrip.Visible = !statusBarToolStripMenuItem.Checked;
        }

        private void pictureBoxImage_Paint(object sender, PaintEventArgs e)
        {
            Graphics grfx = e.Graphics;

            if (m_bmp != null)
            {
                try
                {
                    lock (m_lLockBMP) grfx.DrawImage(m_bmp, 0, 0, m_bmp.Width, m_bmp.Height);
                }
                catch
                {
                }
            }
        }

        void pictureBoxData_MouseUp(object sender, MouseEventArgs e)
        {
            iLevelSelected = -1;
            iLevelCurrent = -1;
        }

        void pictureBoxData_MouseMove(object sender, MouseEventArgs e)
        {
            int iHeight = Math.Min(pictureBoxData.Height, imageSplitContainer.Panel2.Height);
            int iHeightProfile = (int)(iHeight / 3f);

            int iShiftX = imageSplitContainer.Panel2.AutoScrollPosition.X;
            int iShiftY = 0;

            if (imageSplitContainer.Panel2.Height < pictureBoxData.Height)
            {
                iShiftY += imageSplitContainer.Panel2.AutoScrollPosition.Y;
                if (imageSplitContainer.Panel2.Width < pictureBoxData.Width) iShiftY += 20;
            }

            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                if (iLevelSelected == -1)
                    bm.CrossPosition = new Point(e.X, e.Y);
                else
                {
                    if (this.Cursor == Cursors.SizeNS)
                        SetClipLevel(iLevelSelected, (float)((iHeight - e.Y - iShiftY) * 100f / iHeightProfile));
                    else if (this.Cursor == Cursors.SizeWE)
                        SetClipLevel(iLevelSelected, (float)((e.X + iShiftX) * 100f / iHeightProfile));
                }

                pictureBoxData.Invalidate();
            }
            else
            {
                int iLevelHorizontal = 0, iLevelVertical = 0;

                iLevelCurrent = -1;

                for (int i = 0; i < m_sysData.ClipLevels.NumberLevels; i++)
                {
                    iLevelHorizontal = iHeight - (int)(iHeightProfile * Decimal.ToSingle(m_sysData.ClipLevels.Level(i)) / 100f) - iShiftY;
                    iLevelVertical = (int)(iHeightProfile * Decimal.ToSingle(m_sysData.ClipLevels.Level(i)) / 100f - iShiftX);

                    if ((Math.Abs(e.Y - iLevelHorizontal) < DELTA) || (Math.Abs(e.X - iLevelVertical) < DELTA))
                    {
                        iLevelCurrent = i;
                        break;
                    }
                }

                this.Cursor = (iLevelCurrent != -1) ? ((Math.Abs(e.Y - iLevelHorizontal) < DELTA) ? Cursors.SizeNS : Cursors.SizeWE) : Cursors.Default;
            }
        }

        private void DefaultSettings()
        {
            m_sysData.powerData.strSAMName = "SAM.dat";
            m_sysData.powerData.strSAMPath = m_sysData.applicationData.m_strMyCurrentDir;
            m_sysData.powerData.PowerUnits = 0;

            m_sysData.applicationData.bSaveExit = true;
            m_sysData.applicationData.bViewStatusBar = true;
            m_sysData.applicationData.bViewToolbar = true;

            //Profile
            m_sysData.ClipLevels.SetLevel(0, 13.5M);
            m_sysData.ClipLevels.SetLevel(1, 50M);
            m_sysData.ClipLevels.SetLevel(2, 80M);
        }

        private void SetClipLevel(int iIndex, float fValue)
        {
            float fTopLevel = (iIndex < (m_sysData.ClipLevels.NumberLevels - 1)) ? Decimal.ToSingle(m_sysData.ClipLevels.Level(iIndex + 1)) - 0.5f : 99.5f;
            float fBottomLevel = (iIndex > 0) ? Decimal.ToSingle(m_sysData.ClipLevels.Level(iIndex - 1)) + 0.5f : 0.5f;
            float fDelta = fTopLevel - fBottomLevel;

            m_sysData.ClipLevels.SetLevel(iIndex, Convert.ToDecimal(((fValue >= fBottomLevel) && (fValue <= fTopLevel)) ? fValue : (fValue < fBottomLevel) ? fBottomLevel : fTopLevel));
        }

        public void ChangeStatusLabel()
        {
            toolStripStatuslblClip.Text = "Clip Level: " + m_sysData.ClipLevels.Level(0) + "%";
        }

        void pictureBoxData_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                if (iLevelCurrent == -1)
                {
                    bm.CrossPosition = new Point(e.X, e.Y);
                }
                else
                {
                    iLevelSelected = iLevelCurrent;
                }

                pictureBoxData.Invalidate();
            }
            else if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                if (bm.ImageRectangle.Size == bm.MaxImageRectangle.Size)
                {
                    int iSizeX = (int)Math.Min(bm.MaxImageRectangle.Width, imageSplitContainer.Panel2.Width) - 8;
                    int iSizeY = (int)Math.Min(bm.MaxImageRectangle.Height, imageSplitContainer.Panel2.Height) - 8;

                    int iOffsetX = (int)bm.PixelCentroid.X - (int)(iSizeX / 2);
                    int iOffsetY = (int)bm.PixelCentroid.Y - (int)(iSizeY / 2);

                    bm.ImageRectangle = new Rectangle(iOffsetX, iOffsetY, iSizeX, iSizeY);
                }
                else
                {
                    bm.ImageRectangle = bm.MaxImageRectangle;
                }

                pictureBoxImage.Size = bm.ImageRectangle.Size;

                m_bmp = new Bitmap(pictureBoxImage.Width, pictureBoxImage.Height, picturePaletteImage.Format);

                if (picturePaletteImage.Format == PixelFormat.Format8bppIndexed) m_bmp.Palette = picturePaletteImage.Palette;
            }
        }

        void pictureBoxData_Paint(object sender, PaintEventArgs e)
        {
            Graphics grfx = e.Graphics;

            if (m_sysData.Measure == true)
            {
                Point OldPoint = new Point((int)bm.PixelCentroid.X - 20, (int)bm.PixelCentroid.Y - 20);
                Point NewPoint = new Point((int)bm.PixelCentroid.X + 20, (int)bm.PixelCentroid.Y + 20);

                grfx.DrawLine(m_PenCentroid, OldPoint, NewPoint);

                OldPoint = new Point((int)bm.PixelCentroid.X - 20, (int)bm.PixelCentroid.Y + 20);
                NewPoint = new Point((int)bm.PixelCentroid.X + 20, (int)bm.PixelCentroid.Y - 20);

                grfx.DrawLine(m_PenCentroid, OldPoint, NewPoint);

                OldPoint = new Point(bm.ImageRectangle.X, (int)(bm.MaxImageRectangle.Height / 2) - (int)bm.ImageRectangle.Y);
                NewPoint = new Point(bm.ImageRectangle.X + bm.ImageRectangle.Width, (int)(bm.MaxImageRectangle.Height / 2) - (int)(bm.ImageRectangle.Y));

                grfx.DrawLine(m_PenSensor, OldPoint, NewPoint);

                OldPoint = new Point((int)(bm.MaxImageRectangle.Width / 2) - bm.ImageRectangle.X, (int)bm.ImageRectangle.Y);
                NewPoint = new Point((int)(bm.MaxImageRectangle.Width / 2) - bm.ImageRectangle.X, (int)(bm.ImageRectangle.Y + bm.ImageRectangle.Height));

                grfx.DrawLine(m_PenSensor, OldPoint, NewPoint);

                if (plArea != null) grfx.DrawLines(m_PenEllipse, plArea);

                grfx.DrawRectangle(m_PenCentroid, bm.WorkingArea);

                if (bm.typeProfile == BeamOnCL.BeamOnCL.TypeProfile.tpLIne)
                {
                    grfx.DrawLine(m_PenGrid, bm.lineProfileHorizontalLeft, bm.lineProfileHorizontalRight);
                    grfx.DrawLine(m_PenGrid, bm.lineProfileVerticalLeft, bm.lineProfileVerticalRight);
                }

                DrawProfile(m_profileVertical, m_PenGrid, DrawOrientation.doVertical, grfx);
                DrawProfile(m_profileHorizontal, m_PenGrid, DrawOrientation.doHorizontal, grfx);

                int iHeight = Math.Min(pictureBoxData.Height, imageSplitContainer.Panel2.Height);

                int iShiftY = 0;
                if (imageSplitContainer.Panel2.Height < pictureBoxData.Height)
                {
                    iShiftY += imageSplitContainer.Panel2.AutoScrollPosition.Y;
                    if (imageSplitContainer.Panel2.Width < pictureBoxData.Width) iShiftY += 20;
                }

                int iShiftX = imageSplitContainer.Panel2.AutoScrollPosition.X;

                grfx.DrawString("Horizontal Profile", myFont, PaletteBrush, new PointF(20 - iShiftX, iHeight - 100 - iShiftY), m_strfrm);
                grfx.DrawString("ClipLevel: " + String.Format("{0:F1}", Decimal.ToSingle(m_sysData.ClipLevels.Level(2))) + "%" + " Width: " + m_sysData.HorizontalProfile.strWidth[2] + "(µm)", myFont, PaletteBrush, new PointF(20 - iShiftX, iHeight - 80 - iShiftY), m_strfrm);
                grfx.DrawString("ClipLevel: " + String.Format("{0:F1}", Decimal.ToSingle(m_sysData.ClipLevels.Level(1))) + "%" + " Width: " + m_sysData.HorizontalProfile.strWidth[1] + "(µm)", myFont, PaletteBrush, new PointF(20 - iShiftX, iHeight - 60 - iShiftY), m_strfrm);
                grfx.DrawString("ClipLevel: " + String.Format("{0:F1}", Decimal.ToSingle(m_sysData.ClipLevels.Level(0))) + "%" + " Width: " + m_sysData.HorizontalProfile.strWidth[0] + "(µm)", myFont, PaletteBrush, new PointF(20 - iShiftX, iHeight - 40 - iShiftY), m_strfrm);
                if (m_sysData.ViewGaussian == true) grfx.DrawString("Gaussian Correlation: " + String.Format("{0:F1}", m_sysData.HorizontalProfile.m_fCorrelation) + "%", myFont, PaletteBrush, new PointF(20 - iShiftX, iHeight - 20 - iShiftY), m_strfrm);

                grfx.DrawString("Vertical Profile", myFont, PaletteBrush, new PointF(20 - iShiftX, 20 - iShiftY), m_strfrm);
                grfx.DrawString("ClipLevel: " + String.Format("{0:F1}", Decimal.ToSingle(m_sysData.ClipLevels.Level(2))) + "%" + " Width: " + m_sysData.VerticalProfile.strWidth[2] + "(µm)", myFont, PaletteBrush, new PointF(20 - iShiftX, 40 - iShiftY), m_strfrm);
                grfx.DrawString("ClipLevel: " + String.Format("{0:F1}", Decimal.ToSingle(m_sysData.ClipLevels.Level(1))) + "%" + " Width: " + m_sysData.VerticalProfile.strWidth[1] + "(µm)", myFont, PaletteBrush, new PointF(20 - iShiftX, 60 - iShiftY), m_strfrm);
                grfx.DrawString("ClipLevel: " + String.Format("{0:F1}", Decimal.ToSingle(m_sysData.ClipLevels.Level(0))) + "%" + " Width: " + m_sysData.VerticalProfile.strWidth[0] + "(µm)", myFont, PaletteBrush, new PointF(20 - iShiftX, 80 - iShiftY), m_strfrm);
                if (m_sysData.ViewGaussian == true) grfx.DrawString("Gaussian Correlation: " + String.Format("{0:F1}", m_sysData.VerticalProfile.m_fCorrelation) + "%", myFont, PaletteBrush, new PointF(20 - iShiftX, 100 - iShiftY), m_strfrm);
            }
        }

        public void DrawProfile(BeamOnCL.Profile dataProfile, Pen pen, DrawOrientation drawOrientation, Graphics grfx)
        {
            Point OldPoint = new Point();
            Point NewPoint;

            Point OldPointGaussian = new Point();
            Point NewPointGaussian;

            int iHeight = Math.Min(pictureBoxData.Height, imageSplitContainer.Panel2.Height);
            int iHeightProfile = (int)(iHeight / 3f);

            if (dataProfile != null)
            {
                Double MaxProfile = (m_sysData.ScaleProfile == true) ? dataProfile.MaxProfile : ((bm.pixelFormat == PixelFormat.Format8bppIndexed) ? (UInt16)255 : (UInt16)4095);
                Double fCoeffAmpl = (MaxProfile > 0) ? iHeightProfile / MaxProfile : 0;
                Double fCoeffStep = 0;

                if (drawOrientation == DrawOrientation.doHorizontal)
                {
                    fCoeffStep = pictureBoxData.Width / (float)dataProfile.DataProfile.Length;

                    int iShiftY = 0;
                    if (imageSplitContainer.Panel2.Height < pictureBoxData.Height)
                    {
                        iShiftY += imageSplitContainer.Panel2.AutoScrollPosition.Y;
                        if (imageSplitContainer.Panel2.Width < pictureBoxData.Width) iShiftY += 20;
                    }

                    OldPoint = new Point((int)0, iHeight - (int)Math.Ceiling(dataProfile.DataProfile[0] * fCoeffAmpl) - iShiftY);
                    if (m_sysData.ViewGaussian == true) OldPointGaussian = new Point((int)0, iHeight - (int)Math.Ceiling(dataProfile.GaussianData.GaussianData[0] * fCoeffAmpl) - iShiftY);

                    for (int i = 1; i < dataProfile.DataProfile.Length; i++)
                    {
                        NewPoint = new Point((int)Math.Ceiling(i * fCoeffStep), iHeight - (int)Math.Ceiling(dataProfile.DataProfile[i] * fCoeffAmpl) - iShiftY);
                        grfx.DrawLine(pen, OldPoint, NewPoint);
                        OldPoint = NewPoint;

                        if (m_sysData.ViewGaussian == true)
                        {
                            NewPointGaussian = new Point((int)Math.Ceiling(i * fCoeffStep), iHeight - (int)Math.Ceiling(dataProfile.GaussianData.GaussianData[i] * fCoeffAmpl) - iShiftY);
                            grfx.DrawLine(m_PenGaussian, OldPointGaussian, NewPointGaussian);
                            OldPointGaussian = NewPointGaussian;
                        }
                    }

                    if (m_sysData.ScaleProfile == true)
                    {
                        int iLineLevel1 = iHeight - (int)(iHeightProfile * Decimal.ToSingle(m_sysData.ClipLevels.Level(0)) / 100f) - iShiftY;
                        int iLineLevel2 = iHeight - (int)(iHeightProfile * Decimal.ToSingle(m_sysData.ClipLevels.Level(1)) / 100f) - iShiftY;
                        int iLineLevel3 = iHeight - (int)(iHeightProfile * Decimal.ToSingle(m_sysData.ClipLevels.Level(2)) / 100f) - iShiftY;

                        for (int i = 0; i < pictureBoxData.Width; i += 6)
                        {
                            grfx.DrawLine(m_sysData.ClipLevels.LevelPen(0), i, iLineLevel1, i + 3, iLineLevel1);
                            grfx.DrawLine(m_sysData.ClipLevels.LevelPen(1), i, iLineLevel2, i + 3, iLineLevel2);
                            grfx.DrawLine(m_sysData.ClipLevels.LevelPen(2), i, iLineLevel3, i + 3, iLineLevel3);
                        }
                    }
                }
                else
                {
                    fCoeffStep = pictureBoxData.Height / (float)dataProfile.DataProfile.Length;

                    int iShiftX = imageSplitContainer.Panel2.AutoScrollPosition.X;

                    OldPoint = new Point((int)Math.Ceiling(dataProfile.DataProfile[0] * fCoeffAmpl) - iShiftX, 0);
                    if (m_sysData.ViewGaussian == true) OldPointGaussian = new Point((int)Math.Ceiling(dataProfile.GaussianData.GaussianData[0] * fCoeffAmpl) - iShiftX, 0);

                    for (int i = 1; i < dataProfile.DataProfile.Length; i++)
                    {
                        NewPoint = new Point((int)Math.Ceiling(dataProfile.DataProfile[i] * fCoeffAmpl) - iShiftX, (int)Math.Ceiling(i * fCoeffStep));
                        grfx.DrawLine(pen, OldPoint, NewPoint);
                        OldPoint = NewPoint;

                        if (m_sysData.ViewGaussian == true)
                        {
                            NewPointGaussian = new Point((int)Math.Ceiling(dataProfile.GaussianData.GaussianData[i] * fCoeffAmpl) - iShiftX, (int)Math.Ceiling(i * fCoeffStep));
                            grfx.DrawLine(m_PenGaussian, OldPointGaussian, NewPointGaussian);
                            OldPointGaussian = NewPointGaussian;
                        }
                    }

                    if (m_sysData.ScaleProfile == true)
                    {
                        int iLineLevel1 = (int)(iHeightProfile * Decimal.ToSingle(m_sysData.ClipLevels.Level(0)) / 100f - iShiftX);
                        int iLineLevel2 = (int)(iHeightProfile * Decimal.ToSingle(m_sysData.ClipLevels.Level(1)) / 100f - iShiftX);
                        int iLineLevel3 = (int)(iHeightProfile * Decimal.ToSingle(m_sysData.ClipLevels.Level(2)) / 100f - iShiftX);

                        for (int i = 0; i < pictureBoxData.Height; i += 6)
                        {
                            grfx.DrawLine(m_sysData.ClipLevels.LevelPen(0), iLineLevel1, i, iLineLevel1, i + 3);
                            grfx.DrawLine(m_sysData.ClipLevels.LevelPen(1), iLineLevel2, i, iLineLevel2, i + 3);
                            grfx.DrawLine(m_sysData.ClipLevels.LevelPen(2), iLineLevel3, i, iLineLevel3, i + 3);
                        }
                    }
                }
            }
        }

        private void UpdateVisibleAsync(Int64 Timestamp)
        {
            try
            {
                this.Invoke((MethodInvoker)delegate
                {
                    this.Invalidate();
                    dataSplitContainer.Panel2.Invalidate();
                    //            pictureBoxImage.Invalidate();
                    pictureBoxData.Invalidate();
                    toolStripStatusLabelTimeStamp.Text = (1000f / (double)Timestamp).ToString("#.000") + " fps";
                });
            }
            catch
            {
            }
        }

        private void picturePaletteImage_OnChangePalette(object sender, PaletteImage.PaletteImage.ChangePaletteEventArgs e)
        {
            AsyncChangePalette asyncChangePalette = new AsyncChangePalette(UpdateChangePalette);
            asyncChangePalette.BeginInvoke(e.Palette, e.Format, e.colorArray, null, null);
        }

        public Color[] Color
        {
            get { return m_colorArray; }

            set
            {
                if (value != null)
                {
                    if (m_colorArray == null) m_colorArray = new System.Drawing.Color[value.Length];

                    value.CopyTo(m_colorArray, 0);
                }
            }
        }

        private void UpdateChangePalette(ColorPalette Palette, PixelFormat pixelFormat, Color[] color)
        {
            try
            {
                this.Invoke((MethodInvoker)delegate
                {
                    if (m_bmp.PixelFormat != pixelFormat) m_bmp = new Bitmap(bm.ImageRectangle.Width, bm.ImageRectangle.Height, pixelFormat);

                    if (m_bmp.PixelFormat == PixelFormat.Format8bppIndexed)
                        m_bmp.Palette = Palette;
                    else
                        Color = color;

                    bm.pixelFormat = pixelFormat;

                    if (m_frm3D != null) m_frm3D.SetColorPalette(color);
                });
            }
            catch
            {
            }
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            m_sysData.DeserializeAppData(m_sysData.applicationData.m_strMyDataDir + "\\$$$.ini");

            m_sysData.applicationData.m_FormErrorMessage = new FormErrorMessage();
            m_sysData.applicationData.m_FormErrorMessage.Location = new Point(0, 0);
            m_sysData.applicationData.m_FormErrorMessage.ErrorMessage = ErrorStatus.BA_OK;
            m_sysData.applicationData.m_FormErrorMessage.Size = new System.Drawing.Size(160, 22);

            if (bm.Start(PixelFormat.Format8bppIndexed) == true)
            {
                m_sw = Stopwatch.StartNew();

                picturePaletteImage.Format = PixelFormat.Format8bppIndexed;

                pictureBoxImage.Size = bm.ImageRectangle.Size;

                m_bmp = new Bitmap(bm.ImageRectangle.Width, bm.ImageRectangle.Height, picturePaletteImage.Format);

                if (m_bmp.PixelFormat == PixelFormat.Format8bppIndexed)
                    m_bmp.Palette = picturePaletteImage.Palette;
                else
                    Color = picturePaletteImage.colorArray;

                bitsPerPixel8ToolStripMenuItem.Checked = (m_sysData.FormatPixel == PixelFormat.Format8bppIndexed);
                bitsPerPixel12ToolStripMenuItem.Checked = !bitsPerPixel8ToolStripMenuItem.Checked;
                toolStripStatusLabelPixelFormat.Text = (bitsPerPixel12ToolStripMenuItem.Checked == true) ? "12 bpp" : "8 bpp";

                bm.pixelFormat = picturePaletteImage.Format;

                sumProfileToolStripMenuItem.Checked = (m_sysData.ProfileType == BeamOnCL.BeamOnCL.TypeProfile.tpSum);
                lineProfileToolStripMenuItem.Checked = !sumProfileToolStripMenuItem.Checked;

                lineProfileToolStripButton.Checked = lineProfileToolStripMenuItem.Checked;
                sumProfileToolStripButton.Checked = sumProfileToolStripMenuItem.Checked;

                bm.typeProfile = m_sysData.ProfileType;

                toolStripStatusLabelTypeProfile.Text = (sumProfileToolStripMenuItem.Checked) ? "Sum" : "Line";

                trackBarBinning.Maximum = bm.MaxBinning;
                trackBarBinning.Minimum = bm.MinBinning;
                trackBarBinning.Value = bm.Binning;
                labelBinningMin.Text = trackBarBinning.Minimum.ToString();
                labelBinningMax.Text = trackBarBinning.Maximum.ToString();

                trackBarGain.Maximum = bm.MaxGain;
                trackBarGain.Minimum = bm.MinGain;
                trackBarGain.Value = bm.Gain;
                labelGainMin.Text = trackBarGain.Minimum.ToString();
                labelGainMax.Text = trackBarGain.Maximum.ToString();

                trackBarExposure.Maximum = bm.MaxExposure / 1000;
                trackBarExposure.Minimum = bm.MinExposure;
                trackBarExposure.Value = bm.Exposure;
                labelExposureMin.Text = trackBarExposure.Minimum.ToString();
                labelExposureMax.Text = trackBarExposure.Maximum.ToString();

                measuringToolStripMenuItem.Checked = m_sysData.Measure;

                scaleProfileToolStripMenuItem.Enabled = measuringToolStripMenuItem.Checked;
                typeProfileToolStripMenuItem.Enabled = measuringToolStripMenuItem.Checked;
                sumProfileToolStripButton.Enabled = measuringToolStripMenuItem.Checked;
                lineProfileToolStripButton.Enabled = measuringToolStripMenuItem.Checked;
                gaussianToolStripMenuItem.Enabled = measuringToolStripMenuItem.Checked;

                groupBoxPosition.Enabled = m_sysData.Measure;
                ProfileGroupBox.Enabled = m_sysData.Measure;
                gaussGroupBox.Enabled = m_sysData.Measure && m_sysData.ViewGaussian;

                dataSplitContainer.Panel1Collapsed = !propertyBoxToolStripMenuItem.Checked;
            }
            else
            {
                CustomMessageBox.Show("Software cannot found measurement camera." +
                                "Please check connection between measurement camera and computer USB port. ",
                                "Hardware Error #1:",
                                 MessageBoxButtons.OK,
                                 MessageBoxIcon.Stop);

                this.Close();
            }

            MenuToolEnabled();
        }

        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (m_sysData.applicationData.bSaveExit == true) m_sysData.SerializeAppData(m_sysData.applicationData.m_strMyDataDir + "\\$$$.ini");

            bm.Stop();
        }

        private void trackBarBinning_Scroll(object sender, EventArgs e)
        {
            bm.Binning = trackBarBinning.Value;

            m_bmp = new Bitmap(bm.ImageRectangle.Width, bm.ImageRectangle.Height, picturePaletteImage.Format);

            if (m_bmp.PixelFormat == PixelFormat.Format8bppIndexed) m_bmp.Palette = picturePaletteImage.Palette;

            pictureBoxImage.Size = bm.ImageRectangle.Size;

            m_sysData.applicationData.m_FormErrorMessage.Dispose();
        }

        private void trackBarTransparency_Scroll(object sender, EventArgs e)
        {
            //            this.pictureBoxData.BackColor = System.Drawing.Color.FromArgb(trackBarTransparency.Value, pictureBoxData.BackColor.R, pictureBoxData.BackColor.G, pictureBoxData.BackColor.B);
            this.pictureBoxData.BackColor = System.Drawing.Color.FromArgb(trackBarTransparency.Value, 0, 0, 0);
        }

        private void trackBarGain_Scroll(object sender, EventArgs e)
        {
            bm.Gain = trackBarGain.Value;
        }

        private void trackBarExposure_Scroll(object sender, EventArgs e)
        {
            bm.Exposure = trackBarExposure.Value;
        }

        private void scaleProfileToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            m_sysData.ScaleProfile = scaleProfileToolStripMenuItem.Checked;
        }

        private void pixelFormatToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem tsb = (ToolStripMenuItem)sender;

            if (tsb.Name.Contains("12"))
                bitsPerPixel8ToolStripMenuItem.Checked = !tsb.Checked;
            else if (tsb.Name.Contains("8"))
                bitsPerPixel12ToolStripMenuItem.Checked = !tsb.Checked;

            if (bitsPerPixel12ToolStripMenuItem.Checked == true)
                picturePaletteImage.Format = PixelFormat.Format24bppRgb;
            else if (bitsPerPixel8ToolStripMenuItem.Checked == true)
                picturePaletteImage.Format = PixelFormat.Format8bppIndexed;

            toolStripStatusLabelPixelFormat.Text = (bitsPerPixel12ToolStripMenuItem.Checked == true) ? "12 bpp" : "8 bpp";
        }

        private void typeProfileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripItem tsb = (ToolStripItem)sender;

            sumProfileToolStripMenuItem.Checked = ((tsb.Name.Contains("sum") == true) && (bm.typeProfile != BeamOnCL.BeamOnCL.TypeProfile.tpSum));
            lineProfileToolStripMenuItem.Checked = !sumProfileToolStripMenuItem.Checked;

            sumProfileToolStripButton.Checked = sumProfileToolStripMenuItem.Checked;
            lineProfileToolStripButton.Checked = lineProfileToolStripMenuItem.Checked;

            m_sysData.ProfileType =  (sumProfileToolStripMenuItem.Checked == true) ? BeamOnCL.BeamOnCL.TypeProfile.tpSum : BeamOnCL.BeamOnCL.TypeProfile.tpLIne;
            bm.typeProfile = m_sysData.ProfileType;

            toolStripStatusLabelTypeProfile.Text = (bm.typeProfile == BeamOnCL.BeamOnCL.TypeProfile.tpSum) ? "Sum" : "Line";
        }

        private void gaussianToolStripMenuItem_Click(object sender, EventArgs e)
        {
            m_sysData.ViewGaussian = gaussianToolStripMenuItem.Checked;

            gaussGroupBox.Enabled = m_sysData.Measure && m_sysData.ViewGaussian;
        }

        private void dataSplitContainer_Panel2_Paint(object sender, PaintEventArgs e)
        {
            if (m_sysData.Measure == true)
            {
                labelPositionXValue.Text = GetStringFormat(m_sysData.positionData.RealPosition.X);
                labelPositionYValue.Text = GetStringFormat(m_sysData.positionData.RealPosition.Y);

                labelHorizontalValue1.Text = m_sysData.HorizontalProfile.strWidth[0];
                labelHorizontalValue2.Text = m_sysData.HorizontalProfile.strWidth[1];
                labelHorizontalValue3.Text = m_sysData.HorizontalProfile.strWidth[2];

                labelVerticalValue1.Text = m_sysData.VerticalProfile.strWidth[0];
                labelVerticalValue2.Text = m_sysData.VerticalProfile.strWidth[1];
                labelVerticalValue3.Text = m_sysData.VerticalProfile.strWidth[2];

                if (m_sysData.ViewGaussian == true)
                {
                    labelGaussianHorizontalValue1.Text = m_sysData.HorizontalProfile.strGaussWidth[0];
                    labelGaussianHorizontalValue2.Text = m_sysData.HorizontalProfile.strGaussWidth[1];
                    labelGaussianHorizontalValue3.Text = m_sysData.HorizontalProfile.strGaussWidth[2];

                    labelGaussianVerticalValue1.Text = m_sysData.VerticalProfile.strGaussWidth[0];
                    labelGaussianVerticalValue2.Text = m_sysData.VerticalProfile.strGaussWidth[1];
                    labelGaussianVerticalValue3.Text = m_sysData.VerticalProfile.strGaussWidth[2];

                    labelGaussianCorrelationHorizontalValue.Text = GetStringFormat((float)m_sysData.HorizontalProfile.m_fCorrelation);
                    labelGaussianCorrelationVerticalValue.Text = GetStringFormat((float)m_sysData.VerticalProfile.m_fCorrelation);
                }

                labelClipLevel3.Text = Decimal.ToSingle(m_sysData.ClipLevels.Level(2)).ToString("F1") + "%";
                labelClipLevel2.Text = Decimal.ToSingle(m_sysData.ClipLevels.Level(1)).ToString("F1") + "%";
                labelClipLevel1.Text = Decimal.ToSingle(m_sysData.ClipLevels.Level(0)).ToString("F1") + "%";

                labelGaussianClipLevel3.Text = labelClipLevel3.Text;
                labelGaussianClipLevel2.Text = labelClipLevel2.Text;
                labelGaussianClipLevel1.Text = labelClipLevel1.Text;
            }
        }

        private String GetStringFormat(float fData)
        {
            String strFormat;

            if (Math.Abs(fData) >= 1000f)
                strFormat = "{0:F0}";
            else if (Math.Abs(fData) >= 100f)
                strFormat = "{0:F1}";
            else if (Math.Abs(fData) >= 10f)
                strFormat = "{0:F2}";
            else
                strFormat = "{0:F3}";

            return String.Format(strFormat, fData);
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmAboutBox aboutBox = new frmAboutBox();
            aboutBox.ShowDialog();
        }

        private void mnuFilePrint_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem tsm = (ToolStripMenuItem)sender;

            OpenFileDialog openPrintFile = new OpenFileDialog();

            if ((String.CompareOrdinal(tsm.Name, "tbFilePrintBMP") == 0) || (String.CompareOrdinal(tsm.Name, "mnuFilePrintBMP") == 0))
            {
                openPrintFile.Title = "Print Image File";
                openPrintFile.Filter = "Bitmap files (*.bmp)|*.bmp|JPEG files Interchange Format (*.jpg)|*.jpg|All files (*.*)|*.*";
            }
            else if ((String.CompareOrdinal(tsm.Name, "tbFilePrintText") == 0) || (String.CompareOrdinal(tsm.Name, "mnuFilePrintText") == 0))
            {
                openPrintFile.Title = "Print Text File";
                openPrintFile.Filter = "Log files (*.log)|*.log|XML files (*.xml)|*.xml|HTM files (*.htm)|*.htm|Config files (*.ini)|*.ini|Plot files (*.plt)|*.plt|Chart files (*.cht)|*.cht|Text files (*.txt)|*.txt|Snapshot files (*.snp)|*.snp|Power Scope Data files (*.psd)|*.psd|M2 Data files (*.m2d)|*.m2d|All files (*.*)|*.*";
            }
            openPrintFile.FileName = "";

            openPrintFile.AddExtension = true;
            openPrintFile.CheckPathExists = true;
            openPrintFile.CheckFileExists = true;
            openPrintFile.InitialDirectory = m_sysData.applicationData.m_strMyDataDir;
            openPrintFile.RestoreDirectory = true;
            //openPrintFile.FileOk += new CancelEventHandler(openView_FileOk);

            if (openPrintFile.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    if ((String.CompareOrdinal(tsm.Name, "tbFilePrintBMP") == 0) || (String.CompareOrdinal(tsm.Name, "mnuFilePrintBMP") == 0))
                    {
                        printImage = Image.FromFile(openPrintFile.FileName);

                        if (printDialog.ShowDialog() == DialogResult.OK)
                        {
                            printDocument.DocumentName = Text;

                            m_printerData.printerType = ePrinterType.ePrintImage;
                            m_printerData.strFileName = openPrintFile.FileName;
                            m_printerData.iPagePrint = 1;

                            printDocument.Print();
                        }
                    }
                    else if ((String.CompareOrdinal(tsm.Name, "tbFilePrintText") == 0) || (String.CompareOrdinal(tsm.Name, "mnuFilePrintText") == 0))
                    {
                        streamToPrint = new StreamReader(openPrintFile.FileName);

                        try
                        {
                            if (printDialog.ShowDialog() == DialogResult.OK)
                            {
                                printDocument.DocumentName = Text;

                                m_printerData.printerType = ePrinterType.ePrintText;
                                m_printerData.strFileName = openPrintFile.FileName;
                                m_printerData.iPagePrint = 1;

                                printDocument.Print();
                            }
                        }
                        finally
                        {
                            streamToPrint.Close();
                        }
                    }
                }
                catch (Exception ex)
                {
                    CustomMessageBox.Show(ex.Message, "Print Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void printDocument_PrintPage(object sender, System.Drawing.Printing.PrintPageEventArgs ev)
        {
            float linesPerPage = 0;
            float yPos = 0;
            int count = 0;
            string line = null;
            float coeffExpandW;
            float coeffExpandH;
            float coeffExpand;

            Graphics graph = ev.Graphics;
            float HeightFont = printFont.GetHeight(graph);
            RectangleF rectanglefFull, rectanglefText;
            StringFormat stringformat = new StringFormat();

            if (graph.VisibleClipBounds.X < 0)
                rectanglefFull = ev.MarginBounds;
            else
                rectanglefFull = new RectangleF(
                    ev.MarginBounds.Left - (ev.PageBounds.Width - graph.VisibleClipBounds.Width) / 2,
                    ev.MarginBounds.Top - (ev.PageBounds.Height - graph.VisibleClipBounds.Height) / 2,
                    ev.MarginBounds.Width,
                    ev.MarginBounds.Height);

            rectanglefText = RectangleF.Inflate(rectanglefFull, 0, -2 * HeightFont);

            if (m_printerData.printerType == ePrinterType.ePrintText)
            {
                stringformat.Trimming = StringTrimming.EllipsisCharacter;
                stringformat.FormatFlags |= StringFormatFlags.NoWrap;

                linesPerPage = (int)Math.Floor(rectanglefText.Height / HeightFont);
                rectanglefText.Height = linesPerPage * HeightFont;

                // Print each line of the file.
                while (count < linesPerPage && ((line = streamToPrint.ReadLine()) != null))
                {
                    yPos = rectanglefText.Top + (count * HeightFont);
                    graph.DrawString(line, printFont, Brushes.Black, rectanglefText.Left, yPos, stringformat);
                    count++;
                }

                stringformat = new StringFormat();

                stringformat.Alignment = StringAlignment.Far;
                graph.DrawString("Page " + m_printerData.iPagePrint.ToString(), printFont, Brushes.Black, rectanglefFull, stringformat);
                m_printerData.iPagePrint++;

                // If more lines exist, print another page.
                if (line != null)
                    ev.HasMorePages = true;
                else
                {
                    ev.HasMorePages = false;
                    m_printerData.iPagePrint = 1;
                }
            }
            else
            {
                if (rectanglefText.Width < printImage.Size.Width)
                {
                    if (rectanglefText.Height < printImage.Size.Height)
                    {
                        coeffExpandW = rectanglefText.Width / printImage.Size.Width;
                        coeffExpandH = rectanglefText.Height / printImage.Size.Height;
                        coeffExpand = (coeffExpandH < coeffExpandW) ? coeffExpandH : coeffExpandW;
                    }
                    else
                    {
                        coeffExpand = rectanglefText.Width / printImage.Size.Width;
                    }
                }
                else if (rectanglefText.Height < printImage.Size.Height)
                {
                    coeffExpand = rectanglefText.Height / printImage.Size.Height;
                }
                else
                {
                    coeffExpand = 1f;
                }

                graph.DrawImage(printImage, rectanglefText.Left, rectanglefText.Top, printImage.Size.Width * coeffExpand, printImage.Size.Height * coeffExpand);
            }
        }

        private void PrintCaptureWindowImage(object rect)
        {
            Thread.Sleep(500);

            try
            {
                printImage = GetImageForm((Rectangle)rect);

                if (printDialog.ShowDialog() == DialogResult.OK)
                {
                    printDocument.DocumentName = Text;
                    m_printerData.printerType = ePrinterType.ePrintImage;

                    printDocument.Print();
                }
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show(ex.Message, "Print Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CaptureWindowImage(object rect)
        {
            Thread.Sleep(500);

            SaveImageFile(GetImageForm((Rectangle)rect));
        }

        private Image GetImageForm(Rectangle scrR)
        {
            Bitmap MyImage = new Bitmap(scrR.Width, scrR.Height);
            using (Graphics g = Graphics.FromImage(MyImage))
                g.CopyFromScreen(new Point(scrR.Left, scrR.Top), Point.Empty, new Size(scrR.Width, scrR.Height));

            //m_SystemMessage |= (Byte)SystemStatus.M_SS_SAVEDATA;

            return MyImage;
        }

        private void SaveImageFile(Image img)
        {
            SaveFileDialog saveImageFile = new SaveFileDialog();

            saveImageFile.Filter = "Bitmap files (*.bmp)|*.bmp|JPEG files Interchange Format (*.jpg)|*.jpg|All files (*.*)|*.*";
            saveImageFile.FileName = "";

            saveImageFile.AddExtension = true;
            saveImageFile.CheckPathExists = true;
            saveImageFile.CheckFileExists = false;
            saveImageFile.InitialDirectory = m_sysData.applicationData.m_strMyDataDir;
            saveImageFile.RestoreDirectory = true;
            //saveImageFile.FileOk += new CancelEventHandler(ImageFileDialog_FileOk);
            saveImageFile.Title = "Save Image File";

            if (saveImageFile.ShowDialog() == DialogResult.OK)
            {
                FileInfo fi = new FileInfo(saveImageFile.FileName);

                if (fi.Extension == ".bmp")
                {
                    img.Save(saveImageFile.FileName, System.Drawing.Imaging.ImageFormat.Bmp);
                }
                else if (fi.Extension == ".jpg")
                {
                    img.Save(saveImageFile.FileName, System.Drawing.Imaging.ImageFormat.Jpeg);
                }
            }
        }

        void ImageFileDialog_FileOk(object sender, CancelEventArgs e)
        {
            FileDialog ImageFileDialog = (FileDialog)sender;

            FileInfo fi = new FileInfo(ImageFileDialog.FileName);

            if ((fi.Extension != ".bmp") && (fi.Extension != ".jpg"))
            {
                CustomMessageBox.Show("Extension of the Image File should be 'BMP' or 'JPG'",
                                ImageFileDialog.Title, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

                e.Cancel = true;
            }
        }

        private void mnuFilePrintWindow_Click(object sender, EventArgs e)
        {
            Rectangle rect = new Rectangle();

            ToolStripMenuItem tsm = (ToolStripMenuItem)sender;

            if (String.CompareOrdinal(tsm.Name, "mnuFilePrintScreen") == 0)
            {
                rect = this.Bounds;
            }
            else if (String.CompareOrdinal(tsm.Name, "mnuFilePrintWindow") == 0)
            {
                rect = this.ActiveMdiChild.Parent.RectangleToScreen(this.ActiveMdiChild.Bounds);
            }

            Thread PrintCaptureImage = new Thread(PrintCaptureWindowImage);
            PrintCaptureImage.TrySetApartmentState(ApartmentState.STA);

            PrintCaptureImage.Start(rect);
            //PrintCaptureImage.Start(this.RectangleToClient(rect));
        }

        private void mnuFilePageSetup_Click(object sender, EventArgs e)
        {
            if (PrinterExist() == true) pageSetupDialog.ShowDialog();
        }

        static bool PrinterExist()
        {
            return System.Drawing.Printing.PrinterSettings.InstalledPrinters.Count > 0 ? true : false;
        }

        private void MenuToolEnabled()
        {
            if (PrinterExist() == true)
            {
                mnuFilePrint.Enabled = true;
                mnuFilePrintWindow.Enabled = true;
                mnuFilePrintScreen.Enabled = true;
                mnuFilePageSetup.Enabled = true;
                tbFilePrint.Enabled = true;
            }
        }

        private void mnuFileView_Click(object sender, EventArgs e)
        {
            String strFilter;
            String strViewExt = "";

            ToolStripItem tsmi = (ToolStripItem)sender;
            OpenFileDialog openView = new OpenFileDialog();

            strFilter = "All files (*.*)|*.*|";

            if (DataExcel.CheckExcelInst() == true) strFilter += (DataExcel.Version < 12) ? "Excel files (*.xls)|*.xls|" : "Excel files (*.xlsx)|*.xlsx|";

            strFilter += "Config files (*.ini)|*.ini|Plot files (*.plt)|*.plt|Chart files (*.cht)|*.cht|Test files (*.tst)|*.tst|Bitmap files (*.bmp)|*.bmp|JPEG files Interchange Format (*.jpg)|*.jpg|Text files (*.txt)|*.txt|Snapshot files (*.snp)|*.snp|";
            strFilter += "XML files (*.xml)|*.xml|HTM files (*.htm)|*.htm|Power Scope Data files (*.psd)|*.psd|M2 Data files (*.m2d)|*.m2d|All files (*.*)|*.*";

            openView.Filter = strFilter;

            if ((strViewExt == ".log") || (strViewExt == "")) openView.FilterIndex = 1;
            else if ((strViewExt == ".xls") || (strViewExt == ".xlsx")) openView.FilterIndex = 2;
            else if (strViewExt == ".ini") openView.FilterIndex = 3;
            else if (strViewExt == ".plt") openView.FilterIndex = 4;
            else if (strViewExt == ".cht") openView.FilterIndex = 5;
            else if (strViewExt == ".tst") openView.FilterIndex = 6;
            else if (strViewExt == ".bmp") openView.FilterIndex = 7;
            else if (strViewExt == ".jpg") openView.FilterIndex = 8;
            else if (strViewExt == ".txt") openView.FilterIndex = 9;
            else if (strViewExt == ".snp") openView.FilterIndex = 10;
            else if (strViewExt == ".xml") openView.FilterIndex = 11;
            else if (strViewExt == ".htm") openView.FilterIndex = 12;
            else if (strViewExt == ".psd") openView.FilterIndex = 13;
            else if (strViewExt == ".m2d") openView.FilterIndex = 14;
            else openView.FilterIndex = 15;

            openView.CheckPathExists = true;
            openView.CheckFileExists = true;
            openView.InitialDirectory = m_sysData.applicationData.m_strMyDataDir;
            openView.RestoreDirectory = true;
            openView.Title = tsmi.ToolTipText;
            //openView.FileOk += new CancelEventHandler(openView_FileOk);

            if (openView.ShowDialog() == DialogResult.OK)
            {
                FileInfo fi = new FileInfo(openView.FileName);
                strViewExt = fi.Extension;

                try
                {
                    if ((fi.Extension == ".bmp") || (fi.Extension == ".jpg"))
                    {
                        System.Diagnostics.Process.Start(openView.FileName);
                        //frmViewImage FormViewImage = new frmViewImage(openView.FileName);
                        //FormViewImage.Show();
                    }
                    else if (fi.Extension == ".snp")
                    {
                        //StartSnapshot(openView.FileName);
                    }
                    else if ((fi.Extension == ".xml") || (fi.Extension == ".htm"))
                    {
                        System.Diagnostics.Process.Start(openView.FileName);
                    }
                    else if (fi.Extension == ".xls")
                    {
                        System.Diagnostics.Process.Start(openView.FileName);
                    }
                    else if ((fi.Extension == ".xlsx") && (DataExcel.Version >= 12))
                    {
                        System.Diagnostics.Process.Start(openView.FileName);
                    }
                    else if (fi.Extension == ".log")
                    {
                        System.Diagnostics.Process.Start("wordpad.exe", '"' + openView.FileName + '"');
                    }
                    else
                    {
                        System.Diagnostics.Process.Start("notepad", openView.FileName);
                    }
                }
                catch (Exception ex)
                {
                    CustomMessageBox.Show(ex + "Unable to open file " + fi.Name + " .", openView.Title, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        void openView_FileOk(object sender, CancelEventArgs e)
        {
            //OpenFileDialog openView = (OpenFileDialog)sender;

            //DirectoryInfo dInfo = new DirectoryInfo(Path.GetDirectoryName(openView.FileName));

            //if ((((UInt64)dInfo.Attributes & FILE_ATTRIBUTE_VIRTUAL) == FILE_ATTRIBUTE_VIRTUAL) && (Environment.OSVersion.Version.Major >= 6))
            //{
            //    openView.FileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\VirtualStore", openView.FileName.Substring((Path.GetPathRoot(openView.FileName).Length)));
            //}
        }

        private void tbHelpActiveWindow_Click(object sender, EventArgs e)
        {
            Form activeChild = this.ActiveMdiChild;

            if (activeChild != null)
                Help.ShowHelp(this, ProductName + ".chm", HelpNavigator.KeywordIndex, activeChild.Tag);
            else
                Help.ShowHelp(this, ProductName + ".chm", HelpNavigator.KeywordIndex, ProductName);
        }

        private void measuringToolStrip_Click(object sender, EventArgs e)
        {
            if (sender.GetType() == typeof(ToolStripButton))
                m_sysData.Measure = ((ToolStripButton)sender).Checked;
            else if (sender.GetType() == typeof(ToolStripMenuItem))
                m_sysData.Measure = ((ToolStripMenuItem)sender).Checked;

            measuringToolStripMenuItem.Checked = m_sysData.Measure;
            measuringToolStripButton.Checked = m_sysData.Measure;

            groupBoxPosition.Enabled = m_sysData.Measure;
            ProfileGroupBox.Enabled = m_sysData.Measure;
            gaussGroupBox.Enabled = m_sysData.Measure && m_sysData.ViewGaussian;

            scaleProfileToolStripMenuItem.Enabled = measuringToolStripMenuItem.Checked;
            typeProfileToolStripMenuItem.Enabled = measuringToolStripMenuItem.Checked;
            sumProfileToolStripButton.Enabled = measuringToolStripMenuItem.Checked;
            lineProfileToolStripButton.Enabled = measuringToolStripMenuItem.Checked;

            gaussianToolStripMenuItem.Enabled = measuringToolStripMenuItem.Checked;

            tbOptionsSetupDataCollection.Enabled = measuringToolStripMenuItem.Checked;
            mnuFileSetupDataCollection.Enabled = measuringToolStripMenuItem.Checked;
            tbOptionsStartDataCollection.Checked = measuringToolStripMenuItem.Checked;
            mnuFileStartDataCollection.Checked = measuringToolStripMenuItem.Checked;
        }

        private void measuringToolStrip_CheckedChanged(object sender, EventArgs e)
        {
            measuringToolStripButton.Checked = measuringToolStripMenuItem.Checked;
        }

        private void mnuOptionsStartDataCollection_Click(object sender, EventArgs e)
        {
            if ((m_sysData.logData.strFileName == null) || (m_sysData.logData.strFileName.Equals("") == true))
            {
                CustomMessageBox.Show("Not specified the name and path for the Log file.",
                                                        "Save Log File",
                                                        MessageBoxButtons.OK,
                                                        MessageBoxIcon.Error);
                return;
            }

            FileInfo fi = new FileInfo(m_sysData.logData.strFileName);

            if (sender.ToString().Contains("Start") == true)
            {

                if (File.Exists(m_sysData.logData.strFileName))
                {
                    if (CustomMessageBox.Show("This file '" + fi.Name + "' already exists. \nDo you want to replace it?",
                                        "Save Log File",
                                        MessageBoxButtons.YesNo,
                                        MessageBoxIcon.Question) == DialogResult.No)
                        return;
                    else
                        File.Delete(m_sysData.logData.strFileName);
                }
            }

            EnableLogFile(sender.ToString().Contains("Start"));
        }

        private void EnableLogFile(Boolean bEnable)
        {
            if (bEnable == true)
            {
                mnuFileStartDataCollection.Checked = true;
                mnuFileStartDataCollection.Image = global::BeamOn_2K.Properties.Resources.Hand;
                mnuFileStartDataCollection.Text = "&Stop Log File";
                mnuFileStartDataCollection.ToolTipText = "Stop Log File";

                m_fldLog = new FileLogData();
                m_fldLog.OnStopLogFile += new FileLogData.StopLogFile(m_fldLog_OnStopLogFile);
                m_fldLog.CreateHeader();

                m_SystemMessage |= (Byte)SystemStatus.M_SS_LOG;
            }
            else
            {
                mnuFileStartDataCollection.Checked = false;
                mnuFileStartDataCollection.Image = global::BeamOn_2K.Properties.Resources.Start;
                mnuFileStartDataCollection.Text = "&Start Log File";
                mnuFileStartDataCollection.ToolTipText = "Start Log File";

                if ((m_fldLog != null) && (m_fldLog.IsOpen() == true)) m_fldLog.CloseLog();

                m_fldLog = null;

                m_SystemMessage = (Byte)(m_SystemMessage & (~(int)SystemStatus.M_SS_LOG));
            }

            m_sysData.logData.bStart = bEnable;

            AddItemAsyncDelegate asyncSM = new AddItemAsyncDelegate(UpdateSystemMessage);
            asyncSM.BeginInvoke(null, null);

            tbOptionsStartDataCollection.ToolTipText = mnuFileStartDataCollection.ToolTipText;
            tbOptionsStartDataCollection.Image = mnuFileStartDataCollection.Image;
            tbOptionsStartDataCollection.Text = mnuFileStartDataCollection.Text;
            tbOptionsStartDataCollection.Checked = mnuFileStartDataCollection.Checked;

            mnuFileSetupDataCollection.Enabled = !mnuFileStartDataCollection.Checked;
            tbOptionsSetupDataCollection.Enabled = mnuFileSetupDataCollection.Enabled;
        }

        private void CloseLogFile()
        {
            try
            {
                this.Invoke((MethodInvoker)delegate
                {
                    this.mnuFileStartDataCollection.Checked = false;
                    this.mnuFileStartDataCollection.Image = global::BeamOn_2K.Properties.Resources.Start;
                    this.mnuFileStartDataCollection.Text = "Start Lo&g";
                    this.mnuFileStartDataCollection.ToolTipText = "Start Log";

                    this.tbOptionsStartDataCollection.ToolTipText = this.mnuFileStartDataCollection.ToolTipText;
                    this.tbOptionsStartDataCollection.Image = this.mnuFileStartDataCollection.Image;
                    this.tbOptionsStartDataCollection.Text = this.mnuFileStartDataCollection.Text;
                    this.tbOptionsStartDataCollection.Checked = this.mnuFileStartDataCollection.Checked;

                    this.mnuFileSetupDataCollection.Enabled = !this.mnuFileStartDataCollection.Checked;
                    this.tbOptionsSetupDataCollection.Enabled = this.mnuFileSetupDataCollection.Enabled;

                    m_SystemMessage = (Byte)(m_SystemMessage & (~(int)SystemStatus.M_SS_LOG));

                    AddItemAsyncDelegate asyncSM = new AddItemAsyncDelegate(UpdateSystemMessage);
                    asyncSM.BeginInvoke(null, null);
                });
            }
            catch
            {
            }
        }

        void m_fldLog_OnStopLogFile(object sender, EventArgs e)
        {
            CloseLogAsyncDelegate asyncCloseLog = new CloseLogAsyncDelegate(CloseLogFile);
            asyncCloseLog.BeginInvoke(null, null);
        }

        private void UpdateSystemMessage()
        {
            try
            {
                this.Invoke((MethodInvoker)delegate
                {
                    toolStripStatuslblError.SystemMessage = m_SystemMessage;
                    m_sysData.applicationData.m_FormErrorMessage.SystemMessage = m_SystemMessage;

                    toolStripStatuslblError.LogTypeMessage = (Byte)m_sysData.logData.ftFile;
                    m_sysData.applicationData.m_FormErrorMessage.LogTypeMessage = (Byte)m_sysData.logData.ftFile;

                    if (((m_sysData.applicationData.m_FormErrorMessage.SystemMessage != 0) || (m_sysData.applicationData.m_FormErrorMessage.ErrorMessage != ErrorStatus.BA_OK) || (m_sysData.applicationData.m_FormErrorMessage.DemoVersion == true)) && (mainToolStrip.Visible == false))
                    {
                        m_sysData.applicationData.m_FormErrorMessage.Show();
                        m_sysData.applicationData.m_FormErrorMessage.Focus();
                    }
                    else
                        m_sysData.applicationData.m_FormErrorMessage.Hide();
                });

            }
            catch
            {
            }
        }

        private void mnuOptionsSetupDataCollection_Click(object sender, EventArgs e)
        {
            FormSetupLog FormSetupLog = new FormSetupLog();

            if (FormSetupLog.ShowDialog() == DialogResult.OK)
            {
                mnuFileStartDataCollection.Enabled = true;
                tbOptionsStartDataCollection.Enabled = mnuFileStartDataCollection.Enabled;
            }
        }

        private void statusBarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            mainStatusStrip.Visible = statusBarToolStripMenuItem.Checked;
            m_sysData.applicationData.bViewStatusBar = statusBarToolStripMenuItem.Checked;
        }

        private void mnuOptionsSaveSettingsOnExit_Click(object sender, EventArgs e)
        {
            m_sysData.applicationData.bSaveExit = mnuOptionsSaveSettingsOnExit.Checked;
        }

        private void buttonCollapseProperty_Click(object sender, EventArgs e)
        {
            dataSplitContainer.Panel1Collapsed = !dataSplitContainer.Panel1Collapsed;
            propertyBoxToolStripMenuItem.Checked = !dataSplitContainer.Panel1Collapsed;
        }

        private void tbViewProjection_Click(object sender, EventArgs e)
        {
            if (sender.GetType() == typeof(ToolStripButton))
                projection3DToolStripMenuItem.Checked = tbViewProjection.Checked;
            else if (sender.GetType() == typeof(ToolStripMenuItem))
                tbViewProjection.Checked = projection3DToolStripMenuItem.Checked;

            if (projection3DToolStripMenuItem.Checked == true)
            {
                m_frm3D = new Form3DProjection();
                m_frm3D.SetColorPalette(picturePaletteImage.colorArray);
                m_frm3D.FormClosed += new FormClosedEventHandler(m_frm3D_FormClosed);
                m_frm3D.Show(this);
            }
            else
            {
                if (m_frm3D != null) m_frm3D.Close();
                m_frm3D = null;
            }
        }

        void m_frm3D_FormClosed(object sender, FormClosedEventArgs e)
        {
            m_frm3D = null;

            projection3DToolStripMenuItem.Checked = false;
            tbViewProjection.Checked = false;
        }
    }
}
