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

namespace BeamOn_2K
{
    public partial class FormMain : Form
    {
        private delegate void AsyncChangePalette(ColorPalette cpValue, PixelFormat pixelFormat, Color[] color);
        private delegate void AsyncTimeStamp(Int64 iValue);

        Object m_lLockBMP = new Object();
        Bitmap m_bmp = null;
        Color[] m_colorArray = null;

        Stopwatch m_sw = null;

        private BeamOnCL.BeamOnCL bm = null;

        Pen m_PenCentroid = new Pen(System.Drawing.Color.Black, 0.2f);
        Pen m_PenEllipse = new Pen(System.Drawing.Color.Green, 2f);
        Pen m_PenGaussian = new Pen(System.Drawing.Color.Red, 0.2f);
        Font myFont = new Font("Arial", 10, FontStyle.Bold);
        Brush PaletteBrush = new SolidBrush(System.Drawing.Color.DarkGray);
        StringFormat m_strfrm = new StringFormat();

        const UInt16 NUM_POINTS = 64;

        double[] Sin = new double[NUM_POINTS];
        double[] Cos = new double[NUM_POINTS];
        Point[] plArea = new Point[NUM_POINTS + 1];

        public FormMain()
        {
            InitializeComponent();

            bm = new BeamOnCL.BeamOnCL();
            bm.OnImageReceved += new BeamOnCL.BeamOnCL.ImageReceved(bm_OnImageReceved);

            double dStep = Math.PI / (NUM_POINTS / 2f);

            for (int i = 0; i < NUM_POINTS; i++)
            {
                Cos[i] = Math.Cos(dStep * i);
                Sin[i] = Math.Sin(dStep * i);
            }
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
                                                 bm.ImageRectangle,
                                                 System.Drawing.Imaging.ImageLockMode.WriteOnly,
                                                 m_bmp.PixelFormat
                                                 );

                        bm.SetImageDataArray(bmpData.Scan0, m_colorArray);

                        m_bmp.UnlockBits(bmpData);

                        DrawEllipse();
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
            pictureBoxImage.Invalidate();
        }

