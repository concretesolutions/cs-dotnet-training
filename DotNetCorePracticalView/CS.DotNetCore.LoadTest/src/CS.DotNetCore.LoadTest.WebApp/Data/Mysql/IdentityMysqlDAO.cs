namespace CS.DotNetCore.LoadTest.WebApp.Data.Mysql
{
    using System.Threading.Tasks;
    using Business;
    using MySql.Data.MySqlClient;
    using Config;
    using System.Data;
    using Newtonsoft.Json;
    using Schema;
    using Util.Data;

    internal class IdentityMysqlDAO : IIdentityAsyncDAO
    {
        private static readonly string DeleteAllSqlCommand = string.Format
            ("delete from `{0}`;", IdentitySchema.Table);

        private static readonly string InsertSqlCommand = string.Format
            ("insert into `{0}` (`{1}`,`{2}`) values (@{1}, @{2});",
            IdentitySchema.Table,
            IdentitySchema.IdentityNameColumn,
            IdentitySchema.DataColumn);

        private IDatabaseConfig _config;

        public IdentityMysqlDAO(IDatabaseConfig config)
        {
            _config = config;
        }

        private static MySqlParameter[] CreateInsertParameters(Identity identity)
        {
            return new MySqlParameter[2]
            {
                new MySqlParameter(IdentitySchema.IdentityNameColumn, MySqlDbType.VarChar)
                {
                    Direction = ParameterDirection.Input,
                    Value = identity.IdentityName,
                    ParameterName = IdentitySchema.IdentityNameColumn
                },

                new MySqlParameter(IdentitySchema.DataColumn, MySqlDbType.JSON)
                {
                    Direction = ParameterDirection.Input,
                    IsNullable = false,
                    Value = JsonConvert.SerializeObject(identity),
                }
            };
        }

        public async Task<int> DeleteAllAsync()
        {
            using (var conn = new MySqlConnection(_config.String))
            {
                await conn.OpenAsync().ConfigureAwait(false);
                return await DataHelper.ExecuteNonQueryAsync(conn, DeleteAllSqlCommand).ConfigureAwait(false);
            }
        }

        public async Task InsertAsync(Identity identity)
        {
            using (var conn = new MySqlConnection(_config.String))
            {
                await conn.OpenAndRetryAsync(3, 300).ConfigureAwait(false);
                await DataHelper.ExecuteNonQueryAsync(conn, InsertSqlCommand, CreateInsertParameters(identity)).ConfigureAwait(false);
            }
        }
    }
}
