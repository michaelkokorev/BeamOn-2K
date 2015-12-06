using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BeamOn_2K
{
    public enum FileType { ftLog, ftExcel, ftXML };
    public enum LogType { ltTime, ltPoints, ltManual };

    public sealed class SystemData
    {
        public LogData logData;

        static readonly SystemData myInstance = new SystemData();

        static SystemData() { }

        public static SystemData MyInstance
        {
            get { return myInstance; }
        }

        SystemData()
        {
            logData = new LogData();
        }
    }

    public class LogData
    {
        //Status
        public Boolean bStart = false;

        //File
        public FileType ftFile = FileType.ftLog;
        public String strFileName;

        //Data
        public LogType ltMode = LogType.ltTime;
        public UInt32 LogInterval = 0;
        public UInt32 LogDuration = 5;
        public UInt32 LogNumPoints = 1;

        //Last Time
        public Double LastMeasureTime = 0;

        //Data
        public Boolean bPower = true;
        public Boolean bPositionX = true;
        public Boolean bPositionY = true;
        public Boolean bWidthW1 = true;
        public Boolean bWidthW2 = true;
        public Boolean bWidthW3 = true;
        public Boolean bWidthV1 = true;
        public Boolean bWidthV2 = true;
        public Boolean bWidthV3 = true;
        public Boolean bGaussfitV = false;
        public Boolean bGaussfitW = false;
        public Boolean bGaussWidthW1 = false;
        public Boolean bGaussWidthW2 = false;
        public Boolean bGaussWidthW3 = false;
        public Boolean bGaussWidthV1 = false;
        public Boolean bGaussWidthV2 = false;
        public Boolean bGaussWidthV3 = false;

        public Boolean bMajor = false;
        public Boolean bMinor = false;
        public Boolean bOrientation = false;
    }
}
