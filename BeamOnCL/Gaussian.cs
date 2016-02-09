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
        Double m_fDiffSigma = 0f;

        public Gaussian(Gaussian gaus)
        {
            m_dGaussian = new double[gaus.GaussianData.Length];
            gaus.GaussianData.CopyTo(m_dGaussian, 0);

            m_fCorrelation = gaus.Correlation;
            m_dProfileMax = gaus.ProfileMax;
            m_fDiffSigma = gaus.Sigma;
        }

        public Double Sigma
        {
            get { return m_fDiffSigma; }
        }

        public Double ProfileMax
        {
            get { return m_dProfileMax; }
        }

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
            double dValue = 0f;

            if (iLevel == 0)
                dValue = (double)m_dGaussian.Length;
            else if (iLevel >= 100)
                dValue = 0f;
            else
                dValue = 2 * m_fDiffSigma * Math.Sqrt(-2 * Math.Log(iLevel / 100f));

            return dValue;
        }

        public void Create(Double[] profile, Double profileMax, int centroid, Double Sum)
        {
            double l_Sum = 0;
            double l_Sum0 = 0;
            double f_Coeff = 0;

            m_dProfileMax = profileMax;

            m_fDiffSigma = (float)(Sum / 0.926 / (float)m_dProfileMax / Math.Sqrt(Math.PI * 2));

            if (m_fDiffSigma > 0)
            {
                l_Sum = l_Sum0 = 0;

                for (int i = 0; i < profile.Length; i++)
                {
                    f_Coeff = (i - centroid) / m_fDiffSigma;
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
