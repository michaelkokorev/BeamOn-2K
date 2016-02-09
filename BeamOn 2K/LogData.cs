using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using System.Globalization;
using System.Xml.Xsl;
using System.Xml.XPath;

namespace BeamOn_2K
{
    #region LogStatistic

    public class LogStatistic
    {
        Double m_dMin = Double.MaxValue;
        Double m_dMax = Double.MinValue;
        Double m_dAverage = 0;
        Double m_dSTD = 0;
        UInt64 m_uiNumberMeasure = 0;
        Double m_dSumSqr = 0;

        public UInt64 NumMeasure
        {
            get { return m_uiNumberMeasure; }
        }

        public Double Min
        {
            get { return m_dMin; }
        }

        public Double Max
        {
            get { return m_dMax; }
        }

        public Double Average
        {
            get { return m_dAverage; }
        }

        public Double STD
        {
            get { return m_dSTD; }
        }

        public void AddData(Double dValue)
        {
            m_uiNumberMeasure++;

            if (dValue > m_dMax) m_dMax = dValue;
            if (dValue < m_dMin) m_dMin = dValue;

            m_dAverage = (m_dAverage * (m_uiNumberMeasure - 1) + dValue) / (Double)m_uiNumberMeasure;
            m_dSumSqr += dValue * dValue;

            if (m_uiNumberMeasure > 1)
            {
                m_dSTD = Math.Sqrt(Math.Abs(m_dSumSqr - m_uiNumberMeasure * m_dAverage * m_dAverage) / (Double)(m_uiNumberMeasure - 1));
            }

        }

        public void Reset()
        {
            m_dMin = Double.MaxValue;
            m_dMax = Double.MinValue;
            m_dAverage = 0;
            m_dSTD = 0;
            m_uiNumberMeasure = 0;
            m_dSumSqr = 0;
        }
    }

    #endregion

    #region LogDataSample

    public abstract class LogDataSample
    {
        protected SystemData m_sysData = SystemData.MyInstance;
        protected LogStatistics m_lsStatisticData = null;

        public LogDataSample()
        {
            m_lsStatisticData = new LogStatistics();
        }

        public UInt64 NumMeasure
        {
            get { return m_lsStatisticData.NumMeasure; }
        }

        public abstract void CreateHeader();
        public abstract void AddData(Double dTime = 0);
        public abstract void CloseLog();
        public abstract Boolean IsOpen();
    }

    #endregion

    #region LogDataXML

    public class LogDataXML : LogDataSample
    {
        Boolean m_bOpenFile = false;
        XmlTextWriter writer = null;

        public LogDataXML()
            : base()
        {
        }

        public override bool IsOpen()
        {
            return m_bOpenFile;
        }

