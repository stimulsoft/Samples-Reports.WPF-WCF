using Stimulsoft.Base;
using Stimulsoft.Report;
using Stimulsoft.Report.Components;
using Stimulsoft.Report.Dictionary;
using Stimulsoft.Report.WCFService;
using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;

namespace WCFHelper
{
    public static class StiSLRenderingReportHelper
    {
        #region Methods.RenderingInteractions
        public static byte[] RenderingInteractions(byte[] data, DataSet previewDataSet)
        {
            var drillDownContainer = DecodeBinaryRenderingInteractions(data);

            byte[] result = null;
            switch (drillDownContainer.TypeDrillDown)
            {
                case "Sorting":
                    result = StartSorting(drillDownContainer, previewDataSet);
                    break;

                case "DrillDownPage":
                    result = StartDrillDownPage(drillDownContainer, previewDataSet);
                    break;

                case "Collapsing":
                    result = StartCollapsing(drillDownContainer, previewDataSet);
                    break;
            }

            return result;
        }

        private static StiDrillDownContainer DecodeBinaryRenderingInteractions(byte[] data)
        {
            var drillDownContainer = new StiDrillDownContainer();

            using (var stream = new MemoryStream(data))
            using (var reader = new StiBinaryReader(stream))
            {
                drillDownContainer.TypeDrillDown = reader.ReadNullableString();
                drillDownContainer.Report.Load(reader.ReadByteArray());
                drillDownContainer.DrillDownPageName = reader.ReadNullableString();
                drillDownContainer.CollapsingIndex = reader.ReadInt32();
                drillDownContainer.IsCollapsed = reader.ReadBoolean();
                drillDownContainer.PageIndex = reader.ReadInt32();
                drillDownContainer.CompIndex = reader.ReadInt32();

                if (reader.ReadBoolean())
                {
                    int count = reader.ReadInt32();
                    for (int index = 0; index < count; index++)
                    {
                        string key = reader.ReadNullableString();
                        string value = reader.ReadNullableString();

                        drillDownContainer.DrillDownParameters.Add(key, value);
                    }
                }

                drillDownContainer.DataBandName = reader.ReadNullableString();
                drillDownContainer.DataBandColumns = reader.ReadArrayString();
                drillDownContainer.DataBandColumnString = reader.ReadNullableString();

                var sortingDirection = reader.ReadNullableString();
                if (sortingDirection != null)
                    drillDownContainer.SortingDirection = (StiInteractionSortDirection)Enum.Parse(typeof(StiInteractionSortDirection), sortingDirection);

                drillDownContainer.IsControlPress = reader.ReadBoolean();

                if (reader.ReadBoolean())
                {
                    drillDownContainer.InteractionCollapsingStates = new Hashtable();

                    int count = reader.ReadInt32();
                    for (int index = 0; index < count; index++)
                    {
                        var compName = reader.ReadNullableString();
                        if (!drillDownContainer.InteractionCollapsingStates.ContainsKey(compName))
                        {
                            drillDownContainer.InteractionCollapsingStates.Add(compName, new Hashtable());
                        }
                        var currentTable = (Hashtable)drillDownContainer.InteractionCollapsingStates[compName];

                        int count2 = reader.ReadInt32();
                        for (int index2 = 0; index2 < count2; index2++)
                        {
                            int keyValue = reader.ReadInt32();
                            int value = reader.ReadInt32();
                            switch (value)
                            {
                                case 1:
                                    currentTable.Add(keyValue, true);
                                    break;

                                case 0:
                                    currentTable.Add(keyValue, false);
                                    break;

                                default:
                                    currentTable.Add(keyValue, null);
                                    break;
                            }
                        }
                    }
                }
            }

            return drillDownContainer;
        }

        private static byte[] StartDrillDownPage(StiDrillDownContainer drillDownContainer, System.Data.DataSet ds)
        {
            drillDownContainer.Report.RegData(ds);

            drillDownContainer.Report.Compile();
            drillDownContainer.Report.Render(false);
            StiReport compileReport = StiActivator.CreateObject(drillDownContainer.Report.CompiledReport.GetType()) as StiReport;

            compileReport.RegData(drillDownContainer.Report.DataStore);
            compileReport.RegBusinessObject(drillDownContainer.Report.BusinessObjectsStore);

            #region Drill-Down Page
            StiPage drillDownPage = null;

            foreach (StiPage newPage in compileReport.Pages)
            {
                if (newPage.Name == drillDownContainer.DrillDownPageName)
                {
                    newPage.Enabled = true;
                    newPage.Skip = false;
                    drillDownPage = newPage;
                }
                else
                    newPage.Enabled = false;
            }

            #region Clear any reference to drill-down page from other components in report
            //We need do this because during report rendering drill-down pages is skipped
            var comps = compileReport.GetComponents();
            foreach (StiComponent comp in comps)
            {
                if (comp.Interaction != null && comp.Interaction.DrillDownEnabled && comp.Interaction.DrillDownPageGuid == drillDownPage.Guid)
                {
                    comp.Interaction.DrillDownPage = null;
                }
            }
            #endregion

            #region Set DrillDownParameters
            StiPage renderedPage = drillDownContainer.Report.RenderedPages[drillDownContainer.PageIndex];
            StiComponent interactionComp = renderedPage.Components[drillDownContainer.CompIndex];

            if (interactionComp != null && interactionComp.DrillDownParameters != null)
            {
                foreach (var entry in interactionComp.DrillDownParameters)
                {
                    compileReport[entry.Key] = entry.Value;
                }
            }
            #endregion
            #endregion

            compileReport.IsInteractionRendering = true;

            bool error = false;
            try
            {
                compileReport.Render(false);
            }
            catch
            {
                error = true;
            }

            if (error)
            {
                return null;
            }
            else
            {
                compileReport.IsInteractionRendering = false;
                return CheckReportOnInteractions(compileReport, false);
            }
        }

