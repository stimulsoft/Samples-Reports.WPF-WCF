using Microsoft.Win32;
using Stimulsoft.Base.Localization;
using Stimulsoft.Controls.Wpf;
using Stimulsoft.Report;
using Stimulsoft.Report.Viewer;
using Stimulsoft.Report.WCFService;
using Stimulsoft.Report.Wpf;
using Stimulsoft.Report.WpfDesign;
using System;
using System.Data;
using System.Threading.Tasks;
using System.Windows;
using WCF_WpfDesigner.ServiceReference1;

namespace WCF_WpfDesigner
{
    public partial class MainWindow
    {
        #region Fields
        private StiProgressInformation progress;
        private StiWpfDesignerControl designer;
        #endregion

        #region Handlers

        #region RenderReport
        private Task<byte[]> RenderReportTask(byte[] data)
        {
            return Task.Run(() =>
            {
                var service = new ServiceReference1.DesignerServiceClient();
                return service.RenderReport(data);
            });
        }

        private async void WCFService_WCFRenderReport(object sender, Stimulsoft.Report.Events.StiWCFEventArgs e)
        {
            progress.Start(StiLocalization.Get("DesignerFx", "CompilingReport") + "...");

            try
            {
                var result = await RenderReportTask(e.Data);
                if (result != null)
                {
                    designer.ApplyRenderedReport(result);
                }
            }
            catch (Exception ex)
            {
                StiMessageBox.Show(this, ex.Message, string.Empty, MessageBoxButton.OK, MessageBoxImage.Error);
            }

            progress.Hide();
        }
        #endregion

        #region TestConnection

        private Task<byte[]> TestConnectionTask(byte[] data)
        {
            return Task.Run(() =>
            {
                var service = new ServiceReference1.DesignerServiceClient();
                return service.TestConnection(data);
            });
        }

        private async void WCFService_WCFTestConnection(object sender, Stimulsoft.Report.Events.StiWCFEventArgs e)
        {
            progress.Start(StiLocalization.Get("DesignerFx", "TestConnection") + "...");

            try
            {
                var result = await TestConnectionTask(e.Data);

                if (result != null)
                {
                    ((IStiTestConnecting)sender).ApplyResultAfterTestConnection(result);
                }
            }
            catch (Exception ex)
            {
                StiMessageBox.Show(this, ex.Message, string.Empty, MessageBoxButton.OK, MessageBoxImage.Error);
            }

            progress.Hide();
        }

        #endregion

        #region BuildObjects

        private Task<byte[]> BuildObjectsTask(byte[] data)
        {
            return Task.Run(() =>
            {
                var service = new ServiceReference1.DesignerServiceClient();
                return service.BuildObjects(data);
            });
        }

        private async void WCFService_WCFBuildObjects(object sender, Stimulsoft.Report.Events.StiWCFEventArgs e)
        {
            progress.Start(StiLocalization.Get("FormDictionaryDesigner", "RetrievingDatabaseInformation") + "...");

            try
            {
                var result = await BuildObjectsTask(e.Data);

                if (!progress.IsBreaked)
                    ((StiSelectDataWindow)sender).ApplyResultAfterBuildObjects(result);
            }
            catch (Exception ex)
            {
                StiMessageBox.Show(this, ex.Message, string.Empty, MessageBoxButton.OK, MessageBoxImage.Error);
            }

            progress.Hide();
        }

        #endregion

        #region RetrieveColumns

        private Task<byte[]> RetrieveColumnsTask(byte[] data)
        {
            return Task.Run(() =>
            {
                var service = new ServiceReference1.DesignerServiceClient();
                return service.RetrieveColumns(data);
            });
        }

        private async void WCFService_WCFRetrieveColumns(object sender, Stimulsoft.Report.Events.StiWCFEventArgs e)
        {
            progress.Start(StiLocalization.Get("FormDictionaryDesigner", "RetrieveColumns") + "...");

            try
            {
                var result = await RetrieveColumnsTask(e.Data);

                if (!progress.IsBreaked)
                {
                    ((StiDataStoreSourceEditWindow)sender).ApplyResultAfterRetrieveColumns(result);
                }
            }
            catch (Exception ex)
            {
                StiMessageBox.Show(this, ex.Message, string.Empty, MessageBoxButton.OK, MessageBoxImage.Error);
            }

            progress.Hide();
        }

        #endregion

        #region Opening Report

        private Task<byte[]> LoadReportTask()
        {
            return Task.Run(() =>
            {
                var service = new ServiceReference1.DesignerServiceClient();
                return service.LoadReport();
            });
        }

