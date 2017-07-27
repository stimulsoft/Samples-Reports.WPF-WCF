using System.ServiceModel;

namespace WCF_WPFViewer.Web
{
    [ServiceContract]
    interface IViewerService
    {
        [OperationContract]
        string LoadReport(string reportName);
        [OperationContract]
        string RenderingInteractions(string xml);
        [OperationContract]
        string RequestFromUserRenderReport(string xml);
        [OperationContract]
        byte[] ExportDocument(string xml);
        [OperationContract]
        string PrepareRequestFromUserVariables(string xml);
        [OperationContract]
        string InteractiveDataBandSelection(string xml);
    }
}