namespace CS.DotNetCore.LoadTest.WebApp.Util.Data
{
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Threading.Tasks;
    using System.Linq;
    using Newtonsoft.Json;

    internal static class DataHelper
    {
        private static DbCommand CreateCommand(DbConnection conn, string sqlCommand, DbParameter[] commandParameters = null)
        {
            var command = conn.CreateCommand();
            command.CommandText = sqlCommand;

            if (commandParameters != null && commandParameters.Length > 0)
            {
                command.Parameters.AddRange(commandParameters);
            }

            return command;
        }

        internal static async Task<int> ExecuteNonQueryAsync(DbConnection conn, string sqlCommand, DbParameter[] commandParameters = null)
        {
            using (var command = CreateCommand(conn, sqlCommand, commandParameters))
            {
                return await command.ExecuteNonQueryAsync().ConfigureAwait(false);
            }
        }

        internal static int ExecuteNonQuery(DbConnection conn, string sqlCommand, DbParameter[] commandParameters = null)
        {
            using (var command = CreateCommand(conn, sqlCommand, commandParameters))
            {
                return command.ExecuteNonQuery();
            }
        }

        internal static async Task<List<T>> ExecuteReaderAsync<T>(DbConnection conn, string sqlCommand, DbParameter[] commandParameters = null) where T : class
        {
            var result = await ExecuteReaderAsync(conn, sqlCommand, commandParameters).ConfigureAwait(false);
            return result.Select(e => JsonConvert.DeserializeObject<T>(e.ToString())).ToList();
        }

        internal static async Task<List<object>> ExecuteReaderAsync(DbConnection conn, string sqlCommand, DbParameter[] commandParameters = null)
        {
            using (var command = CreateCommand(conn, sqlCommand, commandParameters))
            {
                using (var reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                {
                    var result = new List<object>();

                    while (await reader.ReadAsync().ConfigureAwait(false))
                    {
                        result.Add(reader[0]);
                    }

                    return result;
                }
            }
        }

        internal static List<T> ExecuteReader<T>(DbConnection conn, string sqlCommand, DbParameter[] commandParameters = null) where T : class
        {
            var result = ExecuteReader(conn, sqlCommand, commandParameters);
            return result.Select(e => JsonConvert.DeserializeObject<T>(e.ToString())).ToList();
        }

        internal static List<object> ExecuteReader(DbConnection conn, string sqlCommand, DbParameter[] commandParameters = null)
        {
            using (var command = CreateCommand(conn, sqlCommand, commandParameters))
            {
                using (var reader = command.ExecuteReader())
                {
                    var result = new List<object>();

                    while (reader.Read())
                    {
                        result.Add(reader[0]);
                    }

                    return result;
                }
            }
        }

        internal static object ExecuteScalar(DbConnection conn, string sqlCommand, DbParameter[] commandParameters = null)
        {
            using (var command = CreateCommand(conn, sqlCommand, commandParameters))
            {
                return command.ExecuteScalar();
            }
        }

        internal static async Task<object> ExecuteScalarAsync(DbConnection conn, string sqlCommand, DbParameter[] commandParameters = null)
        {
            using (var command = CreateCommand(conn, sqlCommand, commandParameters))
            {
                return await command.ExecuteScalarAsync().ConfigureAwait(false);
            }
        }
    }
}
