using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BeamOnCL
{
    public class AveragePicture
    {
        protected bool m_fStart = true;
        protected UInt16 m_uiCounter = 0;
        protected ushort m_uiAverageNum = 0;
        protected uint[] m_uiAverageDataSum = null;

        public AveragePicture(UInt32 tSize)
        {
            m_uiAverageDataSum = new UInt32[tSize];
        }

        public virtual UInt16 AverageNum
        {
            get { return m_uiAverageNum; }
            set
            {
                Reset();
                m_uiAverageNum = value;
            }
        }

        public virtual void Reset()
        {
            m_fStart = true;
            m_uiCounter = 0;
        }

        public virtual Boolean Average(IntPtr aAverageData)
        {
            return true;
        }
    }
}
