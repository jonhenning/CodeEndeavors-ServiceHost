using CodeEndeavors.Extensions;
using CodeEndeavors.ServiceHost.Common.Services;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web;

namespace CodeEndeavors.ServiceHost
{
    public class SqlMigrations
    {
        //just using what is in the SQLRepository as it was easier to keep in sync instead of using EntityFramework or Mig#

        private static string migrationTableName
        {
            get
            {
                return ConfigurationManager.AppSettings.GetSetting("SqlMigrations.TableName", "__schema_version");
            }
        }

        public static void Migrate(string connection)
        {
            var assembly = Assembly.GetCallingAssembly();
            Logging.Log(Logging.LoggingLevel.Info, "SqlMigrations.Migrate: " + assembly.FullName);
            migrateSchema(assembly, connection, "dbo", false);
        }

        public static void Migrate(Dictionary<string, string> tenantConnections, bool alwaysApplyCurrentVersion)
        {
            var assembly = Assembly.GetCallingAssembly();
            Logging.Log(Logging.LoggingLevel.Info, "SqlMigrations.Migrate: " + assembly.FullName);
            migrateSchema(assembly, tenantConnections, "dbo", alwaysApplyCurrentVersion);
        }

        public static void Migrate(string connection, bool alwaysApplyCurrentVersion)
        {
            var assembly = Assembly.GetCallingAssembly();
            Logging.Log(Logging.LoggingLevel.Info, "SqlMigrations.Migrate: " + assembly.FullName);
            migrateSchema(assembly, connection, "dbo", alwaysApplyCurrentVersion);
        }

        public static void Migrate(string connection, string databaseSchemaForVersionTable)
        {
            var assembly = Assembly.GetCallingAssembly();
            Logging.Log(Logging.LoggingLevel.Info, "SqlMigrations.Migrate: " + assembly.FullName);
            migrateSchema(assembly, connection, databaseSchemaForVersionTable, false);
        }
        public static void Migrate(string connection, string databaseSchemaForVersionTable, bool alwaysApplyCurrentVersion)
        {
            var assembly = Assembly.GetCallingAssembly();
            Logging.Log(Logging.LoggingLevel.Info, "SqlMigrations.Migrate: " + assembly.FullName);
            migrateSchema(assembly, connection, databaseSchemaForVersionTable, alwaysApplyCurrentVersion);
        }

        public static void Migrate(Dictionary<string, string> tenantConnections, string databaseSchemaForVersionTable, bool alwaysApplyCurrentVersion)
        {
            var assembly = Assembly.GetCallingAssembly();
            Logging.Log(Logging.LoggingLevel.Info, "SqlMigrations.Migrate: " + assembly.FullName);
            migrateSchema(assembly, tenantConnections, databaseSchemaForVersionTable, alwaysApplyCurrentVersion);
        }

        private static DataTable getData(string sql, SqlConnection connection)
        {
            return getData(sql, null, connection);
        }
        private static DataTable getData(string sql, Dictionary<string, object> parameters, SqlConnection connection)
        {
            var dt = new DataTable();
            using (var da = new SqlDataAdapter(sql, connection))
            {
                if (parameters != null)
                    parameters.Keys.ToList().ForEach(key => da.SelectCommand.Parameters.AddWithValue(key, parameters[key]));
                da.Fill(dt);
            }
            return dt;
        }

        private static int executeSql(string sql, SqlConnection connection)
        {
            return executeSql(sql, null, connection);
        }

        private static int executeSql(string sql, Dictionary<string, object> parameters, SqlConnection connection)
        {
            if (!string.IsNullOrWhiteSpace(sql))
            {
                using (var cmd = new SqlCommand(sql, connection))
                {
                    if (parameters != null)
                        parameters.Keys.ToList().ForEach(key => cmd.Parameters.AddWithValue(key, parameters[key]));
                    var ret = cmd.ExecuteNonQuery();
                    return ret;
                }
            }
            return -1;
        }

        private static void ensureSchema(Assembly assembly, SqlConnection connection, string databaseSchemaForVersionTable, string tenant)
        {
            Logging.Log(Logging.LoggingLevel.Debug, "ensuring Schema for assembly: " + assembly.FullName);

            var schemaSql = string.Format("IF NOT EXISTS (SELECT schema_name FROM INFORMATION_SCHEMA.SCHEMATA  WHERE schema_name = '{0}' ) BEGIN exec('create schema {0}') END", databaseSchemaForVersionTable);
            executeSql(schemaSql, connection);

            var versionTable = string.Format("if not exists (select 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = '{0}' AND TABLE_NAME = '{1}') CREATE TABLE [{0}].{1} (tenant varchar(20), version int)", databaseSchemaForVersionTable, migrationTableName);
            executeSql(versionTable, connection);

            var tenants = getTenants(assembly);
            //foreach (var tenant in tenants)
            //{
                var versionSeed = string.Format("if not exists(select 1 from [{0}].{1} WHERE tenant = '{2}') INSERT INTO [{0}].{1} (tenant, version) VALUES ('{2}', 0)", databaseSchemaForVersionTable, migrationTableName, tenant);
                executeSql(versionSeed, connection);
            //}
        }

