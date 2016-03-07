using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BeamOnCL
{
    public class SumAveragePicture<T> : AveragePicture
    {
        public SumAveragePicture(UInt64 tSize)
            : base(tSize)
        {
        }

        public override Boolean Average(ref object[] aAverageData)
        {
            if (m_uiAverageNum > 1)
            {
                if (m_fStart == true)
                {
                    m_fStart = false;
                    for (int i = 0; i < m_uiAverageDataSum.Length; i++) m_uiAverageDataSum[i] = (UInt32)aAverageData[i];
                }
                else
                    for (int i = 0; i < m_uiAverageDataSum.Length; i++) m_uiAverageDataSum[i] += (UInt32)aAverageData[i];

                if (++m_uiCounter >= m_uiAverageNum)
                {
                    Reset();

                    for (int i = 0; i < m_uiAverageDataSum.Length; i++)
                    {
                        aAverageData[i] = (T)(object)(m_uiAverageDataSum[i] / m_uiAverageNum);
                        m_uiAverageDataSum[i] = 0;
                    }
                }
            }

            return (m_uiCounter == 0);
        }
    }
}
