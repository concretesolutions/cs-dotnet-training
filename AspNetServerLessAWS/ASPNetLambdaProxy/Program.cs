using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ASPNetLambdaProxy
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var host = new WebHostBuilder();

			host.UseKestrel()
				.UseContentRoot(Directory.GetCurrentDirectory())
				.UseStartup<Startup>()
				.UseUrls("http://*:8086")
				.Build()
				.Run();
		}
	}
}
