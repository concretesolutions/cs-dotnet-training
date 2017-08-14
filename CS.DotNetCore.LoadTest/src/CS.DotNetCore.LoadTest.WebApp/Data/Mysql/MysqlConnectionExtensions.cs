

namespace CS.DotNetCore.LoadTest.WebApp.Data.Mysql
{
    using MySql.Data.MySqlClient;
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    internal static class MysqlConnectionExtensions
    {
        internal static async Task OpenAndRetryAsync(this MySqlConnection connection, int retry = 3, int retryIntervalMs = 100)
        {
            var errList = new List<Exception>();

            for (int i = 0; i < retry; i++)
            {
                try
                {
                    await connection.OpenAsync().ConfigureAwait(false);
                    break;
                }
                catch (Exception e)
                {
                    errList.Add(e);
                    connection.Close();
                    Thread.Sleep(retryIntervalMs);
                }
            }

            if (errList.Count > 0)
            {
                throw new AggregateException(errList);
            }
        }
    }
}
