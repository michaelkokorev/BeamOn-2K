using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Basler.Pylon;
using System.Drawing;
using System.Drawing.Imaging;

namespace BeamOnCL
{
    public class BeamOnCL
    {
        MeasureCamera mc = null;
        Bitmap m_bmp = null;
        Object m_lLockBMP = new Object();

        public delegate void ImageReceved(object sender, MeasureCamera.NewDataRecevedEventArgs e);
        public event ImageReceved OnImageReceved;

        public BeamOnCL()
        {
            mc = new MeasureCamera();

            mc.OnNewDataReceved += new MeasureCamera.NewDataReceved(mc_OnNewDataReceved);
            mc.OnChangeStatusCamera += new MeasureCamera.ChangeStatusCamera(mc_OnChangeStatusCamera);
        }

        void mc_OnChangeStatusCamera(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
        }

        void mc_OnNewDataReceved(object sender, MeasureCamera.NewDataRecevedEventArgs e)
        {
            OnImageReceved(sender, e);
        }

        public void CreateImage()
        {
            BitmapData bmpData = null;

            if (m_bmp != null)
            {
                lock (m_lLockBMP)
                {
                    try
                    {
                        bmpData = m_bmp.LockBits(
                                                 mc.ImageRectangle,
                                                 System.Drawing.Imaging.ImageLockMode.WriteOnly,
                                                 m_bmp.PixelFormat
                                                 );

                        mc.SetData(bmpData.Scan0);
                    }
                    catch { }
                    finally
                    {
                        if (bmpData != null) m_bmp.UnlockBits(bmpData);
                    }
                }
            }
        }

        public Bitmap Image
        {
            get { return m_bmp; }
        }

        public void Stop()
        {
            mc.Stop();
        }

        public void Start(PixelFormat pixelFormat)
        {
            mc.Start(pixelFormat);
            m_bmp = new Bitmap(mc.ImageRectangle.Width, mc.ImageRectangle.Height, pixelFormat);
        }

        public void ChangePixelFormat(ColorPalette Palette, PixelFormat pixelFormat, Color[] color)
        {
            mc.StopGrabber();

            if (m_bmp.PixelFormat != pixelFormat) CreateData(pixelFormat);

            if (m_bmp.PixelFormat == PixelFormat.Format8bppIndexed)
                m_bmp.Palette = Palette;
            else
                mc.Color = color;

            mc.StartGrabber();
        }

        private void CreateData(PixelFormat pixelFormat)
        {
            mc.CreateData(pixelFormat);

            m_bmp = new Bitmap(mc.ImageRectangle.Width, mc.ImageRectangle.Height, pixelFormat);
        }

        public void SetBitmapData(IntPtr Data)
        {
            mc.SetData(Data);
        }

        public Rectangle ImageRectangle
        {
            get { return mc.ImageRectangle; }
        }
    }
}
