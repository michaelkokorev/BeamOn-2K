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
    public partial class FormMain : Form
    {
        Bitmap m_bmp = null;
        Object m_lLockBMP = new Object();

        public FormMain()
        {
            InitializeComponent();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void trackBarBinning_Scroll(object sender, EventArgs e)
        {

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
                //m_pCrossPosition = new Point(e.X, e.Y);
                //m_lpHorizontal.CrossPoint = new Point(e.X, e.Y);
                //m_lpVertical.CrossPoint = m_lpHorizontal.CrossPoint;
                //pictureBoxData.Invalidate();
            }
        }

        void pictureBoxData_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                //m_pCrossPosition = new Point(e.X, e.Y);
                //m_lpHorizontal.CrossPoint = new Point(e.X, e.Y);
                //m_lpVertical.CrossPoint = m_lpHorizontal.CrossPoint;
                //pictureBoxData.Invalidate();
            }
            else if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                //m_camera.StreamGrabber.Stop();

                //lock (m_lLockBMP)
                //{
                //    if ((splitContainerImage.Panel2.Width < pictureBoxImage.Width) || (splitContainerImage.Panel2.Height < pictureBoxImage.Height))
                //    {
                //        int iSizeX = (int)((m_pPositioning.WorkingArea.Width > splitContainerImage.Panel2.Width) ? m_pPositioning.WorkingArea.Width : splitContainerImage.Panel2.Width - 8);
                //        int iSizeY = (int)((m_pPositioning.WorkingArea.Height > splitContainerImage.Panel2.Height) ? m_pPositioning.WorkingArea.Height : splitContainerImage.Panel2.Height - 8);
                //        int iSizeXMax = (int)m_camera.Parameters[PLCamera.Width].GetMaximum();
                //        int iSizeYMax = (int)m_camera.Parameters[PLCamera.Height].GetMaximum();

                //        iSizeX = (int)(Math.Floor(iSizeX / 4f) * 4);
                //        iSizeY = (int)(Math.Floor(iSizeY / 4f) * 4);

                //        if (iSizeXMax < iSizeX) iSizeX = iSizeXMax;
                //        if (iSizeYMax < iSizeY) iSizeY = iSizeYMax;

                //        m_camera.Parameters[PLCamera.Width].SetValue(iSizeX);
                //        m_camera.Parameters[PLCamera.Height].SetValue(iSizeY);

                //        int iOffsetX = (int)m_pPositioning.Ellipse.Centroid.X - (int)(iSizeX / 2);
                //        int iOffsetY = (int)m_pPositioning.Ellipse.Centroid.Y - (int)(iSizeY / 2);

                //        if (iOffsetX < 0) iOffsetX = 0;
                //        if (iOffsetY < 0) iOffsetY = 0;

                //        iOffsetX = (int)(Math.Floor(iOffsetX / 4f) * 4);
                //        iOffsetY = (int)(Math.Floor(iOffsetY / 4f) * 4);

                //        int IOffsetXMax = (int)m_camera.Parameters[PLCamera.OffsetX].GetMaximum();
                //        int IOffsetYMax = (int)m_camera.Parameters[PLCamera.OffsetY].GetMaximum();
                //        if (IOffsetXMax < iOffsetX) iOffsetX = IOffsetXMax;
                //        if (IOffsetYMax < iOffsetY) iOffsetY = IOffsetYMax;

                //        m_camera.Parameters[PLCamera.OffsetX].SetValue(iOffsetX);
                //        m_camera.Parameters[PLCamera.OffsetY].SetValue(iOffsetY);
                //    }
                //    else
                //    {
                //        m_camera.Parameters[PLCamera.OffsetX].SetValue(0);
                //        m_camera.Parameters[PLCamera.OffsetY].SetValue(0);
                //        m_camera.Parameters[PLCamera.Width].SetValue((int)m_camera.Parameters[PLCamera.Width].GetMaximum());
                //        m_camera.Parameters[PLCamera.Height].SetValue((int)m_camera.Parameters[PLCamera.Height].GetMaximum());
                //    }

                //    pictureBoxImage.Size = new System.Drawing.Size((int)m_camera.Parameters[PLCamera.Width].GetValue(), (int)m_camera.Parameters[PLCamera.Height].GetValue());

                //    m_bmp = new Bitmap(pictureBoxImage.Width,
                //                       pictureBoxImage.Height,
                //                       (m_camera.Parameters[PLCamera.PixelFormat].GetValue() == PLCamera.PixelFormat.Mono8) ? System.Drawing.Imaging.PixelFormat.Format8bppIndexed : System.Drawing.Imaging.PixelFormat.Format24bppRgb);

                //    if (picturePaletteImage.Format == PixelFormat.Format8bppIndexed) m_bmp.Palette = picturePaletteImage.Palette;

                //    CreateProfile();
                //}

                //m_camera.StreamGrabber.Start(GrabStrategy.LatestImages/*.OneByOne*/, GrabLoop.ProvidedByStreamGrabber);
            }
        }

        void pictureBoxData_Paint(object sender, PaintEventArgs e)
        {
            Graphics grfx = e.Graphics;

            //if (m_bFree == false)
            //{
            //    if (m_tpProfile == TypeProfile.tpLIne)
            //    {
            //        grfx.DrawLine(m_PenGrid, m_lpHorizontal.LeftPoint, m_lpHorizontal.RightPoint);
            //        grfx.DrawLine(m_PenGrid, m_lpVertical.LeftPoint, m_lpVertical.RightPoint);

            //        m_lpHorizontal.Draw(m_PenGrid, DrawOrientation.doHorizontal, grfx);
            //        m_lpVertical.Draw(m_PenGrid, DrawOrientation.doVertical, grfx);
            //    }
            //    else
            //    {
            //        m_pPositioning.DrawProfile(m_PenGrid, grfx);
            //    }

            //    m_pPositioning.Draw(m_PenGrid, grfx);
            //}
        }

        private void UpdateVisibleAsync(Int64 Timestamp)
        {
            try
            {
                this.Invoke((MethodInvoker)delegate
                {
                    //lock (this)
                    //{
                    //    toolStripStatusLabelTimeStamp.Text = (1000f / (double)Timestamp).ToString("#.000") + " fps";
                    //}
                });

            }
            catch
            {
            }
        }
    }
}
