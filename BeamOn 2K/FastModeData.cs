using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Xml;
using System.IO;
using System.Globalization;
using System.Xml.Xsl;
using System.Xml.XPath;

namespace BeamOn_U3
{

    #region FastModeDataSample

    public abstract class FastModeDataSample
    {
        protected Boolean m_bOpenFile = false;
        protected SystemData m_sysData = SystemData.MyInstance;

        public abstract void CreateHeader();
        public abstract void AddData(ArrayList arrayFastData);

        public virtual bool IsOpen()
        {
            return m_bOpenFile;
        }
    }

    #endregion

    #region FastModeDataXML

    public class FastModeDataXML : FastModeDataSample
    {
        XmlTextWriter writer = null;

        public override void CreateHeader()
        {
            FileInfo fi = new FileInfo(m_sysData.fastModeData.strFileName);

            try
            {
                writer = new XmlTextWriter(m_sysData.fastModeData.strFileName, System.Text.Encoding.ASCII);

                writer.WriteStartDocument();
                writer.WriteStartElement("Log");

                writer.WriteStartElement("Header");

                writer.WriteStartElement("Version");
                writer.WriteValue(m_sysData.applicationData.ProductName + " Measurement system, Version " + m_sysData.applicationData.ProductVersion + (m_sysData.Demo ? " Demo Version" : ""));
                writer.WriteEndElement();

                writer.WriteStartElement("Date");
                writer.WriteValue(DateTime.Now.ToString("dd MMMM yyyy", CultureInfo.CreateSpecificCulture("en-US")));
                writer.WriteEndElement();

                writer.WriteStartElement("Time");
                writer.WriteValue(DateTime.Now.ToString("hh:mm:ss tt", CultureInfo.InvariantCulture));
                writer.WriteEndElement();

                if ((m_sysData.applicationData.m_strUserTitle != null) && (m_sysData.applicationData.m_strUserTitle.Equals("") == false))
                {
                    writer.WriteStartElement("UserData");
                    writer.WriteValue(m_sysData.applicationData.m_strUserTitle.ToString());
                    writer.WriteEndElement();
                }

                writer.WriteStartElement("Serial");
                writer.WriteValue(m_sysData.applicationData.SystemNumber);
                writer.WriteEndElement();

                writer.WriteStartElement("Data");
                m_bOpenFile = true;
            }
            catch
            {
                if (writer != null) writer.Close();
                m_bOpenFile = false;
            }
        }

        public override void AddData(ArrayList arrayFastData)
        {
            String MyTime = "{0:#0.000}";

            try
            {
                if ((writer != null) && (IsOpen() == true))
                {
                    foreach (double dTime in arrayFastData)
                    {
                        writer.WriteStartElement("Measure");

                        writer.WriteStartElement("Time");
                        writer.WriteValue(String.Format(MyTime, dTime));
                        writer.WriteEndElement();

                        writer.WriteEndElement();
                    }
                }
            }
            catch
            {
                if (writer != null)
                {
                    writer.Close();
                    m_bOpenFile = false;
                }
            }
        }

        private void XML2HTML()
        {
            try
            {
                System.Xml.Xsl.XslCompiledTransform myXslCompiledTransform = new XslCompiledTransform();

                //XslTransform myXslTransform = new XslTransform();
                XPathDocument myXPathDocument = new XPathDocument(m_sysData.logData.strFileName);

                //myXslTransform.Load(m_sysData.applicationData.m_strMyAppDir + "\\LogTranslator.xslt");
                myXslCompiledTransform.Load(m_sysData.applicationData.m_strMyAppDir + "\\FastTranslator.xslt");

                XmlTextWriter writer = new XmlTextWriter(m_sysData.logData.strFileName.Substring(0, m_sysData.logData.strFileName.LastIndexOf(".")) + ".htm", System.Text.Encoding.UTF8);

                //myXslTransform.Transform(myXPathDocument, null, writer);
                myXslCompiledTransform.Transform(myXPathDocument, null, writer);

                writer.Flush();
                writer.Close();

                System.IO.StringWriter stWrite = new System.IO.StringWriter();
                //myXslTransform.Transform(myXPathDocument, null, stWrite);
                myXslCompiledTransform.Transform(myXPathDocument, null, stWrite);
            }
            catch
            {
            }
        }
    }

