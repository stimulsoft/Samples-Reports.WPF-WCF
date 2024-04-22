using System.ServiceModel;

namespace WCF_WPFViewer.Web
{
    [ServiceContract]
    interface IViewerService
    {
        [OperationContract]
        byte[] LoadReport(string reportName);
        [OperationContract]
        byte[] RenderingInteractions(byte[] data);
        [OperationContract]
        byte[] RequestFromUserRenderReport(byte[] data);
        [OperationContract]
        byte[] ExportDocument(byte[] data);
        [OperationContract]
        byte[] PrepareRequestFromUserVariables(byte[] data);
        [OperationContract]
        byte[] InteractiveDataBandSelection(byte[] data);
    }
}