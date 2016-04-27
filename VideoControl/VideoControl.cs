using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Text;
using System.Collections;
using BeamOnCL;
using System.IO;
using System.Diagnostics;

namespace VideoControl
{
    public class VideoControl : UserControl
    {
        public enum RecordType { rtTime, rtPoints, rtManual };

        private ToolStrip toolStripVideoControl;
        private ToolStripButton toolStripButtonStart;
        private ToolStripButton toolStripButtonBack;
        private ToolStripButton toolStripButtonStop;
        private ToolStripButton toolStripButtonRecord;
        private ToolStripButton toolStripButtonForward;
        private ToolStripButton toolStripButtonEnd;
        private ToolStripButton toolStripButtonRepeat;

        public enum VideoControlCommand { vccNone, vccRepeat, vccEnd, vccForward, vccPlay, vccPause, vccRecord, vccStop, vccRewind, vccSkipToStart };
        public enum VideoMode { vmRecord, vmPlay, vmNone };
        public enum MoveMode { mmNone, mmPlay, mmRepeat };

        private VideoControlCommand m_vccComand = VideoControlCommand.vccNone;
        private VideoMode m_vmMode = VideoMode.vmNone;
        private MoveMode m_mmMove = MoveMode.mmNone;

        private UInt32 m_uiCurrentPosition = 0;
        private UInt32 m_uiMinPosition = 0;
        private UInt32 m_uiMaxPosition = 100;
        private StatusStrip statusStrip;
        private ToolStripStatusLabel toolStripStatusLabelFrame;
        private ToolStripStatusLabel toolStripStatusLabelTime;

        private ArrayList m_listSapshot = null;
        private ToolStripButton toolStripButtonPlay;
        private SnapshotBase[] m_arraySnapshot = null;

        //File
        public String strFileName;
        protected long m_lTimeStamp = 0;

        //Data
        private RecordType m_rtMode = RecordType.rtTime;
        private UInt32 m_uiRecordDuration = 5;
        private ProgressBar progressBar;
        private UInt32 m_uiRecordNumPoints = 1;
        private ToolStrip toolStripProperty;
        private ToolStripButton toolStripButtonProperty;
        private ToolStripButton toolStripButtonLoad;

        private Timer m_tmPlay = new Timer();
        private Stopwatch stopWatch = null;

        public delegate void ChangePosition(object sender, MeasureCameraBase.NewDataRecevedEventArgs e);
        public event ChangePosition OnChangePosition;

        public delegate void StopRecordData(object sender, EventArgs e);
        public event StopRecordData OnStopRecordData;

        public delegate void ChangeVideoMode(object sender, NewVideoModeEventArgs e);
        public event ChangeVideoMode OnChangeVideoMode;

        private delegate void ChangePositionAsyncDelegate(VideoMode vm, UInt32 pos);
        private delegate void ChangeRecordStatusAsyncDelegate();

        public class NewVideoModeEventArgs : EventArgs
        {
            private VideoMode vm = VideoMode.vmNone;

            public NewVideoModeEventArgs(VideoMode vm)
            {
                this.vm = vm;
            }