        private static byte[] StartCollapsing(StiDrillDownContainer drillDownContainer, System.Data.DataSet ds)
        {
            drillDownContainer.Report.RegData(ds);

            drillDownContainer.Report.Compile();
            drillDownContainer.Report.CompiledReport.InteractionCollapsingStates = drillDownContainer.InteractionCollapsingStates;
            drillDownContainer.Report.Render(false);

            StiReport compileReport = drillDownContainer.Report.CompiledReport == null ? drillDownContainer.Report : drillDownContainer.Report.CompiledReport;
            StiComponent interactionComp = compileReport.RenderedPages[drillDownContainer.PageIndex].Components[drillDownContainer.CompIndex];

            if (interactionComp != null)
            {
                if (compileReport.InteractionCollapsingStates == null)
                    compileReport.InteractionCollapsingStates = new Hashtable();

                var list = compileReport.InteractionCollapsingStates[interactionComp.Name] as Hashtable;
                if (list == null)
                {
                    list = new Hashtable();
                    compileReport.InteractionCollapsingStates[interactionComp.Name] = list;
                }
                list[drillDownContainer.CollapsingIndex] = drillDownContainer.IsCollapsed;

                #region Render Report
                try
                {
                    drillDownContainer.Report.IsInteractionRendering = true;
                    compileReport.IsInteractionRendering = true;
                    compileReport.Render(false);
                }
                finally
                {
                    compileReport.IsInteractionRendering = false;
                }
                #endregion

                return CheckReportOnInteractions(compileReport, false);
            }
            else
            {
                return null;
            }
        }

        private static byte[] StartSorting(StiDrillDownContainer drillDownContainer, DataSet ds)
        {
            drillDownContainer.Report.RegData(ds);

            drillDownContainer.Report.Compile();
            drillDownContainer.Report.Render(false);

            var compileReport = drillDownContainer.Report.CompiledReport == null ? drillDownContainer.Report : drillDownContainer.Report.CompiledReport;

            var interactionComp = compileReport.RenderedPages[drillDownContainer.PageIndex].Components[drillDownContainer.CompIndex];
            interactionComp.Interaction.SortingDirection = drillDownContainer.SortingDirection;

            StiDataBand dataBand = null;
            if (interactionComp is Stimulsoft.Report.Components.Table.IStiTableComponent)
            {
                dataBand = ((Stimulsoft.Report.Components.Table.IStiTableCell)interactionComp).TableTag as StiDataBand;
            }
            else
            {
                dataBand = compileReport.GetComponentByName(drillDownContainer.DataBandName) as StiDataBand;
            }

            if (dataBand != null)
            {
                if (StiOptions.Engine.Interaction.ForceSortingWithFullTypeConversion)
                {
                    if (drillDownContainer.Report.CalculationMode == StiCalculationMode.Interpretation || StiOptions.Engine.ForceInterpretationMode)
                    {
                        if (dataBand.DataSource != null)
                            drillDownContainer.DataBandColumnString = "{" + StiNameValidator.CorrectName(dataBand.DataSource.Name, drillDownContainer.Report) + "." + drillDownContainer.DataBandColumnString + "}";
                    }
                    else
                    {
                        drillDownContainer.DataBandColumnString = "{Get" + StiNameValidator.CorrectName(dataBand.Name + "." + drillDownContainer.DataBandColumnString, drillDownContainer.Report) + "_Sort}";
                    }
                }

                #region Set Sorting
                if (dataBand.Sort == null || dataBand.Sort.Length == 0)
                {
                    dataBand.Sort = StiSortHelper.AddColumnToSorting(dataBand.Sort, drillDownContainer.DataBandColumnString, true);
                }
                else
                {
                    int sortIndex = StiSortHelper.GetColumnIndexInSorting(dataBand.Sort, drillDownContainer.DataBandColumnString);
                    if (drillDownContainer.IsControlPress)
                    {
                        dataBand.Sort = (sortIndex == -1)
                            ? StiSortHelper.AddColumnToSorting(dataBand.Sort, drillDownContainer.DataBandColumnString, true)
                            : StiSortHelper.ChangeColumnSortDirection(dataBand.Sort, drillDownContainer.DataBandColumnString);
                    }
                    else
                    {
                        if (sortIndex != -1)
                        {
                            var direction = StiSortHelper.GetColumnSortDirection(dataBand.Sort, drillDownContainer.DataBandColumnString);
                            direction = (direction == StiInteractionSortDirection.Ascending) 
                                ? StiInteractionSortDirection.Descending
                                : StiInteractionSortDirection.Ascending;

                            dataBand.Sort = StiSortHelper.AddColumnToSorting(new string[0],
                                drillDownContainer.DataBandColumnString, 
                                direction == StiInteractionSortDirection.Ascending);
                        }
                        else
                        {
                            dataBand.Sort = StiSortHelper.AddColumnToSorting(new string[0], 
                                drillDownContainer.DataBandColumnString, 
                                drillDownContainer.SortingDirection == StiInteractionSortDirection.Ascending);
                        }
                    }
                }
                #endregion

                #region Render Report
                try
                {
                    drillDownContainer.Report.IsInteractionRendering = true;
                    drillDownContainer.Report.CompiledReport.IsInteractionRendering = true;
                    compileReport.Render(false);

                    RefreshInteractions(drillDownContainer.Report);
                }
                finally
                {
                    drillDownContainer.Report.IsInteractionRendering = false;
                    drillDownContainer.Report.CompiledReport.IsInteractionRendering = false;
                    interactionComp = drillDownContainer.Report.CompiledReport.RenderedPages[drillDownContainer.PageIndex].Components[drillDownContainer.CompIndex];
                    interactionComp.Interaction.SortingDirection = drillDownContainer.SortingDirection;
                }
                #endregion

                compileReport = drillDownContainer.Report.CompiledReport == null ? drillDownContainer.Report : drillDownContainer.Report.CompiledReport;
                return CheckReportOnInteractions(drillDownContainer.Report, false);
            }

            return null;
        }

