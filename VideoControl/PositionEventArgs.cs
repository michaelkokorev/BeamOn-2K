using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BeamOnCL;

namespace VideoControl
{
    public class PositionEventArgs : EventArgs
    {
        public UInt32 CurrentPosition { get; set; }

        public SnapshotBase CurrentSnapshot { get; set; }
    }
}
