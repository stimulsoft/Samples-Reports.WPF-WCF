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
        string RenderingInteractions(string xml);

        [OperationContract]
        string RequestFromUserRenderReport(string xml);

        [OperationContract]
        byte[] ExportDocument(string xml);

        [OperationContract]
        string PrepareRequestFromUserVariables(string xml);

        [OperationContract]
        string InteractiveDataBandSelection(string xml);

        //Designer
        [OperationContract]
        bool SaveReport(byte[] buffer);

        [OperationContract]
        string LoadConfiguration();

        [OperationContract]
        string RenderReport(string xml);

        [OperationContract]
        string TestConnection(string settings);

        [OperationContract]
        string BuildObjects(string settings);

        [OperationContract]
        string RetrieveColumns(string settings);

        // Designer.GoogleDocs
        [OperationContract]
        string GoogleDocsGetDocuments(string xml);

        [OperationContract]
        string GoogleDocsCreateCollection(string xml);

        [OperationContract]
        string GoogleDocsDelete(string xml);

        [OperationContract]
        string GoogleDocsOpen(string xml);

        [OperationContract]
        string GoogleDocsSave(string xml);

        // Designer.Script
        [OperationContract]
        string OpenReportScript(string xml);

        [OperationContract]
        string SaveReportScript(string xml);

        [OperationContract]
        string CheckReport(string xml);
    }
}