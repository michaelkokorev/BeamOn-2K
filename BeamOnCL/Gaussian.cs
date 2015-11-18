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

        public void Create(Double[] profile, Double profileMax, int centroid, Double Sum)
        {
            double l_Sum = 0;
            double l_Sum0 = 0;
            double f_Coeff = 0;

            double fSigma = (float)(Sum / 0.926 / (float)profileMax / Math.Sqrt(Math.PI * 2));

            if (fSigma > 0)
            {
                l_Sum = l_Sum0 = 0;

                for (int i = 0; i < profile.Length; i++)
                {
                    f_Coeff = (i - centroid) / fSigma;
                    m_dGaussian[i] = profileMax * Math.Exp(-f_Coeff * f_Coeff / 2);

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
