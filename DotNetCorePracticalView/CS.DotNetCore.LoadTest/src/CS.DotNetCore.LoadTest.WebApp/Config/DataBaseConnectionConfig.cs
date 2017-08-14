namespace CS.DotNetCore.LoadTest.WebApp.Config
{
    using Data;
    using Microsoft.Extensions.Configuration;

    public interface IDatabaseConfig
    {
        LoadTestDBMS DBMS { get; }
        string User { get; }

        string Password { get; }
        string DataBase { get; }

        string String { get; }
        string AdminString { get; }

        string Collate { get; }
        string Ctype { get; }
    }

    internal class DataBaseConnectionConfig : IDatabaseConfig
    {
        private const string SectionPath = "CS:DotNetCoreLoadTest:DatabaseConfig";
        private string _adminStringPath;

        private string _dataBasePath;
        private string _passwordPath;

        private string _stringPath;
        private string _userPath;

        private string _collatePath;
        private string _ctypePath;

        private IConfigurationRoot _configuration;

        public LoadTestDBMS DBMS { get; private set; }

        public string AdminString
        {
            get
            {
                return _configuration[_adminStringPath];
            }
        }

        public string DataBase
        {
            get
            {
                return _configuration[_dataBasePath];
            }
        }

        public string Password
        {
            get
            {
                return _configuration[_passwordPath];
            }
        }

        public string String
        {
            get
            {
                return _configuration[_stringPath];
            }
        }

        public string User
        {
            get
            {
                return _configuration[_userPath];
            }
        }

        public string Collate
        {
            get
            {
                return _configuration[_collatePath];
            }
        }

        public string Ctype
        {
            get
            {
                return _configuration[_ctypePath];
            }
        }

        internal DataBaseConnectionConfig(IConfigurationRoot configuration, LoadTestDBMS dbms)
        {
            DBMS = dbms;
            var dbConfigPath = string.Format("{0}:{1}", SectionPath, dbms.ToString());

            _adminStringPath = dbConfigPath + ":AdminString";
            _dataBasePath = dbConfigPath + ":DataBase";

            _passwordPath = dbConfigPath + ":Password";
            _stringPath = dbConfigPath + ":String";

            _userPath = dbConfigPath + ":User";
            _collatePath = dbConfigPath + ":Collate";

            _ctypePath = dbConfigPath + ":Ctype";
            _configuration = configuration;
        }
    }
}
