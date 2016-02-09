using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Basler.Pylon;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;

namespace BeamOnCL
{
    public class MeasureCamera
    {
        Camera m_camera = null;
        List<ICameraInfo> cameraList = null;

        public delegate void ChangeStatusCamera(object sender, EventArgs e);
        public event ChangeStatusCamera OnChangeStatusCamera;

        public class NewDataRecevedEventArgs : EventArgs, ICloneable
        {
            private SnapshotBase m_snapshot = null;
            private long m_timeStamp = 0;
            private bool m_bClone = false;

            public NewDataRecevedEventArgs(SnapshotBase snapshot, long timeStamp)
            {
                m_snapshot = snapshot;
                m_timeStamp = timeStamp;
            }

            public long Timestamp
            {
                get { return m_timeStamp; }
            }

            public SnapshotBase Snapshot
            {
                get { return m_snapshot; }
            }

            object ICloneable.Clone()
            {
                return this.Clone();
            }

            //
            // Summary:
            //     Indicates if the event arguments have been created by calling NewDataRecevedEventArgs.Clone().
            public bool IsClone { get{return m_bClone;} }

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
                if (m_bClone == true)
                {
                    m_snapshot = null;
                    m_timeStamp = 0;
                }
            }
        }

        public delegate void NewDataReceved(object sender, NewDataRecevedEventArgs e);
        public event NewDataReceved OnNewDataReceved;

        SnapshotBase m_snapshot = null;
        Rectangle m_rImageRectangle = new Rectangle();
        UInt16 m_iCameraFilter = 0;
        private string m_strSerialNumber;
        private string m_strUserDefinedName;

#if WATCHDOG
        static EventWaitHandle evHardwareFailure = new AutoResetEvent(false);
        Thread ThreadWatchDog = null;
        bool fWatchDogContinue;

        private void WatchDog()
        {
            while (camera.StreamGrabber.IsGrabbing)
            {
                if (evHardwareFailure.WaitOne(1500, false))
                {
                }
            }
        }
#endif
        public MeasureCamera()
        {
            cameraList = CameraFinder.Enumerate(DeviceType.Usb);

            if (cameraList.Count > 0)
            {
                m_camera = new Camera(cameraList.ElementAt(0));

                //                m_camera.CameraOpened += new EventHandler<EventArgs>(m_camera_CameraOpened);
                m_camera.CameraClosing += new EventHandler<EventArgs>(m_camera_CameraClosing);
                m_camera.ConnectionLost += new EventHandler<EventArgs>(m_camera_ConnectionLost);

                m_camera.StreamGrabber.ImageGrabbed += new EventHandler<ImageGrabbedEventArgs>(StreamGrabber_ImageGrabbed);
            }
        }

        public String SerialNumber
        {
            get { return m_strSerialNumber; }
        }