        private static List<string> getTenants(Assembly assembly)
        {
            var names = Resources.GetNames("schema.", assembly);
            var len = "schema.".Length;
            return names.Select(n => n.Substring(len).Split('.')[0]).Distinct().ToList();
        }

        private static List<int> getTenantVersions(string tenant, Assembly assembly)
        {
            var names = Resources.GetNames("schema." + tenant + ".", assembly);
            var len = ("schema." + tenant + ".").Length;
            return names.Select(n => n.Substring(len).Split('.')[0]).Distinct().Select(n => int.Parse(n.Substring(1))).ToList();
        }

        private static List<string> getScripts(string tenant, int version, Assembly assembly)
        {
            var names = Resources.GetNames(string.Format("schema.{0}.v{1}.", tenant, version), assembly);

            //move schema to head of list
            var schemaScriptIndex = names.FindIndex(n => n.EndsWith("schema.sql", StringComparison.InvariantCultureIgnoreCase));
            if (schemaScriptIndex > -1)
            {
                var item = names[schemaScriptIndex];
                names.RemoveAt(schemaScriptIndex);
                names.Insert(0, item);
            }

            return names.SelectMany(n => 
            {
                var text = Resources.GetText(n, assembly);

                return Regex.Split(text, @"\n\s*GO\b", RegexOptions.IgnoreCase).ToList();
            }).ToList();
        }

        private static void migrateSchema(Assembly assembly, string connectionString, string databaseSchemaForVersionTable, bool alwaysApplyCurrentVersion)
        {
            var tenantConnections = getTenants(assembly).ToDictionary(t => t, c => connectionString);
            migrateSchema(assembly, tenantConnections, databaseSchemaForVersionTable, alwaysApplyCurrentVersion);
        }
        private static void migrateSchema(Assembly assembly, Dictionary<string, string> tenantConnections, string databaseSchemaForVersionTable, bool alwaysApplyCurrentVersion)
        {
            foreach (var tenantName in tenantConnections.Keys)
            {
                using (var connection = new SqlConnection(tenantConnections[tenantName]))
                {
                    connection.Open();

                    ensureSchema(assembly, connection, databaseSchemaForVersionTable, tenantName);

                    var tenant = getTenants(assembly).Where(t => t == tenantName).FirstOrDefault();
                    if (tenant != null)
                    {
                        //foreach (var tenant in tenants)
                        //{
                        var currentVersion = getSchemaVersion(tenant, connection, databaseSchemaForVersionTable);
                        var versions = getTenantVersions(tenant, assembly);
                        var maxVersion = versions.Max();

                        Logging.Log(Logging.LoggingLevel.Info, "Tenant {0} - current version:{1}  - max version:{2}", tenant, currentVersion, maxVersion);

                        foreach (var version in versions.OrderBy(v => v))
                        {
                            if (version > currentVersion || (version == currentVersion && alwaysApplyCurrentVersion))
                            {
                                var scripts = getScripts(tenant, version, assembly);
                                scripts.ForEach(s => executeSql(s, connection));

                                Logging.Log(Logging.LoggingLevel.Info, "Updating schema {0} version to {1}, executing {2} scripts", tenant, version, scripts.Count);
                                updateSchemaVersion(tenant, version, connection, databaseSchemaForVersionTable);
                            }
                        }
                    }
                    //}
                }
            }
        }

        private static int getSchemaVersion(string tenant, SqlConnection connection, string databaseSchemaForVersionTable)
        {
            var dt = getData(string.Format("SELECT version from [{0}].{1} WHERE tenant = '{2}'", databaseSchemaForVersionTable, migrationTableName, tenant), connection);
            if (dt.Rows.Count > 0)
                return int.Parse(dt.Rows[0]["version"].ToString());
            return 0;
        }
        private static void updateSchemaVersion(string tenant, int version, SqlConnection connection, string databaseSchemaForVersionTable)
        {
            Logging.Log(Logging.LoggingLevel.Debug, "updateSchemaVersion({0}, {1})", tenant, version);
            executeSql(string.Format("UPDATE [{0}].{1} SET Version = @Version WHERE tenant = @tenant", databaseSchemaForVersionTable, migrationTableName), new Dictionary<string, object>() { { "@Version", version }, { "@tenant", tenant } }, connection);
        }

    }
}