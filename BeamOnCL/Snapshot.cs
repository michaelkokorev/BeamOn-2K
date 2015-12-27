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
            if (Adress >= m_tMatrixArray.Length) return (UInt16)0;

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

        public override unsafe void GetData(byte[] bData)
        {
            fixed (byte* pb = bData)
            {
                if (typeof(T) == typeof(byte))
                    Marshal.Copy((IntPtr)pb, m_tMatrixArray as byte[], 0, m_tMatrixArray.Length);
                else
                    Marshal.Copy((IntPtr)pb, m_tMatrixArray as short[], 0, m_tMatrixArray.Length);
            }
        }

        public override unsafe SnapshotBase Clone()
        {
            Snapshot<T> snp = this.MemberwiseClone() as Snapshot<T>;

            snp.m_tMatrixArray = (T[])this.m_tMatrixArray.Clone();

            return snp;
        }

    }
}
