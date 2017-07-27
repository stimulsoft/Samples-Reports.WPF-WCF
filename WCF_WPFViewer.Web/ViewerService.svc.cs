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
        public string LoadReport(string reportName)
        {
            string result = null;

            if (!string.IsNullOrEmpty(reportName))
            {
                StiReport report = new StiReport();
                report.Load(@"c:\Users\Anton\Documents\Source Code\Stimulsoft\Stimulsoft.Reports.Samples.SWPF\WCF\WCF_WPFViewer.Web\Reports\MasterDetail.mrt");

                InvokePreviewDataSet();

                report.RegData(PreviewDataSet);
                report.Render(false);

                result = StiSLRenderingReportHelper.CheckReportOnInteractions(report, true);
            }

            return result;
        }

        public string RenderingInteractions(string xml)
        {
            InvokePreviewDataSet();
            return StiSLRenderingReportHelper.RenderingInteractions(xml, PreviewDataSet);
        }

        public string RequestFromUserRenderReport(string xml)
        {
            InvokePreviewDataSet();
            return StiSLRenderingReportHelper.RequestFromUserRenderReport(xml, PreviewDataSet);
        }

        public string PrepareRequestFromUserVariables(string xml)
        {
            InvokePreviewDataSet();
            return StiSLRenderingReportHelper.PrepareRequestFromUserVariables(xml, PreviewDataSet);
        }

        public string InteractiveDataBandSelection(string xml)
        {
            InvokePreviewDataSet();
            return StiSLRenderingReportHelper.InteractiveDataBandSelection(xml, PreviewDataSet);
        }

        public byte[] ExportDocument(string xml)
        {
            return StiSLExportHelper.StartExport(xml);
        }
        #endregion
    }
}