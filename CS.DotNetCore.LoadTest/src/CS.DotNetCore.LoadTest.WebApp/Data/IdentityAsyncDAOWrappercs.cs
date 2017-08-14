namespace CS.DotNetCore.LoadTest.WebApp.Data
{
    using System;
    using System.Threading.Tasks;
    using Config;
    using Business;
    using Mysql;
    using Mongodb;
    using Pgsql;

    internal class IdentityAsyncDAOWrapper : IIdentityAsyncDAO
    {
        private IIdentityAsyncDAO _dao;

        public IdentityAsyncDAOWrapper(ILoadTestConfig config)
        {
            switch (config.DBMS)
            {
                case LoadTestDBMS.Pgsql:
                    _dao = new IdentityPgsqlDAO(config.PgsqlConnection);
                    break;

                case LoadTestDBMS.Mysql:
                    _dao = new IdentityMysqlDAO(config.MysqlConnection);
                    break;

                case LoadTestDBMS.Mongodb:
                    _dao = new IdentityMongodbDAO(config.MongoConnection);
                    break;

                default:
                    throw new NotImplementedException(config.DBMS.ToString() + "not implemented");
            }
        }

        public Task<int> DeleteAllAsync()
        {
            return _dao.DeleteAllAsync();
        }

        public Task InsertAsync(Identity identity)
        {
            return _dao.InsertAsync(identity);
        }
    }
}
