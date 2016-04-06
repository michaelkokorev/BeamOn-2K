using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Text;
using System.Collections;
using BeamOnCL;

namespace VideoControl
{
    public class VideoControl : UserControl
    {
        private ToolStrip toolStripVideoControl;
        private ToolStripButton toolStripButtonStart;
        private ToolStripButton toolStripButtonBack;
        private ToolStripButton toolStripButtonStop;
        private ToolStripButton toolStripButtonPlay;
        private ToolStripButton toolStripButtonForward;
        private ToolStripButton toolStripButtonEnd;
        private ToolStripButton toolStripButtonRepeat;

        public enum VideoControlCommand { vccNone, vccRepeat, vccEnd, vccForward, vccPlay, vccStop, vccRewind, vccSkipToStart };
        public enum VideoMode { vmRecord, vmPlay, vmNone };

        private VideoControlCommand m_vccComand = VideoControlCommand.vccNone;
        private VideoMode m_vmMode = VideoMode.vmNone;

        private UInt32 m_uiCurrentPosition = 0;
        private UInt32 m_uiMinPosition = 0;
        private UInt32 m_uiMaxPosition = 100;
        private StatusStrip statusStrip;
        private ToolStripStatusLabel toolStripStatusLabelFrame;
        private ToolStripStatusLabel toolStripStatusLabelTime;

        ArrayList m_listSapshot = null;
        SnapshotBase[] m_arraySnapshot = null;

        public delegate void ChangePosition(object sender, MeasureCameraBase.NewDataRecevedEventArgs e);
        public event ChangePosition OnChangePosition;

        public delegate void CloseFastModeData(object sender, EventArgs e);
        public event CloseFastModeData OnCloseFastModeData;

