using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Win32;
using System.Runtime.InteropServices;
using System.Reflection;

namespace BeamOn_U3
{
    class DataExcel
    {
        String sAppProgID = "Excel.Application";
        Object oExcel = null;
        Object oWorkbooks = null;
        Object oWorkbook = null;
        Object oWorksheets = null;
        Object oWorksheet = null;
        Object oRange = null;
        Boolean m_bOpen = false;

        public DataExcel()
        {
            OpenExcel();
        }

        ~DataExcel()
        {
            CloseExcel();
        }

        public void OpenExcel()
        {
            try
            {
                oExcel = Marshal.GetActiveObject(sAppProgID);
            }
            catch
            {
                Type tExcelObj = Type.GetTypeFromProgID(sAppProgID);
                oExcel = Activator.CreateInstance(tExcelObj);
            }

            if (oExcel != null)
            {
                oWorkbooks = oExcel.GetType().InvokeMember(
                                                            "Workbooks",
                                                            BindingFlags.GetProperty,
                                                            null,
                                                            oExcel,
                                                            null
                                                           );
            }
        }

        public void CreateWorkbook()
        {
            if (oWorkbooks != null)
            {
                oWorkbook = oWorkbooks.GetType().InvokeMember(
                                                                "Add",
                                                                BindingFlags.InvokeMethod,
                                                                null,
                                                                oWorkbooks,
                                                                null
                                                                );

                m_bOpen = false;
            }
        }

        public void GetWorksheets()
        {
            if (oWorkbook != null)
            {
                oWorksheets = oWorkbook.GetType().InvokeMember(
                                                                "Worksheets",
                                                                BindingFlags.GetProperty,
                                                                null,
                                                                oWorkbook,
                                                                null
                                                                );
            }
        }

        public void GetWorksheet()
        {
            object[] args = new object[1];
            args[0] = 1;

            if (oWorksheets != null)
            {
                oWorksheet = oWorksheets.GetType().InvokeMember(
                                                                "Item",
                                                                BindingFlags.GetProperty,
                                                                null,
                                                                oWorksheets,
                                                                args
                                                                );
            }
        }

        public void SetData(String strPosition, String strValue)
        {
            if (oWorksheet != null)
            {
                oRange = oWorksheet.GetType().InvokeMember(
                                                            "Range",
                                                            BindingFlags.GetProperty,
                                                            null,
                                                            oWorksheet,
                                                            new object[] { strPosition });

                oRange.GetType().InvokeMember(
                                              "Value",
                                              BindingFlags.SetProperty,
                                              null,
                                              oRange,
                                              new object[] { strValue });
            }
        }

        public void SaveAs(String strFilePath)
        {
            object[] args = new object[1];
            args[0] = strFilePath;

            if (oWorkbook != null)
            {
                oWorkbook.GetType().InvokeMember(
                                                    "SaveAs",
                                                    BindingFlags.InvokeMethod,
                                                    null,
                                                    oWorkbook,
                                                    args);
            }
        }

        public void Open(String strFilePath)
        {
            object[] args = new object[1];
            args[0] = strFilePath;

            if (oWorkbooks != null)
            {
                oWorkbook = oWorkbooks.GetType().InvokeMember(
                                                                "Open",
                                                                BindingFlags.InvokeMethod,
                                                                null,
                                                                oWorkbooks,
                                                                args
                                                                );

                m_bOpen = true;
            }
        }

        public void Close(String strFilePath)
        {
            object[] args;
            if (m_bOpen == true)
            {
                args = new object[1];
                args[0] = true;
            }
            else
            {
                args = new object[2];
                args[0] = true;
                args[1] = strFilePath;
            }

            if (oWorkbook != null)
            {
                oWorkbook.GetType().InvokeMember(
                                                 "Close",
                                                 BindingFlags.InvokeMethod,
                                                 null,
                                                 oWorkbook,
                                                 args
                                                 );
            }
        }

        public void Close()
        {
            object[] args = new object[1];
            args[0] = true;

            if (oWorkbook != null)
            {
                oWorkbook.GetType().InvokeMember(
                                                 "Close",
                                                 BindingFlags.InvokeMethod,
                                                 null,
                                                 oWorkbook,
                                                 args
                                                 );
            }
        }

        public void CloseExcel()
        {
            Marshal.ReleaseComObject(oExcel);
            GC.GetTotalMemory(true);
        }

        public static Boolean CheckExcelInst()
        {
            return (Registry.ClassesRoot.OpenSubKey("CLSID\\{00024500-0000-0000-C000-000000000046}") != null);
        }

        public static UInt16 Version
        {
            get
            {
                UInt16 uiRet = 0;

                RegistryKey key = Registry.ClassesRoot.OpenSubKey("CLSID\\{00024500-0000-0000-C000-000000000046}\\ProgID");

                if ((key != null) && (key.GetValue("") != null))
                {
                    String data = (String)key.GetValue("");

                    int lastLocation = data.IndexOf("Application.");
                    if (lastLocation >= 0) uiRet = Convert.ToUInt16(data.Substring(lastLocation + "Application.".Length));
                }

                return uiRet;
            }
        }
    }
}
