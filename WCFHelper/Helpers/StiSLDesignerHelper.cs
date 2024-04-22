using Stimulsoft.Report;
using Stimulsoft.Report.Check;
using Stimulsoft.Report.Dictionary;
using Stimulsoft.Report.WCFService;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;

namespace WCFHelper
{
    public static class StiSLDesignerHelper
    {
        #region Methods.Render
        public static byte[] RenderReport(byte[] data, DataSet previewDataSet)
        {
            if (data != null)
            {
                var report = new StiReport();
                report.Load(data);

                if (previewDataSet != null)
                    report.RegData("Demo", previewDataSet);

                if (!report.IsRendered)
                {
                    try
                    {
                        report.Compile();
                    }
                    catch
                    {
                    }
                }

                try
                {
                    report.Render(false);
                }
                catch
                {
                }

                return StiSLRenderingReportHelper.CheckReportOnInteractions(report, true);
            }

            return null;
        }
        #endregion

        #region Methods.LoadConfiguration
        public static byte[] LoadConfiguration()
        {
            using (var stream = new MemoryStream())
            using (var writer = new StiBinaryWriter(stream))
            {
                var datas = StiConfig.Services.GetServices(typeof(StiDatabase)).ToList().Where(x => !(x is StiUndefinedDatabase) && x.ServiceEnabled).ToList();

                writer.Write(datas.Count);
                foreach (StiDatabase data in datas)
                {
                    writer.WriteNullableString(data.GetType().FullName);
                }

                writer.Flush();
                return stream.ToArray();
            }
        }
        #endregion

        #region Methods.TestConnection
        public static byte[] TestConnection(byte[] data)
        {
            var settings = StiDatabaseBuildHelper.Input.ParseTestConnection(data);
            string message = (settings.Adapter == null) ? "Type is not found" : settings.Adapter.TestConnection(settings.ConnectionString);

            using (var stream = new MemoryStream())
            using (var writer = new StiBinaryWriter(stream))
            {
                writer.WriteNullableString(message);

                writer.Flush();
                return stream.ToArray();
            }
        }
        #endregion

        #region Methods.BuildObjects
        public static byte[] BuildObjects(byte[] data)
        {
            var database = StiDatabaseBuildHelper.Input.ParseBuildObjects(data);

            if (database != null)
            {
                var info = database.GetDatabaseInformation();
                if (info != null)
                    return StiDatabaseBuildHelper.Output.ParseBuildObjects(info);
            }

            return null;
        }
        #endregion

        #region Methods.RetrieveColumns
        public static byte[] RetrieveColumns(byte[] buffer)
        {
            StiDataColumnsCollection columns = null;
            byte[] result = null;

            try
            {
                var settingsRetrieveColumns = StiDatabaseBuildHelper.Input.ParseRetrieveColumns(buffer);

                settingsRetrieveColumns.connection.ConnectionString = settingsRetrieveColumns.ConnectionString;
                var data = new StiData(settingsRetrieveColumns.Name, settingsRetrieveColumns.connection);

                settingsRetrieveColumns.dataSource.NameInSource = settingsRetrieveColumns.NameInSource;
                settingsRetrieveColumns.dataSource.Name = settingsRetrieveColumns.Name;
                settingsRetrieveColumns.dataSource.Alias = settingsRetrieveColumns.Alias;
                settingsRetrieveColumns.dataSource.SqlCommand = settingsRetrieveColumns.SqlCommand;
                settingsRetrieveColumns.dataSource.Dictionary = new StiDictionary(settingsRetrieveColumns.Report);

                columns = settingsRetrieveColumns.adapter.GetColumnsFromData(data, settingsRetrieveColumns.dataSource);
            }
            finally
            {
                result = StiDatabaseBuildHelper.Output.ParseRetrieveColumns(columns);
            }

            return result;
        }
        #endregion

        #region Methods.ReportScript
        public static byte[] OpenReportScript(byte[] data)
        {
            using (var report = new StiReport())
            {
                report.Load(data);

                report.ScriptUnpack();

                using (var stream = new MemoryStream())
                using (var writer = new StiBinaryWriter(stream))
                {
                    writer.WriteNullableString(report.Script);

                    writer.Flush();
                    return stream.ToArray();
                }
            }
        }

        public static byte[] SaveReportScript(byte[] data)
        {
            using (var report = StiSLRenderingReportHelper.ParseBinarySaveReportScript(data))
            {
                report.ScriptPack();

                using (var stream = new MemoryStream())
                using (var writer = new StiBinaryWriter(stream))
                {
                    writer.WriteNullableString(report.Script);

                    writer.Flush();
                    return stream.ToArray();
                }
            }
        }

