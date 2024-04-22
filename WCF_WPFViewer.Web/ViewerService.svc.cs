using System.Data;
using System.ServiceModel.Activation;
using Stimulsoft.Report;
using WCFHelper;

namespace WCF_WPFViewer.Web
{
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class ViewerService : IViewerService
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
                previewDataSet.ReadXmlSchema(@"c:\Users\Anton\Documents\Source Code\Data\Demo.xsd");
                previewDataSet.ReadXml(@"c:\Users\Anton\Documents\Source Code\Data\Demo.xml");
            }
        }
        #endregion

        #region Methods
        public byte[] LoadReport(string reportName)
        {
            if (!string.IsNullOrEmpty(reportName))
            {
                var report = new StiReport();
                report.Load(@"c:\Users\Anton\Documents\Source Code\Stimulsoft\Stimulsoft.Reports.Samples.SWPF\WCF\WCF_WPFViewer.Web\Reports\MasterDetail.mrt");

                InvokePreviewDataSet();

                report.RegData(PreviewDataSet);
                report.Render(false);

                return StiSLRenderingReportHelper.CheckReportOnInteractions(report, true);
            }

            return null;
        }

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

        public byte[] ExportDocument(byte[] data)
        {
            return StiSLExportHelper.StartExport(data);
        }
        #endregion
    }
}