        private async void WCFService_WCFOpeningReportInDesigner(object sender, Stimulsoft.Report.Events.StiWCFOpeningReportEventArgs e)
        {
            e.Handled = true;
            progress.Start(StiLocalization.Get("DesignerFx", "LoadingReport") + "...");

            try
            {
                var result = await LoadReportTask();

                if (result != null)
                {
                    var report = new StiReport();
                    report.Load(result);
                    designer.Report = report;
                }
            }
            catch (Exception ex)
            {
                StiMessageBox.Show(this, ex.Message, string.Empty, MessageBoxButton.OK, MessageBoxImage.Error);
            }

            progress.Hide();
        }

        #endregion

        #region Saving Report

        private Task<bool> SaveReportTask(byte[] array)
        {
            return Task.Run(() =>
            {
                var service = new ServiceReference1.DesignerServiceClient();
                return service.SaveReport(array);
            });
        }

        private async void GlobalEvents_SavingReportInDesigner(object sender, Stimulsoft.Report.Design.StiSavingObjectEventArgs e)
        {
            progress.Start(StiLocalization.Get("Report", "SavingReport") + "...");

            try
            {
                await SaveReportTask(designer.Report.SaveToByteArray());
            }
            catch (Exception ex)
            {
                StiMessageBox.Show(this, ex.Message, string.Empty, MessageBoxButton.OK, MessageBoxImage.Error);
            }

            progress.Hide();
        }

        #endregion

        #region Export Document

        private Task<byte[]> ExportDocumentTask(byte[] data)
        {
            return Task.Run(() =>
            {
                var service = new ServiceReference1.DesignerServiceClient();
                return service.ExportDocument(data);
            });
        }

