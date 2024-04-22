using System.ServiceModel;

namespace WCF_WPFDesigner.Service
{
    [ServiceContract]
    public interface IDesignerService
    {
        // Viewer
        [OperationContract]
        byte[] LoadReport();

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

        //Designer
        [OperationContract]
        bool SaveReport(byte[] buffer);

        [OperationContract]
        byte[] LoadConfiguration();

        [OperationContract]
        byte[] RenderReport(byte[] data);

        [OperationContract]
        byte[] TestConnection(byte[] data);

        [OperationContract]
        byte[] BuildObjects(byte[] data);

        [OperationContract]
        byte[] RetrieveColumns(byte[] data);

        // Designer.Script
        [OperationContract]
        byte[] OpenReportScript(byte[] data);

        [OperationContract]
        byte[] SaveReportScript(byte[] data);

        [OperationContract]
        byte[] CheckReport(byte[] data);
    }
}