using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace OpenGLControl
{
    public enum TypeGrid { Low, Midle, Hight };
    public enum TypeProjection { NoneProjection, YZProjection, XZProjection, XZ_YZProjection } ;
    public enum MeasureUnits { muMicro, muMiliRad };

    public partial class Image3D : OpenGLControl
    {
        #region Defines

        public class ChangeAngleEventArgs : EventArgs
        {
            private short m_sAngleX = 0;
            private short m_sAngleY = 0;

            public ChangeAngleEventArgs(short AngleX, short AngleY)
            {
                this.m_sAngleX = AngleX;
                this.m_sAngleY = AngleY;
            }

            public short AngleX
            {
                get { return m_sAngleX; }
            }

            public short AngleY
            {
                get { return m_sAngleY; }
            }
        }

        public delegate void ChangeAngle(object sender, ChangeAngleEventArgs e);

        public event ChangeAngle OnChangeAngle;

        public delegate void DoubleClick3D(object sender, EventArgs e);

        public event DoubleClick3D OnDoubleClick3D;

        Rectangle m_rectViewing = new Rectangle();

        private Color[] m_colorArray = null;

        private float m_CubeSize = 1.0f;
        private float m_dHalfWidth = 0.5f;
        private float m_dHalfHeight = 0.5f;
        private float m_dHalfPower = 0.5f;

        private MeasureUnits m_muUnits = MeasureUnits.muMicro;
        private Single m_sFocalLens = 1f;
        private Single m_sUnitsCoeff = 1f;
        private Single m_sOpticalFactor = 1f;
        private uint m_listCube = 0;
        private Boolean m_bCubeChange = false;
        protected uint m_listAxis = 0;
        Point pointSensorCenter = new Point();
        float m_iStepValueY;
        float m_iStepValueX;
        int m_iStepSizeY;
        int m_iStepSizeX;

        #endregion

        public Image3D()
            : base()
        {
            InitializeComponent();

            tmrRotate = new Timer();
            tmrRotate.Interval = 300;
            tmrRotate.Stop();
            tmrRotate.Tick += new EventHandler(tmrRotate_Tick);
            UpDateResolution();

            m_pal = new Color[256];

            for (int i = 0; i < 32; i++)
            {
                m_pal[i] = Color.FromArgb(128 - i * 2, 128 - i * 4, 192 + i);

                m_pal[i + 64] = Color.FromArgb(0, i * 4, 255);

                m_pal[i + 128] = Color.FromArgb(i * 4, 255, 255 - i * 8);

                m_pal[i + 192] = Color.FromArgb(255, 255 - i * 8, i * 4);
            }

            for (int i = 32; i < 64; i++)
            {
                m_pal[i] = Color.FromArgb(128 - i * 2, 0, 192 + i);

                m_pal[i + 64] = Color.FromArgb(0, i * 4, 255);

                m_pal[i + 128] = Color.FromArgb(i * 4, 255, 0);

                m_pal[i + 192] = Color.FromArgb(255, 0, i * 4);
            }
        }

        public Color[] colorArray
        {
            get { return m_colorArray; }

            set
            {
                if (value != null)
                {
                    if ((m_colorArray == null) || (value.Length != m_colorArray.Length)) m_colorArray = new System.Drawing.Color[value.Length];

                    value.CopyTo(m_colorArray, 0);

                    m_dz = 1 / (float)(colorArray.Length - 1);
                }
            }
        }

        public Point SensorCenterPosition
        {
            get { return pointSensorCenter; }
            set { pointSensorCenter = value; }
        }

        public void SetScaleGridX(short Step, float Value)
        {
            m_iStepValueX = Value;
            m_iStepSizeX = Step;
        }

        public void SetScaleGridY(short Step, float Value)
        {
            m_iStepValueY = Value;
            m_iStepSizeY = Step;
        }

        #region Overridables
        protected override void OnInitScene()
        {
            float[] pos0 = { -3.0f, 3.0f, -3.0f, 1.0f };
            float[] dir0 = { -1.0f, -1.0f, 1.0f };
            float[] pos1 = { 3.0f, -3.0f, 3.0f, 1.0f };
            float[] dir1 = { -1.0f, 1.0f, -1.0f };
            float[] pos2 = { 3.0f, 3.0f, 3.0f, 1.0f };
            float[] dir2 = { -1.0f, -1.0f, -1.0f };
            float[] pos3 = { 3.0f, 3.0f, 3.0f, 1.0f };
            float[] dir3 = { -1.0f, -1.0f, -1.0f };
            float[] mat_dif = { 0.8f, 0.8f, 0.8f };
            float[] mat_amb = { 0.2f, 0.2f, 0.2f };
            float[] mat_spec = { 0.6f, 0.6f, 0.6f };
            float shininess = (float)(0.7 * 128);
            float[] light_col = { 1.0f, 1.0f, 1.0f };

            float[] LightAmbient = { 0.5f, 0.5f, 0.5f, 1.0f };
            float[] LightDiffuse = { 1.0f, 1.0f, 1.0f, 1.0f };
            float[] LightPosition = { 0.0f, 0.0f, 2.0f, 1.0f };

            _camera.InitCamera(
                                new OpenGLControl.CVector4D(0, 0, -1),
                                new OpenGLControl.CVector4D(0, 1, 0),
                                new OpenGLControl.CVector4D(0, 0, -1)
                                );

            glEnable(GL_ALPHA_TEST);
            glEnable(GL_DEPTH_TEST);
            glEnable(GL_COLOR_MATERIAL);

            glShadeModel(GL_SMOOTH);
            glEnable(GL_BLEND);
            glBlendFunc(GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA);

            glMaterialfv(GL_FRONT_AND_BACK, GL_AMBIENT, mat_amb);
            glMaterialfv(GL_FRONT_AND_BACK, GL_DIFFUSE, mat_dif);
            glMaterialfv(GL_FRONT_AND_BACK, GL_SPECULAR, mat_spec);
            glMaterialf(GL_FRONT, GL_SHININESS, shininess);

            glEnable(GL_LINE_SMOOTH);
            glEnable(GL_POINT_SMOOTH);
            glHint(GL_POINT_SMOOTH_HINT, GL_NICEST);
            glHint(GL_LINE_SMOOTH_HINT, GL_NICEST);
            glHint(GL_POLYGON_SMOOTH_HINT, GL_NICEST);
            glHint(GL_PERSPECTIVE_CORRECTION_HINT, GL_NICEST);

            glClearDepth(1.0f);
            glEnable(GL_NORMALIZE);

            //glLightfv(GL_LIGHT0, GL_AMBIENT, LightAmbient);	// Setup The Ambient Light
            //glLightfv(GL_LIGHT0, GL_DIFFUSE, LightDiffuse);	// Setup The Diffuse Light
            //glLightfv(GL_LIGHT0, GL_POSITION, LightPosition);	// Position The Light
            //glEnable(GL_LIGHT0);										// Enable Light One
            //glEnable(GL_LIGHTING);
            ////glPolygonMode(GL_FRONT_AND_BACK, GL_FILL);
            ////glEnable(GL_LIGHTING);
            ////glEnable(GL_LIGHT1);
            ////glLightfv(GL_LIGHT1, GL_POSITION, pos0);
            ////glLightfv(GL_LIGHT1, GL_SPOT_DIRECTION, dir0);
            ////glLightfv(GL_LIGHT1, GL_DIFFUSE, light_col);
            ////glLightModeli(GL_LIGHT_MODEL_LOCAL_VIEWER, 1);
            //glLightModeli(GL_LIGHT_MODEL_TWO_SIDE, 1);

            //DrawCube();
            OnCreateGL();
        }
        #endregion

        #region Drawing
        float[][] v = new float[][] {
										 new float [] {-1, 1, -1},
										 new float [] {1,  1, -1},
										 new float [] {1, -1, -1},
										 new float [] {-1, -1, -1},
										 new float [] {-1, 1, 1},
										 new float [] {-1, -1, 1},
										 new float [] {1, -1, 1},
										 new float [] {1, 1, 1}
									 };

        double[][] norm = new double[][] {
											   new double [] {0,  0, -1},
											   new double [] {0,  0,  1},
											   new double [] {-1, 0,  0},
											   new double [] {1,  0,  0},
											   new double [] {0,  1,  0},
											   new double [] {0, -1,  0}
										   };

        uint[,] id = new uint[6, 4]{
			                            {0, 1, 2, 3},
			                            {4, 5, 6, 7},
			                            {0, 3, 5, 4},
			                            {7, 6, 2, 1},
			                            {0, 4, 7, 1},
			                            {5, 3, 2, 6},
		                            };


        private Point _ptCursorPos = new Point();
        private float _fTransX = 0, _fTransY = 0, _fTransZ = -6;

        private float _fHeadTilt = 0f;
        private float _fAngleX = 30f;
        private float _fAngleY = 30f;
        private float _fAngleZ = 30f;
        private Boolean m_bAutoRotateX = false;
        private Boolean m_bAutoRotateY = false;
        private Boolean m_bAutoRotateZ = false;
        private float m_fStepAutoRotateX = 5.0f;
        private float m_fStepAutoRotateY = 5.0f;
        private float m_fStepAutoRotateZ = 5.0f;
        private Boolean m_bDrawGrid = true;
        Timer tmrRotate = null;
        private TypeGrid m_tgResolution = TypeGrid.Low;
        private TypeProjection m_tpDraw3DProjection = TypeProjection.NoneProjection;
        private int m_wStepGrid = 8;
        private float m_dStepGrid;
        private float m_dz;
        private UInt16[] m_wpVerticalBorderMax = null;
        private UInt16[] m_wpVerticalBorderMin = null;
        private UInt16[] m_wpHorizontalBorderMax = null;
        private UInt16[] m_wpHorizontalBorderMin = null;
        Color[] m_pal = null;
        BeamOnCL.SnapshotBase m_bImageData = null;

        public Rectangle ViewingRect
        {
            get { return m_rectViewing; }

            set
            {
                m_rectViewing = value;
                UpDateResolution();
            }
        }

        public TypeProjection Projection
        {
            get { return m_tpDraw3DProjection; }
            set
            {
                m_tpDraw3DProjection = value;

                Invalidate();
            }
        }

        public TypeGrid Resolution
        {
            get { return m_tgResolution; }
            set
            {
                m_tgResolution = value;
                UpDateResolution();

                this.Invalidate();
            }
        }

        public BeamOnCL.SnapshotBase ImageData
        {
            get { return m_bImageData; }
            set
            {
                if (value != null)
                {
                    //lock (this)
                    {
                        m_bImageData = value;

                        //if ((m_rectViewing.Width == 0) || (m_rectViewing.Height == 0)) ViewingRect = new Rectangle(0, 0, m_bImageData.Width, m_bImageData.Height);

                        //m_dStepGrid = (float)(m_wStepGrid / (float)m_rectViewing.Width);
                        //m_dHalfHeight = (float)(m_rectViewing.Height * m_dStepGrid / (2.0 * m_wStepGrid));

                        m_dStepGrid = (float)(m_wStepGrid / (float)m_bImageData.Width);
                        m_dHalfHeight = (float)(m_bImageData.Height * m_dStepGrid / (2.0 * m_wStepGrid));

                        if (m_tpDraw3DProjection != TypeProjection.NoneProjection)
                        {
                            if ((m_wpVerticalBorderMin == null) || (m_wpVerticalBorderMin.Length != m_bImageData.Width))
                                m_wpVerticalBorderMin = new UInt16[m_bImageData.Width];

                            if ((m_wpVerticalBorderMax == null) || (m_wpVerticalBorderMax.Length != m_bImageData.Width))
                                m_wpVerticalBorderMax = new UInt16[m_bImageData.Width];

                            if ((m_wpHorizontalBorderMin == null) || (m_wpHorizontalBorderMin.Length != m_bImageData.Width))
                                m_wpHorizontalBorderMin = new UInt16[m_bImageData.Width];

                            if ((m_wpHorizontalBorderMax == null) || (m_wpHorizontalBorderMax.Length != m_bImageData.Width))
                                m_wpHorizontalBorderMax = new UInt16[m_bImageData.Width];

                            Build3DProjection(m_bImageData);
                        }

                        Invalidate();
                    }
                }
            }
        }

        public Boolean Grid
        {
            get { return m_bDrawGrid; }
            set
            {
                m_bDrawGrid = value;
                Invalidate();
            }
        }

        public float AngleX
        {
            get { return _fAngleX; }
            set
            {
                _fAngleX = value;
                Invalidate();
            }
        }

        public float AngleY
        {
            get { return _fAngleY; }
            set
            {
                _fAngleY = value;
                Invalidate();
            }
        }

        public float AngleZ
        {
            get { return _fAngleZ; }
            set
            {
                _fAngleZ = value;
                Invalidate();
            }
        }

        public float StepAutoRotateX
        {
            get { return m_fStepAutoRotateX; }
            set { m_fStepAutoRotateX = value; }
        }

        public float StepAutoRotateY
        {
            get { return m_fStepAutoRotateY; }
            set { m_fStepAutoRotateY = value; }
        }

        public float StepAutoRotateZ
        {
            get { return m_fStepAutoRotateZ; }
            set { m_fStepAutoRotateZ = value; }
        }

        public Boolean AutoRotateX
        {
            get { return m_bAutoRotateX; }
            set
            {
                m_bAutoRotateX = value;
                if (tmrRotate != null) tmrRotate.Enabled = m_bAutoRotateY || m_bAutoRotateX || m_bAutoRotateZ;
            }
        }

        public Boolean AutoRotateY
        {
            get { return m_bAutoRotateY; }
            set
            {
                m_bAutoRotateY = value;
                if (tmrRotate != null) tmrRotate.Enabled = m_bAutoRotateY || m_bAutoRotateX || m_bAutoRotateZ;
            }
        }

        public Boolean AutoRotateZ
        {
            get { return m_bAutoRotateZ; }
            set
            {
                m_bAutoRotateZ = value;
                if (tmrRotate != null) tmrRotate.Enabled = m_bAutoRotateY || m_bAutoRotateX || m_bAutoRotateZ;
            }
        }

        public MeasureUnits UnitMeasure
        {
            get { return m_muUnits; }
            set
            {
                if (m_muUnits != value)
                {
                    //DrawCube();
                    m_bCubeChange = true;
                    m_muUnits = value;
                    m_sUnitsCoeff = (m_muUnits == MeasureUnits.muMiliRad) ? 1f / (m_sFocalLens * m_sOpticalFactor) : 1f / m_sOpticalFactor;
                }
            }
        }

        public Single FocalLens
        {
            get { return m_sFocalLens; }
            set
            {
                m_sFocalLens = value;
                m_sUnitsCoeff = (m_muUnits == MeasureUnits.muMiliRad) ? 1f / (m_sFocalLens * m_sOpticalFactor) : 1f / m_sOpticalFactor;
            }
        }

        public Single OpticalFactor
        {
            get { return m_sOpticalFactor; }

            set
            {
                m_sOpticalFactor = value;
                m_sUnitsCoeff = (m_muUnits == MeasureUnits.muMiliRad) ? 1f / (m_sFocalLens * m_sOpticalFactor) : 1f / m_sOpticalFactor;
            }
        }

        private String GetValueStringFormat(double Value)
        {
            String strFormat = "";

            if (Math.Abs(Value) < 10)
                strFormat = "{0:F3}";
            else if (Math.Abs(Value) < 100)
                strFormat = "{0:F2}";
            else if (Math.Abs(Value) < 1000)
                strFormat = "{0:F1}";
            else
                strFormat = "{0:F0}";

            return strFormat;
        }

        private void UpDateResolution()
        {
            switch (m_tgResolution)
            {
                case TypeGrid.Low:
                    m_wStepGrid = 8;
                    break;
                case TypeGrid.Midle:
                    m_wStepGrid = 4;
                    break;
                case TypeGrid.Hight:
                    m_wStepGrid = 1;
                    break;
            }
        }

        void tmrRotate_Tick(object sender, EventArgs e)
        {
            if (m_bAutoRotateX)
            {
                float _fAngleXTmp = _fAngleX + m_fStepAutoRotateX;
                while (_fAngleXTmp >= 360) _fAngleXTmp -= 360;
                while (_fAngleXTmp < 0f) _fAngleXTmp += 360f;

                _fAngleX = _fAngleXTmp;

                OnChangeAngle(this, new ChangeAngleEventArgs((Int16)Math.Ceiling(_fAngleX), (Int16)Math.Ceiling(_fAngleY)));
            }

            if (m_bAutoRotateY)
            {
                float _fAngleYTmp = _fAngleY + m_fStepAutoRotateY;
                while (_fAngleYTmp >= 360) _fAngleYTmp -= 360;
                while (_fAngleYTmp < 0f) _fAngleYTmp += 360f;

                _fAngleY = _fAngleYTmp;

                OnChangeAngle(this, new ChangeAngleEventArgs((Int16)Math.Ceiling(_fAngleX), (Int16)Math.Ceiling(_fAngleY)));
            }

            if (m_bAutoRotateZ)
            {
                float _fAngleZTmp = _fAngleZ + m_fStepAutoRotateZ;
                while (_fAngleZTmp >= 360) _fAngleZTmp -= 360;
                while (_fAngleZTmp < 0f) _fAngleZTmp += 360f;

                _fAngleZ = _fAngleZTmp;
            }

            this.Invalidate();
        }

        protected virtual void DrawGrid()
        {
            int N, NG, SubN, i;
            float Half, U, SubU, PSize, YS, j;
            Byte[] Buf = new Byte[10];
            float[] X = new float[30];
            float[] Y = new float[30];
            String strData;

            if (ImageData != null)
            {
                PSize = (float)((ImageData.Width * 11) / 12f);

                //Half = PSize * (m_rectWorkingArea.Height / 2 - 8) / m_rectWorkingArea.Height;
                Half = PSize / 2;

                if (Half >= 10000)
                    U = 10000;
                else if (Half >= 1000)
                    U = 1000;
                else if (Half >= 100)
                    U = 100;
                else if (Half >= 10)
                    U = 10;
                else
                    U = 1;

                N = (int)Math.Floor(Half / U);
                if (N >= 3)
                    SubU = U;
                else if (N == 2)
                    SubU = U / 2f;
                else
                    SubU = U / 5f;

                NG = (int)Math.Floor(Half / SubU);

                YS = (float)(m_dHalfWidth + m_CubeSize / PSize * NG * SubU);

                for (i = -NG; i <= NG; i++)
                {
                    Y[i + NG] = (float)((m_dHalfWidth + m_CubeSize / PSize * i * SubU) - m_dHalfWidth);
                    X[i + NG] = (float)((m_dHalfWidth + m_CubeSize / PSize * i * SubU) - m_dHalfWidth);
                }

                if (N >= 5)
                    SubN = 5;
                else if (N >= 2)
                    SubN = 2;
                else
                    SubN = 1;

                // Draw string.
                strData = "0";

                glPushMatrix();
                glColor3ub(0, 144, 255);
                glTranslated(X[NG] + 0.07f * getPrintHeight(strData) / 2f, -m_dHalfWidth, m_dHalfWidth + 0.07f * getPrintWidth(strData));				// Center Our Text On The Screen
                glScalef(0.07f * m_CubeSize, 0.07f * m_CubeSize, 0.07f * m_CubeSize);
                glRotated(90, 0.0, 1.0, 0.0);
                glRotated(-90, 1.0, 0.0, 0.0);
                glPrint(strData);						// Print GL Text To The Screen
                glPopMatrix();

                glPushMatrix();
                glColor3ub(255, 0, 0);
                glTranslated(0.55f * m_CubeSize, -m_dHalfWidth, Y[NG] + 0.07f * getPrintHeight(strData) / 2f);	// Center Our Text On The Screen
                glScalef(0.07f * m_CubeSize, 0.07f * m_CubeSize, 0.07f * m_CubeSize);
                glRotated(-90, 1.0, 0.0, 0.0);
                glPrint(strData);						// Print GL Text To The Screen
                glPopMatrix();

                strData = String.Format(GetValueStringFormat(SubN * U * m_sUnitsCoeff), SubN * U * m_sUnitsCoeff);
                strData += ((m_muUnits == MeasureUnits.muMicro) ? " (µm)" : " (mrad)");

                glPushMatrix();
                glColor3ub(0, 144, 255);
                glTranslated(X[2 * NG] + 0.07f * getPrintHeight(strData) / 2f, -m_dHalfWidth, m_dHalfWidth + 0.07f * getPrintWidth(strData));				// Center Our Text On The Screen
                glScalef(0.07f * m_CubeSize, 0.07f * m_CubeSize, 0.07f * m_CubeSize);
                glRotated(90, 0.0, 1.0, 0.0);
                glRotated(-90, 1.0, 0.0, 0.0);
                glPrint(strData);						// Print GL Text To The Screen
                glPopMatrix();

                glPushMatrix();
                glColor3ub(0, 144, 255);
                glTranslated(X[0] + 0.07f * getPrintHeight(strData) / 2f, -m_dHalfWidth, m_dHalfWidth + 0.07f * getPrintWidth(strData));				// Center Our Text On The Screen
                glScalef(0.07f * m_CubeSize, 0.07f * m_CubeSize, 0.07f * m_CubeSize);
                glRotated(90, 0.0, 1.0, 0.0);
                glRotated(-90, 1.0, 0.0, 0.0);
                glPrint(strData);						// Print GL Text To The Screen
                glPopMatrix();

                glPushMatrix();
                glColor3ub(255, 0, 0);
                glTranslated(0.55f * m_CubeSize, -m_dHalfWidth, Y[2 * NG] + 0.07f * getPrintHeight(strData) / 2f);	// Center Our Text On The Screen
                glScalef(0.07f * m_CubeSize, 0.07f * m_CubeSize, 0.07f * m_CubeSize);
                glRotated(-90, 1.0, 0.0, 0.0);
                glPrint(strData);						// Print GL Text To The Screen
                glPopMatrix();

                glPushMatrix();
                glColor3ub(255, 0, 0);
                glTranslated(0.55f * m_CubeSize, -m_dHalfWidth, Y[0] + 0.07f * getPrintHeight(strData) / 2f);	// Center Our Text On The Screen
                glScalef(0.07f * m_CubeSize, 0.07f * m_CubeSize, 0.07f * m_CubeSize);
                glRotated(-90, 1.0, 0.0, 0.0);
                glPrint(strData);						// Print GL Text To The Screen
                glPopMatrix();


                for (i = 0; i < 2 * NG + 1; i++)
                {
                    for (j = Y[0]; j <= Y[2 * NG]; j += 3)
                    {
                        //grfx.DrawLine(PenGrid, X[i], j, X[i], j + 1);
                        glBegin(GL_LINES);
                        glColor3f(0.5f, 0.5f, 0.5f);
                        glVertex3d(X[i], -m_dHalfWidth, -m_dHalfWidth);
                        glVertex3d(X[i], -m_dHalfWidth, m_dHalfWidth);
                        glVertex3d(X[i], -m_dHalfWidth, -m_dHalfWidth);
                        glVertex3d(X[i], m_dHalfWidth, -m_dHalfWidth);
                        glEnd();
                    }

                    for (j = X[0]; j <= X[2 * NG]; j += 3)
                    {
                        //grfx.DrawLine(PenGrid, j, Y[i], j + 1, Y[i]);
                        glBegin(GL_LINES);
                        glColor3f(0.5f, 0.5f, 0.5f);
                        glVertex3d(m_dHalfWidth, -m_dHalfWidth, Y[i]);
                        glVertex3d(-m_dHalfWidth, -m_dHalfWidth, Y[i]);
                        glVertex3d(-m_dHalfWidth, m_dHalfWidth, Y[i]);
                        glVertex3d(-m_dHalfWidth, -m_dHalfWidth, Y[i]);
                        glEnd();
                    }
                }
            }
        }

        protected void OnCreateGL()
        {
            int i;
            String strData;

            try
            {
                if ((m_listCube != 0) && (glIsList(m_listCube) == GL_TRUE)) glDeleteLists(m_listCube, 1);
                m_listCube = glGenLists(1);

                glNewList(m_listCube, GL_COMPILE);

                glBegin(GL_LINES);
                // main X
                CVector3D pt = new CVector3D((float)(-m_CubeSize / 2.0), (float)(-m_CubeSize / 2.0), (float)(m_CubeSize / 2.0));
                pt.Transfer(new CVector3D(-0.1f * m_CubeSize, 0.0f, -m_CubeSize));
                //			glColor3ub(0, 0, 255);
                glColor3ub(0, 144, 255);
                glVertex3f(pt.fX, pt.fY, pt.fZ);
                pt.Transfer(new CVector3D(1.2f * m_CubeSize, 0.0f, 0.0f));
                glVertex3f(pt.fX, pt.fY, pt.fZ);
                // main Y
                pt = new CVector3D((float)(-m_CubeSize / 2.0), (float)(-m_CubeSize / 2.0), (float)(m_CubeSize / 2.0));
                pt.Transfer(new CVector3D(0.0f, -0.1f * m_CubeSize, -1.0f * m_CubeSize));
                glColor3ub(0, 255, 0);
                glVertex3f(pt.fX, pt.fY, pt.fZ);
                pt.Transfer(new CVector3D(0.0f, 1.2f * m_CubeSize, 0.0f));
                glVertex3f(pt.fX, pt.fY, pt.fZ);
                // main Z
                pt = new CVector3D((float)(-m_CubeSize / 2.0), (float)(-m_CubeSize / 2.0), (float)(m_CubeSize / 2.0));
                pt.Transfer(new CVector3D(0.0f, 0.0f, 0.1f * m_CubeSize));
                glColor3ub(255, 0, 0);
                glVertex3f(pt.fX, pt.fY, pt.fZ);
                pt.Transfer(new CVector3D(0.0f, 0.0f, -1.2f * m_CubeSize));
                glVertex3f(pt.fX, pt.fY, pt.fZ);
                glEnd();

                float x = m_CubeSize / 2;
                float fStep = (float)(m_CubeSize / 10.0);
                float fSmallStep = (float)(fStep / 4.0);
                glColor3f(0.5f, 0.5f, 0.5f);

                // panel Z
                glBegin(GL_LINE_LOOP);
                glVertex3d(-x, -x, x);
                glVertex3d(x, -x, x);
                glVertex3d(x, -x, -x);
                glVertex3d(-x, -x, -x);
                glEnd();

                // panel Y
                glBegin(GL_LINE_LOOP);
                glVertex3d(-x, -x, x);
                glVertex3d(-x, -x, -x);
                glVertex3d(-x, x, -x);
                glVertex3d(-x, x, x);
                glEnd();

                // panel X
                glBegin(GL_LINE_LOOP);
                glVertex3d(-x, -x, -x);
                glVertex3d(-x, x, -x);
                glVertex3d(x, x, -x);
                glVertex3d(x, -x, -x);
                glEnd();

                glBegin(GL_LINES);
                for (i = 0; i < 5; i++)
                {
                    glVertex3d(-x, x - 2 * fStep * i, x);
                    glVertex3d(-x, x - 2 * fStep * i, -x);

                    glVertex3d(-x, x - 2 * fStep * i - fStep, x);
                    glVertex3d(-x, x - 2 * fStep * i - fStep, x - fSmallStep);

                    glVertex3d(-x, x - 2 * fStep * i - fStep, -x);
                    glVertex3d(-x, x - 2 * fStep * i - fStep, -x + fSmallStep);

                    glVertex3d(-x, x - 2 * fStep * i, -x);
                    glVertex3d(x, x - 2 * fStep * i, -x);

                    glVertex3d(-x, x - 2 * fStep * i - fStep, -x);
                    glVertex3d(-x + fSmallStep, x - 2 * fStep * i - fStep, -x);

                    glVertex3d(x, x - 2 * fStep * i - fStep, -x);
                    glVertex3d(x - fSmallStep, x - 2 * fStep * i - fStep, -x);
                }

                glEnd();

                for (i = 1; i < 6; i++)
                {
                    strData = String.Format("{0:d}", i * 20);

                    glPushMatrix();
                    glColor3ub(0, 255, 0);
                    glTranslated(-m_dHalfWidth, -m_dHalfWidth + 2 * fStep * i, 0.65f * m_CubeSize);	// Center Our Text On The Screen
                    glScalef(0.07f * m_CubeSize, 0.07f * m_CubeSize, 0.07f * m_CubeSize);
                    glRotated(90, 0.0, 1.0, 0.0);
                    glPrint(strData);						// Print GL Text To The Screen
                    glPopMatrix();
                }

                // Cone
                glPolygonMode(GL_FRONT_AND_BACK, GL_FILL);
                float radius = 0.02f;

                GLUquadric pQuadric = gluNewQuadric();
                glPushMatrix();
                glTranslated(-m_CubeSize / 2.0, -m_CubeSize / 2.0, -m_CubeSize / 2.0);
                glColor3ub(255, 255, 255);
                gluQuadricDrawStyle(pQuadric, GLU_FILL);
                gluSphere(pQuadric, radius, 12, 12);
                glPopMatrix();

                glPushMatrix();
                glTranslated(m_CubeSize / 2.0, -m_CubeSize / 2.0, -m_CubeSize / 2.0);
                glRotated(90, 0.0, 1.0, 0.0);
                glColor3ub(0, 144, 255);
                //		glColor3ub(0,0,255);
                gluQuadricDrawStyle(pQuadric, GLU_FILL);
                gluCylinder(pQuadric, radius, 0, 0.11, 12, 12);
                glPopMatrix();

                glPushMatrix();
                glTranslated(-m_CubeSize / 2.0, m_CubeSize / 2.0, -m_CubeSize / 2.0);
                glRotated(270, 1.0, 0.0, 0.0);
                glColor3ub(0, 255, 0);
                gluQuadricDrawStyle(pQuadric, GLU_FILL);
                gluCylinder(pQuadric, radius, 0, 0.11, 12, 12);
                glPopMatrix();

                glPushMatrix();
                glTranslated(-m_CubeSize / 2.0, -m_CubeSize / 2.0, m_CubeSize / 2.0);
                glColor3ub(255, 0, 0);
                gluQuadricDrawStyle(pQuadric, GLU_FILL);
                gluCylinder(pQuadric, radius, 0, 0.11, 12, 12);
                glPopMatrix();

                glEndList();
            }
            catch
            {
            }
        }

        private void Build3DProjection(BeamOnCL.SnapshotBase snp)
        {
            UInt16 PointColor = 0;

            if (m_bImageData != null)
            {
                //lock (this)
                {
                    for (int i = 0; i < m_bImageData.Width; i++)
                    {
                        m_wpVerticalBorderMax[i] = 0;
                        m_wpVerticalBorderMin[i] = (UInt16)colorArray.Length;

                        m_wpHorizontalBorderMax[i] = 0;
                        m_wpHorizontalBorderMin[i] = (UInt16)colorArray.Length;
                    }

                    for (int i = ViewingRect.Top; i < ViewingRect.Bottom; i++)
                    {
                        for (int j = ViewingRect.Left, iShift = i * snp.Width; j < ViewingRect.Right; j++, iShift++)
                        {
                            PointColor = snp.GetPixelColor(iShift);

                            if (m_wpHorizontalBorderMax[j] < PointColor) m_wpHorizontalBorderMax[j] = PointColor;
                            if (m_wpHorizontalBorderMin[j] > PointColor) m_wpHorizontalBorderMin[j] = PointColor;

                            if (m_wpVerticalBorderMax[i] < PointColor) m_wpVerticalBorderMax[i] = PointColor;
                            if (m_wpVerticalBorderMin[i] > PointColor) m_wpVerticalBorderMin[i] = PointColor;
                        }
                    }
                }
            }
        }

        private void Draw3DProjection()
        {
            double x, y;
            int i, j;
            double dStepGrid = 1 / (float)ViewingRect.Width;

            //lock (this)
            {
                try
                {
                    glPushMatrix();

                    if ((m_tpDraw3DProjection == TypeProjection.YZProjection) || (m_tpDraw3DProjection == TypeProjection.XZ_YZProjection))
                    {
                        for (i = ViewingRect.Top, y = -m_dHalfHeight; (y < (m_dHalfHeight - dStepGrid)) && (i < ViewingRect.Bottom); y += dStepGrid, i++)
                        {
                            glBegin(GL_TRIANGLE_STRIP);

                            for (int k = m_wpVerticalBorderMin[i]; k < m_wpVerticalBorderMax[i]; k++)
                            {
                                glColor3ub(colorArray[k].R, colorArray[k].G, colorArray[k].B);
                                glVertex3d(y - dStepGrid, k * m_dz - m_dHalfWidth, -m_dHalfWidth);

                                glColor3ub(colorArray[k].R, colorArray[k].G, colorArray[k].B);
                                glVertex3d(y, k * m_dz - m_dHalfWidth, -m_dHalfWidth);
                            }

                            glEnd();
                        }
                    }

                    if ((m_tpDraw3DProjection == TypeProjection.XZProjection) || (m_tpDraw3DProjection == TypeProjection.XZ_YZProjection))
                    {
                        for (j = ViewingRect.Left, x = -m_dHalfWidth; ((x < (m_dHalfWidth - dStepGrid)) && (j < ViewingRect.Right)); x += dStepGrid, j++)
                        {
                            glBegin(GL_TRIANGLE_STRIP);

                            for (int k = m_wpHorizontalBorderMin[j]; k < m_wpHorizontalBorderMax[j]; k++)
                            {
                                glColor3ub(colorArray[k].R, colorArray[k].G, colorArray[k].B);
                                glVertex3d(-m_dHalfWidth, k * m_dz - m_dHalfWidth, x + dStepGrid);

                                glColor3ub(colorArray[k].R, colorArray[k].G, colorArray[k].B);
                                glVertex3d(-m_dHalfWidth, k * m_dz - m_dHalfWidth, x);
                            }

                            glEnd();
                        }
                    }

                    glPopMatrix();
                }
                catch
                {
                }
            }
        }

        #endregion

        #region Event handlers

        protected override void OnPaint(PaintEventArgs e)
        {
            //lock (this)
            {
                ActivateContext();

                glClearColor(
                                (float)BackColor.R / 255f,
                                (float)BackColor.G / 255f,
                                (float)BackColor.B / 255f,
                                (float)BackColor.A / 255f
                             );

                glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);

                glMatrixMode(GL_MODELVIEW);
                glLoadIdentity();
                _camera.ReInitCamera();
                glTranslated(_fTransX, _fTransY, _fTransZ);

                glRotated(_fAngleY, 1, 0, 0);
                glRotated(_fAngleX, 0, 1, 0);
                //glRotated(_fAngleZ, 0, 0, 1);

                if (m_bDrawGrid == true)
                {
                    if (glIsList(m_listCube) == GL_TRUE) glCallList(m_listCube);

                    OnScaleGL();
                }

                OnPlaneGL();

                if (m_tpDraw3DProjection != TypeProjection.NoneProjection) Draw3DProjection();

                SwapBuffers();

                DeactivateContext();
            }
            base.OnPaint(e);
        }

        protected virtual void OnScaleGL()
        {
            String strData;
            float NewVal;
            float NewPos;
            float fCentrPosX, fCentrPosY;
            float l_fTmpStepValueY;
            float l_fTmpStepValueX;
            float l_iTmpStepSizeY;
            float l_iTmpStepSizeX;
            //float dHalfHeight = m_pctpPrj->m_ViewingSize.cy / (float)(ImageData.Width * 2.0);//m_dHalfWidth;//

            if (ImageData != null)
            {
                // main X
                strData = "Y";
                strData += ((m_muUnits == MeasureUnits.muMicro) ? " (µm)" : " (mrad)");
                glPushMatrix();
                //		glColor3ub(0, 0, 255);
                glColor3ub(0, 144, 255);
                glTranslated((float)(0.63 * m_CubeSize), -m_dHalfWidth, -m_dHalfWidth);				// Center Our Text On The Screen
                glScalef(0.1f * m_CubeSize, 0.1f * m_CubeSize, 0.1f * m_CubeSize);
                glRotated(-90, 1.0, 0.0, 0.0);
                glPrint(strData);						// Print GL Text To The Screen
                glPopMatrix();

                // main Z
                strData = "X";
                strData += ((m_muUnits == MeasureUnits.muMicro) ? " (µm)" : " (mrad)");
                glPushMatrix();
                glColor3ub(255, 0, 0);
                glTranslated(-m_dHalfWidth, -m_dHalfWidth, 0.9f * m_CubeSize);				// Center Our Text On The Screen
                glScalef(0.1f * m_CubeSize, 0.1f * m_CubeSize, 0.1f * m_CubeSize);
                glRotated(90, 0.0, 1.0, 0.0);
                glRotated(-90, 1.0, 0.0, 0.0);
                glPrint(strData);						// Print GL Text To The Screen
                glPopMatrix();

                // main Y
                glPushMatrix();
                glColor3ub(0, 255, 0);
                glTranslated(-m_dHalfWidth, 0.63 * m_CubeSize, -m_dHalfWidth);				// Center Our Text On The Screen
                glScalef(0.1f * m_CubeSize, 0.1f * m_CubeSize, 0.1f * m_CubeSize);
                glRotated(45, 0.0, 1.0, 0.0);
                glPrint("P(%)");						// Print GL Text To The Screen
                glPopMatrix();

                fCentrPosX = (float)(SensorCenterPosition.X / (float)ImageData.Width - m_dHalfWidth);
                fCentrPosY = (float)(SensorCenterPosition.Y / (float)ImageData.Width - m_dHalfHeight);

                NewPos = fCentrPosY;
                NewVal = 0;
                l_fTmpStepValueY = m_iStepValueY;
                l_iTmpStepSizeY = m_iStepSizeY / (float)ImageData.Width;

                while (NewPos >= -m_dHalfWidth)
                {
                    strData = String.Format(GetValueStringFormat(NewVal), NewVal);

                    if (NewPos <= m_dHalfWidth)
                    {
                        glPushMatrix();
                        //				glColor3ub(0, 0, 255);
                        glColor3ub(0, 144, 255);
                        glTranslated(NewPos, -m_dHalfWidth, 0.7f * m_CubeSize);	// Center Our Text On The Screen
                        glScalef(0.07f * m_CubeSize, 0.07f * m_CubeSize, 0.07f * m_CubeSize);
                        glRotated(90, 0.0, 1.0, 0.0);
                        glRotated(-90, 1.0, 0.0, 0.0);
                        glPrint(strData);						// Print GL Text To The Screen
                        glPopMatrix();

                        glColor3f(0.5f, 0.5f, 0.5f);

                        glBegin(GL_LINES);
                        glVertex3d(NewPos, -m_dHalfWidth, -m_dHalfWidth);
                        glVertex3d(NewPos, -m_dHalfWidth, m_dHalfWidth);
                        glVertex3d(NewPos, -m_dHalfWidth, -m_dHalfWidth);
                        glVertex3d(NewPos, m_dHalfWidth, -m_dHalfWidth);
                        glEnd();
                    }

                    NewVal -= l_fTmpStepValueY;
                    NewPos -= l_iTmpStepSizeY;
                }

                NewPos = fCentrPosY + l_iTmpStepSizeY;
                NewVal = l_fTmpStepValueY;

                while (NewPos <= m_dHalfWidth)
                {
                    strData = String.Format(GetValueStringFormat(NewVal), NewVal);

                    if (NewPos >= -m_dHalfWidth)
                    {
                        glPushMatrix();
                        //				glColor3ub(0, 0, 255);
                        glColor3ub(0, 144, 255);
                        glTranslated(NewPos, -m_dHalfWidth, 0.7f * m_CubeSize);	// Center Our Text On The Screen
                        glScalef(0.07f * m_CubeSize, 0.07f * m_CubeSize, 0.07f * m_CubeSize);
                        glRotated(90, 0.0, 1.0, 0.0);
                        glRotated(-90, 1.0, 0.0, 0.0);
                        glPrint(strData);						// Print GL Text To The Screen
                        glPopMatrix();

                        glColor3f(0.5f, 0.5f, 0.5f);

                        glBegin(GL_LINES);
                        glVertex3d(NewPos, -m_dHalfWidth, -m_dHalfWidth);
                        glVertex3d(NewPos, -m_dHalfWidth, m_dHalfWidth);
                        glVertex3d(NewPos, -m_dHalfWidth, -m_dHalfWidth);
                        glVertex3d(NewPos, m_dHalfWidth, -m_dHalfWidth);
                        glEnd();
                    }

                    NewVal += l_fTmpStepValueY;
                    NewPos += l_iTmpStepSizeY;
                }

                NewPos = fCentrPosX;
                NewVal = 0;

                l_fTmpStepValueX = m_iStepValueX;
                l_iTmpStepSizeX = m_iStepSizeX / (float)ImageData.Width;

                while (NewPos <= m_dHalfWidth)
                {
                    strData = String.Format(GetValueStringFormat(NewVal), NewVal);

                    if (NewPos >= -m_dHalfWidth)
                    {
                        glPushMatrix();
                        glColor3ub(255, 0, 0);
                        glTranslated(0.55f * m_CubeSize, -m_dHalfWidth, NewPos);	// Center Our Text On The Screen
                        glScalef(0.07f * m_CubeSize, 0.07f * m_CubeSize, 0.07f * m_CubeSize);
                        glRotated(-90, 1.0, 0.0, 0.0);
                        glPrint(strData);						// Print GL Text To The Screen
                        glPopMatrix();

                        glColor3f(0.5f, 0.5f, 0.5f);

                        glBegin(GL_LINES);
                        glVertex3d(m_dHalfWidth, -m_dHalfWidth, NewPos);
                        glVertex3d(-m_dHalfWidth, -m_dHalfWidth, NewPos);

                        glVertex3d(-m_dHalfWidth, m_dHalfWidth, NewPos);
                        glVertex3d(-m_dHalfWidth, -m_dHalfWidth, NewPos);
                        glEnd();
                    }

                    NewVal += l_fTmpStepValueX;
                    NewPos += l_iTmpStepSizeX;
                }

                NewVal = -l_fTmpStepValueX;
                NewPos = fCentrPosX - l_iTmpStepSizeX;

                while (NewPos >= -m_dHalfWidth)
                {
                    strData = String.Format(GetValueStringFormat(NewVal), NewVal);

                    if (NewPos <= m_dHalfWidth)
                    {
                        glPushMatrix();
                        glColor3ub(255, 0, 0);
                        glTranslated(0.55f * m_CubeSize, -m_dHalfWidth, NewPos);	// Center Our Text On The Screen
                        glScalef(0.07f * m_CubeSize, 0.07f * m_CubeSize, 0.07f * m_CubeSize);
                        glRotated(-90, 1.0, 0.0, 0.0);
                        glPrint(strData);						// Print GL Text To The Screen
                        glPopMatrix();

                        glColor3f(0.5f, 0.5f, 0.5f);

                        glBegin(GL_LINES);
                        glVertex3d(m_dHalfWidth, -m_dHalfWidth, NewPos);
                        glVertex3d(-m_dHalfWidth, -m_dHalfWidth, NewPos);

                        glVertex3d(-m_dHalfWidth, m_dHalfWidth, NewPos);
                        glVertex3d(-m_dHalfWidth, -m_dHalfWidth, NewPos);
                        glEnd();
                    }

                    NewVal -= l_fTmpStepValueX;
                    NewPos -= l_iTmpStepSizeX;
                }
            }
        }

        //protected virtual void OnScaleGL()
        //{
        //    if (m_bCubeChange == true)
        //    {
        //        m_bCubeChange = false;
        //        DrawCube();
        //    }

        //    if (glIsList(m_listCube) == GL_TRUE) glCallList(m_listCube);
        //    DrawGrid();
        //}

        void OnPlaneGL()
        {
            int i, j;
            int iStart, iFinich;
            int jStart, jFinich;
            int zi;
            double x, y, z1, z2, z3, z4;
            Color l_rgb1, l_rgb2, l_rgb3, l_rgb4;
            UInt16 uicolorValue = 0;

            if (m_bImageData != null)
            {
                //lock (this)
                {
                    int dShiftY = (int)(m_wStepGrid * m_bImageData.Width);

                    //iStart = m_bImageData.Height - 1 - (m_rectViewing.Top + m_rectViewing.Height - m_wStepGrid);
                    //iFinich = m_bImageData.Height - 1 - m_rectViewing.Top;

                    //iFinich = (iFinich > m_bImageData.Height) ? m_bImageData.Height : iFinich;

                    //jStart = m_rectViewing.Left;
                    //jFinich = jStart + m_rectViewing.Width - m_wStepGrid;

                    iStart = m_bImageData.Height - 1 - (m_bImageData.Top + m_bImageData.Height - m_wStepGrid);
                    iFinich = m_bImageData.Height - 1 - m_bImageData.Top;

                    iFinich = (iFinich > m_bImageData.Height) ? m_bImageData.Height : iFinich;

                    jStart = m_bImageData.Left;
                    jFinich = jStart + m_bImageData.Width - m_wStepGrid;

                    glPolygonMode(GL_FRONT_AND_BACK, GL_FILL);

                    glPushMatrix();
                    try
                    {

                        for (i = iStart, y = -m_dHalfHeight; (y < (m_dHalfHeight - m_dStepGrid)) && (i < iFinich); y += m_dStepGrid, i += m_wStepGrid)
                        {
                            zi = jStart + i * m_bImageData.Width;
                            uicolorValue = m_bImageData.GetPixelColor(zi);
                            l_rgb1 = m_colorArray[uicolorValue];
                            z1 = uicolorValue * m_dz - m_dHalfPower;
                            if (z1 > m_dHalfPower) z1 = m_dHalfPower;

                            //				zi -= dShiftY;
                            zi += dShiftY;
                            uicolorValue = m_bImageData.GetPixelColor(zi);
                            l_rgb2 = m_colorArray[uicolorValue];
                            z2 = uicolorValue * m_dz - m_dHalfPower;
                            if (z2 > m_dHalfPower) z2 = m_dHalfPower;

                            for (j = jStart, x = -m_dHalfWidth; (x < (m_dHalfWidth - m_dStepGrid)) && (j < jFinich); x += m_dStepGrid, j += m_wStepGrid)
                            {
                                zi += m_wStepGrid;
                                uicolorValue = m_bImageData.GetPixelColor(zi);
                                l_rgb3 = m_colorArray[uicolorValue];
                                z3 = uicolorValue * m_dz - m_dHalfPower;
                                if (z3 > m_dHalfPower) z3 = m_dHalfPower;

                                //					zi += dShiftY;
                                zi -= dShiftY;
                                uicolorValue = m_bImageData.GetPixelColor(zi);
                                l_rgb4 = m_colorArray[uicolorValue];
                                z4 = uicolorValue * m_dz - m_dHalfPower;
                                if (z4 > m_dHalfPower) z4 = m_dHalfPower;

                                glBegin(GL_QUADS);

                                glColor3ub(l_rgb1.R, l_rgb1.G, l_rgb1.B);
                                glVertex3d(y, z1, x);

                                glColor3ub(l_rgb2.R, l_rgb2.G, l_rgb2.B);
                                glVertex3d(y + m_dStepGrid, z2, x);

                                glColor3ub(l_rgb3.R, l_rgb3.G, l_rgb3.B);
                                glVertex3d(y + m_dStepGrid, z3, x + m_dStepGrid);

                                glColor3ub(l_rgb4.R, l_rgb4.G, l_rgb4.B);
                                glVertex3d(y, z4, x + m_dStepGrid);

                                glEnd();

                                l_rgb1 = l_rgb4;
                                z1 = z4;

                                l_rgb2 = l_rgb3;
                                z2 = z3;

                                //					zi -= dShiftY;
                                zi += dShiftY;
                            }
                        }
                    }
                    catch
                    {
                    }

                    glPopMatrix();
                }
            }
        }

        // Mouse Down
        protected override void OnMouseDown(System.Windows.Forms.MouseEventArgs e)
        {
            base.OnMouseDown(e);

            _ptCursorPos.X = e.X;
            _ptCursorPos.Y = e.Y;

            //this.Invalidate();
        }

        // MouseMove
        protected override void OnMouseMove(System.Windows.Forms.MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (MouseButtons != MouseButtons.Left)
                return;
            try
            {
                float _fShiftDY = (float)(e.Y - _ptCursorPos.Y) / 4f;
                float _fShiftDX = (float)(e.X - _ptCursorPos.X) / 4f;

                float _fAngleXTmp = _fAngleX + _fShiftDX;
                float _fAngleYTmp = _fAngleY - _fShiftDY;

                while (_fAngleXTmp >= 360f) _fAngleXTmp -= 360f;
                while (_fAngleXTmp < 0f) _fAngleXTmp += 360f;

                while (_fAngleYTmp >= 360f) _fAngleYTmp -= 360f;
                while (_fAngleYTmp < 0f) _fAngleYTmp += 360f;

                _fAngleX = _fAngleXTmp;
                _fAngleY = _fAngleYTmp;
                OnChangeAngle(this, new ChangeAngleEventArgs((Int16)Math.Ceiling(_fAngleX), (Int16)Math.Ceiling(_fAngleY)));
            }
            catch
            {
            }
            finally
            {
                _ptCursorPos.X = e.X;
                _ptCursorPos.Y = e.Y;
            }

            this.Invalidate();
        }

        //MouseWheel
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);

            if (e.Delta > 0)
                _camera.MoveBackward();
            else if (e.Delta < 0)
                _camera.MoveForward();

            this.Invalidate();
        }

        protected override void OnDoubleClick(EventArgs e)
        {
            base.OnDoubleClick(e);

            OnDoubleClick3D(this, new EventArgs());

            //this.Invalidate();
        }

        #endregion
    }
}
