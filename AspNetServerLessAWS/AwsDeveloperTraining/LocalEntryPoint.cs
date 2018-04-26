namespace AwsDeveloperTraining
{
	using Microsoft.AspNetCore.Hosting;
	using System.IO;

	public class LocalEntryPoint
	{
		public static void Main(string[] args)
		{
			var host = new WebHostBuilder();

			host.UseHttpSys(opt => { opt.UrlPrefixes.Add("http://localhost:8083"); })
				.UseContentRoot(Directory.GetCurrentDirectory())
				.UseStartup<Startup>()
				.Build()
				.Run();
		}
	}
}
