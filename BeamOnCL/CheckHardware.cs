using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using Basler.Pylon;
using System.Diagnostics;

namespace BeamOnCL
{
    public class CheckHardware
    {
        public enum CheckStatus { csHardware = 0, csHead = 1, csTypeHead = 2, csOk = 3 };

        CheckStatus chStatus = CheckStatus.csHardware;

        public class NewCheckLevelEventArgs : EventArgs
        {
            private CheckStatus chStatus = CheckStatus.csHardware;

            public NewCheckLevelEventArgs(CheckStatus chStatus)
            {
                this.chStatus = chStatus;
            }

            public CheckStatus Status
            {
                get { return chStatus; }
            }
        }

        public delegate void GetCheckError(object sender, NewCheckLevelEventArgs e);
        public event GetCheckError OnGetCheckError;

        public delegate void GetCheckLevel(object sender, NewCheckLevelEventArgs e);
        public event GetCheckLevel OnGetCheckLevel;

        Camera m_camera = null;
        List<ICameraInfo> cameraList = null;

        const int cTimeOutMs = 2000;

        public Boolean StartCheck()
        {
            Boolean bRet = false;
            UInt16 nCount = 0;

            chStatus = CheckStatus.csHardware; 
            OnGetCheckLevel(this, new NewCheckLevelEventArgs(CheckStatus.csHardware));

            cameraList = CameraFinder.Enumerate(DeviceType.Usb);

            if (cameraList.Count > 0)
            {
                m_camera = new Camera(cameraList.ElementAt(0));

                if (m_camera != null)
                {
                    chStatus = CheckStatus.csHead;
                    OnGetCheckLevel(this, new NewCheckLevelEventArgs(CheckStatus.csHead));

                    // Set the acquisition mode to free running continuous acquisition when the camera is opened.
                    m_camera.CameraOpened += Configuration.AcquireContinuous;

                    // For demonstration purposes, only add an event handler for connection loss.
                    m_camera.ConnectionLost += OnConnectionLost;

                    m_camera.Open();

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
                                        OnGetCheckLevel(this, new NewCheckLevelEventArgs(CheckStatus.csOk));
                                    }
                                    else if (nCount == 0)
                                    {
                                        chStatus = CheckStatus.csTypeHead;
                                        OnGetCheckLevel(this, new NewCheckLevelEventArgs(CheckStatus.csTypeHead));
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
                            OnGetCheckError(this, new NewCheckLevelEventArgs(chStatus));
                        }
                    }
                }
            }
            else
                OnGetCheckError(this, new NewCheckLevelEventArgs(chStatus));

            if (m_camera != null)
            {
                m_camera.StreamGrabber.Stop();
                m_camera.Close();
            }

            return bRet;
        }

        // Event handler for connection loss, is shown here for demonstration purposes only.
        // Note: This event is always called on a separate thread.
        public void OnConnectionLost(Object sender, EventArgs e)
        {
            // For demonstration purposes, print a message.
            OnGetCheckError(this, new NewCheckLevelEventArgs(chStatus));
        }
    }
}