        public static byte[] CheckReportOnInteractions(StiReport report, bool useBaseReport)
        {
            using (var stream = new MemoryStream())
            using (var writer = new StiBinaryWriter(stream))
            {
                writer.WriteBool(report.CompilerResults != null && report.CompilerResults.Errors.Count > 0);
                if (report.CompilerResults != null && report.CompilerResults.Errors.Count > 0)
                {
                    writer.Write(report.CompilerResults.Errors.Count);

                    foreach (CompilerError error in report.CompilerResults.Errors)
                    {
                        writer.WriteNullableString(error.ErrorText);
                    }

                    writer.Flush();
                    return stream.ToArray();
                }

                var listComps = new List<StiComponent>();

                #region Search Components
                bool isSort = false;
                foreach (StiPage page in report.RenderedPages)
                {
                    foreach (StiComponent comp in page.Components)
                    {
                        var interaction = comp.Interaction;
                        var bandInteraction = comp.Interaction as StiBandInteraction;

                        if (interaction != null)
                        {
                            if ((!string.IsNullOrEmpty(interaction.SortingColumn) ||
                                interaction.DrillDownEnabled && !string.IsNullOrEmpty(interaction.DrillDownPageGuid) ||
                                (bandInteraction != null && (bandInteraction.CollapsingEnabled || bandInteraction.SelectionEnabled))))
                            {
                                comp.Guid = string.Format("Guid_{0}", Guid.NewGuid().ToString().Replace("-", ""));

                                if (!isSort && interaction.SortingDirection != StiInteractionSortDirection.None)
                                {
                                    isSort = true;
                                }

                                listComps.Add(comp);
                            }
                        }
                    }
                }
                #endregion

                #region Check Variables
                bool isRequestFromUser = false;
                foreach (StiVariable variable in report.Dictionary.Variables)
                {
                    if (variable.RequestFromUser)
                    {
                        isRequestFromUser = true;
                        break;
                    }
                }
                #endregion

                #region Set Sorting
                if (!isSort)
                {
                    foreach (StiComponent comp in listComps)
                    {
                        if (!string.IsNullOrEmpty(comp.Interaction.SortingColumn))
                        {
                            comp.Interaction.SortingDirection = StiInteractionSortDirection.Ascending;
                            break;
                        }
                    }
                }
                #endregion

                #region Report
                var compileReport = report.CompiledReport == null ? report : report.CompiledReport;

                writer.WriteByteArray(compileReport.SaveDocumentToByteArray());
                #endregion

                #region BaseReport
                byte[] baseReport = null;
                if (useBaseReport && (listComps.Count > 0 || isRequestFromUser))
                    baseReport = report.SaveToByteArray();
                writer.WriteByteArray(baseReport);
                #endregion

                #region Comps
                writer.WriteBool(listComps.Count > 0);
                if (listComps.Count > 0)
                {
                    writer.Write(listComps.Count);
                    foreach (StiComponent comp in listComps)
                    {
                        writer.WriteNullableString(comp.Guid);

                        #region StiInteraction
                        var interaction = comp.Interaction;

                        writer.WriteNullableString(interaction is StiBandInteraction ? "StiBandInteraction" : "StiInteraction");

                        writer.WriteBool(interaction is StiBandInteraction);
                        if (interaction is StiBandInteraction bandInteraction)
                        {
                            writer.WriteBool(bandInteraction.CollapseGroupFooter);
                            writer.WriteBool(bandInteraction.CollapsingEnabled);
                            writer.WriteBool(bandInteraction.SelectionEnabled);
                        }

                        writer.WriteNullableString(comp.Page.Guid);
                        writer.WriteBool(interaction.DrillDownEnabled);
                        writer.WriteNullableString(interaction.DrillDownPageGuid);
                        writer.WriteNullableString(interaction.SortingColumn);
                        writer.Write((int)interaction.SortingDirection);
                        writer.WriteBool(interaction.SortingEnabled);
                        writer.Write(interaction.SortingIndex);
                        #endregion

                        #region Collapsing
                        var container = comp as StiContainer;
                        writer.WriteBool(container != null);
                        if (container != null)
                        {
                            writer.Write(container.CollapsingIndex);
                            writer.WriteBool(Stimulsoft.Report.Engine.StiDataBandV2Builder.IsCollapsed(container, false));
                        }
                        #endregion

                        #region DrillDownParameters
                        writer.WriteBool(comp.DrillDownParameters != null && comp.DrillDownParameters.Count > 0);
                        if (comp.DrillDownParameters != null && comp.DrillDownParameters.Count > 0)
                        {
                            writer.Write(comp.DrillDownParameters.Count);

                            foreach (string key in comp.DrillDownParameters.Keys)
                            {
                                var obj = comp.DrillDownParameters[key];

                                writer.WriteNullableString(key);
                                writer.WriteNullableString(obj != null ? obj.ToString() : null);
                            }
                        }
                        #endregion
                    }
                }
                #endregion

                #region InteractionCollapsingStates
                var currentReport = (report.CompiledReport == null)
                    ? report
                    : report.CompiledReport;

                writer.WriteBool(currentReport.InteractionCollapsingStates != null && currentReport.InteractionCollapsingStates.Count > 0);
                if (currentReport.InteractionCollapsingStates != null && currentReport.InteractionCollapsingStates.Count > 0)
                {
                    writer.Write(currentReport.InteractionCollapsingStates.Count);

                    foreach (string key in currentReport.InteractionCollapsingStates.Keys)
                    {
                        writer.WriteNullableString(key);

                        var list = currentReport.InteractionCollapsingStates[key] as Hashtable;

                        writer.WriteBool(list != null);
                        if (list != null)
                        {
                            writer.Write(list.Count);
                            foreach (object key1 in list.Keys)
                            {
                                writer.WriteNullableString(key1.ToString());
                                writer.WriteNullableString(list[key1].ToString());
                            }
                        }
                    }
                }
                #endregion

                writer.Flush();
                return stream.ToArray();
            }
        }
        #endregion

