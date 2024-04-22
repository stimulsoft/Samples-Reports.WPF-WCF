using Stimulsoft.Report;
using System.Data;
using System.IO;
using WCFHelper;

namespace WCF_WPFDesigner.Service
{
    //[AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class DesignerService : IDesignerService
    {
        #region PreviewDataSet
        private static DataSet previewDataSet = null;
        public static DataSet PreviewDataSet
        {
            get
            {
                return previewDataSet;
            }
            set
            {
                previewDataSet = value;
            }
        }

        private static void InvokePreviewDataSet()
        {
            if (previewDataSet == null)
            {
                previewDataSet = new DataSet();
                previewDataSet.ReadXmlSchema(@"d:\Data\Demo.xsd");
                previewDataSet.ReadXml(@"d:\Data\Demo.xml");
            }
        }
        #endregion

        #region Method.Load & Save
        public byte[] LoadReport()
        {
            var report = new StiReport();
            report.Load("Reports\\Labels.mrt");

            return report.SaveToByteArray();
        }

        public bool SaveReport(byte[] buffer)
        {
            var fileStream = new FileStream("d:\\Data\\1. Master-Detail.mrt", FileMode.CreateNew);
            fileStream.Write(buffer, 0, buffer.Length);
            fileStream.Flush();
            fileStream.Close();
            fileStream.Dispose();
            fileStream = null;

            return true;
        }
        #endregion

        #region Methods.LoadConfiguration
        public byte[] LoadConfiguration()
        {
            return StiSLDesignerHelper.LoadConfiguration();
        }
        #endregion

        #region Methods.Viewer
        public byte[] RenderingInteractions(byte[] data)
        {
            InvokePreviewDataSet();
            return StiSLRenderingReportHelper.RenderingInteractions(data, PreviewDataSet);
        }

        public byte[] RequestFromUserRenderReport(byte[] data)
        {
            InvokePreviewDataSet();
            return StiSLRenderingReportHelper.RequestFromUserRenderReport(data, PreviewDataSet);
        }

        public byte[] ExportDocument(byte[] data)
        {
            return StiSLExportHelper.StartExport(data);
        }

        public byte[] PrepareRequestFromUserVariables(byte[] data)
        {
            InvokePreviewDataSet();
            return StiSLRenderingReportHelper.PrepareRequestFromUserVariables(data, PreviewDataSet);
        }

        public byte[] InteractiveDataBandSelection(byte[] data)
        {
            InvokePreviewDataSet();
            return StiSLRenderingReportHelper.InteractiveDataBandSelection(data, PreviewDataSet);
        }
        #endregion

        #region Methods.Designer
        public byte[] RenderReport(byte[] data)
        {
            InvokePreviewDataSet();
            return StiSLDesignerHelper.RenderReport(data, PreviewDataSet);
        }

        public byte[] TestConnection(byte[] data)
        {
            return StiSLDesignerHelper.TestConnection(data);
        }

        public byte[] BuildObjects(byte[] data)
        {
            return StiSLDesignerHelper.BuildObjects(data);
        }

        public byte[] RetrieveColumns(byte[] data)
        {
            return StiSLDesignerHelper.RetrieveColumns(data);
        }
        #endregion

        #region Methods.ReportScript
        public byte[] OpenReportScript(byte[] data)
        {
            return StiSLDesignerHelper.OpenReportScript(data);
        }

        public byte[] SaveReportScript(byte[] data)
        {
            return StiSLDesignerHelper.SaveReportScript(data);
        }

        public byte[] CheckReport(byte[] data)
        {
            return StiSLDesignerHelper.CheckReport(data);
        }
        #endregion

        public DesignerService()
        {
            // Connect only if you use the additional database.
            //StiOptions.Dictionary.DataAdapters.TryToLoadDB2Adapter = true;
            //StiOptions.Dictionary.DataAdapters.TryToLoadFirebirdAdapter = true;
            //StiOptions.Dictionary.DataAdapters.TryToLoadMySqlAdapter = true;
            //StiOptions.Dictionary.DataAdapters.TryToLoadDotConnectUniversalAdapter = true;
            //StiOptions.Dictionary.DataAdapters.TryToLoadOracleClientAdapter = true;
            //StiOptions.Dictionary.DataAdapters.TryToLoadOracleODPAdapter = true;
            //StiOptions.Dictionary.DataAdapters.TryToLoadPostgreSQLAdapter = true;
            //StiOptions.Dictionary.DataAdapters.TryToLoadSqlCeAdapter = true;
            //StiOptions.Dictionary.DataAdapters.TryToLoadSQLiteAdapter = true;
            //StiOptions.Dictionary.DataAdapters.TryToLoadVistaDBAdapter = true;
            //StiOptions.Dictionary.DataAdapters.TryToLoadUniDirectAdapter = true;
            //StiOptions.Dictionary.DataAdapters.TryToLoadSybaseADSAdapter = true;
            //StiOptions.Dictionary.DataAdapters.TryToLoadSybaseASEAdapter = true;
            //StiOptions.Dictionary.DataAdapters.TryToLoadInformixAdapter = true;
            //StiOptions.Dictionary.DataAdapters.TryToLoadEffiProzAdapter = true;
        }
    }
}