        private async void WCFService_WCFExportDocument(object sender, Stimulsoft.Report.Events.StiWCFExportEventArgs e)
        {
            progress.Start("Export Report...");

            try
            {
                var result = await ExportDocumentTask(e.Data);

                if (result != null)
                {
                    var saveFileDialog = new SaveFileDialog
                    {
                        Filter = string.Format("Export Document (*.{0})|*.{0}", e.Filter)
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
            catch (Exception ex)
            {
                StiMessageBox.Show(this, ex.Message, string.Empty, MessageBoxButton.OK, MessageBoxImage.Error);
            }

            progress.Hide();
        }
        #endregion

        #region ReportCheck

        private Task<byte[]> CheckReportTask(byte[] data)
        {
            return Task.Run(() =>
            {
                var service = new ServiceReference1.DesignerServiceClient();
                return service.CheckReport(data);
            });
        }

        private async void WCFService_WCFReportCheck(IStiCheckStatusControl sender, Stimulsoft.Report.Events.StiWCFReportCheckEventArgs e)
        {
            progress.Start("Report Check...");

            try
            {
                var result = await CheckReportTask(e.Data);

                sender.ApplyResultAfterReportCheck(result);
            }
            catch (Exception ex)
            {
                StiMessageBox.Show(this, ex.Message, string.Empty, MessageBoxButton.OK, MessageBoxImage.Error);
            }

            progress.Hide();
        }

        #endregion

        #region RenderingInteractions
        private Task<byte[]> RenderingInteractionsTask(byte[] data)
        {
            return Task.Run(() =>
            {
                var service = new ServiceReference1.DesignerServiceClient();
                return service.RenderingInteractions(data);
            });
        }

        private async void WCFService_WCFRenderingInteractions(object viewer, Stimulsoft.Report.Events.StiWCFRenderingInteractionsEventArgs e)
        {
            progress.Start(StiLocalization.Get("DesignerFx", "CompilingReport") + "...");

            try
            {
                var result = await RenderingInteractionsTask(e.Data);

                if (result != null)
                {
                    switch (e.InteractionType)
                    {
                        case StiInteractionType.Collapsing:
                            ((StiWpfViewerControl)viewer).ApplyChangesAfterCollapsing(result);
                            break;

                        case StiInteractionType.DrillDownPage:
                            ((StiWpfViewerControl)viewer).ApplyChangesAfterDrillDownPage(result, e.DrillDownMode, e.Page);
                            break;

                        case StiInteractionType.Sorting:
                            ((StiWpfViewerControl)viewer).ApplyChangesAfterSorting(result);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                StiMessageBox.Show(this, ex.Message, string.Empty, MessageBoxButton.OK, MessageBoxImage.Error);
            }

            progress.Hide();
        }
        #endregion

        #region RequestFromUserRenderReport

        private Task<byte[]> RequestFromUserRenderReportTask(byte[] data)
        {
            return Task.Run(() =>
            {
                var service = new ServiceReference1.DesignerServiceClient();
                return service.RequestFromUserRenderReport(data);
            });
        }

        private async void WCFService_WCFRequestFromUserRenderReport(object sender, Stimulsoft.Report.Events.StiWCFEventArgs e)
        {
            progress.Start(StiLocalization.Get("DesignerFx", "CompilingReport") + "...");

            try
            {
                var result = await RequestFromUserRenderReportTask(e.Data);

                if (result != null)
                    ((StiWpfViewerControl)sender).ApplyRenderedReport(result, true);
            }
            catch (Exception ex)
            {
                StiMessageBox.Show(this, ex.Message, string.Empty, MessageBoxButton.OK, MessageBoxImage.Error);
            }

            progress.Hide();
        }
        #endregion

        #region WCFPrepareRequestFromUserVariables

        private Task<byte[]> PrepareRequestFromUserVariablesTask(byte[] data)
        {
            return Task.Run(() =>
            {
                var service = new ServiceReference1.DesignerServiceClient();
                return service.PrepareRequestFromUserVariables(data);
            });
        }

        private async void WCFService_WCFPrepareRequestFromUserVariables(object sender, Stimulsoft.Report.Events.StiWCFEventArgs e)
        {
            progress.Start("Prepare Variables...");

            try
            {
                var result = await PrepareRequestFromUserVariablesTask(e.Data);

                ((StiWpfViewerControl)sender).ApplyResultAfterPrepareRequestFromUserVariables(null, result);
            }
            catch (Exception ex)
            {
                ((StiWpfViewerControl)sender).ApplyResultAfterPrepareRequestFromUserVariables(ex, null);
            }

            progress.Hide();
        }

        #endregion

        #region WCFInteractiveDataBandSelection

        private Task<byte[]> InteractiveDataBandSelectionTask(byte[] data)
        {
            return Task.Run(() =>
            {
                var service = new ServiceReference1.DesignerServiceClient();
                return service.InteractiveDataBandSelection(data);
            });
        }

        private async void WCFService_WCFInteractiveDataBandSelection(object sender, Stimulsoft.Report.Events.StiWCFEventArgs e)
        {
            progress.Start(StiLocalization.Get("DesignerFx", "CompilingReport") + "...");

            try
            {
                var result = await InteractiveDataBandSelectionTask(e.Data);

                if (!this.progress.IsBreaked)
                    ((StiWpfViewerControl)sender).ApplyChangesAfterInteractiveDataBandSelection(result);
            }
            catch (Exception ex)
            {
                StiMessageBox.Show(this, ex.Message, string.Empty, MessageBoxButton.OK, MessageBoxImage.Error);
            }

            progress.Hide();
        }
        #endregion

        #endregion

        #region Handlers

        private Task<byte[]> LoadConfigurationTask()
        {
            return Task.Run(() =>
            {
                var service = new DesignerServiceClient();
                return service.LoadConfiguration();
            });
        }

        private async void MainWindowLoaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= MainWindowLoaded;

            try
            {
                var result = await LoadConfigurationTask();

                designer.ApplyLoadConfiguration(result, null);
                LoadReport();
            }
            catch (Exception ex)
            {
                designer.ApplyLoadConfiguration(null, ex);
            }
        }
        #endregion

        #region Methods
        private void LoadReport()
        {
            var dt = new DataSet();
            dt.ReadXml("../../Data/Demo.xml");
            //dt.ReadXmlSchema("../../Data/Demo.xsd");

            var report = new StiReport();
            report.Load("../../Data/1. Master-Detail.mrt");
            report.RegData("Demo", "Demo", dt);
            //report.RenderWithWpf(false);

            designer.Report = report;
        }
        #endregion

        public MainWindow()
        {
            StiOptions.WCFService.UseWCFService = true;
            progress = new StiProgressInformation(this)
            {
                IsDialog = false, 
                IsMarquee = true
            };

            // Designer
            StiOptions.WCFService.WCFRenderReport += WCFService_WCFRenderReport; // ok
            StiOptions.WCFService.WCFTestConnection += WCFService_WCFTestConnection; // ok
            StiOptions.WCFService.WCFBuildObjects += WCFService_WCFBuildObjects; // ok
            StiOptions.WCFService.WCFRetrieveColumns += WCFService_WCFRetrieveColumns; // ok
            StiOptions.WCFService.WCFOpeningReportInDesigner += WCFService_WCFOpeningReportInDesigner; // ok
            StiOptions.WCFService.WCFRequestFromUserRenderReport += WCFService_WCFRequestFromUserRenderReport; // ok
            StiOptions.WCFService.WCFReportCheck += WCFService_WCFReportCheck; // ok
            StiOptions.Engine.GlobalEvents.SavingReportInDesigner += GlobalEvents_SavingReportInDesigner; // ok

            // Interactions
            StiOptions.WCFService.WCFRenderingInteractions += WCFService_WCFRenderingInteractions; // ok

            // Viewer
            StiOptions.WCFService.WCFExportDocument += WCFService_WCFExportDocument; // ok

            // Prepare RequestFromUser Variables
            StiOptions.WCFService.WCFPrepareRequestFromUserVariables += WCFService_WCFPrepareRequestFromUserVariables; // ok
            StiOptions.WCFService.WCFInteractiveDataBandSelection += WCFService_WCFInteractiveDataBandSelection;

            InitializeComponent();

            this.designer = new StiWpfDesignerControl();
            panelRoot.Content = this.designer;
        }
    }
}