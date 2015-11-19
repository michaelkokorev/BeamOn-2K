using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BeamOnCL
{
    public class Gaussian
    {
        double[] m_dGaussian = null;
        float m_fCorrelation = 0f;
        Double m_dProfileMax = 0;

        public Gaussian(int Lenght)
        {
            m_dGaussian = new double[Lenght];
        }

        public Double[] GaussianData
        {
            get { return m_dGaussian; }
        }

        public Single Correlation
        {
            get { return m_fCorrelation; }
        }

        public double GetWidth(float iLevel)
        {
            double f_Board;
            int iLeft, iRight;
            double f_Left;
            double f_Right;

            if (iLevel >= 100) return (0);

            f_Board = m_dProfileMax * iLevel / 100f;

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
            for (iLeft = 0; ((iLeft < m_dGaussian.Length) && (m_dGaussian[iLeft] < f_Board)); iLeft++) ;
            for (iRight = m_dGaussian.Length - 1; ((iRight > 0) && (m_dGaussian[iRight] < f_Board)); iRight--) ;

            if ((iLeft == 0) || (m_dGaussian[iLeft - 1] >= m_dGaussian[iLeft])) return (0);
            f_Left = iLeft - (m_dGaussian[iLeft] - f_Board) / (m_dGaussian[iLeft] - m_dGaussian[iLeft - 1]);

            if ((iRight == m_dGaussian.Length - 1) || (m_dGaussian[iRight + 1] >= m_dGaussian[iRight])) return (0);
            f_Right = iRight + (m_dGaussian[iRight] - f_Board) / (m_dGaussian[iRight] - m_dGaussian[iRight + 1]);
            //}

            return (f_Right - f_Left);
        }

        public void Create(Double[] profile, Double profileMax, int centroid, Double Sum)
        {
            double l_Sum = 0;
            double l_Sum0 = 0;
            double f_Coeff = 0;

            m_dProfileMax = profileMax;

            double fSigma = (float)(Sum / 0.926 / (float)m_dProfileMax / Math.Sqrt(Math.PI * 2));

            if (fSigma > 0)
            {
                l_Sum = l_Sum0 = 0;

                for (int i = 0; i < profile.Length; i++)
                {
                    f_Coeff = (i - centroid) / fSigma;
                    m_dGaussian[i] = m_dProfileMax * Math.Exp(-f_Coeff * f_Coeff / 2);

                    f_Coeff = m_dGaussian[i];
                    l_Sum += f_Coeff * f_Coeff;
                    f_Coeff = profile[i] - m_dGaussian[i];
                    l_Sum0 += f_Coeff * f_Coeff;
                }

                m_fCorrelation = (float)(100 * (1 - Math.Sqrt(l_Sum0 / (l_Sum0 + l_Sum))));
            }
        }
    }
}
