namespace CS.DotNetCore.LoadTest.WebApp.Data.Mysql
{
    using System;
    using Config;
    using MySql.Data.MySqlClient;
    using Schema;
    using Util.Data;
    using System.Text;

    internal class DataMigrationMysqlDAO : BaseDataMigrationDAO, IDataMigrationDAO
    {
        private IDatabaseConfig _config;
        private MySqlConnection _connection;

        public override string LatestVersion { get { return "1.0.0"; } }
                
        internal DataMigrationMysqlDAO(IDatabaseConfig config)
        {
            _config = config;
            _connection = new MySqlConnection(config.AdminString);
            OpenConnection(_connection.Open);
            CreateDatabase();
            CreateSchemaVersion();
        }

        private void CreateDatabase()
        {
            //create database if not exists
            var sql = string.Format("create database if not exists `{0}` character set='{1}' collate='{2}';",
                _config.DataBase, _config.Ctype, _config.Collate);

            DataHelper.ExecuteNonQuery(_connection, sql);
        }

        private void CreateSchemaVersion()
        {
            //create table SchemaVersion if not exists
            var sql = string.Format("use {0}; create table if not exists `{1}` (`{2}` char(10) not null);",
                _config.DataBase, SchemaVersionSchema.Table, SchemaVersionSchema.VersionColumn);

            DataHelper.ExecuteNonQuery(_connection, sql);
        }

        public string GetCurrentVersion()
        {
            //resolve current version
            var sql = string.Format("select `{0}` from `{1}`;", SchemaVersionSchema.VersionColumn, SchemaVersionSchema.Table);
            var currentVersion = DataHelper.ExecuteScalar(_connection, sql);
            return currentVersion == null ? null : currentVersion.ToString();
        }

        public void MigrateSchemaVersion(string fromVersion, string toVersion)
        {
            if (!ValidateMigrationOperation("Migrate Schema Version", fromVersion, toVersion))
            {
                return;
            }

            var parameters = new MySqlParameter[1]
            {
                new MySqlParameter(SchemaVersionSchema.VersionColumn, MySqlDbType.VarChar)
                {
                    Direction = System.Data.ParameterDirection.Input,
                    IsNullable = false,
                    Value = toVersion
                }
            };

            var sql = string.Format("update `{0}` set `{1}` = @{1};", SchemaVersionSchema.Table, SchemaVersionSchema.VersionColumn);
            var affected = DataHelper.ExecuteNonQuery(_connection, sql, parameters);

            if (affected == 0)
            {
                sql = string.Format("insert into `{0}` (`{1}`) values (@{1});", SchemaVersionSchema.Table, SchemaVersionSchema.VersionColumn);
                DataHelper.ExecuteNonQuery(_connection, sql, parameters);
            }
        }

        public void MigrateIdentity(string fromVersion, string toVersion)
        {
            if (!ValidateMigrationOperation("Migrate Identity", fromVersion, toVersion))
            {
                return;
            }

            var sql = string.Format("create table `{0}` (`{1}` varchar(100) not null, `{2}` json not null); ",
                IdentitySchema.Table, IdentitySchema.IdentityNameColumn, IdentitySchema.DataColumn);

            sql += string.Format("alter table `{0}` add constraint {1} primary key using btree (`{2}` asc);",
                IdentitySchema.Table, IdentitySchema.IdentityNameIndex, IdentitySchema.IdentityNameColumn);

            DataHelper.ExecuteNonQuery(_connection, sql);
        }

        public void MigrateDbmsUsers(string fromVersion, string toVersion)
        {
            if (!ValidateMigrationOperation("Migrate DBMS Users", fromVersion, toVersion))
            {
                return;
            }

            var sql = string.Format("create user '{0}' identified by '{1}' password expire never; ",
                _config.User, _config.Password);

            sql += string.Format("grant create temporary tables, create view, delete, execute, insert, select, update on `{0}`.* to '{1}';",
                _config.DataBase, _config.User);

            DataHelper.ExecuteNonQuery(_connection, sql);
        }

        public void MigrateTestResult(string fromVersion, string toVersion)
        {
            var sql = new StringBuilder();
            sql.AppendFormat("create table `{0}`", TestResultSchema.Table);

            sql.AppendFormat(" (`{0}` varchar(10) not null,", TestResultSchema.TestIdColumn);
            sql.AppendFormat(" `{0}` varchar(50) null,", TestResultSchema.LanguageColumn);
            sql.AppendFormat(" `{0}` json not null,", TestResultSchema.DataColumn);

            sql.AppendFormat(" constraint {0} unique(`{1}`,`{2}`));",
                TestResultSchema.TestResultTestIndex, TestResultSchema.TestIdColumn, TestResultSchema.LanguageColumn);

            DataHelper.ExecuteNonQuery(_connection, sql.ToString());
        }

        public void Dispose()
        {
            _config = null;
            _connection.Close();

            _connection.Dispose();
            _connection = null;
        }
    }
}
