﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.Diagnostics;

namespace BeamOn_2K
{
    public partial class FormMain : Form
    {
        private delegate void AsyncChangePalette(ColorPalette cpValue, PixelFormat pixelFormat, System.Drawing.Color[] color);
        private delegate void AsyncTimeStamp(Int64 iValue);

        Object m_lLockBMP = new Object();
        Bitmap m_bmp = null;
        System.Drawing.Color[] m_colorArray = null;

        Stopwatch m_sw = null;

        private BeamOnCL.BeamOnCL bm = null;

        Pen m_PenCentroid = new Pen(System.Drawing.Color.Black, 0.2f);
        Pen m_PenEllipse = new Pen(System.Drawing.Color.Green, 2f);
        Pen m_PenGaussian = new Pen(System.Drawing.Color.Red, 0.2f);
        Font myFont = new Font("Arial", 10, FontStyle.Bold);
        Brush PaletteBrush = new SolidBrush(System.Drawing.Color.DarkGray);
        StringFormat m_strfrm = new StringFormat();
        Pen m_PenGrid = new Pen(System.Drawing.Color.DarkGray, 0.1f);

        Pen m_PenClipLevel1 = new Pen(System.Drawing.Color.Red, 0.1f);
        Pen m_PenClipLevel2 = new Pen(System.Drawing.Color.Blue, 0.1f);
        Pen m_PenClipLevel3 = new Pen(System.Drawing.Color.Yellow, 0.1f);

        public enum DrawOrientation { doHorizontal, doVertical };
        Boolean m_bScaleProfile = false;
        Boolean m_bGaussian = false;
        Boolean m_bMeasure = false;

        float[] m_fClipLevelArray = new float[] { 13.5f, 50f, 80f };

        float[] m_fWidthHorizontalClip = new float[3];
        float[] m_fWidthVerticalClip = new float[3];

        float[] m_fWidthGaussianHorizontalClip = new float[3];
        float[] m_fWidthGaussianVerticalClip = new float[3];

        float m_fGaussianHorizontalCorrelation = 0;
        float m_fGaussianVerticalCorrelation = 0;

        BeamOnCL.Profile m_profileHorizontal = null;
        BeamOnCL.Profile m_profileVertical = null;
        Point[] plArea = null;

        public FormMain()
        {
            InitializeComponent();

            bm = new BeamOnCL.BeamOnCL();
            bm.OnImageReceved += new BeamOnCL.BeamOnCL.ImageReceved(bm_OnImageReceved);
        }

