using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace BeamOnCL
{
    class Positioning
    {
        const float SCALE_DIV = 8f;
        const float MAX_POWER_OFFSET = 0.05f;
        const float MIN_POWER = 0.8f;

        SumProfile m_dProfileHorizontal = null;
        SumProfile m_dProfileVertical = null;

        double[] m_dProfile45 = null;
        long[] tmpMax = null;

        double m_dProfile45Max = double.MinValue;

        double m_CoefAlgoritmVert = 1.5f;
        double m_CoefAlgoritmHor = 1.5f;

        float m_fLevel = (float)13.5;

        Rectangle m_AreaRect;
        Area m_WorkingAreaRect;
        Area m_EllipseArea = new Area();
        PointF m_pfOldCentroid = new PointF();
        double m_dOldPower = 0;

        double m_dAreaIntensity = 0;
        float m_fCurrentAreaIntensity = 0;
        long m_lCurrentAreaPoins = 0;
        private double m_fCurrentAreaPower = 0;

        public Area Ellipse
        {
            get { return m_EllipseArea; }
        }

        public Rectangle WorkingArea
        {
            get { return m_WorkingAreaRect; }
        }

        public Single ClipLevel
        {
            get { return m_fLevel; }

            set { if ((value > 0) && (value < 100)) m_fLevel = value; }
        }

        public Positioning(Rectangle rect)
        {
            m_AreaRect = rect;

            m_WorkingAreaRect = m_AreaRect;
            m_WorkingAreaRect.Type = Area.Figure.enRectangle;

            m_dProfileHorizontal = new SumProfile(m_AreaRect);
            m_dProfileHorizontal.Angle = 0f;

            m_dProfileVertical = new SumProfile(m_AreaRect);
            m_dProfileVertical.Angle = Math.PI / 2f;

            m_dProfile45 = new double[m_AreaRect.Width + m_AreaRect.Height];

            tmpMax = new long[Math.Max(m_AreaRect.Height, m_AreaRect.Width)];
        }

        public Profile HorizontalProfile
        {
            get { return m_dProfileHorizontal; }
        }

        public Profile VerticalProfile
        {
            get { return m_dProfileVertical; }
        }

        public void GetData(SnapshotBase snapshot)
        {
            Boolean bFlagFirst = false;
            float fLimitDeltaY;
            float fLimitDeltaX;

            Create(snapshot);

            while (true)
            {
                if (WorkingArea != m_AreaRect)
                {
                    if (bFlagFirst)
                    {
                        m_pfOldCentroid = m_EllipseArea.Centroid;
                        m_WorkingAreaRect = SerchRectangle(snapshot);
                        m_dOldPower = m_dAreaIntensity;

                        Create(snapshot);
                    }
                    else
                    {
                        SizeF sfDelta = new SizeF((m_EllipseArea.Centroid.X - m_pfOldCentroid.X), (m_EllipseArea.Centroid.Y - m_pfOldCentroid.Y));

                        RectangleF rectEllipse = m_EllipseArea;
                        Rectangle rect = m_WorkingAreaRect;

                        if ((rectEllipse.Size.Height / SCALE_DIV) > 2) fLimitDeltaY = (rectEllipse.Size.Height / SCALE_DIV);
                        else fLimitDeltaY = 2f;

                        if (Math.Abs(sfDelta.Height) > fLimitDeltaY) rect.Offset(0, (int)sfDelta.Height);

                        if ((rectEllipse.Size.Width / SCALE_DIV) > 2) fLimitDeltaX = (rectEllipse.Size.Width / SCALE_DIV);
                        else fLimitDeltaX = 2f;

                        if (Math.Abs(sfDelta.Width) > fLimitDeltaX) rect.Offset((int)sfDelta.Width, 0);

                        if (m_AreaRect.Contains(rect) == false)
                        {
                            m_WorkingAreaRect = m_AreaRect;
                            continue;
                        }

                        if ((m_dAreaIntensity > MIN_POWER) && (Math.Abs(m_dAreaIntensity / m_dOldPower - 1) > MAX_POWER_OFFSET) ||
                            (m_dAreaIntensity < MIN_POWER))
                        {
                            m_WorkingAreaRect = m_AreaRect;
                            continue;
                        }

                        if (m_AreaRect == rect)
                        {
                            m_WorkingAreaRect = m_AreaRect;

                            m_pfOldCentroid = m_EllipseArea.Centroid;
                            continue;
                        }

                        m_WorkingAreaRect = rect;
                    }

                    break;
                }
                else
                {
                    m_WorkingAreaRect = SerchRectangle(snapshot);
                    bFlagFirst = (WorkingArea != m_AreaRect);
                    if (bFlagFirst == false) break;
                }
            }

            Create2(snapshot);

            m_fCurrentAreaIntensity = GetAreaIntensity(snapshot, ref m_lCurrentAreaPoins);
        }

        public void Create(SnapshotBase snapshot)
        {
            UInt16 uiData = 0;
            double l_dProfileVerticalMin = double.MaxValue;
            double l_dProfileHorizontalMin = double.MaxValue;
            double l_dProfileVerticalMax = 0;
            double l_dProfileHorizontalMax = 0;
            double l_dPowerPointsCount = 0;

            float f_Threshold = 0;
            double l_Sum = 0;
            double l_Sum0 = 0;
            double l_dPower = 0;

            Rectangle rect = m_WorkingAreaRect;

            m_dProfileHorizontal.ClearProfile();
            m_dProfileVertical.ClearProfile();

            m_dProfile45Max = double.MinValue;

            for (int i = rect.Top; i < rect.Bottom; i++)
            {
                for (int j = rect.Left, iShift = j + i * snapshot.Width; j < rect.Right; j++, iShift++)
                {
                    uiData = snapshot.GetPixelColor(iShift);
                    m_dProfileHorizontal.m_sDataProfile[j] += uiData;
                    m_dProfileVertical.m_sDataProfile[i] += uiData;

                    l_dPowerPointsCount++;
                }

                l_dPower += m_dProfileVertical.m_sDataProfile[i];

                if (l_dProfileVerticalMin > m_dProfileVertical.m_sDataProfile[i])
                    l_dProfileVerticalMin = m_dProfileVertical.m_sDataProfile[i];
                else if (l_dProfileVerticalMax < m_dProfileVertical.m_sDataProfile[i])
                    l_dProfileVerticalMax = m_dProfileVertical.m_sDataProfile[i];
            }

            m_dAreaIntensity = (l_dPowerPointsCount > 0) ? (float)(l_dPower / l_dPowerPointsCount) : 0;

            f_Threshold = (float)((l_dProfileVerticalMax - l_dProfileVerticalMin) * 0.2);

            for (int i = rect.Top; i < rect.Bottom; i++)
            {
                m_dProfileVertical.m_sDataProfile[i] = (m_dProfileVertical.m_sDataProfile[i] > l_dProfileVerticalMin) ? m_dProfileVertical.m_sDataProfile[i] - l_dProfileVerticalMin : 0;

                if (m_dProfileVertical.m_sDataProfile[i] > f_Threshold)
                {
                    l_Sum += m_dProfileVertical.m_sDataProfile[i];
                    l_Sum0 += m_dProfileVertical.m_sDataProfile[i] * i;
                }
            }

            m_EllipseArea.Centroid = new PointF(m_EllipseArea.Centroid.X, (l_Sum == 0) ? (float)(snapshot.Height / 2f) : (float)(l_Sum0 / l_Sum));

            l_dProfileVerticalMax -= l_dProfileVerticalMin;

            m_dProfileVertical.m_sGaussian.Create(m_dProfileVertical.m_sDataProfile, l_dProfileVerticalMax, (int)m_EllipseArea.Centroid.Y, l_Sum);

            for (int i = rect.Left; i < rect.Right; i++)
            {
                if (l_dProfileHorizontalMin > m_dProfileHorizontal.m_sDataProfile[i])
                    l_dProfileHorizontalMin = m_dProfileHorizontal.m_sDataProfile[i];
                else if (l_dProfileHorizontalMax < m_dProfileHorizontal.m_sDataProfile[i])
                    l_dProfileHorizontalMax = m_dProfileHorizontal.m_sDataProfile[i];
            }

            f_Threshold = (float)((l_dProfileHorizontalMax - l_dProfileHorizontalMin) * 0.2);

            l_Sum = l_Sum0 = 0;

            for (int i = rect.Left; i < rect.Right; i++)
            {
                m_dProfileHorizontal.m_sDataProfile[i] = (m_dProfileHorizontal.m_sDataProfile[i] > l_dProfileHorizontalMin) ? m_dProfileHorizontal.m_sDataProfile[i] - l_dProfileHorizontalMin : 0;

                if (m_dProfileHorizontal.m_sDataProfile[i] > f_Threshold)
                {
                    l_Sum += m_dProfileHorizontal.m_sDataProfile[i];
                    l_Sum0 += m_dProfileHorizontal.m_sDataProfile[i] * i;
                }
            }

            m_EllipseArea.Centroid = new PointF((l_Sum == 0) ? (float)(snapshot.Width / 2f) : (float)(l_Sum0 / l_Sum), m_EllipseArea.Centroid.Y);

            l_dProfileHorizontalMax -= l_dProfileHorizontalMin;

            m_dProfileHorizontal.m_sGaussian.Create(m_dProfileHorizontal.m_sDataProfile, l_dProfileHorizontalMax, (int)m_EllipseArea.Centroid.X, l_Sum);

            m_dProfileHorizontal.MaxProfile = l_dProfileHorizontalMax;
            m_dProfileVertical.MaxProfile = l_dProfileVerticalMax;
        }

        public void Create2(SnapshotBase snapshot)
        {
            double l_dProfile45Min = double.MaxValue;
            int l_iProfile45Peak = 0;
            int l_Y, l_X, nCount;

            int iLeft = m_dProfile45.Length;
            int iRight = 0;

            Rectangle rect = m_WorkingAreaRect;

            Profile.ArrayFill<double>(m_dProfile45, 0f);

            for (int i = 0; i < m_dProfile45.Length; i++)
            {
                l_Y = (i < snapshot.Width) ? 0 : i - snapshot.Width + 1;
                l_X = (i < snapshot.Width) ? i : snapshot.Width - 1;

                nCount = 0;

                for (; ((l_X >= 0) && (l_Y < snapshot.Height)); l_X--, l_Y++)
                {
                    if ((l_X >= rect.Left) && (l_X <= rect.Right) && (l_Y >= rect.Top) && (l_Y <= rect.Bottom))
                    {
                        if (i < iLeft) iLeft = i;
                        if (i > iRight) iRight = i;

                        m_dProfile45[i] += snapshot.GetPixelColor(l_X + l_Y * snapshot.Width);
                        nCount++;
                    }
                }

                if (nCount > 0) m_dProfile45[i] /= (float)nCount;

                if (m_dProfile45[i] > m_dProfile45Max)
                {
                    m_dProfile45Max = m_dProfile45[i];
                    l_iProfile45Peak = i;
                }
                else if ((i >= iLeft) && (i <= iRight))
                {
                    if (m_dProfile45[i] < l_dProfile45Min) l_dProfile45Min = m_dProfile45[i];
                }
            }

            for (int i = 0; i < m_dProfile45.Length; i++)
                m_dProfile45[i] = (m_dProfile45[i] > l_dProfile45Min) ? m_dProfile45[i] - l_dProfile45Min : 0f;

            m_dProfile45Max -= l_dProfile45Min;

            double fWidthH45 = GetWidth(m_dProfileHorizontal.DataProfile, m_fLevel, m_dProfileHorizontal.MaxProfile);
            double fWidthV45 = GetWidth(m_dProfileVertical.DataProfile, m_fLevel, m_dProfileVertical.MaxProfile);
            double fWidthV = GetWidth(m_dProfile45, m_fLevel, m_dProfile45Max) / Math.Sqrt(2f);

            double Wo0 = fWidthV45 * fWidthV45 / 4.0;
            double Wo1 = (fWidthV45 + fWidthH45) / Math.Sqrt(2f);
            double Wo2 = fWidthH45 * fWidthH45 / 4.0;

            if (Wo1 > fWidthV)
                Wo1 = fWidthV * fWidthV / 4.0;
            else
                Wo1 = Wo1 * Wo1 / 4.0;

            double Co, Si, Si2, Co2, U;
            double X = (Wo0 + Wo2) / 2.0;
            double Y = Wo0 - X;
            double Z = X - Wo1;

            if ((Math.Abs(Y) < 0.0001 * X) && (Math.Abs(Z) < 0.0001 * X))
            {
                //		m_ElipseArea.SetMajorRadius(fWidthV45 / 2.0);
                //		m_ElipseArea.SetMinorRadius(m_Area.GetMajorRadius());
                Co = 1;
                Si = 0;
            }
            else
            {
                if (Math.Abs(Z) < Math.Abs(Y))
                {
                    if (Y != 0)
                    {
                        Si2 = Z / Y;
                        Si2 = 1 / Math.Sqrt(1 + Si2 * Si2);
                        Co2 = Math.Sqrt(1 - Si2 * Si2);
                        if (Z < 0) Co2 = -Co2;
                        U = Math.Abs(Y / Si2);
                    }
                    else
                    {
                        Co2 = 1;
                        U = 0;
                    }
                }
                else
                {
                    if (Z != 0)
                    {
                        Co2 = Y / Z;
                        Co2 = 1 / Math.Sqrt(1 + Co2 * Co2);
                        if (Z < 0) Co2 = -Co2;
                        U = Z / Co2;
                    }
                    else
                    {
                        Co2 = 1;
                        U = 0;
                    }
                }

                Si = Math.Sqrt((1 - Co2) / 2.0);

                if (Y < 0) Si = -Si;

                Co = Math.Sqrt((1 + Co2) / 2.0);
                //Old Version Ellipse
                //m_EllipseArea.MajorRadius = X + U;
                //m_EllipseArea.MinorRadius = X - U;

                //if (m_EllipseArea.MinorRadius <= 0.01 * m_EllipseArea.MajorRadius)
                //    m_EllipseArea.MinorRadius = Math.Sqrt(0.01 * m_EllipseArea.MajorRadius);
                //else
                //    m_EllipseArea.MinorRadius = Math.Sqrt(m_EllipseArea.MinorRadius);

                //m_EllipseArea.MajorRadius = Math.Sqrt(m_EllipseArea.MajorRadius);
                //
            }

            //			m_Area.SetAngle(-atan(Si/Co));
            if (Math.Abs(-Math.Atan(Si / Co) - Math.PI / 4f) > Math.PI / 2f)
            {
                if ((-Math.Atan(Si / Co) - Math.PI / 4f) < 0)
                    m_EllipseArea.Angle = Math.PI - Math.Atan(Si / Co) - Math.PI / 4f;
                else
                    m_EllipseArea.Angle = -Math.PI - Math.Atan(Si / Co) - Math.PI / 4f;
            }
            else
                m_EllipseArea.Angle = -Math.Atan(Si / Co) - Math.PI / 4f;
            //New Version 
            GetAxesEllipse(snapshot);
        }

        private void GetAxesEllipse(SnapshotBase snapshot)
        {
            double l_dCosAlfa;
            double l_dSinAlfa;
            double l_dTanAlfa;
            float l_fRight, l_fLeft, l_fTop, l_fBottom, l_fRightP, l_fLeftP;
            int l_lSizeH, l_lSizeV;
            int iLeft, iRight, iShift;
            float f_Threshold;
            int j, i, n_Peak;
            double K, Xl, Yl, Xlg, Ylg;
            UInt16 wColorValue = 0;
            double n_MaxColor;
            double n_MinColor;
            Rectangle rect = m_WorkingAreaRect;

            double[] tmpProfH = new double[m_AreaRect.Width + m_AreaRect.Height];
            double[] tmpProfV = new double[m_AreaRect.Width + m_AreaRect.Height];

            if (Math.Abs(m_EllipseArea.Angle) == Math.PI / 2f)
            {
                l_fLeft = (float)rect.Top;
                l_fRight = (float)rect.Bottom;
                l_fTop = (float)rect.Left;
                l_fBottom = (float)rect.Right;

                l_lSizeH = (int)Math.Ceiling(l_fRight - l_fLeft);
                l_lSizeV = (int)Math.Ceiling(l_fBottom - l_fTop);

                for (j = 0; j < l_lSizeV; j++)
                {
                    iShift = (int)(l_fTop + j) + (int)(l_fLeft * m_AreaRect.Width);

                    for (i = 0; i < l_lSizeH; i++)
                    {
                        wColorValue = snapshot.GetPixelColor(iShift + (int)(i * m_AreaRect.Width));

                        tmpProfH[i] += wColorValue;
                        tmpProfV[j] += wColorValue;
                    }
                }
            }
            else if (m_EllipseArea.Angle == 0)
            {
                l_fLeft = (float)rect.Left;
                l_fRight = (float)rect.Right;
                l_fTop = (float)rect.Top;
                l_fBottom = (float)rect.Bottom;

                l_lSizeH = (int)Math.Ceiling((l_fRight - l_fLeft));
                l_lSizeV = (int)Math.Ceiling((l_fBottom - l_fTop));

                for (j = 0; j < l_lSizeV; j++)
                {
                    iShift = (int)(l_fLeft + j) + (int)(l_fTop * m_AreaRect.Width);

                    for (i = 0; i < l_lSizeH; i++)
                    {
                        wColorValue = snapshot.GetPixelColor(iShift + (int)(i * m_AreaRect.Width));

                        tmpProfH[i] += wColorValue;
                        tmpProfV[j] += wColorValue;
                    }
                }
            }
            else
            {
                l_dCosAlfa = Math.Cos(m_EllipseArea.Angle);
                l_dSinAlfa = Math.Sin(m_EllipseArea.Angle);
                l_dTanAlfa = Math.Tan(m_EllipseArea.Angle);

                l_fLeft = (float)(rect.Top - m_EllipseArea.Centroid.Y);
                l_fRight = (float)(rect.Bottom - m_EllipseArea.Centroid.Y);

                l_fTop = (float)((rect.Top - m_EllipseArea.Centroid.Y) * l_dTanAlfa);
                l_fBottom = (float)((rect.Bottom - m_EllipseArea.Centroid.Y) * l_dTanAlfa);

                if (l_fTop < (rect.Left - m_EllipseArea.Centroid.X))
                    l_fLeft = (float)((rect.Left - m_EllipseArea.Centroid.X) / l_dTanAlfa);

                if (l_fTop > (rect.Right - m_EllipseArea.Centroid.X))
                    l_fLeft = (float)((rect.Right - m_EllipseArea.Centroid.X) / l_dTanAlfa);

                if (l_fBottom < (rect.Left - m_EllipseArea.Centroid.X))
                    l_fRight = (float)((rect.Left - m_EllipseArea.Centroid.X) / l_dTanAlfa);

                if (l_fBottom > (rect.Right - m_EllipseArea.Centroid.X))
                    l_fRight = (float)((rect.Right - m_EllipseArea.Centroid.X) / l_dTanAlfa);

                l_fBottom = (float)(rect.Right - m_EllipseArea.Centroid.X);
                l_fTop = (float)(rect.Left - m_EllipseArea.Centroid.X);

                l_fLeftP = (float)((rect.Left - m_EllipseArea.Centroid.X) * l_dTanAlfa);
                l_fRightP = (float)((rect.Right - m_EllipseArea.Centroid.X) * l_dTanAlfa);

                if (l_fLeftP < (rect.Top - m_EllipseArea.Centroid.Y))
                    l_fTop = (float)((rect.Top - m_EllipseArea.Centroid.Y) / l_dTanAlfa);

                if (l_fLeftP > (rect.Bottom - m_EllipseArea.Centroid.Y))
                    l_fTop = (float)((rect.Bottom - m_EllipseArea.Centroid.Y) / l_dTanAlfa);

                if (l_fRightP < (rect.Top - m_EllipseArea.Centroid.Y))
                    l_fBottom = (float)((rect.Top - m_EllipseArea.Centroid.Y) / l_dTanAlfa);

                if (l_fRightP > (rect.Bottom - m_EllipseArea.Centroid.Y))
                    l_fBottom = (float)((rect.Bottom - m_EllipseArea.Centroid.Y) / l_dTanAlfa);

                l_lSizeV = (int)Math.Ceiling((l_fRight - l_fLeft) / l_dCosAlfa);
                l_lSizeH = (int)Math.Ceiling((l_fBottom - l_fTop) / l_dCosAlfa);

                if ((l_lSizeH > 0) && (l_lSizeV > 0))
                {
                    for (j = 0; j < l_lSizeV; j++)
                    {
                        K = -l_dTanAlfa * (l_fLeft + j * l_dCosAlfa);
                        Xl = l_fTop + K;
                        Yl = l_dTanAlfa * Xl - K / (l_dCosAlfa * l_dSinAlfa);
                        Xlg = Xl + m_EllipseArea.Centroid.X;
                        Ylg = Yl + m_EllipseArea.Centroid.Y;

                        iShift = (int)Ylg * m_AreaRect.Width + (int)Xlg;

                        for (i = 0; i < l_lSizeH; i++)
                        {
                            if (((Ylg + i * l_dSinAlfa) >= 0) && ((Ylg + i * l_dSinAlfa) < m_AreaRect.Height) && ((Xlg + i * l_dCosAlfa) >= 0) && ((Xlg + i * l_dCosAlfa) < m_AreaRect.Width))
                            {
                                wColorValue = snapshot.GetPixelColor(iShift + (int)(i * l_dSinAlfa) * m_AreaRect.Width + (int)(i * l_dCosAlfa));

                                tmpProfH[i] += wColorValue;
                                tmpProfV[j] += wColorValue;
                            }
                        }
                    }
                }
            }

            if (l_lSizeH > 0)
            {
                n_MaxColor = n_MinColor = tmpProfH[0];
                n_Peak = 0;

                for (i = 1; i < l_lSizeH; i++)
                {
                    if (n_MaxColor < tmpProfH[i])
                    {
                        n_MaxColor = tmpProfH[i];
                        n_Peak = i;
                    }
                    else if (n_MinColor > tmpProfH[i])
                        n_MinColor = tmpProfH[i];
                }

                f_Threshold = (float)((n_MaxColor - n_MinColor) * 0.135f + n_MinColor);

                for (iLeft = 0; (iLeft < n_Peak) && (tmpProfH[iLeft] < f_Threshold); iLeft++) ;
                for (iRight = l_lSizeH - 1; (iRight > n_Peak) && (tmpProfH[iRight] < f_Threshold); iRight--) ;

                m_EllipseArea.MajorRadius = iRight - iLeft;

                if ((iLeft > 0) && (iRight < (l_lSizeH - 1)))
                    m_EllipseArea.MajorRadius = (m_EllipseArea.MajorRadius + (tmpProfH[iRight] - f_Threshold) / (tmpProfH[iRight] - tmpProfH[iRight + 1]) + (tmpProfH[iLeft] - f_Threshold) / (tmpProfH[iLeft] - tmpProfH[iLeft - 1])) / 2f;
            }

            if (l_lSizeV > 0)
            {
                n_MaxColor = n_MinColor = tmpProfV[0];
                n_Peak = 0;

                for (i = 1; i < l_lSizeV; i++)
                {
                    if (n_MaxColor < tmpProfV[i])
                    {
                        n_MaxColor = tmpProfV[i];
                        n_Peak = i;
                    }
                    else if (n_MinColor > tmpProfV[i])
                        n_MinColor = tmpProfV[i];
                }

                f_Threshold = (float)((n_MaxColor - n_MinColor) * 0.135f + n_MinColor);

                for (iLeft = 0; (iLeft < n_Peak) && (tmpProfV[iLeft] < f_Threshold); iLeft++) ;
                for (iRight = l_lSizeV - 1; (iRight > n_Peak) && (tmpProfV[iRight] < f_Threshold); iRight--) ;

                m_EllipseArea.MinorRadius = iRight - iLeft;

                if ((iLeft > 0) && (iRight < (l_lSizeV - 1)))
                    m_EllipseArea.MinorRadius = (m_EllipseArea.MinorRadius + (tmpProfV[iRight] - f_Threshold) / (tmpProfV[iRight] - tmpProfV[iRight + 1]) + (tmpProfV[iLeft] - f_Threshold) / (tmpProfV[iLeft] - tmpProfV[iLeft - 1])) / 2f;
            }
        }

        double GetWidth(double[] plProf, float iLevel, double lMax)
        {
            double f_Board;
            int iLeft, iRight;
            double f_Left;
            double f_Right;

            if (iLevel >= 100) return (0);

            f_Board = lMax * iLevel / 100f;

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
            for (iLeft = 0; ((iLeft < plProf.Length) && (plProf[iLeft] < f_Board)); iLeft++) ;
            for (iRight = plProf.Length - 1; ((iRight > 0) && (plProf[iRight] < f_Board)); iRight--) ;

            if ((iLeft == 0) || (plProf[iLeft - 1] >= plProf[iLeft])) return (0);
            f_Left = iLeft - (plProf[iLeft] - f_Board) / (plProf[iLeft] - plProf[iLeft - 1]);

            if ((iRight == plProf.Length - 1) || (plProf[iRight + 1] >= plProf[iRight])) return (0);
            f_Right = iRight + (plProf[iRight] - f_Board) / (plProf[iRight] - plProf[iRight + 1]);
            //}

            return (f_Right - f_Left);
        }

        RectangleF SerchRectangle(SnapshotBase snapshot)
        {
            float fCentroid;
            float fWidth = 0, fHeight = 0;
            long n_MaxColor = long.MinValue;
            long n_MinColor = long.MaxValue;
            long n_CurrentColor;
            double l_Sum;
            double l_Sum0;
            float f_Threshold;
            float fLeftTmp = 0;
            float fTopTmp = 0;
            int iLeft, iRight;
            float fLeft, fRight;
            int iPeak = 0;

            for (int i = 0; i < m_AreaRect.Width; i++)
            {
                tmpMax[i] = 0;

                for (int j = 0, iOffset = i; j < m_AreaRect.Height; j++, iOffset += m_AreaRect.Width)
                {
                    if (tmpMax[i] < snapshot.GetPixelColor(iOffset)) tmpMax[i] = snapshot.GetPixelColor(iOffset);
                }

                if (n_MaxColor < tmpMax[i])
                {
                    n_MaxColor = tmpMax[i];
                    iPeak = i;
                }
                else if (n_MinColor > tmpMax[i])
                {
                    n_MinColor = tmpMax[i];
                }
            }

            if (n_MaxColor > 0)
            {
                f_Threshold = (float)((n_MaxColor - n_MinColor) * 0.135f + n_MinColor);

                for (iLeft = 0; ((iLeft < iPeak) && (tmpMax[iLeft] < f_Threshold)); iLeft++) ;
                for (iRight = m_AreaRect.Width - 1; ((iRight > iPeak) && (tmpMax[iRight] < f_Threshold)); iRight--) ;

                fLeft = (iLeft > 0) ? (float)(iLeft - 1 + (f_Threshold - tmpMax[iLeft - 1]) / (float)(tmpMax[iLeft] - tmpMax[iLeft - 1])) : (float)0;
                fRight = (iRight < (m_AreaRect.Width - 1)) ? iRight + 1 - (f_Threshold - tmpMax[iRight + 1]) / (float)(tmpMax[iRight] - tmpMax[iRight + 1]) : m_AreaRect.Width - 1;

                fWidth = (float)(2 * m_CoefAlgoritmHor * (fRight - fLeft));

                l_Sum = l_Sum0 = 0;

                for (int i = 0; i < m_AreaRect.Width; i++)
                {
                    n_CurrentColor = tmpMax[i];

                    if (f_Threshold < n_CurrentColor)
                    {
                        l_Sum += n_CurrentColor;
                        l_Sum0 += n_CurrentColor * i;
                    }
                }

                fCentroid = (float)(l_Sum0 / l_Sum);

                fLeftTmp = fCentroid - fWidth / 2f;
            }

            n_MaxColor = long.MinValue;
            n_MinColor = long.MaxValue;

            for (int i = 0; i < m_AreaRect.Height; i++)
            {
                tmpMax[i] = 0;

                for (int j = 0, iOffset = i * m_AreaRect.Width; j < m_AreaRect.Width; j++, iOffset++)
                {
                    if (tmpMax[i] < snapshot.GetPixelColor(iOffset)) tmpMax[i] = snapshot.GetPixelColor(iOffset);
                }

                if (n_MaxColor < tmpMax[i])
                {
                    n_MaxColor = tmpMax[i];
                    iPeak = i;
                }
                else if (n_MinColor > tmpMax[i])
                {
                    n_MinColor = tmpMax[i];
                }
            }

            if (n_MaxColor > 0)
            {
                f_Threshold = (float)((n_MaxColor - n_MinColor) * 0.135f + n_MinColor);

                for (iLeft = 0; ((iLeft < iPeak) && (tmpMax[iLeft] < f_Threshold)); iLeft++) ;
                for (iRight = m_AreaRect.Height - 1; ((iRight > iPeak) && (tmpMax[iRight] < f_Threshold)); iRight--) ;

                fLeft = (iLeft > 0) ? (float)(iLeft - 1 + (f_Threshold - tmpMax[iLeft - 1]) / (float)(tmpMax[iLeft] - tmpMax[iLeft - 1])) : (float)0;
                fRight = (iRight < (m_AreaRect.Height - 1)) ? iRight + 1 - (f_Threshold - tmpMax[iRight + 1]) / (float)(tmpMax[iRight] - tmpMax[iRight + 1]) : m_AreaRect.Height - 1;

                fHeight = (float)(2 * m_CoefAlgoritmVert * (fRight - fLeft));

                l_Sum = l_Sum0 = 0;

                for (int i = 0; i < m_AreaRect.Height; i++)
                {
                    n_CurrentColor = tmpMax[i];

                    if (f_Threshold < n_CurrentColor)
                    {
                        l_Sum += n_CurrentColor;
                        l_Sum0 += n_CurrentColor * i;
                    }
                }

                fCentroid = (float)(l_Sum0 / l_Sum);

                fTopTmp = fCentroid - fHeight / 2f;
            }

            PointF p = new PointF((fLeftTmp < 0) ? 0 : fLeftTmp, (fTopTmp < 0) ? 0 : fTopTmp);
            SizeF s = new SizeF(fWidth, fHeight);

            if ((p.X + s.Width) > m_AreaRect.Width) s.Width = m_AreaRect.Width - p.X;
            if ((p.Y + s.Height) > m_AreaRect.Height) s.Height = m_AreaRect.Height - p.Y;

            return new RectangleF(p, s);
        }

        float GetAreaIntensity(SnapshotBase snapshot, ref long pnCount)
        {
            float fRet = 0;
            long lCount = 0;
            double m_dSum = 0;
            float fLeft = 0, fRight = 0;
            float fTop = 0, fBottom = 0;

            if (m_EllipseArea.LikeBucket == true)
            {
                m_EllipseArea.GetMaxY(ref fBottom, ref fTop);
                if (fBottom < m_AreaRect.Top) fBottom = m_AreaRect.Top;
                if (fBottom > m_AreaRect.Bottom) fBottom = m_AreaRect.Bottom;

                if (fTop < m_AreaRect.Top) fTop = m_AreaRect.Top;
                if (fTop < m_AreaRect.Bottom) fTop = m_AreaRect.Bottom;

                for (int j = (int)Math.Ceiling(fBottom); j < (int)Math.Floor(fTop); j++)
                {
                    if (m_EllipseArea.GetCrossX(j, ref fLeft, ref fRight))
                    {
                        if (fLeft < m_AreaRect.Left) fLeft = m_AreaRect.Left;
                        if (fLeft > m_AreaRect.Right) fLeft = m_AreaRect.Right;

                        if (fRight < m_AreaRect.Left) fRight = m_AreaRect.Left;
                        if (fRight > m_AreaRect.Right) fRight = m_AreaRect.Right;

                        for (int i = (int)Math.Ceiling(fLeft), iShift = i + j * snapshot.Width; i <= (int)Math.Floor(fRight); i++, iShift++)
                        {
                            lCount++;
                            m_dSum += snapshot.GetPixelColor(iShift);
                        }
                    }
                }
            }
            else
            {
                Rectangle rect = m_WorkingAreaRect;

                m_dProfileHorizontal.ClearProfile();
                m_dProfileVertical.ClearProfile();

                m_dProfile45Max = double.MinValue;

                for (int i = rect.Top; i < rect.Bottom; i++)
                {
                    for (int j = rect.Left, iShift = j + i * snapshot.Width; j < rect.Right; j++, iShift++)
                    {
                        m_dSum += snapshot.GetPixelColor(iShift);
                    }
                }

                lCount = rect.Width * rect.Height;
            }

            m_fCurrentAreaPower = m_dSum;

            if (lCount > 0) fRet = (float)(m_fCurrentAreaPower / (float)lCount);

            pnCount = lCount;

            return fRet;
        }

        public float CurrentAreaIntensity
        {
            get { return m_fCurrentAreaIntensity; }
        }

        public long CurrentAreaPoins
        {
            get { return m_lCurrentAreaPoins; }
        }

        public double CurrentAreaPower
        {
            get { return m_fCurrentAreaPower; }
        }
    }
}
