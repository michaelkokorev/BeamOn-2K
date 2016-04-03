using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using Basler.Pylon;
using System.Diagnostics;
using System.Threading;

namespace BeamOnCL
{
    public class CheckHardware
    {
        public enum CheckStatus { csHardware = 0, csHead = 1, csTypeHead = 2, csOk = 3 };

        CheckStatus chStatus = CheckStatus.csHardware;
        UInt16 nCount = 0;

        public class NewCheckLevelEventArgs : EventArgs
        {
            private CheckStatus chStatus = CheckStatus.csHardware;
            private string m_strUserDefinedName;

            public NewCheckLevelEventArgs(CheckStatus chStatus, string strUserDefinedName)
            {
                this.chStatus = chStatus;
                this.m_strUserDefinedName = strUserDefinedName;
            }

            public CheckStatus Status
            {
                get { return chStatus; }
            }

            public String UserDefinedName
            {
                get { return m_strUserDefinedName; }
            }
        }

        public delegate void GetCheckError(object sender, NewCheckLevelEventArgs e);
        public event GetCheckError OnGetCheckError;

        public delegate void GetCheckLevel(object sender, NewCheckLevelEventArgs e);
        public event GetCheckLevel OnGetCheckLevel;

        const int cTimeOutMs = 2000;
        private string m_strSerialNumber;
        private string m_strUserDefinedName;

        public String UserDefinedName
        {
            get { return m_strUserDefinedName; }
        }

#if BSLR
        public Boolean StartCheck()
        {
            Boolean bRet = false;

            chStatus = CheckStatus.csHardware;
            OnGetCheckLevel(this, new NewCheckLevelEventArgs(CheckStatus.csHardware, m_strUserDefinedName));

            Camera m_camera = null;
            List<ICameraInfo> cameraList = null;

            cameraList = CameraFinder.Enumerate(DeviceType.Usb);

            if (cameraList.Count > 0)
            {
                m_camera = new Camera(cameraList.ElementAt(0));

                if (m_camera.CameraInfo.ContainsKey(CameraInfoKey.DefaultGateway)) m_strSerialNumber = m_camera.CameraInfo[CameraInfoKey.DefaultGateway];

                if (m_camera != null)
                {
                    chStatus = CheckStatus.csHead;
                    OnGetCheckLevel(this, new NewCheckLevelEventArgs(CheckStatus.csHead, m_strUserDefinedName));

                    // Set the acquisition mode to free running continuous acquisition when the camera is opened.
                    m_camera.CameraOpened += Configuration.AcquireContinuous;

                    // For demonstration purposes, only add an event handler for connection loss.
                    m_camera.ConnectionLost += OnConnectionLost;

                    m_camera.Open();

                    m_strUserDefinedName = m_camera.Parameters[PLCamera.DeviceUserID].GetValueOrDefault("No Name"); 

                    // Start the grabbing.
                    m_camera.StreamGrabber.Start();

                    Stopwatch stopWatch = new Stopwatch();
                    stopWatch.Start();

                    // Grab and display images until timeout.
                    while (m_camera.StreamGrabber.IsGrabbing && stopWatch.ElapsedMilliseconds < cTimeOutMs)
                    {
                        try
                        {
                            // Wait for an image and then retrieve it. A timeout of 5000 ms is used.
                            IGrabResult grabResult = m_camera.StreamGrabber.RetrieveResult(5000, TimeoutHandling.ThrowException);

                            using (grabResult)
                            {
                                // Image grabbed successfully?
                                if (grabResult.GrabSucceeded)
                                {
                                    if (nCount > 50)
                                    {
                                        chStatus = CheckStatus.csOk;
                                        OnGetCheckLevel(this, new NewCheckLevelEventArgs(CheckStatus.csOk, m_strUserDefinedName));
                                    }
                                    else if (nCount == 0)
                                    {
                                        chStatus = CheckStatus.csTypeHead;
                                        OnGetCheckLevel(this, new NewCheckLevelEventArgs(CheckStatus.csTypeHead, m_strUserDefinedName));
                                        nCount++;
                                    }
                                    else
                                    {
                                        nCount++;
                                    }
                                }
                            }
                        }
                        catch (Exception)
                        {
                            OnGetCheckError(this, new NewCheckLevelEventArgs(chStatus, m_strUserDefinedName));
                        }
                    }
                }
            }
            else
                OnGetCheckError(this, new NewCheckLevelEventArgs(chStatus, m_strUserDefinedName));

            if (m_camera != null)
            {
                m_camera.StreamGrabber.Stop();
                m_camera.Close();
            }

            return bRet;
        }
#elif IDS
        void cam_EventFrame(object sender, EventArgs e)
        {
            uEye.Camera camera = sender as uEye.Camera;

            if (camera.IsOpened) nCount++;
        }

