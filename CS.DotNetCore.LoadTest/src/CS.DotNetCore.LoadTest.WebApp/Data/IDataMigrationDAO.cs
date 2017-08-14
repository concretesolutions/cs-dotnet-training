namespace CS.DotNetCore.LoadTest.WebApp.Data
{
    using System;

    interface IDataMigrationDAO : IDisposable
    {
        string LatestVersion { get; }

        string GetCurrentVersion();

        void MigrateSchemaVersion(string fromVersion, string toVersion);

        void MigrateIdentity(string fromVersion, string toVersion);

        void MigrateDbmsUsers(string fromVersion, string toVersion);

        void MigrateTestResult(string fromVersion, string toVersion);
    }
}