        #region Methods.RequestFromUser
        public static byte[] RequestFromUserRenderReport(byte[] data, DataSet previewDataSet)
        {
            var report = DecodeXmlRequestFromUser(data, previewDataSet);

            if (report.CompilerResults != null && report.CompilerResults.Errors.Count > 0)
            {
                using (var stream = new MemoryStream())
                using (var writer = new StiBinaryWriter(stream))
                {
                    writer.WriteBool(report.CompilerResults.Errors.Count > 0);
                    if (report.CompilerResults.Errors.Count > 0)
                    {
                        writer.Write(report.CompilerResults.Errors.Count);
                        foreach (CompilerError compilerError in report.CompilerResults.Errors)
                        {
                            writer.WriteNullableString(compilerError.ErrorText);
                        }

                        writer.Flush();
                        return stream.ToArray();
                    }
                }
            }

            try
            {
                report.Render(false);
            }
            catch
            {
            }

            return CheckReportOnInteractions(report, false);
        }

        public static byte[] PrepareRequestFromUserVariables(byte[] data, DataSet previewDataSet)
        {
            var report = DecodeXmlPrepareRequestFromUserVariables(data, previewDataSet);

            byte[] result = null;
            if (report.CompiledReport != null)
            {
                report.CompiledReport.Dictionary.Connect();
                Stimulsoft.Report.Engine.StiVariableHelper.FillItemsOfVariables(report.CompiledReport != null ? report.CompiledReport : report);

                result = GetPrepareRequestFromUserVariables(report.CompiledReport);
                report.CompiledReport.Dictionary.Disconnect();
            }

            return result;
        }

