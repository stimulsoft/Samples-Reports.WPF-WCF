using Stimulsoft.Report;
using Stimulsoft.Report.Dictionary;
using Stimulsoft.Report.WCFService;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using WCFHelper.Helpers;

namespace WCFHelper
{
    public static class StiDatabaseBuildHelper
    {
        #region class SettingsTestConnection
        public class SettingsTestConnection
        {
            public string ConnectionString;
            public StiSqlAdapterService Adapter;
        }
        #endregion

        #region class SettingsRetrieveColumns
        public class SettingsRetrieveColumns
        {
            public string Name;
            public string Alias;
            public string NameInSource;
            public string ConnectionString;
            public bool PromptUserNameAndPassword; // sqlDatabase.PromptUserNameAndPassword ? "1" : "0"
            public string SqlCommand;

            public StiSqlAdapterService adapter;
            public StiSqlSource dataSource;
            public DbConnection connection;

            public StiReport Report;
        }
        #endregion

        #region Input
        public static class Input
        {
            public static SettingsTestConnection ParseTestConnection(byte[] data)
            {
                var settings = new SettingsTestConnection();

                using (var stream = new MemoryStream(data))
                using (var reader = new StiBinaryReader(stream))
                {
                    var typeStr = reader.ReadNullableString();
                    if (!string.IsNullOrEmpty(typeStr))
                        settings.Adapter = Stimulsoft.Base.StiActivator.CreateObject(typeStr) as StiSqlAdapterService;

                    settings.ConnectionString = reader.ReadNullableString();
                }

                return settings;
            }

            #region BuildObjects
            public static StiDatabase ParseBuildObjects(byte[] data)
            {
                StiDatabase result = null;

                using (var stream = new MemoryStream(data))
                using (var reader = new StiBinaryReader(stream))
                {
                    string databaseType = reader.ReadNullableString();

                    result = Stimulsoft.Base.StiActivator.CreateObject(databaseType) as StiDatabase;
                    if (result == null) return result;

                    if (result is StiXmlDatabase xml)
                    {
                        xml.Name = reader.ReadNullableString();
                        xml.Alias = reader.ReadNullableString();
                        xml.PathData = reader.ReadNullableString();
                        xml.PathSchema = reader.ReadNullableString();
                    }
                    else if (result is StiSqlDatabase sql)
                    {
                        sql.Name = reader.ReadNullableString();
                        sql.Alias = reader.ReadNullableString();
                        sql.ConnectionString = reader.ReadNullableString();
                        sql.PromptUserNameAndPassword = reader.ReadBoolean();
                    }
                }

                return result;
            }
            #endregion

            #region ParseRetrieveColumns
            public static SettingsRetrieveColumns ParseRetrieveColumns(byte[] data)
            {
                var settings = new SettingsRetrieveColumns();

                using (var stream = new MemoryStream(data))
                using (var reader = new StiBinaryReader(stream))
                {
                    settings.Report = new StiReport();
                    settings.Report.Load(reader.ReadByteArray());

                    #region DataAdapterType
                    var typeStr = reader.ReadNullableString();

                    settings.adapter = Stimulsoft.Base.StiActivator.CreateObject(typeStr) as StiSqlAdapterService;
                    settings.dataSource = Stimulsoft.Base.StiActivator.CreateObject(settings.adapter.GetDataSourceType()) as StiSqlSource;
                    settings.connection = CreateDataAdapterTypeByName(settings.adapter.GetType().Name);
                    #endregion

                    settings.Name = reader.ReadNullableString();
                    settings.Alias = reader.ReadNullableString();
                    settings.NameInSource = reader.ReadNullableString();
                    settings.ConnectionString = reader.ReadNullableString();
                    settings.PromptUserNameAndPassword = reader.ReadBoolean();
                    settings.SqlCommand = reader.ReadNullableString();
                }

                return settings;
            }