        void bm_OnImageReceved(object sender, BeamOnCL.MeasureCamera.NewDataRecevedEventArgs e)
        {
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

                        //// Assign a temporary variable to dispose the bitmap after assigning the new bitmap to the display control.
                        //Bitmap bitmapOld = pictureBoxImage.Image as Bitmap;
                        // Provide the display control with the new bitmap. This action automatically updates the display.
                        //pictureBoxImage.Image = m_bmp;
                        //if (bitmapOld != null)
                        //{
                        //    // Dispose the bitmap.
                        //    bitmapOld.Dispose();
                        //}


                        if (m_bMeasure == true)
                        {
                            bm.GetMeasure(e.Snapshot);

                            plArea = bm.CreateEllipse();

                            m_profileHorizontal = new BeamOnCL.Profile(bm.profileHorizontal);
                            m_profileVertical = new BeamOnCL.Profile(bm.profileVertical);

                            for (int i = 0; i < m_fClipLevelArray.Length; i++)
                            {
                                m_fWidthHorizontalClip[i] = (float)m_profileHorizontal.GetWidth(m_fClipLevelArray[i]);
                                m_fWidthVerticalClip[i] = (float)m_profileVertical.GetWidth(m_fClipLevelArray[i]);
                            }

                            if (m_bGaussian == true)
                            {
                                for (int i = 0; i < m_fClipLevelArray.Length; i++)
                                {
                                    m_fWidthGaussianHorizontalClip[i] = (float)m_profileHorizontal.GaussianData.GetWidth(m_fClipLevelArray[i]);
                                    m_fWidthGaussianVerticalClip[i] = (float)m_profileVertical.GaussianData.GetWidth(m_fClipLevelArray[i]);
                                }

                                m_fGaussianHorizontalCorrelation = m_profileHorizontal.GaussianData.Correlation;
                                m_fGaussianVerticalCorrelation = m_profileVertical.GaussianData.Correlation;
                            }
                        }
                    }
                    catch { }
                }
            }

            m_sw.Stop();

            AsyncTimeStamp asyncTimeStamp = new AsyncTimeStamp(UpdateVisibleAsync);
            asyncTimeStamp.BeginInvoke(m_sw.ElapsedMilliseconds, null, null);

            m_sw = Stopwatch.StartNew();
            m_sw.Start();

            this.Invalidate();
            dataSplitContainer.Panel2.Invalidate();
            //            pictureBoxImage.Invalidate();
            pictureBoxData.Invalidate();

            System.Threading.Thread.Sleep(10);
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
            mainToolStrip.Visible = !toolBarToolStripMenuItem.Checked;
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
        }

        void pictureBoxData_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                bm.CrossPosition = new Point(e.X, e.Y);
                pictureBoxData.Invalidate();
            }
        }

        void pictureBoxData_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                bm.CrossPosition = new Point(e.X, e.Y);
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

            if (m_bMeasure == true)
            {
                Point OldPoint = new Point((int)bm.PixelCentroid.X - 20, (int)bm.PixelCentroid.Y - 20);
                Point NewPoint = new Point((int)bm.PixelCentroid.X + 20, (int)bm.PixelCentroid.Y + 20);

                grfx.DrawLine(m_PenCentroid, OldPoint, NewPoint);

                OldPoint = new Point((int)bm.PixelCentroid.X - 20, (int)bm.PixelCentroid.Y + 20);
                NewPoint = new Point((int)bm.PixelCentroid.X + 20, (int)bm.PixelCentroid.Y - 20);

                grfx.DrawLine(m_PenCentroid, OldPoint, NewPoint);

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
                grfx.DrawString("ClipLevel: " + String.Format("{0:F1}", m_fClipLevelArray[2]) + "%" + " Width: " + String.Format("{0:F2}", m_fWidthHorizontalClip[2]) + "(µm)", myFont, PaletteBrush, new PointF(20 - iShiftX, iHeight - 80 - iShiftY), m_strfrm);
                grfx.DrawString("ClipLevel: " + String.Format("{0:F1}", m_fClipLevelArray[1]) + "%" + " Width: " + String.Format("{0:F2}", m_fWidthHorizontalClip[1]) + "(µm)", myFont, PaletteBrush, new PointF(20 - iShiftX, iHeight - 60 - iShiftY), m_strfrm);
                grfx.DrawString("ClipLevel: " + String.Format("{0:F1}", m_fClipLevelArray[0]) + "%" + " Width: " + String.Format("{0:F2}", m_fWidthHorizontalClip[0]) + "(µm)", myFont, PaletteBrush, new PointF(20 - iShiftX, iHeight - 40 - iShiftY), m_strfrm);
                if (m_bGaussian == true) grfx.DrawString("Gaussian Correlation: " + String.Format("{0:F1}", m_fGaussianHorizontalCorrelation) + "%", myFont, PaletteBrush, new PointF(20 - iShiftX, iHeight - 20 - iShiftY), m_strfrm);

                grfx.DrawString("Vertical Profile", myFont, PaletteBrush, new PointF(20 - iShiftX, 20 - iShiftY), m_strfrm);
                grfx.DrawString("ClipLevel: " + String.Format("{0:F1}", m_fClipLevelArray[2]) + "%" + " Width: " + String.Format("{0:F2}", m_fWidthVerticalClip[2]) + "(µm)", myFont, PaletteBrush, new PointF(20 - iShiftX, 40 - iShiftY), m_strfrm);
                grfx.DrawString("ClipLevel: " + String.Format("{0:F1}", m_fClipLevelArray[1]) + "%" + " Width: " + String.Format("{0:F2}", m_fWidthVerticalClip[1]) + "(µm)", myFont, PaletteBrush, new PointF(20 - iShiftX, 60 - iShiftY), m_strfrm);
                grfx.DrawString("ClipLevel: " + String.Format("{0:F1}", m_fClipLevelArray[0]) + "%" + " Width: " + String.Format("{0:F2}", m_fWidthVerticalClip[0]) + "(µm)", myFont, PaletteBrush, new PointF(20 - iShiftX, 80 - iShiftY), m_strfrm);
                if (m_bGaussian == true) grfx.DrawString("Gaussian Correlation: " + String.Format("{0:F1}", m_fGaussianVerticalCorrelation) + "%", myFont, PaletteBrush, new PointF(20 - iShiftX, 100 - iShiftY), m_strfrm);
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
                Double MaxProfile = (m_bScaleProfile == true) ? dataProfile.MaxProfile : ((bm.pixelFormat == PixelFormat.Format8bppIndexed) ? (UInt16)255 : (UInt16)4095);
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
                    if (m_bGaussian == true) OldPointGaussian = new Point((int)0, iHeight - (int)Math.Ceiling(dataProfile.GaussianData.GaussianData[0] * fCoeffAmpl) - iShiftY);

                    for (int i = 1; i < dataProfile.DataProfile.Length; i++)
                    {
                        NewPoint = new Point((int)Math.Ceiling(i * fCoeffStep), iHeight - (int)Math.Ceiling(dataProfile.DataProfile[i] * fCoeffAmpl) - iShiftY);
                        grfx.DrawLine(pen, OldPoint, NewPoint);
                        OldPoint = NewPoint;

                        if (m_bGaussian == true)
                        {
                            NewPointGaussian = new Point((int)Math.Ceiling(i * fCoeffStep), iHeight - (int)Math.Ceiling(dataProfile.GaussianData.GaussianData[i] * fCoeffAmpl) - iShiftY);
                            grfx.DrawLine(m_PenGaussian, OldPointGaussian, NewPointGaussian);
                            OldPointGaussian = NewPointGaussian;
                        }
                    }

                    if (m_bScaleProfile == true)
                    {
                        int iLineLevel1 = iHeight - (int)(iHeightProfile * m_fClipLevelArray[0] / 100f) - iShiftY;
                        int iLineLevel2 = iHeight - (int)(iHeightProfile * m_fClipLevelArray[1] / 100f) - iShiftY;
                        int iLineLevel3 = iHeight - (int)(iHeightProfile * m_fClipLevelArray[2] / 100f) - iShiftY;

                        for (int i = 0; i < pictureBoxData.Width; i += 6)
                        {
                            grfx.DrawLine(m_PenClipLevel1, i, iLineLevel1, i + 3, iLineLevel1);
                            grfx.DrawLine(m_PenClipLevel2, i, iLineLevel2, i + 3, iLineLevel2);
                            grfx.DrawLine(m_PenClipLevel3, i, iLineLevel3, i + 3, iLineLevel3);
                        }
                    }
                }
                else
                {
                    fCoeffStep = pictureBoxData.Height / (float)dataProfile.DataProfile.Length;

                    int iShiftX = imageSplitContainer.Panel2.AutoScrollPosition.X;

                    OldPoint = new Point((int)Math.Ceiling(dataProfile.DataProfile[0] * fCoeffAmpl) - iShiftX, 0);
                    if (m_bGaussian == true) OldPointGaussian = new Point((int)Math.Ceiling(dataProfile.GaussianData.GaussianData[0] * fCoeffAmpl) - iShiftX, 0);

                    for (int i = 1; i < dataProfile.DataProfile.Length; i++)
                    {
                        NewPoint = new Point((int)Math.Ceiling(dataProfile.DataProfile[i] * fCoeffAmpl) - iShiftX, (int)Math.Ceiling(i * fCoeffStep));
                        grfx.DrawLine(pen, OldPoint, NewPoint);
                        OldPoint = NewPoint;

                        if (m_bGaussian == true)
                        {
                            NewPointGaussian = new Point((int)Math.Ceiling(dataProfile.GaussianData.GaussianData[i] * fCoeffAmpl) - iShiftX, (int)Math.Ceiling(i * fCoeffStep));
                            grfx.DrawLine(m_PenGaussian, OldPointGaussian, NewPointGaussian);
                            OldPointGaussian = NewPointGaussian;
                        }
                    }

                    if (m_bScaleProfile == true)
                    {
                        int iLineLevel1 = (int)(iHeightProfile * m_fClipLevelArray[0] / 100f - iShiftX);
                        int iLineLevel2 = (int)(iHeightProfile * m_fClipLevelArray[1] / 100f - iShiftX);
                        int iLineLevel3 = (int)(iHeightProfile * m_fClipLevelArray[2] / 100f - iShiftX);

                        for (int i = 0; i < pictureBoxData.Height; i += 6)
                        {
                            grfx.DrawLine(m_PenClipLevel1, iLineLevel1, i, iLineLevel1, i + 3);
                            grfx.DrawLine(m_PenClipLevel2, iLineLevel2, i, iLineLevel2, i + 3);
                            grfx.DrawLine(m_PenClipLevel3, iLineLevel3, i, iLineLevel3, i + 3);
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
                });
            }
            catch
            {
            }
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
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

                toolStripButtonPixelFormat.Checked = (picturePaletteImage.Format == PixelFormat.Format24bppRgb);
                bitsPerPixel12ToolStripMenuItem.Checked = toolStripButtonPixelFormat.Checked;
                bitsPerPixel8ToolStripMenuItem.Checked = !toolStripButtonPixelFormat.Checked;
                toolStripStatusLabelPixelFormat.Text = (toolStripButtonPixelFormat.Checked == true) ? "12 bpp" : "8 bpp";

                bm.pixelFormat = picturePaletteImage.Format;

                toolStripButtonTypeProfile.Checked = (bm.typeProfile == BeamOnCL.BeamOnCL.TypeProfile.tpSum);
                lineProfileToolStripMenuItem.Checked = !toolStripButtonTypeProfile.Checked;
                sumProfileToolStripMenuItem.Checked = toolStripButtonTypeProfile.Checked;
                toolStripStatusLabelTypeProfile.Text = (toolStripButtonTypeProfile.Checked == true) ? "Sum" : "Line";

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

                measuringToolStripMenuItem.Checked = m_bMeasure;

                scaleProfileToolStripMenuItem.Enabled = measuringToolStripMenuItem.Checked;
                typeProfileToolStripMenuItem.Enabled = measuringToolStripMenuItem.Checked;
                toolStripButtonTypeProfile.Enabled = measuringToolStripMenuItem.Checked;
                gaussianToolStripMenuItem.Enabled = measuringToolStripMenuItem.Checked;

                groupBoxPosition.Enabled = m_bMeasure;
                ProfileGroupBox.Enabled = m_bMeasure;
                gaussGroupBox.Enabled = m_bMeasure && m_bGaussian;

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
        }

        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            bm.Stop();
        }

        private void trackBarBinning_Scroll(object sender, EventArgs e)
        {
            bm.Binning = trackBarBinning.Value;

            m_bmp = new Bitmap(bm.ImageRectangle.Width, bm.ImageRectangle.Height, picturePaletteImage.Format);

            if (m_bmp.PixelFormat == PixelFormat.Format8bppIndexed) m_bmp.Palette = picturePaletteImage.Palette;

            pictureBoxImage.Size = bm.ImageRectangle.Size;
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

        private void measuringToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            m_bMeasure = measuringToolStripMenuItem.Checked;

            groupBoxPosition.Enabled = m_bMeasure;
            ProfileGroupBox.Enabled = m_bMeasure;
            gaussGroupBox.Enabled = m_bMeasure && m_bGaussian;

            scaleProfileToolStripMenuItem.Enabled = measuringToolStripMenuItem.Checked;
            typeProfileToolStripMenuItem.Enabled = measuringToolStripMenuItem.Checked;
            toolStripButtonTypeProfile.Enabled = measuringToolStripMenuItem.Checked;
            gaussianToolStripMenuItem.Enabled = measuringToolStripMenuItem.Checked;
        }

        private void scaleProfileToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            m_bScaleProfile = scaleProfileToolStripMenuItem.Checked;
        }

        private void toolStripButtonPixelFormat_CheckedChanged(object sender, EventArgs e)
        {
            ToolStripButton tsb = (ToolStripButton)sender;

            picturePaletteImage.Format = (tsb.Checked == true) ? PixelFormat.Format24bppRgb : PixelFormat.Format8bppIndexed;

            toolStripButtonPixelFormat.Image = (picturePaletteImage.Format == PixelFormat.Format8bppIndexed) ? global::BeamOn_2K.Properties.Resources.black_12bit_mode : global::BeamOn_2K.Properties.Resources.black_8bit_mode;

            bitsPerPixel12ToolStripMenuItem.Checked = toolStripButtonPixelFormat.Checked;
            bitsPerPixel8ToolStripMenuItem.Checked = !toolStripButtonPixelFormat.Checked;

            toolStripStatusLabelPixelFormat.Text = (toolStripButtonPixelFormat.Checked == true) ? "12 bpp" : "8 bpp";

        }

        private void pixelFormatToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem tsb = (ToolStripMenuItem)sender;

            toolStripButtonPixelFormat.Checked = ((tsb.Name.Contains("12") == true) && (picturePaletteImage.Format != PixelFormat.Format24bppRgb));
        }

        private void typeProfileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem tsb = (ToolStripMenuItem)sender;

            toolStripButtonTypeProfile.Checked = ((tsb.Name.Contains("sum") == true) && (bm.typeProfile != BeamOnCL.BeamOnCL.TypeProfile.tpSum));
        }

        private void toolStripButtonTypeProfile_CheckedChanged(object sender, EventArgs e)
        {
            ToolStripButton tsb = (ToolStripButton)sender;

            bm.typeProfile = (toolStripButtonTypeProfile.Checked == true) ? BeamOnCL.BeamOnCL.TypeProfile.tpSum : BeamOnCL.BeamOnCL.TypeProfile.tpLIne;

            lineProfileToolStripMenuItem.Checked = !toolStripButtonTypeProfile.Checked;
            sumProfileToolStripMenuItem.Checked = toolStripButtonTypeProfile.Checked;

            toolStripButtonTypeProfile.Text = (bm.typeProfile == BeamOnCL.BeamOnCL.TypeProfile.tpLIne) ? "Sum" : "Line";
            toolStripStatusLabelTypeProfile.Text = (bm.typeProfile == BeamOnCL.BeamOnCL.TypeProfile.tpSum) ? "Sum" : "Line";
        }

        private void gaussianToolStripMenuItem_Click(object sender, EventArgs e)
        {
            m_bGaussian = gaussianToolStripMenuItem.Checked;

            gaussGroupBox.Enabled = m_bMeasure && m_bGaussian;
        }

        private void dataSplitContainer_Panel2_Paint(object sender, PaintEventArgs e)
        {
            if (m_bMeasure == true)
            {
                labelPositionXValue.Text = bm.Centroid.X.ToString();
                labelPositionYValue.Text = bm.Centroid.Y.ToString();

                labelHorizontalValue1.Text = m_fWidthHorizontalClip[0].ToString();
                labelHorizontalValue2.Text = m_fWidthHorizontalClip[1].ToString();
                labelHorizontalValue3.Text = m_fWidthHorizontalClip[2].ToString();

                labelVerticalValue1.Text = m_fWidthVerticalClip[0].ToString();
                labelVerticalValue2.Text = m_fWidthVerticalClip[1].ToString();
                labelVerticalValue3.Text = m_fWidthVerticalClip[2].ToString();

                if (m_bGaussian == true)
                {
                    labelGaussianHorizontalValue1.Text = m_fWidthGaussianHorizontalClip[0].ToString();
                    labelGaussianHorizontalValue2.Text = m_fWidthGaussianHorizontalClip[1].ToString();
                    labelGaussianHorizontalValue3.Text = m_fWidthGaussianHorizontalClip[2].ToString();

                    labelGaussianVerticalValue1.Text = m_fWidthGaussianVerticalClip[0].ToString();
                    labelGaussianVerticalValue2.Text = m_fWidthGaussianVerticalClip[1].ToString();
                    labelGaussianVerticalValue3.Text = m_fWidthGaussianVerticalClip[2].ToString();

                    labelGaussianCorrelationHorizontalValue.Text = m_fGaussianHorizontalCorrelation.ToString();
                    labelGaussianCorrelationVerticalValue.Text = m_fGaussianVerticalCorrelation.ToString();
                }
            }
        }
    }
}