        private static StiReport DecodeXmlPrepareRequestFromUserVariables(byte[] data, DataSet previewDataSet)
        {
            var report = new StiReport();

            report.Load(data);
            //report.Dictionary.DataSources.Clear();
            //report.Dictionary.Databases.Clear();
            //report.Dictionary.DataStore.Clear();

            if (previewDataSet != null) report.RegData(previewDataSet);
            report.Dictionary.Synchronize();

            try
            {
                report.Compile();
            }
            catch
            {
                return report;
            }

            return report;
        }

        private static byte[] GetPrepareRequestFromUserVariables(StiReport compileReport)
        {
            using (var stream = new MemoryStream())
            using (var writer = new StiBinaryWriter(stream))
            {
                writer.Write(compileReport.Dictionary.Variables.Count);

                foreach (StiVariable variable in compileReport.Dictionary.Variables)
                {
                    var infos = variable.DialogInfo.GetDialogInfoItems(variable.Type);

                    writer.WriteBool(infos != null && infos.Count > 0);
                    if (infos != null && infos.Count > 0)
                    {
                        writer.WriteNullableString(variable.Name);
                        writer.Write(infos.Count);

                        foreach (StiDialogInfoItem info in infos)
                        {
                            string keyObjectTo = (info.KeyObjectTo != null) 
                                ? info.KeyObjectTo.ToString() 
                                : string.Empty;

                            writer.WriteNullableString(info.KeyObject.ToString());
                            writer.WriteNullableString(keyObjectTo);
                            writer.WriteNullableString(info.Value);
                        }
                    }
                }

                writer.Flush();
                return stream.ToArray();
            }
        }

        private static StiReport DecodeXmlRequestFromUser(byte[] data, System.Data.DataSet ds)
        {
            var report = new StiReport();

            using (var stream = new MemoryStream(data))
            using (var reader = new StiBinaryReader(stream))
            {
                #region Report
                report.Load(reader.ReadByteArray());
                if (ds != null) report.RegData("Demo", ds);

                try
                {
                    report.Compile();
                }
                catch
                {
                    return report;
                }
                #endregion

                #region RequestFromUser
                int count = reader.ReadInt32();
                for (int index = 0; index < count; index++)
                {
                    string variableName = reader.ReadNullableString();
                    var variableType = (StiRequestFromUserType)reader.ReadInt32();

                    ParseVariable(report, variableName, variableType, reader);
                }
                #endregion
            }

            return report;
        }

