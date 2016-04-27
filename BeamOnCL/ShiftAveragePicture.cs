using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace BeamOnCL
{
    class ShiftAveragePicture<T> : AveragePicture
    {
        T[] m_aAverageData = null;

        public ShiftAveragePicture(UInt32 tSize)
            : base(tSize)
        {
        }

        public override UInt16 AverageNum
        {
            get { return m_uiAverageNum; }
            set
            {
                Reset();
                m_uiAverageNum = value;

                m_aAverageData = new T[m_uiAverageDataSum.Length * m_uiAverageNum];
            }
        }

        public override void Reset()
        {
            m_fStart = true;
            m_uiCounter = 0;
        }

        public unsafe override Boolean Average(IntPtr aAverageData)
        {
            if ((m_uiAverageNum > 1) && (aAverageData != null))
            {
                if (m_fStart == true)
                {
                    m_fStart = false;

                    if (typeof(T) == typeof(byte))
                    {
                        byte* ptr = (byte*)aAverageData.ToPointer();

                        for (UInt16 i = 0; i < m_uiAverageNum; i++) Marshal.Copy(aAverageData, m_aAverageData as byte[], i * m_uiAverageDataSum.Length, m_uiAverageDataSum.Length);

                        for (int i = 0; i < m_uiAverageDataSum.Length; i++) m_uiAverageDataSum[i] = (UInt32)ptr[i] * m_uiAverageNum;
                    }
                    else
                    {
                        short* ptr = (short*)aAverageData.ToPointer();

                        for (UInt16 i = 0; i < m_uiAverageNum; i++) Marshal.Copy(aAverageData, m_aAverageData as short[], i * m_uiAverageDataSum.Length, m_uiAverageDataSum.Length);

                        for (int i = 0; i < m_uiAverageDataSum.Length; i++) m_uiAverageDataSum[i] = (UInt32)ptr[i] * m_uiAverageNum;
                    }
                }
                else
                {
                    int l_uiAverageCurrent = m_uiCounter * m_uiAverageDataSum.Length;

                    if (typeof(T) == typeof(byte))
                    {
                        byte* ptr = (byte*)aAverageData.ToPointer();
                        byte[] data = m_aAverageData as byte[];

                        for (int i = 0; i < m_uiAverageDataSum.Length; i++)
                        {
                            m_uiAverageDataSum[i] -= (UInt32)data[l_uiAverageCurrent + i];
                            m_uiAverageDataSum[i] += ptr[i];
                        }

                        Marshal.Copy(aAverageData, data, l_uiAverageCurrent, m_uiAverageDataSum.Length);

                        for (int i = 0; i < m_uiAverageDataSum.Length; i++) ptr[i] = (byte)(m_uiAverageDataSum[i] / m_uiAverageNum);
                    }
                    else
                    {
                        short* ptr = (short*)aAverageData.ToPointer();
                        short[] data = m_aAverageData as short[];

                        for (int i = 0; i < m_uiAverageDataSum.Length; i++)
                        {
                            m_uiAverageDataSum[i] -= (UInt32)data[l_uiAverageCurrent + i];
                            m_uiAverageDataSum[i] += (UInt32)ptr[i];
                        }

                        Marshal.Copy(aAverageData, data, l_uiAverageCurrent, m_uiAverageDataSum.Length);

                        for (int i = 0; i < m_uiAverageDataSum.Length; i++) ptr[i] = (short)(m_uiAverageDataSum[i] / m_uiAverageNum);
                    }
                }

                if (++m_uiCounter >= m_uiAverageNum) m_uiCounter = 0;
            }

            return true;
        }

        //public unsafe override Boolean Average(IntPtr aAverageData)
        //{
        //    if (m_fStart == true)
        //    {
        //        m_fStart = false;

        //        if (typeof(T) == typeof(byte))
        //        {
        //            byte* ptr = (byte*)aAverageData.ToPointer();
                    
        //            for (UInt16 i = 0; i < m_uiAverageNum; i++) Marshal.Copy(aAverageData, m_aAverageData as byte[], i * m_uiAverageDataSum.Length, m_uiAverageDataSum.Length);
                    
        //            for (int i = 0; i < m_uiAverageDataSum.Length; i++) m_uiAverageDataSum[i] = (UInt32)ptr[i] * m_uiAverageNum;
        //        }
        //        else
        //        {
        //            short* ptr = (short*)aAverageData.ToPointer();

        //            for (UInt16 i = 0; i < m_uiAverageNum; i++) Marshal.Copy(aAverageData, m_aAverageData as short[], i * m_uiAverageDataSum.Length, m_uiAverageDataSum.Length);

        //            for (int i = 0; i < m_uiAverageDataSum.Length; i++) m_uiAverageDataSum[i] = (UInt32)ptr[i] * m_uiAverageNum;
        //        }
        //    }
        //    else
        //    {
        //        int l_uiAverageCurrent = m_uiCounter * m_uiAverageDataSum.Length;

        //        if (typeof(T) == typeof(byte))
        //        {
        //            byte* ptr = (byte*)aAverageData.ToPointer();

        //            for (int i = 0; i < m_uiAverageDataSum.Length; i++)
        //            {
        //                m_uiAverageDataSum[i] -= (UInt32)(object)m_aAverageData[l_uiAverageCurrent + i];
        //                m_uiAverageDataSum[i] += ptr[i];
        //            }

        //            Marshal.Copy(aAverageData, m_aAverageData as byte[], l_uiAverageCurrent, m_uiAverageDataSum.Length);

        //            for (int i = 0; i < m_uiAverageDataSum.Length; i++) ptr[i] = (byte)(m_uiAverageDataSum[i] / m_uiAverageNum);
        //        }
        //        else
        //        {
        //            short* ptr = (short*)aAverageData.ToPointer();
        //            short[] data = m_aAverageData as short[];

        //            for (int i = 0; i < m_uiAverageDataSum.Length; i++)
        //            {
        //                m_uiAverageDataSum[i] -= (UInt32)data[l_uiAverageCurrent + i];
        //                m_uiAverageDataSum[i] += (UInt32)ptr[i];
        //            }

        //            Marshal.Copy(aAverageData, data, l_uiAverageCurrent, m_uiAverageDataSum.Length);

        //            for (int i = 0; i < m_uiAverageDataSum.Length; i++) ptr[i] = (short)(m_uiAverageDataSum[i] / m_uiAverageNum);
        //        }
        //    }


        //    if (++m_uiCounter >= m_uiAverageNum) m_uiCounter = 0;






        //    //if ((m_uiAverageNum > 1) && (aAverageData != null))
        //    //{
        //    //    if (m_fStart == true)
        //    //    {
        //    //        m_fStart = false;
        //    //        for (UInt16 i = 0; i < m_uiAverageNum; i++)
        //    //            Array.Copy(aAverageData, 0, m_aAverageData, i * m_uiAverageDataSum.Length, m_uiAverageDataSum.Length);

        //    //        for (int i = 0; i < m_uiAverageDataSum.Length; i++) m_uiAverageDataSum[i] = (UInt32)aAverageData[i] * m_uiAverageNum;
        //    //    }
        //    //    else
        //    //    {
        //    //        UInt32 l_uiAverageCurrent = m_uiCounter * (UInt32)m_uiAverageDataSum.Length;

        //    //        for (int i = 0; i < m_uiAverageDataSum.Length; i++)
        //    //        {
        //    //            m_uiAverageDataSum[i] -= (UInt32)(object)m_aAverageData[l_uiAverageCurrent + i];
        //    //            m_uiAverageDataSum[i] += (UInt32)aAverageData[i];
        //    //            m_aAverageData[l_uiAverageCurrent + i] = (T)aAverageData[i];
        //    //        }
        //    //    }

        //    //    for (int i = 0; i < m_uiAverageDataSum.Length; i++) aAverageData[i] = (T)(object)(m_uiAverageDataSum[i] / m_uiAverageNum);

        //    //    if (++m_uiCounter >= m_uiAverageNum) m_uiCounter = 0;
        //    //}

        //    return true;
        //}
    }
}
