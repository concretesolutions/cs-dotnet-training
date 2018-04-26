namespace AWSDeveloperTraining.Practices.Config
{
	using Microsoft.Extensions.Configuration;
	using System;
	using System.Collections.Generic;
	using System.Security.Cryptography;
	using System.Text;

	public abstract class BaseConfigSection
    {
		private class SecretSetting
		{
			public string SecuredValue { get; set; }

			public string Value { get; set; }
		}

		private readonly Dictionary<string, SecretSetting> _secretsContainer = new Dictionary<string, SecretSetting>();

		protected IConfigurationRoot ConfigRoot { get; private set; }

		protected BaseConfigSection(IConfigurationRoot configRoot)
		{
			ConfigRoot = configRoot;
		}

		public virtual string GetSecret(string secretPath)
		{
			var encryptedSetting = ConfigRoot[secretPath];
			var secretSetting = (SecretSetting)null;

			if (encryptedSetting == null)
			{
				return null;
			}

			if (_secretsContainer.ContainsKey(secretPath) == false)
			{
				secretSetting = new SecretSetting();
				_secretsContainer.Add(secretPath, secretSetting);
			}

			secretSetting = _secretsContainer[secretPath];

			if (encryptedSetting.Equals(secretSetting.SecuredValue))
			{
				return secretSetting.Value;
			}

			secretSetting.SecuredValue = encryptedSetting;

			var base64IV = Environment.GetEnvironmentVariable("APP_SECRET_IV");
			var base64Key = Environment.GetEnvironmentVariable("APP_SECRET_KEY");

			using (var cypher = new AesManaged())
			{
				cypher.IV = Convert.FromBase64String(base64IV);
				cypher.Key = Convert.FromBase64String(base64Key);

				using (var decryptor = cypher.CreateDecryptor())
				{
					var bytesSecuredValue = Convert.FromBase64String(secretSetting.SecuredValue);
					var bytesCleanValue = decryptor.TransformFinalBlock(bytesSecuredValue, 0, bytesSecuredValue.Length);

					secretSetting.Value = Encoding.UTF8.GetString(bytesCleanValue);
					return secretSetting.Value;
				}
			}
		}

		public static string SecureSecret(string cleanSecret)
		{
			var base64IV = Environment.GetEnvironmentVariable("APP_SECRET_IV");
			var base64Key = Environment.GetEnvironmentVariable("APP_SECRET_KEY");

			using (var cypher = new AesManaged())
			{
				cypher.IV = Convert.FromBase64String(base64IV);
				cypher.Key = Convert.FromBase64String(base64Key);

				using (var encryptor = cypher.CreateEncryptor())
				{
					var bytesCleanValue = Encoding.UTF8.GetBytes(cleanSecret);
					var bytesSecuredValue = encryptor.TransformFinalBlock(bytesCleanValue, 0, bytesCleanValue.Length);

					return Convert.ToBase64String(bytesSecuredValue);
				}
			}
		}
	}
}