        private static void ParseVariable(StiReport report, string variableName, StiRequestFromUserType type, StiBinaryReader reader)
        {
            var currentReport = (report.CompiledReport != null) ? report.CompiledReport : report;
            var variable = report.Dictionary.Variables[variableName];
            if (variable == null) return;

            Range range = null;
            IStiList list = null;
            object value = null;

            #region Create Type
            switch (type)
            {
                case StiRequestFromUserType.ListBool:
                    list = new BoolList();
                    break;
                case StiRequestFromUserType.ListChar:
                    list = new CharList();
                    break;
                case StiRequestFromUserType.ListDateTime:
                    list = new DateTimeList();
                    break;
                case StiRequestFromUserType.ListTimeSpan:
                    list = new TimeSpanList();
                    break;
                case StiRequestFromUserType.ListDecimal:
                    list = new DecimalList();
                    break;
                case StiRequestFromUserType.ListFloat:
                    list = new FloatList();
                    break;
                case StiRequestFromUserType.ListDouble:
                    list = new DoubleList();
                    break;
                case StiRequestFromUserType.ListByte:
                    list = new ByteList();
                    break;
                case StiRequestFromUserType.ListShort:
                    list = new ShortList();
                    break;
                case StiRequestFromUserType.ListInt:
                    list = new IntList();
                    break;
                case StiRequestFromUserType.ListLong:
                    list = new LongList();
                    break;
                case StiRequestFromUserType.ListGuid:
                    list = new GuidList();
                    break;
                case StiRequestFromUserType.ListString:
                    list = new StringList();
                    break;
                case StiRequestFromUserType.RangeByte:
                    range = new ByteRange();
                    break;
                case StiRequestFromUserType.RangeChar:
                    range = new CharRange();
                    break;
                case StiRequestFromUserType.RangeDateTime:
                    range = new DateTimeRange();
                    break;
                case StiRequestFromUserType.RangeDecimal:
                    range = new DecimalRange();
                    break;
                case StiRequestFromUserType.RangeDouble:
                    range = new DoubleRange();
                    break;
                case StiRequestFromUserType.RangeFloat:
                    range = new FloatRange();
                    break;
                case StiRequestFromUserType.RangeGuid:
                    range = new GuidRange();
                    break;
                case StiRequestFromUserType.RangeInt:
                    range = new IntRange();
                    break;
                case StiRequestFromUserType.RangeLong:
                    range = new LongRange();
                    break;
                case StiRequestFromUserType.RangeShort:
                    range = new ShortRange();
                    break;
                case StiRequestFromUserType.RangeString:
                    range = new StringRange();
                    break;
                case StiRequestFromUserType.RangeTimeSpan:
                    range = new TimeSpanRange();
                    break;
            }
            #endregion

            #region Range
            if (list is Range)
            {
                string from = reader.ReadNullableString();
                string to = reader.ReadNullableString();

                switch (type)
                {
                    case StiRequestFromUserType.RangeByte:
                        range.FromObject = byte.Parse(from);
                        range.ToObject = byte.Parse(to);
                        break;

                    case StiRequestFromUserType.RangeChar:
                        range.FromObject = char.Parse(from);
                        range.ToObject = char.Parse(to);
                        break;

                    case StiRequestFromUserType.RangeDateTime:
                        range.FromObject = DateTime.Parse(from);
                        range.ToObject = DateTime.Parse(to);
                        break;

                    case StiRequestFromUserType.RangeDecimal:
                        range.FromObject = decimal.Parse(from);
                        range.ToObject = decimal.Parse(to);
                        break;

                    case StiRequestFromUserType.RangeDouble:
                        range.FromObject = double.Parse(from);
                        range.ToObject = double.Parse(to);
                        break;

                    case StiRequestFromUserType.RangeFloat:
                        range.FromObject = float.Parse(from);
                        range.ToObject = float.Parse(to);
                        break;

                    case StiRequestFromUserType.RangeGuid:
                        range.FromObject = Guid.Parse(from);
                        range.ToObject = Guid.Parse(to);
                        break;

                    case StiRequestFromUserType.RangeInt:
                        range.FromObject = int.Parse(from);
                        range.ToObject = int.Parse(to);
                        break;
                    case StiRequestFromUserType.RangeLong:
                        range.FromObject = long.Parse(from);
                        range.ToObject = long.Parse(to);
                        break;
                    case StiRequestFromUserType.RangeShort:
                        range.FromObject = short.Parse(from);
                        range.ToObject = short.Parse(to);
                        break;
                    case StiRequestFromUserType.RangeString:
                        range.FromObject = from;
                        range.ToObject = to;
                        break;
                    case StiRequestFromUserType.RangeTimeSpan:
                        range.FromObject = TimeSpan.Parse(from);
                        range.ToObject = TimeSpan.Parse(to);
                        break;
                }
            }
            #endregion

            #region IStiList
            else if (list is IStiList)
            {
                if (!reader.ReadBoolean())
                {
                    var values = reader.ReadListString();
                    if (values != null && values.Count > 0)
                    {
                        foreach (var textValue in values)
                        {
                            switch (type)
                            {
                                case StiRequestFromUserType.ListBool:
                                    list.AddElement(bool.Parse(textValue));
                                    break;
                                case StiRequestFromUserType.ListChar:
                                    list.AddElement(char.Parse(textValue));
                                    break;
                                case StiRequestFromUserType.ListDateTime:
                                    list.AddElement(DateTime.Parse(textValue));
                                    break;
                                case StiRequestFromUserType.ListTimeSpan:
                                    list.AddElement(TimeSpan.Parse(textValue));
                                    break;
                                case StiRequestFromUserType.ListDecimal:
                                    list.AddElement(decimal.Parse(textValue));
                                    break;
                                case StiRequestFromUserType.ListFloat:
                                    list.AddElement(float.Parse(textValue));
                                    break;
                                case StiRequestFromUserType.ListDouble:
                                    list.AddElement(double.Parse(textValue));
                                    break;
                                case StiRequestFromUserType.ListByte:
                                    list.AddElement(byte.Parse(textValue));
                                    break;
                                case StiRequestFromUserType.ListShort:
                                    list.AddElement(short.Parse(textValue));
                                    break;
                                case StiRequestFromUserType.ListInt:
                                    list.AddElement(int.Parse(textValue));
                                    break;
                                case StiRequestFromUserType.ListLong:
                                    list.AddElement(long.Parse(textValue));
                                    break;
                                case StiRequestFromUserType.ListGuid:
                                    list.AddElement(Guid.Parse(textValue));
                                    break;
                                case StiRequestFromUserType.ListString:
                                    list.AddElement(textValue);
                                    break;
                            }
                        }
                    }
                }
            }
            #endregion

            #region Value & Nullable Value
            else
            {
                var valueStr = reader.ReadNullableString();

                #region Nullablealue
                if ((int)type >= (int)StiRequestFromUserType.ValueNullableBool)
                {
                    if (string.IsNullOrEmpty(valueStr))
                    {
                        variable.ValueObject = null;
                    }
                }
                else
                {
                    switch (type)
                    {
                        case StiRequestFromUserType.ValueBool:
                        case StiRequestFromUserType.ValueNullableBool:
                            variable.ValueObject = bool.Parse(valueStr);
                            break;

                        case StiRequestFromUserType.ValueNullableByte:
                        case StiRequestFromUserType.ValueByte:
                            variable.ValueObject = byte.Parse(valueStr);
                            break;

                        case StiRequestFromUserType.ValueNullableChar:
                        case StiRequestFromUserType.ValueChar:
                            variable.ValueObject = char.Parse(valueStr);
                            break;

                        case StiRequestFromUserType.ValueNullableDateTime:
                        case StiRequestFromUserType.ValueDateTime:
                            variable.ValueObject = DateTime.Parse(valueStr);
                            break;

                        case StiRequestFromUserType.ValueNullableDecimal:
                        case StiRequestFromUserType.ValueDecimal:
                            variable.ValueObject = decimal.Parse(valueStr);
                            break;

                        case StiRequestFromUserType.ValueNullableDouble:
                        case StiRequestFromUserType.ValueDouble:
                            variable.ValueObject = double.Parse(valueStr);
                            break;

                        case StiRequestFromUserType.ValueNullableFloat:
                        case StiRequestFromUserType.ValueFloat:
                            variable.ValueObject = float.Parse(valueStr);
                            break;

                        case StiRequestFromUserType.ValueNullableGuid:
                        case StiRequestFromUserType.ValueGuid:
                            variable.ValueObject = Guid.Parse(valueStr);
                            break;

                        case StiRequestFromUserType.ValueNullableInt:
                        case StiRequestFromUserType.ValueInt:
                            variable.ValueObject = int.Parse(valueStr);
                            break;

                        case StiRequestFromUserType.ValueNullableLong:
                        case StiRequestFromUserType.ValueLong:
                            variable.ValueObject = long.Parse(valueStr);
                            break;

                        case StiRequestFromUserType.ValueNullableSbyte:
                        case StiRequestFromUserType.ValueSbyte:
                            variable.ValueObject = sbyte.Parse(valueStr);
                            break;

                        case StiRequestFromUserType.ValueNullableShort:
                        case StiRequestFromUserType.ValueShort:
                            variable.ValueObject = short.Parse(valueStr);
                            break;

                        case StiRequestFromUserType.ValueNullableTimeSpan:
                        case StiRequestFromUserType.ValueTimeSpan:
                            variable.ValueObject = TimeSpan.Parse(valueStr);
                            break;

                        case StiRequestFromUserType.ValueNullableUint:
                        case StiRequestFromUserType.ValueUint:
                            variable.ValueObject = uint.Parse(valueStr);
                            break;

                        case StiRequestFromUserType.ValueNullableUlong:
                        case StiRequestFromUserType.ValueUlong:
                            variable.ValueObject = ulong.Parse(valueStr);
                            break;

                        case StiRequestFromUserType.ValueNullableUshort:
                        case StiRequestFromUserType.ValueUshort:
                            variable.ValueObject = ushort.Parse(valueStr);
                            break;

                        case StiRequestFromUserType.ValueString:
                            variable.ValueObject = valueStr;
                            break;
                        case StiRequestFromUserType.ValueImage:
                            variable.ValueObject = valueStr;
                            break;
                    }

                    #region Reflection
                    var fi = currentReport.GetType().GetField(variable.Name);
                    if (fi != null)
                    {
                        var initBy = variable.InitBy;
                        variable.InitBy = StiVariableInitBy.Value;

                        fi.SetValue(currentReport, variable.ValueObject);

                        variable.InitBy = initBy;
                    }
                    #endregion
                }
                #endregion
            }
            #endregion

            StiReport compileReport = report.CompiledReport;
            if (compileReport != null)
            {
                var field = compileReport.GetType().GetField(variableName);
                if (field != null)
                {
                    if (range != null)
                        field.SetValue(compileReport, range);
                    else if (list != null)
                        field.SetValue(compileReport, list);
                    else if (value != null)
                        field.SetValue(compileReport, value);
                }
            }
        }
        #endregion

