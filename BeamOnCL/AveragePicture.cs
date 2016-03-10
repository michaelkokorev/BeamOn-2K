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

        public AveragePicture(UInt64 tSize)
        {
            m_uiAverageDataSum = new UInt32[tSize];
        }

        public UInt16 AverageNum
        {
            get { return m_uiAverageNum; }
            set
            {
                Reset();
                m_uiAverageNum = value;
            }
        }

        public void Reset()
        {
            m_fStart = true;
            m_uiCounter = 0;
        }

        public virtual Boolean Average(ref object[] aAverageData)
        {
            return true;
        }
    }
}
