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

        Profile m_lpHorizontal = null;
        Profile m_lpVertical = null;
        Positioning m_pPositioning = null;

        Point m_pCrossPosition = new Point(0, 0);
        public enum TypeProfile { tpLIne, tpSum };
        TypeProfile m_tpProfile = TypeProfile.tpLIne;

        Boolean m_bMeasure = false;

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

        public Boolean Measure
        {
            get { return m_bMeasure; }

            set { m_bMeasure = value; }
        }

        public TypeProfile typeProfile
        {
            get { return m_tpProfile; }
            set { m_tpProfile = value; }
        }

        public Point CrossPosition
        {
            get { return m_pCrossPosition; }

            set
            {
                m_pCrossPosition = value;
                m_lpHorizontal.CrossPoint = m_pCrossPosition;
                m_lpVertical.CrossPoint = m_pCrossPosition;
            }
        }

        public PointF lineProfileHorizontalLeft
        {
            get { return m_lpHorizontal.LeftPoint; }
        }

        public PointF lineProfileHorizontalRight
        {
            get { return m_lpHorizontal.RightPoint; }
        }

        public PointF lineProfileVerticalLeft
        {
            get { return m_lpVertical.LeftPoint; }
        }

        public PointF lineProfileVerticalRight
        {
            get { return m_lpVertical.RightPoint; }
        }

        public Double[] profileHorizontal
        {
            get { return (m_tpProfile == TypeProfile.tpLIne) ? m_lpHorizontal.DataProfile : m_pPositioning.HorizontalProfile; }
        }

        public Double[] profileVertical
        {
            get { return (m_tpProfile == TypeProfile.tpLIne) ? m_lpVertical.DataProfile : m_pPositioning.VerticalProfile; }
        }

        public Double maxProfileHorizontal
        {
            get { return (m_tpProfile == TypeProfile.tpLIne) ? m_lpHorizontal.MaxProfile : m_pPositioning.MaxHorizontalProfile; }
        }

        public Double maxProfileVertical
        {
            get { return (m_tpProfile == TypeProfile.tpLIne) ? m_lpVertical.MaxProfile : m_pPositioning.MaxVerticalProfile; }
        }

        void mc_OnNewDataReceved(object sender, MeasureCamera.NewDataRecevedEventArgs e)
        {
            if (m_bMeasure == true)
            {
                m_pPositioning.GetData(mc.Snapshot);//.Create(m_snapshot);

                m_lpHorizontal.Create(mc.Snapshot);
                m_lpVertical.Create(mc.Snapshot);
            }

            OnImageReceved(sender, e);
        }

        public Area Ellipse
        {
            get { return m_pPositioning.Ellipse; }
        }

        public Rectangle WorkingArea
        {
            get { return m_pPositioning.WorkingArea; }
        }

        public void Stop()
        {
            mc.Stop();
        }

        public Boolean Start(PixelFormat pixelFormat)
        {
            Boolean bRet = mc.Start(pixelFormat);

            CreateProfile();

            return bRet;
        }

        public PixelFormat pixelFormat
        {
            get{return mc.pixelFormat;}

            set
            {
                mc.StopGrabber();

                mc.pixelFormat = value;

                CreateProfile();

                mc.StartGrabber();
            }
        }

        public void SetImageDataArray(IntPtr Data, Color[] colorArray = null)
        {
            mc.SetImageDataArray(Data, colorArray);
        }

        private void CreateProfile()
        {
            //m_lpHorizontal = new SumProfile(new Rectangle(0, 0, pictureBoxImage.Width, pictureBoxImage.Height));
            m_lpHorizontal = new LineProfile(new Rectangle(0, 0, mc.Snapshot.Width, mc.Snapshot.Height));
            m_lpHorizontal.CrossPoint = m_pCrossPosition;
            m_lpHorizontal.Angle = 0;

            //m_lpVertical = new SumProfile(new Rectangle(0, 0, pictureBoxImage.Width, pictureBoxImage.Height));
            m_lpVertical = new LineProfile(new Rectangle(0, 0, mc.Snapshot.Width, mc.Snapshot.Height));
            m_lpVertical.CrossPoint = m_pCrossPosition;
            m_lpVertical.Angle = Math.PI / 2f;

            m_pPositioning = new Positioning(new Rectangle(0, 0, mc.Snapshot.Width, mc.Snapshot.Height));
        }

        public Rectangle ImageRectangle
        {
            get { return mc.ImageRectangle; }

            set
            {
                mc.StopGrabber();

                mc.ImageRectangle = value;

                CreateProfile();

                mc.StartGrabber();
            }
        }

        public Rectangle MaxImageRectangle
        {
            get { return mc.MaxImageRectangle; }
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

                CreateProfile();

                mc.StartGrabber();
            }
        }

        public int MaxGain
        {
            get { return mc.MaxGain; }
        }

        public int MinGain
        {
            get { return mc.MinGain; }
        }

        public int Gain
        {
            get { return mc.Gain; }

            set { mc.Gain = value; }
        }

        public int MaxExposure
        {
            get { return mc.MaxExposure; }
        }

        public int MinExposure
        {
            get { return mc.MinExposure; }
        }

        public int Exposure
        {
            get { return mc.Exposure; }

            set { mc.Exposure = value; }
        }
    }
}
