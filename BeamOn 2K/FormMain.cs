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
using System.Collections;

namespace BeamOn_U3
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
        M_SS_FAST_MODE = 256
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

        System.Timers.Timer m_tUpdateSnapshot = null;

        private BeamOnCL.BeamOnCL bm = null;

        Pen m_PenSensor = new Pen(System.Drawing.Color.Yellow, 1.2f);
        Pen m_PenCentroid = new Pen(System.Drawing.Color.Blue, 0.2f);
        Pen m_PenEllipse = new Pen(System.Drawing.Color.Green, 2f);
        Pen m_PenGaussian = new Pen(System.Drawing.Color.Red, 0.2f);
        Font myFont = new Font("Arial", 10, FontStyle.Bold);
        Brush PaletteBrush = new SolidBrush(System.Drawing.Color.DarkGray);
        StringFormat m_strfrm = new StringFormat();
        StringFormat m_strfrmVertical = new StringFormat(StringFormatFlags.DirectionVertical);
        Pen m_PenGrid = new Pen(System.Drawing.Color.DarkGray, 0.1f);
        Pen m_PenLineProfile = new Pen(System.Drawing.Color.Red, 0.1f);
        Brush CentroidSensorBrush = new SolidBrush(System.Drawing.Color.Yellow);

        public enum DrawOrientation { doHorizontal, doVertical };

        public const UInt16 DELTA = 2;
        private int iLevelSelected = -1;
        private int iLevelCurrent = -1;

        BeamOnCL.Profile m_profileHorizontal = null;
        BeamOnCL.Profile m_profileVertical = null;
        Point[] plArea = null;
        Point pointSensorCenter = new Point();

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
        FileFastModeData m_ffmddLog = null;

        UInt16 m_SystemMessage = 0;
        Form3DProjection m_frm3D = null;

        Image imageWhell = null;
        Point pImageWhellPosition = new Point(0, 0);
        Rectangle rImageWhellRectangle = new Rectangle(0, 0, 143, 177);
        private static System.Timers.Timer whellTimer;
        Point[] m_pFilterPosition = new Point[4];
        String[] m_strFilterName = new String[4];
        int m_iFilterRadius = 25;
        int m_iWhellDirection = -1;
        int m_iWhellStep = 0;

        public const UInt16 POWER_CALIBRATION_NUM = 20;
        private bool m_bPowerCalibration = false;
        private ushort m_iPowerCalibrationCount = POWER_CALIBRATION_NUM;
        private double m_uiPowerDataSum = 0;
        private FormPowerCalibration m_formPowerCalibration = null;
        private double m_dAveragePowerDataSum;

        public String m_strSystemTitle;
        private BeamOnCL.SnapshotBase m_snapshot = null;
        private bool m_bFreezePicture = false;

        ArrayList m_arraySapshot = null;

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

            imageWhell = global::BeamOn_U3.Properties.Resources.MotorizeFilterWheel_W910smm1;
            m_pFilterPosition[0] = new Point(71, 101);
            m_pFilterPosition[1] = new Point(108, 64);
            m_pFilterPosition[2] = new Point(71, 28);
            m_pFilterPosition[3] = new Point(35, 64);

            m_strFilterName[0] = "ND8";
            m_strFilterName[1] = "ND64";
            m_strFilterName[2] = "ND200";
            m_strFilterName[3] = "ND1000";
        }

        void bm_OnImageReceved(object sender, BeamOnCL.MeasureCamera.NewDataRecevedEventArgs e)
        {
            if (e.FastMode == true)
            {
                if (m_ffmddLog != null) m_ffmddLog.AddData(e.Snapshot.TimeStamp);
                if (m_arraySapshot == null) m_arraySapshot = new ArrayList();
                m_arraySapshot.Add(e.Snapshot.Clone());
                return;
            }

            //if (InvokeRequired)
            //{
            //    // If called from a different thread, we must use the Invoke method to marshal the call to the proper GUI thread.
            //    // The grab result will be disposed after the event call. Clone the event arguments for marshaling to the GUI thread. 
            //    BeginInvoke(new EventHandler<BeamOnCL.MeasureCamera.NewDataRecevedEventArgs>(bm_OnImageReceved), sender, e.Clone());
            //    return;
            //}
            //BeamOnCL.MeasureCamera.NewDataRecevedEventArgs e = ee.Clone();

            if (m_bFreezePicture == false)
            {
                m_snapshot = e.Snapshot.Clone();

                if (m_bmp != null)
                {
                    lock (m_lLockBMP)
                    {
                        try
                        {
                            BitmapData bmpData = m_bmp.LockBits(
                                                                new Rectangle(new Point(0, 0), m_snapshot.ImageRectangle.Size),
                                                                System.Drawing.Imaging.ImageLockMode.WriteOnly,
                                                                m_bmp.PixelFormat
                                                                );

                            e.Snapshot.SetImageDataArray(bmpData.Scan0, m_colorArray);

                            m_bmp.UnlockBits(bmpData);
                        }
                        catch { }
                        finally
                        {
                            // Dispose the DataReceved result if needed for returning it to the grab loop.
                            e.DisposeDataRecevedIfClone();
                        }
                    }
                }
            }

            if (m_snapshot != null)
            {
                if (m_frm3D != null) m_frm3D.ImageData = m_snapshot;

                if (m_sysData.Measure == true)
                {
                    bm.GetMeasure(m_snapshot);

                    plArea = bm.CreateEllipse();

                    m_sysData.positionData.RealPosition = bm.Centroid;
                    m_sysData.positionData.Ellipse.Major = bm.MajorRadius;
                    m_sysData.positionData.Ellipse.Minor = bm.MinorRadius;
                    m_sysData.positionData.Ellipse.Orientation = bm.Angle;

                    m_profileHorizontal = new BeamOnCL.Profile(bm.profileHorizontal);
                    m_profileVertical = new BeamOnCL.Profile(bm.profileVertical);

                    if ((m_sysData.ProfileType == BeamOnCL.BeamOnCL.TypeProfile.tpLIne) && (m_sysData.LineProfileType == BeamOnCL.BeamOnCL.TypeLineProfile.tpLineCentroid))
                        bm.CrossPosition = new Point((int)bm.PixelCentroid.X, (int)bm.PixelCentroid.Y);

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
                        m_sysData.logData.LastMeasureTime = m_snapshot.TimeStamp;
                        m_fldLog.AddData();
                    }

                    if (m_bPowerCalibration == true)
                    {
                        if (m_iPowerCalibrationCount > 0)
                        {
                            if (m_iPowerCalibrationCount == POWER_CALIBRATION_NUM)
                                m_uiPowerDataSum = bm.CurrentAreaPower;
                            else
                                m_uiPowerDataSum += bm.CurrentAreaPower;

                            m_iPowerCalibrationCount--;

                            if (m_formPowerCalibration != null) m_formPowerCalibration.MeasureCount = POWER_CALIBRATION_NUM - m_iPowerCalibrationCount;
                        }
                        else
                        {
                            m_dAveragePowerDataSum = m_uiPowerDataSum / (double)POWER_CALIBRATION_NUM;
                        }
                    }
                    else
                    {
                        m_sysData.powerData.Power = (float)((bm.CurrentAreaPower / m_sysData.powerData.fSensitivity) / m_sysData.powerData.fSensFactor / m_sysData.powerData.PowerCalibr.PowerCoeff(bm.Exposure, bm.Gain));
                        m_sysData.powerData.mwPower = (m_sysData.powerData.Power / m_sysData.powerData.realFilterFactor) / m_sysData.powerData.currentSAMFactor - m_sysData.powerData.mwOffsetPower * Math.Abs(Convert.ToInt16(m_sysData.powerData.bIndOffset));
                    }
                }
            }

            //m_sw.Stop();

            AsyncTimeStamp asyncTimeStamp = new AsyncTimeStamp(UpdateVisibleAsync);
            asyncTimeStamp.BeginInvoke(m_sw.ElapsedMilliseconds, null, null);

            //System.Threading.Thread.Sleep(10);

            //m_sw = Stopwatch.StartNew();
            //m_sw.Start();
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

        private void toolBarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            m_sysData.applicationData.bViewToolbar = toolBarToolStripMenuItem.Checked;

            ViewToolStrip(m_sysData.applicationData.bViewToolbar);
        }

        private void ViewToolStrip(Boolean bValue)
        {
            mainToolStrip.Visible = bValue;
            toolBarToolStripMenuItem.Checked = bValue;
            numericUpDownAngle.Visible = toolBarToolStripMenuItem.Checked;

            //            formErrorMessage.Visible = !bValue;
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

        private void pictureBoxImageSmall_Paint(object sender, PaintEventArgs e)
        {
            Graphics grfx = e.Graphics;

            if (m_bmp != null)
            {
                try
                {
                    lock (m_lLockBMP) grfx.DrawImage(m_bmp, 0, 0, pictureBoxImageSmall.Width, pictureBoxImageSmall.Height);
                    //Rectungle

                    Rectangle rect = new Rectangle(
                            new Point(
                            (int)Math.Floor(-imageSplitContainer.Panel2.AutoScrollPosition.X * pictureBoxImageSmall.Height / (float)Math.Min(pictureBoxData.Height, imageSplitContainer.Panel2.Height)),
                            (int)Math.Floor(-imageSplitContainer.Panel2.AutoScrollPosition.Y * pictureBoxImageSmall.Width / (float)Math.Min(pictureBoxData.Width, imageSplitContainer.Panel2.Width))),
                            new System.Drawing.Size(
                            (int)Math.Floor(pictureBoxImageSmall.Width * Math.Min(pictureBoxData.Width, imageSplitContainer.Panel2.Width) / (float)pictureBoxData.Width),
                            (int)Math.Floor(pictureBoxImageSmall.Height * Math.Min(pictureBoxData.Height, imageSplitContainer.Panel2.Height) / (float)pictureBoxData.Height)));

                    grfx.DrawRectangle(m_PenCentroid, rect);
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
            int iHeightProfile = (int)(iHeight / 4f);

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
            //Power
            m_sysData.powerData.fSensFactor = 1.0f;
            m_sysData.powerData.fSensitivity = 1f;
            m_sysData.powerData.currentFilter = 3;
            m_sysData.powerData.PowerUnits = 0;

            m_sysData.applicationData.bSaveExit = true;
            m_sysData.applicationData.bViewStatusBar = true;
            m_sysData.applicationData.bViewToolbar = true;

            //Profile
            m_sysData.ProfileType = BeamOnCL.BeamOnCL.TypeProfile.tpLIne;
            m_sysData.LineProfileType = BeamOnCL.BeamOnCL.TypeLineProfile.tpLineCentroid;

            m_sysData.ClipLevels.SetLevel(0, 13.5M);
            m_sysData.ClipLevels.SetLevel(1, 50M);
            m_sysData.ClipLevels.SetLevel(2, 80M);

            //Wavelenght
            m_sysData.powerData.uiWavelenghtMin = 350;
            m_sysData.powerData.uiWavelenghtMax = 1100;
            m_sysData.powerData.uiWavelenght = 633;

            //Log
            m_sysData.logData.LogInterval = 1;
            m_sysData.logData.LogDuration = 5;
            m_sysData.logData.ltMode = LogType.ltTime;
            m_sysData.logData.ftFile = FileType.ftLog;
            m_sysData.logData.strFileName = m_sysData.applicationData.m_strMyDataDir + "\\" + m_sysData.applicationData.ProductName + ".log";

            //Fast Mode
            m_sysData.fastModeData.LogDuration = 5;
            m_sysData.fastModeData.ltMode = LogType.ltTime;
            m_sysData.fastModeData.ftFile = FileType.ftLog;
            m_sysData.fastModeData.strFileName = m_sysData.applicationData.m_strMyDataDir + "\\" + m_sysData.applicationData.ProductName + ".fst";

            m_sysData.videoDeviceData.pixelFormat = PixelFormat.Format24bppRgb;
            m_sysData.Measure = true;
            m_sysData.applicationData.bViewDataPanel = true;
            m_sysData.applicationData.bViewControlPanel = true;
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
            toolStripStatuslblClip.Text = "Clip Level: " + m_sysData.ClipLevels.Level(0).ToString("f1") + "%";
        }

        void pictureBoxData_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                if (iLevelCurrent == -1)
                {
                    if ((m_sysData.ProfileType == BeamOnCL.BeamOnCL.TypeProfile.tpLIne) && (m_sysData.LineProfileType == BeamOnCL.BeamOnCL.TypeLineProfile.tpLineFree)) bm.CrossPosition = new Point(e.X, e.Y);
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

                pictureBoxImageSmall.Size = new System.Drawing.Size(pictureBoxImageSmall.Width, (int)Math.Floor(pictureBoxImageSmall.Width * (pictureBoxImage.Height / (float)pictureBoxImage.Width)));

                m_bmp = new Bitmap(pictureBoxImage.Width, pictureBoxImage.Height, picturePaletteImage.Format);

                if (picturePaletteImage.Format == PixelFormat.Format8bppIndexed) m_bmp.Palette = picturePaletteImage.Palette;

                pointSensorCenter = new Point((int)(bm.MaxImageRectangle.Width / 2) - bm.ImageRectangle.X, (int)(bm.MaxImageRectangle.Height / 2) - (int)bm.ImageRectangle.Y);

                if (m_frm3D != null) m_frm3D.SensorCenterPosition = pointSensorCenter;
            }
        }

        void pictureBoxData_Paint(object sender, PaintEventArgs e)
        {
            Graphics grfx = e.Graphics;

            if (m_sysData.Measure == true)
            {
                //Centroid
                Point OldPoint = new Point((int)bm.PixelCentroid.X - 20, (int)bm.PixelCentroid.Y - 20);
                Point NewPoint = new Point((int)bm.PixelCentroid.X + 20, (int)bm.PixelCentroid.Y + 20);

                grfx.DrawLine(m_PenCentroid, OldPoint, NewPoint);

                OldPoint = new Point((int)bm.PixelCentroid.X - 20, (int)bm.PixelCentroid.Y + 20);
                NewPoint = new Point((int)bm.PixelCentroid.X + 20, (int)bm.PixelCentroid.Y - 20);

                grfx.DrawLine(m_PenCentroid, OldPoint, NewPoint);

                //Sensor Center
                OldPoint = new Point(0, pointSensorCenter.Y);
                NewPoint = new Point(bm.ImageRectangle.Width, pointSensorCenter.Y);

                grfx.DrawLine(m_PenSensor, OldPoint, NewPoint);

                OldPoint = new Point(pointSensorCenter.X, 0);
                NewPoint = new Point(pointSensorCenter.X, bm.ImageRectangle.Height);

                grfx.DrawLine(m_PenSensor, OldPoint, NewPoint);

                //
                int iiHeight = Math.Min(pictureBoxData.Height, imageSplitContainer.Panel2.Height);

                int iiShiftY = 0;
                if (imageSplitContainer.Panel2.Height < pictureBoxData.Height)
                {
                    iiShiftY += imageSplitContainer.Panel2.AutoScrollPosition.Y;
                    if (imageSplitContainer.Panel2.Width < pictureBoxData.Width) iiShiftY += 20;
                }

                int iiShiftX = 0;
                if (imageSplitContainer.Panel2.Width < pictureBoxData.Width)
                {
                    iiShiftX += -imageSplitContainer.Panel2.AutoScrollPosition.X;
                }

                grfx.DrawString("0" + ((m_sysData.UnitMeasure == MeasureUnits.muMicro) ? "(µm)" : "(mrad)"), myFont, CentroidSensorBrush, new PointF(iiShiftX + 5, pointSensorCenter.Y), m_strfrm);
                grfx.DrawString("0" + ((m_sysData.UnitMeasure == MeasureUnits.muMicro) ? "(µm)" : "(mrad)"), myFont, CentroidSensorBrush, new PointF(pointSensorCenter.X, iiHeight - iiShiftY - 20), m_strfrm);

                //Ellipse
                if (plArea != null) grfx.DrawLines(m_PenEllipse, plArea);

                //Main ROI
                //grfx.DrawRectangle(m_PenCentroid, bm.WorkingArea);

                //Line Profile Line
                if (bm.typeProfile == BeamOnCL.BeamOnCL.TypeProfile.tpLIne)
                {
                    grfx.DrawLine(m_PenLineProfile, bm.lineProfileHorizontalLeft, bm.lineProfileHorizontalRight);
                    grfx.DrawLine(m_PenLineProfile, bm.lineProfileVerticalLeft, bm.lineProfileVerticalRight);
                }

                DrawProfile(m_profileVertical, m_PenGrid, DrawOrientation.doVertical, grfx);
                DrawProfile(m_profileHorizontal, m_PenGrid, DrawOrientation.doHorizontal, grfx);

                if (m_sysData.applicationData.bViewDataPanel == false)
                {
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
        }

        public void DrawProfile(BeamOnCL.Profile dataProfile, Pen pen, DrawOrientation drawOrientation, Graphics grfx)
        {
            Point OldPoint = new Point();
            Point NewPoint;

            Point OldPointGaussian = new Point();
            Point NewPointGaussian;

            int iHeight = Math.Min(pictureBoxData.Height, imageSplitContainer.Panel2.Height);
            //int iHeightProfile = (int)(iHeight / 3f);
            int iHeightProfile = (int)(iHeight / 4f);

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

                    int iShiftX = imageSplitContainer.Panel2.Width - 60;
                    if (imageSplitContainer.Panel2.Width < pictureBoxData.Width) iShiftX += -imageSplitContainer.Panel2.AutoScrollPosition.X;

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

                        grfx.DrawString(m_sysData.ClipLevels.Level(0).ToString("f1") + "%", myFont, m_sysData.ClipLevels.LevelBrush(0), new PointF(iShiftX, iLineLevel1 - 15), m_strfrm);
                        grfx.DrawString(m_sysData.ClipLevels.Level(1).ToString("f1") + "%", myFont, m_sysData.ClipLevels.LevelBrush(1), new PointF(iShiftX, iLineLevel2 - 15), m_strfrm);
                        grfx.DrawString(m_sysData.ClipLevels.Level(2).ToString("f1") + "%", myFont, m_sysData.ClipLevels.LevelBrush(2), new PointF(iShiftX, iLineLevel3 - 15), m_strfrm);
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

                        grfx.DrawString(m_sysData.ClipLevels.Level(0).ToString("f1") + "%", myFont, m_sysData.ClipLevels.LevelBrush(0), new PointF(iLineLevel1, 0), m_strfrmVertical);
                        grfx.DrawString(m_sysData.ClipLevels.Level(1).ToString("f1") + "%", myFont, m_sysData.ClipLevels.LevelBrush(1), new PointF(iLineLevel2, 0), m_strfrmVertical);
                        grfx.DrawString(m_sysData.ClipLevels.Level(2).ToString("f1") + "%", myFont, m_sysData.ClipLevels.LevelBrush(2), new PointF(iLineLevel3, 0), m_strfrmVertical);
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
                    pictureBoxImageSmall.Invalidate();
                    toolStripStatusLabelTimeStamp.Text = (1000f / (double)Timestamp).ToString("#.000") + " fps";
                    toolStripStatuslblDate.Text = DateTime.Now.ToString();
                    toolStripStatuslblWave.Text = "Wavelength: " + m_sysData.powerData.uiWavelenght + "nm";
                    toolStripStatuslblFilter.Text = "Filter: " + m_strFilterName[m_sysData.powerData.currentFilter];
                    toolStripStatuslblClip.Text = "Clip Level: " + m_sysData.ClipLevels.Level(0).ToString("f1") + "%";
                    toolStripStatuslblAverage.Text = "Average: " + ((m_sysData.AverageOn == true) ? m_sysData.Average.ToString() : "Off");
                    toolStripStatusLabelTypeProfile.Text = (m_sysData.ProfileType == BeamOnCL.BeamOnCL.TypeProfile.tpSum) ? "Sum" : "Line " + ((m_sysData.LineProfileType == BeamOnCL.BeamOnCL.TypeLineProfile.tpLineCentroid) ? "Centroid" : "Free");
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

                    if (m_sysData.SnapshotView == false) bm.pixelFormat = pixelFormat;

                    if (m_frm3D != null) m_frm3D.SetColorPalette(color);

                    toolStripStatusLabelPixelFormat.Text = (m_sysData.videoDeviceData.pixelFormat == PixelFormat.Format8bppIndexed) ? "8 bpp" : "12 bpp";
                });
            }
            catch
            {
            }
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            DefaultSettings();

            m_sysData.DeserializeAppData(m_sysData.applicationData.m_strMyDataDir + "\\$$$.ini");

            if (bm.Start(m_sysData.videoDeviceData.pixelFormat) == true)
            {
                m_sysData.powerData.ReadSensitivitySensor(m_sysData.applicationData.m_strMyAppDir + "\\" + bm.SerialNumber + ".xml");

                m_sysData.powerData.SetFilterSensitivityFile(0, m_sysData.applicationData.m_strMyAppDir, bm.SerialNumber + ".nd8");
                m_sysData.powerData.SetFilterSensitivityFile(1, m_sysData.applicationData.m_strMyAppDir, bm.SerialNumber + ".nd64");
                m_sysData.powerData.SetFilterSensitivityFile(2, m_sysData.applicationData.m_strMyAppDir, bm.SerialNumber + ".nd200");
                m_sysData.powerData.SetFilterSensitivityFile(3, m_sysData.applicationData.m_strMyAppDir, bm.SerialNumber + ".nd1000");

                m_sysData.powerData.ReadSAMFile();

                bm.CameraFilter = m_sysData.powerData.currentFilter;

                pImageWhellPosition.Offset(-m_sysData.powerData.currentFilter * 5 * rImageWhellRectangle.Width, 0);
                if (pImageWhellPosition.X <= -(imageWhell.Width - 1)) pImageWhellPosition.X = 0;
                if (pImageWhellPosition.X >= 1) pImageWhellPosition.X = -(imageWhell.Width - 1 - rImageWhellRectangle.Width);

                m_sysData.powerData.ReadFilterFile(bm.CameraFilter);

                m_sysData.powerData.SetSensitivity();
                m_sysData.powerData.SetFilterFactor();
                m_sysData.powerData.SetSAMFactor();

                m_sw = Stopwatch.StartNew();

                picturePaletteImage.Format = m_sysData.videoDeviceData.pixelFormat;

                pictureBoxImage.Size = bm.ImageRectangle.Size;

                pictureBoxImageSmall.Size = new System.Drawing.Size(pictureBoxImageSmall.Width, (int)Math.Floor(pictureBoxImageSmall.Width * (pictureBoxImage.Height / (float)pictureBoxImage.Width)));

                m_bmp = new Bitmap(bm.ImageRectangle.Width, bm.ImageRectangle.Height, picturePaletteImage.Format);

                if (m_bmp.PixelFormat == PixelFormat.Format8bppIndexed)
                    m_bmp.Palette = picturePaletteImage.Palette;
                else
                    Color = picturePaletteImage.colorArray;

                toolStripStatusLabelPixelFormat.Text = (m_sysData.videoDeviceData.pixelFormat == PixelFormat.Format8bppIndexed) ? "8 bpp" : "12 bpp";

                bm.pixelFormat = picturePaletteImage.Format;

                sumProfileToolStripMenuItem.Checked = (m_sysData.ProfileType == BeamOnCL.BeamOnCL.TypeProfile.tpSum);
                lineProfileToolStripMenuItem.Checked = !sumProfileToolStripMenuItem.Checked;

                lineProfileToolStripButton.Checked = lineProfileToolStripMenuItem.Checked;
                sumProfileToolStripButton.Checked = sumProfileToolStripMenuItem.Checked;

                AngleLineProfileToolStripLabel.Enabled = lineProfileToolStripButton.Checked;
                numericUpDownAngle.Enabled = lineProfileToolStripButton.Checked;

                typeLineProfileToolStripMenuItem.Enabled = lineProfileToolStripButton.Checked;
                centroidToolStripMenuItem.Checked = (m_sysData.LineProfileType == BeamOnCL.BeamOnCL.TypeLineProfile.tpLineCentroid);
                freeLineToolStripMenuItem.Checked = (m_sysData.LineProfileType == BeamOnCL.BeamOnCL.TypeLineProfile.tpLineFree);

                bm.lineProfileAngle = m_sysData.lineProfileAngle;

                bm.typeProfile = m_sysData.ProfileType;

                bm.LineProfileType = m_sysData.LineProfileType;

                bm.Binning = m_sysData.videoDeviceData.uiBinning;

                if (bm.Binning == m_sysData.videoDeviceData.uiBinning)
                {
                    m_bmp = new Bitmap(bm.ImageRectangle.Width, bm.ImageRectangle.Height, picturePaletteImage.Format);

                    if (m_bmp.PixelFormat == PixelFormat.Format8bppIndexed) m_bmp.Palette = picturePaletteImage.Palette;

                    pictureBoxImage.Size = bm.ImageRectangle.Size;

                    pictureBoxImageSmall.Size = new System.Drawing.Size(pictureBoxImageSmall.Width, (int)Math.Floor(pictureBoxImageSmall.Width * (pictureBoxImage.Height / (float)pictureBoxImage.Width)));
                }

                trackBarGain.Maximum = bm.MaxGain;
                trackBarGain.Minimum = bm.MinGain;
                trackBarGain.Value = bm.Gain;
                labelGainMin.Text = trackBarGain.Minimum.ToString();
                labelGainMax.Text = trackBarGain.Maximum.ToString();
                labelGainValue.Text = trackBarGain.Value + " dB";

                trackBarExposure.Maximum = bm.MaxExposure / 1000;
                trackBarExposure.Minimum = bm.MinExposure;
                trackBarExposure.Value = bm.Exposure;
                labelExposureMin.Text = trackBarExposure.Minimum.ToString();
                labelExposureMax.Text = trackBarExposure.Maximum.ToString();
                labelExposureValue.Text = trackBarExposure.Value + " µs";

                measuringToolStripMenuItem.Checked = m_sysData.Measure;
                measuringToolStripButton.Checked = m_sysData.Measure;

                typeProfileToolStripMenuItem.Enabled = measuringToolStripMenuItem.Checked;
                sumProfileToolStripButton.Enabled = measuringToolStripMenuItem.Checked;
                lineProfileToolStripButton.Enabled = measuringToolStripMenuItem.Checked;
                typeLineProfileToolStripMenuItem.Enabled = lineProfileToolStripButton.Checked;

                groupBoxPosition.Enabled = m_sysData.Measure;
                ProfileGroupBox.Enabled = m_sysData.Measure;
                gaussGroupBox.Visible = m_sysData.ViewGaussian;
                gaussGroupBox.Enabled = m_sysData.Measure && m_sysData.ViewGaussian;
                powerGroupBox.Enabled = m_sysData.Measure;

                pointSensorCenter = new Point((int)(bm.MaxImageRectangle.Width / 2) - bm.ImageRectangle.X, (int)(bm.MaxImageRectangle.Height / 2) - (int)bm.ImageRectangle.Y);

                dataCheckBox.Checked = m_sysData.applicationData.bViewDataPanel;
                propertyCheckBox.Checked = m_sysData.applicationData.bViewControlPanel;
                viewCheckBox.Checked = m_sysData.applicationData.bViewImagePanel;

                ChangeViewPanels();

                mnuOptionsSaveSettingsOnExit.Checked = m_sysData.applicationData.bSaveExit;
                ViewToolStrip(m_sysData.applicationData.bViewToolbar);

                statusBarToolStripMenuItem.Checked = m_sysData.applicationData.bViewStatusBar;
                mainStatusStrip.Visible = statusBarToolStripMenuItem.Checked;

                UpdateViewUnits();
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

            m_strSystemTitle = m_sysData.applicationData.ProductName;
            m_sysData.applicationData.SystemNumber = bm.UserDefinedName;
            UpdateView();

            MenuToolEnabled();
        }

        private void FormMain_Resize(object sender, EventArgs e)
        {
            if (dataSplitContainer.Panel1Collapsed == false) dataSplitContainer.SplitterDistance = dataSplitContainer.Panel1MinSize;
        }

        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {

            if (m_sysData.SnapshotView == true)
            {
                StopSnapshot();

                e.Cancel = true;
            }
            else
            {
                if (m_sysData.applicationData.bSaveExit == true) m_sysData.SerializeAppData(m_sysData.applicationData.m_strMyDataDir + "\\$$$.ini");

                bm.Stop();
            }
        }

        private void trackBarTransparency_Scroll(object sender, EventArgs e)
        {
            //            this.pictureBoxData.BackColor = System.Drawing.Color.FromArgb(trackBarTransparency.Value, pictureBoxData.BackColor.R, pictureBoxData.BackColor.G, pictureBoxData.BackColor.B);
            this.pictureBoxData.BackColor = System.Drawing.Color.FromArgb(trackBarTransparency.Value, 0, 0, 0);
        }

        private void trackBarGain_Scroll(object sender, EventArgs e)
        {
            bm.Gain = trackBarGain.Value;
            labelGainValue.Text = trackBarGain.Value + " dB";
        }

        private void trackBarExposure_Scroll(object sender, EventArgs e)
        {
            bm.Exposure = trackBarExposure.Value;
            labelExposureValue.Text = trackBarExposure.Value + " µs";
        }

        private void typeProfileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripItem tsb = (ToolStripItem)sender;

            sumProfileToolStripMenuItem.Checked = ((tsb.Name.Contains("sum") == true) && (bm.typeProfile != BeamOnCL.BeamOnCL.TypeProfile.tpSum));
            lineProfileToolStripMenuItem.Checked = !sumProfileToolStripMenuItem.Checked;

            sumProfileToolStripButton.Checked = sumProfileToolStripMenuItem.Checked;
            lineProfileToolStripButton.Checked = lineProfileToolStripMenuItem.Checked;

            m_sysData.ProfileType = (sumProfileToolStripMenuItem.Checked == true) ? BeamOnCL.BeamOnCL.TypeProfile.tpSum : BeamOnCL.BeamOnCL.TypeProfile.tpLIne;
            bm.typeProfile = m_sysData.ProfileType;

            AngleLineProfileToolStripLabel.Enabled = lineProfileToolStripButton.Checked;
            numericUpDownAngle.Enabled = lineProfileToolStripButton.Checked;

            typeLineProfileToolStripMenuItem.Enabled = lineProfileToolStripButton.Checked;
        }

        private void dataSplitContainer_Panel2_Paint(object sender, PaintEventArgs e)
        {
            if (m_sysData.Measure == true)
            {
                labelPositionXValue.Text = GetStringFormat(m_sysData.positionData.PositionX);
                labelPositionYValue.Text = GetStringFormat(m_sysData.positionData.PositionY);

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

                labelPower.Text = m_sysData.powerData.ToString();
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
            frmAboutBox aboutBox = new frmAboutBox(bm.UserDefinedName);
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

        private Image GetImageForm(Rectangle scrR)
        {
            Bitmap MyImage = new Bitmap(scrR.Width, scrR.Height);
            using (Graphics g = Graphics.FromImage(MyImage))
                g.CopyFromScreen(new Point(scrR.Left, scrR.Top), Point.Empty, new Size(scrR.Width, scrR.Height));

            //m_SystemMessage |= (UInt16)SystemStatus.M_SS_SAVEDATA;

            return MyImage;
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

        private void UpdateViewUnits()
        {
            groupBoxPosition.Text = (m_sysData.UnitMeasure == MeasureUnits.muMicro) ? "Position(µm)" : "Position(mrad)";
            ProfileGroupBox.Text = (m_sysData.UnitMeasure == MeasureUnits.muMicro) ? "Profile(µm)" : "Profile(mrad)";
            gaussGroupBox.Text = (m_sysData.UnitMeasure == MeasureUnits.muMicro) ? "Gaussian(µm)" : "Gaussian(mrad)";

            powerGroupBox.Text = "Power(" + PowerData.bufUnits[m_sysData.powerData.PowerUnits] + ")";
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
            gaussGroupBox.Visible = m_sysData.ViewGaussian;
            powerGroupBox.Enabled = m_sysData.Measure;

            typeProfileToolStripMenuItem.Enabled = measuringToolStripMenuItem.Checked;
            sumProfileToolStripButton.Enabled = measuringToolStripMenuItem.Checked;
            lineProfileToolStripButton.Enabled = measuringToolStripMenuItem.Checked;

            tbOptionsSetupDataCollection.Enabled = measuringToolStripMenuItem.Checked;
            mnuFileSetupDataCollection.Enabled = measuringToolStripMenuItem.Checked;
            tbOptionsStartDataCollection.Checked = measuringToolStripMenuItem.Checked;
            mnuFileStartDataCollection.Checked = measuringToolStripMenuItem.Checked;

            mnuFileSetupRunningMode.Enabled = measuringToolStripMenuItem.Checked;
            runningSetupToolStripButton.Enabled = measuringToolStripMenuItem.Checked;
            runningToolStripButton.Checked = measuringToolStripMenuItem.Checked;
            mnuFileStartRunningMode.Checked = measuringToolStripMenuItem.Checked;
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
                mnuFileStartDataCollection.Image = global::BeamOn_U3.Properties.Resources.Hand;
                mnuFileStartDataCollection.Text = "&Stop Log File";
                mnuFileStartDataCollection.ToolTipText = "Stop Log File";

                m_fldLog = new FileLogData();
                m_fldLog.OnStopLogFile += new FileLogData.StopLogFile(m_fldLog_OnStopLogFile);
                m_fldLog.CreateHeader();

                m_SystemMessage |= (UInt16)SystemStatus.M_SS_LOG;
            }
            else
            {
                mnuFileStartDataCollection.Checked = false;
                mnuFileStartDataCollection.Image = global::BeamOn_U3.Properties.Resources.Start;
                mnuFileStartDataCollection.Text = "&Start Log File";
                mnuFileStartDataCollection.ToolTipText = "Start Log File";

                if ((m_fldLog != null) && (m_fldLog.IsOpen() == true)) m_fldLog.CloseLog();

                m_fldLog = null;

                m_SystemMessage = (UInt16)(m_SystemMessage & (~(int)SystemStatus.M_SS_LOG));
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
                    this.mnuFileStartDataCollection.Image = global::BeamOn_U3.Properties.Resources.Start;
                    this.mnuFileStartDataCollection.Text = "Start Lo&g";
                    this.mnuFileStartDataCollection.ToolTipText = "Start Log";

                    this.tbOptionsStartDataCollection.ToolTipText = this.mnuFileStartDataCollection.ToolTipText;
                    this.tbOptionsStartDataCollection.Image = this.mnuFileStartDataCollection.Image;
                    this.tbOptionsStartDataCollection.Text = this.mnuFileStartDataCollection.Text;
                    this.tbOptionsStartDataCollection.Checked = this.mnuFileStartDataCollection.Checked;

                    this.mnuFileSetupDataCollection.Enabled = !this.mnuFileStartDataCollection.Checked;
                    this.tbOptionsSetupDataCollection.Enabled = this.mnuFileSetupDataCollection.Enabled;

                    m_SystemMessage = (UInt16)(m_SystemMessage & (~(int)SystemStatus.M_SS_LOG));

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
                    formErrorMessage.SystemMessage = m_SystemMessage;

                    toolStripStatuslblError.LogTypeMessage = (Byte)m_sysData.logData.ftFile;
                    formErrorMessage.LogTypeMessage = (Byte)m_sysData.logData.ftFile;

                    if (((formErrorMessage.SystemMessage != 0) || (formErrorMessage.ErrorMessage != ErrorStatus.BA_OK) || (formErrorMessage.DemoVersion == true)) && (mainToolStrip.Visible == false))
                    {
                        formErrorMessage.Visible = true;
                        formErrorMessage.Focus();
                    }
                    else
                        formErrorMessage.Visible = false;
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
                m_frm3D.SensorCenterPosition = pointSensorCenter;
                m_frm3D.SetScaleGridX(200, 117f);
                m_frm3D.SetScaleGridY(200, 117f);
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

        private void propertyToolStripButton_Click(object sender, EventArgs e)
        {
            FormSetup frmSetup = new FormSetup();

            if (frmSetup.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (bm.Binning != m_sysData.videoDeviceData.uiBinning)
                {
                    bm.Binning = m_sysData.videoDeviceData.uiBinning;

                    if (bm.Binning == m_sysData.videoDeviceData.uiBinning)
                    {
                        m_bmp = new Bitmap(bm.ImageRectangle.Width, bm.ImageRectangle.Height, picturePaletteImage.Format);

                        if (m_bmp.PixelFormat == PixelFormat.Format8bppIndexed) m_bmp.Palette = picturePaletteImage.Palette;

                        pictureBoxImage.Size = bm.ImageRectangle.Size;

                        pictureBoxImageSmall.Size = new System.Drawing.Size(pictureBoxImageSmall.Width, pictureBoxImageSmall.Width * (pictureBoxImage.Height / pictureBoxImage.Width));
                    }
                }

                if (bm.pixelFormat != m_sysData.videoDeviceData.pixelFormat)
                {
                    picturePaletteImage.Format = m_sysData.videoDeviceData.pixelFormat;
                }

                gaussGroupBox.Enabled = m_sysData.Measure && m_sysData.ViewGaussian;
                gaussGroupBox.Visible = m_sysData.ViewGaussian;

                bm.AverageNum = (m_sysData.AverageOn == true) ? m_sysData.Average : (UInt16)1;

                UpdateViewUnits();
            }
        }

        private void pictureBoxFilterWhell_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            g.DrawImage(imageWhell, pImageWhellPosition);
        }

        private void SetTimer()
        {
            whellTimer = new System.Timers.Timer(100);
            whellTimer.Elapsed += new System.Timers.ElapsedEventHandler(whellTimer_Elapsed);
            whellTimer.AutoReset = true;
            whellTimer.Enabled = true;
        }

        void whellTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (m_iWhellStep > 0)
            {
                pImageWhellPosition.Offset(m_iWhellDirection * rImageWhellRectangle.Width, 0);
                if (pImageWhellPosition.X <= -(imageWhell.Width - 1)) pImageWhellPosition.X = 0;
                if (pImageWhellPosition.X >= 1) pImageWhellPosition.X = -(imageWhell.Width - 1 - rImageWhellRectangle.Width);
                m_iWhellStep--;
            }
            else
            {
                whellTimer.Stop();
                whellTimer.Dispose();
                whellTimer = null;
            }

            pictureBoxFilterWhell.Invalidate();
        }

        private void pictureBoxFilterWhell_MouseLeave(object sender, EventArgs e)
        {
            panelFilterName.Visible = false;
            pictureBoxFilterWhell.Cursor = Cursors.Default;
        }

        void pictureBoxFilterWhell_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            int size2filter = 0;
            int i = 0;

            if (whellTimer == null)
            {
                for (i = 0; i < m_pFilterPosition.Length; i++)
                {
                    size2filter = (int)Math.Floor(Math.Sqrt(Math.Pow(Math.Abs(m_pFilterPosition[i].X - e.X), 2) + Math.Pow(Math.Abs(m_pFilterPosition[i].Y - e.Y), 2)));
                    if (size2filter <= m_iFilterRadius)
                    {
                        m_iWhellDirection = (i == 3) ? 1 : ((i == 0) ? 0 : -1);
                        m_iWhellStep = (i == 2) ? 10 : 5;
                        int iPos = (int)((pImageWhellPosition.X / rImageWhellRectangle.Width + m_iWhellStep * m_iWhellDirection) / 5);
                        if (iPos < -3) iPos += 4;
                        if (iPos > 0) iPos -= 4;

                        panelFilterName.Location = new Point(e.X + 30, e.Y + 40);
                        pictureBoxFilterWhell.Cursor = Cursors.Hand;

                        labelFilterName.Text = m_strFilterName[(UInt16)Math.Abs(iPos)];

                        break;
                    }
                }

                panelFilterName.Visible = (i != m_pFilterPosition.Length);
                pictureBoxFilterWhell.Cursor = (i != m_pFilterPosition.Length) ? Cursors.Hand : Cursors.Default;
            }
        }

        private void pictureBoxFilterWhell_MouseClick(object sender, MouseEventArgs e)
        {
            int size2filter = 0;

            if ((m_sysData.SnapshotView == false) && ((e.Button == System.Windows.Forms.MouseButtons.Left) && (whellTimer == null)))
            {
                panelFilterName.Visible = false;
                pictureBoxFilterWhell.Cursor = Cursors.Default;

                if (whellTimer == null)
                {
                    for (int i = 1; i < m_pFilterPosition.Length; i++)
                    {
                        size2filter = (int)Math.Floor(Math.Sqrt(Math.Pow(Math.Abs(m_pFilterPosition[i].X - e.X), 2) + Math.Pow(Math.Abs(m_pFilterPosition[i].Y - e.Y), 2)));
                        if (size2filter <= m_iFilterRadius)
                        {
                            m_iWhellDirection = (i == 3) ? 1 : -1;
                            m_iWhellStep = (i == 2) ? 10 : 5;
                            int iPos = (int)((pImageWhellPosition.X / rImageWhellRectangle.Width + m_iWhellStep * m_iWhellDirection) / 5);
                            if (iPos < -3) iPos += 4;
                            if (iPos > 0) iPos -= 4;

                            m_sysData.powerData.currentFilter = (UInt16)Math.Abs(iPos);
                            bm.CameraFilter = m_sysData.powerData.currentFilter;
                            SetTimer();

                            m_sysData.powerData.ReadFilterFile(bm.CameraFilter);
                            m_sysData.powerData.SetFilterFactor();

                            break;
                        }
                    }
                }
            }
        }

        private void buttonProperty_Click(object sender, EventArgs e)
        {
            m_sysData.applicationData.bViewControlPanel = !m_sysData.applicationData.bViewControlPanel;

            dataSplitContainer.Panel1Collapsed = !m_sysData.applicationData.bViewControlPanel;
            dataSplitContainer.SplitterDistance = dataSplitContainer.Panel1MinSize;
            propertyBoxToolStripMenuItem.Checked = !dataSplitContainer.Panel1Collapsed;
        }

        private void powerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Do you want to start power calibration?", "Power calibration", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.Yes)
            {
                m_bPowerCalibration = true;
                m_iPowerCalibrationCount = POWER_CALIBRATION_NUM;

                m_formPowerCalibration = new FormPowerCalibration();
                if (m_formPowerCalibration.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    m_sysData.powerData.SetSensitivityFactor(m_formPowerCalibration.PowerCalibration, (float)m_dAveragePowerDataSum);
                    m_sysData.powerData.PowerCalibr.InitializePowerCalibration(bm.Exposure, bm.Gain);
                    m_bPowerCalibration = false;
                    m_sysData.powerData.SetSensitivity();
                }
            }
        }

        private void mnuOptionsUserData_Click(object sender, EventArgs e)
        {
            FormUserData FormUserData = new FormUserData();

            if (FormUserData.ShowDialog() == DialogResult.OK) UpdateView();
        }

        private void UpdateView()
        {

            this.Text = m_strSystemTitle;

            //this.Text += " " + bm.UserDefinedName;
            //if (m_sysData.linkData.Status == LinkStatus.lsClientMode)
            //    this.Text += " (Client Mode)";

            if (m_sysData.SnapshotView == true) this.Text += " (Snapshot View)";

            if ((m_sysData.applicationData.m_strUserTitle != null) && (m_sysData.applicationData.m_strUserTitle.Equals("") == false))
                this.Text += " [" + m_sysData.applicationData.m_strUserTitle + "]";
        }

        private void dumaOptronicsOnTheWebToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem tsm = (ToolStripMenuItem)sender;

            try
            {
                if (String.CompareOrdinal(tsm.Name, "dumaOptronicsOnTheWebToolStripMenuItem") == 0)
                    System.Diagnostics.Process.Start("http://www.duma.co.il");
                else if (String.CompareOrdinal(tsm.Name, "mnuHelpMellesOnTheWeb") == 0)
                    System.Diagnostics.Process.Start("http://www.cvimellesgriot.com");
                else if (String.CompareOrdinal(tsm.Name, "mnuHelpCoherentOnTheWeb") == 0)
                    System.Diagnostics.Process.Start("http://www.coherent.com");
                else if (String.CompareOrdinal(tsm.Name, "mnuHelpOptoSigmaOnTheWeb") == 0)
                    System.Diagnostics.Process.Start("http://www.optosigma.com");
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show(ex + "Unable to open link.", "System Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void warrantyExtensionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start("http://www.duma.co.il/contact-us/warranty-extension.html");
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show(ex + "Unable to open link.", "System Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void saveImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Rectangle rect = this.Bounds;

            if ((rect.Width != 0) && (rect.Height != 0))
            {
                Thread CaptureImage = new Thread(CaptureWindowImage);
                CaptureImage.TrySetApartmentState(ApartmentState.STA);

                CaptureImage.Start(rect);
            }
        }

        private void CaptureWindowImage(object rect)
        {
            Thread.Sleep(500);

            SaveImageFile(GetImageForm((Rectangle)rect));
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
            saveImageFile.FileOk += new CancelEventHandler(ImageFileDialog_FileOk);
            saveImageFile.Title = "Save Image Data";

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

                SaveSnapshotFile(saveImageFile.FileName + ".snp");
            }
        }

        private void saveSnapshotToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveSnapshotFile = new SaveFileDialog();

            saveSnapshotFile.Filter = "Snapshot files (*.snp)|*.snp|All Files(*.*)|*.*";
            saveSnapshotFile.FileName = "*.snp";

            saveSnapshotFile.DefaultExt = "snp";
            //saveSnapshotFile.AddExtension = true;
            saveSnapshotFile.CheckPathExists = true;
            saveSnapshotFile.CheckFileExists = false;

            saveSnapshotFile.InitialDirectory = m_sysData.applicationData.m_strMyDataDir;
            saveSnapshotFile.RestoreDirectory = true;
            saveSnapshotFile.FileOk += new CancelEventHandler(SnapshotFile_FileOk);
            saveSnapshotFile.Title = "Save Snapshot File";

            if (saveSnapshotFile.ShowDialog() == DialogResult.OK) SaveSnapshotFile(saveSnapshotFile.FileName);
        }

        private void SnapshotFile_FileOk(object sender, CancelEventArgs e)
        {
            FileDialog SnapshotFileDialog = (FileDialog)sender;

            FileInfo fi = new FileInfo(SnapshotFileDialog.FileName);

            if (fi.Extension != ".snp")
            {
                CustomMessageBox.Show("Extension of the Snapshot File should be 'SNP'",
                                SnapshotFileDialog.Title, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

                e.Cancel = true;
            }
        }


        private Boolean SaveSnapshotFile(String strFileName)
        {
            Boolean bRet = false;

            FileStream fs = new FileStream(strFileName, FileMode.Create);
            BinaryWriter bw = new BinaryWriter(fs);

            try
            {
                bw.Write(m_sysData.applicationData.ProductName + " Snapshot");
                bw.Write(m_sysData.applicationData.SystemNumber);
                bw.Write(m_sysData.powerData.uiWavelenght);
                bw.Write(m_sysData.powerData.currentFilter);
                bw.Write(m_sysData.powerData.realFilterFactor);

                bw.Write(m_sysData.powerData.bIndSAM);
                if (m_sysData.powerData.bIndSAM == true)
                {
                    bw.Write(m_sysData.powerData.strSAMName);
                    bw.Write(m_sysData.powerData.currentSAMFactor);
                }

                bw.Write(bm.Exposure);
                bw.Write(bm.Gain);
                bw.Write(m_sysData.powerData.PowerCalibr.PowerCalibrationExposure);
                bw.Write(m_sysData.powerData.PowerCalibr.PowerCalibrationGain);

                bw.Write((Byte)((m_sysData.videoDeviceData.pixelFormat == PixelFormat.Format8bppIndexed) ? 8 : 12));
                bw.Write((UInt16)bm.ImageRectangle.Left);
                bw.Write((UInt16)bm.ImageRectangle.Top);
                bw.Write((UInt16)bm.ImageRectangle.Width);
                bw.Write((UInt16)bm.ImageRectangle.Height);
                bw.Write((UInt16)bm.MaxImageRectangle.Width);
                bw.Write((UInt16)bm.MaxImageRectangle.Height);

                for (int i = 0, iShift = 0; i < m_snapshot.Height; i++, iShift = i * m_snapshot.Width)
                {
                    for (int j = 0; j < m_snapshot.Width; j++, iShift++)
                    {
                        if (m_sysData.videoDeviceData.pixelFormat == PixelFormat.Format8bppIndexed)
                            bw.Write((Byte)m_snapshot.GetPixelColor(iShift));
                        else
                            bw.Write(m_snapshot.GetPixelColor(iShift));
                    }
                }

                bRet = true;
            }
            catch (Exception ex)
            {
                FileInfo fi = new FileInfo(strFileName);
                CustomMessageBox.Show(ex + "Unable to save file " + fi.Name + " .", "Save Snapshot File Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                bw.Close();
                fs.Close();
            }

            return bRet;
        }

        private void openSnapshotToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openSnapshotToolStripMenuItem.Checked == true)
            {
                OpenFileDialog openSnapshotFile = new OpenFileDialog();

                openSnapshotFile.Filter = "Snapshot files (*.snp)|*.snp";
                openSnapshotFile.FileName = "*.snp";

                openSnapshotFile.DefaultExt = "snp";
                openSnapshotFile.AddExtension = true;
                openSnapshotFile.CheckPathExists = true;
                openSnapshotFile.CheckFileExists = true;

                openSnapshotFile.InitialDirectory = m_sysData.applicationData.m_strMyDataDir;
                openSnapshotFile.RestoreDirectory = true;
                openSnapshotFile.FileOk += new CancelEventHandler(SnapshotFile_FileOk);
                openSnapshotFile.Title = "Open Snapshot File";

                if (openSnapshotFile.ShowDialog() == DialogResult.OK) StartSnapshot(openSnapshotFile.FileName);
            }
            else
            {
                StopSnapshot();
            }
        }

        private void StartUpdateSnapshotTimer()
        {
            m_tUpdateSnapshot = new System.Timers.Timer();
            m_tUpdateSnapshot.Elapsed += new System.Timers.ElapsedEventHandler(m_tUpdateSnapshot_Elapsed);
            m_tUpdateSnapshot.Interval = 50;
        }

        void m_tUpdateSnapshot_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (m_sysData.SnapshotView == true) bm_OnImageReceved(sender, new BeamOnCL.MeasureCamera.NewDataRecevedEventArgs(m_snapshot, false));
        }

        void StopSnapshot()
        {
            m_sysData.SnapshotView = false;

            if (m_tUpdateSnapshot != null) m_tUpdateSnapshot.Stop();

            m_SystemMessage = (UInt16)(m_SystemMessage & (~(int)SystemStatus.M_SS_SNAPSHOT));

            AddItemAsyncDelegate asyncSM = new AddItemAsyncDelegate(UpdateSystemMessage);
            asyncSM.BeginInvoke(null, null);

            bm.OnImageReceved += new BeamOnCL.BeamOnCL.ImageReceved(bm_OnImageReceved);

            FormMain_Load(this, new EventArgs());
        }

        void StartSnapshot(String strFileName)
        {
            if ((m_sysData.Simulation == false) && (m_sysData.SnapshotView == false))
            {
                bm.Stop();
            }

            m_sysData.SerializeAppData(m_sysData.applicationData.m_strMyDataDir + "\\$$$.ini");

            if (m_tUpdateSnapshot != null) m_tUpdateSnapshot.Stop();

            if (ReadSnapshotFile(strFileName) == true)
            {
                if (bm != null) bm.OnImageReceved -= new BeamOnCL.BeamOnCL.ImageReceved(bm_OnImageReceved);

                DefaultSettings();

                m_sysData.SnapshotView = true;

                if (m_tUpdateSnapshot == null) StartUpdateSnapshotTimer();
                m_tUpdateSnapshot.Start();

                m_SystemMessage |= (UInt16)SystemStatus.M_SS_SNAPSHOT;

                AddItemAsyncDelegate asyncSM = new AddItemAsyncDelegate(UpdateSystemMessage);
                asyncSM.BeginInvoke(null, null);
            }
            else
                StopSnapshot();
        }

        private Boolean ReadSnapshotFile(String strFileName)
        {
            Boolean bRet = false;

            FileStream fs = new FileStream(strFileName, FileMode.Open, FileAccess.Read);
            BinaryReader br = new BinaryReader(fs);

            try
            {
                String pn = br.ReadString();

                m_sysData.applicationData.SystemNumber = br.ReadString();
                m_sysData.powerData.uiWavelenght = br.ReadUInt16();

                m_sysData.powerData.currentFilter = br.ReadUInt16();
                pImageWhellPosition.Offset(-m_sysData.powerData.currentFilter * 5 * rImageWhellRectangle.Width, 0);
                if (pImageWhellPosition.X <= -(imageWhell.Width - 1)) pImageWhellPosition.X = 0;
                if (pImageWhellPosition.X >= 1) pImageWhellPosition.X = -(imageWhell.Width - 1 - rImageWhellRectangle.Width);

                m_sysData.powerData.realFilterFactor = br.ReadSingle();

                m_sysData.powerData.bIndSAM = br.ReadBoolean();
                if (m_sysData.powerData.bIndSAM == true)
                {
                    m_sysData.powerData.strSAMName = br.ReadString();
                    m_sysData.powerData.realSAMFactor = br.ReadSingle();
                    m_sysData.powerData.currentSAMFactor = m_sysData.powerData.realSAMFactor;
                }
                else
                {
                    m_sysData.powerData.strSAMName = "";
                    m_sysData.powerData.realSAMFactor = 1;
                    m_sysData.powerData.currentSAMFactor = 1;
                }

                m_sysData.powerData.bLoadSAM = m_sysData.powerData.bIndSAM;

                trackBarExposure.Enabled = false;
                labelExposureMin.Enabled = false;
                labelExposureMax.Enabled = false;
                labelExposureValue.Text = br.ReadInt32().ToString() + " µs";

                trackBarGain.Enabled = false;
                labelGainMin.Enabled = false;
                labelGainMax.Enabled = false;
                labelGainValue.Text = br.ReadInt32().ToString() + " dB";

                m_sysData.powerData.PowerCalibr.PowerCalibrationExposure = br.ReadInt32();
                m_sysData.powerData.PowerCalibr.PowerCalibrationGain = br.ReadInt32();

                m_sysData.videoDeviceData.pixelFormat = (br.ReadByte() == 12) ? PixelFormat.Format24bppRgb : PixelFormat.Format8bppIndexed;
                picturePaletteImage.Format = m_sysData.videoDeviceData.pixelFormat;

                int iLeft = br.ReadUInt16();//Left
                int iTop = br.ReadUInt16();//Top

                pictureBoxImage.Size = new Size(br.ReadUInt16(), br.ReadUInt16());

                pictureBoxImageSmall.Size = new System.Drawing.Size(pictureBoxImageSmall.Width, pictureBoxImageSmall.Width * (pictureBoxImage.Height / pictureBoxImage.Width));

                pointSensorCenter = new Point((int)(br.ReadUInt16() / 2) - iLeft, (int)(br.ReadUInt16() / 2) - iTop);

                m_bmp = new Bitmap(pictureBoxImage.Width, pictureBoxImage.Height, picturePaletteImage.Format);

                if (m_bmp.PixelFormat == PixelFormat.Format8bppIndexed)
                    m_bmp.Palette = picturePaletteImage.Palette;
                else
                    Color = picturePaletteImage.colorArray;

                toolStripStatusLabelPixelFormat.Text = (m_sysData.videoDeviceData.pixelFormat == PixelFormat.Format8bppIndexed) ? "8 bpp" : "12 bpp";
                /*

                                for (int i = 0, iShift = 0; i < m_snapshot.Height; i++, iShift = i * m_snapshot.Width)
                                {
                                    for (int j = 0; j < m_snapshot.Width; j++, iShift++)
                                    {
                                        if (m_sysData.videoDeviceData.pixelFormat == PixelFormat.Format8bppIndexed)
                                            bw.Write((Byte)m_snapshot.GetPixelColor(iShift));
                                        else
                                            bw.Write(m_snapshot.GetPixelColor(iShift));
                                    }
                                }
                                */

                UpdateView();

                MenuToolEnabled();

                bRet = true;
            }
            catch (Exception ex)
            {
                FileInfo fi = new FileInfo(strFileName);
                CustomMessageBox.Show(ex + "Unable to open file " + fi.Name + " .", "Open Snapshot File Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                br.Close();
                fs.Close();
            }

            return bRet;
        }

        private void dataPanelToolStrip_Click(object sender, EventArgs e)
        {
            if (sender.GetType() == typeof(ToolStripButton))
                dataPanelToolStripMenuItem.Checked = dataViewToolStripButton.Checked;
            else if (sender.GetType() == typeof(ToolStripMenuItem))
                dataViewToolStripButton.Checked = dataPanelToolStripMenuItem.Checked;

            m_sysData.applicationData.bViewDataPanel = dataPanelToolStripMenuItem.Checked;
            controlSplitContainer.Panel2Collapsed = !m_sysData.applicationData.bViewDataPanel;

            mainSplitContainer.Panel2MinSize = ((controlSplitContainer.Panel2Collapsed == false) ? /*controlSplitContainer.Panel2MinSize*/321 : 0) + /*controlSplitContainer.Panel1MinSize*/ 37 + controlSplitContainer.SplitterWidth;
            mainSplitContainer.SplitterDistance = mainSplitContainer.Width - mainSplitContainer.SplitterWidth - mainSplitContainer.Panel2MinSize;

            //            mainSplitContainer.Panel2Collapsed = !m_sysData.applicationData.bViewDataPanel;
            propertyBoxToolStripMenuItem.Enabled = m_sysData.applicationData.bViewDataPanel;
        }

        private void numericUpDownAngle_ValueChanged(object sender, EventArgs e)
        {
            AsyncChangeAngle asyncChangeAngle = new AsyncChangeAngle(UpdateAngleProfile);
            asyncChangeAngle.BeginInvoke(Convert.ToDouble(-numericUpDownAngle.Value), null, null);
        }

        private void UpdateAngleProfile(Double Angle)
        {
            try
            {
                this.Invoke((MethodInvoker)delegate
                {
                    m_sysData.lineProfileAngle = (float)(Angle * Math.PI / 180f);
                    bm.lineProfileAngle = m_sysData.lineProfileAngle;
                });

            }
            catch
            {
            }
        }

        private void typeLineProfileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem tsmi = (ToolStripMenuItem)sender;

            if (tsmi.Name.Contains("centroid") == true)
                m_sysData.LineProfileType = BeamOnCL.BeamOnCL.TypeLineProfile.tpLineCentroid;
            else if (tsmi.Name.Contains("free") == true)
                m_sysData.LineProfileType = BeamOnCL.BeamOnCL.TypeLineProfile.tpLineFree;

            bm.LineProfileType = m_sysData.LineProfileType;

            centroidToolStripMenuItem.Checked = (m_sysData.LineProfileType == BeamOnCL.BeamOnCL.TypeLineProfile.tpLineCentroid);
            freeLineToolStripMenuItem.Checked = (m_sysData.LineProfileType == BeamOnCL.BeamOnCL.TypeLineProfile.tpLineFree);
        }

        private void freezeToolStripButton_Click(object sender, EventArgs e)
        {
            m_bFreezePicture = freezeToolStripButton.Checked;
        }

        private void imageSplitContainer_Panel2_Scroll(object sender, ScrollEventArgs e)
        {
            pictureBoxData.Invalidate();
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            pictureBoxData.Invalidate();
            base.OnMouseWheel(e);
        }

        private void runningToolStripButton_Click(object sender, EventArgs e)
        {
            if ((m_sysData.fastModeData.strFileName == null) || (m_sysData.fastModeData.strFileName.Equals("") == true))
            {
                CustomMessageBox.Show("Not specified the name and path for the Fast Mode data file.",
                                                        "Save Fast Mode data File",
                                                        MessageBoxButtons.OK,
                                                        MessageBoxIcon.Error);
                return;
            }

            FileInfo fi = new FileInfo(m_sysData.fastModeData.strFileName);

            if (sender.ToString().Contains("Start") == true)
            {
                if (File.Exists(m_sysData.fastModeData.strFileName))
                {
                    if (CustomMessageBox.Show("This file '" + fi.Name + "' already exists. \nDo you want to replace it?",
                                        "Save Fast Mode data File",
                                        MessageBoxButtons.YesNo,
                                        MessageBoxIcon.Question) == DialogResult.No)
                        return;
                    else
                        File.Delete(m_sysData.fastModeData.strFileName);
                }

            }

            EnableFastModeData(sender.ToString().Contains("Start"));
        }

        private void EnableFastModeData(Boolean bEnable)
        {
            bm.FastMode = bEnable;

            if (bm.FastMode == true)
            {
                mnuFileStartRunningMode.Checked = true;
                //                mnuFileStartRunningMode.Image = global::BeamOn_U3.Properties.Resources.Hand;
                mnuFileStartRunningMode.Text = "Stop &Running Mode";
                mnuFileStartRunningMode.ToolTipText = "Stop Running Mode";
                runningToolStripButton.Text = "Stop Running";

                m_ffmddLog = new FileFastModeData();
                m_ffmddLog.OnStopFastMode += new FileFastModeData.StopFastMode(m_ffmddLog_OnStopFastMode);

                m_arraySapshot = null;

                m_SystemMessage |= (UInt16)SystemStatus.M_SS_FAST_MODE;
            }
            else
            {
                mnuFileStartRunningMode.Checked = false;
                //                mnuFileStartRunningMode.Image = global::BeamOn_U3.Properties.Resources.Start;
                mnuFileStartRunningMode.Text = "Start &Running Mode";
                mnuFileStartRunningMode.ToolTipText = "Start Running Mode";
                runningToolStripButton.Text = "Start Running";

                m_ffmddLog.CreateFastModeDataFile();

                m_SystemMessage = (UInt16)(m_SystemMessage & (~(int)SystemStatus.M_SS_FAST_MODE));
            }

            AddItemAsyncDelegate asyncSM = new AddItemAsyncDelegate(UpdateSystemMessage);
            asyncSM.BeginInvoke(null, null);

            runningToolStripButton.ToolTipText = mnuFileStartRunningMode.ToolTipText;
            //            runningToolStripButton.Image = mnuFileStartRunningMode.Image;
            runningToolStripButton.Checked = mnuFileStartRunningMode.Checked;


            mnuFileSetupDataCollection.Enabled = !mnuFileStartRunningMode.Checked;
            runningToolStripButton.Enabled = mnuFileStartRunningMode.Enabled;
        }

        private void CloseFastModeData()
        {
            try
            {
                this.Invoke((MethodInvoker)delegate
                {
                    bm.FastMode = false;

                    this.mnuFileStartRunningMode.Checked = false;
                    //                    this.mnuFileStartRunningMode.Image = global::BeamOn_U3.Properties.Resources.Start;
                    this.mnuFileStartRunningMode.Text = "Start &Running Mode";
                    this.mnuFileStartRunningMode.ToolTipText = "Start Running Mode";

                    this.runningToolStripButton.ToolTipText = this.mnuFileStartRunningMode.ToolTipText;
                    //this.runningToolStripButton.Image = this.mnuFileStartRunningMode.Image;
                    this.runningToolStripButton.Text = "Start Running";
                    this.runningToolStripButton.Checked = this.mnuFileStartRunningMode.Checked;

                    this.mnuFileSetupRunningMode.Enabled = !this.mnuFileStartRunningMode.Checked;
                    this.runningSetupToolStripButton.Enabled = this.mnuFileSetupRunningMode.Enabled;

                    m_SystemMessage = (UInt16)(m_SystemMessage & (~(int)SystemStatus.M_SS_FAST_MODE));

                    AddItemAsyncDelegate asyncSM = new AddItemAsyncDelegate(UpdateSystemMessage);
                    asyncSM.BeginInvoke(null, null);
                });
            }
            catch
            {
            }
        }

        void m_ffmddLog_OnStopFastMode(object sender, EventArgs e)
        {
            CloseLogAsyncDelegate asyncCloseLog = new CloseLogAsyncDelegate(CloseFastModeData);
            asyncCloseLog.BeginInvoke(null, null);
        }

        private void runningSetupToolStripItem_Click(object sender, EventArgs e)
        {
            FormSetupFastMode formSetupFastMode = new FormSetupFastMode();

            if (formSetupFastMode.ShowDialog() == DialogResult.OK)
            {
                mnuFileStartRunningMode.Enabled = true;
                runningToolStripButton.Enabled = mnuFileStartRunningMode.Enabled;
            }
        }

        private void ChangeViewPanels()
        {
            controlSplitContainer.Panel2Collapsed = ((m_sysData.applicationData.bViewControlPanel == false) && (m_sysData.applicationData.bViewDataPanel == false) && (m_sysData.applicationData.bViewImagePanel == false));
            dataSplitContainer.Panel1Collapsed = ((m_sysData.applicationData.bViewControlPanel == false) && (m_sysData.applicationData.bViewImagePanel == false));
            dataSplitContainer.Panel2Collapsed = (m_sysData.applicationData.bViewDataPanel == false);
            viewSplitContainer.Panel1Collapsed = (m_sysData.applicationData.bViewImagePanel == false);
            viewSplitContainer.Panel2Collapsed = (m_sysData.applicationData.bViewControlPanel == false);

            mainSplitContainer.Panel2MinSize = ((controlSplitContainer.Panel2Collapsed == false) ? /*controlSplitContainer.Panel2MinSize*/321 : 0) + /*controlSplitContainer.Panel1MinSize*/ 37 + controlSplitContainer.SplitterWidth;
            mainSplitContainer.SplitterDistance = mainSplitContainer.Width - mainSplitContainer.SplitterWidth - mainSplitContainer.Panel2MinSize;

            dataPanelToolStripMenuItem.Checked = m_sysData.applicationData.bViewDataPanel;
            dataViewToolStripButton.Checked = dataPanelToolStripMenuItem.Checked;
            propertyBoxToolStripMenuItem.Checked = m_sysData.applicationData.bViewControlPanel;

            if (m_sysData.applicationData.bViewDataPanel) dataSplitContainer.SplitterDistance = dataSplitContainer.Panel1MinSize;

            //mainSplitContainer.Panel2Collapsed = !m_sysData.applicationData.bViewDataPanel;
        }

        private void checkBox_Click(object sender, EventArgs e)
        {
            m_sysData.applicationData.bViewDataPanel = dataCheckBox.Checked;
            m_sysData.applicationData.bViewControlPanel = propertyCheckBox.Checked;
            m_sysData.applicationData.bViewImagePanel = viewCheckBox.Checked;

            ChangeViewPanels();
        }
    }
}
