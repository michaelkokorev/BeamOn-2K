using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace BeamOnCL
{
    class SumProfile : Profile
    {
        public SumProfile(Rectangle rArea, float fPixelSize = 5.86f)
            : base(rArea, fPixelSize)
        {
            CrossPoint = new Point(0, 0);
        }

        public override void Create(SnapshotBase snapshot)
        {
            PointF dot = m_LeftPoint;
            SizeF sf = new SizeF((float)m_dCos, (float)m_dSin);
            UInt32 nCount = 0;
            Int32 iAdress;

            SizeF sfp = new SizeF(-(float)m_dSin, (float)m_dCos);
            //SizeF sfp = new SizeF(-(float)m_dSin, -(float)m_dCos);
            SizeF sfp1 = new SizeF((float)m_dSin, (float)m_dCos);
            try
            {
                if ((snapshot != null) && (m_sDataProfile != null))
                {
                    m_sMaxProfile = 0;
                    m_sMinProfile = Double.MaxValue;

                    for (int i = 0; i < m_sDataProfile.Length; i++)
                    {
                        if ((m_rArea.Left <= dot.X) && (m_rArea.Right > dot.X) && (m_rArea.Top <= dot.Y) && (m_rArea.Bottom > dot.Y))
                        {
                            iAdress = (Int32)dot.X + (Int32)dot.Y * (Int32)m_rArea.Width;
                            m_sDataProfile[i] = snapshot.GetPixelColor(iAdress);

                            nCount = 1;

                            PointF pp = dot + sfp;
                            iAdress = (Int32)pp.X + (Int32)pp.Y * (Int32)m_rArea.Width;

                            while ((m_rArea.Left <= pp.X) && (m_rArea.Right > pp.X) && (m_rArea.Top <= pp.Y) && (m_rArea.Bottom > pp.Y))
                            {
                                m_sDataProfile[i] += snapshot.GetPixelColor(iAdress);
                                pp += sfp;
                                iAdress = (Int32)pp.X + (Int32)pp.Y * (Int32)m_rArea.Width;
                                nCount++;
                            }

                            pp = dot + sfp1;
                            iAdress = (Int32)pp.X + (Int32)pp.Y * (Int32)m_rArea.Width;

                            while ((m_rArea.Left <= pp.X) && (m_rArea.Right > pp.X) && (m_rArea.Top <= pp.Y) && (m_rArea.Bottom > pp.Y))
                            {
                                m_sDataProfile[i] += snapshot.GetPixelColor(iAdress);
                                pp += sfp1;
                                iAdress = (Int32)pp.X + (Int32)pp.Y * (Int32)m_rArea.Width;
                                nCount++;
                            }

                            m_sDataProfile[i] /= (float)nCount;

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
    }
}
