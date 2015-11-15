using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BeamOnCL
{
    public class SnapshotBase
    {
        protected Rectangle m_rArea;

        public UInt16 Width
        {
            get { return (UInt16)m_rArea.Width; }
        }

        public UInt16 Height
        {
            get { return (UInt16)m_rArea.Height; }
        }

        public virtual unsafe void GetData(byte[] bData)
        {
        }

        public virtual void SetData(IntPtr Data)
        {
        }

        public SnapshotBase(Rectangle rArea)
        {
            m_rArea = rArea;
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
