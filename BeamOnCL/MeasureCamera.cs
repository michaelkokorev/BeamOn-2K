using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Basler.Pylon;
using System.Drawing;
using System.Drawing.Imaging;

namespace BeamOnCL
{
    public class MeasureCamera
    {
        Camera m_camera = null;
        List<ICameraInfo> cameraList = null;

        public delegate void ChangeStatusCamera(object sender, EventArgs e);
        public event ChangeStatusCamera OnChangeStatusCamera;

        public class NewDataRecevedEventArgs : EventArgs
        {
            private SnapshotBase m_snapshot = null;

            public NewDataRecevedEventArgs(SnapshotBase snapshot)
            {
                m_snapshot = snapshot;
            }

            public SnapshotBase Snapshot
            {
                get { return m_snapshot; }
            }
        }

        public delegate void NewDataReceved(object sender, NewDataRecevedEventArgs e);
        public event NewDataReceved OnNewDataReceved;

        SnapshotBase m_snapshot = null;

        public MeasureCamera()
        {
            cameraList = CameraFinder.Enumerate(DeviceType.Usb);

            if (cameraList.Count > 0)
            {
                m_camera = new Camera(cameraList.ElementAt(0));

                m_camera.CameraOpened += new EventHandler<EventArgs>(m_camera_CameraOpened);
                m_camera.CameraClosing += new EventHandler<EventArgs>(m_camera_CameraClosing);
                m_camera.ConnectionLost += new EventHandler<EventArgs>(m_camera_ConnectionLost);

                m_camera.StreamGrabber.ImageGrabbed += new EventHandler<ImageGrabbedEventArgs>(StreamGrabber_ImageGrabbed);
            }
        }

        public SnapshotBase Snapshot
        {
            get { return m_snapshot; }
        }

        public Boolean Start(PixelFormat pixelFormat)
        {
            Boolean bRet = false;

            if (m_camera != null)
            {
                m_camera.CameraOpened += Configuration.AcquireContinuous;

                m_camera.Open();

                m_camera.Parameters[PLCamera.GainAuto].TrySetValue(PLCamera.GainAuto.Off);
                m_camera.Parameters[PLCamera.ExposureAuto].TrySetValue(PLCamera.ExposureAuto.Off);

                m_camera.Parameters[PLCamera.BinningHorizontal].SetValue((int)m_camera.Parameters[PLCamera.BinningHorizontal].GetMinimum());
                m_camera.Parameters[PLCamera.BinningVertical].SetValue((int)m_camera.Parameters[PLCamera.BinningHorizontal].GetMinimum());

                m_camera.Parameters[PLCamera.Width].SetValue((int)m_camera.Parameters[PLCamera.Width].GetMaximum());
                m_camera.Parameters[PLCamera.Height].SetValue((int)m_camera.Parameters[PLCamera.Height].GetMaximum());

                m_camera.Parameters[PLCamera.OffsetX].SetValue(0);
                m_camera.Parameters[PLCamera.OffsetY].SetValue(0);

                this.pixelFormat = pixelFormat;

                StartGrabber();

                bRet = true;
            }

            return bRet;
        }

        public void Stop()
        {
            if (m_camera != null)
            {
                m_camera.StreamGrabber.Stop();
                m_camera.Close();
            }
        }

        public void StartGrabber()
        {
            if (m_camera != null) m_camera.StreamGrabber.Start(GrabStrategy.LatestImages/*.OneByOne*/, GrabLoop.ProvidedByStreamGrabber);
        }

        public void StopGrabber()
        {
            if (m_camera != null) m_camera.StreamGrabber.Stop();
        }

        void m_camera_ConnectionLost(object sender, EventArgs e)
        {
            OnChangeStatusCamera(sender, e);
        }

        void m_camera_CameraClosing(object sender, EventArgs e)
        {
            OnChangeStatusCamera(sender, e);
        }

        void m_camera_CameraOpened(object sender, EventArgs e)
        {
            OnChangeStatusCamera(sender, e);
        }

