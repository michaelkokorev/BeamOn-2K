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

        float m_fPixelSize = 5.86f;

        double[] m_dProfileHorizontal = null;
        double[] m_dProfileVertical = null;
        double[] m_dGaussianHorizontal = null;
        double[] m_dGaussianVertical = null;
        double[] m_dProfile45 = null;

        double m_dProfileVerticalMax = double.MinValue;
        double m_dProfileHorizontalMax = double.MinValue;
        double m_dProfile45Max = double.MinValue;

        double m_fCorrelationHorizontal = 0f;
        double m_fCorrelationVertical = 0f;

        double m_CoefAlgoritmVert = 1.5f;
        double m_CoefAlgoritmHor = 1.5f;

        float m_fLevel = (float)13.5;

        Rectangle m_AreaRect;
        Area m_WorkingAreaRect;
        Area m_EllipseArea = new Area();
        PointF m_pfOldCentroid = new PointF();

        float m_fClipLevel1 = 13.5f;
        float m_fClipLevel2 = 50f;
        float m_fWidthHorizontalProfileClipLevel1 = 0f;
        float m_fWidthHorizontalProfileClipLevel2 = 0f;
        float m_fWidthVerticalProfileClipLevel1 = 0f;
        float m_fWidthVerticalProfileClipLevel2 = 0f;

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

        public Positioning(Rectangle rect)
        {
            m_AreaRect = rect;

            m_WorkingAreaRect = m_AreaRect;
            m_WorkingAreaRect.Type = Area.Figure.enRectangle;

            m_dProfileHorizontal = new double[m_AreaRect.Width];
            m_dProfileVertical = new double[m_AreaRect.Height];
            m_dGaussianHorizontal = new double[m_AreaRect.Width];
            m_dGaussianVertical = new double[m_AreaRect.Height];
            m_dProfile45 = new double[m_AreaRect.Width + m_AreaRect.Height];
        }

        public void GetData(SnapshotBase snapshot)
        {
            Boolean bFlagFirst = false;
            float fLimitDeltaY;
            float fLimitDeltaX;

            Create(snapshot);

            while (true)
            {
                if (m_WorkingAreaRect != m_AreaRect)
                {
                    if (bFlagFirst)
                    {
                        m_pfOldCentroid = m_EllipseArea.Centroid;
                        m_WorkingAreaRect = SerchRectangle(snapshot);

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
                    bFlagFirst = (m_WorkingAreaRect != m_AreaRect);
                    if (bFlagFirst == true) continue;
                }
            }
        }

        public void Create(SnapshotBase snapshot)
        {
            UInt16 uiData = 0;
            int iShift = 0;
            double l_dProfileVerticalMin = double.MaxValue;
            double l_dProfileHorizontalMin = double.MaxValue;
            double l_dProfile45Min = double.MaxValue;

            int l_iProfileVerticalPeak = 0;
            int l_iProfileHorizontalPeak = 0;
            int l_iProfile45Peak = 0;

            float f_Threshold = 0;
            double l_Sum = 0;
            double l_Sum0 = 0;
            double f_Coeff = 0;

            int l_Y, l_X, nCount;

            Rectangle rect = m_WorkingAreaRect;

            ArrayFill<double>(m_dProfileHorizontal, 0f);
            ArrayFill<double>(m_dProfileVertical, 0f);
            ArrayFill<double>(m_dGaussianHorizontal, 0f);
            ArrayFill<double>(m_dGaussianVertical, 0f);
            ArrayFill<double>(m_dProfile45, 0f);

            m_dProfileVerticalMax = double.MinValue;
            m_dProfileHorizontalMax = double.MinValue;
            m_dProfile45Max = double.MinValue;

            for (int i = rect.Top; i < rect.Bottom; i++)
            {
                iShift = i * snapshot.Width + rect.Left;

                for (int j = rect.Left; j < rect.Right; j++, iShift++)
                {
                    uiData = snapshot.GetPixelColor(iShift);
                    m_dProfileHorizontal[j] += uiData;
                    m_dProfileVertical[i] += uiData;
                }

                if (l_dProfileVerticalMin > m_dProfileVertical[i])
                    l_dProfileVerticalMin = m_dProfileVertical[i];
                else if (m_dProfileVerticalMax < m_dProfileVertical[i])
                {
                    m_dProfileVerticalMax = m_dProfileVertical[i];
                    l_iProfileVerticalPeak = i;
                }
            }

            f_Threshold = (float)((m_dProfileVerticalMax - l_dProfileVerticalMin) * 0.2);

            for (int i = rect.Top; i < rect.Bottom; i++)
            {
                m_dProfileVertical[i] = (m_dProfileVertical[i] > l_dProfileVerticalMin) ? m_dProfileVertical[i] - l_dProfileVerticalMin : 0;

                if (m_dProfileVertical[i] > f_Threshold)
                {
                    l_Sum += m_dProfileVertical[i] / 10000.0;
                    l_Sum0 += (m_dProfileVertical[i] / 10000.0) * i;
                }
            }

            m_EllipseArea.Centroid = new PointF(m_EllipseArea.Centroid.X, (l_Sum == 0) ? (float)(snapshot.Height / 2f) : (float)(l_Sum0 / l_Sum));

            m_dProfileVerticalMax -= l_dProfileVerticalMin;

            double fSigma = (float)(l_Sum * 10000.0 / 0.926 / (float)m_dProfileVerticalMax / Math.Sqrt(Math.PI * 2));

            if (fSigma > 0)
            {
                l_Sum = l_Sum0 = 0;

                for (int i = rect.Top; i < rect.Bottom; i++)
                {
                    f_Coeff = (i - m_EllipseArea.Centroid.Y) / fSigma;
                    m_dGaussianVertical[i] = m_dProfileVerticalMax * Math.Exp(-f_Coeff * f_Coeff / 2);

                    f_Coeff = m_dGaussianVertical[i];
                    l_Sum += f_Coeff * f_Coeff;
                    f_Coeff = m_dProfileVertical[i] - m_dGaussianVertical[i];
                    l_Sum0 += f_Coeff * f_Coeff;
                }

                m_fCorrelationVertical = 100 * (1 - Math.Sqrt(l_Sum0 / (l_Sum0 + l_Sum)));
            }

            f_Threshold = (float)((m_dProfileVerticalMax - l_dProfileVerticalMin) * 0.2);

            for (int i = rect.Left; i < rect.Right; i++, iShift++)
            {
                if (l_dProfileHorizontalMin > m_dProfileHorizontal[i])
                    l_dProfileHorizontalMin = m_dProfileHorizontal[i];
                else if (m_dProfileHorizontalMax < m_dProfileHorizontal[i])
                {
                    m_dProfileHorizontalMax = m_dProfileHorizontal[i];
                    l_iProfileHorizontalPeak = i;
                }
            }

            l_Sum = l_Sum0 = 0;

            for (int i = rect.Left; i < rect.Right; i++, iShift++)
            {
                m_dProfileHorizontal[i] = (m_dProfileHorizontal[i] > l_dProfileHorizontalMin) ? m_dProfileHorizontal[i] - l_dProfileHorizontalMin : 0;

                if (m_dProfileHorizontal[i] > f_Threshold)
                {
                    l_Sum += m_dProfileHorizontal[i] / 10000.0;
                    l_Sum0 += (m_dProfileHorizontal[i] / 10000.0) * i;
                }
            }

            m_EllipseArea.Centroid = new PointF((l_Sum == 0) ? (float)(snapshot.Width / 2f) : (float)(l_Sum0 / l_Sum), m_EllipseArea.Centroid.Y);

            m_dProfileHorizontalMax -= l_dProfileHorizontalMin;

            fSigma = (float)(l_Sum * 10000.0 / 0.926 / (float)m_dProfileHorizontalMax / Math.Sqrt(Math.PI * 2));

            if (fSigma > 0)
            {
                l_Sum = l_Sum0 = 0;

                for (int i = rect.Left; i < rect.Right; i++)
                {
                    f_Coeff = (i - m_EllipseArea.Centroid.X) / fSigma;
                    m_dGaussianHorizontal[i] = m_dProfileHorizontalMax * Math.Exp(-f_Coeff * f_Coeff / 2);

                    f_Coeff = m_dGaussianHorizontal[i];
                    l_Sum += f_Coeff * f_Coeff;
                    f_Coeff = m_dProfileHorizontal[i] - m_dGaussianHorizontal[i];
                    l_Sum0 += f_Coeff * f_Coeff;
                }

                m_fCorrelationHorizontal = (float)(100 * (1 - Math.Sqrt(l_Sum0 / (l_Sum0 + l_Sum))));
            }

            int iLeft = m_dProfile45.Length;
            int iRight = 0;

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

            m_fWidthHorizontalProfileClipLevel1 = (float)(GetWidth(m_dProfileHorizontal, m_fClipLevel1, (int)m_EllipseArea.Centroid.X, m_dProfileHorizontalMax) * m_fPixelSize);
            m_fWidthHorizontalProfileClipLevel2 = (float)(GetWidth(m_dProfileHorizontal, m_fClipLevel2, (int)m_EllipseArea.Centroid.X, m_dProfileHorizontalMax) * m_fPixelSize);
            m_fWidthVerticalProfileClipLevel1 = (float)(GetWidth(m_dProfileVertical, m_fClipLevel1, (int)m_EllipseArea.Centroid.Y, m_dProfileVerticalMax) * m_fPixelSize);
            m_fWidthVerticalProfileClipLevel2 = (float)(GetWidth(m_dProfileVertical, m_fClipLevel2, (int)m_EllipseArea.Centroid.Y, m_dProfileVerticalMax) * m_fPixelSize);

            double fWidthH45 = GetWidth(m_dProfileHorizontal, m_fLevel, (int)m_EllipseArea.Centroid.X, m_dProfileHorizontalMax);
            double fWidthV45 = GetWidth(m_dProfileVertical, m_fLevel, (int)m_EllipseArea.Centroid.Y, m_dProfileVerticalMax);
            double fWidthV = GetWidth(m_dProfile45, m_fLevel, l_iProfile45Peak, m_dProfile45Max) / Math.Sqrt(2f);

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

                //		m_ElipseArea.SetMajorRadius(X + U);
                //		m_ElipseArea.SetMinorRadius(X - U);

                //		if (m_ElipseArea.GetMinorRadius() <= 0.01 * m_ElipseArea.GetMajorRadius())
                //			m_ElipseArea.SetMinorRadius(sqrt(0.01 * m_ElipseArea.GetMajorRadius()));
                //		else
                //			m_ElipseArea.SetMinorRadius(sqrt(m_ElipseArea.GetMinorRadius()));

                //		m_ElipseArea.SetMajorRadius(sqrt(m_ElipseArea.GetMajorRadius()));
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

            double[] tmpProfH = new double[m_AreaRect.Width + m_AreaRect.Height];
            double[] tmpProfV = new double[m_AreaRect.Width + m_AreaRect.Height];

            if (Math.Abs(m_EllipseArea.Angle) == Math.PI / 2f)
            {
                l_fLeft = (float)m_AreaRect.Top;
                l_fRight = (float)m_AreaRect.Bottom;
                l_fTop = (float)m_AreaRect.Left;
                l_fBottom = (float)m_AreaRect.Right;

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
                l_fLeft = (float)m_AreaRect.Left;
                l_fRight = (float)m_AreaRect.Right;
                l_fTop = (float)m_AreaRect.Top;
                l_fBottom = (float)m_AreaRect.Bottom;

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

                l_fLeft = (float)(m_AreaRect.Top - m_EllipseArea.Centroid.Y);
                l_fRight = (float)(m_AreaRect.Bottom - m_EllipseArea.Centroid.Y);

                l_fTop = (float)((m_AreaRect.Top - m_EllipseArea.Centroid.Y) * l_dTanAlfa);
                l_fBottom = (float)((m_AreaRect.Bottom - m_EllipseArea.Centroid.Y) * l_dTanAlfa);

                if (l_fTop < (m_AreaRect.Left - m_EllipseArea.Centroid.X))
                    l_fLeft = (float)((m_AreaRect.Left - m_EllipseArea.Centroid.X) / l_dTanAlfa);

                if (l_fTop > (m_AreaRect.Right - m_EllipseArea.Centroid.X))
                    l_fLeft = (float)((m_AreaRect.Right - m_EllipseArea.Centroid.X) / l_dTanAlfa);

                if (l_fBottom < (m_AreaRect.Left - m_EllipseArea.Centroid.X))
                    l_fRight = (float)((m_AreaRect.Left - m_EllipseArea.Centroid.X) / l_dTanAlfa);

                if (l_fBottom > (m_AreaRect.Right - m_EllipseArea.Centroid.X))
                    l_fRight = (float)((m_AreaRect.Right - m_EllipseArea.Centroid.X) / l_dTanAlfa);

                l_fBottom = (float)(m_AreaRect.Right - m_EllipseArea.Centroid.X);
                l_fTop = (float)(m_AreaRect.Left - m_EllipseArea.Centroid.X);

                l_fLeftP = (float)((m_AreaRect.Left - m_EllipseArea.Centroid.X) * l_dTanAlfa);
                l_fRightP = (float)((m_AreaRect.Right - m_EllipseArea.Centroid.X) * l_dTanAlfa);

                if (l_fLeftP < (m_AreaRect.Top - m_EllipseArea.Centroid.Y))
                    l_fTop = (float)((m_AreaRect.Top - m_EllipseArea.Centroid.Y) / l_dTanAlfa);

                if (l_fLeftP > (m_AreaRect.Bottom - m_EllipseArea.Centroid.Y))
                    l_fTop = (float)((m_AreaRect.Bottom - m_EllipseArea.Centroid.Y) / l_dTanAlfa);

                if (l_fRightP < (m_AreaRect.Top - m_EllipseArea.Centroid.Y))
                    l_fBottom = (float)((m_AreaRect.Top - m_EllipseArea.Centroid.Y) / l_dTanAlfa);

                if (l_fRightP > (m_AreaRect.Bottom - m_EllipseArea.Centroid.Y))
                    l_fBottom = (float)((m_AreaRect.Bottom - m_EllipseArea.Centroid.Y) / l_dTanAlfa);

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
                        Ylg = Yl + m_EllipseArea.Centroid.X;

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

                if ((iLeft > 0) && (iRight < (l_lSizeH - 1)))
                    m_EllipseArea.MajorRadius = (iRight - iLeft + (tmpProfH[iRight] - f_Threshold) / (tmpProfH[iRight] - tmpProfH[iRight + 1]) + (tmpProfH[iLeft] - f_Threshold) / (tmpProfH[iLeft] - tmpProfH[iLeft - 1])) / 2f;
                else
                    m_EllipseArea.MajorRadius = l_lSizeH - 1;

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

                if ((iLeft > 0) && (iRight < (l_lSizeV - 1)))
                    m_EllipseArea.MinorRadius = (iRight - iLeft + (tmpProfV[iRight] - f_Threshold) / (tmpProfV[iRight] - tmpProfV[iRight + 1]) + (tmpProfV[iLeft] - f_Threshold) / (tmpProfV[iLeft] - tmpProfV[iLeft - 1])) / 2f;
                else
                    m_EllipseArea.MinorRadius = l_lSizeV - 1;
            }
        }

        double GetWidth(double[] plProf, float iLevel, int iStart, double lMax)
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

            long[] tmpProf = new long[Math.Max(m_AreaRect.Height, m_AreaRect.Width)];
            long[] tmpMax = new long[Math.Max(m_AreaRect.Height, m_AreaRect.Width)];
            long tmpMin;

            for (int i = 0; i < m_AreaRect.Width; i++)
            {
                for (int j = 0; j < m_AreaRect.Height; j++) tmpProf[j] = snapshot.GetPixelColor(j * m_AreaRect.Width + i);

                tmpMin = tmpMax[i] = tmpProf[0];

                for (int j = 1; j < m_AreaRect.Height; j++)
                {
                    n_CurrentColor = tmpProf[j];

                    if (tmpMax[i] < n_CurrentColor)
                        tmpMax[i] = n_CurrentColor;
                    else if (tmpMin > n_CurrentColor)
                        tmpMin = n_CurrentColor;
                }

                tmpMax[i] -= tmpMin;

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
                //01.06.14
                //		f_Threshold = (float)((n_MaxColor - n_MinColor) * 0.5f + n_MinColor);
                f_Threshold = (float)((n_MaxColor - n_MinColor) * 0.135f + n_MinColor);

                for (iLeft = 0; ((iLeft < iPeak) && (tmpMax[iLeft] < f_Threshold)); iLeft++) ;
                for (iRight = m_AreaRect.Width - 1; ((iRight > iPeak) && (tmpMax[iRight] < f_Threshold)); iRight--) ;

                fLeft = (iLeft > 0) ? (float)(iLeft - 1 + (f_Threshold - tmpMax[iLeft - 1]) / (float)(tmpMax[iLeft] - tmpMax[iLeft - 1])) : (float)0;
                fRight = (iRight < (m_AreaRect.Width - 1)) ? iRight + 1 - (f_Threshold - tmpMax[iRight + 1]) / (float)(tmpMax[iRight] - tmpMax[iRight + 1]) : m_AreaRect.Width - 1;
                //01.06.14
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

                //		iRadius = (int)(m_CoefAlgoritm * 0.764297584 * l_Sum / (float)n_MaxColor);
                //		iRadius = (int)(m_CoefAlgoritm * 0.50406319 * l_Sum / (float)n_MaxColor);

                fLeftTmp = fCentroid - fWidth / 2f;
            }

            n_MaxColor = long.MinValue;
            n_MinColor = long.MaxValue;

            for (int i = 0; i < m_AreaRect.Height; i++)
            {
                for (int j = 0; j < m_AreaRect.Width; j++) tmpProf[j] = snapshot.GetPixelColor(j + i * m_AreaRect.Width);

                tmpMin = tmpMax[i] = tmpProf[0];

                for (int j = 1; j < m_AreaRect.Width; j++)
                {
                    n_CurrentColor = tmpProf[j];

                    if (tmpMax[i] < n_CurrentColor)
                        tmpMax[i] = n_CurrentColor;
                    else if (tmpMin > n_CurrentColor)
                        tmpMin = n_CurrentColor;
                }

                tmpMax[i] -= tmpMin;

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
                //01.06.14
                //		f_Threshold = (float)((n_MaxColor - n_MinColor) * 0.5f + n_MinColor);
                f_Threshold = (float)((n_MaxColor - n_MinColor) * 0.135f + n_MinColor);


                for (iLeft = 0; ((iLeft < iPeak) && (tmpMax[iLeft] < f_Threshold)); iLeft++) ;
                for (iRight = m_AreaRect.Height - 1; ((iRight > iPeak) && (tmpMax[iRight] < f_Threshold)); iRight--) ;


                fLeft = (iLeft > 0) ? (float)(iLeft - 1 + (f_Threshold - tmpMax[iLeft - 1]) / (float)(tmpMax[iLeft] - tmpMax[iLeft - 1])) : (float)0;
                fRight = (iRight < (m_AreaRect.Height - 1)) ? iRight + 1 - (f_Threshold - tmpMax[iRight + 1]) / (float)(tmpMax[iRight] - tmpMax[iRight + 1]) : m_AreaRect.Height - 1;
                //01.06.14
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

                //		iRadius = (int)(m_CoefAlgoritm * 0.764297584 * l_Sum / (float)n_MaxColor);
                //		iRadius = (int)(m_CoefAlgoritm * 0.50406319 * l_Sum / (float)n_MaxColor);

                fTopTmp = fCentroid - fHeight / 2f;
            }

            PointF p = new PointF((fLeftTmp < 0) ? 0 : fLeftTmp, (fTopTmp < 0) ? 0 : fTopTmp);
            SizeF s = new SizeF(fWidth, fHeight);

            if ((p.X + s.Width) > m_AreaRect.Width) s.Width = m_AreaRect.Width - p.X;
            if ((p.Y + s.Height) > m_AreaRect.Height) s.Height = m_AreaRect.Height - p.Y;

            return new RectangleF(p, s);
        }
    }
}
