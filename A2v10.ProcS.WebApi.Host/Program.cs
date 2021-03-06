// Copyright � 2020 Alex Kukhtin, Artur Moshkola. All rights reserved.

using System;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

using A2v10.ProcS.Infrastructure;
using A2v10.ProcS.WebApi.Host.Classes;
using A2v10.ProcS.SqlServer;
using A2v10.Data;
using A2v10.Data.Interfaces;
using Microsoft.Extensions.Logging.EventLog;

namespace A2v10.ProcS.WebApi.Host
{
	using Host = Microsoft.Extensions.Hosting.Host;

	public static class Program
	{

		public static void Main(String[] args)
		{
			CreateHostBuilder(args).Build().Run();
		}

		private static void ConfigureLogger(ILoggingBuilder builder)
		{
			builder.ClearProviders();
			builder.AddEventLog(c =>
			{
				c.SourceName = "ProcS_WebHost";
			});
			builder.AddConsole();
			builder.SetMinimumLevel(LogLevel.Trace);
		}

		private class DatabaseConfig : IDataConfiguration
		{
			private readonly IConfiguration _config;
			public DatabaseConfig(IConfiguration config)
			{
				_config = config;
			}
			public String ConnectionString(String source)
			{
				if (String.IsNullOrEmpty(source))
					source = "Default";
				return _config.GetConnectionString(source);
			}

			public TimeSpan CommandTimeout => _config.GetValue<TimeSpan>("A2v10:Data:CommandTimeout");
		}

		public static IHostBuilder CreateHostBuilder(String[] args) =>
			Host.CreateDefaultBuilder(args)
				.UseWindowsService()
				.ConfigureLogging(ConfigureLogger)
				.ConfigureServices((ctx, services) =>
				{
					services.AddSingleton(s => s.GetRequiredService<ILoggerFactory>().CreateLogger("ProcS"));
					
					var conf = ctx.Configuration;

					services.AddHostedService<Service>();

					var dbc = new DatabaseConfig(conf);

					services.AddSingleton<IDataProfiler, NullDataProfiler>();
					services.AddSingleton<IDataConfiguration>(dbc);
					services.AddSingleton<IDataLocalizer, NullDataLocalizer>();
					services.AddSingleton<IDbContext, SqlDbContext>();

					services.AddSingleton<INotifyManager, NotifyManager>();

					var tm = new TaskManager();
					services.AddSingleton<ITaskManager>(tm);

					var cat = new FilesystemWorkflowCatalogue(conf["ProcS:Workflows"]);
					services.AddSingleton<IWorkflowCatalogue>(cat);
					var epm = new EndpointManager();

					services.AddSingleton<IEndpointManager>(epm);
					services.AddSingleton<IEndpointResolver>(epm);

					services.AddSingleton<IWorkflowStorage, SqlServerWorkflowStorage>();
					services.AddSingleton<IInstanceStorage, SqlServerInstanceStorage>();

					services.AddSingleton<IScriptEngine, ScriptEngine>();
					services.AddSingleton<IRepository, Repository>();
					services.AddSingleton<ServiceBus>();
					services.AddSingleton<ServiceBusAsync>();

					services.AddSingleton<IWorkflowEngine, WorkflowEngine>();

					services.AddSingleton<ISagaKeeper, SqlServerSagaKeeper>();

					services.AddSingleton<ResourceManager>();
					services.AddSingleton<SagaManager>();
					services.AddSingleton<PluginManager>();
					services.AddSingleton<NotifyManager>();

					services.AddSingleton<IResourceManager>(svc => svc.GetService<ResourceManager>());
					services.AddSingleton<IResourceWrapper>(svc => svc.GetService<ResourceManager>());
					services.AddSingleton<ISagaManager, SagaManager>();
					services.AddSingleton<IPluginManager, PluginManager>();
					services.AddSingleton<IServiceBus, ServiceBusAsync>();

					services.AddSingleton(svc => svc.GetService<ISagaManager>().Resolver);

					services.AddSingleton<Api.ProcessApi>();
				})
				.ConfigureWebHostDefaults(webBuilder =>
				{
					webBuilder.UseStartup<Startup>().UseUrls("http://localhost:55580/");
				});

		

		
	}
}