        void StreamGrabber_ImageGrabbed(object sender, ImageGrabbedEventArgs e)
        {
            IGrabResult grabResult = e.GrabResult;

            if (grabResult.GrabSucceeded)
            {
                if (m_snapshot != null)
                {
                    m_snapshot.GetData(grabResult.PixelData as byte[]);

                    OnNewDataReceved(this, new NewDataRecevedEventArgs(m_snapshot));
                }
            }
            else
            {
                Console.WriteLine("Error: {0} {1}", grabResult.ErrorCode, grabResult.ErrorDescription);
                StartGrabber();
            }
        }

        public void SetImageDataArray(IntPtr Data, Color[] colorArray = null)
        {
            if (m_snapshot != null) m_snapshot.SetImageDataArray(Data, colorArray);
        }

        public Rectangle MaxImageRectangle
        {
            get { return (m_camera != null) ? new Rectangle(0, 0, (int)m_camera.Parameters[PLCamera.Width].GetMaximum(), (int)m_camera.Parameters[PLCamera.Height].GetMaximum()) : new Rectangle(); }
        }

        public Rectangle ImageRectangle
        {
            get { return (m_snapshot != null) ? m_snapshot.ImageRectangle : new Rectangle(0, 0, 0, 0); }

            set
            {
                if (m_camera != null)
                {
                    int iOffsetX = (int)(Math.Floor(value.X / 4f) * 4);

                    if (iOffsetX < (int)m_camera.Parameters[PLCamera.OffsetX].GetMinimum()) iOffsetX = (int)m_camera.Parameters[PLCamera.OffsetX].GetMinimum();
                    if (iOffsetX > (int)m_camera.Parameters[PLCamera.OffsetX].GetMaximum()) iOffsetX = (int)m_camera.Parameters[PLCamera.OffsetX].GetMaximum();

                    m_camera.Parameters[PLCamera.OffsetX].SetValue(iOffsetX);

                    int iOffsetY = (int)(Math.Floor(value.Y / 4f) * 4);

                    if (iOffsetY < (int)m_camera.Parameters[PLCamera.OffsetY].GetMinimum()) iOffsetY = (int)m_camera.Parameters[PLCamera.OffsetY].GetMinimum();
                    if (iOffsetY > (int)m_camera.Parameters[PLCamera.OffsetY].GetMaximum()) iOffsetY = (int)m_camera.Parameters[PLCamera.OffsetY].GetMaximum();

                    m_camera.Parameters[PLCamera.OffsetY].SetValue(iOffsetY);

                    int iWidth = (int)(Math.Floor(value.Width / 4f) * 4);

                    if (iWidth < (int)m_camera.Parameters[PLCamera.Width].GetMinimum()) iWidth = (int)m_camera.Parameters[PLCamera.Width].GetMinimum();
                    if (iWidth > (int)m_camera.Parameters[PLCamera.Width].GetMaximum()) iWidth = (int)m_camera.Parameters[PLCamera.Width].GetMaximum();

                    m_camera.Parameters[PLCamera.Width].SetValue(iWidth);

                    int iHeight = (int)(Math.Floor(value.Height / 4f) * 4);

                    if (iHeight < (int)m_camera.Parameters[PLCamera.Height].GetMinimum()) iHeight = (int)m_camera.Parameters[PLCamera.Height].GetMinimum();
                    if (iHeight > (int)m_camera.Parameters[PLCamera.Height].GetMaximum()) iHeight = (int)m_camera.Parameters[PLCamera.Height].GetMaximum();

                    m_camera.Parameters[PLCamera.Height].SetValue(iHeight);

                    if (m_camera.Parameters[PLCamera.PixelFormat].GetValue() == PLCamera.PixelFormat.Mono8)
                        m_snapshot = new Snapshot<byte>(new Rectangle(0, 0, (int)m_camera.Parameters[PLCamera.Width].GetValue(), (int)m_camera.Parameters[PLCamera.Height].GetValue()));
                    else
                        m_snapshot = new Snapshot<ushort>(new Rectangle(0, 0, (int)m_camera.Parameters[PLCamera.Width].GetValue(), (int)m_camera.Parameters[PLCamera.Height].GetValue()));
                }
            }
        }

