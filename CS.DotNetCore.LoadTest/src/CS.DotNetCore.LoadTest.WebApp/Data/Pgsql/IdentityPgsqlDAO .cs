namespace CS.DotNetCore.LoadTest.WebApp.Data.Pgsql
{
    using System.Threading.Tasks;
    using Business;
    using Config;
    using Npgsql;
    using Newtonsoft.Json;
    using System.Data;
    using NpgsqlTypes;
    using Schema;
    using Util.Data;

    internal class IdentityPgsqlDAO : IIdentityAsyncDAO
    {
        private static readonly string DeleteSqlCommand = string.Format
            ("delete from \"{0}\";", IdentitySchema.Table);

        private static readonly string InsertSqlCommand = string.Format
            ("insert into \"{0}\" (\"{1}\",\"{2}\") values (:{1}, :{2});",
            IdentitySchema.Table,
            IdentitySchema.IdentityNameColumn,
            IdentitySchema.DataColumn);

        private IDatabaseConfig _config;

        public IdentityPgsqlDAO(IDatabaseConfig config)
        {
            _config = config;
        }

        private static NpgsqlParameter[] CreateInsertParameters(Identity identity)
        {
            return new NpgsqlParameter[2]
            {
                new NpgsqlParameter(IdentitySchema.IdentityNameColumn, NpgsqlDbType.Varchar)
                {
                    Direction = ParameterDirection.Input,
                    NpgsqlValue = identity.IdentityName,
                    ParameterName = IdentitySchema.IdentityNameColumn
                },

                new NpgsqlParameter(IdentitySchema.DataColumn, NpgsqlDbType.Json)
                {
                    Direction = ParameterDirection.Input,
                    IsNullable = false,
                    NpgsqlValue = JsonConvert.SerializeObject(identity),
                }
            };
        }

        public async Task InsertAsync(Identity identity)
        {
            using (var conn = new NpgsqlConnection(_config.String))
            {
                await conn.OpenAsync().ConfigureAwait(false);
                await DataHelper.ExecuteNonQueryAsync(conn, InsertSqlCommand, CreateInsertParameters(identity)).ConfigureAwait(false);
            }
        }

        public async Task<int> DeleteAllAsync()
        {
            using (var conn = new NpgsqlConnection(_config.String))
            {
                await conn.OpenAsync().ConfigureAwait(false);
                return await DataHelper.ExecuteNonQueryAsync(conn, DeleteSqlCommand).ConfigureAwait(false);
            }
        }
    }
}
