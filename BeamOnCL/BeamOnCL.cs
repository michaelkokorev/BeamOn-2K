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

        const UInt16 NUM_POINTS = 64;

        double[] Sin = new double[NUM_POINTS];
        double[] Cos = new double[NUM_POINTS];
        Point[] plArea = new Point[NUM_POINTS + 1];

        float m_fPixelSize = 5.86f;

        Profile m_lpHorizontal = null;
        Profile m_lpVertical = null;
        Positioning m_pPositioning = null;

        Point m_pCrossPosition = new Point(0, 0);
        public enum TypeProfile { tpLIne, tpSum };
        TypeProfile m_tpProfile = TypeProfile.tpLIne;

        public delegate void ImageReceved(object sender, MeasureCamera.NewDataRecevedEventArgs e);
        public event ImageReceved OnImageReceved;

        public BeamOnCL()
        {
            mc = new MeasureCamera();

            mc.OnNewDataReceved += new MeasureCamera.NewDataReceved(mc_OnNewDataReceved);
            mc.OnChangeStatusCamera += new MeasureCamera.ChangeStatusCamera(mc_OnChangeStatusCamera);

            double dStep = Math.PI / (NUM_POINTS / 2f);

            for (int i = 0; i < NUM_POINTS; i++)
            {
                Cos[i] = Math.Cos(dStep * i);
                Sin[i] = Math.Sin(dStep * i);
            }
        }

        void mc_OnChangeStatusCamera(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
        }

        public Point[] CreateEllipse()
        {
            int i;
            double A11, A21, A12, A22;
            double Si1, Co1;
            Si1 = Math.Sin(m_pPositioning.Ellipse.Angle);
            Co1 = Math.Cos(m_pPositioning.Ellipse.Angle);

            A11 = m_pPositioning.Ellipse.MajorRadius * Co1;
            A21 = -m_pPositioning.Ellipse.MajorRadius * Si1;
            A12 = m_pPositioning.Ellipse.MinorRadius * Si1;
            A22 = m_pPositioning.Ellipse.MinorRadius * Co1;

            switch (m_pPositioning.Ellipse.Type)
            {
                case Area.Figure.enRectangle:
                    {
                        plArea[0] = new Point((int)(-A11 - A12 + m_pPositioning.Ellipse.Centroid.X), (int)(A21 + A22 + m_pPositioning.Ellipse.Centroid.Y));
                        plArea[1] = new Point((int)(-A11 + A12 + m_pPositioning.Ellipse.Centroid.X), (int)(A21 - A22 + m_pPositioning.Ellipse.Centroid.Y));
                        plArea[2] = new Point((int)(A11 + A12 + m_pPositioning.Ellipse.Centroid.X), (int)(-A21 - A22 + m_pPositioning.Ellipse.Centroid.Y));
                        plArea[3] = new Point((int)(A11 - A12 + m_pPositioning.Ellipse.Centroid.X), (int)(-A21 + A22 + m_pPositioning.Ellipse.Centroid.Y));
                    }
                    break;
                case Area.Figure.enCircle:
                //{
                //    //A11 = CircleRadius * Co1;
                //    //A21 = -CircleRadius * Si1;
                //    //A12 = CircleRadius * Si1;
                //    //A22 = CircleRadius * Co1;
                //}
                case Area.Figure.enEllipse:
                    {
                        plArea[0] = new Point((int)(A11 * Cos[0] + A12 * Sin[0] + m_pPositioning.Ellipse.Centroid.X), (int)(-A21 * Cos[0] - A22 * Sin[0] + m_pPositioning.Ellipse.Centroid.Y));
                        for (i = 1; i < NUM_POINTS; i++)
                        {
                            plArea[i] = new Point((int)(A11 * Cos[i] + A12 * Sin[i] + m_pPositioning.Ellipse.Centroid.X), (int)(-A21 * Cos[i] - A22 * Sin[i] + m_pPositioning.Ellipse.Centroid.Y));
                        }

                        plArea[NUM_POINTS] = plArea[0];
                    }
                    break;
            }

            return plArea;
        }

        public double Angle
        {
            get { return m_pPositioning.Ellipse.Angle; }
        }

        public double MinorRadius
        {
            get { return m_pPositioning.Ellipse.MinorRadius * m_fPixelSize; }
        }

        public double MajorRadius
        {
            get { return m_pPositioning.Ellipse.MajorRadius * m_fPixelSize; }
        }

        public PointF Centroid
        {
            get
            {
                return new PointF(
                    (mc.ImageRectangle.X + m_pPositioning.Ellipse.Centroid.X - mc.MaxImageRectangle.Width / 2f) * m_fPixelSize,
                    (mc.ImageRectangle.Y + m_pPositioning.Ellipse.Centroid.Y - mc.MaxImageRectangle.Height / 2f) * m_fPixelSize);
            }
        }

        public PointF PixelCentroid
        {
            get { return new PointF(m_pPositioning.Ellipse.Centroid.X, m_pPositioning.Ellipse.Centroid.Y); }
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

        public Profile profileHorizontal
        {
            get { return (m_tpProfile == TypeProfile.tpLIne) ? m_lpHorizontal : m_pPositioning.HorizontalProfile; }
        }

        public Profile profileVertical
        {
            get { return (m_tpProfile == TypeProfile.tpLIne) ? m_lpVertical : m_pPositioning.VerticalProfile; }
        }

        public void GetMeasure(SnapshotBase snapshot)
        {
            m_pPositioning.GetData(snapshot);

            m_lpHorizontal.Create(snapshot);
            m_lpVertical.Create(snapshot);
        }

        void mc_OnNewDataReceved(object sender, MeasureCamera.NewDataRecevedEventArgs e)
        {
            OnImageReceved(sender, e);
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

            if (bRet == true) CreateProfile();

            return bRet;
        }

        public PixelFormat pixelFormat
        {
            get { return mc.pixelFormat; }

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
            m_lpHorizontal = new LineProfile(new Rectangle(0, 0, mc.Snapshot.Width, mc.Snapshot.Height), m_fPixelSize);
            m_lpHorizontal.CrossPoint = m_pCrossPosition;
            m_lpHorizontal.Angle = 0;

            //m_lpVertical = new SumProfile(new Rectangle(0, 0, pictureBoxImage.Width, pictureBoxImage.Height));
            m_lpVertical = new LineProfile(new Rectangle(0, 0, mc.Snapshot.Width, mc.Snapshot.Height), m_fPixelSize);
            m_lpVertical.CrossPoint = m_pCrossPosition;
            m_lpVertical.Angle = Math.PI / 2f;

            m_pPositioning = new Positioning(new Rectangle(0, 0, mc.Snapshot.Width, mc.Snapshot.Height), m_fPixelSize);
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
