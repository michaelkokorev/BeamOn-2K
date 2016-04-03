using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VideoControl
{
    public class PositionEventArgs : EventArgs
    {
        public UInt32 CurrentPosition { get; set; }
    }
}
