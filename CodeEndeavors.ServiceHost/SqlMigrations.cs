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

            migrateSchema(assembly, connection);
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
            using (var cmd = new SqlCommand(sql, connection))
            {
                if (parameters != null)
                    parameters.Keys.ToList().ForEach(key => cmd.Parameters.AddWithValue(key, parameters[key]));
                var ret = cmd.ExecuteNonQuery();
                return ret;
            }
        }

        private static void ensureSchema(Assembly assembly, SqlConnection connection)
        {
            Logging.Log(Logging.LoggingLevel.Debug, "ensuring Schema for assembly: " + assembly.FullName);

            var versionTable = "if not exists (select 1 from sysobjects where name = '" + migrationTableName + "') CREATE TABLE " + migrationTableName + " (tenant varchar(20), version int)";
            executeSql(versionTable, connection);

            var tenants = getTenants(assembly);
            foreach (var tenant in tenants)
            {
                var versionSeed = "if not exists(select 1 from " + migrationTableName + " WHERE tenant = '" + tenant + "') INSERT INTO " + migrationTableName + " (tenant, version) VALUES ('" + tenant + "', 0)";
                executeSql(versionSeed, connection);
            }
        }

        private static List<string> getTenants(Assembly assembly)
        {
            var names = Resources.GetNames("schema.", assembly);
            var len = "schema.".Length;
            return names.Select(n => n.Substring(len).Split('.')[0]).Distinct().ToList();
        }

        private static List<int> getTenantVersions(string tenant, Assembly assembly)
        {
            var names = Resources.GetNames("schema." + tenant, assembly);
            var len = ("schema." + tenant + ".").Length;
            return names.Select(n => n.Substring(len).Split('.')[0]).Distinct().Select(n => int.Parse(n.Substring(1))).ToList();
        }

        private static List<string> getScripts(string tenant, int version, Assembly assembly)
        {
            var names = Resources.GetNames(string.Format("schema.{0}.v{1}", tenant, version), assembly);
            return names.Select(n => Resources.GetText(n, assembly)).ToList();
        }

        private static void migrateSchema(Assembly assembly, string connectionString)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                ensureSchema(assembly, connection);

                var tenants = getTenants(assembly);
                foreach (var tenant in tenants)
                {
                    var currentVersion = getSchemaVersion(tenant, connection);
                    var versions = getTenantVersions(tenant, assembly);
                    var maxVersion = versions.Max();

                    Logging.Log(Logging.LoggingLevel.Info, "Tenant {0} - current version:{1}  - max version:{2}", tenant, currentVersion, maxVersion);

                    foreach (var version in versions)
                    {
                        if (version > currentVersion)
                        {
                            var scripts = getScripts(tenant, version, assembly);
                            scripts.ForEach(s => executeSql(s, connection));

                            Logging.Log(Logging.LoggingLevel.Info, "Updating schema {0} version to {1}, executing {2} scripts", tenant, version, scripts.Count);
                            updateSchemaVersion(tenant, version, connection);
                        }
                    }
                }
            }
        }

        private static int getSchemaVersion(string tenant, SqlConnection connection)
        {
            var dt = getData("SELECT version from " + migrationTableName + " WHERE tenant = '" + tenant + "'", connection);
            if (dt.Rows.Count > 0)
                return int.Parse(dt.Rows[0]["version"].ToString());
            return 0;
        }
        private static void updateSchemaVersion(string tenant, int version, SqlConnection connection)
        {
            Logging.Log(Logging.LoggingLevel.Debug, "updateSchemaVersion({0}, {1})", tenant, version);
            executeSql("UPDATE " + migrationTableName + " SET Version = @Version WHERE tenant = @tenant", new Dictionary<string, object>() { { "@Version", version }, { "@tenant", tenant } }, connection);
        }

    }
}