        #region Methods.Interactive
        public static byte[] InteractiveDataBandSelection(byte[] data, DataSet previewDataSet)
        {
            var helper = DecodeXmlDataBandSelection(data);
            if (previewDataSet != null) helper.Report.RegData(previewDataSet);

            helper.Report.IsInteractionRendering = true;

            try
            {
                helper.Report.Compile();

                if (helper.Report.CompiledReport != null)
                {
                    int index = -1;
                    while(++index < helper.DataBandNames.Length)
                    {
                        StiDataBand band = helper.Report.CompiledReport.GetComponentByName(helper.DataBandNames[index]) as StiDataBand;
                        if (band != null)
                            band.SelectedLine = helper.SelectedLines[index];
                    }
                }

                helper.Report.Render(false);
                RefreshInteractions(helper.Report);
            }
            catch
            {
            }

            helper.Report.IsInteractionRendering = false;

            var result = CheckReportOnInteractions(helper.Report, true);

            return result;
        }

        private static StiDataBandSelectionContainer DecodeXmlDataBandSelection(byte[] data)
        {
            var helper = new StiDataBandSelectionContainer();

            using (var stream = new MemoryStream(data))
            using (var reader = new StiBinaryReader(stream))
            {
                helper.Report.Load(reader.ReadByteArray());

                int count = reader.ReadInt32();
                helper.DataBandNames = new string[count];
                helper.SelectedLines = new int[count];

                for (int index = 0; index < count; index++)
                {
                    helper.DataBandNames[index] = reader.ReadNullableString();
                    helper.SelectedLines[index] = reader.ReadInt32();
                }
            }

            return helper;
        }

