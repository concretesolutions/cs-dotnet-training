namespace CS.DotNetCore.LoadTest.WebApp.Business
{
    using Config;
    using Newtonsoft.Json;
    using System.Security.Cryptography;

    public class Identity
    {
        [JsonProperty]
        internal byte[] Salt { get; private set; }

        [JsonProperty]
        internal byte[] Password { get; private set; }

        [JsonProperty]
        internal string IdentityName { get; private set; }

        internal Identity(string identityName)
        {
            IdentityName = identityName;
        }

        internal void SetPassword(string password, ILoadTestConfig config)
        {
            var saltBytes = config.Encoding.GetBytes(config.Security.PasswordSalt);

            using (var aes = Aes.Create())
            {
                using (var cipher = aes.CreateEncryptor(config.Security.PasswordSaltKey, config.Security.PasswordSaltIV))
                {
                    Salt = cipher.TransformFinalBlock(saltBytes, 0, saltBytes.Length);
                }
            }

            var saltedPassword = config.Security.PasswordSalt + password;
            var saltedPasswordBytes = config.Encoding.GetBytes(saltedPassword);

            using (var sha = SHA256.Create())
            {
                Password = sha.ComputeHash(saltedPasswordBytes);
            }
        }
    }
}
