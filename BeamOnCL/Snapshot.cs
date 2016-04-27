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
            //m_uiAverageDataSum = new UInt32[m_tMatrixArray.Length];

            //            rgbValues = new byte[m_tMatrixArray.Length * 3];
        }

        public override UInt16 AverageNum
        {
            get { return (m_apAverage != null) ? m_apAverage.AverageNum : (UInt16)0; }
            set
            {
                if ((value > 1) && (m_apAverage == null)) m_apAverage = new ShiftAveragePicture<T>((UInt32)m_tMatrixArray.Length);

                if (m_apAverage != null) m_apAverage.AverageNum = value;
            }
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

        public override UInt16 MaxColor
        {
            get { return (typeof(T) == typeof(byte))? (UInt16)252 : (UInt16)4090; }
        }

        public unsafe override void SetPixelColor(Int32 Adress, UInt16 uiColor)
        {
            if ((Adress < m_tMatrixArray.Length) || (Adress >= 0))
            {
                if (typeof(T) == typeof(byte))
                {
                    fixed (byte* ptr = m_tMatrixArray as byte[])
                    {
                        ptr[Adress] = (Byte)uiColor;
                    }
                }
                else
                {
                    fixed (short* ptr = m_tMatrixArray as short[])
                    {
                        ptr[Adress] = (short)uiColor;
                    }
                }
            }
        }

        public override void SetPixelColor(Point point, UInt16 uiColor)
        {
            if ((point.X + point.Y * (int)m_rArea.Width) < m_tMatrixArray.Length) SetPixelColor(point.X + point.Y * (int)m_rArea.Width, uiColor);
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

            return GetPixelColor(point.X + point.Y * (int)m_rArea.Width);
        }

        public override unsafe Boolean GetData(IntPtr bData, long timeStamp)
        {
            m_lTimeStamp = timeStamp;
            Boolean bRet = false;

            try
            {
                if (typeof(T) == typeof(byte))
                {
                    Marshal.Copy(bData, m_tMatrixArray as byte[], 0, m_tMatrixArray.Length);
                    if (m_apAverage != null)
                    {
                        fixed (byte* ptr = m_tMatrixArray as byte[])
                        {
                            bRet = m_apAverage.Average((IntPtr)ptr);
                        }
                    }
                    else
                        bRet = true;
                }
                else
                {
                    Marshal.Copy(bData, m_tMatrixArray as short[], 0, m_tMatrixArray.Length);
                    if (m_apAverage != null)
                    {
                        fixed (short* ptr = m_tMatrixArray as short[])
                        {
                            bRet = m_apAverage.Average((IntPtr)ptr);
                        }
                    }
                    else
                        bRet = true;
                }
            }
            catch { }

            return bRet;
        }

        public override unsafe Boolean GetData(byte[] bData, long timeStamp)
        {
            fixed (byte* pSource = bData)
            {
                return GetData((IntPtr)pSource, timeStamp);
            }
        }

        public override unsafe SnapshotBase Clone()
        {
            Snapshot<T> snp = this.MemberwiseClone() as Snapshot<T>;

            snp.m_tMatrixArray = (T[])this.m_tMatrixArray.Clone();

            return snp;
        }

        //private Boolean Average()
        //{
        //    if (m_uiAverageNum > 1)
        //    {
        //        if (m_fStart == true)
        //        {
        //            m_fStart = false;

        //            if (typeof(T) == typeof(byte))
        //                for (int i = 0; i < m_uiAverageDataSum.Length; i++) m_uiAverageDataSum[i] = (Byte)(object)m_tMatrixArray[i];
        //            else
        //                for (int i = 0; i < m_uiAverageDataSum.Length; i++) m_uiAverageDataSum[i] = (UInt16)(object)m_tMatrixArray[i];
        //        }
        //        else
        //        {
        //            if (typeof(T) == typeof(byte))
        //                for (int i = 0; i < m_uiAverageDataSum.Length; i++) m_uiAverageDataSum[i] += (Byte)(object)m_tMatrixArray[i];
        //            else
        //                for (int i = 0; i < m_uiAverageDataSum.Length; i++) m_uiAverageDataSum[i] += (UInt16)(object)m_tMatrixArray[i];
        //        }

        //        if (++m_uiCounter >= m_uiAverageNum)
        //        {
        //            AverageReset();

        //            for (int i = 0; i < m_uiAverageDataSum.Length; i++)
        //            {
        //                if (typeof(T) == typeof(byte))
        //                    (m_tMatrixArray as byte[])[i] = (byte)(m_uiAverageDataSum[i] / m_uiAverageNum);
        //                else
        //                    (m_tMatrixArray as UInt16[])[i] = (UInt16)(m_uiAverageDataSum[i] / m_uiAverageNum);

        //                m_uiAverageDataSum[i] = 0;
        //            }
        //        }
        //    }

        //    return (m_uiCounter == 0);
        //}
    }
}