        public PixelFormat pixelFormat
        {
            get { return (m_camera != null) ? ((m_camera.Parameters[PLCamera.PixelFormat].GetValue() == PLCamera.PixelFormat.Mono8) ? PixelFormat.Format8bppIndexed : PixelFormat.Format24bppRgb) : PixelFormat.DontCare; }

            set
            {
                if (m_camera != null)
                {
                    m_camera.Parameters[PLCamera.PixelFormat].TrySetValue((value == PixelFormat.Format8bppIndexed) ? PLCamera.PixelFormat.Mono8 : PLCamera.PixelFormat.Mono12);

                    if (m_camera.Parameters[PLCamera.PixelFormat].GetValue() == PLCamera.PixelFormat.Mono8)
                        m_snapshot = new Snapshot<byte>(new Rectangle(0, 0, (int)m_camera.Parameters[PLCamera.Width].GetValue(), (int)m_camera.Parameters[PLCamera.Height].GetValue()));
                    else
                        m_snapshot = new Snapshot<ushort>(new Rectangle(0, 0, (int)m_camera.Parameters[PLCamera.Width].GetValue(), (int)m_camera.Parameters[PLCamera.Height].GetValue()));
                }
            }
        }

        public int MaxBinning
        {
            get { return (m_camera != null) ? (int)m_camera.Parameters[PLCamera.BinningHorizontal].GetMaximum() : 0; }
        }

        public int MinBinning
        {
            get { return (m_camera != null) ? (int)m_camera.Parameters[PLCamera.BinningHorizontal].GetMinimum() : 0; }
        }

        public int Binning
        {
            get { return (m_camera != null) ? (int)m_camera.Parameters[PLCamera.BinningHorizontal].GetValue() : 0; }

            set
            {
                if (m_camera != null)
                {
                    if ((value >= (int)m_camera.Parameters[PLCamera.BinningHorizontal].GetMinimum()) && (value <= (int)m_camera.Parameters[PLCamera.BinningHorizontal].GetMaximum()))
                    {
                        m_camera.Parameters[PLCamera.BinningHorizontal].SetValue(value);
                        m_camera.Parameters[PLCamera.BinningVertical].SetValue(value);

                        if (m_camera.Parameters[PLCamera.PixelFormat].GetValue() == PLCamera.PixelFormat.Mono8)
                            m_snapshot = new Snapshot<byte>(new Rectangle(0, 0, (int)m_camera.Parameters[PLCamera.Width].GetValue(), (int)m_camera.Parameters[PLCamera.Height].GetValue()));
                        else
                            m_snapshot = new Snapshot<ushort>(new Rectangle(0, 0, (int)m_camera.Parameters[PLCamera.Width].GetValue(), (int)m_camera.Parameters[PLCamera.Height].GetValue()));
                    }
                }
            }
        }

        public int MaxGain
        {
            get { return (m_camera != null) ? (int)m_camera.Parameters[PLCamera.Gain].GetMaximum() : 0; }
        }

        public int MinGain
        {
            get { return (m_camera != null) ? (int)m_camera.Parameters[PLCamera.Gain].GetMinimum() : 0; }
        }

        public int Gain
        {
            get { return (m_camera != null) ? (int)m_camera.Parameters[PLCamera.Gain].GetValue() : 0; }

            set
            {
                if (m_camera != null)
                {
                    if ((value >= (int)m_camera.Parameters[PLCamera.Gain].GetMinimum()) && (value <= (int)m_camera.Parameters[PLCamera.Gain].GetMaximum()))
                    {
                        m_camera.Parameters[PLCamera.Gain].SetValue(value);
                    }
                }
            }
        }

        public int MaxExposure
        {
            get { return (m_camera != null) ? (int)m_camera.Parameters[PLCamera.ExposureTime].GetMaximum() : 0; }
        }

        public int MinExposure
        {
            get { return (m_camera != null) ? (int)m_camera.Parameters[PLCamera.ExposureTime].GetMinimum() : 0; }
        }

        public int Exposure
        {
            get { return (m_camera != null) ? (int)m_camera.Parameters[PLCamera.ExposureTime].GetValue() : 0; }

            set
            {
                if (m_camera != null)
                {
                    if ((value >= (int)m_camera.Parameters[PLCamera.ExposureTime].GetMinimum()) && (value <= (int)m_camera.Parameters[PLCamera.ExposureTime].GetMaximum()))
                    {
                        m_camera.Parameters[PLCamera.ExposureTime].SetValue(value);
                    }
                }
            }
        }
    }
}
