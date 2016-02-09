using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace BeamOnCL
{
    class LineProfile : Profile
    {
        public LineProfile(Rectangle rect)
            : base(rect)
        {
            CrossPoint = new Point((int)(m_rArea.Width / 2f), (int)(m_rArea.Height / 2f));
        }
    }
}
