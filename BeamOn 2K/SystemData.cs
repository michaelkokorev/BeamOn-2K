using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;
using System.Windows.Forms;
using System.Globalization;
using System.Drawing;
using System.Runtime.Serialization;
using System.Drawing.Imaging;
using System.Xml;
using System.Collections;

namespace BeamOn_2K
{
    public enum FileType { ftLog, ftExcel, ftXML };
    public enum LogType { ltTime, ltPoints, ltManual };

    public sealed class SystemData
    {
        [DataContract(Name = "Setup")]
        private class Data
        {
            public Boolean bSimulation = false;
            public Boolean fSnapshotView = false;
            [DataMember(Name = "ScaleProfile")]
            public Boolean m_bScaleProfile = false;
            [DataMember(Name = "Gaussian")]
            public Boolean m_bGaussian = false;
            [DataMember(Name = "Measure")]
            public Boolean m_bMeasure = false;
            [DataMember(Name = "ProfileDataHorizontal")]
            public ProfileData m_profDataHorizontal = new ProfileData();
            [DataMember(Name = "ProfileDataVertical")]
            public ProfileData m_profDataVertical = new ProfileData();
            [DataMember(Name = "PositionData")]
            public PositionData m_positionData = new PositionData();
            [DataMember(Name = "ProjectionData")]
            public ProjectionData m_projectionData = new ProjectionData();
            [DataMember(Name = "Levels")]
            public LevelsData m_ldLevels = new LevelsData();
            [DataMember(Name = "LogData")]
            public LogData m_logData = new LogData();
            [DataMember(Name = "PowerData")]
            public PowerData m_powerData = new PowerData();
            [DataMember(Name = "ApplicationData")]
            public ApplicationData m_applicationData = new ApplicationData();
            [DataMember(Name = "VideoDeviceData")]
            public VideoDevice m_videoDevice = new VideoDevice();
            public Boolean m_bDemo = false;
            public Boolean m_bDebug = false;
            public Boolean m_bCalibr = false;
            [DataMember(Name = "Average")]
            public UInt16 m_iAverage = 2;
            [DataMember(Name = "AverageOn")]
            public Boolean m_bAverageOn = false;
            public Single m_sFocalLens = 1f;
            [DataMember(Name = "UnitsCoeff")]
            public Single m_sUnitsCoeff = 1f;
            [DataMember(Name = "OpticalFactor")]
            public Single m_sOpticalFactor = 1f;
            [DataMember(Name = "Units")]
            public MeasureUnits m_muUnits = MeasureUnits.muMicro;
            [DataMember(Name = "TypeProfile")]
            public BeamOnCL.BeamOnCL.TypeProfile m_tpProfile = BeamOnCL.BeamOnCL.TypeProfile.tpSum;
            [DataMember(Name = "LineProfileAngle")]
            public Single lineProfileAngle = 0f;
            [DataMember(Name = "TypeLineProfile")]
            public BeamOnCL.BeamOnCL.TypeLineProfile m_tlpLine = BeamOnCL.BeamOnCL.TypeLineProfile.tpLineCentroid;
            [DataMember(Name = "PixelSize")]
            public Single m_fPixelSize = 5.86f;

            public Data()
            {
                InitializeComponent();
            }

            public void InitializeComponent()
            {
                m_profDataHorizontal.InitializeComponent();
                m_profDataVertical.InitializeComponent();
                m_positionData.InitializeComponent();
                m_ldLevels.InitializeComponent();
                m_logData.InitializeComponent();
                m_powerData.InitializeComponent();
                m_applicationData.InitializeComponent();
                m_projectionData.InitializeComponent();
                m_videoDevice.InitializeComponent();

                m_profDataHorizontal.PixelSize = m_fPixelSize;
                m_profDataVertical.PixelSize = m_fPixelSize;
                m_positionData.PixelSize = m_fPixelSize;

                string strProgramDataPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
                string strProgramFilePath = Environment.GetFolderPath(System.Environment.SpecialFolder.ProgramFiles);
                string strPath = Directory.GetCurrentDirectory().Substring(strProgramFilePath.Length + 1);

                string strNewPath;
                string strCompanyName = "";

                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCompanyAttribute), false);

                if (attributes.Length != 0)
                {
                    strCompanyName = ((AssemblyCompanyAttribute)attributes[0]).Company;
                    int pos = strCompanyName.IndexOf("Ltd");
                    if (pos > -1) strCompanyName = strCompanyName.Substring(0, pos);

                    strNewPath = Path.Combine(strProgramDataPath, strCompanyName, strPath);
                }
                else
                    strNewPath = Path.Combine(strProgramDataPath, strPath);

                m_applicationData.m_strMyAppDir = Application.StartupPath;

                m_applicationData.m_strMyCurrentDir = strNewPath;

                m_applicationData.m_strMyDataDir = strNewPath + "\\Data";
                if (Directory.Exists(m_applicationData.m_strMyDataDir) == false) Directory.CreateDirectory(m_applicationData.m_strMyDataDir);

                m_applicationData.m_strMyTempDir = strNewPath + "\\Temp";
                if (Directory.Exists(m_applicationData.m_strMyTempDir) == false) Directory.CreateDirectory(m_applicationData.m_strMyTempDir);

                bSimulation = false;
                fSnapshotView = false;

                FocalLens = m_sFocalLens;
                OpticalFactor = m_sOpticalFactor;
            }

            public Single OpticalFactor
            {
                get { return m_sOpticalFactor; }

                set
                {
                    m_sOpticalFactor = value;
                    m_positionData.OpticalFactor = m_sOpticalFactor;
                    //                projectionData.OpticalFactor = m_sOpticalFactor;

                    m_profDataHorizontal.OpticalFactor = m_sOpticalFactor;
                    m_profDataVertical.OpticalFactor = m_sOpticalFactor;

                    m_sUnitsCoeff = (m_muUnits == MeasureUnits.muMiliRad) ? 1f / (m_sFocalLens * m_sOpticalFactor) : 1f / m_sOpticalFactor;
                }
            }

