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

        public void Stop()
        {
            mc.Stop();
        }

        public void Start(PixelFormat pixelFormat)
        {
            mc.Start(pixelFormat);
        }

        public void ChangePixelFormat(PixelFormat pixelFormat)
        {
            mc.StopGrabber();

            mc.CreateData(pixelFormat);

            mc.StartGrabber();
        }

        public void SetImageDataArray(IntPtr Data, Color[] colorArray = null)
        {
            mc.SetImageDataArray(Data, colorArray);
        }

        public Rectangle ImageRectangle
        {
            get { return mc.ImageRectangle; }
        }

        public int MaxBinning
        {
            get { return mc.MaxBinning; }
        }

        public int MinBinning
        {
            get { return mc.MinBinning; }
        }

        public int Binning
        {
            get { return mc.Binning; }

            set
            {
                mc.StopGrabber();

                mc.Binning = value;

                mc.StartGrabber();
            }
        }
    }
}
