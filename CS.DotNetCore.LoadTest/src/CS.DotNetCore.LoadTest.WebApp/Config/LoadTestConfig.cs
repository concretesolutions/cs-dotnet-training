namespace CS.DotNetCore.LoadTest.WebApp.Config
{
    using Microsoft.Extensions.Configuration;
    using System.Text;
    using System;
    using Data;

    public interface ILoadTestConfig
    {
        Encoding Encoding { get; }
        LoadTestDBMS DBMS { get; }
               
        ILoadTestSecurityConfig Security { get; }
        IDatabaseConfig PgsqlConnection { get; }

        IDatabaseConfig MysqlConnection { get; }
        IDatabaseConfig MongoConnection { get; }
    }

    internal class LoadTestConfig : ILoadTestConfig
    {
        private const string SectionPath = "CS:DotNetCoreLoadTest";
        
        private const string EncodingOption = SectionPath + ":EncodingCodePage";
        private const string DatabaseConfigOption = SectionPath + ":DatabaseConfig";
                
        private const string PgsqlConnStringOption = DatabaseConfigOption + ":Pgsql";
        private const string MysqlConnStringOption = DatabaseConfigOption + ":Mysql";

        private const string MongodbConnStringOption = DatabaseConfigOption + ":Mongodb";
        private const string DbmsOption = SectionPath + ":DBMS";

        private static IConfigurationRoot _configuration;
        private static readonly Type _dbmsTypeCache = typeof(LoadTestDBMS);

        private string _lastDBMS;
        private LoadTestDBMS _dbmsCache;

        public ILoadTestSecurityConfig Security { get; private set; }
        public IDatabaseConfig PgsqlConnection { get; private set; }

        public IDatabaseConfig MysqlConnection { get; private set; }
        public IDatabaseConfig MongoConnection { get; private set; }

        public Encoding Encoding
        {
            get
            {
                return Encoding.GetEncoding(int.Parse(_configuration[EncodingOption]));
            }
        }

        public LoadTestDBMS DBMS
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_configuration[DbmsOption]))
                {
                    throw new Exception("DBMS is not configured.");
                }

                if (_lastDBMS == null || _lastDBMS != _configuration[DbmsOption])
                {
                    _lastDBMS = _configuration[DbmsOption];
                    _dbmsCache = (LoadTestDBMS)Enum.Parse(_dbmsTypeCache, _lastDBMS);
                }

                return _dbmsCache;
            }
        }
               
        public LoadTestConfig()
        {
            Security = new LoadTestSecurityConfig(_configuration);
            PgsqlConnection = new DataBaseConnectionConfig(_configuration, LoadTestDBMS.Pgsql);

            MysqlConnection = new DataBaseConnectionConfig(_configuration, LoadTestDBMS.Mysql);
            MongoConnection = new DataBaseConnectionConfig(_configuration, LoadTestDBMS.Mongodb);
        }

        internal static void UseConfiguration(IConfigurationRoot configuration)
        {
            _configuration = configuration;
        }
    }
}