        public String UserDefinedName
        {
            get { return m_strUserDefinedName; }
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
                try
                {
                    m_camera.Open(5000, TimeoutHandling.ThrowException);

                    if (m_camera.CameraInfo.ContainsKey(CameraInfoKey.SerialNumber)) m_strSerialNumber = m_camera.CameraInfo[CameraInfoKey.SerialNumber];
                    m_strUserDefinedName = m_camera.Parameters[PLCamera.DeviceUserID].GetValueOrDefault("No Name"); 

                    m_camera.Parameters[PLCamera.GainAuto].TrySetValue(PLCamera.GainAuto.Off);
                    m_camera.Parameters[PLCamera.ExposureAuto].TrySetValue(PLCamera.ExposureAuto.Off);

                    if (m_camera.Parameters[PLCamera.BinningHorizontal].IsWritable == true) m_camera.Parameters[PLCamera.BinningHorizontal].TrySetValue((int)m_camera.Parameters[PLCamera.BinningHorizontal].GetMinimum());
                    if (m_camera.Parameters[PLCamera.BinningHorizontal].IsWritable == true) m_camera.Parameters[PLCamera.BinningVertical].TrySetValue((int)m_camera.Parameters[PLCamera.BinningHorizontal].GetMinimum());

                    m_camera.Parameters[PLCamera.Width].SetValue((int)m_camera.Parameters[PLCamera.Width].GetMaximum());
                    m_camera.Parameters[PLCamera.Height].SetValue((int)m_camera.Parameters[PLCamera.Height].GetMaximum());

                    m_camera.Parameters[PLCamera.OffsetX].SetValue(0);
                    m_camera.Parameters[PLCamera.OffsetY].SetValue(0);

                    m_rImageRectangle = new Rectangle(
                                                        (int)m_camera.Parameters[PLCamera.OffsetX].GetValue(),
                                                        (int)m_camera.Parameters[PLCamera.OffsetY].GetValue(),
                                                        (int)m_camera.Parameters[PLCamera.Width].GetValue(),
                                                        (int)m_camera.Parameters[PLCamera.Height].GetValue()
                                                      );
                    this.pixelFormat = pixelFormat;

                    //m_camera.Parameters[PLTransportLayer.HeartbeatTimeout].TrySetValue(1000, IntegerValueCorrection.Nearest);  // 1000 ms timeout

                    //                StartGrabber();
#if WATCHDOG
                fWatchDogContinue = true;
                ThreadWatchDog = new Thread(new ThreadStart(WatchDog));
                ThreadWatchDog.Start();
#endif

                    bRet = true;
                }
                catch (Exception)
                {

                }
            }

