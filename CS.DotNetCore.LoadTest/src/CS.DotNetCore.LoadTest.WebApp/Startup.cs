namespace CS.DotNetCore.LoadTest.WebApp.WebApi
{
    using Config;
    using Data;
    using Data.Memory;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.HttpOverrides;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Middlewares;
    using MongoDB.Bson.Serialization.Conventions;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;
    using System;

    public class Startup
    {
        private static readonly JsonSerializerSettings DefaultJsonSerializer;

        static Startup()
        {
            DefaultJsonSerializer = new JsonSerializerSettings()
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                Formatting = Formatting.None,
                CheckAdditionalContent = false,
                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                DateParseHandling = DateParseHandling.DateTimeOffset,
                DateTimeZoneHandling = DateTimeZoneHandling.RoundtripKind
            };

            var camelCaseConvention = new ConventionPack { new CamelCaseElementNameConvention() };
            ConventionRegistry.Register("CamelCase", camelCaseConvention, type => true);
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            //serialization settings for controllers boundary
            var mvcBuilder = services.AddMvcCore()
                                     .AddJsonFormatters()
                                     .AddRazorViewEngine()
                                     .AddViews();

            mvcBuilder.AddJsonOptions((options) =>
            {
                options.SerializerSettings.ContractResolver = DefaultJsonSerializer.ContractResolver;
                options.SerializerSettings.Formatting = DefaultJsonSerializer.Formatting;
            });

            //setting DI
            services = services.AddSingleton<ILoadTestConfig, LoadTestConfig>()
                               .AddSingleton<IIdentityDAO, IdentityMemoryDAO>()
                               .AddSingleton<IEventResultDAO, EventResultMemoryDAO>()
                               .AddSingleton<IIdentityAsyncDAO, IdentityAsyncDAOWrapper>()
                               .AddSingleton<ITestResultAsyncDAO, TestResultAsyncDAOWrapper>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            //setting log
            loggerFactory.AddConsole(LogLevel.Error, true);

            //setting configuration
            var configBuilder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
                .AddJsonFile("Secrets/secrets.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"Secrets/secrets.{env.EnvironmentName}.json", optional: true, reloadOnChange: true);

            LoadTestConfig.UseConfiguration(configBuilder.Build());
            var config = new LoadTestConfig();

            //building pipeline
            var appBuilder = app.UseMiddleware<EventResultMiddleware>();

            //settign Forwarded Headers
            appBuilder = appBuilder.UseForwardedHeaders(new ForwardedHeadersOptions()
            {
                ForwardedHeaders = ForwardedHeaders.All
            });

            //setting default route for controllers
            appBuilder.UseMvcWithDefaultRoute();

            //serialization settings
            JsonConvert.DefaultSettings = new Func<JsonSerializerSettings>(() =>
            {
                return DefaultJsonSerializer;
            });

            //executing migration
            var migration = new DataMigration(config);
            migration.Execute();
        }
    }
}