    #endregion

    #region FastModeDataText

    public class FastModeDataText : FastModeDataSample
    {

        public override void AddData(ArrayList arrayFastData)
        {
            String MyTime = "{0:#0.0000}";

            using (StreamWriter w = File.AppendText(m_sysData.fastModeData.strFileName))
            {
                foreach (double dTime in arrayFastData)
                {
                    w.Write("{0, 10}", String.Format(MyTime, dTime));
                    w.WriteLine();
                }

                w.Close();
            }
        }

        public override void CreateHeader()
        {
            FileInfo fi = new FileInfo(m_sysData.fastModeData.strFileName);

            using (StreamWriter sw = File.CreateText(m_sysData.fastModeData.strFileName))
            {
                sw.WriteLine("*** " + m_sysData.applicationData.ProductName + " Measurement system, Version " + m_sysData.applicationData.ProductVersion + (m_sysData.Demo ? " Demo Version" : "") + " ***");
                sw.WriteLine();

                sw.WriteLine("File:  " + fi.Name.ToString());
                sw.WriteLine("Date:  " + DateTime.Now.ToString("dd MMMM yyyy", CultureInfo.CreateSpecificCulture("en-US")));
                sw.WriteLine("Time:  " + DateTime.Now.ToString("hh:mm:ss tt", CultureInfo.InvariantCulture));
                sw.WriteLine();

                if ((m_sysData.applicationData.m_strUserTitle != null) && (m_sysData.applicationData.m_strUserTitle.Equals("") == false))
                {
                    sw.WriteLine("User Data: [" + m_sysData.applicationData.m_strUserTitle.ToString() + "]");
                    sw.WriteLine();
                }

                sw.WriteLine("S/N: " + m_sysData.applicationData.SystemNumber);
                sw.WriteLine();

                sw.Write("{0, 10}", "Time");

                sw.WriteLine();

                sw.Write("{0, 10}", "(sec)");
                sw.WriteLine();

                sw.WriteLine();

                sw.Close();

                m_bOpenFile = true;
            }
        }
    }

    #endregion

    #region FastModeDataExcel

    public class FastModeDataExcel : FastModeDataSample
    {
        UInt16 m_uiCurrentRowNumber = 1;
        DataExcel m_de = null;

        public override void AddData(ArrayList arrayFastData)
        {
            Char cColumnLeter = 'A';

            foreach (double dTime in arrayFastData)
            {

                String MyTime = "{0:#0.0}";

                m_de.SetData(cColumnLeter + m_uiCurrentRowNumber.ToString(), String.Format(MyTime, dTime));

                m_uiCurrentRowNumber++;
            }
        }

