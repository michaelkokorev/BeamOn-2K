using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace BeamOnCL
{
    public class Profile
    {
        public Double[] m_sDataProfile = null;
        public Gaussian m_sGaussian = null;

        protected RectangleF m_rArea;
        protected Double m_sMaxProfile;
        protected Double m_sMinProfile;

        protected Double m_dAngle = 0;

        protected Double m_dTan = 0;
        protected Double m_dCos = 0;
        protected Double m_dSin = 0;

        protected PointF m_LeftPoint = new PointF();
        protected PointF m_RightPoint = new PointF();

        protected Point m_CrossPoint = new Point();

        protected int m_iCentroid = 0;

        double[] delta = new double[4];

        public Gaussian GaussianData
        {
            get { return m_sGaussian; }
        }

        public Double[] DataProfile
        {
            get { return m_sDataProfile; }
        }

        public Profile(Rectangle rArea)
        {
            m_rArea = rArea;
            Angle = 0;
        }

        public Point CrossPoint
        {
            get { return m_CrossPoint; }

            set
            {
                if (m_rArea.Contains(value) == true)
                {
                    m_CrossPoint = value;
                    UpdateCrossPosition();
                }
            }
        }

        public PointF LeftPoint
        {
            get { return m_LeftPoint; }
        }

        public PointF RightPoint
        {
            get { return m_RightPoint; }
        }

        public Double MaxProfile
        {
            get { return m_sMaxProfile; }
            set { m_sMaxProfile = value; }
        }

        public double GetWidth(float iLevel)
        {
            double f_Board;
            int iLeft, iRight;
            double f_Left;
            double f_Right;

            if (iLevel >= 100) return (0);

            f_Board = m_sMaxProfile * iLevel / 100f;

            if (f_Board <= 0) return (0);

            //if(m_wWidthMetod == M_METOD_DIFF_INS)
            //{
            ////Inside
            //    for (iLeft = iStart; ((iLeft > 0) && (plProf[iLeft] > f_Board)); iLeft--);
            //    for (iRight = iStart; ((iRight < plProf.Length) && (plProf[iRight] > f_Board)); iRight++) ;

            //    if (plProf[iLeft + 1] <= plProf[iLeft]) return (0);
            //    f_Left = iLeft + (f_Board - plProf[iLeft]) / (plProf[iLeft + 1] - plProf[iLeft]);

            //    if (plProf[iRight - 1] <= plProf[iRight]) return (0);
            //    f_Right = iRight - (f_Board - plProf[iRight]) / (plProf[iRight - 1] - plProf[iRight]);
            //}
            //else
            //{
            // Outside
            for (iLeft = 0; ((iLeft < m_sDataProfile.Length) && (m_sDataProfile[iLeft] < f_Board)); iLeft++) ;
            for (iRight = m_sDataProfile.Length - 1; ((iRight > 0) && (m_sDataProfile[iRight] < f_Board)); iRight--) ;

            if ((iLeft == 0) || (m_sDataProfile[iLeft - 1] >= m_sDataProfile[iLeft])) return (0);
            f_Left = iLeft - (m_sDataProfile[iLeft] - f_Board) / (m_sDataProfile[iLeft] - m_sDataProfile[iLeft - 1]);

            if ((iRight == m_sDataProfile.Length - 1) || (m_sDataProfile[iRight + 1] >= m_sDataProfile[iRight])) return (0);
            f_Right = iRight + (m_sDataProfile[iRight] - f_Board) / (m_sDataProfile[iRight] - m_sDataProfile[iRight + 1]);
            //}

            return (f_Right - f_Left);
        }

        public virtual void Create(SnapshotBase snapshot)
        {
            PointF dot = m_LeftPoint;
            SizeF sf = new SizeF((float)m_dCos, (float)m_dSin);
            UInt32 nCount = 0;

            SizeF sfp = new SizeF(-(float)m_dSin, (float)m_dCos);
            //SizeF sfp = new SizeF(-(float)m_dSin, -(float)m_dCos);
            SizeF sfp1 = new SizeF((float)m_dSin, (float)m_dCos);

            Double l_Sum = 0;
            Double l_Sum0 = 0;

            try
            {
                if ((snapshot != null) && (m_sDataProfile != null))
                {
                    m_sMaxProfile = 0;
                    m_sMinProfile = Double.MaxValue;

                    for (int i = 0; i < m_sDataProfile.Length; i++)
                    {
                        if (m_rArea.Contains(dot))
                        {
                            m_sDataProfile[i] = GetPixelColor(snapshot, dot);

                            if (this.GetType() == typeof(SumProfile))
                            {
                                if (true)
                                {
                                    nCount = 1;

                                    PointF pp = dot + sfp;
                                    Int32 iAdress = (Int32)pp.X + (Int32)pp.Y * (Int32)m_rArea.Width;

                                    while (m_rArea.Contains(pp))
                                    {
                                        m_sDataProfile[i] += snapshot.GetPixelColor(iAdress);
                                        pp += sfp;
                                        iAdress = (Int32)pp.X + (Int32)pp.Y * (Int32)m_rArea.Width;
                                        nCount++;
                                    }

                                    pp = dot + sfp1;
                                    iAdress = (Int32)pp.X + (Int32)pp.Y * (Int32)m_rArea.Width;

                                    while (m_rArea.Contains(pp))
                                    {
                                        m_sDataProfile[i] += snapshot.GetPixelColor(iAdress);
                                        pp += sfp1;
                                        iAdress = (Int32)pp.X + (Int32)pp.Y * (Int32)m_rArea.Width;
                                        nCount++;
                                    }
                                }
                                else
                                {
                                    nCount = 1;

                                    PointF pp = dot + sfp;

                                    while (m_rArea.Contains(pp))
                                    {
                                        m_sDataProfile[i] += GetPixelColor(snapshot, pp);
                                        pp += sfp;
                                        nCount++;
                                    }

                                    pp = dot + sfp1;

                                    while (m_rArea.Contains(pp))
                                    {
                                        m_sDataProfile[i] += GetPixelColor(snapshot, pp);
                                        pp += sfp1;
                                        nCount++;
                                    }
                                }

                                m_sDataProfile[i] /= (float)nCount;
                            }

                            if (m_sMaxProfile < m_sDataProfile[i]) m_sMaxProfile = m_sDataProfile[i];
                            if (m_sMinProfile > m_sDataProfile[i]) m_sMinProfile = m_sDataProfile[i];
                        }

                        dot += sf;
                    }

                    float f_Threshold = (float)((m_sMaxProfile - m_sMinProfile) * 0.2);

                    for (int i = 0; i < m_sDataProfile.Length; i++)
                    {
                        m_sDataProfile[i] = (m_sDataProfile[i] > m_sMinProfile) ? m_sDataProfile[i] - m_sMinProfile : 0;

                        if (m_sDataProfile[i] > f_Threshold)
                        {
                            l_Sum += m_sDataProfile[i];
                            l_Sum0 += m_sDataProfile[i] * i;
                        }
                    }

                    m_sMaxProfile -= m_sMinProfile;


                    m_iCentroid = (l_Sum == 0) ? (int)(snapshot.Height / 2f) : (int)(l_Sum0 / l_Sum);


                    m_sGaussian.Create(m_sDataProfile, m_sMaxProfile, m_iCentroid, l_Sum);
                }
            }
            catch { }
        }

        protected Double GetPixelColor(SnapshotBase snapshot, PointF point)
        {
            Double dColor = 0;
            double deltaSum = 0;
            double deltaR = 0;

            Point sPoint = new Point((int)Math.Floor(point.X), (int)Math.Floor(point.Y));
            Int32 iAdress = sPoint.X + sPoint.Y * (Int32)m_rArea.Width;

            if (true)
            {
                dColor = snapshot.GetPixelColor(iAdress);
            }
            else
            {
                SizeF szf = new SizeF((float)(point.X - sPoint.X), (float)(point.Y - sPoint.Y));

                deltaR = szf.Width * szf.Width + szf.Height * szf.Height;

                delta[0] = 1 - Math.Sqrt(deltaR);
                delta[1] = 1 - Math.Sqrt(deltaR - 2 * szf.Width + 1);
                delta[2] = 1 - Math.Sqrt(deltaR - 2 * (szf.Width - szf.Height + 1));
                delta[3] = 1 - Math.Sqrt(deltaR - 2 * szf.Height + 1);

                if (delta[0] < 0) delta[0] = 0f;
                if (delta[1] < 0) delta[1] = 0f;
                if (delta[2] < 0) delta[2] = 0f;
                if (delta[3] < 0) delta[3] = 0f;

                deltaSum = delta[0] + delta[1] + delta[2] + delta[3];

                dColor = (delta[0] != 0) ? snapshot.GetPixelColor(iAdress) * delta[0] / deltaSum : 0;

                if ((sPoint.X != (m_rArea.Width - 1)) && (sPoint.Y != (m_rArea.Height - 1)))
                {
                    if (delta[1] != 0) dColor += snapshot.GetPixelColor(iAdress + 1) * delta[1] / deltaSum;
                    if (delta[2] != 0) dColor += snapshot.GetPixelColor(iAdress + 1 + (Int32)m_rArea.Width) * delta[2] / deltaSum;
                    if (delta[3] != 0) dColor += snapshot.GetPixelColor(iAdress + (Int32)m_rArea.Width) * delta[3] / deltaSum;
                }
                else if ((delta[1] != 0) && (sPoint.X != (m_rArea.Width - 1)))
                    dColor += snapshot.GetPixelColor(iAdress + 1) * delta[1] / deltaSum;
                else if ((delta[3] != 0) && (sPoint.Y != (m_rArea.Height - 1)))
                    dColor += snapshot.GetPixelColor(iAdress + (Int32)m_rArea.Width) * delta[3] / deltaSum;
            }

            return dColor;
        }

        public Double Angle
        {
            get { return m_dAngle; }

            set
            {
                if ((value >= -Math.PI / 2f) && (value <= Math.PI / 2f))
                    m_dAngle = value;
                else if (value <= -Math.PI / 2f)
                    m_dAngle = value + Math.PI;
                else
                    m_dAngle = value - Math.PI;


                m_dTan = Math.Tan(m_dAngle);
                m_dCos = Math.Cos(m_dAngle);
                m_dSin = Math.Sin(m_dAngle);

                UpdateCrossPosition();
            }
        }

        protected void UpdateCrossPosition()
        {
            if (Angle == 0)
            {
                m_LeftPoint.X = m_rArea.Left;
                m_RightPoint.X = m_rArea.Right;
                m_LeftPoint.Y = m_RightPoint.Y = m_CrossPoint.Y;
            }
            else if (Math.Abs(Math.Abs(Angle) - Math.PI / 2f) < 0.001)
            {
                m_LeftPoint.X = m_RightPoint.X = m_CrossPoint.X;
                m_LeftPoint.Y = m_rArea.Top;
                m_RightPoint.Y = m_rArea.Bottom;
            }
            else
            {
                Double dLeftY = m_CrossPoint.Y + (m_rArea.Left - m_CrossPoint.X) * m_dTan;
                Double dRightY = m_CrossPoint.Y + (m_rArea.Right - m_CrossPoint.X) * m_dTan;

                if ((dLeftY >= m_rArea.Top) && (dLeftY <= m_rArea.Bottom))
                {
                    m_LeftPoint.X = m_rArea.Left;
                    m_LeftPoint.Y = (float)dLeftY;
                }
                else
                {
                    if (dLeftY > m_rArea.Bottom)
                    {
                        m_LeftPoint.X = (float)(m_CrossPoint.X + (m_rArea.Bottom - m_CrossPoint.Y) / m_dTan);
                        m_LeftPoint.Y = m_rArea.Bottom;
                    }
                    else
                    {
                        m_LeftPoint.X = (float)(m_CrossPoint.X + (m_rArea.Top - m_CrossPoint.Y) / m_dTan);
                        m_LeftPoint.Y = m_rArea.Top;
                    }
                }

                if ((dRightY >= m_rArea.Top) && (dRightY <= m_rArea.Bottom))
                {
                    m_RightPoint.X = m_rArea.Right;
                    m_RightPoint.Y = (float)dRightY;
                }
                else
                {
                    if (dRightY > m_rArea.Bottom)
                    {
                        m_RightPoint.X = (float)(m_CrossPoint.X + (m_rArea.Bottom - m_CrossPoint.Y) / m_dTan);
                        m_RightPoint.Y = m_rArea.Bottom;
                    }
                    else
                    {
                        m_RightPoint.X = (float)(m_CrossPoint.X + (m_rArea.Top - m_CrossPoint.Y) / m_dTan);
                        m_RightPoint.Y = m_rArea.Top;
                    }
                }
            }

            int LenghtProfile = (int)Math.Ceiling(Math.Sqrt(((m_RightPoint.X - m_LeftPoint.X) * (m_RightPoint.X - m_LeftPoint.X)) + ((m_RightPoint.Y - m_LeftPoint.Y) * (m_RightPoint.Y - m_LeftPoint.Y))));

            m_sDataProfile = new double[LenghtProfile];
            m_sGaussian = new Gaussian(LenghtProfile);
        }

        public void ClearProfile()
        {
            ArrayFill<double>(m_sDataProfile, 0f);
        }

        public static void ArrayFill<T>(T[] arrayToFill, T fillValue)
        {
            // if called with a single value, wrap the value in an array and call the main function
            ArrayFill<T>(arrayToFill, new T[] { fillValue });
        }

        public static void ArrayFill<T>(T[] arrayToFill, T[] fillValue)
        {
            if (fillValue.Length >= arrayToFill.Length)
            {
                throw new ArgumentException("fillValue array length must be smaller than length of arrayToFill");
            }

            // set the initial array value
            Array.Copy(fillValue, arrayToFill, fillValue.Length);

            int arrayToFillHalfLength = arrayToFill.Length / 2;

            for (int i = fillValue.Length; i < arrayToFill.Length; i *= 2)
            {
                int copyLength = i;
                if (i > arrayToFillHalfLength)
                {
                    copyLength = arrayToFill.Length - i;
                }

                Array.Copy(arrayToFill, 0, arrayToFill, i, copyLength);
            }
        }
    }
}
