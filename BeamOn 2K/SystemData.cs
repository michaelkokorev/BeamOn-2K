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

namespace BeamOn_2K
{
    public enum FileType { ftLog, ftExcel, ftXML };
    public enum LogType { ltTime, ltPoints, ltManual };

    public sealed class SystemData
    {
        [DataContract(Name = "Setup")]
        private class Data
        {
            [DataMember(Name = "PixelFormat")]
            public PixelFormat m_pixelFormat = PixelFormat.Format8bppIndexed;
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
                    m_sUnitsCoeff = (m_muUnits == MeasureUnits.muMiliRad) ? 1f / (m_sFocalLens * m_sOpticalFactor) : 1f / m_sOpticalFactor;

                    m_positionData.FocalLens = m_sFocalLens;
                    //projectionData.FocalLens = m_sFocalLens;

                    m_profDataHorizontal.FocalLens = m_sFocalLens;
                    m_profDataVertical.FocalLens = m_sFocalLens;
                }
            }
        }

        public BeamOnCL.BeamOnCL.TypeProfile ProfileType
        {
            get { return m_data.m_tpProfile; }
            set { m_data.m_tpProfile = value; }
        }

        public PixelFormat FormatPixel
        {
            get { return m_data.m_pixelFormat; }
            set { m_data.m_pixelFormat = value; }
        }

        public Boolean ViewGaussian
        {
            get { return m_data.m_bGaussian; }
            set { m_data.m_bGaussian = value; }
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
            var ds = new DataContractSerializer(typeof(Data));
            using (Stream s = File.OpenRead(strPath))
                m_data = (Data)ds.ReadObject(s); // Десериализация

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

    [DataContract(Name = "Setup")]
    public class ApplicationData
    {
        [DataMember(Name = "ToolBar")]
        public Boolean bViewToolbar = true;

        [DataMember(Name = "StatusBar")]
        public Boolean bViewStatusBar = true;

        [DataMember(Name = "SaveExit")]
        public Boolean bSaveExit = true;

        [DataMember(Name = "ViewPower")]
        public Boolean bStatusViewPower;

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
        public FormErrorMessage m_FormErrorMessage = null;

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
            get { return m_strHelpNamespace; ;}
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

    [DataContract(Name = "Power")]
    public class PowerData
    {
        public float fSensFactor = 1.0f;                                             /* Sensitivity factor */
        public float fSensitivity = 0.387f;                                          /* Overral sensitivity */
        public FilterData[] headData = null;

        public Boolean bLoadFilter;
        public Boolean bLoadSAM;
        public Boolean bIndFilter = false;
        public Boolean bIndSAM = false;
        public Boolean bIndOffset = false;

        [DataMember(Name = "Wavelenght")]
        public UInt16 uiWavelenght;

        public UInt16 uiWavelenghtMin;
        public UInt16 uiWavelenghtMax;
        public String strFilterName;
        public String strFilterPath;
        public String strSAMName;
        public String strSAMPath;
        public String strSensitivityName;
        public String strSensitivityPath;
        public Single realSAMFactor = 1f;
        public Single realFilterFactor = 1f;
        public Single currentFilterFactor = 1f;
        public Single currentSAMFactor = 1f;
        public FilterData[] filterData = null;
        public FilterData[] SAMData = null;
        private UInt16 iPowerUnits;

        public Single Power;
        public Single mwPower;
        public Single mwOffsetPower;
        public Boolean mbOffsetPowerOn = false;
        public Single dMin;
        public Single dMax;
        public static String[] bufUnits = { "mW", "µW", "nm", "dBm", "W", "kW" };

        public UInt16 PowerUnits
        {
            get { return iPowerUnits; }
            set { iPowerUnits = value; }
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

        public void SetSensitivityFactor(Single fPowerCalibration)
        {
            fSensFactor *= Power / ((fPowerCalibration + mwOffsetPower * Math.Abs(Convert.ToInt16(bIndOffset))) * currentFilterFactor * currentSAMFactor);
        }

        public void SetPointSensitivityFactor(Single fPowerCalibration, UInt16 uiNumStep)
        {
            int i = 0;

            Single fPointSensFactor = Power / ((fPowerCalibration + mwOffsetPower * Math.Abs(Convert.ToInt16(bIndOffset))) * currentFilterFactor * currentSAMFactor);

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

            Single fPointSensFactor = Power / ((fPowerCalibration * 1000f + mwOffsetPower * Math.Abs(Convert.ToInt16(bIndOffset))) * currentFilterFactor * currentSAMFactor);

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

        public Boolean ReadSensitivityFile()
        {
            Boolean bRet = true;

            String strData;
            int lastLocation;

            if (File.Exists(strSensitivityPath + "\\" + strSensitivityName) == false)
            {
                strSensitivityName = "";
                strSensitivityPath = "";
            }
            else
            {
                using (StreamReader sr = new StreamReader(strSensitivityPath + "\\" + strSensitivityName))
                {
                    strData = sr.ReadLine();
                    strData = strData.Trim(' ');
                    lastLocation = strData.IndexOf(" ");
                    if (lastLocation >= 0) strData = strData.Substring(lastLocation + 1);

                    headData = new FilterData[Convert.ToUInt16(strData)];

                    try
                    {
                        for (int i = 0; i < headData.Length; i++)
                        {
                            strData = sr.ReadLine();
                            strData = strData.Trim(' ');
                            lastLocation = strData.LastIndexOf(" ");

                            headData[i].Wavelength = Convert.ToUInt16(strData.Substring(0, lastLocation));
                            headData[i].Sensitivity = Convert.ToSingle(strData.Substring(lastLocation + 1).Normalize(), CultureInfo.InvariantCulture.NumberFormat);
                        }
                    }
                    catch
                    {
                        CustomMessageBox.Show("Error in the sensitivity file " + strSensitivityName + " .", "Error reading sensitivity file.", MessageBoxButtons.OK, MessageBoxIcon.Error);

                        strSensitivityName = "";
                        strSensitivityPath = "";

                        bRet = false;
                    }
                    finally
                    {
                        sr.Close();
                    }
                }
            }

            return bRet;
        }

        public Boolean ReadSAMFile()
        {
            Boolean bRet = true;

            String strData;
            int lastLocation;

            if (File.Exists(strSAMPath + "\\" + strSAMName) == false)
            {
                strSAMName = "";
                strSAMPath = "";
                bIndSAM = false;
                bLoadSAM = false;
                realSAMFactor = 1.0f;
            }
            else
            {
                using (StreamReader sr = new StreamReader(strSAMPath + "\\" + strSAMName))
                {
                    strData = sr.ReadLine();
                    strData = strData.Trim(' ');
                    lastLocation = strData.IndexOf(" ");
                    if (lastLocation >= 0) strData = strData.Substring(lastLocation + 1);

                    SAMData = new FilterData[Convert.ToUInt16(strData)];

                    try
                    {
                        for (int i = 0; i < SAMData.Length; i++)
                        {
                            strData = sr.ReadLine();
                            strData = strData.Trim(' ');
                            lastLocation = strData.LastIndexOf(" ");

                            SAMData[i].Wavelength = Convert.ToUInt16(strData.Substring(0, lastLocation));
                            SAMData[i].Sensitivity = Convert.ToSingle(strData.Substring(lastLocation + 1).Normalize(), CultureInfo.InvariantCulture.NumberFormat);
                            bLoadSAM = true;
                        }
                    }
                    catch
                    {
                        CustomMessageBox.Show("Error in the SAM file " + strSAMName + " .", "Error reading SAM file.", MessageBoxButtons.OK, MessageBoxIcon.Error);

                        strSAMName = "";
                        strSAMPath = "";
                        bIndSAM = false;
                        bLoadSAM = false;
                        realSAMFactor = 1.0f;

                        bRet = false;
                    }
                    finally
                    {
                        sr.Close();
                    }
                }
            }

            return bRet;
        }

        public Boolean ReadFilterFile()
        {
            Boolean bRet = true;

            String strData;
            int lastLocation;

            if (File.Exists(strFilterPath + "\\" + strFilterName) == false)
            {
                strFilterName = "";
                strFilterPath = "";
                bIndFilter = false;
                bLoadFilter = false;
                realFilterFactor = 1.0f;
            }
            else
            {
                using (StreamReader sr = new StreamReader(strFilterPath + "\\" + strFilterName))
                {
                    strData = sr.ReadLine();
                    strData = strData.Trim(' ');
                    lastLocation = strData.IndexOf(" ");
                    if (lastLocation >= 0) strData = strData.Substring(lastLocation + 1);

                    filterData = new FilterData[Convert.ToUInt16(strData)];

                    try
                    {
                        for (int i = 0; i < filterData.Length; i++)
                        {
                            strData = sr.ReadLine();
                            strData = strData.Trim(' ');
                            lastLocation = strData.LastIndexOf(" ");

                            filterData[i].Wavelength = Convert.ToUInt16(strData.Substring(0, lastLocation));
                            filterData[i].Sensitivity = Convert.ToSingle(strData.Substring(lastLocation + 1).Normalize(), CultureInfo.InvariantCulture.NumberFormat);
                            bLoadFilter = true;
                        }
                    }
                    catch
                    {
                        CustomMessageBox.Show("Error in the filter file " + strFilterName + " .", "Error reading filter file.", MessageBoxButtons.OK, MessageBoxIcon.Error);

                        strFilterName = "";
                        strFilterPath = "";
                        bIndFilter = false;
                        bLoadFilter = false;
                        realFilterFactor = 1.0f;

                        bRet = false;
                    }
                    finally
                    {
                        sr.Close();
                    }
                }
            }

            return bRet;
        }

        internal void InitializeComponent()
        {
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

        public double Major
        {
            get { return m_dMajorRadius; }
            set { m_dMajorRadius = value; }
        }

        public double Minor
        {
            get { return m_dMinorRadius; }
            set { m_dMinorRadius = value; }
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
                m_realPosition = value;

                PositionX = (Single)(m_realPosition.X * Math.Cos(Math.PI * iHeadTilt / 180f) - m_realPosition.Y * Math.Sin(Math.PI * iHeadTilt / 180f));
                PositionY = (Single)(m_realPosition.X * Math.Sin(Math.PI * iHeadTilt / 180f) + m_realPosition.Y * Math.Cos(Math.PI * iHeadTilt / 180f));
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
            m_fWidth[iIndex] = dValue;

            m_strFormat[iIndex] = GetValueStringFormat(m_fWidth[iIndex] * m_sUnitsCoeff);

            m_strWidth[iIndex] = String.Format(m_strFormat[iIndex], m_fWidth[iIndex] * m_sUnitsCoeff);

            m_strRadFormat[iIndex] = GetValueStringFormat(m_fWidth[iIndex] * m_sUnitsCoeff);

            m_strRadWidth[iIndex] = String.Format(m_strRadFormat[iIndex], m_fWidth[iIndex] * m_sUnitsCoeff);
        }

        public void SetWidthGauss(int iIndex, Double dValue)
        {
            m_fGaussWidth[iIndex] = dValue;

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
