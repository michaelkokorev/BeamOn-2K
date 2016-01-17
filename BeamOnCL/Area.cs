using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace BeamOnCL
{
    public class Area
    {
        public enum Figure { enCircle, enEllipse, enRectangle };

        PointF m_pfCentroid = new PointF();
        double m_dMinorRadius = 0f;
        double m_dMajorRadius = 0f;
        double m_dAngle = 0;
        double m_dCos;
        double m_dSin;

        double m_dCosSq;
        double m_dSinSq;
        double m_dCoef_AX;
        double m_dCoef_AY;
        double m_dCoef_BX;
        double m_dCoef_BY;
        double m_dCoef_DY;
        double m_dCoef_C;

        Figure m_fType = Figure.enEllipse;

        Boolean m_bLikeBucket = true;

        public Boolean LikeBucket
        {
            get { return m_bLikeBucket; }
            set { m_bLikeBucket = value; }
        }

        public Area()
        {
            Angle = 0;
        }

        public Figure Type
        {
            get { return m_fType; }
            set { m_fType = value; }
        }

        public double Angle
        {
            get { return m_dAngle; }
            set
            {
                m_dAngle = value;

                m_dCos = Math.Cos(m_dAngle);
                m_dSin = Math.Sin(m_dAngle);
                if (m_dCos == 1) m_dSin = 0f;
                if (m_dSin == 1) m_dCos = 0f;

                m_dCosSq = m_dCos * m_dCos;
                m_dSinSq = m_dSin * m_dSin;

                SetCoeff();
            }
        }

        public double MinorRadius
        {
            get { return m_dMinorRadius; }
            set
            {
                m_dMinorRadius = value;

                SetCoeff();
            }
        }

        public double MajorRadius
        {
            get { return m_dMajorRadius; }
            set
            {
                m_dMajorRadius = value;

                SetCoeff();
            }
        }

        public PointF Centroid
        {
            get { return m_pfCentroid; }
            set
            {
                m_pfCentroid = value;
            }
        }

        private void SetCoeff()
        {
            m_dCoef_AX = m_dMinorRadius * m_dMinorRadius * m_dSinSq + m_dMajorRadius * m_dMajorRadius * m_dCosSq;
            m_dCoef_BX = m_dCos * m_dSin * (m_dMajorRadius * m_dMajorRadius - m_dMinorRadius * m_dMinorRadius);
            m_dCoef_AY = m_dMinorRadius * m_dMinorRadius * m_dCosSq + m_dMajorRadius * m_dMajorRadius * m_dSinSq;
            m_dCoef_BY = m_dCos * m_dSin * (m_dMajorRadius * m_dMajorRadius + m_dMinorRadius * m_dMinorRadius);
            m_dCoef_DY = m_dMajorRadius * m_dMajorRadius * m_dSinSq - m_dMinorRadius * m_dMinorRadius * m_dCosSq;
            m_dCoef_C = m_dMajorRadius * m_dMinorRadius;
        }

        public void GetMaxY(ref float pfY1, ref float pfY2)
        {
            double dDelta;
            float fTmp;

            switch (Type)
            {
                case Figure.enRectangle:
                    {
                        dDelta = Math.Abs(m_dMinorRadius * m_dCos) + Math.Abs(m_dMajorRadius * m_dSin);
                        pfY1 = (float)(Centroid.Y - dDelta);
                        pfY2 = (float)(Centroid.Y + dDelta);
                        if (pfY1 > pfY2)
                        {
                            fTmp = pfY1;
                            pfY1 = pfY2;
                            pfY2 = fTmp;
                        }
                    }
                    break;
                case Figure.enCircle:
                //pfY1 = (float)(Centroid.Y - m_pdCircleRadius);
                //pfY2 = (float)(Centroid.Y + m_pdCircleRadius);
                //break;
                case Figure.enEllipse:
                    pfY1 = (float)(Centroid.Y - Math.Sqrt(m_dCoef_AY));
                    pfY2 = (float)(Centroid.Y + Math.Sqrt(m_dCoef_AY));
                    break;
            }
        }

        public Boolean GetCrossX(float fY, ref float pfX1, ref float pfX2)
        {
            Boolean bRet = false;
            double newY;
            double newYSq;
            double dDelta;
            float fTmp;

            newY = fY - Centroid.Y;
            newYSq = newY * newY;

            switch (Type)
            {
                case Figure.enRectangle:

                    dDelta = Math.Abs(m_dMinorRadius * m_dCos) + Math.Abs(m_dMajorRadius * m_dSin);

                    if (Math.Abs(newY) <= dDelta)
                    {
                        dDelta = Math.Abs(m_dMajorRadius * m_dCos) + Math.Abs(m_dMinorRadius * m_dSin);
                        bRet = true;

                        if ((Math.Abs(m_dCos) > 0.0001) && (Math.Abs(m_dSin) > 0.0001))
                        {
                            if (dDelta >= Math.Abs((newY * m_dSin - m_dMajorRadius) / m_dCos))
                                pfX1 = (float)(Centroid.X + (newY * m_dSin - m_dMajorRadius) / m_dCos);
                            else if ((dDelta >= Math.Abs((newY * m_dSin + m_dMajorRadius) / m_dCos)))
                                pfX1 = (float)(Centroid.X + (newY * m_dSin + m_dMajorRadius) / m_dCos);
                            else if (dDelta >= Math.Abs((newY * m_dCos - m_dMinorRadius) / m_dSin))
                                pfX1 = (float)(Centroid.X - (newY * m_dCos - m_dMinorRadius) / m_dSin);
                            else if (dDelta >= Math.Abs((newY * m_dCos + m_dMinorRadius) / m_dSin))
                                pfX1 = (float)(Centroid.X - (newY * m_dCos + m_dMinorRadius) / m_dSin);

                            if (dDelta >= Math.Abs((newY * m_dSin - m_dMajorRadius) / m_dCos))
                                pfX2 = (float)(Centroid.X + (newY * m_dSin - m_dMajorRadius) / m_dCos);
                            if ((dDelta >= Math.Abs((newY * m_dSin + m_dMajorRadius) / m_dCos)))
                                pfX2 = (float)(Centroid.X + (newY * m_dSin + m_dMajorRadius) / m_dCos);
                            if (dDelta >= Math.Abs((newY * m_dCos - m_dMinorRadius) / m_dSin))
                                pfX2 = (float)(Centroid.X - (newY * m_dCos - m_dMinorRadius) / m_dSin);
                            if (dDelta >= Math.Abs((newY * m_dCos + m_dMinorRadius) / m_dSin))
                                pfX2 = (float)(Centroid.X - (newY * m_dCos + m_dMinorRadius) / m_dSin);

                        }
                        else if (Math.Abs(m_dCos) > 0.0001)
                        {
                            pfX1 = (float)(Centroid.X - m_dMajorRadius);
                            pfX2 = (float)(Centroid.X + m_dMajorRadius);
                        }
                        else
                        {
                            pfX1 = (float)(Centroid.X - m_dMinorRadius);
                            pfX2 = (float)(Centroid.X + m_dMinorRadius);
                        }

                        if (pfX1 > pfX2)
                        {
                            fTmp = pfX1;
                            pfX1 = pfX2;
                            pfX2 = fTmp;
                        }
                    }
                    break;
                case Figure.enCircle:
                    //dDelta = (m_pdCircleRadius * m_pdCircleRadius) - newYSq;

                    //if (dDelta >= 0)
                    //{
                    //    bRet = true;
                    //    pfX1 = (float)(Centroid.X - Math.Sqrt(dDelta));
                    //    pfX2 = (float)(Centroid.X + Math.Sqrt(dDelta));
                    //}
                    break;
                case Figure.enEllipse:
                    dDelta = m_dCoef_AY - newYSq;

                    if (dDelta >= 0)
                    {
                        bRet = true;
                        pfX1 = (float)(Centroid.X + (m_dCoef_BX * newY - m_dCoef_C * Math.Sqrt(dDelta)) / m_dCoef_AY);
                        pfX2 = (float)(Centroid.X + (m_dCoef_BX * newY + m_dCoef_C * Math.Sqrt(dDelta)) / m_dCoef_AY);
                    }
                    break;
            }

            return bRet;
        }

        public static implicit operator Area(Rectangle sarRect)
        {
            Area tmp = new Area();

            tmp.Centroid = new PointF(sarRect.Left + sarRect.Width / 2f, sarRect.Top + (float)sarRect.Height / 2f);
            tmp.MajorRadius = (sarRect.Height * tmp.m_dSin - sarRect.Width * tmp.m_dCos) / (tmp.m_dSinSq - tmp.m_dCosSq);
            tmp.MinorRadius = (sarRect.Width * tmp.m_dSin - sarRect.Height * tmp.m_dCos) / (tmp.m_dSinSq - tmp.m_dCosSq);

            tmp.MajorRadius /= 2.0;
            tmp.MinorRadius /= 2.0;

            return tmp;
        }

        public static implicit operator Area(RectangleF sarRect)
        {
            Area tmp = new Area();

            tmp.Centroid = new PointF(sarRect.Left + sarRect.Width / 2f, sarRect.Top + (float)sarRect.Height / 2f);
            tmp.MajorRadius = (sarRect.Height * tmp.m_dSin - sarRect.Width * tmp.m_dCos) / (tmp.m_dSinSq - tmp.m_dCosSq);
            tmp.MinorRadius = (sarRect.Width * tmp.m_dSin - sarRect.Height * tmp.m_dCos) / (tmp.m_dSinSq - tmp.m_dCosSq);

            tmp.MajorRadius /= 2.0;
            tmp.MinorRadius /= 2.0;

            return tmp;
        }

        public static implicit operator RectangleF(Area sarArea)
        {
            double A11, A21, A12, A22;
            float[] DeltaX = new float[4];
            float[] DeltaY = new float[4];
            RectangleF rect = new RectangleF();

            A11 = sarArea.MajorRadius * sarArea.m_dCos;
            A21 = -sarArea.MajorRadius * sarArea.m_dSin;
            A12 = sarArea.MinorRadius * sarArea.m_dSin;
            A22 = sarArea.MinorRadius * sarArea.m_dCos;

            DeltaX[0] = (float)(-A11 - A12 + sarArea.Centroid.X);
            DeltaX[1] = (float)(-A11 + A12 + sarArea.Centroid.X);
            DeltaX[2] = (float)(A11 + A12 + sarArea.Centroid.X);
            DeltaX[3] = (float)(A11 - A12 + sarArea.Centroid.X);

            if (DeltaX[0] < DeltaX[1])
            {
                rect.X = DeltaX[0];
                rect.Width = DeltaX[1] - DeltaX[0];
            }
            else
            {
                rect.X = DeltaX[1];
                rect.Width = DeltaX[0] - DeltaX[1];
            }

            if (rect.X > DeltaX[2])
                rect.X = DeltaX[2];
            else if (rect.Width < (DeltaX[2] - rect.X))
                rect.Width = (DeltaX[2] - rect.X);

            if (rect.X > DeltaX[3])
                rect.X = DeltaX[3];
            else if (rect.Width < (DeltaX[3] - rect.X))
                rect.Width = (DeltaX[3] - rect.X);

            DeltaY[0] = (float)(A21 + A22 + sarArea.Centroid.Y);
            DeltaY[1] = (float)(A21 - A22 + sarArea.Centroid.Y);
            DeltaY[2] = (float)(-A21 - A22 + sarArea.Centroid.Y);
            DeltaY[3] = (float)(-A21 + A22 + sarArea.Centroid.Y);

            if (DeltaY[0] < DeltaY[1])
            {
                rect.Y = DeltaY[0];
                rect.Height = DeltaY[1] - DeltaY[0];
            }
            else
            {
                rect.Y = DeltaY[1];
                rect.Height = DeltaY[0] - DeltaY[1];
            }

            if (rect.Y > DeltaY[2])
                rect.Y = DeltaY[2];
            else if (rect.Height < (DeltaY[2] - rect.Y))
                rect.Height = (DeltaY[2] - rect.Y);

            if (rect.Y > DeltaY[3])
                rect.Y = DeltaY[3];
            else if (rect.Height < (DeltaY[3] - rect.Y))
                rect.Height = (DeltaY[3] - rect.Y);

            return rect;
        }

        public static implicit operator Rectangle(Area sarArea)
        {
            double A11, A21, A12, A22;
            int[] DeltaX = new int[4];
            int[] DeltaY = new int[4];
            Rectangle rect = new Rectangle();

            A11 = sarArea.MajorRadius * sarArea.m_dCos;
            A21 = -sarArea.MajorRadius * sarArea.m_dSin;
            A12 = sarArea.MinorRadius * sarArea.m_dSin;
            A22 = sarArea.MinorRadius * sarArea.m_dCos;

            DeltaX[0] = (int)(-A11 - A12 + sarArea.Centroid.X);
            DeltaX[1] = (int)(-A11 + A12 + sarArea.Centroid.X);
            DeltaX[2] = (int)(A11 + A12 + sarArea.Centroid.X);
            DeltaX[3] = (int)(A11 - A12 + sarArea.Centroid.X);

            if (DeltaX[0] < DeltaX[1])
            {
                rect.X = DeltaX[0];
                rect.Width = DeltaX[1] - DeltaX[0];
            }
            else
            {
                rect.X = DeltaX[1];
                rect.Width = DeltaX[0] - DeltaX[1];
            }

            if (rect.X > DeltaX[2])
                rect.X = DeltaX[2];
            else if (rect.Width < (DeltaX[2] - rect.X))
                rect.Width = (DeltaX[2] - rect.X);

            if (rect.X > DeltaX[3])
                rect.X = DeltaX[3];
            else if (rect.Width < (DeltaX[3] - rect.X))
                rect.Width = (DeltaX[3] - rect.X);

            DeltaY[0] = (int)(A21 + A22 + sarArea.Centroid.Y);
            DeltaY[1] = (int)(A21 - A22 + sarArea.Centroid.Y);
            DeltaY[2] = (int)(-A21 - A22 + sarArea.Centroid.Y);
            DeltaY[3] = (int)(-A21 + A22 + sarArea.Centroid.Y);

            if (DeltaY[0] < DeltaY[1])
            {
                rect.Y = DeltaY[0];
                rect.Height = DeltaY[1] - DeltaY[0];
            }
            else
            {
                rect.Y = DeltaY[1];
                rect.Height = DeltaY[0] - DeltaY[1];
            }

            if (rect.Y > DeltaY[2])
                rect.Y = DeltaY[2];
            else if (rect.Height < (DeltaY[2] - rect.Y))
                rect.Height = (DeltaY[2] - rect.Y);

            if (rect.Y > DeltaY[3])
                rect.Y = DeltaY[3];
            else if (rect.Height < (DeltaY[3] - rect.Y))
                rect.Height = (DeltaY[3] - rect.Y);

            return rect;
        }
    }
}
