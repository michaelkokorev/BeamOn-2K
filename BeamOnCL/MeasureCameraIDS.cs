using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Drawing;

namespace BeamOnCL
{
    public class MeasureCameraIDS : MeasureCameraBase
    {
        uEye.Camera m_camera = null;
        uEye.Types.CameraInformation[] cameraList;
        private Stopwatch stopWatch = null;

        public MeasureCameraIDS()
            : base()
        {
            uEye.Info.Camera.GetCameraList(out cameraList);

            if ((cameraList != null) && (cameraList.Length > 0))
            {
                m_camera = new uEye.Camera();

                if (m_camera.Init(1) == uEye.Defines.Status.SUCCESS)
                {
                    if (m_camera.Memory.Allocate() == uEye.Defines.Status.SUCCESS)
                    {
                        m_camera.EventFrame += new EventHandler(cam_EventFrame);
                    }
                }
            }

        }

        void cam_EventFrame(object sender, EventArgs e)
        {
            uEye.Camera camera = sender as uEye.Camera;

            if (camera.IsOpened)
            {
                uEye.Defines.DisplayMode mode;
                camera.Display.Mode.Get(out mode);

                // only display in dib mode
                if (mode == uEye.Defines.DisplayMode.DiB)
                {
                    Int32 s32MemID;
                    camera.Memory.GetActive(out s32MemID);

                    if (m_snapshot != null)
                    {
                        System.IntPtr ptrImage;
                        camera.Memory.ToIntPtr(s32MemID, out ptrImage);

                        stopWatch.Stop();

                        if(m_bFastMode) m_lTimeStamp += stopWatch.ElapsedMilliseconds;

                        if (m_snapshot.GetData(ptrImage, m_lTimeStamp) == true)
                            m_camera_CameraOpened(this, new NewDataRecevedEventArgs(m_snapshot, m_bFastMode));

                        stopWatch = Stopwatch.StartNew();
                        stopWatch.Start();
                    }
                }
            }
        }

        public override Boolean Start(PixelFormat pixelFormat)
        {
            Boolean bRet = false;

            if (m_camera != null)
            {
                try
                {
                    stopWatch = Stopwatch.StartNew();
                    stopWatch.Start();

                    uEye.Types.CameraInfo cameraInfo;
                    m_camera.Information.GetCameraInfo(out cameraInfo);

                    m_strSerialNumber = cameraInfo.SerialNumber;

                    uEye.Defines.Status statusRet;
                    uEye.Types.Range<Double> range;

                    statusRet = m_camera.Timing.Framerate.GetFrameRateRange(out range);
                    // set framerate
                    statusRet = m_camera.Timing.Framerate.Set(range.Maximum);


                    m_camera.AutoFeatures.Software.Gain.SetEnable(false);
                    m_camera.AutoFeatures.Software.Shutter.SetEnable(false);
                    m_camera.AutoFeatures.Software.WhiteBalance.SetEnable(false);

                    m_camera.Size.Binning.Set(uEye.Defines.BinningMode.Disable);

                    this.ImageRectangle = MaxImageRectangle;
                    this.pixelFormat = pixelFormat;

                    bRet = true;
                }
                catch (Exception)
                {
                }
            }

            return bRet;
        }

