using System.Windows;
using Stimulsoft.Report;
using Stimulsoft.Report.Wpf;
using Microsoft.Win32;
using Stimulsoft.Base.Localization;
using System.Windows.Controls;
using System.Threading.Tasks;
using System;

namespace WCF_WPFViewer
{
    public partial class MainWindow
    {
        #region Fields
        private StiInteractionType interactionType;
        private StiProgressInformation progress;
        #endregion

        #region Handlers

        #region ExportDocument
        private string exportFilter;

        private Task<byte[]> ExportDocumentRask(byte[] data)
        {
            return Task.Run(() =>
            {
                var service = new ServiceReference1.ViewerServiceClient();
                return service.ExportDocument(data);
            });
        }


        private async void WCFService_WCFExportDocument(object sender, Stimulsoft.Report.Events.StiWCFExportEventArgs e)
        {
            exportFilter = e.Filter;
            progress.Start("Export Report...");

            try
            {
                var result = await ExportDocumentRask(e.Data);
                progress.Hide();

                if (result != null)
                {
                    var saveFileDialog = new SaveFileDialog
                    {
                        Filter = string.Format("Export Document (*.{0})|*.{0}", exportFilter)
                    };
                    if (saveFileDialog.ShowDialog() == true)
                    {
                        var stream = saveFileDialog.OpenFile();
                        stream.Write(result, 0, result.Length);

                        stream.Flush();
                        stream.Close();
                        stream.Dispose();
                        stream = null;
                    }
                }
            }
            catch (Exception)
            {
                progress.Hide();
            }
        }

        #endregion

        #region buttonLoad_Click

        private Task<byte[]> LoadReportTask(string reportName)
        {
            return Task.Run(() =>
            {
                var service = new ServiceReference1.ViewerServiceClient();
                return service.LoadReportAsync(reportName);
            });
        }

        private async void buttonLoad_Click(object sender, RoutedEventArgs e)
        {
            if (cbReports.SelectedItem != null)
            {
                progress.Start(StiLocalization.Get("DesignerFx", "LoadingDocument") + "...");

                try
                {
                    var result = await LoadReportTask(((ComboBoxItem)cbReports.SelectedItem).Content.ToString());

                    progress.Hide();
                    if (result != null && result.Length > 2)
                        viewerControl.ApplyRenderedReport(result);
                }
                catch
                {
                    progress.Hide();
                }
            }
        }

        #endregion

        #region RenderingInteractions

        private Task<byte[]> RenderingInteractionsTask(byte[] data)
        {
            return Task.Run(() =>
            {
                var service = new ServiceReference1.ViewerServiceClient();
                return service.RenderingInteractions(data);
            });
        }

        private async void WCFService_WCFRenderingInteractions(object viewer, Stimulsoft.Report.Events.StiWCFRenderingInteractionsEventArgs e)
        {
            progress.Start(StiLocalization.Get("DesignerFx", "CompilingReport") + "...");

            interactionType = e.InteractionType;

            try
            {
                var result = await RenderingInteractionsTask(e.Data);
                if (result != null)
                {
                    switch (interactionType)
                    {
                        case StiInteractionType.Collapsing:
                            viewerControl.ApplyChangesAfterCollapsing(result);
                            break;

                        case StiInteractionType.DrillDownPage:
                            viewerControl.ApplyChangesAfterDrillDownPage(result, e.DrillDownMode, e.Page);
                            break;

                        case StiInteractionType.Sorting:
                            viewerControl.ApplyChangesAfterSorting(result);
                            break;
                    }
                }
            }
            catch
            {

            }

            progress.Hide();
        }

        #endregion

        #region RequestFromUserRenderReport

        private Task<byte[]> RequestFromUserRenderReportTask(byte[] data)
        {
            return Task.Run(() =>
            {
                var service = new ServiceReference1.ViewerServiceClient();
                return service.RequestFromUserRenderReport(data);
            });
        }

        private async void WCFService_WCFRequestFromUserRenderReport(object sender, Stimulsoft.Report.Events.StiWCFEventArgs e)
        {
            progress.Start(StiLocalization.Get("DesignerFx", "CompilingReport") + "...");

            try
            {
                var result = await RequestFromUserRenderReportTask(e.Data);

                if (result != null && result.Length > 2)
                    viewerControl.ApplyRenderedReport(result, true);
            }
            catch
            {

            }

            progress.Hide();
        }

        #endregion

        #region WCFPrepareRequestFromUserVariables

        private Task<byte[]> PrepareRequestFromUserVariablesTask(byte[] data)
        {
            return Task.Run(() =>
            {
                var service = new ServiceReference1.ViewerServiceClient();
                return service.PrepareRequestFromUserVariables(data);
            });
        }

        private async void WCFService_WCFPrepareRequestFromUserVariables(object sender, Stimulsoft.Report.Events.StiWCFEventArgs e)
        {
            var service = new ServiceReference1.ViewerServiceClient();
            try
            {
                var result = await PrepareRequestFromUserVariablesTask(e.Data);
                viewerControl.ApplyResultAfterPrepareRequestFromUserVariables(null, result);
            }
            catch (Exception ex)
            {
                viewerControl.ApplyResultAfterPrepareRequestFromUserVariables(ex, null);
            }
        }

        #endregion

        #region WCFInteractiveDataBandSelection

        private Task<byte[]> InteractiveDataBandSelectionTask(byte[] data)
        {
            return Task.Run(() =>
            {
                var service = new ServiceReference1.ViewerServiceClient();
                return service.InteractiveDataBandSelection(data);
            });
        }

        private async void WCFService_WCFInteractiveDataBandSelection(object sender, Stimulsoft.Report.Events.StiWCFEventArgs e)
        {
            progress.Start(StiLocalization.Get("DesignerFx", "LoadingReport") + "...");

            try
            {
                var result = await InteractiveDataBandSelectionTask(e.Data);
                this.viewerControl.ApplyChangesAfterInteractiveDataBandSelection(result);
                progress.Hide();
            }
            catch (Exception ex)
            {
                progress.Hide();
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK);
            }
        }

        #endregion

        #endregion

        public MainWindow()
        {
            StiOptions.WCFService.UseWCFService = true;
            progress = new StiProgressInformation(this);
            progress.IsDialog = false;
            progress.IsMarquee = true;
            // Viewer
            Stimulsoft.Report.StiOptions.WCFService.WCFExportDocument += WCFService_WCFExportDocument;
            Stimulsoft.Report.StiOptions.WCFService.WCFRequestFromUserRenderReport += WCFService_WCFRequestFromUserRenderReport;

            // Interactions
            Stimulsoft.Report.StiOptions.WCFService.WCFRenderingInteractions += WCFService_WCFRenderingInteractions;
            Stimulsoft.Report.StiOptions.WCFService.WCFInteractiveDataBandSelection += WCFService_WCFInteractiveDataBandSelection;

            // Prepare RequestFromUser Variables
            Stimulsoft.Report.StiOptions.WCFService.WCFPrepareRequestFromUserVariables += WCFService_WCFPrepareRequestFromUserVariables;

            InitializeComponent();
        }
    }
}