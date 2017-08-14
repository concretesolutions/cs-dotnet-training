namespace CS.DotNetCore.LoadTest.WebApp.Config
{
    using Microsoft.Extensions.Configuration;
    using System;

    public interface ILoadTestSecurityConfig
    {
        string PasswordSalt { get; }
        byte[] PasswordSaltIV { get; }
        byte[] PasswordSaltKey { get; }
    }

    internal class LoadTestSecurityConfig : ILoadTestSecurityConfig
    {
        private const string SectionPath = "CS:DotNetCoreLoadTest:Security";
        private const string PasswordSaltIVOption = SectionPath + ":PasswordSaltIV";

        private const string PasswordSaltKeyOption = SectionPath + ":PasswordSaltKey";
        private const string PasswordSaltOption = SectionPath + ":PasswordSalt";

        private IConfigurationRoot _configuration;

        public byte[] PasswordSaltIV
        {
            get
            {
                var base64 = _configuration[PasswordSaltIVOption];
                return Convert.FromBase64String(base64);
            }
        }

        public byte[] PasswordSaltKey
        {
            get
            {
                var base64 = _configuration[PasswordSaltKeyOption];
                return Convert.FromBase64String(base64);
            }
        }

        public string PasswordSalt
        {
            get
            {
                return _configuration[PasswordSaltOption];
            }
        }

        internal LoadTestSecurityConfig(IConfigurationRoot configuration)
        {
            _configuration = configuration;
        }
    }
}
