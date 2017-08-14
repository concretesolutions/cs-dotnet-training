

namespace CS.DotNetCore.LoadTest.WebApp.Data
{
    using Config;
    using Mongodb;
    using Mysql;
    using Pgsql;
    using System;

    internal class DataMigration : IDisposable
    {
        private IDataMigrationDAO _dataMigrationDAO;
        private bool _disposeDAO;

        internal DataMigration(IDataMigrationDAO migrationDAO)
        {
            _dataMigrationDAO = migrationDAO;
            _disposeDAO = false;
        }

        internal DataMigration(ILoadTestConfig config)
        {
            _disposeDAO = true;

            switch (config.DBMS)
            {
                case LoadTestDBMS.Pgsql:
                    _dataMigrationDAO = new DataMigrationPgsqlDAO(config.PgsqlConnection);
                    break;

                case LoadTestDBMS.Mysql:
                    _dataMigrationDAO = new DataMigrationMysqlDAO(config.MysqlConnection);
                    break;

                case LoadTestDBMS.Mongodb:
                    _dataMigrationDAO = new DataMigrationMongodbDAO(config.MongoConnection);
                    break;

                default:
                    throw new NotImplementedException("DBMS not implemented");
            }
        }

        public void Execute()
        {
            var currentVersion = _dataMigrationDAO.GetCurrentVersion();

            if (currentVersion == _dataMigrationDAO.LatestVersion)
            {
                Console.WriteLine("Schema Updated");
                return;
            }

            //installing latestVersion
            _dataMigrationDAO.MigrateSchemaVersion(currentVersion, _dataMigrationDAO.LatestVersion);
            Console.WriteLine("Migrate Schema Version - OK");

            _dataMigrationDAO.MigrateIdentity(currentVersion, _dataMigrationDAO.LatestVersion);
            Console.WriteLine("Migrate Identity - OK");

            _dataMigrationDAO.MigrateTestResult(currentVersion, _dataMigrationDAO.LatestVersion);
            Console.WriteLine("Migrate Test Results - OK");

            _dataMigrationDAO.MigrateDbmsUsers(currentVersion, _dataMigrationDAO.LatestVersion);
            Console.WriteLine("Migrate DBMS Users - OK");
        }

        public void Dispose()
        {
            if (_disposeDAO)
            {
                _dataMigrationDAO.Dispose();
            }

            _dataMigrationDAO = null;
        }
    }
}
