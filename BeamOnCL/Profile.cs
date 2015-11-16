using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace BeamOnCL
{
    class Profile
    {
        public enum DrawOrientation { doHorizontal, doVertical };
        protected Double[] m_sDataProfile = null;

        protected RectangleF m_rArea;
        protected Double m_sMaxProfile;
        protected Double m_sMinProfile;

        protected Double m_dAngle = 0;

        protected Double m_dTan = 0;
        protected Double m_dCos = 0;
        protected Double m_dSin = 0;

        protected UInt16 m_uiMaxValue = 0;

        protected PointF m_LeftPoint = new PointF();
        protected PointF m_RightPoint = new PointF();

        protected Point m_CrossPoint = new Point();

        Boolean m_bScaleProfile = false;
        double[] delta = new double[4];

        float m_fClipLevel1 = 13.5f;
        float m_fClipLevel2 = 50f;

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

        public Boolean ScaleProfile
        {
            get { return m_bScaleProfile; }

            set { m_bScaleProfile = value; }
        }

        public Single ClipLevel2
        {
            get { return m_fClipLevel2; }

            set
            {
                if ((value >= 0) && (value <= 100)) m_fClipLevel2 = value;
            }
        }

        public Single ClipLevel1
        {
            get { return m_fClipLevel1; }

            set
            {
                if ((value >= 0) && (value <= 100)) m_fClipLevel1 = value;
            }
        }

        public virtual void Create(SnapshotBase snapshot)
        {
            PointF dot = m_LeftPoint;
            SizeF sf = new SizeF((float)m_dCos, (float)m_dSin);
            UInt32 nCount = 0;

            SizeF sfp = new SizeF(-(float)m_dSin, (float)m_dCos);
            //SizeF sfp = new SizeF(-(float)m_dSin, -(float)m_dCos);
            SizeF sfp1 = new SizeF((float)m_dSin, (float)m_dCos);

            try
            {
                if ((snapshot != null) && (m_sDataProfile != null))
                {
                    m_uiMaxValue = (snapshot.GetType() == typeof(Snapshot<byte>)) ? (UInt16)255 : (UInt16)4095;
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

                    for (int i = 0; i < m_sDataProfile.Length; i++) m_sDataProfile[i] -= m_sMinProfile;
                    m_sMaxProfile -= m_sMinProfile;
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
        }
    }
}