        private delegate void ChangePositionAsyncDelegate(UInt32 pos);

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.toolStripVideoControl = new System.Windows.Forms.ToolStrip();
            this.toolStripButtonStart = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonBack = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonStop = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonPlay = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonForward = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonEnd = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonRepeat = new System.Windows.Forms.ToolStripButton();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabelFrame = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabelTime = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripVideoControl.SuspendLayout();
            this.statusStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStripVideoControl
            // 
            this.toolStripVideoControl.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStripVideoControl.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButtonStart,
            this.toolStripButtonBack,
            this.toolStripButtonStop,
            this.toolStripButtonPlay,
            this.toolStripButtonForward,
            this.toolStripButtonEnd,
            this.toolStripButtonRepeat});
            this.toolStripVideoControl.Location = new System.Drawing.Point(0, 0);
            this.toolStripVideoControl.Name = "toolStripVideoControl";
            this.toolStripVideoControl.RenderMode = System.Windows.Forms.ToolStripRenderMode.Professional;
            this.toolStripVideoControl.Size = new System.Drawing.Size(283, 54);
            this.toolStripVideoControl.TabIndex = 9;
            this.toolStripVideoControl.Text = "toolStrip1";
            // 
            // toolStripButtonStart
            // 
            this.toolStripButtonStart.Image = global::VideoControl.Properties.Resources.black_skip_to_start_32;
            this.toolStripButtonStart.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.toolStripButtonStart.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonStart.Name = "toolStripButtonStart";
            this.toolStripButtonStart.Size = new System.Drawing.Size(36, 51);
            this.toolStripButtonStart.Text = "Start";
            this.toolStripButtonStart.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.toolStripButtonStart.ToolTipText = "Move To Start";
            this.toolStripButtonStart.Click += new System.EventHandler(this.toolStripButtonVideoControl_Click);
            // 
            // toolStripButtonBack
            // 
            this.toolStripButtonBack.Image = global::VideoControl.Properties.Resources.black_rewind_32;
            this.toolStripButtonBack.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.toolStripButtonBack.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonBack.Name = "toolStripButtonBack";
            this.toolStripButtonBack.Size = new System.Drawing.Size(36, 51);
            this.toolStripButtonBack.Text = "Back";
            this.toolStripButtonBack.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.toolStripButtonBack.ToolTipText = "Move To Back";
            this.toolStripButtonBack.Click += new System.EventHandler(this.toolStripButtonVideoControl_Click);
            // 
            // toolStripButtonStop
            // 
            this.toolStripButtonStop.Image = global::VideoControl.Properties.Resources.black_stop_32;
            this.toolStripButtonStop.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.toolStripButtonStop.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonStop.Name = "toolStripButtonStop";
            this.toolStripButtonStop.Size = new System.Drawing.Size(36, 51);
            this.toolStripButtonStop.Text = "Stop";
            this.toolStripButtonStop.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.toolStripButtonStop.Click += new System.EventHandler(this.toolStripButtonVideoControl_Click);
            // 
            // toolStripButtonPlay
            // 
            this.toolStripButtonPlay.Image = global::VideoControl.Properties.Resources.black_play_32;
            this.toolStripButtonPlay.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.toolStripButtonPlay.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonPlay.Name = "toolStripButtonPlay";
            this.toolStripButtonPlay.Size = new System.Drawing.Size(36, 51);
            this.toolStripButtonPlay.Text = "Play";
            this.toolStripButtonPlay.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.toolStripButtonPlay.Click += new System.EventHandler(this.toolStripButtonVideoControl_Click);
            // 
            // toolStripButtonForward
            // 
            this.toolStripButtonForward.Image = global::VideoControl.Properties.Resources.black_fast_forward_32;
            this.toolStripButtonForward.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.toolStripButtonForward.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonForward.Name = "toolStripButtonForward";
            this.toolStripButtonForward.Size = new System.Drawing.Size(54, 51);
            this.toolStripButtonForward.Text = "Forward";
            this.toolStripButtonForward.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.toolStripButtonForward.ToolTipText = "Move To Forward";
            this.toolStripButtonForward.Click += new System.EventHandler(this.toolStripButtonVideoControl_Click);
            // 
            // toolStripButtonEnd
            // 
            this.toolStripButtonEnd.Image = global::VideoControl.Properties.Resources.black_end_32;
            this.toolStripButtonEnd.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.toolStripButtonEnd.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonEnd.Name = "toolStripButtonEnd";
            this.toolStripButtonEnd.Size = new System.Drawing.Size(36, 51);
            this.toolStripButtonEnd.Text = "End";
            this.toolStripButtonEnd.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.toolStripButtonEnd.ToolTipText = "Move To End";
            this.toolStripButtonEnd.Click += new System.EventHandler(this.toolStripButtonVideoControl_Click);
            // 
            // toolStripButtonRepeat
            // 
            this.toolStripButtonRepeat.Image = global::VideoControl.Properties.Resources.black_repeat_32;
            this.toolStripButtonRepeat.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.toolStripButtonRepeat.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonRepeat.Name = "toolStripButtonRepeat";
            this.toolStripButtonRepeat.Size = new System.Drawing.Size(47, 51);
            this.toolStripButtonRepeat.Text = "Repeat";
            this.toolStripButtonRepeat.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.toolStripButtonRepeat.Click += new System.EventHandler(this.toolStripButtonVideoControl_Click);
            // 
            // statusStrip
            // 
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabelFrame,
            this.toolStripStatusLabelTime});
            this.statusStrip.Location = new System.Drawing.Point(0, 54);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(283, 22);
            this.statusStrip.TabIndex = 10;
            // 
            // toolStripStatusLabelFrame
            // 
            this.toolStripStatusLabelFrame.AutoSize = false;
            this.toolStripStatusLabelFrame.BorderStyle = System.Windows.Forms.Border3DStyle.RaisedOuter;
            this.toolStripStatusLabelFrame.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripStatusLabelFrame.Name = "toolStripStatusLabelFrame";
            this.toolStripStatusLabelFrame.Size = new System.Drawing.Size(150, 17);
            this.toolStripStatusLabelFrame.Text = "0 frame of 0 frames";
            // 
            // toolStripStatusLabelTime
            // 
            this.toolStripStatusLabelTime.AutoSize = false;
            this.toolStripStatusLabelTime.BorderStyle = System.Windows.Forms.Border3DStyle.RaisedOuter;
            this.toolStripStatusLabelTime.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripStatusLabelTime.Name = "toolStripStatusLabelTime";
            this.toolStripStatusLabelTime.Size = new System.Drawing.Size(118, 17);
            this.toolStripStatusLabelTime.Text = "0 ms of 0 ms";
            // 
            // VideoControl
            // 
            this.AutoSize = true;
            this.Controls.Add(this.statusStrip);
            this.Controls.Add(this.toolStripVideoControl);
            this.Name = "VideoControl";
            this.Size = new System.Drawing.Size(283, 76);
            this.toolStripVideoControl.ResumeLayout(false);
            this.toolStripVideoControl.PerformLayout();
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        public VideoControl()
        {
            InitializeComponent();
        }

        public UInt32 CurrentPosition
        {
            get { return m_uiCurrentPosition; }
            set
            {
                if ((value >= m_uiMinPosition) && (value <= m_uiMaxPosition))
                {
                    m_uiCurrentPosition = value;
                    UpdateVideoControlButtons();
                }
            }
        }

        private void UpdateVideoControlButtons()
        {
            toolStripButtonStart.Enabled = (CurrentPosition > MinPosition);
            toolStripButtonBack.Enabled = (CurrentPosition > MinPosition);
            toolStripButtonEnd.Enabled = (CurrentPosition < MaxPosition);
            toolStripButtonForward.Enabled = (CurrentPosition < MaxPosition);

            toolStripButtonPlay.Enabled = (CurrentPosition < MaxPosition);

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
            if (m_vmMode == VideoMode.vmPlay)
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
                        break;
                    case VideoControlCommand.vccPlay:
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
            }

            ChangePositionAsyncDelegate asyncStopCollectData = new ChangePositionAsyncDelegate(ChangeCurrentPosition);
            asyncStopCollectData.BeginInvoke(m_uiCurrentPosition, null, null);
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

            ChangeVideoControlState(m_vccComand, cb.Checked);
        }

        public void AddData(global::BeamOnCL.SnapshotBase globalBeamOnCLSnapshotBase)
        {
            if (m_vmMode == VideoMode.vmRecord)
            {
                try
                {
                    if (m_listSapshot == null) m_listSapshot = new ArrayList();
                    m_listSapshot.Add(globalBeamOnCLSnapshotBase);
                }
                catch (OutOfMemoryException ee)
                {
                    m_listSapshot.RemoveAt(m_listSapshot.Count - 1);
                    StopRecordData();
                    OnCloseFastModeData(this, new EventArgs());
                }
            }
        }

        public VideoMode Mode
        {
            get { return m_vmMode; }
        }

        public void StartRecordData()
        {
            m_listSapshot = null;
            m_arraySnapshot = null;

            m_vmMode = VideoMode.vmRecord;

            ChangePositionAsyncDelegate asyncStopCollectData = new ChangePositionAsyncDelegate(ChangeCurrentPosition);
            asyncStopCollectData.BeginInvoke(m_uiCurrentPosition, null, null);
        }

        public void StartPlayData()
        {
            if ((m_arraySnapshot != null) && (m_arraySnapshot.Length > 0))
            {
                m_vmMode = VideoMode.vmPlay;

                ChangePositionAsyncDelegate asyncStopCollectData = new ChangePositionAsyncDelegate(ChangeCurrentPosition);
                asyncStopCollectData.BeginInvoke(m_uiCurrentPosition, null, null);
            }
            else
                m_vmMode = VideoMode.vmNone;
        }

        public void StopRecordData()
        {
            if (m_vmMode == VideoMode.vmRecord)
            {
                m_uiCurrentPosition = 0;
                m_uiMinPosition = 0;
                m_uiMaxPosition = (m_listSapshot.Count > 1) ? (UInt32)(m_listSapshot.Count - 1) : 0;

                m_arraySnapshot = (SnapshotBase[])m_listSapshot.ToArray(typeof(SnapshotBase));
            }
        }

        private void ChangeCurrentPosition(UInt32 pos)
        {
            try
            {
                this.Invoke((MethodInvoker)delegate
                {
                    if ((m_vmMode == VideoMode.vmPlay) && (m_listSapshot != null))
                    {
                        this.toolStripStatusLabelFrame.Text = pos.ToString() + " frame of " + m_uiMaxPosition.ToString() + " frames";
                        this.toolStripStatusLabelTime.Text = (m_arraySnapshot[pos].TimeStamp - m_arraySnapshot[0].TimeStamp).ToString() + " ms of " + (m_arraySnapshot[m_uiMaxPosition].TimeStamp - m_arraySnapshot[0].TimeStamp).ToString() + "  ms";
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
    }
}
