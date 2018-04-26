namespace AspNetServerPublish
{
	using System;
	using System.IO;
	using System.Security.Cryptography;

	internal class AppPackage
    {
		public byte[] PackageBytes { get; private set; }

		public string PackageSha256 { get; private set; }

		internal AppPackage(string packagePath)
		{
			PackageBytes = File.ReadAllBytes(packagePath);

			using (var alg = SHA256.Create())
			{
				PackageSha256 = Convert.ToBase64String(alg.ComputeHash(PackageBytes));
			}
		}
    }
}