        public override void Stop()
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
                m_camera.Exit();
                m_camera = null;
            }
        }

        public override void StartGrabber()
        {
            Boolean isLive;
            uEye.Defines.Status statusRet;

            // memory reallocation
            Int32[] memList;
            statusRet = m_camera.Memory.GetList(out memList);
            statusRet = m_camera.Memory.Free(memList);
            statusRet = m_camera.Memory.Allocate();

            statusRet = m_camera.Acquisition.HasStarted(out isLive);

            if (!isLive) statusRet = m_camera.Acquisition.Capture();
        }

        public override void StopGrabber()
        {
            Boolean isLive;
            uEye.Defines.Status statusRet;

            statusRet = m_camera.Acquisition.HasStarted(out isLive);

            if (isLive) statusRet = m_camera.Acquisition.Stop(uEye.Defines.DeviceParameter.Wait);
        }

        public override Rectangle MaxImageRectangle
        {
            get
            {
                Rectangle rect = new Rectangle(0, 0, 0, 0);
                try
                {
                    if (m_camera != null)
                    {
                        uEye.Types.Range<Int32> rangeWidth, rangeHeight;

                        m_camera.Size.AOI.GetSizeRange(out rangeWidth, out rangeHeight);

                        rect = new Rectangle(0, 0, rangeWidth.Maximum, rangeHeight.Maximum);
                    }
                }
                catch
                {
                }

                return rect;
            }
        }

        public override Rectangle ImageRectangle
        {
            get { return m_rImageRectangle; }

            set
            {
                if (m_camera != null)
                {
                    uEye.Types.Range<Int32> rangePosX, rangePosY;
                    uEye.Types.Range<Int32> rangeWidth, rangeHeight;

                    m_camera.Size.AOI.GetPosRange(out rangePosX, out rangePosY);
                    m_camera.Size.AOI.GetSizeRange(out rangeWidth, out rangeHeight);

                    while ((value.Y % rangePosY.Increment) != 0) --value.Y;

                    while ((value.X % rangePosX.Increment) != 0) --value.X;

                    while ((value.Width % rangeWidth.Increment) != 0) --value.Width;

                    while ((value.Height % rangeHeight.Increment) != 0) --value.Height;

                    m_camera.Size.AOI.Set(value);

                    // memory reallocation
                    Int32[] memList;
                    m_camera.Memory.GetList(out memList);
                    m_camera.Memory.Free(memList);
                    m_camera.Memory.Allocate();

                    m_camera.Size.AOI.Get(out m_rImageRectangle);

                    CreateSnapshot();
                }
            }
        }

        public override PixelFormat pixelFormat
        {
            get
            {
                if (m_camera != null)
                {
                    uEye.Defines.ColorMode colorMode;

                    uEye.Defines.Status statusRet = m_camera.PixelFormat.Get(out colorMode);

                    return (colorMode == uEye.Defines.ColorMode.Mono8) ? PixelFormat.Format8bppIndexed : PixelFormat.Format24bppRgb;
                }
                else
                    return PixelFormat.DontCare;
            }

            set
            {
                if (m_camera != null)
                {
                    uEye.Defines.ColorMode colorMode = (value == PixelFormat.Format8bppIndexed) ? uEye.Defines.ColorMode.Mono8 : uEye.Defines.ColorMode.Mono12;
                    uEye.Defines.ColorConvertMode convertMode = uEye.Defines.ColorConvertMode.None;// = uEye.Defines.ColorConvertMode.Hardware3X3;

                    uEye.Defines.Status statusRet = m_camera.PixelFormat.Set(colorMode);
                    m_camera.Color.Converter.Set(colorMode, convertMode);

                    CreateSnapshot();
                }
            }
        }

        public override int MaxBinning
        {
            get
            {
                int iRet = 0;

                if (m_camera != null)
                {
                    uEye.Defines.BinningMode mode;
                    m_camera.Size.Binning.GetSupported(out mode);

                    if ((mode & uEye.Defines.BinningMode.Disable) == mode)
                        iRet = 1;
                    else
                    {
                        if (m_camera.Size.Binning.IsSupported(uEye.Defines.BinningMode.Vertical16X))
                            iRet = 5;
                        else if (m_camera.Size.Binning.IsSupported(uEye.Defines.BinningMode.Vertical8X))
                            iRet = 4;
                        else if (m_camera.Size.Binning.IsSupported(uEye.Defines.BinningMode.Vertical4X))
                            iRet = 3;
                        else if (m_camera.Size.Binning.IsSupported(uEye.Defines.BinningMode.Vertical2X))
                            iRet = 2;
                    }
                }

                return iRet;
            }
        }

        public override int MinBinning
        {
            get
            {
                int iRet = 0;

                if (m_camera != null)
                {
                    uEye.Defines.BinningMode mode;
                    m_camera.Size.Binning.GetSupported(out mode);

                    if ((mode & uEye.Defines.BinningMode.Disable) == mode)
                        iRet = 1;
                    else
                    {
                        if (m_camera.Size.Binning.IsSupported(uEye.Defines.BinningMode.Vertical2X))
                            iRet = 2;
                        else if (m_camera.Size.Binning.IsSupported(uEye.Defines.BinningMode.Vertical4X))
                            iRet = 3;
                        else if (m_camera.Size.Binning.IsSupported(uEye.Defines.BinningMode.Vertical8X))
                            iRet = 4;
                        else if (m_camera.Size.Binning.IsSupported(uEye.Defines.BinningMode.Vertical16X))
                            iRet = 5;
                    }
                }

                return iRet;
            }
        }

        public override int Binning
        {
            get
            {
                int iRet = 0;

                if (m_camera != null)
                {
                    uEye.Defines.BinningMode mode;
                    m_camera.Size.Binning.Get(out mode);
                }

                return iRet;
            }

            set
            {
                if (m_camera != null)
                {
                    uEye.Defines.BinningMode mode;
                    m_camera.Size.Binning.GetSupported(out mode);

                    if ((mode & uEye.Defines.BinningMode.Disable) == mode)
                        m_camera.Size.Binning.Set(uEye.Defines.BinningMode.Disable);
                    else
                    {
                        if ((value == 2) && (m_camera.Size.Binning.IsSupported(uEye.Defines.BinningMode.Vertical2X)))
                            m_camera.Size.Binning.Set(uEye.Defines.BinningMode.Horizontal2X | uEye.Defines.BinningMode.Vertical2X);
                        else if ((value == 3) && (m_camera.Size.Binning.IsSupported(uEye.Defines.BinningMode.Vertical4X)))
                            m_camera.Size.Binning.Set(uEye.Defines.BinningMode.Horizontal4X | uEye.Defines.BinningMode.Vertical4X);
                        else if ((value == 4) && (m_camera.Size.Binning.IsSupported(uEye.Defines.BinningMode.Vertical8X)))
                            m_camera.Size.Binning.Set(uEye.Defines.BinningMode.Horizontal8X | uEye.Defines.BinningMode.Vertical8X);
                        else if ((value == 5) && (m_camera.Size.Binning.IsSupported(uEye.Defines.BinningMode.Vertical16X)))
                            m_camera.Size.Binning.Set(uEye.Defines.BinningMode.Horizontal16X | uEye.Defines.BinningMode.Vertical16X);
                    }

                    ImageRectangle = MaxImageRectangle;

                    CreateSnapshot();
                }
            }
        }

        protected override void CreateSnapshot()
        {
            // get actual aoi
            System.Drawing.Rectangle rect;
            m_camera.Size.AOI.Get(out rect);

            if (pixelFormat == PixelFormat.Format8bppIndexed)
                m_snapshot = new Snapshot<byte>(new Rectangle(0, 0, rect.Width, rect.Height));
            else
                m_snapshot = new Snapshot<ushort>(new Rectangle(0, 0, rect.Width, rect.Height));

            m_snapshot.AverageNum = m_uiAverageNum;
        }

        public override int MaxGain
        {
            get { return 100; }
        }

        public override int MinGain
        {
            get { return 0; }
        }

        public override int Gain
        {
            get
            {
                int iRet = 0;

                if (m_camera != null)
                {
                    Boolean isRedSupported, isBlueSupported, isGreenSupported, isMasterSupported;
                    m_camera.Gain.Hardware.GetSupported(out isMasterSupported, out isRedSupported,
                                                        out isGreenSupported, out isBlueSupported);

                    if (isMasterSupported == true) m_camera.Gain.Hardware.Scaled.GetMaster(out iRet);
                }

                return iRet;
            }

            set
            {
                if ((m_camera != null) && (value >= MinGain) && (value <= MaxGain))
                {
                    Boolean isRedSupported, isBlueSupported, isGreenSupported, isMasterSupported;
                    m_camera.Gain.Hardware.GetSupported(out isMasterSupported, out isRedSupported,
                                                        out isGreenSupported, out isBlueSupported);
                    if (isMasterSupported == true) m_camera.Gain.Hardware.Scaled.SetMaster(value);
                }
            }
        }

        public override int MaxExposure
        {
            get
            {
                double f64Max = 0;

                if (m_camera != null)
                {
                    double f64Min, f64Inc;
                    bool bSupported;

                    m_camera.Timing.Exposure.GetSupported(out bSupported);

                    if (bSupported) m_camera.Timing.Exposure.GetRange(out f64Min, out f64Max, out f64Inc);
                }

                return (Int32)(f64Max * 1000);
            }
        }

        public override int MinExposure
        {
            get
            {
                double f64Min = 0;

                if (m_camera != null)
                {
                    double f64Max, f64Inc;
                    bool bSupported;

                    m_camera.Timing.Exposure.GetSupported(out bSupported);

                    if (bSupported) m_camera.Timing.Exposure.GetRange(out f64Min, out f64Max, out f64Inc);
                }

                return (Int32)(f64Min * 1000);
            }
        }

        public override int Exposure
        {
            get
            {
                double f64Value = 0;

                if (m_camera != null)
                {
                    bool bSupported;

                    m_camera.Timing.Exposure.GetSupported(out bSupported);

                    if (bSupported) m_camera.Timing.Exposure.Get(out f64Value);

                }

                return (Int32)(f64Value * 1000);
            }

            set
            {
                double f64Value = value / 1000f;

                if (m_camera != null)
                {
                    bool bSupported;

                    m_camera.Timing.Exposure.GetSupported(out bSupported);

                    if (bSupported) m_camera.Timing.Exposure.Set(f64Value);
                }
            }
        }

        public override UInt16 CameraFilter
        {
            get { return m_iCameraFilter; }
            set
            {
                //if ((value < 4) && (m_iCameraFilter != value))
                //if (value < 4)
                //{
                m_iCameraFilter = value;
                //    if (m_camera.Parameters[PLCamera.LineSelector].IsWritable == true)
                //    {
                //        m_camera.Parameters[PLCamera.LineSelector].SetValue(PLCamera.LineSelector.Line3);
                //        m_camera.Parameters[PLCamera.LineMode].SetValue(PLCamera.LineMode.Output);
                //        m_camera.Parameters[PLCamera.LineSource].SetValue(PLCamera.LineSource.UserOutput2);
                //        m_camera.Parameters[PLCamera.UserOutputSelector].SetValue(PLCamera.UserOutputSelector.UserOutput2);
                //        m_camera.Parameters[PLCamera.LineInverter].SetValue(true);
                //        m_camera.Parameters[PLCamera.UserOutputValue].SetValue(true);

                //        m_camera.Parameters[PLCamera.LineSelector].SetValue(PLCamera.LineSelector.Line4);
                //        m_camera.Parameters[PLCamera.LineMode].SetValue(PLCamera.LineMode.Output);
                //        m_camera.Parameters[PLCamera.LineSource].SetValue(PLCamera.LineSource.UserOutput3);
                //        m_camera.Parameters[PLCamera.UserOutputSelector].SetValue(PLCamera.UserOutputSelector.UserOutput3);

                //        Thread.Sleep(100);
                //        for (int i = 0; i < m_iCameraFilter;/* + 1;*/ i++)
                //        {
                //            m_camera.Parameters[PLCamera.UserOutputValue].SetValue(true);
                //            Thread.Sleep(50);
                //            m_camera.Parameters[PLCamera.UserOutputValue].SetValue(false);
                //            Thread.Sleep(100);
                //        }

                //        m_camera.Parameters[PLCamera.LineSelector].SetValue(PLCamera.LineSelector.Line3);
                //        m_camera.Parameters[PLCamera.LineMode].SetValue(PLCamera.LineMode.Output);
                //        m_camera.Parameters[PLCamera.LineSource].SetValue(PLCamera.LineSource.UserOutput2);
                //        m_camera.Parameters[PLCamera.UserOutputSelector].SetValue(PLCamera.UserOutputSelector.UserOutput2);
                //        m_camera.Parameters[PLCamera.UserOutputValue].SetValue(false);
                //    }
                //}
            }
        }
    }
}
