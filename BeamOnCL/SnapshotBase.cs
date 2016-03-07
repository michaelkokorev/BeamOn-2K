using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BeamOnCL
{
    public class SnapshotBase : System.ICloneable
    {
        protected Rectangle m_rArea;
        protected bool m_fStart = true;
        protected UInt16 m_uiCounter = 0;
        protected ushort m_uiAverageNum = 0;
        protected uint[] m_uiAverageDataSum = null;
        protected long m_lTimeStamp = 0;

        public void AverageReset()
        {
            m_fStart = true;
            m_uiCounter = 0;
        }

        public UInt16 AverageNum
        {
            get { return m_uiAverageNum; }
            set
            {
                AverageReset();
                m_uiAverageNum = value;
            }
        }

        public long TimeStamp
        {
            get { return m_lTimeStamp; }
        }

        public UInt16 Width
        {
            get { return (UInt16)m_rArea.Width; }
        }

        public UInt16 Height
        {
            get { return (UInt16)m_rArea.Height; }
        }

        public UInt16 Top
        {
            get { return (UInt16)m_rArea.Top; }
        }

        public UInt16 Left
        {
            get { return (UInt16)m_rArea.Left; }
        }

        public virtual unsafe Boolean GetData(byte[] bData, long timeStamp)
        {
            m_lTimeStamp = timeStamp;

            return true;
        }

        public virtual void SetData(IntPtr Data)
        {
        }

        public SnapshotBase(Rectangle rArea)
        {
            m_rArea = rArea;
        }

        object ICloneable.Clone()
        {
            return this.Clone();
        }

        public virtual SnapshotBase Clone()
        {
            SnapshotBase snp = this.MemberwiseClone() as SnapshotBase;

            snp.m_rArea = this.m_rArea;

            return snp;
        }

        public Rectangle ImageRectangle
        {
            get { return m_rArea; }
        }

        public virtual UInt16 GetPixelColor(Int32 Adress)
        {
            return (UInt16)0;
        }

        public virtual UInt16 GetPixelColor(Point point)
        {
            return (UInt16)0;
        }

        public virtual void SetImageDataArray(IntPtr Data, Color[] colorArray = null)
        {
        }
    }
}
