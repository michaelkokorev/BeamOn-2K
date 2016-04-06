using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;

namespace BeamOnCL
{
    public class MeasureCameraBase
    {

        public delegate void ChangeStatusCamera(object sender, EventArgs e);
        public event ChangeStatusCamera OnChangeStatusCamera;

        public class NewDataRecevedEventArgs : EventArgs, ICloneable
        {
            private SnapshotBase m_snapshot = null;
            private Boolean m_bFastMode = false;
            private bool m_bClone = false;

            public NewDataRecevedEventArgs(SnapshotBase snapshot, Boolean bFastMode)
            {
                m_snapshot = snapshot;
                m_bFastMode = bFastMode;
            }

            public SnapshotBase Snapshot
            {
                get { return m_snapshot; }
            }

            object ICloneable.Clone()
            {
                return this.Clone();
            }

            public Boolean FastMode
            {
                get { return m_bFastMode; }
                set { m_bFastMode = value; }
            }
            //
            // Summary:
            //     Indicates if the event arguments have been created by calling NewDataRecevedEventArgs.Clone().
            public bool IsClone { get { return m_bClone; } }

            // Summary:
            //     Clones the event arguments including the Snapshot & Timestamp result.
            //
            // Returns:
            //     Returns a copy of the event arguments with a clone of the contained Snapshot & Timestamp
            //     result. The cloned Snapshot & Timestamp result must be disposed.
            //
            // Remarks:
            //     The Snapshot & Timestamp result or the cloned event must be disposed.
            public NewDataRecevedEventArgs Clone()
            {
                NewDataRecevedEventArgs ret = this.MemberwiseClone() as NewDataRecevedEventArgs; ;

                ret.m_snapshot = this.m_snapshot.Clone();

                return ret;
            }
            //
            // Summary:
            //     Disposes the grab result held if the event arguments have been created by
            //     calling NewDataRecevedEventArgs.Clone().
            public void DisposeDataRecevedIfClone()
            {
                if (m_bClone == true) m_snapshot = null;
            }
        }

        public delegate void NewDataReceved(object sender, NewDataRecevedEventArgs e);
        public event NewDataReceved OnNewDataReceved;

        protected SnapshotBase m_snapshot = null;
        protected Rectangle m_rImageRectangle = new Rectangle();
        protected UInt16 m_iCameraFilter = 0;
        protected string m_strSerialNumber;
        protected string m_strUserDefinedName;
        protected UInt16 m_uiAverageNum = 1;
        protected Boolean m_bFastMode = false;
        protected long m_lTimeStamp = 0;

        protected void m_camera_ConnectionLost(object sender, EventArgs e)
        {
            OnChangeStatusCamera(sender, e);
        }

        protected void m_camera_CameraClosing(object sender, EventArgs e)
        {
            OnChangeStatusCamera(sender, e);
        }

        protected void m_camera_CameraOpened(object sender, EventArgs e)
        {
            OnChangeStatusCamera(sender, e);
        }

        protected void m_camera_CameraOpened(object sender, NewDataRecevedEventArgs e)
        {
            OnNewDataReceved(sender, e);
        }

        public virtual Boolean FastMode
        {
            get { return m_bFastMode; }
            set
            {
                m_bFastMode = value;
                m_lTimeStamp = 0;
            }
        }

        public virtual String SerialNumber
        {
            get { return m_strSerialNumber; }
        }

        public virtual String UserDefinedName
        {
            get { return m_strUserDefinedName; }
        }

        public SnapshotBase Snapshot
        {
            get { return m_snapshot; }
        }

        public virtual void AverageReset()
        {
            m_snapshot.AverageReset();
        }

        public virtual UInt16 AverageNum
        {
            get { return m_uiAverageNum; }
            set
            {
                m_uiAverageNum = value;
                if (m_snapshot != null) m_snapshot.AverageNum = m_uiAverageNum;
            }
        }

        public virtual Boolean Start(PixelFormat pixelFormat)
        {
            Boolean bRet = false;

            return bRet;
        }

        public virtual void Stop()
        {
        }

        public virtual void StartGrabber()
        {
        }

        public virtual void StopGrabber()
        {
        }

        public void SetImageDataArray(IntPtr Data, Color[] colorArray = null)
        {
            if (m_snapshot != null) m_snapshot.SetImageDataArray(Data, colorArray);
        }

        public virtual Rectangle MaxImageRectangle
        {
            get
            {
                Rectangle rect = new Rectangle(0, 0, 0, 0);

                return rect;
            }
        }

        public virtual Rectangle ImageRectangle
        {
            get { return m_rImageRectangle; }

            set
            {
            }
        }

        public virtual PixelFormat pixelFormat
        {
            get { return PixelFormat.DontCare; }

            set
            {
            }
        }

        public virtual int MaxBinning
        {
            get { return 0; }
        }

        public virtual int MinBinning
        {
            get { return 0; }
        }

        public virtual int Binning
        {
            get { return 0; }

            set
            {
            }
        }

        protected virtual void CreateSnapshot()
        {
        }

        public virtual int MaxGain
        {
            get { return 0; }
        }

        public virtual int MinGain
        {
            get { return 0; }
        }

        public virtual int Gain
        {
            get { return 0; }

            set
            {
            }
        }

        public virtual int MaxExposure
        {
            get { return 0; }
        }

        public virtual int MinExposure
        {
            get { return 0; }
        }

        public virtual int Exposure
        {
            get { return 0; }

            set
            {
            }
        }

        public virtual UInt16 CameraFilter
        {
            get { return m_iCameraFilter; }
            set
            {
                m_iCameraFilter = value;
            }
        }
    }
}