            return bRet;
        }

        public void Stop()
        {
#if WATCHDOG
            if ((ThreadWatchDog != null) && (ThreadWatchDog.IsAlive == true))
            {
                fWatchDogContinue = false;
                ThreadWatchDog.Abort();
                ThreadWatchDog.Join();
            }
#endif
            if (m_camera != null)
            {
                StopGrabber();
                m_camera.Close();
            }
        }

        // Starts the continuous grabbing of images and handles exceptions.
        private void ContinuousShot()
        {
            try
            {
                if ((m_camera != null) && (m_camera.StreamGrabber.IsGrabbing == false))
                {
                    // Start the grabbing of images until grabbing is stopped.
                    m_camera.Parameters[PLCamera.AcquisitionMode].SetValue(PLCamera.AcquisitionMode.Continuous);
                    m_camera.StreamGrabber.Start(GrabStrategy.OneByOne, GrabLoop.ProvidedByStreamGrabber);
                }
            }
            catch (Exception exception)
            {
                ShowException(exception);
            }
        }

        // Starts the grabbing of a single image and handles exceptions.
        private void OneShot()
        {
            try
            {
                if ((m_camera != null) && (m_camera.StreamGrabber.IsGrabbing == false))
                {
                    // Starts the grabbing of one image.
                    m_camera.Parameters[PLCamera.AcquisitionMode].SetValue(PLCamera.AcquisitionMode.SingleFrame);
                    m_camera.StreamGrabber.Start(1, GrabStrategy.OneByOne, GrabLoop.ProvidedByStreamGrabber);
                }
            }
            catch (Exception exception)
            {
                ShowException(exception);
            }
        }


        //// Stops the grabbing of images and handles exceptions.
        //private void Stop()
        //{
        //    // Stop the grabbing.
        //    try
        //    {
        //        m_camera.StreamGrabber.Stop();
        //    }
        //    catch (Exception exception)
        //    {
        //        ShowException(exception);
        //    }
        //}

        // Closes the camera object and handles exceptions.
        private void DestroyCamera()
        {
            // Destroy the camera object.
            try
            {
                if (m_camera != null)
                {
                    m_camera.Close();
                    m_camera.Dispose();
                    m_camera = null;
                }
            }
            catch (Exception exception)
            {
                ShowException(exception);
            }
        }

        // Shows exceptions in a message box.
        private void ShowException(Exception exception)
        {
 //           System.Windows.Form.MessageBox.Show("Exception caught:\n" + exception.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public void StartGrabber()
        {
            if ((m_camera != null) && (m_camera.StreamGrabber.IsGrabbing == false)) m_camera.StreamGrabber.Start(GrabStrategy.LatestImages/*.OneByOne*/, GrabLoop.ProvidedByStreamGrabber);
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

                    OnNewDataReceved(this, new NewDataRecevedEventArgs(m_snapshot, grabResult.Timestamp));
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
            get { return (m_camera != null) ? new Rectangle(0, 0, (int)m_camera.Parameters[PLCamera.WidthMax/*.SensorWidth*/].GetValue(), (int)m_camera.Parameters[PLCamera.HeightMax/*.SensorHeight*/].GetValue()) : new Rectangle(); }
        }

        public Rectangle ImageRectangle
        {
            get { return m_rImageRectangle; }

            set
            {
                if (m_camera != null)
                {
                    m_camera.Parameters[PLCamera.OffsetX].SetValue(m_camera.Parameters[PLCamera.OffsetX].GetMinimum());
                    m_camera.Parameters[PLCamera.OffsetY].SetValue(m_camera.Parameters[PLCamera.OffsetY].GetMinimum());

                    int iWidth = (int)(Math.Floor(value.Width / 4f) * 4);

                    if (iWidth < (int)m_camera.Parameters[PLCamera.Width].GetMinimum()) iWidth = (int)m_camera.Parameters[PLCamera.Width].GetMinimum();
                    if (iWidth > (int)m_camera.Parameters[PLCamera.Width].GetMaximum()) iWidth = (int)m_camera.Parameters[PLCamera.Width].GetMaximum();

                    m_camera.Parameters[PLCamera.Width].SetValue(iWidth);

                    int iHeight = (int)(Math.Floor(value.Height / 4f) * 4);

                    if (iHeight < (int)m_camera.Parameters[PLCamera.Height].GetMinimum()) iHeight = (int)m_camera.Parameters[PLCamera.Height].GetMinimum();
                    if (iHeight > (int)m_camera.Parameters[PLCamera.Height].GetMaximum()) iHeight = (int)m_camera.Parameters[PLCamera.Height].GetMaximum();

                    m_camera.Parameters[PLCamera.Height].SetValue(iHeight);

                    int iOffsetX = (int)(Math.Floor(value.X / 4f) * 4);

                    if (iOffsetX < (int)m_camera.Parameters[PLCamera.OffsetX].GetMinimum()) iOffsetX = (int)m_camera.Parameters[PLCamera.OffsetX].GetMinimum();
                    if (iOffsetX > (int)m_camera.Parameters[PLCamera.OffsetX].GetMaximum()) iOffsetX = (int)m_camera.Parameters[PLCamera.OffsetX].GetMaximum();

                    m_camera.Parameters[PLCamera.OffsetX].SetValue(iOffsetX);

                    int iOffsetY = (int)(Math.Floor(value.Y / 4f) * 4);

                    if (iOffsetY < (int)m_camera.Parameters[PLCamera.OffsetY].GetMinimum()) iOffsetY = (int)m_camera.Parameters[PLCamera.OffsetY].GetMinimum();
                    if (iOffsetY > (int)m_camera.Parameters[PLCamera.OffsetY].GetMaximum()) iOffsetY = (int)m_camera.Parameters[PLCamera.OffsetY].GetMaximum();

                    m_camera.Parameters[PLCamera.OffsetY].SetValue(iOffsetY);

                    m_rImageRectangle = new Rectangle(
                                                        (int)m_camera.Parameters[PLCamera.OffsetX].GetValue(), 
                                                        (int)m_camera.Parameters[PLCamera.OffsetY].GetValue(), 
                                                        (int)m_camera.Parameters[PLCamera.Width].GetValue(), 
                                                        (int)m_camera.Parameters[PLCamera.Height].GetValue()
                                                      );
                    CreateSnapshot();
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

                    CreateSnapshot();
                }
            }
        }

        public int MaxBinning
        {
            get { return ((m_camera != null) && (m_camera.Parameters[PLCamera.BinningHorizontal].IsReadable)) ? (int)m_camera.Parameters[PLCamera.BinningHorizontal].GetMaximum() : 0; }
        }

        public int MinBinning
        {
            get { return ((m_camera != null) && (m_camera.Parameters[PLCamera.BinningHorizontal].IsReadable)) ? (int)m_camera.Parameters[PLCamera.BinningHorizontal].GetMinimum() : 0; }
        }

        public int Binning
        {
            get { return ((m_camera != null) && (m_camera.Parameters[PLCamera.BinningHorizontal].IsReadable)) ? (int)m_camera.Parameters[PLCamera.BinningHorizontal].GetValue() : 0; }

            set
            {
                if ((m_camera != null) && (m_camera.Parameters[PLCamera.BinningHorizontal].IsWritable))
                {
                    if ((value >= (int)m_camera.Parameters[PLCamera.BinningHorizontal].GetMinimum()) && (value <= (int)m_camera.Parameters[PLCamera.BinningHorizontal].GetMaximum()))
                    {
                        m_camera.Parameters[PLCamera.BinningHorizontal].SetValue(value);
                        m_camera.Parameters[PLCamera.BinningVertical].SetValue(value);

                        ImageRectangle = MaxImageRectangle;

                        CreateSnapshot();
                    }
                }
            }
        }

        private void CreateSnapshot()
        {
            if (m_camera.Parameters[PLCamera.PixelFormat].GetValue() == PLCamera.PixelFormat.Mono8)
                m_snapshot = new Snapshot<byte>(new Rectangle(0, 0, (int)m_camera.Parameters[PLCamera.Width].GetValue(), (int)m_camera.Parameters[PLCamera.Height].GetValue()));
            else
                m_snapshot = new Snapshot<ushort>(new Rectangle(0, 0, (int)m_camera.Parameters[PLCamera.Width].GetValue(), (int)m_camera.Parameters[PLCamera.Height].GetValue()));
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

        public UInt16 CameraFilter
        {
            get { return m_iCameraFilter; }
            set
            {
                //if ((value < 4) && (m_iCameraFilter != value))
                    if (value < 4)
                    {
                    m_iCameraFilter = value;
                    if (m_camera.Parameters[PLCamera.LineSelector].IsWritable == true)
                    {
                        m_camera.Parameters[PLCamera.LineSelector].SetValue(PLCamera.LineSelector.Line3);
                        m_camera.Parameters[PLCamera.LineMode].SetValue(PLCamera.LineMode.Output);
                        m_camera.Parameters[PLCamera.LineSource].SetValue(PLCamera.LineSource.UserOutput2);
                        m_camera.Parameters[PLCamera.UserOutputSelector].SetValue(PLCamera.UserOutputSelector.UserOutput2);
                        m_camera.Parameters[PLCamera.LineInverter].SetValue(true);
                        m_camera.Parameters[PLCamera.UserOutputValue].SetValue(true);

                        m_camera.Parameters[PLCamera.LineSelector].SetValue(PLCamera.LineSelector.Line4);
                        m_camera.Parameters[PLCamera.LineMode].SetValue(PLCamera.LineMode.Output);
                        m_camera.Parameters[PLCamera.LineSource].SetValue(PLCamera.LineSource.UserOutput3);
                        m_camera.Parameters[PLCamera.UserOutputSelector].SetValue(PLCamera.UserOutputSelector.UserOutput3);

                        Thread.Sleep(100);
                        for (int i = 0; i < m_iCameraFilter;/* + 1;*/ i++)
                        {
                           m_camera.Parameters[PLCamera.UserOutputValue].SetValue(true);
                            Thread.Sleep(50);
                            m_camera.Parameters[PLCamera.UserOutputValue].SetValue(false);
                            Thread.Sleep(100);
                        }

                        m_camera.Parameters[PLCamera.LineSelector].SetValue(PLCamera.LineSelector.Line3);
                        m_camera.Parameters[PLCamera.LineMode].SetValue(PLCamera.LineMode.Output);
                        m_camera.Parameters[PLCamera.LineSource].SetValue(PLCamera.LineSource.UserOutput2);
                        m_camera.Parameters[PLCamera.UserOutputSelector].SetValue(PLCamera.UserOutputSelector.UserOutput2);
                        m_camera.Parameters[PLCamera.UserOutputValue].SetValue(false);
                    }
                }
            }
        }
    }
}
