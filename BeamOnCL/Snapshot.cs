using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Drawing;

namespace BeamOnCL
{
    class Snapshot<T> : SnapshotBase
    {
        public T[] m_tMatrixArray = null;
        //        byte[] rgbValues = null;

        public Snapshot(Rectangle rArea)
            : base(rArea)
        {
            m_tMatrixArray = new T[(int)m_rArea.Width * (int)m_rArea.Height];
            m_uiAverageDataSum = new UInt32[m_tMatrixArray.Length];

            //            rgbValues = new byte[m_tMatrixArray.Length * 3];
        }

        public unsafe override void SetImageDataArray(IntPtr Data, Color[] colorArray = null)
        {
            if (typeof(T) == typeof(byte))
                Marshal.Copy(m_tMatrixArray as byte[], 0, Data, m_tMatrixArray.Length);
            else
            {
                Color color;

                if (colorArray != null)
                {
                    byte* bp = (byte*)Data;
                    for (int i = 0; i < m_tMatrixArray.Length; i++)
                    {
                        color = colorArray[(UInt16)((object)m_tMatrixArray[i])];

                        bp[i * 3] = color.B;
                        bp[i * 3 + 1] = color.G;
                        bp[i * 3 + 2] = color.R;
                    }

                    //for (int i = 0; i < m_tMatrixArray.Length; i++)
                    //{
                    //    object d = m_tMatrixArray[i];

                    //    rgbValues[i * 3] = colorArray[(UInt16)d].B;
                    //    rgbValues[i * 3 + 1] = colorArray[(UInt16)d].G;
                    //    rgbValues[i * 3 + 2] = colorArray[(UInt16)d].R;
                    //}

                    //// Copy the RGB values back to the bitmap
                    //Marshal.Copy(rgbValues, 0, Data, rgbValues.Length);
                }
            }
        }

        public override UInt16 GetPixelColor(Int32 Adress)
        {
            if ((Adress >= m_tMatrixArray.Length) || (Adress < 0)) return (UInt16)0;

            if (typeof(T) == typeof(byte))
                return (Byte)(object)m_tMatrixArray[Adress];
            else
                return (UInt16)(object)m_tMatrixArray[Adress];
        }

        public override UInt16 GetPixelColor(Point point)
        {
            if ((point.X + point.Y * (int)m_rArea.Width) >= m_tMatrixArray.Length) return (UInt16)0;

            if (typeof(T) == typeof(byte))
                return (Byte)(object)m_tMatrixArray[point.X + point.Y * (int)m_rArea.Width];
            else
                return (UInt16)(object)m_tMatrixArray[point.X + point.Y * (int)m_rArea.Width];
        }

        public override unsafe Boolean GetData(byte[] bData, long timeStamp)
        {
            m_lTimeStamp = timeStamp;

            fixed (byte* pSource = bData)
            {
                if (typeof(T) == typeof(byte))
                    Marshal.Copy((IntPtr)pSource, m_tMatrixArray as byte[], 0, m_tMatrixArray.Length);
                else
                {
                    Marshal.Copy((IntPtr)pSource, m_tMatrixArray as short[], 0, m_tMatrixArray.Length);
/*
                    fixed (ushort* uMatrix = m_tMatrixArray as ushort[])
                    {
                        byte* ps = pSource;

                        //for (int i = 0, j = 0; i < bData.Length; i += 3, j += 2)
                        //{
                        //    //UInt32 ui = (UInt32)(bData[i + 2] << 16) + (UInt32)(bData[i + 1] << 8) + bData[i];
                        //    //uMatrix[j] = (UInt16)(ui & 0x0fff);
                        //    //uMatrix[j + 1] = (UInt16)(ui >> 12);

                        //    //uMatrix[j] = (UInt16)(((UInt16)(bData[i + 1] << 8) + bData[i]) & 0x0fff);
                        //    //uMatrix[j + 1] = (UInt16)(((UInt16)(bData[i + 2] << 8) + bData[i + 1]) >> 4);

                        //    uMatrix[j] = (UInt16)((UInt16)(bData[i]));
                        //    uMatrix[j + 1] = (UInt16)((UInt16)(bData[i + 2]));
                        //}

                        for (int j = 0; j < m_tMatrixArray.Length; j += 2)
                        {
                            uMatrix[j] = (UInt16)((*(ushort*)ps) & 0x0fff);
                            ps++;
                            uMatrix[j + 1] = (UInt16)(((*(ushort*)ps) >> 4) & 0x0fff);
                            ps += 2;
                        }
                    }
 */ 
                }
            }

            return Average();
        }

        public override unsafe SnapshotBase Clone()
        {
            Snapshot<T> snp = this.MemberwiseClone() as Snapshot<T>;

            snp.m_tMatrixArray = (T[])this.m_tMatrixArray.Clone();

            return snp;
        }

        private Boolean Average()
        {
            if (m_uiAverageNum > 1)
            {
                if (m_fStart == true)
                {
                    m_fStart = false;

                    if (typeof(T) == typeof(byte))
                        for (int i = 0; i < m_uiAverageDataSum.Length; i++) m_uiAverageDataSum[i] = (Byte)(object)m_tMatrixArray[i];
                    else
                        for (int i = 0; i < m_uiAverageDataSum.Length; i++) m_uiAverageDataSum[i] = (UInt16)(object)m_tMatrixArray[i];
                }
                else
                {
                    if (typeof(T) == typeof(byte))
                        for (int i = 0; i < m_uiAverageDataSum.Length; i++) m_uiAverageDataSum[i] += (Byte)(object)m_tMatrixArray[i];
                    else
                        for (int i = 0; i < m_uiAverageDataSum.Length; i++) m_uiAverageDataSum[i] += (UInt16)(object)m_tMatrixArray[i];
                }

                if (++m_uiCounter >= m_uiAverageNum)
                {
                    AverageReset();

                    for (int i = 0; i < m_uiAverageDataSum.Length; i++)
                    {
                        if (typeof(T) == typeof(byte))
                            (m_tMatrixArray as byte[])[i] = (byte)(m_uiAverageDataSum[i] / m_uiAverageNum);
                        else
                            (m_tMatrixArray as UInt16[])[i] = (UInt16)(m_uiAverageDataSum[i] / m_uiAverageNum);

                        m_uiAverageDataSum[i] = 0;
                    }
                }
            }

            return (m_uiCounter == 0);
        }

    }
}
