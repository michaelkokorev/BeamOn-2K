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

        public void Start(PixelFormat pixelFormat)
        {
            m_camera.CameraOpened += Configuration.AcquireContinuous;

            m_camera.Open();

            m_camera.Parameters[PLCamera.BinningHorizontal].SetValue((int)m_camera.Parameters[PLCamera.BinningHorizontal].GetMinimum());
            m_camera.Parameters[PLCamera.BinningVertical].SetValue((int)m_camera.Parameters[PLCamera.BinningHorizontal].GetMinimum());

            m_camera.Parameters[PLCamera.Width].SetValue((int)m_camera.Parameters[PLCamera.Width].GetMaximum());
            m_camera.Parameters[PLCamera.Height].SetValue((int)m_camera.Parameters[PLCamera.Height].GetMaximum());

            m_camera.Parameters[PLCamera.OffsetX].SetValue(0);
            m_camera.Parameters[PLCamera.OffsetY].SetValue(0);

            CreateData(pixelFormat);

            StartGrabber();
        }

        public void Stop()
        {
            m_camera.StreamGrabber.Stop();
            m_camera.Close();
        }

        public void StartGrabber()
        {
            m_camera.StreamGrabber.Start(GrabStrategy.LatestImages/*.OneByOne*/, GrabLoop.ProvidedByStreamGrabber);
        }

        public void StopGrabber()
        {
            m_camera.StreamGrabber.Stop();
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

        public Rectangle ImageRectangle
        {
            get { return (m_snapshot != null) ? m_snapshot.ImageRectangle : new Rectangle(0, 0, 0, 0); }
        }

        public void CreateData(PixelFormat pixelFormat)
        {
            m_camera.Parameters[PLCamera.PixelFormat].TrySetValue((pixelFormat == PixelFormat.Format8bppIndexed) ? PLCamera.PixelFormat.Mono8 : PLCamera.PixelFormat.Mono12);

            if (m_camera.Parameters[PLCamera.PixelFormat].GetValue() == PLCamera.PixelFormat.Mono8)
                m_snapshot = new Snapshot<byte>(new Rectangle(0, 0, (int)m_camera.Parameters[PLCamera.Width].GetValue(), (int)m_camera.Parameters[PLCamera.Height].GetValue()));
            else
                m_snapshot = new Snapshot<ushort>(new Rectangle(0, 0, (int)m_camera.Parameters[PLCamera.Width].GetValue(), (int)m_camera.Parameters[PLCamera.Height].GetValue()));
        }

        public int MaxBinning
        {
            get { return (int)m_camera.Parameters[PLCamera.BinningHorizontal].GetMaximum(); }
        }

        public int MinBinning
        {
            get { return (int)m_camera.Parameters[PLCamera.BinningHorizontal].GetMinimum(); }
        }

        public int Binning
        {
            get { return (int)m_camera.Parameters[PLCamera.BinningHorizontal].GetValue(); }

            set
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
}