        public override void CreateHeader()
        {
            FileInfo fi = new FileInfo(m_sysData.fastModeData.strFileName);

            m_uiCurrentRowNumber = 1;

            m_de = new DataExcel();

            if (File.Exists(m_sysData.fastModeData.strFileName))
            {
                m_de.Open(m_sysData.fastModeData.strFileName);
            }
            else
            {
                m_de.CreateWorkbook();
            }

            m_de.GetWorksheets();
            m_de.GetWorksheet();

            m_de.SetData("A" + m_uiCurrentRowNumber.ToString(), "*** " + m_sysData.applicationData.ProductName + " Measurement system, Version " + m_sysData.applicationData.ProductVersion + (m_sysData.Demo ? " Demo Version" : "") + " ***");
            m_uiCurrentRowNumber++;
            m_uiCurrentRowNumber++;
            m_uiCurrentRowNumber++;

            m_de.SetData("A" + m_uiCurrentRowNumber.ToString(), "File:");
            m_de.SetData("B" + m_uiCurrentRowNumber.ToString(), fi.Name.ToString());
            m_uiCurrentRowNumber++;

            m_de.SetData("A" + m_uiCurrentRowNumber.ToString(), "Date:");
            m_de.SetData("B" + m_uiCurrentRowNumber.ToString(), DateTime.Now.ToString("dd MMMM yyyy", CultureInfo.CreateSpecificCulture("en-US")));
            m_uiCurrentRowNumber++;

            m_de.SetData("A" + m_uiCurrentRowNumber.ToString(), "Time:");
            m_de.SetData("B" + m_uiCurrentRowNumber.ToString(), DateTime.Now.ToString("hh:mm:ss tt", CultureInfo.InvariantCulture));
            m_uiCurrentRowNumber++;
            m_uiCurrentRowNumber++;

            if ((m_sysData.applicationData.m_strUserTitle != null) && (m_sysData.applicationData.m_strUserTitle.Equals("") == false))
            {
                m_de.SetData("A" + m_uiCurrentRowNumber.ToString(), "User Data:");
                m_de.SetData("B" + m_uiCurrentRowNumber.ToString(), m_sysData.applicationData.m_strUserTitle.ToString());
                m_uiCurrentRowNumber++;
                m_uiCurrentRowNumber++;
            }

            m_de.SetData("A" + m_uiCurrentRowNumber.ToString(), "S/N:");
            m_de.SetData("B" + m_uiCurrentRowNumber.ToString(), m_sysData.applicationData.SystemNumber);
            m_uiCurrentRowNumber++;
            m_uiCurrentRowNumber++;

            Char cColumnLeter = 'A';

            m_de.SetData(cColumnLeter + m_uiCurrentRowNumber.ToString(), "Time");
            m_uiCurrentRowNumber++;

            m_de.SetData("A" + m_uiCurrentRowNumber.ToString(), "(sec)");
            m_uiCurrentRowNumber++;

            m_bOpenFile = true;
        }
    }

    #endregion

    public class FileFastModeData
    {
        public delegate void StopFastMode(object sender, EventArgs e);
        public event StopFastMode OnStopFastMode;

        private FastModeDataSample m_FastModeData = null;
        private SystemData m_sysData = SystemData.MyInstance;
        private ArrayList m_arrayFastData = null;

        private long m_dStartTime = 0;

        public void AddData(long Timestamp)
        {
            Double dDuration;

            if (m_arrayFastData == null)
            {
                m_arrayFastData = new ArrayList();
                m_dStartTime = Timestamp;
            }

//            dDuration = (Timestamp - m_dStartTime) / 1000000000f;
            dDuration = (Timestamp - m_dStartTime) / 1000f;

            m_arrayFastData.Add(dDuration);

            if (((m_sysData.fastModeData.ltMode == LogType.ltTime) && (dDuration >= m_sysData.fastModeData.LogDuration)) ||
                ((m_sysData.fastModeData.ltMode == LogType.ltPoints) && (m_arrayFastData.Count >= m_sysData.fastModeData.LogNumPoints)))
            {
                CreateFastModeDataFile();
                OnStopFastMode(this, new EventArgs());
            }
        }

        public void CreateFastModeDataFile()
        {
            if ((m_arrayFastData != null) && (m_arrayFastData.Count > 0))
            {
                if (m_sysData.logData.ftFile == FileType.ftLog)
                    m_FastModeData = new FastModeDataText();
                else if (m_sysData.logData.ftFile == FileType.ftXML)
                    m_FastModeData = new FastModeDataXML();
                else
                    m_FastModeData = new FastModeDataExcel();

                m_FastModeData.CreateHeader();

                m_FastModeData.AddData(m_arrayFastData);
            }
        }

        public static String ChangeExtension(String strFileName, FileType ftFile)
        {
            if ((strFileName != null) && (strFileName != ""))
            {
                int lastPathLocation = strFileName.LastIndexOf("\\");

                if ((lastPathLocation >= 0) && ((lastPathLocation + 1) < strFileName.Length))
                {
                    int lastLocation = strFileName.IndexOf(".", lastPathLocation, strFileName.Length - lastPathLocation - 1);
                    if (lastLocation >= 0) strFileName = strFileName.Substring(0, lastLocation);
                }

                if ((lastPathLocation + 1) < strFileName.Length)
                {
                    if (ftFile == FileType.ftLog)
                        strFileName += ".fst";
                    else if (ftFile == FileType.ftExcel)
                        strFileName += (DataExcel.Version < 12) ? ".xls" : ".xlsx";
                    else
                        strFileName += ".xml";
                }
            }

            return strFileName;
        }
    }
}