            public MeasureUnits UnitMeasure
            {
                get { return m_muUnits; }
                set
                {
                    m_muUnits = value;
                    m_positionData.UnitMeasure = m_muUnits;
                    //                projectionData.UnitMeasure = m_muUnits;

                    m_profDataHorizontal.UnitMeasure = m_muUnits;
                    m_profDataVertical.UnitMeasure = m_muUnits;

                    m_sUnitsCoeff = (m_muUnits == MeasureUnits.muMiliRad) ? 1f / (m_sFocalLens * m_sOpticalFactor) : 1f / m_sOpticalFactor;
                }
            }

            public Single FocalLens
            {
                get { return m_sFocalLens; }
                set
                {
                    m_sFocalLens = value;
                    UnitMeasure = m_muUnits;

                    m_positionData.FocalLens = m_sFocalLens;
                    //projectionData.FocalLens = m_sFocalLens;

                    m_profDataHorizontal.FocalLens = m_sFocalLens;
                    m_profDataVertical.FocalLens = m_sFocalLens;
                }
            }
        }

        public Single PixelSize
        {
            get { return m_data.m_fPixelSize; }
            set { m_data.m_fPixelSize = value; }
        }

        public BeamOnCL.BeamOnCL.TypeLineProfile LineProfileType
        {
            get { return m_data.m_tlpLine; }
            set { m_data.m_tlpLine = value; }
        }

        public BeamOnCL.BeamOnCL.TypeProfile ProfileType
        {
            get { return m_data.m_tpProfile; }
            set { m_data.m_tpProfile = value; }
        }

        public Single lineProfileAngle
        {
            get { return m_data.lineProfileAngle; }
            set { m_data.lineProfileAngle = value; }
        }

        public Boolean ViewGaussian
        {
            get { return m_data.m_bGaussian; }
            set { m_data.m_bGaussian = value; }
        }

        public Boolean Simulation
        {
            get { return m_data.bSimulation; }
            set { m_data.bSimulation = value; }
        }

        public Boolean SnapshotView
        {
            get { return m_data.fSnapshotView; }
            set { m_data.fSnapshotView = value; }
        }

        public Boolean ScaleProfile
        {
            get { return m_data.m_bScaleProfile; }
            set { m_data.m_bScaleProfile = value; }
        }

        public Boolean Measure
        {
            get { return m_data.m_bMeasure; }
            set { m_data.m_bMeasure = value; }
        }

        public Boolean Calibr
        {
            get { return m_data.m_bCalibr; }
            set { m_data.m_bCalibr = value; }
        }

        public Boolean Debug
        {
            get { return m_data.m_bDebug; }
            set { m_data.m_bDebug = value; }
        }

        public Boolean AverageOn
        {
            get { return m_data.m_bAverageOn; }
            set { m_data.m_bAverageOn = value; }
        }

        public UInt16 Average
        {
            get { return m_data.m_iAverage; }
            set { m_data.m_iAverage = value; }
        }

        public Boolean Demo
        {
            get { return m_data.m_bDemo; }
            set { m_data.m_bDemo = value; }
        }

        public ProfileData HorizontalProfile
        {
            get { return m_data.m_profDataHorizontal; }
        }

        public ProfileData VerticalProfile
        {
            get { return m_data.m_profDataVertical; }
        }

        public ProjectionData projectionData
        {
            get { return m_data.m_projectionData; }
        }

        public PositionData positionData
        {
            get { return m_data.m_positionData; }
        }

        public LevelsData ClipLevels
        {
            get { return m_data.m_ldLevels; }
        }

        public PowerData powerData
        {
            get { return m_data.m_powerData; }
        }

        public ApplicationData applicationData
        {
            get { return m_data.m_applicationData; }
        }

        public VideoDevice videoDeviceData
        {
            get { return m_data.m_videoDevice; }
        }

        public LogData logData
        {
            get { return m_data.m_logData; }
        }

        private Data m_data = null;

        private static readonly SystemData myInstance = new SystemData();

        private SystemData()
        {
            m_data = new Data();
        }

        public static SystemData MyInstance
        {
            get { return myInstance; }
        }

        public void SerializeAppData(String strPath)
        {
            var ds = new DataContractSerializer(typeof(Data));
            using (Stream s = File.Create(strPath))
                ds.WriteObject(s, m_data); // Сериализация
        }

        public void DeserializeAppData(String strPath)
        {
            if (File.Exists(strPath) == true)
            {
                var ds = new DataContractSerializer(typeof(Data));
                using (Stream s = File.OpenRead(strPath))
                    m_data = (Data)ds.ReadObject(s); // Десериализация
            }

            m_data.InitializeComponent();
        }

        public Single UnitsCoeff
        {
            get { return m_data.m_sUnitsCoeff; }
        }

        public MeasureUnits UnitMeasure
        {
            get { return m_data.UnitMeasure; }
            set { m_data.UnitMeasure = value; }
        }

        public Single FocalLens
        {
            get { return m_data.FocalLens; }
            set { m_data.FocalLens = value; }
        }
    }

    [DataContract(Name = "VideoDevice")]
    public class VideoDevice
    {
        [DataMember(Name = "Video")]
        public UInt16 uiBinning = 1;
        [DataMember(Name = "PixelFormat")]
        public PixelFormat pixelFormat = PixelFormat.Format24bppRgb;

        public void InitializeComponent()
        {
        }

        public VideoDevice()
        {
            InitializeComponent();
        }
    }

    [DataContract(Name = "Setup")]
    public class ApplicationData
    {
        [DataMember(Name = "ToolBar")]
        public Boolean bViewToolbar = true;

        [DataMember(Name = "StatusBar")]
        public Boolean bViewStatusBar = true;

        [DataMember(Name = "SaveExit")]
        public Boolean bSaveExit = true;

        [DataMember(Name = "DataPanel")]
        public bool bViewDataPanel;

        public String m_strMyDataDir;
        public String m_strMySADataDir;
        public String m_strMyCurrentDir;
        public String m_strMyTempDir;
        public String m_strMyAppDir;
        public String m_strUserTitle;
        public String m_strTitle;

        public String SystemNumber;
        public String ProductName;
        public String ProductTrademark;
        public String ProductVersion;
        public String Copyright;
        public String CompanyName;

        String m_strHelpNamespace;

        #region Assembly Attribute Accessors

        static string AssemblyTitle
        {
            get
            {
                // Get all Title attributes on this assembly
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
                // If there is at least one Title attribute
                if (attributes.Length > 0)
                {
                    // Select the first one
                    AssemblyTitleAttribute titleAttribute = (AssemblyTitleAttribute)attributes[0];
                    // If it is not an empty string, return it
                    if (titleAttribute.Title != "")
                        return titleAttribute.Title;
                }
                // If there was no Title attribute, or if the Title attribute was the empty string, return the .exe name
                return System.IO.Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().CodeBase);
            }
        }

        static string AssemblyVersion
        {
            get
            {
                return Assembly.GetExecutingAssembly().GetName().Version.ToString();
            }
        }

        static string AssemblyDescription
        {
            get
            {
                // Get all Description attributes on this assembly
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false);
                // If there aren't any Description attributes, return an empty string
                if (attributes.Length == 0)
                    return "";
                // If there is a Description attribute, return its value
                return ((AssemblyDescriptionAttribute)attributes[0]).Description;
            }
        }

        static string AssemblyProduct
        {
            get
            {
                // Get all Product attributes on this assembly
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyProductAttribute), false);
                // If there aren't any Product attributes, return an empty string
                if (attributes.Length == 0)
                    return "";
                // If there is a Product attribute, return its value
                return ((AssemblyProductAttribute)attributes[0]).Product;
            }
        }

        static string AssemblyCopyright
        {
            get
            {
                // Get all Copyright attributes on this assembly
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
                // If there aren't any Copyright attributes, return an empty string
                if (attributes.Length == 0)
                    return "";
                // If there is a Copyright attribute, return its value
                return ((AssemblyCopyrightAttribute)attributes[0]).Copyright;
            }
        }

        static string AssemblyCompany
        {
            get
            {
                // Get all Company attributes on this assembly
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCompanyAttribute), false);
                // If there aren't any Company attributes, return an empty string
                if (attributes.Length == 0)
                    return "";
                // If there is a Company attribute, return its value
                return ((AssemblyCompanyAttribute)attributes[0]).Company;
            }
        }

        static string AssemblyTrademark
        {
            get
            {
                // Get all Title attributes on this assembly
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyTrademarkAttribute), false);
                // If there is at least one Title attribute
                if (attributes.Length > 0)
                {
                    // Select the first one
                    AssemblyTrademarkAttribute titleAttribute = (AssemblyTrademarkAttribute)attributes[0];
                    // If it is not an empty string, return it
                    if (titleAttribute.Trademark != "")
                        return titleAttribute.Trademark;
                }
                // If there was no Title attribute, or if the Title attribute was the empty string, return the .exe name
                return System.IO.Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().CodeBase);
            }
        }
        #endregion

        public void InitializeComponent()
        {
            ProductName = AssemblyProduct;
            ProductVersion = AssemblyVersion;
            Copyright = AssemblyCopyright;
            CompanyName = AssemblyCompany;
            ProductTrademark = AssemblyTrademark;
        }

        public String HelpNamespace
        {
            get { return m_strHelpNamespace; }
        }

        public ApplicationData()
        {
            InitializeComponent();
        }
    }

    [DataContract]
    public class LogData
    {
        //Status
        public Boolean bStart = false;

        //File
        [DataMember]
        public FileType ftFile = FileType.ftLog;
        public String strFileName;

        //Data
        [DataMember]
        public LogType ltMode = LogType.ltTime;
        [DataMember]
        public UInt32 LogInterval = 0;
        [DataMember]
        public UInt32 LogDuration = 5;
        [DataMember]
        public UInt32 LogNumPoints = 1;

        //Last Time
        public long LastMeasureTime = 0;

        //Data
        [DataMember]
        public Boolean bPower = true;
        [DataMember]
        public Boolean bPositionX = true;
        [DataMember]
        public Boolean bPositionY = true;
        [DataMember]
        public Boolean bHorizontalProfileWidthLevel1 = true;
        [DataMember]
        public Boolean bHorizontalProfileWidthLevel2 = true;
        [DataMember]
        public Boolean bHorizontalProfileWidthLevel3 = true;
        [DataMember]
        public Boolean bVerticalProfileWidthLevel1 = true;
        [DataMember]
        public Boolean bVerticalProfileWidthLevel2 = true;
        [DataMember]
        public Boolean bVerticalProfileWidthLevel3 = true;
        [DataMember]
        public Boolean bVerticalGaussianFit = false;
        [DataMember]
        public Boolean bHorizontalGaussianFit = false;
        [DataMember]
        public Boolean bHorizontalGaussianWidthLevel1 = false;
        [DataMember]
        public Boolean bHorizontalGaussianWidthLevel2 = false;
        [DataMember]
        public Boolean bHorizontalGaussianWidthLevel3 = false;
        [DataMember]
        public Boolean bVerticalGaussianWidthLevel1 = false;
        [DataMember]
        public Boolean bVerticalGaussianWidthLevel2 = false;
        [DataMember]
        public Boolean bVerticalGaussianWidthLevel3 = false;

        [DataMember]
        public Boolean bMajor = false;
        [DataMember]
        public Boolean bMinor = false;
        [DataMember]
        public Boolean bOrientation = false;

        public void InitializeComponent()
        {
        }
    }

    public struct FilterData
    {
        public UInt16 Wavelength;
        public Single Sensitivity;
    }

    [DataContract(Name = "PowerCalibration")]
    public class PowerCalibration
    {
        [DataMember(Name = "PowerCalibrationExposure")]
        private int m_iPowerCalibrationExposure = 1;
        [DataMember(Name = "PowerCalibrationGain")]
        private int m_iPowerCalibrationGain = 1;

        public void InitializePowerCalibration(int iExposure, int iGain)
        {
            m_iPowerCalibrationExposure = iExposure;
            m_iPowerCalibrationGain = iGain;
        }

        public double PowerCoeff(int iExposure, int iGain)
        {
            return (iExposure / (float)m_iPowerCalibrationExposure) * ((iGain + 1) / (float)(m_iPowerCalibrationGain + 1));
        }

        public int PowerCalibrationExposure
        {
            get { return m_iPowerCalibrationExposure; }
            set { m_iPowerCalibrationExposure = value; }
        }

        public int PowerCalibrationGain
        {
            get { return m_iPowerCalibrationGain; }
            set { m_iPowerCalibrationGain = value; }
        }
    }

    [DataContract(Name = "Power")]
    public class PowerData
    {
        [DataMember(Name = "PowerCalibrationValue")]
        public PowerCalibration PowerCalibr = new PowerCalibration();

        [DataMember(Name = "PowerCalibrationSensitivity")]
        public float fSensFactor;                                              /* Sensitivity factor */
        public float fSensitivity;                                             /* Overral sensitivity */

        FilterData[] headData = null;
        FilterData[] filterData = null;
        FilterData[] SAMData = null;

        public Boolean bLoadFilter;
        public Boolean bLoadSAM;
        public Boolean bIndSAM = false;
        public Boolean bIndOffset = false;

        [DataMember(Name = "Wavelenght")]
        public UInt16 uiWavelenght;
        [DataMember(Name = "WavelenghtMin")]
        public UInt16 uiWavelenghtMin;
        [DataMember(Name = "WavelenghtMax")]
        public UInt16 uiWavelenghtMax;

        private String[] m_strFilterName = new String[4];
        private String[] m_strFilterPath = new String[4];

        public String strSAMName;
        public String strSAMPath;

        public String strSensitivityName;
        public String strSensitivityPath;

        public Single realSAMFactor = 1f;
        public Single realFilterFactor = 1f;
        [DataMember(Name = "CurrentFilter")]
        public UInt16 currentFilter;
        [DataMember(Name = "SAMFactor")]
        public Single currentSAMFactor = 1f;

        private UInt16 iPowerUnits;

        public Single Power;
        public Single mwPower;
        public Single mwOffsetPower;
        public Boolean mbOffsetPowerOn = false;
        public Single dMin;
        public Single dMax;
        public static String[] bufUnits = { "mW", "µW", "nm", "dBm", "W", "kW" };

        public Boolean SetFilterSensitivityFile(UInt16 iIndex, String strFilterPath, String strFilterName)
        {
            Boolean bRet = false;

            if ((m_strFilterPath != null) && (iIndex < m_strFilterPath.Length) && (File.Exists(strFilterPath + "//" + strFilterName) == true))
            {
                m_strFilterName[iIndex] = strFilterName;
                m_strFilterPath[iIndex] = strFilterPath;
            }

            return bRet;
        }

        public UInt16 PowerUnits
        {
            get { return iPowerUnits; }
            set { iPowerUnits = value; }
        }

        public class WavelenghtCaomparer : IComparer
        {
            int IComparer.Compare(Object x, Object y)
            {
                FilterData fd1 = (FilterData)x;
                FilterData fd2 = (FilterData)y;

                return (fd1.Wavelength > fd2.Wavelength) ? 1 : (fd1.Wavelength < fd2.Wavelength) ? -1 : 0;
            }
        }

        public FilterData[] ReadSensitivity(String strFileName)
        {
            FilterData[] fdRet = null;
            FilterData fdTemp = new FilterData();
            ArrayList alDataSensitivity = null;

            if (File.Exists(strFileName) == true)
            {
                alDataSensitivity = null;

                try
                {
                    using (XmlTextReader reader = new XmlTextReader(strFileName))
                    {
                        while (reader.Read())
                        {
                            if (reader.IsStartElement())
                            {
                                switch (reader.Name)
                                {
                                    case "Point":
                                        reader.Read();
                                        fdTemp.Wavelength = Convert.ToUInt16(reader.ReadString());
                                        reader.Read();
                                        fdTemp.Sensitivity = Convert.ToSingle(reader.ReadString());
                                        if (alDataSensitivity == null) alDataSensitivity = new ArrayList();
                                        alDataSensitivity.Add(fdTemp);
                                        break;
                                }
                            }
                        }
                    }

                    if ((alDataSensitivity != null) && (alDataSensitivity.Count > 0))
                    {
                        alDataSensitivity.Sort(new WavelenghtCaomparer());
                        fdRet = (FilterData[])alDataSensitivity.ToArray(typeof(FilterData));
                    }
                }
                catch { }
            }

            return fdRet;
        }

        private String GetScaleStringFormat(Single Value)
        {
            String strFormat = "";
            String strValue = "";

            if ((iPowerUnits < 3) || (iPowerUnits > 3))
            {
                if (Math.Abs(Value) < 10)
                    strFormat = "{0:F3}";
                else if (Math.Abs(Value) < 100)
                    strFormat = "{0:F2}";
                else if (Math.Abs(Value) < 1000)
                    strFormat = "{0:F1}";
                else
                    strFormat = "{0:F0}";

                strValue = String.Format(strFormat, Value);
            }
            else if (iPowerUnits == 3)
            {
                strValue = String.Format("{0:F2}", 10 * Math.Log10(Value));
            }

            return strValue;
        }

        private String GetValueStringFormat(Single Value)
        {
            String strFormat = "";
            String strValue = "";

            if (iPowerUnits < 3)
            {
                if (Math.Abs(Value * Math.Pow(1000, PowerUnits)) < 10)
                    strFormat = "{0:F3}";
                else if (Math.Abs(Value * Math.Pow(1000, PowerUnits)) < 100)
                    strFormat = "{0:F2}";
                else if (Math.Abs(Value * Math.Pow(1000, PowerUnits)) < 1000)
                    strFormat = "{0:F1}";
                else
                    strFormat = "{0:F0}";

                strValue = String.Format(strFormat, Value * Math.Pow(1000, PowerUnits));
            }
            else if (iPowerUnits == 3)
            {
                strValue = String.Format("{0:F2}", 10 * Math.Log10(Value));
            }
            else
            {
                if (Math.Abs(Value / Math.Pow(1000, PowerUnits - 3)) < 10)
                    strFormat = "{0:F3}";
                else if (Math.Abs(Value / Math.Pow(1000, PowerUnits - 3)) < 100)
                    strFormat = "{0:F2}";
                else if (Math.Abs(Value / Math.Pow(1000, PowerUnits - 3)) < 1000)
                    strFormat = "{0:F1}";
                else
                    strFormat = "{0:F0}";

                strValue = String.Format(strFormat, Value / Math.Pow(1000, PowerUnits - 3));
            }

            return strValue;
        }

        public override string ToString()
        {
            String strValue;

            if (iPowerUnits < 3)
                strValue = (Math.Abs(mwPower * Math.Pow(1000, PowerUnits)) < 10000) ? GetValueStringFormat(mwPower) : "----";
            else if (iPowerUnits == 3)
                strValue = (mwPower > 0) ? String.Format("{0:F2}", 10 * Math.Log10(mwPower)) : "----";
            else
                strValue = (Math.Abs(mwPower / Math.Pow(1000, PowerUnits - 3)) < 10000) ? GetValueStringFormat(mwPower) : "----";

            return strValue;
        }

        public static String GetValueFormat(Single Value, UInt16 PowerUnits)
        {
            String strFormat = "";

            if (PowerUnits < 3)
            {
                if (Math.Abs(Value * Math.Pow(1000, PowerUnits)) < 10)
                    strFormat = "{0:F3}";
                else if (Math.Abs(Value * Math.Pow(1000, PowerUnits)) < 100)
                    strFormat = "{0:F2}";
                else if (Math.Abs(Value * Math.Pow(1000, PowerUnits)) < 1000)
                    strFormat = "{0:F1}";
                else
                    strFormat = "{0:F0}";
            }
            else if (PowerUnits == 3)
            {
                strFormat = "{0:F2}";
            }
            else
            {
                if (Math.Abs(Value / Math.Pow(1000, PowerUnits - 3)) < 10)
                    strFormat = "{0:F3}";
                else if (Math.Abs(Value / Math.Pow(1000, PowerUnits - 3)) < 100)
                    strFormat = "{0:F2}";
                else if (Math.Abs(Value / Math.Pow(1000, PowerUnits - 3)) < 1000)
                    strFormat = "{0:F1}";
                else
                    strFormat = "{0:F0}";
            }

            return strFormat;
        }

        public static String GetValueStringFormat(Single Value, UInt16 PowerUnits)
        {
            String strFormat = "";
            String strValue = "";

            if (PowerUnits < 3)
            {
                if (Math.Abs(Value * Math.Pow(1000, PowerUnits)) < 10)
                    strFormat = "{0:F3}";
                else if (Math.Abs(Value * Math.Pow(1000, PowerUnits)) < 100)
                    strFormat = "{0:F2}";
                else if (Math.Abs(Value * Math.Pow(1000, PowerUnits)) < 1000)
                    strFormat = "{0:F1}";
                else
                    strFormat = "{0:F0}";

                strValue = String.Format(strFormat, Value * Math.Pow(1000, PowerUnits));
            }
            else if (PowerUnits == 3)
            {
                strValue = (Value > 0) ? String.Format("{0:F2}", 10 * Math.Log10(Value)) : "----";
            }
            else
            {
                if (Math.Abs(Value / Math.Pow(1000, PowerUnits - 3)) < 10)
                    strFormat = "{0:F3}";
                else if (Math.Abs(Value / Math.Pow(1000, PowerUnits - 3)) < 100)
                    strFormat = "{0:F2}";
                else if (Math.Abs(Value / Math.Pow(1000, PowerUnits - 3)) < 1000)
                    strFormat = "{0:F1}";
                else
                    strFormat = "{0:F0}";

                strValue = String.Format(strFormat, Value / Math.Pow(1000, PowerUnits - 3));
            }

            return strValue;
        }

        public void SetSensitivityFactor(Single fPowerCalibration, Single fPowerMeasure)
        {
            fSensFactor *= fPowerMeasure / ((fPowerCalibration + mwOffsetPower * Math.Abs(Convert.ToInt16(bIndOffset))) * realFilterFactor * currentSAMFactor);
        }

        public void SetPointSensitivityFactor(Single fPowerCalibration, UInt16 uiNumStep)
        {
            int i = 0;

            Single fPointSensFactor = Power / ((fPowerCalibration + mwOffsetPower * Math.Abs(Convert.ToInt16(bIndOffset))) * realFilterFactor * currentSAMFactor);

            for (i = 0; (i < headData.Length) && (headData[i].Wavelength <= uiWavelenght); i++) headData[i].Sensitivity *= fPointSensFactor;

            for (int j = i; j <= uiNumStep; j++) headData[j].Sensitivity *= (fPointSensFactor - ((fPointSensFactor - 1) * (j - i)) / (uiNumStep - i));
        }

        public void SetSensitivity()
        {
            UInt16 i;

            if (headData != null)
            {
                if (uiWavelenght <= headData[0].Wavelength)
                {
                    fSensitivity = headData[0].Sensitivity;
                }
                else if (uiWavelenght >= headData[headData.Length - 1].Wavelength)
                {
                    fSensitivity = headData[headData.Length - 1].Sensitivity;
                }
                else
                {
                    for (i = 1; ((i < headData.Length) && (uiWavelenght >= headData[i].Wavelength)); i++) ;

                    fSensitivity = headData[i - 1].Sensitivity + (uiWavelenght - headData[i - 1].Wavelength) * (headData[i].Sensitivity - headData[i - 1].Sensitivity) / (headData[i].Wavelength - headData[i - 1].Wavelength);
                }

                fSensitivity *= fSensFactor;
            }
        }

        public void SetFilterFactor()
        {
            int i;

            if (filterData != null)
            {
                if (uiWavelenght <= filterData[0].Wavelength)
                {
                    realFilterFactor = filterData[0].Sensitivity;
                }
                else if (uiWavelenght >= filterData[filterData.Length - 1].Wavelength)
                {
                    realFilterFactor = filterData[filterData.Length - 1].Sensitivity;
                }
                else
                {
                    for (i = 0; ((i < filterData.Length) && (uiWavelenght >= filterData[i].Wavelength)); i++) ;

                    realFilterFactor = filterData[i - 1].Sensitivity + (uiWavelenght - filterData[i - 1].Wavelength) * (filterData[i].Sensitivity - filterData[i - 1].Sensitivity) / (float)(filterData[i].Wavelength - filterData[i - 1].Wavelength);
                }
            }
            else
            {
                realFilterFactor = 1f;
            }
        }

        public void SetPointSAMFactor(Single fPowerCalibration)
        {
            int i = 0;

            Single fPointSensFactor = Power / ((fPowerCalibration * 1000f + mwOffsetPower * Math.Abs(Convert.ToInt16(bIndOffset))) * realFilterFactor * currentSAMFactor);

            for (i = 0; (i < SAMData.Length) && (SAMData[i].Wavelength < uiWavelenght); i++) ;

            if (SAMData[i].Wavelength == uiWavelenght)
            {
                SAMData[i].Sensitivity *= fPointSensFactor;
            }
            else
            {
                FilterData[] SAMData1 = new FilterData[SAMData.Length + 1];

                for (i = 0; (i < SAMData.Length) && (uiWavelenght > SAMData[i].Wavelength); i++) SAMData1[i] = SAMData[i];

                SAMData1[i].Wavelength = uiWavelenght;
                SAMData1[i].Sensitivity = realSAMFactor * fPointSensFactor;

                i++;

                for (; i < SAMData1.Length; i++) SAMData1[i] = SAMData[i - 1];

                SAMData = SAMData1;
            }

            if (File.Exists(strSAMPath + "\\" + strSAMName) == true)
            {
                using (StreamWriter sw = new StreamWriter(strSAMPath + "\\" + strSAMName))
                {
                    try
                    {
                        sw.WriteLine("SAM " + SAMData.Length.ToString());
                        for (i = 0; i < SAMData.Length; i++) sw.WriteLine(SAMData[i].Wavelength.ToString() + " " + SAMData[i].Sensitivity);
                    }
                    catch
                    {
                        CustomMessageBox.Show("Error in the SAM file " + strSAMName + ".", "Error writing SAM file.", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    finally
                    {
                        sw.Close();
                    }
                }
            }
        }

        public void SetSAMFactor()
        {
            int i;

            if (SAMData != null)
            {
                if (uiWavelenght <= SAMData[0].Wavelength)
                    realSAMFactor = SAMData[0].Sensitivity;
                else if (uiWavelenght >= SAMData[SAMData.Length - 1].Wavelength)
                    realSAMFactor = SAMData[SAMData.Length - 1].Sensitivity;
                else
                {
                    for (i = 0; ((i < SAMData.Length) && (uiWavelenght >= SAMData[i].Wavelength)); i++) ;

                    realSAMFactor = SAMData[i - 1].Sensitivity + (uiWavelenght - SAMData[i - 1].Wavelength) * (SAMData[i].Sensitivity - SAMData[i - 1].Sensitivity) / (float)(SAMData[i].Wavelength - SAMData[i - 1].Wavelength);
                }
            }
            else
                realSAMFactor = 1f;
        }

        public Boolean ReadSAMFile()
        {
            SAMData = ReadSensitivity(strSAMPath + "\\" + strSAMName);

            bLoadSAM = (SAMData != null);

            if (bLoadSAM == false)
            {
                //CustomMessageBox.Show("Error in the SAM file " + ((strSAMName != null) ? strSAMName : "") + " .", "Error reading SAM file.", MessageBoxButtons.OK, MessageBoxIcon.Error);

                strSAMName = "";
                strSAMPath = "";
                bIndSAM = false;
                realSAMFactor = 1.0f;
            }

            return bLoadSAM;
        }

        public Boolean ReadSensitivitySensor(String strFileName)
        {
            if (strFileName != null)
                headData = ReadSensitivity(strFileName);
            else
                headData = null;

            return (headData != null);
        }

        public Boolean ReadFilterFile(UInt16 uiIndex)
        {
            if ((m_strFilterPath != null) && (m_strFilterName != null))
                filterData = ReadSensitivity(m_strFilterPath[uiIndex] + "//" + m_strFilterName[uiIndex]);
            else
                filterData = null;

            bLoadFilter = (filterData != null);

            if (bLoadFilter == false)
            {
                //                CustomMessageBox.Show("Error in the filter file " + ((m_strFilterName[uiIndex] != null) ? m_strFilterName[uiIndex] : "") + " .", "Error reading filter file.", MessageBoxButtons.OK, MessageBoxIcon.Error);

                m_strFilterName[uiIndex] = "";
                m_strFilterPath[uiIndex] = "";
                realFilterFactor = 1.0f;
            }

            return bLoadFilter;
        }

        internal void InitializeComponent()
        {
            m_strFilterName = new String[4];
            m_strFilterPath = new String[4];

            bufUnits = new String[] { "mW", "µW", "nm", "dBm", "W", "kW" };
            //PowerCalibr = new PowerCalibration();
        }
    }

    [DataContract(Name = "Clip")]
    public class LevelsData
    {
        [DataMember]
        LevelData[] m_ld = null;

        public LevelsData()
        {
            NumberLevels = 3;

            m_ld[0].Level = 13.5M;
            m_ld[1].Level = 50M;
            m_ld[2].Level = 80M;

            InitializeComponent();
        }

        public void InitializeComponent()
        {
            m_ld[0].LevelBrush = new SolidBrush(Color.Red);
            m_ld[0].LevelPen = new Pen(m_ld[0].LevelBrush, 0.1f);
            m_ld[1].LevelBrush = new SolidBrush(Color.Blue);
            m_ld[1].LevelPen = new Pen(m_ld[1].LevelBrush, 0.1f);
            m_ld[2].LevelBrush = new SolidBrush(Color.Green);
            m_ld[2].LevelPen = new Pen(m_ld[2].LevelBrush, 0.1f);
        }

        public Decimal Level(int Index)
        {
            return m_ld[Index].Level;
        }

        public Pen LevelPen(int Index)
        {
            return m_ld[Index].LevelPen;
        }

        public Brush LevelBrush(int Index)
        {
            return m_ld[Index].LevelBrush;
        }

        public void SetLevel(int Index, Decimal dValue)
        {
            m_ld[Index].Level = dValue;
        }

        public UInt16 NumberLevels
        {
            get { return (m_ld != null) ? (UInt16)m_ld.Length : (UInt16)0; }

            set
            {
                if (((m_ld != null) && (m_ld.Length != value)) || (m_ld == null))
                {
                    m_ld = new LevelData[value];

                    for (int i = 0; i < m_ld.Length; i++)
                    {
                        m_ld[i] = new LevelData();

                        m_ld[i].Level = 0M;
                        m_ld[i].LevelBrush = new SolidBrush(Color.Black);
                        m_ld[i].LevelPen = new Pen(m_ld[i].LevelBrush, 0.1f);
                    }
                }
            }
        }
    }

    [DataContract]
    public class LevelData
    {
        [DataMember]
        Decimal LevelValue;
        public Brush LevelBrush;
        public Pen LevelPen;

        public Decimal Level
        {
            get { return LevelValue; }

            set
            {
                if ((value > 0) && (value < 100)) LevelValue = value;
            }
        }
    }

    public enum MeasureUnits { muMicro, muMiliRad };

    public class EllipseData
    {
        private double m_dMajorRadius = 0f;
        private double m_dMinorRadius = 0f;
        private double m_dAngleOrientation = 0f;
        private double m_dAngleOrientationRad = 0f;
        private Single m_fPixelSize = 5.86f;

        public double Major
        {
            get { return m_dMajorRadius; }
            set { m_dMajorRadius = value * m_fPixelSize; }
        }

        public double Minor
        {
            get { return m_dMinorRadius; }
            set { m_dMinorRadius = value * m_fPixelSize; }
        }

        public double Orientation
        {
            get { return m_dAngleOrientation; }
            set { m_dAngleOrientation = value; }
        }

        public double OrientationRad
        {
            get { return m_dAngleOrientationRad; }
            set { m_dAngleOrientationRad = value; }
        }

        public bool EllipseError { get; set; }

        public Single PixelSize
        {
            get { return m_fPixelSize; }
            set { m_fPixelSize = value; }
        }
    }

    [DataContract]
    public class PositionData
    {
        private PointF m_realPosition = new PointF();
        public Int16 iHeadTilt;
        public Single[] PositionWidth = new Single[7];
        public Boolean bZoomPosition;
        public Boolean bGridOn;
        private Single PrX = 0f;
        private Single PrY = 0f;
        private String strPrX = "0.0";
        private String strPrY = "0.0";
        private String strRadPrX = "0.0";
        private String strRadPrY = "0.0";
        private String strFormatX = "{0:F1}";
        private String strFormatY = "{0:F1}";
        private String strRadFormatX = "{0:F1}";
        private String strRadFormatY = "{0:F1}";

        public Color BackgroundColor = Color.DarkBlue;
        public Color ForegroundColor = Color.White;
        public Color PlotColor = Color.GreenYellow;
        private MeasureUnits m_muUnits = MeasureUnits.muMicro;
        private Single m_sFocalLens = 1f;
        private Single m_sUnitsCoeff = 1f;
        private Single m_sOpticalFactor = 1f;
        private EllipseData m_edEllipse = new EllipseData();

        private Single m_fPixelSize = 5.86f;

        public void InitializeComponent()
        {
            m_realPosition = new PointF();
            PositionWidth = new Single[7];
            strPrX = "0.0";
            strPrY = "0.0";
            strRadPrX = "0.0";
            strRadPrY = "0.0";
            strFormatX = "{0:F1}";
            strFormatY = "{0:F1}";
            strRadFormatX = "{0:F1}";
            strRadFormatY = "{0:F1}";

            BackgroundColor = Color.DarkBlue;
            ForegroundColor = Color.White;
            PlotColor = Color.GreenYellow;
            m_muUnits = MeasureUnits.muMicro;
            m_sFocalLens = 1f;
            m_sUnitsCoeff = 1f;
            m_sOpticalFactor = 1f;
            m_edEllipse = new EllipseData();
            m_edEllipse.PixelSize = m_fPixelSize;
        }

        public Single PixelSize
        {
            get { return m_fPixelSize; }
            set
            {
                m_fPixelSize = value;
                m_edEllipse.PixelSize = m_fPixelSize;
            }
        }

        public EllipseData Ellipse
        {
            get { return m_edEllipse; }
        }

        public String FormatX
        {
            get { return (m_muUnits == MeasureUnits.muMicro) ? strFormatX : strRadFormatX; }
        }

        public String FormatY
        {
            get { return (m_muUnits == MeasureUnits.muMicro) ? strFormatY : strRadFormatY; }
        }

        public Single PositionX
        {
            get { return PrX; }
            set
            {
                PrX = value;
                strFormatX = SetFormat(PrX * m_sUnitsCoeff);
                strPrX = String.Format(strFormatX, PrX * m_sUnitsCoeff);

                strRadFormatX = SetFormat(PrX * m_sUnitsCoeff);
                strRadPrX = String.Format(strRadFormatX, PrX * m_sUnitsCoeff);
            }
        }

        public Single PositionY
        {
            get { return PrY; }
            set
            {
                PrY = value;

                strFormatY = SetFormat(PrY * m_sUnitsCoeff);
                strPrY = String.Format(strFormatY, PrY * m_sUnitsCoeff);

                strRadFormatY = SetFormat(PrY * m_sUnitsCoeff);
                strRadPrY = String.Format(strRadFormatY, PrY * m_sUnitsCoeff);
            }
        }

        public Single UnitsCoeff
        {
            get { return m_sUnitsCoeff; }
        }

        public PointF RealPosition
        {
            get { return m_realPosition; }

            set
            {
                m_realPosition = new PointF(value.X * m_fPixelSize, value.Y * m_fPixelSize);

                PositionX = (Single)((m_realPosition.X * Math.Cos(Math.PI * iHeadTilt / 180f) - m_realPosition.Y * Math.Sin(Math.PI * iHeadTilt / 180f)) * m_sUnitsCoeff);
                PositionY = (Single)((m_realPosition.X * Math.Sin(Math.PI * iHeadTilt / 180f) + m_realPosition.Y * Math.Cos(Math.PI * iHeadTilt / 180f)) * m_sUnitsCoeff);
            }
        }

        public String strMajor
        {
            get
            {
                return (m_muUnits == MeasureUnits.muMicro) ? String.Format(SetFormat((Single)(m_edEllipse.Major * m_sUnitsCoeff)), m_edEllipse.Major * m_sUnitsCoeff) : String.Format(SetFormat((Single)(m_edEllipse.Major * m_sUnitsCoeff)), m_edEllipse.Major * m_sUnitsCoeff);
            }
        }

        public String strMinor
        {
            get
            {
                return (m_muUnits == MeasureUnits.muMicro) ? String.Format(SetFormat((Single)(m_edEllipse.Minor * m_sUnitsCoeff)), m_edEllipse.Minor * m_sUnitsCoeff) : String.Format(SetFormat((Single)(m_edEllipse.Minor * m_sUnitsCoeff)), m_edEllipse.Minor * m_sUnitsCoeff);
            }
        }

        public String strPositionX
        {
            get { return (m_muUnits == MeasureUnits.muMicro) ? strPrX : strRadPrX; }
        }

        public String strPositionY
        {
            get { return (m_muUnits == MeasureUnits.muMicro) ? strPrY : strRadPrY; }
        }

        private String SetFormat(float fData)
        {
            String strFormat;

            if (Math.Abs(fData) >= 1000f)
                strFormat = "{0:F0}";
            else if (Math.Abs(fData) >= 100f)
                strFormat = "{0:F1}";
            else if (Math.Abs(fData) >= 10f)
                strFormat = "{0:F2}";
            else
                strFormat = "{0:F3}";

            return strFormat;
        }

        public MeasureUnits UnitMeasure
        {
            get { return m_muUnits; }
            set
            {
                m_muUnits = value;
                m_sUnitsCoeff = (m_muUnits == MeasureUnits.muMiliRad) ? 1f / (m_sFocalLens * m_sOpticalFactor) : 1f / m_sOpticalFactor;
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
    }

    [DataContract]
    public class ProfileData
    {
        private MeasureUnits m_muUnits = MeasureUnits.muMicro;
        private Single m_sFocalLens = 1f;
        private Single m_sUnitsCoeff = 1f;
        private Single m_sOpticalFactor = 1f;

        private Single m_fPixelSize = 5.86f;

        public const UInt16 NUM_LEVELS = 3;
        public double m_fCorrelation;

        double[] m_fWidth = new double[NUM_LEVELS];
        String[] m_strWidth = new String[NUM_LEVELS];
        String[] m_strRadWidth = new String[NUM_LEVELS];
        String[] m_strFormat = new String[NUM_LEVELS];
        String[] m_strRadFormat = new String[NUM_LEVELS];

        double[] m_fGaussWidth = new double[NUM_LEVELS];
        String[] m_strGaussFormat = new String[NUM_LEVELS];
        String[] m_strGaussWidth = new String[NUM_LEVELS];
        String[] m_strRadGaussFormat = new String[NUM_LEVELS];
        String[] m_strRadGaussWidth = new String[NUM_LEVELS];

        public void InitializeComponent()
        {
            m_sUnitsCoeff = 1f;

            m_fWidth = new double[NUM_LEVELS];
            m_strWidth = new String[NUM_LEVELS];
            m_strRadWidth = new String[NUM_LEVELS];
            m_strFormat = new String[NUM_LEVELS];
            m_strRadFormat = new String[NUM_LEVELS];

            m_fGaussWidth = new double[NUM_LEVELS];
            m_strGaussFormat = new String[NUM_LEVELS];
            m_strGaussWidth = new String[NUM_LEVELS];
            m_strRadGaussFormat = new String[NUM_LEVELS];
            m_strRadGaussWidth = new String[NUM_LEVELS];
        }

        public Single PixelSize
        {
            get { return m_fPixelSize; }
            set { m_fPixelSize = value; }
        }

        public double[] Width
        {
            get { return m_fWidth; }
        }

        public String[] strWidth
        {
            get { return (m_muUnits == MeasureUnits.muMicro) ? m_strWidth : m_strRadWidth; }
        }

        public double[] GaussWidth
        {
            get { return m_fGaussWidth; }
        }

        public String[] strGaussWidth
        {
            get { return (m_muUnits == MeasureUnits.muMicro) ? m_strGaussWidth : m_strRadGaussWidth; }
        }

        public void SetWidthProfile(int iIndex, Double dValue)
        {
            m_fWidth[iIndex] = dValue * m_fPixelSize;

            m_strFormat[iIndex] = GetValueStringFormat(m_fWidth[iIndex] * m_sUnitsCoeff);

            m_strWidth[iIndex] = String.Format(m_strFormat[iIndex], m_fWidth[iIndex] * m_sUnitsCoeff);

            m_strRadFormat[iIndex] = GetValueStringFormat(m_fWidth[iIndex] * m_sUnitsCoeff);

            m_strRadWidth[iIndex] = String.Format(m_strRadFormat[iIndex], m_fWidth[iIndex] * m_sUnitsCoeff);
        }

        public void SetWidthGauss(int iIndex, Double dValue)
        {
            m_fGaussWidth[iIndex] = dValue * m_fPixelSize;

            m_strGaussFormat[iIndex] = GetValueStringFormat(m_fGaussWidth[iIndex] * m_sUnitsCoeff);

            m_strGaussWidth[iIndex] = String.Format(m_strGaussFormat[iIndex], m_fGaussWidth[iIndex] * m_sUnitsCoeff);

            m_strRadGaussFormat[iIndex] = GetValueStringFormat(m_fGaussWidth[iIndex] * m_sUnitsCoeff);

            m_strRadGaussWidth[iIndex] = String.Format(m_strRadGaussFormat[iIndex], m_fGaussWidth[iIndex] * m_sUnitsCoeff);
        }

        public MeasureUnits UnitMeasure
        {
            get { return m_muUnits; }
            set
            {
                m_muUnits = value;
                m_sUnitsCoeff = (m_muUnits == MeasureUnits.muMiliRad) ? 1f / (m_sFocalLens * m_sOpticalFactor) : 1f / m_sOpticalFactor;
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
    }

    [DataContract]
    public class ProjectionData
    {
        [DataMember]
        public Boolean bViewGrid = false;
        [DataMember]
        public Int16 StepRotation = 5;
        [DataMember]
        public Int16 m_iAngleRotation = 30;
        [DataMember]
        public Int16 m_iAngleTilt = 30;
        [DataMember]
        public OpenGLControl.TypeGrid m_eResolution3D = OpenGLControl.TypeGrid.Low;
        [DataMember]
        public Boolean bAutoRot = false;
        [DataMember]
        public Boolean bProjection = false;
        [DataMember]
        public OpenGLControl.TypeProjection Projection = OpenGLControl.TypeProjection.NoneProjection;

        public Int16 AngleTilt
        {
            get { return m_iAngleTilt; }
            set
            {
                m_iAngleTilt = value;
                while (m_iAngleTilt > 359) m_iAngleTilt -= 360;
                while (m_iAngleTilt < 0) m_iAngleTilt += 360;
            }
        }

        public Int16 AngleRotation
        {
            get { return m_iAngleRotation; }
            set
            {
                m_iAngleRotation = value;
                while (m_iAngleRotation > 359) m_iAngleRotation -= 360;
                while (m_iAngleRotation < 0) m_iAngleRotation += 360;
            }
        }

        public Rectangle m_rectImage;
        public Byte[][] m_pbImageData = null;

        public void InitializeComponent()
        {
        }
    }
}