        private void DrawEllipse()
        {

            int i;
            double A11, A21, A12, A22;
            double Si1, Co1;
            Si1 = Math.Sin(bm.Ellipse.Angle);
            Co1 = Math.Cos(bm.Ellipse.Angle);

            A11 = bm.Ellipse.MajorRadius * Co1;
            A21 = -bm.Ellipse.MajorRadius * Si1;
            A12 = bm.Ellipse.MinorRadius * Si1;
            A22 = bm.Ellipse.MinorRadius * Co1;

            switch (bm.Ellipse.Type)
            {
                case BeamOnCL.Area.Figure.enRectangle:
                    {
                        plArea[0] = new Point((int)(-A11 - A12 + bm.Ellipse.Centroid.X), (int)(A21 + A22 + bm.Ellipse.Centroid.Y));
                        plArea[1] = new Point((int)(-A11 + A12 + bm.Ellipse.Centroid.X), (int)(A21 - A22 + bm.Ellipse.Centroid.Y));
                        plArea[2] = new Point((int)(A11 + A12 + bm.Ellipse.Centroid.X), (int)(-A21 - A22 + bm.Ellipse.Centroid.Y));
                        plArea[3] = new Point((int)(A11 - A12 + bm.Ellipse.Centroid.X), (int)(-A21 + A22 + bm.Ellipse.Centroid.Y));
                    }
                    break;
                case BeamOnCL.Area.Figure.enCircle:
                //{
                //    //A11 = CircleRadius * Co1;
                //    //A21 = -CircleRadius * Si1;
                //    //A12 = CircleRadius * Si1;
                //    //A22 = CircleRadius * Co1;
                //}
                case BeamOnCL.Area.Figure.enEllipse:
                    {
                        plArea[0] = new Point((int)(A11 * Cos[0] + A12 * Sin[0] + bm.Ellipse.Centroid.X), (int)(-A21 * Cos[0] - A22 * Sin[0] + bm.Ellipse.Centroid.Y));
                        for (i = 1; i < NUM_POINTS; i++)
                        {
                            plArea[i] = new Point((int)(A11 * Cos[i] + A12 * Sin[i] + bm.Ellipse.Centroid.X), (int)(-A21 * Cos[i] - A22 * Sin[i] + bm.Ellipse.Centroid.Y));
                        }

                        plArea[NUM_POINTS] = plArea[0];
                    }
                    break;
            }
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
                if (bm.ImageRectangle == bm.MaxImageRectangle)
                {
                    int iSizeX = (int)((bm.WorkingArea.Width > imageSplitContainer.Panel2.Width) ? bm.WorkingArea.Width : imageSplitContainer.Panel2.Width - 8);
                    int iSizeY = (int)((bm.WorkingArea.Height > imageSplitContainer.Panel2.Height) ? bm.WorkingArea.Height : imageSplitContainer.Panel2.Height - 8);

                    int iOffsetX = (int)bm.Ellipse.Centroid.X - (int)(iSizeX / 2);
                    int iOffsetY = (int)bm.Ellipse.Centroid.Y - (int)(iSizeY / 2);

                    bm.ImageRectangle = new Rectangle(iOffsetX, iOffsetY, iSizeX, iSizeY);
                }
                else
                {
                    bm.ImageRectangle = bm.MaxImageRectangle;
                }

                pictureBoxImage.Size = new System.Drawing.Size((int)bm.ImageRectangle.Width, (int)bm.ImageRectangle.Height);

                m_bmp = new Bitmap(pictureBoxImage.Width, pictureBoxImage.Height, picturePaletteImage.Format);

                if (picturePaletteImage.Format == PixelFormat.Format8bppIndexed) m_bmp.Palette = picturePaletteImage.Palette;
            }
        }

        void pictureBoxData_Paint(object sender, PaintEventArgs e)
        {
            Graphics grfx = e.Graphics;

            if (bm.Measure == true)
            {
                Point OldPoint = new Point((int)bm.Ellipse.Centroid.X - 20, (int)bm.Ellipse.Centroid.Y - 20);
                Point NewPoint = new Point((int)bm.Ellipse.Centroid.X + 20, (int)bm.Ellipse.Centroid.Y + 20);

                grfx.DrawLine(m_PenCentroid, OldPoint, NewPoint);

                OldPoint = new Point((int)bm.Ellipse.Centroid.X - 20, (int)bm.Ellipse.Centroid.Y + 20);
                NewPoint = new Point((int)bm.Ellipse.Centroid.X + 20, (int)bm.Ellipse.Centroid.Y - 20);

                grfx.DrawLine(m_PenCentroid, OldPoint, NewPoint);

                grfx.DrawLines(m_PenEllipse, plArea);

                grfx.DrawRectangle(m_PenCentroid, bm.WorkingArea);

                //if (m_tpProfile == TypeProfile.tpLIne)
                //{
                //    grfx.DrawLine(m_PenGrid, m_lpHorizontal.LeftPoint, m_lpHorizontal.RightPoint);
                //    grfx.DrawLine(m_PenGrid, m_lpVertical.LeftPoint, m_lpVertical.RightPoint);

                //    m_lpHorizontal.Draw(m_PenGrid, DrawOrientation.doHorizontal, grfx);
                //    m_lpVertical.Draw(m_PenGrid, DrawOrientation.doVertical, grfx);
                //}
                //else
                //{
                //    m_pPositioning.DrawProfile(m_PenGrid, grfx);
                //}

                //m_pPositioning.Draw(m_PenGrid, grfx);
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

                    Array.Copy(value, m_colorArray, value.Length);
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

                    bm.ChangePixelFormat(pixelFormat);
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

                pictureBoxImage.Size = new System.Drawing.Size(bm.ImageRectangle.Width, bm.ImageRectangle.Height);

                m_bmp = new Bitmap(bm.ImageRectangle.Width, bm.ImageRectangle.Height, picturePaletteImage.Format);

                if (m_bmp.PixelFormat == PixelFormat.Format8bppIndexed)
                    m_bmp.Palette = picturePaletteImage.Palette;
                else
                    Color = picturePaletteImage.colorArray;

                toolStripButtonPixelFormat.Checked = (picturePaletteImage.Format == PixelFormat.Format24bppRgb);
                bitsPerPixel12ToolStripMenuItem.Checked = toolStripButtonPixelFormat.Checked;
                bitsPerPixel8ToolStripMenuItem.Checked = !toolStripButtonPixelFormat.Checked;
                toolStripStatusLabelPixelFormat.Text = (toolStripButtonPixelFormat.Checked == true) ? "12 bpp" : "8 bpp";

                bm.ChangePixelFormat(picturePaletteImage.Format);

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

                measuringToolStripMenuItem.Checked = bm.Measure;
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

        private void trackBarBinning_Scroll(object sender, EventArgs e)
        {
            bm.Binning = trackBarBinning.Value;

            m_bmp = new Bitmap(bm.ImageRectangle.Width, bm.ImageRectangle.Height, picturePaletteImage.Format);

            if (m_bmp.PixelFormat == PixelFormat.Format8bppIndexed) m_bmp.Palette = picturePaletteImage.Palette;

            pictureBoxImage.Size = new System.Drawing.Size(bm.ImageRectangle.Width, bm.ImageRectangle.Height);
        }

        private void trackBarTransparency_Scroll(object sender, EventArgs e)
        {
            this.pictureBoxData.BackColor = System.Drawing.Color.FromArgb(trackBarTransparency.Value, pictureBoxData.BackColor.R, pictureBoxData.BackColor.G, pictureBoxData.BackColor.B);
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
            bm.Measure = measuringToolStripMenuItem.Checked;
        }
    }
}