            public VideoMode Mode
            {
                get { return vm; }
            }
        }

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.toolStripVideoControl = new System.Windows.Forms.ToolStrip();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabelFrame = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabelTime = new System.Windows.Forms.ToolStripStatusLabel();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.toolStripProperty = new System.Windows.Forms.ToolStrip();
            this.toolStripButtonProperty = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonLoad = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonStart = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonBack = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonStop = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonPlay = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonRecord = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonForward = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonEnd = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonRepeat = new System.Windows.Forms.ToolStripButton();
            this.toolStripVideoControl.SuspendLayout();
            this.statusStrip.SuspendLayout();
            this.toolStripProperty.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStripVideoControl
            // 
            this.toolStripVideoControl.CanOverflow = false;
            this.toolStripVideoControl.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStripVideoControl.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButtonStart,
            this.toolStripButtonBack,
            this.toolStripButtonStop,
            this.toolStripButtonPlay,
            this.toolStripButtonRecord,
            this.toolStripButtonForward,
            this.toolStripButtonEnd,
            this.toolStripButtonRepeat});
            this.toolStripVideoControl.Location = new System.Drawing.Point(0, 0);
            this.toolStripVideoControl.Name = "toolStripVideoControl";
            this.toolStripVideoControl.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.toolStripVideoControl.Size = new System.Drawing.Size(303, 39);
            this.toolStripVideoControl.TabIndex = 9;
            // 
            // statusStrip
            // 
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabelFrame,
            this.toolStripStatusLabelTime});
            this.statusStrip.Location = new System.Drawing.Point(0, 98);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(303, 22);
            this.statusStrip.TabIndex = 10;
            // 
            // toolStripStatusLabelFrame
            // 
            this.toolStripStatusLabelFrame.AutoSize = false;
            this.toolStripStatusLabelFrame.BorderStyle = System.Windows.Forms.Border3DStyle.RaisedInner;
            this.toolStripStatusLabelFrame.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripStatusLabelFrame.Name = "toolStripStatusLabelFrame";
            this.toolStripStatusLabelFrame.Size = new System.Drawing.Size(150, 17);
            this.toolStripStatusLabelFrame.Text = "0 frame of 0 frames";
            // 
            // toolStripStatusLabelTime
            // 
            this.toolStripStatusLabelTime.AutoSize = false;
            this.toolStripStatusLabelTime.BorderStyle = System.Windows.Forms.Border3DStyle.RaisedInner;
            this.toolStripStatusLabelTime.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripStatusLabelTime.Name = "toolStripStatusLabelTime";
            this.toolStripStatusLabelTime.Size = new System.Drawing.Size(118, 17);
            this.toolStripStatusLabelTime.Text = "0 ms of 0 ms";
            // 
            // progressBar
            // 
            this.progressBar.Dock = System.Windows.Forms.DockStyle.Top;
            this.progressBar.Location = new System.Drawing.Point(0, 39);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(303, 11);
            this.progressBar.TabIndex = 11;
            // 
            // toolStripProperty
            // 
            this.toolStripProperty.AutoSize = false;
            this.toolStripProperty.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStripProperty.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButtonProperty,
            this.toolStripButtonLoad});
            this.toolStripProperty.Location = new System.Drawing.Point(0, 50);
            this.toolStripProperty.Name = "toolStripProperty";
            this.toolStripProperty.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.toolStripProperty.Size = new System.Drawing.Size(303, 39);
            this.toolStripProperty.TabIndex = 12;
            // 
            // toolStripButtonProperty
            // 
            this.toolStripButtonProperty.AutoSize = false;
            this.toolStripButtonProperty.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonProperty.Image = global::VideoControl.Properties.Resources.PropertyS;
            this.toolStripButtonProperty.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.toolStripButtonProperty.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonProperty.Name = "toolStripButtonProperty";
            this.toolStripButtonProperty.Size = new System.Drawing.Size(36, 36);
            this.toolStripButtonProperty.ToolTipText = "Setup Record Snapshot";
            this.toolStripButtonProperty.Click += new System.EventHandler(this.toolStripButtonProperty_Click);
            // 
            // toolStripButtonLoad
            // 
            this.toolStripButtonLoad.AutoSize = false;
            this.toolStripButtonLoad.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonLoad.Image = global::VideoControl.Properties.Resources._1460382430_178_Download;
            this.toolStripButtonLoad.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonLoad.Name = "toolStripButtonLoad";
            this.toolStripButtonLoad.Size = new System.Drawing.Size(36, 36);
            this.toolStripButtonLoad.ToolTipText = "Load Snapshot Video";
            // 
            // toolStripButtonStart
            // 
            this.toolStripButtonStart.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonStart.Image = global::VideoControl.Properties.Resources.black_skip_to_start_32;
            this.toolStripButtonStart.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.toolStripButtonStart.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonStart.Name = "toolStripButtonStart";
            this.toolStripButtonStart.Size = new System.Drawing.Size(36, 36);
            this.toolStripButtonStart.Text = "Start";
            this.toolStripButtonStart.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.toolStripButtonStart.ToolTipText = "Move To Start";
            this.toolStripButtonStart.Click += new System.EventHandler(this.toolStripButtonVideoControl_Click);
            // 
            // toolStripButtonBack
            // 
            this.toolStripButtonBack.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonBack.Image = global::VideoControl.Properties.Resources.black_rewind_32;
            this.toolStripButtonBack.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.toolStripButtonBack.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonBack.Name = "toolStripButtonBack";
            this.toolStripButtonBack.Size = new System.Drawing.Size(36, 36);
            this.toolStripButtonBack.Text = "Back";
            this.toolStripButtonBack.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.toolStripButtonBack.ToolTipText = "Move To Back";
            this.toolStripButtonBack.Click += new System.EventHandler(this.toolStripButtonVideoControl_Click);
            // 
            // toolStripButtonStop
            // 
            this.toolStripButtonStop.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonStop.Image = global::VideoControl.Properties.Resources.black_stop_32;
            this.toolStripButtonStop.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.toolStripButtonStop.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonStop.Name = "toolStripButtonStop";
            this.toolStripButtonStop.Size = new System.Drawing.Size(36, 36);
            this.toolStripButtonStop.Text = "Stop";
            this.toolStripButtonStop.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.toolStripButtonStop.Click += new System.EventHandler(this.toolStripButtonVideoControl_Click);
            // 
            // toolStripButtonPlay
            // 
            this.toolStripButtonPlay.CheckOnClick = true;
            this.toolStripButtonPlay.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonPlay.Image = global::VideoControl.Properties.Resources.black_play_32;
            this.toolStripButtonPlay.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.toolStripButtonPlay.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonPlay.Name = "toolStripButtonPlay";
            this.toolStripButtonPlay.Size = new System.Drawing.Size(36, 36);
            this.toolStripButtonPlay.Text = "Play";
            this.toolStripButtonPlay.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.toolStripButtonPlay.Click += new System.EventHandler(this.toolStripButtonVideoControl_Click);
            // 
            // toolStripButtonRecord
            // 
            this.toolStripButtonRecord.CheckOnClick = true;
            this.toolStripButtonRecord.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonRecord.Image = global::VideoControl.Properties.Resources._1460041379_player_record;
            this.toolStripButtonRecord.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.toolStripButtonRecord.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonRecord.Name = "toolStripButtonRecord";
            this.toolStripButtonRecord.Size = new System.Drawing.Size(36, 36);
            this.toolStripButtonRecord.Text = "Record";
            this.toolStripButtonRecord.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.toolStripButtonRecord.Click += new System.EventHandler(this.toolStripButtonVideoControl_Click);
            // 
            // toolStripButtonForward
            // 
            this.toolStripButtonForward.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonForward.Image = global::VideoControl.Properties.Resources.black_fast_forward_32;
            this.toolStripButtonForward.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.toolStripButtonForward.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonForward.Name = "toolStripButtonForward";
            this.toolStripButtonForward.Size = new System.Drawing.Size(36, 36);
            this.toolStripButtonForward.Text = "Forward";
            this.toolStripButtonForward.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.toolStripButtonForward.ToolTipText = "Move To Forward";
            this.toolStripButtonForward.Click += new System.EventHandler(this.toolStripButtonVideoControl_Click);
            // 
            // toolStripButtonEnd
            // 
            this.toolStripButtonEnd.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonEnd.Image = global::VideoControl.Properties.Resources.black_end_32;
            this.toolStripButtonEnd.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.toolStripButtonEnd.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonEnd.Name = "toolStripButtonEnd";
            this.toolStripButtonEnd.Size = new System.Drawing.Size(36, 36);
            this.toolStripButtonEnd.Text = "End";
            this.toolStripButtonEnd.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.toolStripButtonEnd.ToolTipText = "Move To End";
            this.toolStripButtonEnd.Click += new System.EventHandler(this.toolStripButtonVideoControl_Click);
            // 
            // toolStripButtonRepeat
            // 
            this.toolStripButtonRepeat.CheckOnClick = true;
            this.toolStripButtonRepeat.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonRepeat.Image = global::VideoControl.Properties.Resources.black_repeat_32;
            this.toolStripButtonRepeat.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.toolStripButtonRepeat.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonRepeat.Name = "toolStripButtonRepeat";
            this.toolStripButtonRepeat.Size = new System.Drawing.Size(36, 36);
            this.toolStripButtonRepeat.Text = "Repeat";
            this.toolStripButtonRepeat.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.toolStripButtonRepeat.Click += new System.EventHandler(this.toolStripButtonVideoControl_Click);
            // 
            // VideoControl
            // 
            this.AutoSize = true;
            this.Controls.Add(this.toolStripProperty);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.statusStrip);
            this.Controls.Add(this.toolStripVideoControl);
            this.Name = "VideoControl";
            this.Size = new System.Drawing.Size(303, 120);
            this.toolStripVideoControl.ResumeLayout(false);
            this.toolStripVideoControl.PerformLayout();
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.toolStripProperty.ResumeLayout(false);
            this.toolStripProperty.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        public VideoControl()
        {
            InitializeComponent();

            m_tmPlay.Stop();
            m_tmPlay.Interval = 1000;
            m_tmPlay.Tick += new EventHandler(tm_Tick);
        }

        public UInt32 CurrentPosition
        {
            get { return m_uiCurrentPosition; }
            set
            {
                if ((value >= m_uiMinPosition) && (value <= m_uiMaxPosition)) m_uiCurrentPosition = value;

                UpdateVideoControlButtons();
            }
        }

        private void UpdateVideoControlButtons()
        {
            toolStripButtonStart.Enabled = (CurrentPosition > MinPosition) && (m_vmMode == VideoMode.vmPlay);
            toolStripButtonBack.Enabled = (CurrentPosition > MinPosition) && (m_vmMode == VideoMode.vmPlay);
            toolStripButtonEnd.Enabled = (CurrentPosition < MaxPosition) && (m_vmMode == VideoMode.vmPlay);
            toolStripButtonForward.Enabled = (CurrentPosition < MaxPosition) && (m_vmMode == VideoMode.vmPlay);

            toolStripButtonPlay.Enabled = (m_vmMode == VideoMode.vmPlay);
            toolStripButtonRepeat.Enabled = (m_vmMode == VideoMode.vmPlay);

            toolStripButtonRecord.Enabled = (m_vmMode != VideoMode.vmPlay);
            toolStripButtonStop.Enabled = (m_vmMode != VideoMode.vmNone);


            try
            {
                MeasureCameraBase.NewDataRecevedEventArgs pea = new MeasureCameraBase.NewDataRecevedEventArgs(m_arraySnapshot[CurrentPosition], false);

                OnChangePosition(this, pea);
            }
            catch { }
        }

        public UInt32 MinPosition
        {
            get { return m_uiMinPosition; }
            set
            {
                if (value < m_uiMaxPosition)
                {
                    m_uiMinPosition = value;
                    if (m_uiMinPosition > m_uiCurrentPosition) m_uiCurrentPosition = m_uiMinPosition;
                }
            }
        }

        public UInt32 MaxPosition
        {
            get { return m_uiMaxPosition; }
            set
            {
                if (value > m_uiMinPosition)
                {
                    m_uiMaxPosition = value;
                    if (m_uiCurrentPosition > m_uiMaxPosition) m_uiCurrentPosition = m_uiMaxPosition;
                }
            }
        }

        private void ChangeVideoControlState(VideoControlCommand m_vccComand, bool p)
        {
            switch (m_vccComand)
            {
                case VideoControlCommand.vccSkipToStart:
                    CurrentPosition = m_uiMinPosition;
                    break;
                case VideoControlCommand.vccRewind:
                    CurrentPosition--;
                    break;
                case VideoControlCommand.vccStop:
                    {
                        if (m_vmMode == VideoMode.vmPlay)
                            StopPlayData();
                        else
                            StopRecordSnapshotData();
                    }
                    break;
                case VideoControlCommand.vccRecord:
                    if (p == true)
                        toolStripButtonRecord.Checked = StartRecordData();
                    else
                        StopRecordSnapshotData();
                    break;
                case VideoControlCommand.vccPlay:
                    if (p == true)
                        StartMoveData();
                    else
                        StopMoveData();
                    break;
                case VideoControlCommand.vccForward:
                    CurrentPosition++;
                    break;
                case VideoControlCommand.vccEnd:
                    CurrentPosition = m_uiMaxPosition;
                    break;
                case VideoControlCommand.vccRepeat:
                    break;
                case VideoControlCommand.vccNone:
                    break;
            }

            ChangePositionAsyncDelegate asyncStopCollectData = new ChangePositionAsyncDelegate(ChangeCurrentPosition);
            asyncStopCollectData.BeginInvoke(m_vmMode, m_uiCurrentPosition, null, null);
        }

        private void StopMoveData()
        {
            m_mmMove = MoveMode.mmNone;
            m_tmPlay.Stop();
        }

        private void StartMoveData()
        {
            m_mmMove = MoveMode.mmPlay;
            m_tmPlay.Start();
        }

        void tm_Tick(object sender, EventArgs e)
        {
            if (m_vmMode == VideoMode.vmPlay)
            {
                ChangeVideoControlState(VideoControlCommand.vccForward, false);
                if ((m_mmMove == MoveMode.mmPlay) && (CurrentPosition == MaxPosition))
                {
                    toolStripButtonPlay.Checked = false;
                    StopMoveData();
                }
            }
        }

        private void toolStripButtonVideoControl_Click(object sender, EventArgs e)
        {
            ToolStripButton cb = (ToolStripButton)sender;

            if (m_vmMode == VideoMode.vmPlay)
            {
                if (cb.Name.Contains("Repeat") == true)
                    m_vccComand = VideoControlCommand.vccRepeat;
                else if (cb.Name.Contains("End") == true)
                    m_vccComand = VideoControlCommand.vccEnd;
                else if (cb.Name.Contains("Forward") == true)
                    m_vccComand = VideoControlCommand.vccForward;
                else if (cb.Name.Contains("Play") == true)
                    m_vccComand = VideoControlCommand.vccPlay;
                else if (cb.Name.Contains("Stop") == true)
                    m_vccComand = VideoControlCommand.vccStop;
                else if (cb.Name.Contains("Back") == true)
                    m_vccComand = VideoControlCommand.vccRewind;
                else if (cb.Name.Contains("Start") == true)
                    m_vccComand = VideoControlCommand.vccSkipToStart;
            }
            else
            {
                if (cb.Name.Contains("Record") == true)
                    m_vccComand = VideoControlCommand.vccRecord;
                else if (cb.Name.Contains("Stop") == true)
                    m_vccComand = VideoControlCommand.vccStop;
            }

            ChangeVideoControlState(m_vccComand, cb.Checked);
        }

        public void AddData(global::BeamOnCL.SnapshotBase globalBeamOnCLSnapshotBase)
        {
            try
            {
                if (m_listSapshot == null)
                {
                    m_listSapshot = new ArrayList();
                    m_lTimeStamp = 0;
                }
                else
                {
                    stopWatch.Stop();
                    m_lTimeStamp += stopWatch.ElapsedMilliseconds;
                }

                globalBeamOnCLSnapshotBase.TimeStamp = m_lTimeStamp;
                m_listSapshot.Add(globalBeamOnCLSnapshotBase);

                ChangePositionAsyncDelegate asyncStopCollectData = new ChangePositionAsyncDelegate(ChangeCurrentPosition);
                asyncStopCollectData.BeginInvoke(m_vmMode, (UInt32)m_listSapshot.Count - 1, null, null);

                Double dDuration = m_lTimeStamp / 1000f;

                if (((m_rtMode == RecordType.rtTime) && (dDuration >= m_uiRecordDuration)) ||
                    ((m_rtMode == RecordType.rtPoints) && (m_listSapshot.Count >= m_uiRecordNumPoints))) StopRecordSnapshotData();


                stopWatch = Stopwatch.StartNew();
                stopWatch.Start();
            }
            catch (OutOfMemoryException ee)
            {
                m_listSapshot.RemoveAt(m_listSapshot.Count - 1);
                StopRecordSnapshotData();
            }
        }

        public VideoMode Mode
        {
            get { return m_vmMode; }
        }

        public bool StartRecordData()
        {
            bool bRet = false;

            if ((strFileName == null) || (strFileName.Equals("") == true))
            {
                MessageBox.Show("Not specified the name and path for the Fast Mode data file.",
                                                        "Save Snapshot Video File",
                                                        MessageBoxButtons.OK,
                                                        MessageBoxIcon.Error);
                return bRet;
            }

            FileInfo fi = new FileInfo(strFileName);

            if (File.Exists(strFileName))
            {
                if (MessageBox.Show("This file '" + fi.Name + "' already exists. \nDo you want to replace it?",
                                    "Save Fast Mode data File",
                                    MessageBoxButtons.YesNo,
                                    MessageBoxIcon.Question) == DialogResult.No)
                    return bRet;
                else
                    File.Delete(strFileName);
            }

            toolStripButtonRecord.Checked = true;

            m_listSapshot = null;
            m_arraySnapshot = null;

            progressBar.Maximum = (m_rtMode == RecordType.rtTime) ? (int)RecordDuration : (int)RecordNumPoints;
            progressBar.Minimum = 0;

            m_vmMode = VideoMode.vmRecord;
            OnChangeVideoMode(this, new NewVideoModeEventArgs(m_vmMode));

            ChangePositionAsyncDelegate asyncStopCollectData = new ChangePositionAsyncDelegate(ChangeCurrentPosition);
            asyncStopCollectData.BeginInvoke(m_vmMode, m_uiCurrentPosition, null, null);

            bRet = true;

            return bRet;
        }

        public void StartPlayData()
        {
            if ((m_arraySnapshot != null) && (m_arraySnapshot.Length > 0))
            {
                m_vmMode = VideoMode.vmPlay;

                m_uiCurrentPosition = 0;
                m_uiMinPosition = 0;
                m_uiMaxPosition = (m_listSapshot.Count > 1) ? (UInt32)(m_listSapshot.Count - 1) : 0;

                ChangePositionAsyncDelegate asyncStopCollectData = new ChangePositionAsyncDelegate(ChangeCurrentPosition);
                asyncStopCollectData.BeginInvoke(m_vmMode, m_uiCurrentPosition, null, null);
            }
            else
                m_vmMode = VideoMode.vmNone;
        }

        private void StopPlayData()
        {
            m_vmMode = VideoMode.vmNone;
        }

        public void StopRecordSnapshotData()
        {
            if (m_vmMode == VideoMode.vmRecord)
            {
                m_uiCurrentPosition = 0;
                m_uiMinPosition = 0;
                m_uiMaxPosition = (m_listSapshot.Count > 1) ? (UInt32)(m_listSapshot.Count - 1) : 0;

                m_arraySnapshot = (SnapshotBase[])m_listSapshot.ToArray(typeof(SnapshotBase));

                ChangeRecordStatusAsyncDelegate asyncChangeRecordStatus = new ChangeRecordStatusAsyncDelegate(ChangeRecordStatus);
                asyncChangeRecordStatus.BeginInvoke(null, null);

                OnStopRecordData(this, new EventArgs());
            }
        }

        private void ChangeRecordStatus()
        {
            try
            {
                this.Invoke((MethodInvoker)delegate
                {
                    toolStripButtonRecord.Checked = false;
                });
            }
            catch
            {
            }
        }

        private void ChangeCurrentPosition(VideoMode vm, UInt32 pos)
        {
            try
            {
                this.Invoke((MethodInvoker)delegate
                {
                    UpdateVideoControlButtons();

                    if ((vm == VideoMode.vmPlay) && (m_listSapshot != null))
                    {
                        this.toolStripStatusLabelFrame.Text = pos.ToString() + " frame of " + m_uiMaxPosition.ToString() + " frames";
                        this.toolStripStatusLabelTime.Text = (m_arraySnapshot[pos].TimeStamp - m_arraySnapshot[0].TimeStamp).ToString() + " ms of " + (m_arraySnapshot[m_uiMaxPosition - 1].TimeStamp - m_arraySnapshot[0].TimeStamp).ToString() + "  ms";
                    }
                    else if ((vm == VideoMode.vmRecord) && (m_listSapshot != null))
                    {
                        try
                        {
                            this.progressBar.Value = (int)(((SnapshotBase)(m_listSapshot[(int)(m_listSapshot.Count - 1)])).TimeStamp / 1000f);
                        }
                        catch { }
                    }
                    else
                    {
                        this.toolStripStatusLabelFrame.Text = "-- frame of -- frames";
                        this.toolStripStatusLabelTime.Text = "-- ms of -- ms";
                    }
                });
            }
            catch
            {
            }
        }

        public RecordType Type
        {
            get { return m_rtMode; }
            set
            {
                m_rtMode = value;
            }
        }

        public uint RecordDuration
        {
            get { return m_uiRecordDuration; }
            set { m_uiRecordDuration = value; }
        }

        public uint RecordNumPoints
        {
            get { return m_uiRecordNumPoints; }
            set { m_uiRecordNumPoints = value; }
        }

        private void toolStripButtonProperty_Click(object sender, EventArgs e)
        {
            FormSetupRecord formSetupRecord = new FormSetupRecord(this);

            if (formSetupRecord.ShowDialog() == DialogResult.OK)
            {
            }
        }
    }
}
