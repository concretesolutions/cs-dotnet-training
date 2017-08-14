namespace CS.DotNetCore.LoadTest.WebApp.Data.Pgsql
{
    using Config;
    using Npgsql;
    using NpgsqlTypes;
    using Schema;
    using System.Data;
    using System.Text;
    using Util.Data;
    using System;

    internal class DataMigrationPgsqlDAO : BaseDataMigrationDAO, IDataMigrationDAO
    {
        private IDatabaseConfig _config;
        private NpgsqlConnection _connection;

        public override string LatestVersion
        {
            get
            {
                return "1.0.0";
            }
        }

        internal DataMigrationPgsqlDAO(IDatabaseConfig config)
        {
            _config = config;
            _connection = new NpgsqlConnection(config.AdminString);
            OpenConnection(_connection.Open);

            CreateDatabase();
            ChangeDatabase(config.DataBase);

            CreateSchemaVersion();
        }

        private void ChangeDatabase(string database = "postgres")
        {
            if (_connection.Database != database)
            {
                _connection.Close();
                _connection.Dispose();

                var connString = new NpgsqlConnectionStringBuilder(_config.AdminString);
                connString.Database = database;

                _connection = new NpgsqlConnection(connString.ConnectionString);
                _connection.Open();
            }
        }

        private void CreateDatabase()
        {
            var sql = new StringBuilder();
            sql.Append("select 1 from pg_database where datname = :dbname");

            var parameters = new NpgsqlParameter[1]
            {
                new NpgsqlParameter("dbName", NpgsqlDbType.Varchar)
                {
                    Direction = ParameterDirection.Input,
                    IsNullable = false,
                    NpgsqlValue = _config.DataBase,
                }
            };

            var dbExists = DataHelper.ExecuteScalar(_connection, sql.ToString(), parameters);

            //create database if not exists
            if (dbExists == null)
            {
                sql.Clear();

                sql.AppendFormat("create database \"{0}\"", _config.DataBase);
                sql.Append(" with template='template0' encoding='UTF8' tablespace=pg_default");

                sql.AppendFormat(" lc_collate='{0}' lc_ctype='{1}' connection limit=-1;", _config.Collate, _config.Ctype);
                DataHelper.ExecuteNonQuery(_connection, sql.ToString());
            }
        }

        private void CreateSchemaVersion()
        {
            var sql = new StringBuilder();
            sql.AppendFormat("create table if not exists \"{0}\"", SchemaVersionSchema.Table);

            sql.AppendFormat(" (\"{0}\" char(5) not null);", SchemaVersionSchema.VersionColumn);
            DataHelper.ExecuteNonQuery(_connection, sql.ToString());
        }

        public string GetCurrentVersion()
        {
            //resolve current version
            var sql = string.Format("select \"{0}\" from \"{1}\";",
                SchemaVersionSchema.VersionColumn, SchemaVersionSchema.Table);

            var version = DataHelper.ExecuteScalar(_connection, sql.ToString());
            return version == null ? null : version.ToString();
        }

        public void MigrateDbmsUsers(string fromVersion, string toVersion)
        {
            if (!ValidateMigrationOperation("Migrate DBMS Users", fromVersion, toVersion))
            {
                return;
            }

            var sql = new StringBuilder();

            //create user
            sql.AppendFormat("create user {0} with encrypted password '{1}';",
                _config.User, _config.Password);

            //grant access to connect on database
            sql.AppendFormat(" grant connect, temporary on database {0} TO {1};",
                _config.DataBase, _config.User);

            //grant access to tables
            sql.AppendFormat(" grant select, insert, update, delete, truncate, references, trigger on all tables in schema public to {0};",
                _config.User);

            DataHelper.ExecuteNonQuery(_connection, sql.ToString());
        }
        
        public void MigrateIdentity(string fromVersion, string toVersion)
        {
            if (!ValidateMigrationOperation("Migrate Identity", fromVersion, toVersion))
            {
                return;
            }

            var sql = new StringBuilder();

            sql.AppendFormat("create table \"{0}\"(\"{1}\" varchar(100) not null constraint {2} primary key,",
                IdentitySchema.Table, IdentitySchema.IdentityNameColumn, IdentitySchema.IdentityNameIndex);

            sql.AppendFormat(" \"{0}\" json not null);", IdentitySchema.DataColumn);
            DataHelper.ExecuteNonQuery(_connection, sql.ToString());
        }

        public void MigrateSchemaVersion(string fromVersion, string toVersion)
        {
            if (!ValidateMigrationOperation("Migrate Schema Version", fromVersion, toVersion))
            {
                return;
            }

            var parameter = new NpgsqlParameter[1]
            {
                new NpgsqlParameter(SchemaVersionSchema.VersionColumn, NpgsqlDbType.Char)
                {
                    Direction = ParameterDirection.Input,
                    IsNullable = false,
                    NpgsqlValue = toVersion
                }
            };

            var sql = string.Format("update \"{0}\" set \"{1}\"=:{1};", SchemaVersionSchema.Table, SchemaVersionSchema.VersionColumn);
            var affected = DataHelper.ExecuteNonQuery(_connection, sql, parameter);

            if (affected == 0)
            {
                sql = string.Format("insert into \"{0}\" (\"{1}\") values (:{1});",
                    SchemaVersionSchema.Table, SchemaVersionSchema.VersionColumn);

                parameter = new NpgsqlParameter[1] { parameter[0].Clone() };
                DataHelper.ExecuteNonQuery(_connection, sql, parameter);
            }
        }

        public void MigrateTestResult(string fromVersion, string toVersion)
        {
            if (!ValidateMigrationOperation("Migrate Test Result", fromVersion, toVersion))
            {
                return;
            }

            var sql = new StringBuilder();
            sql.AppendFormat("create table \"{0}\"", TestResultSchema.Table);

            sql.AppendFormat(" (\"{0}\" varchar(10) not null,", TestResultSchema.TestIdColumn);
            sql.AppendFormat(" \"{0}\" varchar(50) null,", TestResultSchema.LanguageColumn);
            sql.AppendFormat(" \"{0}\" json not null,", TestResultSchema.DataColumn);

            sql.AppendFormat(" constraint \"{0}\" unique (\"{1}\",\"{2}\"));",
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