        private static void RefreshInteractions(StiReport report)
        {
            if (report == null || report.RenderedPages.CacheMode) return;

            var templateReport = report.CompiledReport == null ? report : report.CompiledReport;

            #region Create List of DataBand's
            var dataBands = new List<StiDataBand>();
            var comps = templateReport.GetComponents();
            foreach (StiComponent comp in comps)
            {
                var dataBand = comp as StiDataBand;
                if (dataBand != null && dataBand.Sort != null && dataBand.Sort.Length > 0)
                    dataBands.Add(dataBand);
            }
            #endregion

            #region Create List of Interaction's
            var interactions = GetListOfInteractions(report);
            #endregion

            foreach (var dataBand in dataBands)
            {
                var list = interactions[dataBand.Name] as List<StiInteraction>;
                if (list != null)
                {
                    //sort interactions by SortColumns
                    var hashSortColumns = new Hashtable();
                    foreach (var interaction in list)
                    {
                        var str2 = interaction.GetSortColumnsString();
                        var list2 = hashSortColumns[str2] as List<StiInteraction>;
                        if (list2 == null)
                        {
                            list2 = new List<StiInteraction>();
                            hashSortColumns[str2] = list2;
                        }
                        list2.Add(interaction);
                    }

                    #region Process Interaction's for specified DataBand
                    var addedLists = new ArrayList();
                    var sortIndex = 1;
                    var sortStr = "";
                    var isAsc = true;
                    var index = 0;
                    foreach (var str in dataBand.Sort)
                    {
                        #region Add sorting str
                        if (str != "ASC" && str != "DESC")
                        {
                            if (sortStr.Length == 0)
                                sortStr = str;
                            else
                                sortStr += "." + str;
                        }
                        #endregion

                        if (str == "ASC" || str == "DESC" || index == dataBand.Sort.Length - 1)
                        {
                            #region If Sorting string is not empty then process it
                            if (sortStr.Length > 0)
                            {
                                #region Try to Search sorting string in Interaction's
                                foreach (var key in hashSortColumns.Keys)
                                {
                                    var str2 = key as string;
                                    if (sortStr.Contains(str2) || sortStr.Contains(str2.Replace(".", "_")))
                                    {
                                        #region We have finded sorting string
                                        var list2 = hashSortColumns[key] as List<StiInteraction>;
                                        foreach (var interaction in list2)
                                        {
                                            interaction.SortingDirection = isAsc
                                                ? StiInteractionSortDirection.Ascending
                                                : StiInteractionSortDirection.Descending;
                                            interaction.SortingIndex = sortIndex;
                                        }
                                        hashSortColumns.Remove(key);
                                        addedLists.Add(list2);
                                        break;
                                        #endregion
                                    }
                                }
                                #endregion

                                sortStr = "";
                                sortIndex++;

                                //if we don't have more Interaction's in our list for specified 
                                //DataBand then breaks search
                                if (hashSortColumns.Count == 0) break;
                            }
                            #endregion

                            isAsc = str == "ASC";
                        }
                        index++;
                    }
                    #endregion

                    if (addedLists.Count == 1)
                    {
                        var list2 = addedLists[0] as List<StiInteraction>;
                        foreach (var interaction in list2)
                        {
                            interaction.SortingIndex = 0;
                        }
                    }
                }
            }
        }

        private static Hashtable GetListOfInteractions(StiReport report)
        {
            var interactions = new Hashtable();
            foreach (StiPage page in report.RenderedPages)
            {
                var comps2 = page.GetComponents();
                foreach (StiComponent comp in comps2)
                {
                    if (comp is IStiInteraction)
                    {
                        var interaction = ((IStiInteraction)comp).Interaction;
                        if (interaction != null && interaction.SortingEnabled)
                        {
                            var dataBandName = interaction.GetSortDataBandName();
                            var list = interactions[dataBandName] as List<StiInteraction>;
                            if (list == null)
                            {
                                list = new List<StiInteraction>();
                                interactions[dataBandName] = list;
                            }

                            list.Add(interaction);
                        }
                    }
                }
            }

            return interactions;
        }
        #endregion

        #region Methods.SaveReportScript.Input
        public static StiReport ParseBinarySaveReportScript(byte[] data)
        {
            var report = new StiReport();

            using (var stream = new MemoryStream(data))
            using (var reader = new StiBinaryReader(stream))
            {
                var reportData = reader.ReadByteArray();
                report.Load(reportData);

                report.Script = reader.ReadNullableString();
            }
            
            return report;
        }
        #endregion
    }
}