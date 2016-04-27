using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BeamOnCL
{
    public class SumAveragePicture<T> : AveragePicture
    {
        public SumAveragePicture(UInt32 tSize)
            : base(tSize)
        {
        }

        public unsafe override Boolean Average(IntPtr aAverageData)
        {
            if (m_uiAverageNum > 1)
            {
                if (m_fStart == true)
                {
                    m_fStart = false;

                    if (typeof(T) == typeof(byte))
                    {
                        byte* ptr = (byte*)aAverageData.ToPointer();

                        for (int i = 0; i < m_uiAverageDataSum.Length; i++) m_uiAverageDataSum[i] = (UInt32)ptr[i];
                    }
                    else
                    {
                        short* ptr = (short*)aAverageData.ToPointer();

                        for (int i = 0; i < m_uiAverageDataSum.Length; i++) m_uiAverageDataSum[i] = (UInt32)ptr[i];
                    }
                }
                else
                {
                    if (typeof(T) == typeof(byte))
                    {
                        byte* ptr = (byte*)aAverageData.ToPointer();

                        for (int i = 0; i < m_uiAverageDataSum.Length; i++) m_uiAverageDataSum[i] += (UInt32)ptr[i];
                    }
                    else
                    {
                        short* ptr = (short*)aAverageData.ToPointer();

                        for (int i = 0; i < m_uiAverageDataSum.Length; i++) m_uiAverageDataSum[i] += (UInt32)ptr[i];
                    }
                }

                if (++m_uiCounter >= m_uiAverageNum)
                {
                    Reset();

                    if (typeof(T) == typeof(byte))
                    {
                        byte* ptr = (byte*)aAverageData.ToPointer();
                        for (int i = 0; i < m_uiAverageDataSum.Length; i++)
                        {
                            ptr[i] = (byte)(m_uiAverageDataSum[i] / m_uiAverageNum);
                            m_uiAverageDataSum[i] = 0;
                        }
                    }
                    else
                    {
                        short* ptr = (short*)aAverageData.ToPointer();
                        for (int i = 0; i < m_uiAverageDataSum.Length; i++)
                        {
                            ptr[i] = (short)(m_uiAverageDataSum[i] / m_uiAverageNum);
                            m_uiAverageDataSum[i] = 0;
                        }
                    }
                }
            }

            return (m_uiCounter == 0);
        }
    }
}