        public static byte[] CheckReport(byte[] data)
        {
            byte[] reportByteArray = null;
            List<string> disabledChecks = null;

            using (var stream = new MemoryStream(data))
            using (var reader = new StiBinaryReader(stream))
            {
                reportByteArray = reader.ReadByteArray();
                disabledChecks = reader.ReadListString();
            }

            var report = new StiReport();
            report.Load(reportByteArray);

            #region Parse Disabled Checks
            if (disabledChecks != null)
            {
                foreach (var key in disabledChecks)
                {
                    StiSettings.Set("ReportChecks", key, false);
                }
            }
            #endregion

            #region Compile report in any case

            try
            {
                report.Compile();
            }
            catch
            {
            }

            var engine = new StiCheckEngine();
            var checks = engine.CheckReport(report);

            #endregion

            #region Preparing summary

            var container = new List<StiCheckObject>();

            foreach (var check in checks)
            {
                #region CheckStatus

                var obj = new StiCheckObject
                {
                    Name = check.GetType().Name,
                    ElementName = check.ElementName,
                    ObjectType = (StiWCFCheckObjectType)((int)check.ObjectType)
                };

                #region Additional properties

                #region StiCompilationErrorCheck

                if (check is StiCompilationErrorCheck)
                {
                    var check1 = (StiCompilationErrorCheck)check;

                    obj.Error = check1.Error;
                    obj.ComponentName = check1.ComponentName;
                    obj.PropertyName = check1.PropertyName;
                }

                #endregion

                #region StiKeysNotFoundRelationCheck

                if (check is StiKeysNotFoundRelationCheck)
                {
                    var check1 = (StiKeysNotFoundRelationCheck)check;

                    obj.Columns = check1.Columns;
                }

                #endregion

                #region StiKeysTypesMismatchDataRelationCheck

                if (check is StiKeysTypesMismatchDataRelationCheck)
                {
                    var check1 = (StiKeysTypesMismatchDataRelationCheck)check;

                    obj.Columns = check1.Columns;
                }

                #endregion

                #region StiNoNameDataRelationCheck

                if (check is StiNoNameDataRelationCheck)
                {
                    var check1 = (StiNoNameDataRelationCheck)check;

                    obj.DataSources = check1.DataSources;
                }

                #endregion

                #region StiLostPointsOnPageCheck

                if (check is StiLostPointsOnPageCheck)
                {
                    var check1 = (StiLostPointsOnPageCheck)check;

                    obj.LostPointsNames = check1.LostPointsNames;
                }

                #endregion

                #region StiDuplicatedNameCheck

                if (check is StiDuplicatedNameCheck)
                {
                    var check1 = (StiDuplicatedNameCheck)check;

                    obj.IsDataSource = check1.IsDataSource;
                }

                #endregion

                #region StiDuplicatedNameInSourceInDataRelationReportCheck

                if (check is StiDuplicatedNameInSourceInDataRelationReportCheck)
                {
                    var check1 = (StiDuplicatedNameInSourceInDataRelationReportCheck)check;

                    obj.RelationsNames = check1.RelationsNames;
                    obj.RelationsNameInSource = check1.RelationsNameInSource;
                }

                #endregion

                #endregion

                if (check.Actions.Count > 0)
                {
                    obj.Actions = new List<string>();
                    foreach (var action in check.Actions)
                    {
                        obj.Actions.Add(action.GetType().Name);
                    }
                }

                container.Add(obj);

                #endregion
            }

            #endregion

            #region Save
            using (var stream = new MemoryStream())
            using (var writer = new StiBinaryWriter(stream))
            {
                writer.Write(container.Count);
                foreach (var check in container)
                {
                    writer.WriteNullableString(check.Name);
                    writer.WriteNullableString(check.ElementName);
                    writer.WriteNullableString(check.ObjectType.ToString());

                    writer.WriteBool(check.Error != null);
                    if (check.Error != null)
                    {
                        writer.WriteNullableString(check.Error.FileName);
                        writer.Write(check.Error.Line);
                        writer.Write(check.Error.Column);
                        writer.WriteNullableString(check.Error.ErrorNumber);
                        writer.WriteNullableString(check.Error.ErrorText);
                    }

                    writer.WriteNullableString(check.ComponentName);
                    writer.WriteNullableString(check.PropertyName);

                    writer.WriteNullableString(check.Columns);
                    writer.WriteNullableString(check.DataSources);

                    writer.WriteNullableString(check.LostPointsNames);
                    writer.WriteBool(check.IsDataSource);

                    writer.WriteNullableString(check.RelationsNames);
                    writer.WriteNullableString(check.RelationsNameInSource);

                    writer.WriteListString(check.Actions);
                }

                writer.Flush();
                return stream.ToArray();
            }
            #endregion
        }

        #endregion
    }
}