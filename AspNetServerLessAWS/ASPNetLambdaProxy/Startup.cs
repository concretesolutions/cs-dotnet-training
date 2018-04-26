namespace ASPNetLambdaProxy
{
	using System;
	using Amazon;
	using Amazon.Runtime;
	using ASPNetLambdaProxy.Config;
	using ASPNetLambdaProxy.Integration;
	using AWSDeveloperTraining.Practices;
	using AWSDeveloperTraining.Practices.Config;
	using Microsoft.AspNetCore.Builder;
	using Microsoft.AspNetCore.Hosting;
	using Microsoft.Extensions.Configuration;
	using Microsoft.Extensions.DependencyInjection;
	using Newtonsoft.Json;
	using Newtonsoft.Json.Serialization;

	public class Startup
	{
		private static readonly JsonSerializerSettings DefaultJsonSerializer;

		internal IConfigurationRoot ConfigurationRoot { get; private set; }

		static Startup()
		{
			DefaultJsonSerializer = new JsonSerializerSettings()
			{
				ContractResolver = new CamelCasePropertyNamesContractResolver(),
				Formatting = Formatting.None,

				CheckAdditionalContent = false,
				DateFormatHandling = DateFormatHandling.IsoDateFormat,

				DateParseHandling = DateParseHandling.DateTimeOffset,
				DateTimeZoneHandling = DateTimeZoneHandling.RoundtripKind,

				NullValueHandling = NullValueHandling.Ignore
			};
		}

		public Startup(IConfiguration configuration)
		{

		}

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			JsonConvert.DefaultSettings = new Func<JsonSerializerSettings>(() => { return DefaultJsonSerializer; });

			//setting di
			services = services
				.AddSingleton<IASPNetLambdaProxyConfigSection>(sp => new ASPNetLambdaProxyConfigSection(ConfigurationRoot))
				.AddSingleton<IAWSConfigSection>(sp => sp.GetService<IASPNetLambdaProxyConfigSection>().AWS)
				
				.AddScoped<IAWSClientFactory, AWSClientFactory>()
				.AddScoped(sp => { return sp.GetService<IAWSClientFactory>().CreateS3Client(); })
				.AddScoped(sp => { return sp.GetService<IAWSClientFactory>().CreateLambdaClient(); });
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IHostingEnvironment env)
		{
			var configBuilder = new ConfigurationBuilder()
				.SetBasePath(env.ContentRootPath)
				.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
				.AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
				.AddJsonFile("Secrets/secrets.json", optional: true, reloadOnChange: true)
				.AddJsonFile($"Secrets/secrets.{env.EnvironmentName}.json", optional: true, reloadOnChange: true);

			ConfigurationRoot = configBuilder.Build();
			app.UseMiddleware<ASPNetLambdaProxyMiddleware>();
		}
	}
}