        public Boolean StartCheck()
        {
            Boolean bRet = false;

            chStatus = CheckStatus.csHardware;
            OnGetCheckLevel(this, new NewCheckLevelEventArgs(CheckStatus.csHardware, m_strUserDefinedName));

            uEye.Types.CameraInformation[] cameraList = null;
            uEye.Info.Camera.GetCameraList(out cameraList);

            uEye.Camera m_camera = null;

            if ((cameraList != null) && (cameraList.Length > 0))
            {
                m_camera = new uEye.Camera();

                if ((m_camera != null) && (m_camera.Init(1) == uEye.Defines.Status.SUCCESS))
                {
                    uEye.Types.CameraInfo cameraInfo;
                    m_camera.Information.GetCameraInfo(out cameraInfo);

                    m_strSerialNumber = cameraInfo.SerialNumber;

                    chStatus = CheckStatus.csHead;
                    OnGetCheckLevel(this, new NewCheckLevelEventArgs(CheckStatus.csHead, m_strUserDefinedName));

                    if (m_camera.Memory.Allocate() == uEye.Defines.Status.SUCCESS)
                    {
                        m_camera.EventFrame += new EventHandler(cam_EventFrame);

                        if (m_camera.Acquisition.Capture() == uEye.Defines.Status.SUCCESS)
                        {
                            Stopwatch stopWatch = new Stopwatch();
                            stopWatch.Start();

                            Thread.Sleep(300);
                            if (nCount > 0) OnGetCheckLevel(this, new NewCheckLevelEventArgs(CheckStatus.csTypeHead, m_strUserDefinedName));

                            Thread.Sleep(3000);

                            //// Grab and display images until timeout.
                            //while (m_camera.IsOpened && stopWatch.ElapsedMilliseconds < cTimeOutMs)
                            //{
                            //    if (nCount > 50) break;
                            //}

                            if (nCount > 50)
                                OnGetCheckLevel(this, new NewCheckLevelEventArgs(CheckStatus.csOk, m_strUserDefinedName));
                            else
                                OnGetCheckError(this, new NewCheckLevelEventArgs(chStatus, m_strUserDefinedName));
                        }
                        else
                            OnGetCheckError(this, new NewCheckLevelEventArgs(chStatus, m_strUserDefinedName));
                    }
                    else
                        OnGetCheckError(this, new NewCheckLevelEventArgs(chStatus, m_strUserDefinedName));
                }
                else
                    OnGetCheckError(this, new NewCheckLevelEventArgs(chStatus, m_strUserDefinedName));
            }
            else
                OnGetCheckError(this, new NewCheckLevelEventArgs(chStatus, m_strUserDefinedName));

            if (m_camera != null) m_camera.Exit();

            return bRet;
        }
#endif
        // Event handler for connection loss, is shown here for demonstration purposes only.
        // Note: This event is always called on a separate thread.
        public void OnConnectionLost(Object sender, EventArgs e)
        {
            // For demonstration purposes, print a message.
            OnGetCheckError(this, new NewCheckLevelEventArgs(chStatus, m_strUserDefinedName));
        }
    }
}
