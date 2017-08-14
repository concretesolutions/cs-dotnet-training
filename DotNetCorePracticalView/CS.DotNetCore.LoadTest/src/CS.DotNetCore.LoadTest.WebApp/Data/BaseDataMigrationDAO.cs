namespace CS.DotNetCore.LoadTest.WebApp.Data
{
    using System;
    using System.Data;
    using System.Threading;

    internal abstract class BaseDataMigrationDAO
    {
        private string _migrationErrorTemplate = "{0} from version:\"{1}\" to version:\"{2}\" not implemented.";

        public abstract string LatestVersion { get; }

        internal virtual bool ValidateMigrationOperation(string operation, string fromVersion, string toVersion)
        {
            if (fromVersion != null || toVersion != LatestVersion)
            {
                throw new NotImplementedException(string.Format(_migrationErrorTemplate, operation, fromVersion, toVersion));
            }

            if (fromVersion == toVersion)
            {
                Console.WriteLine(operation + " - Updated");
                return false;
            }

            return true;
        }

        internal void OpenConnection(Action openConnection)
        {
            for (int retry = 0; retry < 3; retry++)
            {
                try
                {
                    openConnection.Invoke();

                    if (retry > 0)
                    {
                        Console.WriteLine("Data Migration connection success");
                    }

                    break;
                }
                catch
                {
                    if (retry == 2)
                    {
                        throw;
                    }

                    Console.WriteLine("Data Migration connection fail, try: " + (retry + 1).ToString() + " starting retry...");
                    Thread.Sleep(10000);
                }
            }
        }
    }
}