            private static DbConnection CreateDataAdapterTypeByName(string name)
            {
                DbConnection result = null;
                switch (name)
                {
                    case "StiSqlAdapterService":
                        result = new SqlConnection();
                        break;

                    case "StiOleDbAdapterService":
                        result = new System.Data.OleDb.OleDbConnection();
                        break;

                    case "StiOdbcAdapterService":
                        result = new System.Data.Odbc.OdbcConnection();
                        break;

                    case "StiMSAccessAdapterService":
                        result = new System.Data.OleDb.OleDbConnection();
                        break;

                    case "StiDB2AdapterService":
                        result = Stimulsoft.Base.StiActivator.CreateObject("IBM.Data.DB2.DB2Connection") as DbConnection;
                        break;

                    case "StiDotConnectUniversalAdapterService":
                        result = Stimulsoft.Base.StiActivator.CreateObject("Devart.Data.Universal.UniConnection") as DbConnection;
                        break;

                    case "StiEffiProzAdapterService":
                        result = Stimulsoft.Base.StiActivator.CreateObject("System.Data.EffiProz.EfzConnection") as DbConnection;
                        break;

                    case "StiFirebirdAdapterService":
                        result = Stimulsoft.Base.StiActivator.CreateObject("FirebirdSql.Data.FirebirdClient.FbConnection") as DbConnection;
                        break;

                    case "StiInformixAdapterService":
                        result = Stimulsoft.Base.StiActivator.CreateObject("IBM.Data.Informix.IfxConnection") as DbConnection;
                        break;

                    case "StiMySqlAdapterService":
                        result = Stimulsoft.Base.StiActivator.CreateObject("MySql.Data.MySqlClient.MySqlConnection") as DbConnection;
                        break;

                    case "StiOracleAdapterService":
                        result = Stimulsoft.Base.StiActivator.CreateObject("System.Data.OracleClient.OracleConnection") as DbConnection;
                        break;

                    case "StiOracleODPAdapterService":
                        result = Stimulsoft.Base.StiActivator.CreateObject("Oracle.DataAccess.Client.OracleConnection") as DbConnection;
                        break;

                    case "StiPostgreSQLAdapterService":
                        result = Stimulsoft.Base.StiActivator.CreateObject("Npgsql.NpgsqlConnection") as DbConnection;
                        break;

                    case "StiSqlCeAdapterService":
                        result = Stimulsoft.Base.StiActivator.CreateObject("System.Data.SqlServerCe.SqlCeConnection") as DbConnection;
                        break;

                    case "StiSQLiteAdapterService":
                        result = Stimulsoft.Base.StiActivator.CreateObject("System.Data.SQLite.SQLiteConnection") as DbConnection;
                        break;

                    case "StiSybaseAdsAdapterService":
                        result = Stimulsoft.Base.StiActivator.CreateObject("Advantage.Data.Provider.AdsConnection") as DbConnection;
                        break;

                    case "StiSybaseAseAdapterService":
                        result = Stimulsoft.Base.StiActivator.CreateObject("Sybase.Data.AseClient.AseConnection") as DbConnection;
                        break;

                    case "StiUniDirectAdapterService":
                        result = Stimulsoft.Base.StiActivator.CreateObject("CoreLab.UniDirect.UniConnection") as DbConnection;
                        break;

                    case "StiVistaDBAdapterService":
                        result = Stimulsoft.Base.StiActivator.CreateObject("VistaDB.Provider.VistaDBConnection") as DbConnection;
                        break;

                    // BrightEye
                    //case "MesDataAdapterService":
                    //    settings.connection = new MesConnection(ServerHost.Current.Domain);
                    //    continue;
                }

                if (result != null)
                    return result;

                // Если не удалось найти тип из стандартных, вызываем событие, чтобы пользователь сам указал тип
                if (CreateCustomDataAdapterType != null)
                {
                    var args = new StiCustomDataAdapterTypeEventArgs(name);
                    CreateCustomDataAdapterType(null, args);
                    return args.Connection;
                }

                return null;
            }
            #endregion

            #region Events
            public static StiCustomDataAdapterTypeEventHandlers CreateCustomDataAdapterType;
            #endregion
        }
        #endregion

        #region Output
        public static class Output
        {
            public static byte[] ParseBuildObjects(StiDatabaseInformation info)
            {
                var hashTypes = new List<Type>();

                byte[] buffer = null;
                using (var stream = new MemoryStream())
                using (var writer = new StiBinaryWriter(stream))
                {
                    #region Tables
                    writer.Write(info.Tables.Count);
                    if (info.Tables.Count > 0)
                    {
                        foreach (DataTable table in info.Tables)
                        {
                            writer.WriteNullableString(table.TableName);
                            writer.Write(table.Columns.Count);

                            foreach (DataColumn column in table.Columns)
                            {
                                if (!hashTypes.Contains(column.DataType))
                                {
                                    hashTypes.Add(column.DataType);
                                }
                                int ident = hashTypes.IndexOf(column.DataType);

                                writer.WriteNullableString(column.ColumnName);
                                writer.Write(ident);
                            }
                        }
                    }
                    #endregion

                    #region Views
                    writer.Write(info.Views.Count);
                    if (info.Views.Count > 0)
                    {
                        foreach (DataTable table in info.Views)
                        {
                            writer.WriteNullableString(table.TableName);
                            writer.Write(table.Columns.Count);

                            foreach (DataColumn column in table.Columns)
                            {
                                if (!hashTypes.Contains(column.DataType))
                                {
                                    hashTypes.Add(column.DataType);
                                }
                                int ident = hashTypes.IndexOf(column.DataType);

                                writer.WriteNullableString(column.ColumnName);
                                writer.Write(ident);
                            }
                        }
                    }
                    #endregion

                    #region StoredProcedures
                    var storedProcedures = info.StoredProcedures.Where(x => x.TableName.IndexOfAny(new char[] { '~', '(', ')' }) == -1).ToList();
                    writer.Write(storedProcedures.Count);
                    if (storedProcedures.Count > 0)
                    {
                        foreach (DataTable table in storedProcedures)
                        {
                            writer.WriteNullableString(table.TableName);
                            writer.Write(table.Columns.Count);

                            foreach (DataColumn column in table.Columns)
                            {
                                if (!hashTypes.Contains(column.DataType))
                                {
                                    hashTypes.Add(column.DataType);
                                }
                                int ident = hashTypes.IndexOf(column.DataType);

                                writer.WriteNullableString(column.ColumnName);
                                writer.Write(ident);
                            }
                        }
                    }
                    #endregion

                    writer.Flush();
                    buffer = stream.ToArray();
                }

                using (var stream = new MemoryStream())
                using (var writer = new StiBinaryWriter(stream))
                {
                    writer.Write(hashTypes.Count);
                    foreach (var type in hashTypes)
                    {
                        writer.WriteNullableString(type.ToString());
                    }

                    writer.WriteByteArray(buffer);

                    writer.Flush();
                    return stream.ToArray();
                }
            }

            public static byte[] ParseRetrieveColumns(StiDataColumnsCollection columns)
            {
                if (columns == null || columns.Count == 0) return null;

                using (var stream = new MemoryStream())
                using (var writer = new StiBinaryWriter(stream))
                {
                    writer.Write(columns.Count);

                    foreach (StiDataColumn column in columns)
                    {
                        writer.WriteNullableString(column.Name);
                        writer.WriteNullableString(column.Type.ToString());
                    }

                    writer.Flush();
                    return stream.ToArray();
                }
            }
        }
        #endregion
    }
}