        public override void CreateHeader()
        {
            FileInfo fi = new FileInfo(m_sysData.logData.strFileName);

            try
            {
                writer = new XmlTextWriter(m_sysData.logData.strFileName, System.Text.Encoding.ASCII);

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

                writer.WriteStartElement("Average");
                writer.WriteValue((m_sysData.AverageOn == true) ? m_sysData.Average.ToString() : "OFF");
                writer.WriteEndElement();

                writer.WriteStartElement("Wavelength");
                writer.WriteValue(m_sysData.powerData.uiWavelenght.ToString());
                writer.WriteEndElement();

                writer.WriteStartElement("Null");
                writer.WriteValue(m_sysData.powerData.bIndOffset.ToString());
                writer.WriteEndElement();

                if (m_sysData.powerData.bIndOffset == true)
                {
                    writer.WriteStartElement("Value");
                    writer.WriteValue(String.Format(PowerData.GetValueFormat(m_sysData.powerData.mwOffsetPower, 0), m_sysData.powerData.mwOffsetPower));
                    writer.WriteEndElement();
                }

                writer.WriteStartElement("Transmission");
                writer.WriteValue(String.Format("{0:F2}", m_sysData.powerData.realFilterFactor));
                writer.WriteEndElement();

                writer.WriteStartElement("SAM");
                writer.WriteValue(m_sysData.powerData.bIndSAM.ToString());
                writer.WriteEndElement();

                if (m_sysData.powerData.bIndSAM == true)
                {
                    writer.WriteStartElement(" SAM Transmission");
                    writer.WriteValue(String.Format("{0:F2}", m_sysData.powerData.realSAMFactor));
                    writer.WriteEndElement();
                }

                writer.WriteStartElement("Levels");

                for (int i = 0; i < m_sysData.ClipLevels.NumberLevels; i++)
                {
                    writer.WriteStartElement("L" + (i + 1).ToString());
                    writer.WriteValue(m_sysData.ClipLevels.Level(i));
                    writer.WriteEndElement();
                }

                writer.WriteEndElement();

                m_lsStatisticData.Reset();
                m_bOpenFile = true;

                writer.WriteEndElement();

                writer.WriteStartElement("Data");
                m_bOpenFile = true;
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

        public override void AddData(Double dTime)
        {
            String MyTime = "";

            m_lsStatisticData.AddData();

            m_sysData.powerData.mwPower = (m_sysData.powerData.Power / m_sysData.powerData.realFilterFactor) / m_sysData.powerData.currentSAMFactor - m_sysData.powerData.mwOffsetPower * Math.Abs(Convert.ToInt16(m_sysData.powerData.bIndOffset));

            if ((m_sysData.powerData.mwPower < 0) && (m_sysData.powerData.bIndOffset == false)) m_sysData.powerData.mwPower = 0;

            MyTime = (m_sysData.logData.LogInterval == 0) ? "{0:#0.0}" : "{0:#0}";

            try
            {
                if ((writer != null) && (IsOpen() == true))
                {
                    writer.WriteStartElement("Measure");

                    writer.WriteStartElement("Time");
                    writer.WriteValue(String.Format(MyTime, dTime));
                    writer.WriteEndElement();

                    if (m_sysData.logData.bPower == true)
                    {
                        writer.WriteStartElement("Power");
                        writer.WriteValue(PowerData.GetValueStringFormat(m_sysData.powerData.mwPower, 0));
                        writer.WriteEndElement();
                    }

                    if ((m_sysData.logData.bPositionX == true) || (m_sysData.logData.bPositionY == true))
                    {
                        writer.WriteStartElement("Position");

                        if (m_sysData.logData.bPositionX == true)
                        {
                            writer.WriteStartElement("X");
                            writer.WriteValue(m_sysData.positionData.strPositionX);
                            writer.WriteEndElement();
                        }

                        if (m_sysData.logData.bPositionY == true)
                        {
                            writer.WriteStartElement("Y");
                            writer.WriteValue(m_sysData.positionData.strPositionY);
                            writer.WriteEndElement();
                        }

                        writer.WriteEndElement();
                    }

                    if ((m_sysData.logData.bVerticalProfileWidthLevel1 == true) || (m_sysData.logData.bVerticalProfileWidthLevel2 == true) || (m_sysData.logData.bVerticalProfileWidthLevel3 == true) ||
                        (m_sysData.logData.bHorizontalProfileWidthLevel1 == true) || (m_sysData.logData.bHorizontalProfileWidthLevel2 == true) || (m_sysData.logData.bHorizontalProfileWidthLevel3 == true))
                    {
                        writer.WriteStartElement("Widths");

                        if ((m_sysData.logData.bHorizontalProfileWidthLevel1 == true) || (m_sysData.logData.bHorizontalProfileWidthLevel2 == true) || (m_sysData.logData.bHorizontalProfileWidthLevel3 == true))
                        {
                            writer.WriteStartElement("W");
                            {
                                if (m_sysData.logData.bHorizontalProfileWidthLevel1 == true)
                                {
                                    writer.WriteStartElement("W1");
                                    writer.WriteValue(m_sysData.HorizontalProfile.strWidth[0]);
                                    writer.WriteEndElement();
                                }

                                if (m_sysData.logData.bHorizontalProfileWidthLevel2 == true)
                                {
                                    writer.WriteStartElement("W2");
                                    writer.WriteValue(m_sysData.HorizontalProfile.strWidth[1]);
                                    writer.WriteEndElement();
                                }

                                if (m_sysData.logData.bHorizontalProfileWidthLevel3 == true)
                                {
                                    writer.WriteStartElement("W3");
                                    writer.WriteValue(m_sysData.HorizontalProfile.strWidth[2]);
                                    writer.WriteEndElement();
                                }
                            }
                            writer.WriteEndElement();
                        }

                        if ((m_sysData.logData.bVerticalProfileWidthLevel1 == true) || (m_sysData.logData.bVerticalProfileWidthLevel2 == true) || (m_sysData.logData.bVerticalProfileWidthLevel3 == true))
                        {
                            writer.WriteStartElement("V");
                            {
                                if (m_sysData.logData.bVerticalProfileWidthLevel1 == true)
                                {
                                    writer.WriteStartElement("V1");
                                    writer.WriteValue(m_sysData.VerticalProfile.strWidth[0]);
                                    writer.WriteEndElement();
                                }

                                if (m_sysData.logData.bVerticalProfileWidthLevel2 == true)
                                {
                                    writer.WriteStartElement("V2");
                                    writer.WriteValue(m_sysData.VerticalProfile.strWidth[1]);
                                    writer.WriteEndElement();
                                }

                                if (m_sysData.logData.bVerticalProfileWidthLevel3 == true)
                                {
                                    writer.WriteStartElement("V3");
                                    writer.WriteValue(m_sysData.VerticalProfile.strWidth[2]);
                                    writer.WriteEndElement();
                                }
                            }
                            writer.WriteEndElement();
                        }
                        writer.WriteEndElement();
                    }

                    if ((m_sysData.logData.bMajor == true) || (m_sysData.logData.bMinor == true) || (m_sysData.logData.bOrientation == true))
                    {
                        writer.WriteStartElement("EllipseData");

                        if (m_sysData.logData.bMajor == true)
                        {
                            writer.WriteStartElement("Major");

                            if (m_sysData.positionData.Ellipse.EllipseError == false)
                                writer.WriteValue(m_sysData.positionData.strMajor);
                            else
                                writer.WriteValue("-----");

                            writer.WriteEndElement();
                        }

                        if (m_sysData.logData.bMinor == true)
                        {
                            writer.WriteStartElement("Minor");

                            if (m_sysData.positionData.Ellipse.EllipseError == false)
                                writer.WriteValue(m_sysData.positionData.strMinor);
                            else
                                writer.WriteValue("-----");

                            writer.WriteEndElement();
                        }

                        if (m_sysData.logData.bOrientation == true)
                        {
                            writer.WriteStartElement("Orientation");

                            if (m_sysData.positionData.Ellipse.EllipseError == false)
                                writer.WriteValue(String.Format("{0:#0.000}", m_sysData.positionData.Ellipse.Orientation));
                            else
                                writer.WriteValue("-----");

                            writer.WriteEndElement();
                        }

                        writer.WriteEndElement();
                    }

                    if ((m_sysData.logData.bVerticalGaussianFit == true) || (m_sysData.logData.bHorizontalGaussianFit == true))
                    {
                        writer.WriteStartElement("Correlation");
                        {
                            if (m_sysData.logData.bHorizontalGaussianFit == true)
                            {
                                writer.WriteStartElement("W");
                                writer.WriteValue(String.Format("{0:#0.000}", m_sysData.HorizontalProfile.m_fCorrelation));
                                writer.WriteEndElement();
                            }

                            if (m_sysData.logData.bVerticalGaussianFit == true)
                            {
                                writer.WriteStartElement("V");
                                writer.WriteValue(String.Format("{0:#0.000}", m_sysData.VerticalProfile.m_fCorrelation));
                                writer.WriteEndElement();
                            }
                        }
                        writer.WriteEndElement();
                    }
                    if ((m_sysData.logData.bVerticalGaussianWidthLevel1 == true) || (m_sysData.logData.bVerticalGaussianWidthLevel2 == true) || (m_sysData.logData.bVerticalGaussianWidthLevel3 == true) ||
                        (m_sysData.logData.bHorizontalGaussianWidthLevel1 == true) || (m_sysData.logData.bHorizontalGaussianWidthLevel2 == true) || (m_sysData.logData.bHorizontalGaussianWidthLevel3 == true))
                    {
                        writer.WriteStartElement("GaussWidths");

                        if ((m_sysData.logData.bHorizontalGaussianWidthLevel1 == true) || (m_sysData.logData.bHorizontalGaussianWidthLevel2 == true) || (m_sysData.logData.bHorizontalGaussianWidthLevel3 == true))
                        {
                            writer.WriteStartElement("W");
                            {
                                if (m_sysData.logData.bHorizontalGaussianWidthLevel1 == true)
                                {
                                    writer.WriteStartElement("W1");
                                    writer.WriteValue(m_sysData.HorizontalProfile.strGaussWidth[0]);
                                    writer.WriteEndElement();
                                }

                                if (m_sysData.logData.bHorizontalGaussianWidthLevel2 == true)
                                {
                                    writer.WriteStartElement("W2");
                                    writer.WriteValue(m_sysData.HorizontalProfile.strGaussWidth[1]);
                                    writer.WriteEndElement();
                                }

                                if (m_sysData.logData.bHorizontalGaussianWidthLevel3 == true)
                                {
                                    writer.WriteStartElement("W3");
                                    writer.WriteValue(m_sysData.HorizontalProfile.strGaussWidth[2]);
                                    writer.WriteEndElement();
                                }
                            }
                            writer.WriteEndElement();
                        }

                        if ((m_sysData.logData.bVerticalGaussianWidthLevel1 == true) || (m_sysData.logData.bVerticalGaussianWidthLevel2 == true) || (m_sysData.logData.bVerticalGaussianWidthLevel3 == true))
                        {
                            writer.WriteStartElement("V");
                            {
                                if (m_sysData.logData.bVerticalGaussianWidthLevel1 == true)
                                {
                                    writer.WriteStartElement("V1");
                                    writer.WriteValue(m_sysData.VerticalProfile.strGaussWidth[0]);
                                    writer.WriteEndElement();
                                }

                                if (m_sysData.logData.bVerticalGaussianWidthLevel2 == true)
                                {
                                    writer.WriteStartElement("V2");
                                    writer.WriteValue(m_sysData.VerticalProfile.strGaussWidth[1]);
                                    writer.WriteEndElement();
                                }

                                if (m_sysData.logData.bVerticalGaussianWidthLevel3 == true)
                                {
                                    writer.WriteStartElement("V3");
                                    writer.WriteValue(m_sysData.VerticalProfile.strGaussWidth[2]);
                                    writer.WriteEndElement();
                                }
                            }
                            writer.WriteEndElement();
                        }
                        writer.WriteEndElement();
                    }

                    writer.WriteEndElement();
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

        public override void CloseLog()
        {
            try
            {
                if ((writer != null) && (IsOpen() == true))
                {
                    writer.WriteEndElement();

                    writer.WriteStartElement("Statistics");
                    {
                        writer.WriteStartElement("Min");
                        {

                            if (m_sysData.logData.bPower == true)
                            {
                                writer.WriteStartElement("Power");
                                {
                                    writer.WriteValue(PowerData.GetValueStringFormat((Single)m_lsStatisticData.m_lsPower.Min, 0));
                                }
                                writer.WriteEndElement();
                            }

                            if ((m_sysData.logData.bPositionX == true) || (m_sysData.logData.bPositionY == true))
                            {
                                writer.WriteStartElement("Position");
                                {

                                    if (m_sysData.logData.bPositionX == true)
                                    {
                                        writer.WriteStartElement("X");
                                        {
                                            writer.WriteValue(String.Format("{0:#0.000}", m_lsStatisticData.m_lsPositionX.Min * m_sysData.UnitsCoeff));
                                        }
                                        writer.WriteEndElement();
                                    }

                                    if (m_sysData.logData.bPositionY == true)
                                    {
                                        writer.WriteStartElement("Y");
                                        {
                                            writer.WriteValue(String.Format("{0:#0.000}", m_lsStatisticData.m_lsPositionY.Min * m_sysData.UnitsCoeff));
                                        }
                                        writer.WriteEndElement();
                                    }
                                }
                                writer.WriteEndElement();
                            }

                            if ((m_sysData.logData.bHorizontalProfileWidthLevel1 == true) || (m_sysData.logData.bHorizontalProfileWidthLevel2 == true) || (m_sysData.logData.bHorizontalProfileWidthLevel3 == true) ||
                                (m_sysData.logData.bVerticalProfileWidthLevel1 == true) || (m_sysData.logData.bVerticalProfileWidthLevel2 == true) || (m_sysData.logData.bVerticalProfileWidthLevel3 == true))
                            {
                                writer.WriteStartElement("Widths");
                                {
                                    if ((m_sysData.logData.bHorizontalProfileWidthLevel1 == true) || (m_sysData.logData.bHorizontalProfileWidthLevel2 == true) || (m_sysData.logData.bHorizontalProfileWidthLevel3 == true))
                                    {
                                        writer.WriteStartElement("W");
                                        {
                                            if (m_sysData.logData.bHorizontalProfileWidthLevel1 == true)
                                            {
                                                writer.WriteStartElement("W1");
                                                {
                                                    writer.WriteValue(String.Format("{0:#0.000}", m_lsStatisticData.m_lsWidthV[0].Min * m_sysData.UnitsCoeff));
                                                }
                                                writer.WriteEndElement();
                                            }

                                            if (m_sysData.logData.bHorizontalProfileWidthLevel2 == true)
                                            {
                                                writer.WriteStartElement("W2");
                                                {
                                                    writer.WriteValue(String.Format("{0:#0.000}", m_lsStatisticData.m_lsWidthV[1].Min * m_sysData.UnitsCoeff));
                                                }
                                                writer.WriteEndElement();
                                            }

                                            if (m_sysData.logData.bHorizontalProfileWidthLevel3 == true)
                                            {
                                                writer.WriteStartElement("W3");
                                                {
                                                    writer.WriteValue(String.Format("{0:#0.000}", m_lsStatisticData.m_lsWidthV[2].Min * m_sysData.UnitsCoeff));
                                                }
                                                writer.WriteEndElement();
                                            }
                                        }
                                        writer.WriteEndElement();
                                    }

                                    if ((m_sysData.logData.bVerticalProfileWidthLevel1 == true) || (m_sysData.logData.bVerticalProfileWidthLevel2 == true) || (m_sysData.logData.bVerticalProfileWidthLevel3 == true))
                                    {
                                        writer.WriteStartElement("V");
                                        {

                                            if (m_sysData.logData.bVerticalProfileWidthLevel1 == true)
                                            {
                                                writer.WriteStartElement("V1");
                                                {
                                                    writer.WriteValue(String.Format("{0:#0.000}", m_lsStatisticData.m_lsWidthW[0].Min * m_sysData.UnitsCoeff));
                                                }
                                                writer.WriteEndElement();
                                            }

                                            if (m_sysData.logData.bVerticalProfileWidthLevel2 == true)
                                            {
                                                writer.WriteStartElement("V2");
                                                {
                                                    writer.WriteValue(String.Format("{0:#0.000}", m_lsStatisticData.m_lsWidthW[1].Min * m_sysData.UnitsCoeff));
                                                }
                                                writer.WriteEndElement();
                                            }

                                            if (m_sysData.logData.bVerticalProfileWidthLevel3 == true)
                                            {
                                                writer.WriteStartElement("V3");
                                                {
                                                    writer.WriteValue(String.Format("{0:#0.000}", m_lsStatisticData.m_lsWidthW[2].Min * m_sysData.UnitsCoeff));
                                                }
                                                writer.WriteEndElement();
                                            }
                                        }
                                        writer.WriteEndElement();
                                    }
                                }
                                writer.WriteEndElement();
                            }

                            if ((m_sysData.logData.bMajor == true) || (m_sysData.logData.bMinor == true) || (m_sysData.logData.bOrientation == true))
                            {
                                writer.WriteStartElement("EllipseData");
                                {
                                    if (m_sysData.logData.bMajor == true)
                                    {
                                        writer.WriteStartElement("Major");
                                        {
                                            writer.WriteValue(String.Format("{0:#0.000}", m_lsStatisticData.m_lsMajor.Min * m_sysData.UnitsCoeff));
                                        }
                                        writer.WriteEndElement();
                                    }

                                    if (m_sysData.logData.bMinor == true)
                                    {
                                        writer.WriteStartElement("Minor");
                                        {
                                            writer.WriteValue(String.Format("{0:#0.000}", m_lsStatisticData.m_lsMinor.Min * m_sysData.UnitsCoeff));
                                        }
                                        writer.WriteEndElement();
                                    }

                                    if (m_sysData.logData.bOrientation == true)
                                    {
                                        writer.WriteStartElement("Orientation");
                                        {
                                            writer.WriteValue(String.Format("{0:#0.000}", m_lsStatisticData.m_lsOrientation.Min));
                                        }
                                        writer.WriteEndElement();
                                    }
                                }
                                writer.WriteEndElement();
                            }

                            if ((m_sysData.logData.bVerticalGaussianFit == true) || (m_sysData.logData.bHorizontalGaussianFit == true))
                            {
                                writer.WriteStartElement("Correlation");
                                {
                                    if (m_sysData.logData.bHorizontalGaussianFit == true)
                                    {
                                        writer.WriteStartElement("W");
                                        writer.WriteValue(String.Format("{0:#0.000}", m_lsStatisticData.m_lsCorrelationV.Min));
                                        writer.WriteEndElement();
                                    }

                                    if (m_sysData.logData.bVerticalGaussianFit == true)
                                    {
                                        writer.WriteStartElement("V");
                                        writer.WriteValue(String.Format("{0:#0.000}", m_lsStatisticData.m_lsCorrelationW.Min));
                                        writer.WriteEndElement();
                                    }
                                }
                                writer.WriteEndElement();
                            }
                            if ((m_sysData.logData.bHorizontalGaussianWidthLevel1 == true) || (m_sysData.logData.bHorizontalGaussianWidthLevel2 == true) || (m_sysData.logData.bHorizontalGaussianWidthLevel3 == true) ||
                                (m_sysData.logData.bVerticalGaussianWidthLevel1 == true) || (m_sysData.logData.bVerticalGaussianWidthLevel2 == true) || (m_sysData.logData.bVerticalGaussianWidthLevel3 == true))
                            {
                                writer.WriteStartElement("GaussWidths");
                                {
                                    if ((m_sysData.logData.bHorizontalGaussianWidthLevel1 == true) || (m_sysData.logData.bHorizontalGaussianWidthLevel2 == true) || (m_sysData.logData.bHorizontalGaussianWidthLevel3 == true))
                                    {
                                        writer.WriteStartElement("W");
                                        {
                                            if (m_sysData.logData.bHorizontalGaussianWidthLevel1 == true)
                                            {
                                                writer.WriteStartElement("W1");
                                                {
                                                    writer.WriteValue(String.Format("{0:#0.000}", m_lsStatisticData.m_lsGaussWidthV[0].Min * m_sysData.UnitsCoeff));
                                                }
                                                writer.WriteEndElement();
                                            }

                                            if (m_sysData.logData.bHorizontalGaussianWidthLevel2 == true)
                                            {
                                                writer.WriteStartElement("W2");
                                                {
                                                    writer.WriteValue(String.Format("{0:#0.000}", m_lsStatisticData.m_lsGaussWidthV[1].Min * m_sysData.UnitsCoeff));
                                                }
                                                writer.WriteEndElement();
                                            }

                                            if (m_sysData.logData.bHorizontalGaussianWidthLevel3 == true)
                                            {
                                                writer.WriteStartElement("W3");
                                                {
                                                    writer.WriteValue(String.Format("{0:#0.000}", m_lsStatisticData.m_lsGaussWidthV[2].Min * m_sysData.UnitsCoeff));
                                                }
                                                writer.WriteEndElement();
                                            }
                                        }
                                        writer.WriteEndElement();
                                    }

                                    if ((m_sysData.logData.bVerticalGaussianWidthLevel1 == true) || (m_sysData.logData.bVerticalGaussianWidthLevel2 == true) || (m_sysData.logData.bVerticalGaussianWidthLevel3 == true))
                                    {
                                        writer.WriteStartElement("V");
                                        {

                                            if (m_sysData.logData.bVerticalGaussianWidthLevel1 == true)
                                            {
                                                writer.WriteStartElement("V1");
                                                {
                                                    writer.WriteValue(String.Format("{0:#0.000}", m_lsStatisticData.m_lsGaussWidthW[0].Min * m_sysData.UnitsCoeff));
                                                }
                                                writer.WriteEndElement();
                                            }

                                            if (m_sysData.logData.bVerticalGaussianWidthLevel2 == true)
                                            {
                                                writer.WriteStartElement("V2");
                                                {
                                                    writer.WriteValue(String.Format("{0:#0.000}", m_lsStatisticData.m_lsGaussWidthW[1].Min * m_sysData.UnitsCoeff));
                                                }
                                                writer.WriteEndElement();
                                            }

                                            if (m_sysData.logData.bVerticalGaussianWidthLevel3 == true)
                                            {
                                                writer.WriteStartElement("V3");
                                                {
                                                    writer.WriteValue(String.Format("{0:#0.000}", m_lsStatisticData.m_lsGaussWidthW[2].Min * m_sysData.UnitsCoeff));
                                                }
                                                writer.WriteEndElement();
                                            }
                                        }
                                        writer.WriteEndElement();
                                    }
                                }
                                writer.WriteEndElement();
                            }
                        }
                        writer.WriteEndElement();

                        writer.WriteStartElement("Max");
                        {
                            if (m_sysData.logData.bPower == true)
                            {
                                writer.WriteStartElement("Power");
                                {
                                    writer.WriteValue(PowerData.GetValueStringFormat((Single)m_lsStatisticData.m_lsPower.Max, 0));
                                }
                                writer.WriteEndElement();
                            }

                            if ((m_sysData.logData.bPositionX == true) || (m_sysData.logData.bPositionY == true))
                            {
                                writer.WriteStartElement("Position");
                                {
                                    if (m_sysData.logData.bPositionX == true)
                                    {
                                        writer.WriteStartElement("X");
                                        {
                                            writer.WriteValue(String.Format("{0:#0.000}", m_lsStatisticData.m_lsPositionX.Max * m_sysData.UnitsCoeff));
                                        }
                                        writer.WriteEndElement();
                                    }

                                    if (m_sysData.logData.bPositionY == true)
                                    {
                                        writer.WriteStartElement("Y");
                                        {
                                            writer.WriteValue(String.Format("{0:#0.000}", m_lsStatisticData.m_lsPositionY.Max * m_sysData.UnitsCoeff));
                                        }
                                        writer.WriteEndElement();
                                    }
                                }
                                writer.WriteEndElement();
                            }

                            if ((m_sysData.logData.bHorizontalProfileWidthLevel1 == true) || (m_sysData.logData.bHorizontalProfileWidthLevel2 == true) || (m_sysData.logData.bHorizontalProfileWidthLevel3 == true) ||
                                (m_sysData.logData.bVerticalProfileWidthLevel1 == true) || (m_sysData.logData.bVerticalProfileWidthLevel2 == true) || (m_sysData.logData.bVerticalProfileWidthLevel3 == true))
                            {
                                writer.WriteStartElement("Widths");
                                {
                                    if ((m_sysData.logData.bHorizontalProfileWidthLevel1 == true) || (m_sysData.logData.bHorizontalProfileWidthLevel2 == true) || (m_sysData.logData.bHorizontalProfileWidthLevel3 == true))
                                    {
                                        writer.WriteStartElement("W");
                                        {
                                            if (m_sysData.logData.bHorizontalProfileWidthLevel1 == true)
                                            {
                                                writer.WriteStartElement("W1");
                                                {
                                                    writer.WriteValue(String.Format("{0:#0.000}", m_lsStatisticData.m_lsWidthV[0].Max * m_sysData.UnitsCoeff));
                                                }
                                                writer.WriteEndElement();
                                            }

                                            if (m_sysData.logData.bHorizontalProfileWidthLevel2 == true)
                                            {
                                                writer.WriteStartElement("W2");
                                                {
                                                    writer.WriteValue(String.Format("{0:#0.000}", m_lsStatisticData.m_lsWidthV[1].Max * m_sysData.UnitsCoeff));
                                                }
                                                writer.WriteEndElement();
                                            }

                                            if (m_sysData.logData.bHorizontalProfileWidthLevel3 == true)
                                            {
                                                writer.WriteStartElement("W3");
                                                {
                                                    writer.WriteValue(String.Format("{0:#0.000}", m_lsStatisticData.m_lsWidthV[2].Max * m_sysData.UnitsCoeff));
                                                }
                                                writer.WriteEndElement();
                                            }
                                        }
                                        writer.WriteEndElement();
                                    }

                                    if ((m_sysData.logData.bVerticalProfileWidthLevel1 == true) || (m_sysData.logData.bVerticalProfileWidthLevel2 == true) || (m_sysData.logData.bVerticalProfileWidthLevel3 == true))
                                    {
                                        writer.WriteStartElement("V");
                                        {
                                            if (m_sysData.logData.bVerticalProfileWidthLevel1 == true)
                                            {
                                                writer.WriteStartElement("V1");
                                                {
                                                    writer.WriteValue(String.Format("{0:#0.000}", m_lsStatisticData.m_lsWidthW[0].Max * m_sysData.UnitsCoeff));
                                                }
                                                writer.WriteEndElement();
                                            }

                                            if (m_sysData.logData.bVerticalProfileWidthLevel2 == true)
                                            {
                                                writer.WriteStartElement("V2");
                                                {
                                                    writer.WriteValue(String.Format("{0:#0.000}", m_lsStatisticData.m_lsWidthW[1].Max * m_sysData.UnitsCoeff));
                                                }
                                                writer.WriteEndElement();
                                            }

                                            if (m_sysData.logData.bVerticalProfileWidthLevel3 == true)
                                            {
                                                writer.WriteStartElement("V3");
                                                {
                                                    writer.WriteValue(String.Format("{0:#0.000}", m_lsStatisticData.m_lsWidthW[2].Max * m_sysData.UnitsCoeff));
                                                }
                                                writer.WriteEndElement();
                                            }
                                        }
                                        writer.WriteEndElement();
                                    }
                                }
                                writer.WriteEndElement();
                            }

                            if ((m_sysData.logData.bMajor == true) || (m_sysData.logData.bMinor == true) || (m_sysData.logData.bOrientation == true))
                            {
                                writer.WriteStartElement("EllipseData");
                                {
                                    if (m_sysData.logData.bMajor == true)
                                    {
                                        writer.WriteStartElement("Major");
                                        {
                                            writer.WriteValue(String.Format("{0:#0.000}", m_lsStatisticData.m_lsMajor.Max * m_sysData.UnitsCoeff));
                                        }
                                        writer.WriteEndElement();
                                    }

                                    if (m_sysData.logData.bMinor == true)
                                    {
                                        writer.WriteStartElement("Minor");
                                        {
                                            writer.WriteValue(String.Format("{0:#0.000}", m_lsStatisticData.m_lsMinor.Max * m_sysData.UnitsCoeff));
                                        }
                                        writer.WriteEndElement();
                                    }

                                    if (m_sysData.logData.bOrientation == true)
                                    {
                                        writer.WriteStartElement("Orientation");
                                        {
                                            writer.WriteValue(String.Format("{0:#0.000}", m_lsStatisticData.m_lsOrientation.Max));
                                        }
                                        writer.WriteEndElement();
                                    }
                                }
                                writer.WriteEndElement();
                            }

                            if ((m_sysData.logData.bVerticalGaussianFit == true) || (m_sysData.logData.bHorizontalGaussianFit == true))
                            {
                                writer.WriteStartElement("Correlation");
                                {
                                    if (m_sysData.logData.bHorizontalGaussianFit == true)
                                    {
                                        writer.WriteStartElement("W");
                                        writer.WriteValue(String.Format("{0:#0.000}", m_lsStatisticData.m_lsCorrelationV.Max));
                                        writer.WriteEndElement();
                                    }

                                    if (m_sysData.logData.bVerticalGaussianFit == true)
                                    {
                                        writer.WriteStartElement("V");
                                        writer.WriteValue(String.Format("{0:#0.000}", m_lsStatisticData.m_lsCorrelationW.Max));
                                        writer.WriteEndElement();
                                    }
                                }
                                writer.WriteEndElement();
                            }

                            if ((m_sysData.logData.bHorizontalGaussianWidthLevel1 == true) || (m_sysData.logData.bHorizontalGaussianWidthLevel2 == true) || (m_sysData.logData.bHorizontalGaussianWidthLevel3 == true) ||
                                (m_sysData.logData.bVerticalGaussianWidthLevel1 == true) || (m_sysData.logData.bVerticalGaussianWidthLevel2 == true) || (m_sysData.logData.bVerticalGaussianWidthLevel3 == true))
                            {
                                writer.WriteStartElement("GaussWidths");
                                {
                                    if ((m_sysData.logData.bHorizontalGaussianWidthLevel1 == true) || (m_sysData.logData.bHorizontalGaussianWidthLevel2 == true) || (m_sysData.logData.bHorizontalGaussianWidthLevel3 == true))
                                    {
                                        writer.WriteStartElement("W");
                                        {
                                            if (m_sysData.logData.bHorizontalGaussianWidthLevel1 == true)
                                            {
                                                writer.WriteStartElement("W1");
                                                {
                                                    writer.WriteValue(String.Format("{0:#0.000}", m_lsStatisticData.m_lsGaussWidthV[0].Max * m_sysData.UnitsCoeff));
                                                }
                                                writer.WriteEndElement();
                                            }

                                            if (m_sysData.logData.bHorizontalGaussianWidthLevel2 == true)
                                            {
                                                writer.WriteStartElement("W2");
                                                {
                                                    writer.WriteValue(String.Format("{0:#0.000}", m_lsStatisticData.m_lsGaussWidthV[1].Max * m_sysData.UnitsCoeff));
                                                }
                                                writer.WriteEndElement();
                                            }

                                            if (m_sysData.logData.bHorizontalGaussianWidthLevel3 == true)
                                            {
                                                writer.WriteStartElement("W3");
                                                {
                                                    writer.WriteValue(String.Format("{0:#0.000}", m_lsStatisticData.m_lsGaussWidthV[2].Max * m_sysData.UnitsCoeff));
                                                }
                                                writer.WriteEndElement();
                                            }
                                        }
                                        writer.WriteEndElement();
                                    }

                                    if ((m_sysData.logData.bVerticalGaussianWidthLevel1 == true) || (m_sysData.logData.bVerticalGaussianWidthLevel2 == true) || (m_sysData.logData.bVerticalGaussianWidthLevel3 == true))
                                    {
                                        writer.WriteStartElement("V");
                                        {
                                            if (m_sysData.logData.bVerticalGaussianWidthLevel1 == true)
                                            {
                                                writer.WriteStartElement("V1");
                                                {
                                                    writer.WriteValue(String.Format("{0:#0.000}", m_lsStatisticData.m_lsGaussWidthW[0].Max * m_sysData.UnitsCoeff));
                                                }
                                                writer.WriteEndElement();
                                            }

                                            if (m_sysData.logData.bVerticalGaussianWidthLevel2 == true)
                                            {
                                                writer.WriteStartElement("V2");
                                                {
                                                    writer.WriteValue(String.Format("{0:#0.000}", m_lsStatisticData.m_lsGaussWidthW[1].Max * m_sysData.UnitsCoeff));
                                                }
                                                writer.WriteEndElement();
                                            }

                                            if (m_sysData.logData.bVerticalGaussianWidthLevel3 == true)
                                            {
                                                writer.WriteStartElement("V3");
                                                {
                                                    writer.WriteValue(String.Format("{0:#0.000}", m_lsStatisticData.m_lsGaussWidthW[2].Max * m_sysData.UnitsCoeff));
                                                }
                                                writer.WriteEndElement();
                                            }
                                        }
                                        writer.WriteEndElement();
                                    }
                                }
                                writer.WriteEndElement();
                            }
                        }
                        writer.WriteEndElement();

                        writer.WriteStartElement("Average");
                        {
                            if (m_sysData.logData.bPower == true)
                            {
                                writer.WriteStartElement("Power");
                                {
                                    writer.WriteValue(PowerData.GetValueStringFormat((Single)m_lsStatisticData.m_lsPower.Average, 0));
                                }
                                writer.WriteEndElement();
                            }

                            if ((m_sysData.logData.bPositionX == true) || (m_sysData.logData.bPositionY == true))
                            {
                                writer.WriteStartElement("Position");
                                {
                                    if (m_sysData.logData.bPositionX == true)
                                    {
                                        writer.WriteStartElement("X");
                                        {
                                            writer.WriteValue(String.Format("{0:#0.000}", m_lsStatisticData.m_lsPositionX.Average * m_sysData.UnitsCoeff));
                                        }
                                        writer.WriteEndElement();
                                    }

                                    if (m_sysData.logData.bPositionY == true)
                                    {
                                        writer.WriteStartElement("Y");
                                        {
                                            writer.WriteValue(String.Format("{0:#0.000}", m_lsStatisticData.m_lsPositionY.Average * m_sysData.UnitsCoeff));
                                        }
                                        writer.WriteEndElement();
                                    }
                                }
                                writer.WriteEndElement();
                            }

                            if ((m_sysData.logData.bHorizontalProfileWidthLevel1 == true) || (m_sysData.logData.bHorizontalProfileWidthLevel2 == true) || (m_sysData.logData.bHorizontalProfileWidthLevel3 == true) ||
                                (m_sysData.logData.bVerticalProfileWidthLevel1 == true) || (m_sysData.logData.bVerticalProfileWidthLevel2 == true) || (m_sysData.logData.bVerticalProfileWidthLevel3 == true))
                            {
                                writer.WriteStartElement("Widths");
                                {
                                    if ((m_sysData.logData.bHorizontalProfileWidthLevel1 == true) || (m_sysData.logData.bHorizontalProfileWidthLevel2 == true) || (m_sysData.logData.bHorizontalProfileWidthLevel3 == true))
                                    {
                                        writer.WriteStartElement("W");
                                        {
                                            if (m_sysData.logData.bHorizontalProfileWidthLevel1 == true)
                                            {
                                                writer.WriteStartElement("W1");
                                                {
                                                    writer.WriteValue(String.Format("{0:#0.000}", m_lsStatisticData.m_lsWidthV[0].Average * m_sysData.UnitsCoeff));
                                                }
                                                writer.WriteEndElement();
                                            }

                                            if (m_sysData.logData.bHorizontalProfileWidthLevel2 == true)
                                            {
                                                writer.WriteStartElement("W2");
                                                {
                                                    writer.WriteValue(String.Format("{0:#0.000}", m_lsStatisticData.m_lsWidthV[1].Average * m_sysData.UnitsCoeff));
                                                }
                                                writer.WriteEndElement();
                                            }

                                            if (m_sysData.logData.bHorizontalProfileWidthLevel3 == true)
                                            {
                                                writer.WriteStartElement("W3");
                                                {
                                                    writer.WriteValue(String.Format("{0:#0.000}", m_lsStatisticData.m_lsWidthV[2].Average * m_sysData.UnitsCoeff));
                                                }
                                                writer.WriteEndElement();
                                            }
                                        }
                                        writer.WriteEndElement();
                                    }

                                    if ((m_sysData.logData.bVerticalProfileWidthLevel1 == true) || (m_sysData.logData.bVerticalProfileWidthLevel2 == true) || (m_sysData.logData.bVerticalProfileWidthLevel3 == true))
                                    {
                                        writer.WriteStartElement("V");
                                        {
                                            if (m_sysData.logData.bVerticalProfileWidthLevel1 == true)
                                            {
                                                writer.WriteStartElement("V1");
                                                {
                                                    writer.WriteValue(String.Format("{0:#0.000}", m_lsStatisticData.m_lsWidthW[0].Average * m_sysData.UnitsCoeff));
                                                }
                                                writer.WriteEndElement();
                                            }

                                            if (m_sysData.logData.bVerticalProfileWidthLevel2 == true)
                                            {
                                                writer.WriteStartElement("V2");
                                                {
                                                    writer.WriteValue(String.Format("{0:#0.000}", m_lsStatisticData.m_lsWidthW[1].Average * m_sysData.UnitsCoeff));
                                                }
                                                writer.WriteEndElement();
                                            }

                                            if (m_sysData.logData.bVerticalProfileWidthLevel3 == true)
                                            {
                                                writer.WriteStartElement("V3");
                                                {
                                                    writer.WriteValue(String.Format("{0:#0.000}", m_lsStatisticData.m_lsWidthW[2].Average * m_sysData.UnitsCoeff));
                                                }
                                                writer.WriteEndElement();
                                            }
                                        }
                                        writer.WriteEndElement();
                                    }
                                }
                                writer.WriteEndElement();
                            }

                            if ((m_sysData.logData.bMajor == true) || (m_sysData.logData.bMinor == true) || (m_sysData.logData.bOrientation == true))
                            {
                                writer.WriteStartElement("EllipseData");
                                {
                                    if (m_sysData.logData.bMajor == true)
                                    {
                                        writer.WriteStartElement("Major");
                                        {
                                            writer.WriteValue(String.Format("{0:#0.000}", m_lsStatisticData.m_lsMajor.Average * m_sysData.UnitsCoeff));
                                        }
                                        writer.WriteEndElement();
                                    }

                                    if (m_sysData.logData.bMinor == true)
                                    {
                                        writer.WriteStartElement("Minor");
                                        {
                                            writer.WriteValue(String.Format("{0:#0.000}", m_lsStatisticData.m_lsMinor.Average * m_sysData.UnitsCoeff));
                                        }
                                        writer.WriteEndElement();
                                    }

                                    if (m_sysData.logData.bOrientation == true)
                                    {
                                        writer.WriteStartElement("Orientation");
                                        {
                                            writer.WriteValue(String.Format("{0:#0.000}", m_lsStatisticData.m_lsOrientation.Average));
                                        }
                                        writer.WriteEndElement();
                                    }
                                }
                                writer.WriteEndElement();
                            }

                            if ((m_sysData.logData.bVerticalGaussianFit == true) || (m_sysData.logData.bHorizontalGaussianFit == true))
                            {
                                writer.WriteStartElement("Correlation");
                                {
                                    if (m_sysData.logData.bHorizontalGaussianFit == true)
                                    {
                                        writer.WriteStartElement("W");
                                        writer.WriteValue(String.Format("{0:#0.000}", m_lsStatisticData.m_lsCorrelationV.Average));
                                        writer.WriteEndElement();
                                    }

                                    if (m_sysData.logData.bVerticalGaussianFit == true)
                                    {
                                        writer.WriteStartElement("V");
                                        writer.WriteValue(String.Format("{0:#0.000}", m_lsStatisticData.m_lsCorrelationW.Average));
                                        writer.WriteEndElement();
                                    }
                                }
                                writer.WriteEndElement();
                            }

                            if ((m_sysData.logData.bHorizontalGaussianWidthLevel1 == true) || (m_sysData.logData.bHorizontalGaussianWidthLevel2 == true) || (m_sysData.logData.bHorizontalGaussianWidthLevel3 == true) ||
                                (m_sysData.logData.bVerticalGaussianWidthLevel1 == true) || (m_sysData.logData.bVerticalGaussianWidthLevel2 == true) || (m_sysData.logData.bVerticalGaussianWidthLevel3 == true))
                            {
                                writer.WriteStartElement("GaussWidths");
                                {
                                    if ((m_sysData.logData.bHorizontalGaussianWidthLevel1 == true) || (m_sysData.logData.bHorizontalGaussianWidthLevel2 == true) || (m_sysData.logData.bHorizontalGaussianWidthLevel3 == true))
                                    {
                                        writer.WriteStartElement("W");
                                        {
                                            if (m_sysData.logData.bHorizontalGaussianWidthLevel1 == true)
                                            {
                                                writer.WriteStartElement("W1");
                                                {
                                                    writer.WriteValue(String.Format("{0:#0.000}", m_lsStatisticData.m_lsGaussWidthV[0].Average * m_sysData.UnitsCoeff));
                                                }
                                                writer.WriteEndElement();
                                            }

                                            if (m_sysData.logData.bHorizontalGaussianWidthLevel2 == true)
                                            {
                                                writer.WriteStartElement("W2");
                                                {
                                                    writer.WriteValue(String.Format("{0:#0.000}", m_lsStatisticData.m_lsGaussWidthV[1].Average * m_sysData.UnitsCoeff));
                                                }
                                                writer.WriteEndElement();
                                            }

                                            if (m_sysData.logData.bHorizontalGaussianWidthLevel3 == true)
                                            {
                                                writer.WriteStartElement("W3");
                                                {
                                                    writer.WriteValue(String.Format("{0:#0.000}", m_lsStatisticData.m_lsGaussWidthV[2].Average * m_sysData.UnitsCoeff));
                                                }
                                                writer.WriteEndElement();
                                            }
                                        }
                                        writer.WriteEndElement();
                                    }

                                    if ((m_sysData.logData.bVerticalGaussianWidthLevel1 == true) || (m_sysData.logData.bVerticalGaussianWidthLevel2 == true) || (m_sysData.logData.bVerticalGaussianWidthLevel3 == true))
                                    {
                                        writer.WriteStartElement("V");
                                        {
                                            if (m_sysData.logData.bVerticalGaussianWidthLevel1 == true)
                                            {
                                                writer.WriteStartElement("V1");
                                                {
                                                    writer.WriteValue(String.Format("{0:#0.000}", m_lsStatisticData.m_lsGaussWidthW[0].Average * m_sysData.UnitsCoeff));
                                                }
                                                writer.WriteEndElement();
                                            }

                                            if (m_sysData.logData.bVerticalGaussianWidthLevel2 == true)
                                            {
                                                writer.WriteStartElement("V2");
                                                {
                                                    writer.WriteValue(String.Format("{0:#0.000}", m_lsStatisticData.m_lsGaussWidthW[1].Average * m_sysData.UnitsCoeff));
                                                }
                                                writer.WriteEndElement();
                                            }

                                            if (m_sysData.logData.bVerticalGaussianWidthLevel3 == true)
                                            {
                                                writer.WriteStartElement("V3");
                                                {
                                                    writer.WriteValue(String.Format("{0:#0.000}", m_lsStatisticData.m_lsGaussWidthW[2].Average * m_sysData.UnitsCoeff));
                                                }
                                                writer.WriteEndElement();
                                            }
                                        }
                                        writer.WriteEndElement();
                                    }
                                }
                                writer.WriteEndElement();
                            }
                        }
                        writer.WriteEndElement();

                        writer.WriteStartElement("STD");
                        {
                            if (m_sysData.logData.bPower == true)
                            {
                                writer.WriteStartElement("Power");
                                {
                                    writer.WriteValue(PowerData.GetValueStringFormat((Single)m_lsStatisticData.m_lsPower.STD, 0));
                                }
                                writer.WriteEndElement();
                            }

                            if ((m_sysData.logData.bPositionX == true) || (m_sysData.logData.bPositionY == true))
                            {
                                writer.WriteStartElement("Position");
                                {
                                    if (m_sysData.logData.bPositionX == true)
                                    {
                                        writer.WriteStartElement("X");
                                        {
                                            writer.WriteValue(String.Format("{0:#0.000}", m_lsStatisticData.m_lsPositionX.STD * m_sysData.UnitsCoeff));
                                        }
                                        writer.WriteEndElement();
                                    }

                                    if (m_sysData.logData.bPositionY == true)
                                    {
                                        writer.WriteStartElement("Y");
                                        {
                                            writer.WriteValue(String.Format("{0:#0.000}", m_lsStatisticData.m_lsPositionY.STD * m_sysData.UnitsCoeff));
                                        }
                                        writer.WriteEndElement();
                                    }
                                    writer.WriteEndElement();
                                }
                            }

                            if ((m_sysData.logData.bHorizontalProfileWidthLevel1 == true) || (m_sysData.logData.bHorizontalProfileWidthLevel2 == true) || (m_sysData.logData.bHorizontalProfileWidthLevel3 == true) ||
                                (m_sysData.logData.bVerticalProfileWidthLevel1 == true) || (m_sysData.logData.bVerticalProfileWidthLevel2 == true) || (m_sysData.logData.bVerticalProfileWidthLevel3 == true))
                            {
                                writer.WriteStartElement("Widths");
                                {
                                    if ((m_sysData.logData.bHorizontalProfileWidthLevel1 == true) || (m_sysData.logData.bHorizontalProfileWidthLevel2 == true) || (m_sysData.logData.bHorizontalProfileWidthLevel3 == true))
                                    {
                                        writer.WriteStartElement("W");
                                        {
                                            if (m_sysData.logData.bHorizontalProfileWidthLevel1 == true)
                                            {
                                                writer.WriteStartElement("W1");
                                                {
                                                    writer.WriteValue(String.Format("{0:#0.000}", m_lsStatisticData.m_lsWidthV[0].STD * m_sysData.UnitsCoeff));
                                                }
                                                writer.WriteEndElement();
                                            }

                                            if (m_sysData.logData.bHorizontalProfileWidthLevel2 == true)
                                            {
                                                writer.WriteStartElement("W2");
                                                {
                                                    writer.WriteValue(String.Format("{0:#0.000}", m_lsStatisticData.m_lsWidthV[1].STD * m_sysData.UnitsCoeff));
                                                }
                                                writer.WriteEndElement();
                                            }

                                            if (m_sysData.logData.bHorizontalProfileWidthLevel3 == true)
                                            {
                                                writer.WriteStartElement("W3");
                                                {
                                                    writer.WriteValue(String.Format("{0:#0.000}", m_lsStatisticData.m_lsWidthV[2].STD * m_sysData.UnitsCoeff));
                                                }
                                                writer.WriteEndElement();
                                            }
                                        }
                                        writer.WriteEndElement();
                                    }

                                    if ((m_sysData.logData.bVerticalProfileWidthLevel1 == true) || (m_sysData.logData.bVerticalProfileWidthLevel2 == true) || (m_sysData.logData.bVerticalProfileWidthLevel3 == true))
                                    {
                                        writer.WriteStartElement("V");
                                        {
                                            if (m_sysData.logData.bVerticalProfileWidthLevel1 == true)
                                            {
                                                writer.WriteStartElement("V1");
                                                {
                                                    writer.WriteValue(String.Format("{0:#0.000}", m_lsStatisticData.m_lsWidthW[0].STD * m_sysData.UnitsCoeff));
                                                }
                                                writer.WriteEndElement();
                                            }

                                            if (m_sysData.logData.bVerticalProfileWidthLevel2 == true)
                                            {
                                                writer.WriteStartElement("V2");
                                                {
                                                    writer.WriteValue(String.Format("{0:#0.000}", m_lsStatisticData.m_lsWidthW[1].STD * m_sysData.UnitsCoeff));
                                                }
                                                writer.WriteEndElement();
                                            }

                                            if (m_sysData.logData.bVerticalProfileWidthLevel3 == true)
                                            {
                                                writer.WriteStartElement("V3");
                                                {
                                                    writer.WriteValue(String.Format("{0:#0.000}", m_lsStatisticData.m_lsWidthW[2].STD * m_sysData.UnitsCoeff));
                                                }
                                                writer.WriteEndElement();
                                            }
                                        }
                                        writer.WriteEndElement();
                                    }
                                }
                                writer.WriteEndElement();
                            }

                            if ((m_sysData.logData.bMajor == true) || (m_sysData.logData.bMinor == true) || (m_sysData.logData.bOrientation == true))
                            {
                                writer.WriteStartElement("EllipseData");
                                {
                                    if (m_sysData.logData.bMajor == true)
                                    {
                                        writer.WriteStartElement("Major");
                                        {
                                            writer.WriteValue(String.Format("{0:#0.000}", m_lsStatisticData.m_lsMajor.STD * m_sysData.UnitsCoeff));
                                        }
                                        writer.WriteEndElement();
                                    }

                                    if (m_sysData.logData.bMinor == true)
                                    {
                                        writer.WriteStartElement("Minor");
                                        {
                                            writer.WriteValue(String.Format("{0:#0.000}", m_lsStatisticData.m_lsMinor.STD * m_sysData.UnitsCoeff));
                                        }
                                        writer.WriteEndElement();
                                    }

                                    if (m_sysData.logData.bOrientation == true)
                                    {
                                        writer.WriteStartElement("Orientation");
                                        {
                                            writer.WriteValue(String.Format("{0:#0.000}", m_lsStatisticData.m_lsOrientation.STD));
                                        }
                                        writer.WriteEndElement();
                                    }
                                }
                                writer.WriteEndElement();
                            }

                            if ((m_sysData.logData.bVerticalGaussianFit == true) || (m_sysData.logData.bHorizontalGaussianFit == true))
                            {
                                writer.WriteStartElement("Correlation");
                                {
                                    if (m_sysData.logData.bHorizontalGaussianFit == true)
                                    {
                                        writer.WriteStartElement("W");
                                        writer.WriteValue(String.Format("{0:#0.000}", m_lsStatisticData.m_lsCorrelationV.STD));
                                        writer.WriteEndElement();
                                    }

                                    if (m_sysData.logData.bVerticalGaussianFit == true)
                                    {
                                        writer.WriteStartElement("V");
                                        writer.WriteValue(String.Format("{0:#0.000}", m_lsStatisticData.m_lsCorrelationW.STD));
                                        writer.WriteEndElement();
                                    }
                                }
                                writer.WriteEndElement();
                            }

                            if ((m_sysData.logData.bHorizontalGaussianWidthLevel1 == true) || (m_sysData.logData.bHorizontalGaussianWidthLevel2 == true) || (m_sysData.logData.bHorizontalGaussianWidthLevel3 == true) ||
                                (m_sysData.logData.bVerticalGaussianWidthLevel1 == true) || (m_sysData.logData.bVerticalGaussianWidthLevel2 == true) || (m_sysData.logData.bVerticalGaussianWidthLevel3 == true))
                            {
                                writer.WriteStartElement("GaussWidths");
                                {
                                    if ((m_sysData.logData.bHorizontalGaussianWidthLevel1 == true) || (m_sysData.logData.bHorizontalGaussianWidthLevel2 == true) || (m_sysData.logData.bHorizontalGaussianWidthLevel3 == true))
                                    {
                                        writer.WriteStartElement("W");
                                        {
                                            if (m_sysData.logData.bHorizontalGaussianWidthLevel1 == true)
                                            {
                                                writer.WriteStartElement("W1");
                                                {
                                                    writer.WriteValue(String.Format("{0:#0.000}", m_lsStatisticData.m_lsGaussWidthV[0].STD * m_sysData.UnitsCoeff));
                                                }
                                                writer.WriteEndElement();
                                            }

                                            if (m_sysData.logData.bHorizontalGaussianWidthLevel2 == true)
                                            {
                                                writer.WriteStartElement("W2");
                                                {
                                                    writer.WriteValue(String.Format("{0:#0.000}", m_lsStatisticData.m_lsGaussWidthV[1].STD * m_sysData.UnitsCoeff));
                                                }
                                                writer.WriteEndElement();
                                            }

                                            if (m_sysData.logData.bHorizontalGaussianWidthLevel3 == true)
                                            {
                                                writer.WriteStartElement("W3");
                                                {
                                                    writer.WriteValue(String.Format("{0:#0.000}", m_lsStatisticData.m_lsGaussWidthV[2].STD * m_sysData.UnitsCoeff));
                                                }
                                                writer.WriteEndElement();
                                            }
                                        }
                                        writer.WriteEndElement();
                                    }

                                    if ((m_sysData.logData.bVerticalGaussianWidthLevel1 == true) || (m_sysData.logData.bVerticalGaussianWidthLevel2 == true) || (m_sysData.logData.bVerticalGaussianWidthLevel3 == true))
                                    {
                                        writer.WriteStartElement("V");
                                        {
                                            if (m_sysData.logData.bVerticalGaussianWidthLevel1 == true)
                                            {
                                                writer.WriteStartElement("V1");
                                                {
                                                    writer.WriteValue(String.Format("{0:#0.000}", m_lsStatisticData.m_lsGaussWidthW[0].STD * m_sysData.UnitsCoeff));
                                                }
                                                writer.WriteEndElement();
                                            }

                                            if (m_sysData.logData.bVerticalGaussianWidthLevel2 == true)
                                            {
                                                writer.WriteStartElement("V2");
                                                {
                                                    writer.WriteValue(String.Format("{0:#0.000}", m_lsStatisticData.m_lsGaussWidthW[1].STD * m_sysData.UnitsCoeff));
                                                }
                                                writer.WriteEndElement();
                                            }

                                            if (m_sysData.logData.bVerticalGaussianWidthLevel3 == true)
                                            {
                                                writer.WriteStartElement("V3");
                                                {
                                                    writer.WriteValue(String.Format("{0:#0.000}", m_lsStatisticData.m_lsGaussWidthW[2].STD * m_sysData.UnitsCoeff));
                                                }
                                                writer.WriteEndElement();
                                            }
                                        }
                                        writer.WriteEndElement();
                                    }
                                }
                                writer.WriteEndElement();
                            }
                        }
                        writer.WriteEndElement();
                    }
                    writer.WriteEndElement();

                    writer.WriteEndDocument();
                }
            }
            catch
            {
            }
            finally
            {
                if (writer != null)
                {
                    writer.Flush();
                    writer.Close();
                    m_bOpenFile = false;
                    XML2HTML();
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
                myXslCompiledTransform.Load(m_sysData.applicationData.m_strMyAppDir + "\\LogTranslator.xslt");

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

    #region LogDataText

    public class LogDataText : LogDataSample
    {
        Boolean m_bOpenFile = false;

        public LogDataText()
            : base()
        {
        }

        public override bool IsOpen()
        {
            return m_bOpenFile;
        }

        public override void CloseLog()
        {
            using (StreamWriter sw = File.AppendText(m_sysData.logData.strFileName))
            {
                sw.WriteLine();
                sw.WriteLine("***************  Statistics  ***************");
                sw.WriteLine();

                //sw.WriteLine(String.Format("{0, -10}{1, 10}{2, 13}{3, 13}{4, 13}{5, 13}{6, 13}{7, 13}{8, 13}{9, 13}",
                //                           "Min",
                //                           PowerData.GetValueStringFormat((Single)m_lsStatisticData.m_lsPower.Min, 0),//, m_sysData.powerData.PowerUnits),
                //                           String.Format("{0:#0.000}", m_lsStatisticData.m_lsPositionX.Min),
                //                           String.Format("{0:#0.000}", m_lsStatisticData.m_lsPositionY.Min),
                //                           String.Format("{0:#0.000}", m_lsStatisticData.m_lsWidthV[0].Min),
                //                           String.Format("{0:#0.000}", m_lsStatisticData.m_lsWidthV[1].Min),
                //                           String.Format("{0:#0.000}", m_lsStatisticData.m_lsWidthV[2].Min),
                //                           String.Format("{0:#0.000}", m_lsStatisticData.m_lsWidthW[0].Min),
                //                           String.Format("{0:#0.000}", m_lsStatisticData.m_lsWidthW[1].Min),
                //                           String.Format("{0:#0.000}", m_lsStatisticData.m_lsWidthW[2].Min)
                //                           ));

                sw.Write(String.Format("{0, -10}", "Min"));

                if (m_sysData.logData.bPower == true) sw.Write("{0, 10}", PowerData.GetValueStringFormat((Single)m_lsStatisticData.m_lsPower.Min, 0));
                if (m_sysData.logData.bPositionX == true) sw.Write("{0, 13}", String.Format("{0:#0.000}", m_lsStatisticData.m_lsPositionX.Min * m_sysData.UnitsCoeff));
                if (m_sysData.logData.bPositionY == true) sw.Write("{0, 13}", String.Format("{0:#0.000}", m_lsStatisticData.m_lsPositionY.Min * m_sysData.UnitsCoeff));
                if (m_sysData.logData.bHorizontalProfileWidthLevel1 == true) sw.Write("{0, 13}", String.Format("{0:#0.000}", m_lsStatisticData.m_lsWidthV[0].Min * m_sysData.UnitsCoeff));
                if (m_sysData.logData.bHorizontalProfileWidthLevel2 == true) sw.Write("{0, 13}", String.Format("{0:#0.000}", m_lsStatisticData.m_lsWidthV[1].Min * m_sysData.UnitsCoeff));
                if (m_sysData.logData.bHorizontalProfileWidthLevel3 == true) sw.Write("{0, 13}", String.Format("{0:#0.000}", m_lsStatisticData.m_lsWidthV[2].Min * m_sysData.UnitsCoeff));
                if (m_sysData.logData.bVerticalProfileWidthLevel1 == true) sw.Write("{0, 13}", String.Format("{0:#0.000}", m_lsStatisticData.m_lsWidthW[0].Min * m_sysData.UnitsCoeff));
                if (m_sysData.logData.bVerticalProfileWidthLevel2 == true) sw.Write("{0, 13}", String.Format("{0:#0.000}", m_lsStatisticData.m_lsWidthW[1].Min * m_sysData.UnitsCoeff));
                if (m_sysData.logData.bVerticalProfileWidthLevel3 == true) sw.Write("{0, 13}", String.Format("{0:#0.000}", m_lsStatisticData.m_lsWidthW[2].Min * m_sysData.UnitsCoeff));
                if (m_sysData.logData.bMajor == true) sw.Write("{0, 13}", String.Format("{0:#0.000}", m_lsStatisticData.m_lsMajor.Min * m_sysData.UnitsCoeff));
                if (m_sysData.logData.bMinor == true) sw.Write("{0, 13}", String.Format("{0:#0.000}", m_lsStatisticData.m_lsMinor.Min * m_sysData.UnitsCoeff));
                if (m_sysData.logData.bOrientation == true) sw.Write("{0, 13}", String.Format("{0:#0.000}", m_lsStatisticData.m_lsOrientation.Min));
                if (m_sysData.logData.bHorizontalGaussianFit == true) sw.Write("{0, 16}", String.Format("{0:#0.000}", m_lsStatisticData.m_lsCorrelationV.Min));
                if (m_sysData.logData.bVerticalGaussianFit == true) sw.Write("{0, 16}", String.Format("{0:#0.000}", m_lsStatisticData.m_lsCorrelationW.Min));
                if (m_sysData.logData.bHorizontalGaussianWidthLevel1 == true) sw.Write("{0, 16}", String.Format("{0:#0.000}", m_lsStatisticData.m_lsGaussWidthV[0].Min * m_sysData.UnitsCoeff));
                if (m_sysData.logData.bHorizontalGaussianWidthLevel2 == true) sw.Write("{0, 16}", String.Format("{0:#0.000}", m_lsStatisticData.m_lsGaussWidthV[1].Min * m_sysData.UnitsCoeff));
                if (m_sysData.logData.bHorizontalGaussianWidthLevel3 == true) sw.Write("{0, 16}", String.Format("{0:#0.000}", m_lsStatisticData.m_lsGaussWidthV[2].Min * m_sysData.UnitsCoeff));
                if (m_sysData.logData.bVerticalGaussianWidthLevel1 == true) sw.Write("{0, 16}", String.Format("{0:#0.000}", m_lsStatisticData.m_lsGaussWidthW[0].Min * m_sysData.UnitsCoeff));
                if (m_sysData.logData.bVerticalGaussianWidthLevel2 == true) sw.Write("{0, 16}", String.Format("{0:#0.000}", m_lsStatisticData.m_lsGaussWidthW[1].Min * m_sysData.UnitsCoeff));
                if (m_sysData.logData.bVerticalGaussianWidthLevel3 == true) sw.Write("{0, 16}", String.Format("{0:#0.000}", m_lsStatisticData.m_lsGaussWidthW[2].Min * m_sysData.UnitsCoeff));

                sw.WriteLine();

                //sw.WriteLine(String.Format("{0, -10}{1, 10}{2, 13}{3, 13}{4, 13}{5, 13}{6, 13}{7, 13}{8, 13}{9, 13}",
                //                           "Max",
                //                           PowerData.GetValueStringFormat((Single)m_lsStatisticData.m_lsPower.Max, 0),//, m_sysData.powerData.PowerUnits),
                //                           String.Format("{0:#0.000}", m_lsStatisticData.m_lsPositionX.Max),
                //                           String.Format("{0:#0.000}", m_lsStatisticData.m_lsPositionY.Max),
                //                           String.Format("{0:#0.000}", m_lsStatisticData.m_lsWidthV[0].Max),
                //                           String.Format("{0:#0.000}", m_lsStatisticData.m_lsWidthV[1].Max),
                //                           String.Format("{0:#0.000}", m_lsStatisticData.m_lsWidthV[2].Max),
                //                           String.Format("{0:#0.000}", m_lsStatisticData.m_lsWidthW[0].Max),
                //                           String.Format("{0:#0.000}", m_lsStatisticData.m_lsWidthW[1].Max),
                //                           String.Format("{0:#0.000}", m_lsStatisticData.m_lsWidthW[2].Max)
                //                           ));

                sw.Write(String.Format("{0, -10}", "Max"));

                if (m_sysData.logData.bPower == true) sw.Write("{0, 10}", PowerData.GetValueStringFormat((Single)m_lsStatisticData.m_lsPower.Max, 0));
                if (m_sysData.logData.bPositionX == true) sw.Write("{0, 13}", String.Format("{0:#0.000}", m_lsStatisticData.m_lsPositionX.Max * m_sysData.UnitsCoeff));
                if (m_sysData.logData.bPositionY == true) sw.Write("{0, 13}", String.Format("{0:#0.000}", m_lsStatisticData.m_lsPositionY.Max * m_sysData.UnitsCoeff));
                if (m_sysData.logData.bHorizontalProfileWidthLevel1 == true) sw.Write("{0, 13}", String.Format("{0:#0.000}", m_lsStatisticData.m_lsWidthV[0].Max * m_sysData.UnitsCoeff));
                if (m_sysData.logData.bHorizontalProfileWidthLevel2 == true) sw.Write("{0, 13}", String.Format("{0:#0.000}", m_lsStatisticData.m_lsWidthV[1].Max * m_sysData.UnitsCoeff));
                if (m_sysData.logData.bHorizontalProfileWidthLevel3 == true) sw.Write("{0, 13}", String.Format("{0:#0.000}", m_lsStatisticData.m_lsWidthV[2].Max * m_sysData.UnitsCoeff));
                if (m_sysData.logData.bVerticalProfileWidthLevel1 == true) sw.Write("{0, 13}", String.Format("{0:#0.000}", m_lsStatisticData.m_lsWidthW[0].Max * m_sysData.UnitsCoeff));
                if (m_sysData.logData.bVerticalProfileWidthLevel2 == true) sw.Write("{0, 13}", String.Format("{0:#0.000}", m_lsStatisticData.m_lsWidthW[1].Max * m_sysData.UnitsCoeff));
                if (m_sysData.logData.bVerticalProfileWidthLevel3 == true) sw.Write("{0, 13}", String.Format("{0:#0.000}", m_lsStatisticData.m_lsWidthW[2].Max * m_sysData.UnitsCoeff));
                if (m_sysData.logData.bMajor == true) sw.Write("{0, 13}", String.Format("{0:#0.000}", m_lsStatisticData.m_lsMajor.Max * m_sysData.UnitsCoeff));
                if (m_sysData.logData.bMinor == true) sw.Write("{0, 13}", String.Format("{0:#0.000}", m_lsStatisticData.m_lsMinor.Max * m_sysData.UnitsCoeff));
                if (m_sysData.logData.bOrientation == true) sw.Write("{0, 13}", String.Format("{0:#0.000}", m_lsStatisticData.m_lsOrientation.Max));
                if (m_sysData.logData.bHorizontalGaussianFit == true) sw.Write("{0, 16}", String.Format("{0:#0.000}", m_lsStatisticData.m_lsCorrelationV.Max));
                if (m_sysData.logData.bVerticalGaussianFit == true) sw.Write("{0, 16}", String.Format("{0:#0.000}", m_lsStatisticData.m_lsCorrelationW.Max));
                if (m_sysData.logData.bHorizontalGaussianWidthLevel1 == true) sw.Write("{0, 16}", String.Format("{0:#0.000}", m_lsStatisticData.m_lsGaussWidthV[0].Max * m_sysData.UnitsCoeff));
                if (m_sysData.logData.bHorizontalGaussianWidthLevel2 == true) sw.Write("{0, 16}", String.Format("{0:#0.000}", m_lsStatisticData.m_lsGaussWidthV[1].Max * m_sysData.UnitsCoeff));
                if (m_sysData.logData.bHorizontalGaussianWidthLevel3 == true) sw.Write("{0, 16}", String.Format("{0:#0.000}", m_lsStatisticData.m_lsGaussWidthV[2].Max * m_sysData.UnitsCoeff));
                if (m_sysData.logData.bVerticalGaussianWidthLevel1 == true) sw.Write("{0, 16}", String.Format("{0:#0.000}", m_lsStatisticData.m_lsGaussWidthW[0].Max * m_sysData.UnitsCoeff));
                if (m_sysData.logData.bVerticalGaussianWidthLevel2 == true) sw.Write("{0, 16}", String.Format("{0:#0.000}", m_lsStatisticData.m_lsGaussWidthW[1].Max * m_sysData.UnitsCoeff));
                if (m_sysData.logData.bVerticalGaussianWidthLevel3 == true) sw.Write("{0, 16}", String.Format("{0:#0.000}", m_lsStatisticData.m_lsGaussWidthW[2].Max * m_sysData.UnitsCoeff));

                sw.WriteLine();

                //sw.WriteLine(String.Format("{0, -10}{1, 10}{2, 13}{3, 13}{4, 13}{5, 13}{6, 13}{7, 13}{8, 13}{9, 13}",
                //                           "Aver",
                //                           PowerData.GetValueStringFormat((Single)m_lsStatisticData.m_lsPower.Average, 0),//, m_sysData.powerData.PowerUnits),
                //                           String.Format("{0:#0.000}", m_lsStatisticData.m_lsPositionX.Average),
                //                           String.Format("{0:#0.000}", m_lsStatisticData.m_lsPositionY.Average),
                //                           String.Format("{0:#0.000}", m_lsStatisticData.m_lsWidthV[0].Average),
                //                           String.Format("{0:#0.000}", m_lsStatisticData.m_lsWidthV[1].Average),
                //                           String.Format("{0:#0.000}", m_lsStatisticData.m_lsWidthV[2].Average),
                //                           String.Format("{0:#0.000}", m_lsStatisticData.m_lsWidthW[0].Average),
                //                           String.Format("{0:#0.000}", m_lsStatisticData.m_lsWidthW[1].Average),
                //                           String.Format("{0:#0.000}", m_lsStatisticData.m_lsWidthW[2].Average)
                //                           ));

                sw.Write(String.Format("{0, -10}", "Aver"));

                if (m_sysData.logData.bPower == true) sw.Write("{0, 10}", PowerData.GetValueStringFormat((Single)m_lsStatisticData.m_lsPower.Average, 0));
                if (m_sysData.logData.bPositionX == true) sw.Write("{0, 13}", String.Format("{0:#0.000}", m_lsStatisticData.m_lsPositionX.Average * m_sysData.UnitsCoeff));
                if (m_sysData.logData.bPositionY == true) sw.Write("{0, 13}", String.Format("{0:#0.000}", m_lsStatisticData.m_lsPositionY.Average * m_sysData.UnitsCoeff));
                if (m_sysData.logData.bHorizontalProfileWidthLevel1 == true) sw.Write("{0, 13}", String.Format("{0:#0.000}", m_lsStatisticData.m_lsWidthV[0].Average * m_sysData.UnitsCoeff));
                if (m_sysData.logData.bHorizontalProfileWidthLevel2 == true) sw.Write("{0, 13}", String.Format("{0:#0.000}", m_lsStatisticData.m_lsWidthV[1].Average * m_sysData.UnitsCoeff));
                if (m_sysData.logData.bHorizontalProfileWidthLevel3 == true) sw.Write("{0, 13}", String.Format("{0:#0.000}", m_lsStatisticData.m_lsWidthV[2].Average * m_sysData.UnitsCoeff));
                if (m_sysData.logData.bVerticalProfileWidthLevel1 == true) sw.Write("{0, 13}", String.Format("{0:#0.000}", m_lsStatisticData.m_lsWidthW[0].Average * m_sysData.UnitsCoeff));
                if (m_sysData.logData.bVerticalProfileWidthLevel2 == true) sw.Write("{0, 13}", String.Format("{0:#0.000}", m_lsStatisticData.m_lsWidthW[1].Average * m_sysData.UnitsCoeff));
                if (m_sysData.logData.bVerticalProfileWidthLevel3 == true) sw.Write("{0, 13}", String.Format("{0:#0.000}", m_lsStatisticData.m_lsWidthW[2].Average * m_sysData.UnitsCoeff));
                if (m_sysData.logData.bMajor == true) sw.Write("{0, 13}", String.Format("{0:#0.000}", m_lsStatisticData.m_lsMajor.Average * m_sysData.UnitsCoeff));
                if (m_sysData.logData.bMinor == true) sw.Write("{0, 13}", String.Format("{0:#0.000}", m_lsStatisticData.m_lsMinor.Average * m_sysData.UnitsCoeff));
                if (m_sysData.logData.bOrientation == true) sw.Write("{0, 13}", String.Format("{0:#0.000}", m_lsStatisticData.m_lsOrientation.Average));
                if (m_sysData.logData.bHorizontalGaussianFit == true) sw.Write("{0, 16}", String.Format("{0:#0.000}", m_lsStatisticData.m_lsCorrelationV.Average));
                if (m_sysData.logData.bVerticalGaussianFit == true) sw.Write("{0, 16}", String.Format("{0:#0.000}", m_lsStatisticData.m_lsCorrelationW.Average));
                if (m_sysData.logData.bHorizontalGaussianWidthLevel1 == true) sw.Write("{0, 16}", String.Format("{0:#0.000}", m_lsStatisticData.m_lsGaussWidthV[0].Average * m_sysData.UnitsCoeff));
                if (m_sysData.logData.bHorizontalGaussianWidthLevel2 == true) sw.Write("{0, 16}", String.Format("{0:#0.000}", m_lsStatisticData.m_lsGaussWidthV[1].Average * m_sysData.UnitsCoeff));
                if (m_sysData.logData.bHorizontalGaussianWidthLevel3 == true) sw.Write("{0, 16}", String.Format("{0:#0.000}", m_lsStatisticData.m_lsGaussWidthV[2].Average * m_sysData.UnitsCoeff));
                if (m_sysData.logData.bVerticalGaussianWidthLevel1 == true) sw.Write("{0, 16}", String.Format("{0:#0.000}", m_lsStatisticData.m_lsGaussWidthW[0].Average * m_sysData.UnitsCoeff));
                if (m_sysData.logData.bVerticalGaussianWidthLevel2 == true) sw.Write("{0, 16}", String.Format("{0:#0.000}", m_lsStatisticData.m_lsGaussWidthW[1].Average * m_sysData.UnitsCoeff));
                if (m_sysData.logData.bVerticalGaussianWidthLevel3 == true) sw.Write("{0, 16}", String.Format("{0:#0.000}", m_lsStatisticData.m_lsGaussWidthW[2].Average * m_sysData.UnitsCoeff));

                sw.WriteLine();

                //sw.WriteLine(String.Format("{0, -10}{1, 10}{2, 13}{3, 13}{4, 13}{5, 13}{6, 13}{7, 13}{8, 13}{9, 13}",
                //                           "STD",
                //                           String.Format("{0:#0.000}", m_lsStatisticData.m_lsPower.STD),
                //                           String.Format("{0:#0.000}", m_lsStatisticData.m_lsPositionX.STD),
                //                           String.Format("{0:#0.000}", m_lsStatisticData.m_lsPositionY.STD),
                //                           String.Format("{0:#0.000}", m_lsStatisticData.m_lsWidthV[0].STD),
                //                           String.Format("{0:#0.000}", m_lsStatisticData.m_lsWidthV[1].STD),
                //                           String.Format("{0:#0.000}", m_lsStatisticData.m_lsWidthV[2].STD),
                //                           String.Format("{0:#0.000}", m_lsStatisticData.m_lsWidthW[0].STD),
                //                           String.Format("{0:#0.000}", m_lsStatisticData.m_lsWidthW[1].STD),
                //                           String.Format("{0:#0.000}", m_lsStatisticData.m_lsWidthW[2].STD)
                //                           ));

                sw.Write(String.Format("{0, -10}", "STD"));

                if (m_sysData.logData.bPower == true) sw.Write("{0, 10}", PowerData.GetValueStringFormat((Single)m_lsStatisticData.m_lsPower.STD, 0));
                if (m_sysData.logData.bPositionX == true) sw.Write("{0, 13}", String.Format("{0:#0.000}", m_lsStatisticData.m_lsPositionX.STD * m_sysData.UnitsCoeff));
                if (m_sysData.logData.bPositionY == true) sw.Write("{0, 13}", String.Format("{0:#0.000}", m_lsStatisticData.m_lsPositionY.STD * m_sysData.UnitsCoeff));
                if (m_sysData.logData.bHorizontalProfileWidthLevel1 == true) sw.Write("{0, 13}", String.Format("{0:#0.000}", m_lsStatisticData.m_lsWidthV[0].STD * m_sysData.UnitsCoeff));
                if (m_sysData.logData.bHorizontalProfileWidthLevel2 == true) sw.Write("{0, 13}", String.Format("{0:#0.000}", m_lsStatisticData.m_lsWidthV[1].STD * m_sysData.UnitsCoeff));
                if (m_sysData.logData.bHorizontalProfileWidthLevel3 == true) sw.Write("{0, 13}", String.Format("{0:#0.000}", m_lsStatisticData.m_lsWidthV[2].STD * m_sysData.UnitsCoeff));
                if (m_sysData.logData.bVerticalProfileWidthLevel1 == true) sw.Write("{0, 13}", String.Format("{0:#0.000}", m_lsStatisticData.m_lsWidthW[0].STD * m_sysData.UnitsCoeff));
                if (m_sysData.logData.bVerticalProfileWidthLevel2 == true) sw.Write("{0, 13}", String.Format("{0:#0.000}", m_lsStatisticData.m_lsWidthW[1].STD * m_sysData.UnitsCoeff));
                if (m_sysData.logData.bVerticalProfileWidthLevel3 == true) sw.Write("{0, 13}", String.Format("{0:#0.000}", m_lsStatisticData.m_lsWidthW[2].STD * m_sysData.UnitsCoeff));
                if (m_sysData.logData.bMajor == true) sw.Write("{0, 13}", String.Format("{0:#0.000}", m_lsStatisticData.m_lsMajor.STD * m_sysData.UnitsCoeff));
                if (m_sysData.logData.bMinor == true) sw.Write("{0, 13}", String.Format("{0:#0.000}", m_lsStatisticData.m_lsMinor.STD * m_sysData.UnitsCoeff));
                if (m_sysData.logData.bOrientation == true) sw.Write("{0, 13}", String.Format("{0:#0.000}", m_lsStatisticData.m_lsOrientation.STD));
                if (m_sysData.logData.bHorizontalGaussianFit == true) sw.Write("{0, 16}", String.Format("{0:#0.000}", m_lsStatisticData.m_lsCorrelationV.STD));
                if (m_sysData.logData.bVerticalGaussianFit == true) sw.Write("{0, 16}", String.Format("{0:#0.000}", m_lsStatisticData.m_lsCorrelationW.STD));
                if (m_sysData.logData.bHorizontalGaussianWidthLevel1 == true) sw.Write("{0, 16}", String.Format("{0:#0.000}", m_lsStatisticData.m_lsGaussWidthV[0].STD * m_sysData.UnitsCoeff));
                if (m_sysData.logData.bHorizontalGaussianWidthLevel2 == true) sw.Write("{0, 16}", String.Format("{0:#0.000}", m_lsStatisticData.m_lsGaussWidthV[1].STD * m_sysData.UnitsCoeff));
                if (m_sysData.logData.bHorizontalGaussianWidthLevel3 == true) sw.Write("{0, 16}", String.Format("{0:#0.000}", m_lsStatisticData.m_lsGaussWidthV[2].STD * m_sysData.UnitsCoeff));
                if (m_sysData.logData.bVerticalGaussianWidthLevel1 == true) sw.Write("{0, 16}", String.Format("{0:#0.000}", m_lsStatisticData.m_lsGaussWidthW[0].STD * m_sysData.UnitsCoeff));
                if (m_sysData.logData.bVerticalGaussianWidthLevel2 == true) sw.Write("{0, 16}", String.Format("{0:#0.000}", m_lsStatisticData.m_lsGaussWidthW[1].STD * m_sysData.UnitsCoeff));
                if (m_sysData.logData.bVerticalGaussianWidthLevel3 == true) sw.Write("{0, 16}", String.Format("{0:#0.000}", m_lsStatisticData.m_lsGaussWidthW[2].STD * m_sysData.UnitsCoeff));

                sw.WriteLine();

                sw.WriteLine();
                sw.WriteLine("***************  End Statistics  ************");
                sw.WriteLine();

                m_bOpenFile = false;
                sw.Close();
            }
        }

        public override void AddData(Double dTime)
        {
            String MyTime = "";

            m_lsStatisticData.AddData();

            m_sysData.powerData.mwPower = (m_sysData.powerData.Power / m_sysData.powerData.realFilterFactor) / m_sysData.powerData.currentSAMFactor - m_sysData.powerData.mwOffsetPower * Math.Abs(Convert.ToInt16(m_sysData.powerData.bIndOffset));

            if ((m_sysData.powerData.mwPower < 0) && (m_sysData.powerData.bIndOffset == false)) m_sysData.powerData.mwPower = 0;

            MyTime = (m_sysData.logData.LogInterval == 0) ? "{0:#0.0}" : "{0:#0}";

            //using (StreamWriter w = File.AppendText(m_sysData.logData.strFileName))
            //{
            //    w.WriteLine(String.Format("{0, 10}{1, 10}{2, 13}{3, 13}{4, 13}{5, 13}{6, 13}{7, 13}{8, 13}{9, 13}",
            //                               String.Format(MyTime, dTime),
            //                               PowerData.GetValueStringFormat(m_sysData.powerData.mwPower, 0),//m_sysData.powerData.PowerUnits),
            //                               String.Format("{0:#0.000}", m_sysData.positionData.PrX),
            //                               String.Format("{0:#0.000}", m_sysData.positionData.PrY),
            //                               String.Format("{0:#0.000}", m_sysData.HorizontalProfile.Width[0]),
            //                               String.Format("{0:#0.000}", m_sysData.HorizontalProfile.Width[1]),
            //                               String.Format("{0:#0.000}", m_sysData.HorizontalProfile.Width[2]),
            //                               String.Format("{0:#0.000}", m_sysData.VerticalProfile.Width[0]),
            //                               String.Format("{0:#0.000}", m_sysData.VerticalProfile.Width[1]),
            //                               String.Format("{0:#0.000}", m_sysData.VerticalProfile.Width[2])
            //                               ));
            //    w.Close();
            //}

            using (StreamWriter w = File.AppendText(m_sysData.logData.strFileName))
            {
                w.Write("{0, 10}", String.Format(MyTime, dTime));

                if (m_sysData.logData.bPower == true) w.Write("{0, 10}", PowerData.GetValueStringFormat(m_sysData.powerData.mwPower, 0));
                if (m_sysData.logData.bPositionX == true) w.Write("{0, 13}", m_sysData.positionData.strPositionX);
                if (m_sysData.logData.bPositionY == true) w.Write("{0, 13}", m_sysData.positionData.strPositionY);
                if (m_sysData.logData.bHorizontalProfileWidthLevel1 == true) w.Write("{0, 13}", m_sysData.HorizontalProfile.strWidth[0]);
                if (m_sysData.logData.bHorizontalProfileWidthLevel2 == true) w.Write("{0, 13}", m_sysData.HorizontalProfile.strWidth[1]);
                if (m_sysData.logData.bHorizontalProfileWidthLevel3 == true) w.Write("{0, 13}", m_sysData.HorizontalProfile.strWidth[2]);
                if (m_sysData.logData.bVerticalProfileWidthLevel1 == true) w.Write("{0, 13}", m_sysData.VerticalProfile.strWidth[0]);
                if (m_sysData.logData.bVerticalProfileWidthLevel2 == true) w.Write("{0, 13}", m_sysData.VerticalProfile.strWidth[1]);
                if (m_sysData.logData.bVerticalProfileWidthLevel3 == true) w.Write("{0, 13}", m_sysData.VerticalProfile.strWidth[2]);

                if (m_sysData.logData.bMajor == true)
                {
                    if (m_sysData.positionData.Ellipse.EllipseError == false)
                        w.Write("{0, 13}", m_sysData.positionData.strMajor);
                    else
                        w.Write("{0, 13}", "-----");
                }

                if (m_sysData.logData.bMinor == true)
                {
                    if (m_sysData.positionData.Ellipse.EllipseError == false)
                        w.Write("{0, 13}", m_sysData.positionData.strMinor);
                    else
                        w.Write("{0, 13}", "-----");
                }

                if (m_sysData.logData.bOrientation == true)
                {
                    if (m_sysData.positionData.Ellipse.EllipseError == false)
                        w.Write("{0, 13}", String.Format("{0:#0.000}", m_sysData.positionData.Ellipse.Orientation));
                    else
                        w.Write("{0, 13}", "-----");
                }

                if (m_sysData.logData.bHorizontalGaussianFit == true) w.Write("{0, 16}", String.Format("{0:#0.000}", m_sysData.HorizontalProfile.m_fCorrelation));
                if (m_sysData.logData.bVerticalGaussianFit == true) w.Write("{0, 16}", String.Format("{0:#0.000}", m_sysData.VerticalProfile.m_fCorrelation));
                if (m_sysData.logData.bHorizontalGaussianWidthLevel1 == true) w.Write("{0, 16}", m_sysData.HorizontalProfile.strGaussWidth[0]);
                if (m_sysData.logData.bHorizontalGaussianWidthLevel2 == true) w.Write("{0, 16}", m_sysData.HorizontalProfile.strGaussWidth[1]);
                if (m_sysData.logData.bHorizontalGaussianWidthLevel3 == true) w.Write("{0, 16}", m_sysData.HorizontalProfile.strGaussWidth[2]);
                if (m_sysData.logData.bVerticalGaussianWidthLevel1 == true) w.Write("{0, 16}", m_sysData.VerticalProfile.strGaussWidth[0]);
                if (m_sysData.logData.bVerticalGaussianWidthLevel2 == true) w.Write("{0, 16}", m_sysData.VerticalProfile.strGaussWidth[1]);
                if (m_sysData.logData.bVerticalGaussianWidthLevel3 == true) w.Write("{0, 16}", m_sysData.VerticalProfile.strGaussWidth[2]);

                w.WriteLine();

                w.Close();
            }
        }

        public override void CreateHeader()
        {
            FileInfo fi = new FileInfo(m_sysData.logData.strFileName);

            using (StreamWriter sw = File.CreateText(m_sysData.logData.strFileName))
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

                //sw.Write("System: ");
                //if (m_sysData.HightResolution == true)
                //{
                //    sw.Write("High Resolution Mode");
                //}
                //else
                //{
                //    sw.Write("Low Resolution Mode");
                //}
                //sw.WriteLine();

                sw.Write("Average: " + ((m_sysData.AverageOn == true) ? m_sysData.Average.ToString() : "OFF"));
                sw.WriteLine();

                sw.WriteLine("Wavelength: " + m_sysData.powerData.uiWavelenght.ToString() + " (nm)");
                sw.WriteLine();

                sw.WriteLine("HeadTilt: " + m_sysData.positionData.iHeadTilt.ToString() + " (grad)");
                sw.WriteLine();

                sw.WriteLine("Level I: " + String.Format("{0:F1}", m_sysData.ClipLevels.Level(0)) + "%");
                sw.WriteLine("Level II: " + String.Format("{0:F1}", m_sysData.ClipLevels.Level(1)) + "%");
                sw.WriteLine("Level III: " + String.Format("{0:F1}", m_sysData.ClipLevels.Level(2)) + "%");

                sw.Write("Null: " + ((m_sysData.powerData.bIndOffset == true) ? "Yes" : "No"));

                sw.WriteLine();

                if (m_sysData.powerData.bIndOffset == true) sw.WriteLine("Null Value: " + String.Format(PowerData.GetValueFormat(m_sysData.powerData.mwOffsetPower, /*m_sysData.powerData.PowerUnits*/0), m_sysData.powerData.mwOffsetPower) + PowerData.bufUnits[/*m_sysData.powerData.PowerUnits*/0]);

                sw.WriteLine("Filter Transmission: " + String.Format("{0:F2}", m_sysData.powerData.realFilterFactor) + "%");
                sw.WriteLine();

                sw.Write("SAM: " + ((m_sysData.powerData.bIndSAM == true) ? "Yes" : "No"));
                sw.WriteLine();

                if (m_sysData.powerData.bIndSAM == true) sw.WriteLine("SAM Transmission: " + String.Format("{0:F2}", m_sysData.powerData.realSAMFactor) + "%");
                sw.WriteLine();

                sw.WriteLine();

                //sw.WriteLine(String.Format("{0, 10}{1, 10}{2, 13}{3, 13}{4, 13}{5, 13}{6, 13}{7, 13}{8, 13}{9, 13}",
                //    "Time", "Power", "Pos.X", "Pos.Y", "W Width I", "W Width II", "W Width III", "V Width I", "V Width II", "V Width III"));

                //sw.WriteLine(String.Format("{0, 10}{1, 10}{2, 13}{2, 13}{2, 13}{2, 13}{2, 13}{2, 13}{2, 13}{2, 13}",
                //    "(sec)", "(" + PowerData.bufUnits[/*m_sysData.powerData.PowerUnits*/0] + ")", "(micron)"));

                sw.Write("{0, 10}", "Time");

                if (m_sysData.logData.bPower == true) sw.Write("{0, 10}", "Power");
                if (m_sysData.logData.bPositionX == true) sw.Write("{0, 13}", "Pos.X");
                if (m_sysData.logData.bPositionY == true) sw.Write("{0, 13}", "Pos.Y");
                if (m_sysData.logData.bHorizontalProfileWidthLevel1 == true) sw.Write("{0, 13}", "W Width I");
                if (m_sysData.logData.bHorizontalProfileWidthLevel2 == true) sw.Write("{0, 13}", "W Width II");
                if (m_sysData.logData.bHorizontalProfileWidthLevel3 == true) sw.Write("{0, 13}", "W Width III");
                if (m_sysData.logData.bVerticalProfileWidthLevel1 == true) sw.Write("{0, 13}", "V Width I");
                if (m_sysData.logData.bVerticalProfileWidthLevel2 == true) sw.Write("{0, 13}", "V Width II");
                if (m_sysData.logData.bVerticalProfileWidthLevel3 == true) sw.Write("{0, 13}", "V Width III");
                if (m_sysData.logData.bMajor == true) sw.Write("{0, 13}", "Major");
                if (m_sysData.logData.bMinor == true) sw.Write("{0, 13}", "Minor");
                if (m_sysData.logData.bOrientation == true) sw.Write("{0, 13}", "Orientation");
                if (m_sysData.logData.bHorizontalGaussianFit == true) sw.Write("{0, 16}", "W Correlation");
                if (m_sysData.logData.bVerticalGaussianFit == true) sw.Write("{0, 16}", "V Correlation");
                if (m_sysData.logData.bHorizontalGaussianWidthLevel1 == true) sw.Write("{0, 16}", "W GaussWidth I");
                if (m_sysData.logData.bHorizontalGaussianWidthLevel2 == true) sw.Write("{0, 16}", "W GaussWidth II");
                if (m_sysData.logData.bHorizontalGaussianWidthLevel3 == true) sw.Write("{0, 16}", "W GaussWidth III");
                if (m_sysData.logData.bVerticalGaussianWidthLevel1 == true) sw.Write("{0, 16}", "V GaussWidth I");
                if (m_sysData.logData.bVerticalGaussianWidthLevel2 == true) sw.Write("{0, 16}", "V GaussWidth II");
                if (m_sysData.logData.bVerticalGaussianWidthLevel3 == true) sw.Write("{0, 16}", "V GaussWidth III");

                sw.WriteLine();

                sw.Write("{0, 10}", "(sec)");

                if (m_sysData.logData.bPower == true) sw.Write("{0, 10}", "(" + PowerData.bufUnits[/*m_sysData.powerData.PowerUnits*/0] + ")");
                if (m_sysData.logData.bPositionX == true) sw.Write("{0, 13}", (m_sysData.UnitMeasure == MeasureUnits.muMicro) ? "(micron)" : "(mrad)");
                if (m_sysData.logData.bPositionY == true) sw.Write("{0, 13}", (m_sysData.UnitMeasure == MeasureUnits.muMicro) ? "(micron)" : "(mrad)");
                if (m_sysData.logData.bHorizontalProfileWidthLevel1 == true) sw.Write("{0, 13}", (m_sysData.UnitMeasure == MeasureUnits.muMicro) ? "(micron)" : "(mrad)");
                if (m_sysData.logData.bHorizontalProfileWidthLevel2 == true) sw.Write("{0, 13}", (m_sysData.UnitMeasure == MeasureUnits.muMicro) ? "(micron)" : "(mrad)");
                if (m_sysData.logData.bHorizontalProfileWidthLevel3 == true) sw.Write("{0, 13}", (m_sysData.UnitMeasure == MeasureUnits.muMicro) ? "(micron)" : "(mrad)");
                if (m_sysData.logData.bVerticalProfileWidthLevel1 == true) sw.Write("{0, 13}", (m_sysData.UnitMeasure == MeasureUnits.muMicro) ? "(micron)" : "(mrad)");
                if (m_sysData.logData.bVerticalProfileWidthLevel2 == true) sw.Write("{0, 13}", (m_sysData.UnitMeasure == MeasureUnits.muMicro) ? "(micron)" : "(mrad)");
                if (m_sysData.logData.bVerticalProfileWidthLevel3 == true) sw.Write("{0, 13}", (m_sysData.UnitMeasure == MeasureUnits.muMicro) ? "(micron)" : "(mrad)");
                if (m_sysData.logData.bMajor == true) sw.Write("{0, 13}", (m_sysData.UnitMeasure == MeasureUnits.muMicro) ? "(micron)" : "(mrad)");
                if (m_sysData.logData.bMinor == true) sw.Write("{0, 13}", (m_sysData.UnitMeasure == MeasureUnits.muMicro) ? "(micron)" : "(mrad)");
                if (m_sysData.logData.bOrientation == true) sw.Write("{0, 13}", "(grad)");
                if (m_sysData.logData.bHorizontalGaussianFit == true) sw.Write("{0, 16}", "(percent)");
                if (m_sysData.logData.bVerticalGaussianFit == true) sw.Write("{0, 16}", "(percent)");
                if (m_sysData.logData.bHorizontalGaussianWidthLevel1 == true) sw.Write("{0, 16}", (m_sysData.UnitMeasure == MeasureUnits.muMicro) ? "(micron)" : "(mrad)");
                if (m_sysData.logData.bHorizontalGaussianWidthLevel2 == true) sw.Write("{0, 16}", (m_sysData.UnitMeasure == MeasureUnits.muMicro) ? "(micron)" : "(mrad)");
                if (m_sysData.logData.bHorizontalGaussianWidthLevel3 == true) sw.Write("{0, 16}", (m_sysData.UnitMeasure == MeasureUnits.muMicro) ? "(micron)" : "(mrad)");
                if (m_sysData.logData.bVerticalGaussianWidthLevel1 == true) sw.Write("{0, 16}", (m_sysData.UnitMeasure == MeasureUnits.muMicro) ? "(micron)" : "(mrad)");
                if (m_sysData.logData.bVerticalGaussianWidthLevel2 == true) sw.Write("{0, 16}", (m_sysData.UnitMeasure == MeasureUnits.muMicro) ? "(micron)" : "(mrad)");
                if (m_sysData.logData.bVerticalGaussianWidthLevel3 == true) sw.Write("{0, 16}", (m_sysData.UnitMeasure == MeasureUnits.muMicro) ? "(micron)" : "(mrad)");

                sw.WriteLine();

                sw.WriteLine();

                sw.Close();

                m_lsStatisticData.Reset();
                m_bOpenFile = true;
            }
        }
    }

    #endregion

    #region LogDataExcel

    public class LogDataExcel : LogDataSample
    {
        Boolean m_bOpenFile = false;
        UInt16 m_uiCurrentRowNumber = 1;
        DataExcel m_de = null;

        public LogDataExcel()
            : base()
        {
        }

        public override bool IsOpen()
        {
            return m_bOpenFile;
        }

        public override void CloseLog()
        {
            //m_de.SetData("A" + m_uiCurrentRowNumber.ToString(), "***************  Statistics  ***************");
            //m_uiCurrentRowNumber++;

            //m_de.SetData("A" + m_uiCurrentRowNumber.ToString(), "Min");
            //m_de.SetData("B" + m_uiCurrentRowNumber.ToString(), PowerData.GetValueStringFormat((Single)m_lsStatisticData.m_lsPower.Min, 0));//m_sysData.powerData.PowerUnits));
            //m_de.SetData("C" + m_uiCurrentRowNumber.ToString(), String.Format("{0:#0.000}", m_lsStatisticData.m_lsPositionX.Min));
            //m_de.SetData("D" + m_uiCurrentRowNumber.ToString(), String.Format("{0:#0.000}", m_lsStatisticData.m_lsPositionY.Min));
            //m_de.SetData("E" + m_uiCurrentRowNumber.ToString(), String.Format("{0:#0.000}", m_lsStatisticData.m_lsWidthV[0].Min));
            //m_de.SetData("F" + m_uiCurrentRowNumber.ToString(), String.Format("{0:#0.000}", m_lsStatisticData.m_lsWidthV[1].Min));
            //m_de.SetData("G" + m_uiCurrentRowNumber.ToString(), String.Format("{0:#0.000}", m_lsStatisticData.m_lsWidthV[2].Min));
            //m_de.SetData("H" + m_uiCurrentRowNumber.ToString(), String.Format("{0:#0.000}", m_lsStatisticData.m_lsWidthW[0].Min));
            //m_de.SetData("I" + m_uiCurrentRowNumber.ToString(), String.Format("{0:#0.000}", m_lsStatisticData.m_lsWidthW[1].Min));
            //m_de.SetData("J" + m_uiCurrentRowNumber.ToString(), String.Format("{0:#0.000}", m_lsStatisticData.m_lsWidthW[2].Min));
            //m_uiCurrentRowNumber++;

            //m_de.SetData("A" + m_uiCurrentRowNumber.ToString(), "Max");
            //m_de.SetData("B" + m_uiCurrentRowNumber.ToString(), PowerData.GetValueStringFormat((Single)m_lsStatisticData.m_lsPower.Max, 0));//m_sysData.powerData.PowerUnits));
            //m_de.SetData("C" + m_uiCurrentRowNumber.ToString(), String.Format("{0:#0.000}", m_lsStatisticData.m_lsPositionX.Max));
            //m_de.SetData("D" + m_uiCurrentRowNumber.ToString(), String.Format("{0:#0.000}", m_lsStatisticData.m_lsPositionY.Max));
            //m_de.SetData("E" + m_uiCurrentRowNumber.ToString(), String.Format("{0:#0.000}", m_lsStatisticData.m_lsWidthV[0].Max));
            //m_de.SetData("F" + m_uiCurrentRowNumber.ToString(), String.Format("{0:#0.000}", m_lsStatisticData.m_lsWidthV[1].Max));
            //m_de.SetData("G" + m_uiCurrentRowNumber.ToString(), String.Format("{0:#0.000}", m_lsStatisticData.m_lsWidthV[2].Max));
            //m_de.SetData("H" + m_uiCurrentRowNumber.ToString(), String.Format("{0:#0.000}", m_lsStatisticData.m_lsWidthW[0].Max));
            //m_de.SetData("I" + m_uiCurrentRowNumber.ToString(), String.Format("{0:#0.000}", m_lsStatisticData.m_lsWidthW[1].Max));
            //m_de.SetData("J" + m_uiCurrentRowNumber.ToString(), String.Format("{0:#0.000}", m_lsStatisticData.m_lsWidthW[2].Max));
            //m_uiCurrentRowNumber++;

            //m_de.SetData("A" + m_uiCurrentRowNumber.ToString(), "Aver");
            //m_de.SetData("B" + m_uiCurrentRowNumber.ToString(), PowerData.GetValueStringFormat((Single)m_lsStatisticData.m_lsPower.Average, 0));//, m_sysData.powerData.PowerUnits));
            //m_de.SetData("C" + m_uiCurrentRowNumber.ToString(), String.Format("{0:#0.000}", m_lsStatisticData.m_lsPositionX.Average));
            //m_de.SetData("D" + m_uiCurrentRowNumber.ToString(), String.Format("{0:#0.000}", m_lsStatisticData.m_lsPositionY.Average));
            //m_de.SetData("E" + m_uiCurrentRowNumber.ToString(), String.Format("{0:#0.000}", m_lsStatisticData.m_lsWidthV[0].Average));
            //m_de.SetData("F" + m_uiCurrentRowNumber.ToString(), String.Format("{0:#0.000}", m_lsStatisticData.m_lsWidthV[1].Average));
            //m_de.SetData("G" + m_uiCurrentRowNumber.ToString(), String.Format("{0:#0.000}", m_lsStatisticData.m_lsWidthV[2].Average));
            //m_de.SetData("H" + m_uiCurrentRowNumber.ToString(), String.Format("{0:#0.000}", m_lsStatisticData.m_lsWidthW[0].Average));
            //m_de.SetData("I" + m_uiCurrentRowNumber.ToString(), String.Format("{0:#0.000}", m_lsStatisticData.m_lsWidthW[1].Average));
            //m_de.SetData("J" + m_uiCurrentRowNumber.ToString(), String.Format("{0:#0.000}", m_lsStatisticData.m_lsWidthW[2].Average));
            //m_uiCurrentRowNumber++;

            //m_de.SetData("A" + m_uiCurrentRowNumber.ToString(), "STD");
            //m_de.SetData("B" + m_uiCurrentRowNumber.ToString(), String.Format("{0:#0.000}", m_lsStatisticData.m_lsPower.STD));
            //m_de.SetData("C" + m_uiCurrentRowNumber.ToString(), String.Format("{0:#0.000}", m_lsStatisticData.m_lsPositionX.STD));
            //m_de.SetData("D" + m_uiCurrentRowNumber.ToString(), String.Format("{0:#0.000}", m_lsStatisticData.m_lsPositionY.STD));
            //m_de.SetData("E" + m_uiCurrentRowNumber.ToString(), String.Format("{0:#0.000}", m_lsStatisticData.m_lsWidthV[0].STD));
            //m_de.SetData("F" + m_uiCurrentRowNumber.ToString(), String.Format("{0:#0.000}", m_lsStatisticData.m_lsWidthV[1].STD));
            //m_de.SetData("G" + m_uiCurrentRowNumber.ToString(), String.Format("{0:#0.000}", m_lsStatisticData.m_lsWidthV[2].STD));
            //m_de.SetData("H" + m_uiCurrentRowNumber.ToString(), String.Format("{0:#0.000}", m_lsStatisticData.m_lsWidthW[0].STD));
            //m_de.SetData("I" + m_uiCurrentRowNumber.ToString(), String.Format("{0:#0.000}", m_lsStatisticData.m_lsWidthW[1].STD));
            //m_de.SetData("J" + m_uiCurrentRowNumber.ToString(), String.Format("{0:#0.000}", m_lsStatisticData.m_lsWidthW[2].STD));
            //m_uiCurrentRowNumber++;

            //m_de.SetData("A" + m_uiCurrentRowNumber.ToString(), "***************  End Statistics  ************");

            //m_de.SaveAs(m_sysData.logData.strFileName);
            m_de.Close(m_sysData.logData.strFileName);
            m_de.CloseExcel();
            m_de = null;
            m_bOpenFile = false;
        }

        public override void AddData(Double dTime)
        {
            Char cColumnLeter;

            String MyTime = "";

            m_lsStatisticData.AddData();

            m_sysData.powerData.mwPower = (m_sysData.powerData.Power / m_sysData.powerData.realFilterFactor) / m_sysData.powerData.currentSAMFactor - m_sysData.powerData.mwOffsetPower * Math.Abs(Convert.ToInt16(m_sysData.powerData.bIndOffset));

            if ((m_sysData.powerData.mwPower < 0) && (m_sysData.powerData.bIndOffset == false)) m_sysData.powerData.mwPower = 0;

            MyTime = (m_sysData.logData.LogInterval == 0) ? "{0:#0.0}" : "{0:#0}";

            m_de.SetData("A" + m_uiCurrentRowNumber.ToString(), String.Format(MyTime, dTime));

            cColumnLeter = 'A';

            if (m_sysData.logData.bPower == true) m_de.SetData(Convert.ToChar(++cColumnLeter) + m_uiCurrentRowNumber.ToString(), PowerData.GetValueStringFormat(m_sysData.powerData.mwPower, 0));//, m_sysData.powerData.PowerUnits));
            if (m_sysData.logData.bPositionX == true) m_de.SetData(Convert.ToChar(++cColumnLeter) + m_uiCurrentRowNumber.ToString(), m_sysData.positionData.strPositionX);
            if (m_sysData.logData.bPositionY == true) m_de.SetData(Convert.ToChar(++cColumnLeter) + m_uiCurrentRowNumber.ToString(), m_sysData.positionData.strPositionY);
            if (m_sysData.logData.bHorizontalProfileWidthLevel1 == true) m_de.SetData(Convert.ToChar(++cColumnLeter) + m_uiCurrentRowNumber.ToString(), m_sysData.HorizontalProfile.strWidth[0]);
            if (m_sysData.logData.bHorizontalProfileWidthLevel2 == true) m_de.SetData(Convert.ToChar(++cColumnLeter) + m_uiCurrentRowNumber.ToString(), m_sysData.HorizontalProfile.strWidth[1]);
            if (m_sysData.logData.bHorizontalProfileWidthLevel3 == true) m_de.SetData(Convert.ToChar(++cColumnLeter) + m_uiCurrentRowNumber.ToString(), m_sysData.HorizontalProfile.strWidth[2]);
            if (m_sysData.logData.bVerticalProfileWidthLevel1 == true) m_de.SetData(Convert.ToChar(++cColumnLeter) + m_uiCurrentRowNumber.ToString(), m_sysData.VerticalProfile.strWidth[0]);
            if (m_sysData.logData.bVerticalProfileWidthLevel2 == true) m_de.SetData(Convert.ToChar(++cColumnLeter) + m_uiCurrentRowNumber.ToString(), m_sysData.VerticalProfile.strWidth[1]);
            if (m_sysData.logData.bVerticalProfileWidthLevel3 == true) m_de.SetData(Convert.ToChar(++cColumnLeter) + m_uiCurrentRowNumber.ToString(), m_sysData.VerticalProfile.strWidth[2]);

            if (m_sysData.logData.bMajor == true)
            {
                if (m_sysData.positionData.Ellipse.EllipseError == false)
                    m_de.SetData(Convert.ToChar(++cColumnLeter) + m_uiCurrentRowNumber.ToString(), m_sysData.positionData.strMajor);
                else
                    m_de.SetData(Convert.ToChar(++cColumnLeter) + m_uiCurrentRowNumber.ToString(), "-----");
            }
            if (m_sysData.logData.bMinor == true)
            {
                if (m_sysData.positionData.Ellipse.EllipseError == false)
                    m_de.SetData(Convert.ToChar(++cColumnLeter) + m_uiCurrentRowNumber.ToString(), m_sysData.positionData.strMinor);
                else
                    m_de.SetData(Convert.ToChar(++cColumnLeter) + m_uiCurrentRowNumber.ToString(), "-----");
            }

            if (m_sysData.logData.bOrientation == true)
            {
                if (m_sysData.positionData.Ellipse.EllipseError == false)
                    m_de.SetData(Convert.ToChar(++cColumnLeter) + m_uiCurrentRowNumber.ToString(), String.Format("{0:#0.000}", m_sysData.positionData.Ellipse.Orientation));
                else
                    m_de.SetData(Convert.ToChar(++cColumnLeter) + m_uiCurrentRowNumber.ToString(), "-----");
            }

            if (m_sysData.logData.bHorizontalGaussianFit == true) m_de.SetData(Convert.ToChar(++cColumnLeter) + m_uiCurrentRowNumber.ToString(), String.Format("{0:#0.000}", m_sysData.HorizontalProfile.m_fCorrelation));
            if (m_sysData.logData.bVerticalGaussianFit == true) m_de.SetData(Convert.ToChar(++cColumnLeter) + m_uiCurrentRowNumber.ToString(), String.Format("{0:#0.000}", m_sysData.VerticalProfile.m_fCorrelation));
            if (m_sysData.logData.bHorizontalGaussianWidthLevel1 == true) m_de.SetData(Convert.ToChar(++cColumnLeter) + m_uiCurrentRowNumber.ToString(), m_sysData.HorizontalProfile.strGaussWidth[0]);
            if (m_sysData.logData.bHorizontalGaussianWidthLevel2 == true) m_de.SetData(Convert.ToChar(++cColumnLeter) + m_uiCurrentRowNumber.ToString(), m_sysData.HorizontalProfile.strGaussWidth[1]);
            if (m_sysData.logData.bHorizontalGaussianWidthLevel3 == true) m_de.SetData(Convert.ToChar(++cColumnLeter) + m_uiCurrentRowNumber.ToString(), m_sysData.HorizontalProfile.strGaussWidth[2]);
            if (m_sysData.logData.bVerticalGaussianWidthLevel1 == true) m_de.SetData(Convert.ToChar(++cColumnLeter) + m_uiCurrentRowNumber.ToString(), m_sysData.VerticalProfile.strGaussWidth[0]);
            if (m_sysData.logData.bVerticalGaussianWidthLevel2 == true) m_de.SetData(Convert.ToChar(++cColumnLeter) + m_uiCurrentRowNumber.ToString(), m_sysData.VerticalProfile.strGaussWidth[1]);
            if (m_sysData.logData.bVerticalGaussianWidthLevel3 == true) m_de.SetData(Convert.ToChar(++cColumnLeter) + m_uiCurrentRowNumber.ToString(), m_sysData.VerticalProfile.strGaussWidth[2]);

            m_uiCurrentRowNumber++;
        }

        public override void CreateHeader()
        {
            Char cColumnLeter;
            FileInfo fi = new FileInfo(m_sysData.logData.strFileName);

            m_uiCurrentRowNumber = 1;

            m_de = new DataExcel();

            if (File.Exists(m_sysData.logData.strFileName))
            {
                m_de.Open(m_sysData.logData.strFileName);
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

            m_de.SetData("A" + m_uiCurrentRowNumber.ToString(), "Average:");
            if (m_sysData.AverageOn == true)
            {
                m_de.SetData("B" + m_uiCurrentRowNumber.ToString(), m_sysData.Average.ToString());
            }
            else
            {
                m_de.SetData("B" + m_uiCurrentRowNumber.ToString(), "OFF");
            }
            m_uiCurrentRowNumber++;
            m_uiCurrentRowNumber++;


            m_de.SetData("A" + m_uiCurrentRowNumber.ToString(), "Wavelength:");
            m_de.SetData("B" + m_uiCurrentRowNumber.ToString(), m_sysData.powerData.uiWavelenght.ToString() + " (nm)");
            m_uiCurrentRowNumber++;
            m_uiCurrentRowNumber++;

            m_de.SetData("A" + m_uiCurrentRowNumber.ToString(), "HeadTilt:");
            m_de.SetData("B" + m_uiCurrentRowNumber.ToString(), m_sysData.positionData.iHeadTilt.ToString() + "(grad)");
            m_uiCurrentRowNumber++;

            m_de.SetData("A" + m_uiCurrentRowNumber.ToString(), "Level I:");
            m_de.SetData("B" + m_uiCurrentRowNumber.ToString(), String.Format("{0:F1}", m_sysData.ClipLevels.Level(0)) + "%");
            m_uiCurrentRowNumber++;
            m_de.SetData("A" + m_uiCurrentRowNumber.ToString(), "Level II:");
            m_de.SetData("B" + m_uiCurrentRowNumber.ToString(), String.Format("{0:F1}", m_sysData.ClipLevels.Level(1)) + "%");
            m_uiCurrentRowNumber++;
            m_de.SetData("A" + m_uiCurrentRowNumber.ToString(), "Level III:");
            m_de.SetData("B" + m_uiCurrentRowNumber.ToString(), String.Format("{0:F1}", m_sysData.ClipLevels.Level(2)) + "%");
            m_uiCurrentRowNumber++;
            m_uiCurrentRowNumber++;

            m_de.SetData("A" + m_uiCurrentRowNumber.ToString(), "Null:");
            if (m_sysData.powerData.bIndOffset == true)
                m_de.SetData("B" + m_uiCurrentRowNumber.ToString(), "Yes");
            else
                m_de.SetData("B" + m_uiCurrentRowNumber.ToString(), "No");
            m_uiCurrentRowNumber++;

            if (m_sysData.powerData.bIndOffset == true)
            {
                m_de.SetData("A" + m_uiCurrentRowNumber.ToString(), "Null Value:");
                m_de.SetData("B" + m_uiCurrentRowNumber.ToString(), String.Format(PowerData.GetValueFormat(m_sysData.powerData.mwOffsetPower, 0/*m_sysData.powerData.PowerUnits*/), m_sysData.powerData.mwOffsetPower) + PowerData.bufUnits[/*m_sysData.powerData.PowerUnits*/0]);
                m_uiCurrentRowNumber++;
            }

            m_de.SetData("A" + m_uiCurrentRowNumber.ToString(), "Filter Transmission:");
            m_de.SetData("B" + m_uiCurrentRowNumber.ToString(), String.Format("{0:F2}", m_sysData.powerData.realFilterFactor) + "%");
            m_uiCurrentRowNumber++;

            m_de.SetData("A" + m_uiCurrentRowNumber.ToString(), "SAM:");
            m_de.SetData("B" + m_uiCurrentRowNumber.ToString(), (m_sysData.powerData.bIndSAM == true) ? "Yes" : "No");
            m_uiCurrentRowNumber++;

            if (m_sysData.powerData.bIndSAM == true)
            {
                m_de.SetData("A" + m_uiCurrentRowNumber.ToString(), "SAM Transmission:");
                m_de.SetData("B" + m_uiCurrentRowNumber.ToString(), String.Format("{0:F2}", m_sysData.powerData.realSAMFactor) + "%");
                m_uiCurrentRowNumber++;
            }

            m_uiCurrentRowNumber++;

            m_de.SetData("A" + m_uiCurrentRowNumber.ToString(), "Time");

            cColumnLeter = 'A';
            if (m_sysData.logData.bPower == true) m_de.SetData(Convert.ToChar(++cColumnLeter) + m_uiCurrentRowNumber.ToString(), "Power");
            if (m_sysData.logData.bPositionX == true) m_de.SetData(Convert.ToChar(++cColumnLeter) + m_uiCurrentRowNumber.ToString(), "Pos.X");
            if (m_sysData.logData.bPositionY == true) m_de.SetData(Convert.ToChar(++cColumnLeter) + m_uiCurrentRowNumber.ToString(), "Pos.Y");
            if (m_sysData.logData.bHorizontalProfileWidthLevel1 == true) m_de.SetData(Convert.ToChar(++cColumnLeter) + m_uiCurrentRowNumber.ToString(), "W Width I");
            if (m_sysData.logData.bHorizontalProfileWidthLevel2 == true) m_de.SetData(Convert.ToChar(++cColumnLeter) + m_uiCurrentRowNumber.ToString(), "W Width II");
            if (m_sysData.logData.bHorizontalProfileWidthLevel3 == true) m_de.SetData(Convert.ToChar(++cColumnLeter) + m_uiCurrentRowNumber.ToString(), "W Width III");
            if (m_sysData.logData.bVerticalProfileWidthLevel1 == true) m_de.SetData(Convert.ToChar(++cColumnLeter) + m_uiCurrentRowNumber.ToString(), "V Width I");
            if (m_sysData.logData.bVerticalProfileWidthLevel2 == true) m_de.SetData(Convert.ToChar(++cColumnLeter) + m_uiCurrentRowNumber.ToString(), "V Width II");
            if (m_sysData.logData.bVerticalProfileWidthLevel3 == true) m_de.SetData(Convert.ToChar(++cColumnLeter) + m_uiCurrentRowNumber.ToString(), "V Width III");

            if (m_sysData.logData.bMajor == true) m_de.SetData(Convert.ToChar(++cColumnLeter) + m_uiCurrentRowNumber.ToString(), "Major");
            if (m_sysData.logData.bMinor == true) m_de.SetData(Convert.ToChar(++cColumnLeter) + m_uiCurrentRowNumber.ToString(), "Minor");
            if (m_sysData.logData.bOrientation == true) m_de.SetData(Convert.ToChar(++cColumnLeter) + m_uiCurrentRowNumber.ToString(), "Orientation");

            if (m_sysData.logData.bHorizontalGaussianFit == true) m_de.SetData(Convert.ToChar(++cColumnLeter) + m_uiCurrentRowNumber.ToString(), "W Correlation");
            if (m_sysData.logData.bVerticalGaussianFit == true) m_de.SetData(Convert.ToChar(++cColumnLeter) + m_uiCurrentRowNumber.ToString(), "V Correlation");
            if (m_sysData.logData.bHorizontalGaussianWidthLevel1 == true) m_de.SetData(Convert.ToChar(++cColumnLeter) + m_uiCurrentRowNumber.ToString(), "W GaussWidth I");
            if (m_sysData.logData.bHorizontalGaussianWidthLevel2 == true) m_de.SetData(Convert.ToChar(++cColumnLeter) + m_uiCurrentRowNumber.ToString(), "W GaussWidth II");
            if (m_sysData.logData.bHorizontalGaussianWidthLevel3 == true) m_de.SetData(Convert.ToChar(++cColumnLeter) + m_uiCurrentRowNumber.ToString(), "W GaussWidth III");
            if (m_sysData.logData.bVerticalGaussianWidthLevel1 == true) m_de.SetData(Convert.ToChar(++cColumnLeter) + m_uiCurrentRowNumber.ToString(), "V GaussWidth I");
            if (m_sysData.logData.bVerticalGaussianWidthLevel2 == true) m_de.SetData(Convert.ToChar(++cColumnLeter) + m_uiCurrentRowNumber.ToString(), "V GaussWidth II");
            if (m_sysData.logData.bVerticalGaussianWidthLevel3 == true) m_de.SetData(Convert.ToChar(++cColumnLeter) + m_uiCurrentRowNumber.ToString(), "V GaussWidth III");
            m_uiCurrentRowNumber++;

            m_de.SetData("A" + m_uiCurrentRowNumber.ToString(), "(sec)");
            cColumnLeter = 'A';
            if (m_sysData.logData.bPower == true) m_de.SetData(Convert.ToChar(++cColumnLeter) + m_uiCurrentRowNumber.ToString(), "(" + PowerData.bufUnits[/*m_sysData.powerData.PowerUnits*/0] + ")");
            if (m_sysData.logData.bPositionX == true) m_de.SetData(Convert.ToChar(++cColumnLeter) + m_uiCurrentRowNumber.ToString(), (m_sysData.UnitMeasure == MeasureUnits.muMicro) ? "(micron)" : "(mrad)");
            if (m_sysData.logData.bPositionY == true) m_de.SetData(Convert.ToChar(++cColumnLeter) + m_uiCurrentRowNumber.ToString(), (m_sysData.UnitMeasure == MeasureUnits.muMicro) ? "(micron)" : "(mrad)");
            if (m_sysData.logData.bHorizontalProfileWidthLevel1 == true) m_de.SetData(Convert.ToChar(++cColumnLeter) + m_uiCurrentRowNumber.ToString(), (m_sysData.UnitMeasure == MeasureUnits.muMicro) ? "(micron)" : "(mrad)");
            if (m_sysData.logData.bHorizontalProfileWidthLevel2 == true) m_de.SetData(Convert.ToChar(++cColumnLeter) + m_uiCurrentRowNumber.ToString(), (m_sysData.UnitMeasure == MeasureUnits.muMicro) ? "(micron)" : "(mrad)");
            if (m_sysData.logData.bHorizontalProfileWidthLevel3 == true) m_de.SetData(Convert.ToChar(++cColumnLeter) + m_uiCurrentRowNumber.ToString(), (m_sysData.UnitMeasure == MeasureUnits.muMicro) ? "(micron)" : "(mrad)");
            if (m_sysData.logData.bVerticalProfileWidthLevel1 == true) m_de.SetData(Convert.ToChar(++cColumnLeter) + m_uiCurrentRowNumber.ToString(), (m_sysData.UnitMeasure == MeasureUnits.muMicro) ? "(micron)" : "(mrad)");
            if (m_sysData.logData.bVerticalProfileWidthLevel2 == true) m_de.SetData(Convert.ToChar(++cColumnLeter) + m_uiCurrentRowNumber.ToString(), (m_sysData.UnitMeasure == MeasureUnits.muMicro) ? "(micron)" : "(mrad)");
            if (m_sysData.logData.bVerticalProfileWidthLevel3 == true) m_de.SetData(Convert.ToChar(++cColumnLeter) + m_uiCurrentRowNumber.ToString(), (m_sysData.UnitMeasure == MeasureUnits.muMicro) ? "(micron)" : "(mrad)");

            if (m_sysData.logData.bMajor == true) m_de.SetData(Convert.ToChar(++cColumnLeter) + m_uiCurrentRowNumber.ToString(), (m_sysData.UnitMeasure == MeasureUnits.muMicro) ? "(micron)" : "(mrad)");
            if (m_sysData.logData.bMinor == true) m_de.SetData(Convert.ToChar(++cColumnLeter) + m_uiCurrentRowNumber.ToString(), (m_sysData.UnitMeasure == MeasureUnits.muMicro) ? "(micron)" : "(mrad)");
            if (m_sysData.logData.bOrientation == true) m_de.SetData(Convert.ToChar(++cColumnLeter) + m_uiCurrentRowNumber.ToString(), "(grad)");
            if (m_sysData.logData.bHorizontalGaussianFit == true) m_de.SetData(Convert.ToChar(++cColumnLeter) + m_uiCurrentRowNumber.ToString(), "(percent)");
            if (m_sysData.logData.bVerticalGaussianFit == true) m_de.SetData(Convert.ToChar(++cColumnLeter) + m_uiCurrentRowNumber.ToString(), "(percent)");
            if (m_sysData.logData.bHorizontalGaussianWidthLevel1 == true) m_de.SetData(Convert.ToChar(++cColumnLeter) + m_uiCurrentRowNumber.ToString(), (m_sysData.UnitMeasure == MeasureUnits.muMicro) ? "(micron)" : "(mrad)");
            if (m_sysData.logData.bHorizontalGaussianWidthLevel2 == true) m_de.SetData(Convert.ToChar(++cColumnLeter) + m_uiCurrentRowNumber.ToString(), (m_sysData.UnitMeasure == MeasureUnits.muMicro) ? "(micron)" : "(mrad)");
            if (m_sysData.logData.bHorizontalGaussianWidthLevel3 == true) m_de.SetData(Convert.ToChar(++cColumnLeter) + m_uiCurrentRowNumber.ToString(), (m_sysData.UnitMeasure == MeasureUnits.muMicro) ? "(micron)" : "(mrad)");
            if (m_sysData.logData.bVerticalGaussianWidthLevel1 == true) m_de.SetData(Convert.ToChar(++cColumnLeter) + m_uiCurrentRowNumber.ToString(), (m_sysData.UnitMeasure == MeasureUnits.muMicro) ? "(micron)" : "(mrad)");
            if (m_sysData.logData.bVerticalGaussianWidthLevel2 == true) m_de.SetData(Convert.ToChar(++cColumnLeter) + m_uiCurrentRowNumber.ToString(), (m_sysData.UnitMeasure == MeasureUnits.muMicro) ? "(micron)" : "(mrad)");
            if (m_sysData.logData.bVerticalGaussianWidthLevel3 == true) m_de.SetData(Convert.ToChar(++cColumnLeter) + m_uiCurrentRowNumber.ToString(), (m_sysData.UnitMeasure == MeasureUnits.muMicro) ? "(micron)" : "(mrad)");
            m_uiCurrentRowNumber++;

            m_lsStatisticData.Reset();
            m_bOpenFile = true;
        }
    }

    #endregion

    #region LogStatistics

    public class LogStatistics
    {
        SystemData m_sysData = SystemData.MyInstance;

        public LogStatistic m_lsPower = new LogStatistic();
        public LogStatistic m_lsPositionX = new LogStatistic();
        public LogStatistic m_lsPositionY = new LogStatistic();
        public LogStatistic m_lsMajor = new LogStatistic();
        public LogStatistic m_lsMinor = new LogStatistic();
        public LogStatistic m_lsOrientation = new LogStatistic();
        public LogStatistic[] m_lsWidthV = new LogStatistic[3];
        public LogStatistic[] m_lsWidthW = new LogStatistic[3];
        public LogStatistic m_lsCorrelationV = new LogStatistic();
        public LogStatistic m_lsCorrelationW = new LogStatistic();
        public LogStatistic[] m_lsGaussWidthV = new LogStatistic[3];
        public LogStatistic[] m_lsGaussWidthW = new LogStatistic[3];

        public LogStatistic m_lsPowerPeak = new LogStatistic();
        public LogStatistic m_lsPowerPeak2PeakW = new LogStatistic();
        public LogStatistic m_lsPowerPeak2Peak = new LogStatistic();

        public LogStatistics()
        {
            for (int i = 0; i < m_lsWidthV.Length; i++)
            {
                m_lsWidthV[i] = new LogStatistic();
                m_lsWidthW[i] = new LogStatistic();
                m_lsGaussWidthV[i] = new LogStatistic();
                m_lsGaussWidthW[i] = new LogStatistic();
            }

            Reset();
        }

        public void Reset()
        {
            m_lsPower.Reset();
            m_lsPositionX.Reset();
            m_lsPositionY.Reset();
            m_lsMajor.Reset();
            m_lsMinor.Reset();
            m_lsOrientation.Reset();

            m_lsPowerPeak.Reset();
            m_lsPowerPeak2PeakW.Reset();
            m_lsPowerPeak2Peak.Reset();

            for (int i = 0; i < m_lsWidthV.Length; i++)
            {
                m_lsWidthV[i].Reset();
                m_lsWidthW[i].Reset();
                m_lsGaussWidthV[i].Reset();
                m_lsGaussWidthW[i].Reset();
            }

            m_lsCorrelationV.Reset();
            m_lsCorrelationW.Reset();
        }

        public void AddData()
        {
            Double l_dPower = (m_sysData.powerData.Power / m_sysData.powerData.realFilterFactor) / m_sysData.powerData.currentSAMFactor - m_sysData.powerData.mwOffsetPower * Math.Abs(Convert.ToInt16(m_sysData.powerData.bIndOffset)); ;

            if ((l_dPower < 0) && (m_sysData.powerData.bIndOffset == false)) l_dPower = 0;

            //if (m_sysData.powerData.PowerUnits < 3)
            //{
            //    l_dPower = l_dPower * Math.Pow(1000, m_sysData.powerData.PowerUnits);
            //}
            //else if (m_sysData.powerData.PowerUnits == 3)
            //{
            //    if (l_dPower > 0)
            //        l_dPower = 10 * Math.Log10(l_dPower);
            //    else
            //        l_dPower = 0;
            //}

            m_lsPower.AddData(l_dPower);

            m_lsPositionX.AddData(m_sysData.positionData.PositionX);
            m_lsPositionY.AddData(m_sysData.positionData.PositionY);

            if (m_sysData.positionData.Ellipse.EllipseError == false)
            {
                m_lsMajor.AddData(m_sysData.positionData.Ellipse.Major);
                m_lsMinor.AddData(m_sysData.positionData.Ellipse.Minor);
                m_lsOrientation.AddData(m_sysData.positionData.Ellipse.Orientation);
            }

            for (int i = 0; i < m_lsWidthV.Length; i++)
            {
                m_lsWidthV[i].AddData(m_sysData.HorizontalProfile.Width[i]);
                m_lsWidthW[i].AddData(m_sysData.VerticalProfile.Width[i]);
                m_lsGaussWidthV[i].AddData(m_sysData.HorizontalProfile.GaussWidth[i]);
                m_lsGaussWidthW[i].AddData(m_sysData.VerticalProfile.GaussWidth[i]);
            }

            m_lsCorrelationV.AddData(m_sysData.HorizontalProfile.m_fCorrelation);
            m_lsCorrelationW.AddData(m_sysData.VerticalProfile.m_fCorrelation);
        }

        public UInt64 NumMeasure
        {
            get
            {
                return m_lsPower.NumMeasure;
            }
        }
    }

    #endregion

    #region FileLogData

    public class FileLogData
    {
        public delegate void StopLogFile(object sender, EventArgs e);
        public event StopLogFile OnStopLogFile;

        LogDataSample m_LogData = null;
        SystemData m_sysData = SystemData.MyInstance;

        long m_dStartTime = 0;
        long m_dLastTime = 0;

        public FileLogData()
        {
            //m_LogData = new LogDataText(m_sysData);
        }

        ~FileLogData()
        {
            if ((m_LogData != null) && (m_LogData.IsOpen() == true))
            {
                m_LogData.CloseLog();
            }
        }

        public Boolean IsOpen()
        {
            Boolean bRet = false;

            if (m_LogData != null)
            {
                bRet = m_LogData.IsOpen();
            }

            return bRet;
        }

        public void CreateHeader()
        {
            if ((m_LogData == null) || (m_LogData.IsOpen() == false))
            {
                if (m_sysData.logData.ftFile == FileType.ftLog)
                    m_LogData = new LogDataText();
                else if (m_sysData.logData.ftFile == FileType.ftXML)
                    m_LogData = new LogDataXML();
                else
                    m_LogData = new LogDataExcel();

                m_LogData.CreateHeader();
            }
        }

        public void AddData()
        {
            Double dDuration;
            Double dInterval;

            if ((m_LogData != null) && (m_LogData.IsOpen() == true))
            {
                if (m_LogData.NumMeasure == 0)
                {
                    m_dStartTime = m_sysData.logData.LastMeasureTime;
                    m_dLastTime = m_dStartTime;
                }

                dDuration = (m_sysData.logData.LastMeasureTime - m_dStartTime) / 1000000000f;
                dInterval = (m_sysData.logData.LastMeasureTime - m_dLastTime) / 1000000000f;

                if (((m_sysData.logData.ltMode == LogType.ltTime) && ((m_sysData.logData.LogInterval <= dInterval) || (m_LogData.NumMeasure == 0))) ||
                    (m_sysData.logData.ltMode == LogType.ltPoints) || (m_sysData.logData.ltMode == LogType.ltManual))
                {
                    m_LogData.AddData(dDuration);

                    m_dLastTime = m_sysData.logData.LastMeasureTime;
                }

                if (((m_sysData.logData.ltMode == LogType.ltTime) && (dDuration >= m_sysData.logData.LogDuration)) ||
                    ((m_sysData.logData.ltMode == LogType.ltPoints) && (m_LogData.NumMeasure >= m_sysData.logData.LogNumPoints)))
                {
                    CloseLog();
                    OnStopLogFile(this, new EventArgs());
                }
            }
        }

        public void CloseLog()
        {
            if ((m_LogData != null) && (m_LogData.IsOpen() == true)) m_LogData.CloseLog();
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
                        strFileName += ".log";
                    else if (ftFile == FileType.ftExcel)
                        strFileName += (DataExcel.Version < 12) ? ".xls" : ".xlsx";
                    else
                        strFileName += ".xml";
                }
            }

            return strFileName;
        }
    }

    #endregion
}
