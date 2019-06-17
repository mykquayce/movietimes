using Jaeger;
using Jaeger.Reporters;
using Jaeger.Samplers;
using Jaeger.Senders;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTracing;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace MovieTimes.CineworldService.ConsoleApp
{
	public static class Program
	{
		public static Task Main()
		{
			var builder = new HostBuilder();

			builder
				.ConfigureAppConfiguration((hostBuilderContext, configurationBuilder) =>
				{
					if (string.IsNullOrWhiteSpace(hostBuilderContext.HostingEnvironment.ApplicationName))
					{
						hostBuilderContext.HostingEnvironment.ApplicationName = System.Reflection.Assembly.GetAssembly(typeof(Program)).GetName().Name;
					}

					var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? Environments.Production;

					hostBuilderContext.HostingEnvironment.EnvironmentName = environmentName;

					configurationBuilder
						.SetBasePath(Environment.CurrentDirectory)
						.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
						.AddJsonFile($"appsettings.{environmentName}.json", optional: true, reloadOnChange: true)
						.AddDockerSecret("MySqlCineworldPassword", optional: hostBuilderContext.HostingEnvironment.IsDevelopment(), reloadOnChange: true);
				});

			builder
				.ConfigureLogging((hostBuilderContext, loggingBuilder) =>
				{
					loggingBuilder
						.AddConfiguration(hostBuilderContext.Configuration.GetSection("Logging"))
						.AddConsole()
						.AddDebug()
						.AddEventSourceLogger();
				});

			builder
				.ConfigureServices((hostBuilderContext, services) =>
				{
					var jaegerSettings = hostBuilderContext.Configuration
						.GetSection(nameof(Configuration.JaegerSettings))
						.Get<Configuration.JaegerSettings>();

					services
						.AddJaegerTracing(hostBuilderContext.HostingEnvironment.ApplicationName, jaegerSettings.Host, jaegerSettings.Port);

					services
						.AddHttpClient(
							nameof(Clients.Concrete.CineworldClient),
							(_, client) =>
							{
								var uriSettings = hostBuilderContext.Configuration
									.GetSection(nameof(Configuration.Uris))
									.Get<Configuration.Uris>();

								client.Timeout = TimeSpan.FromSeconds(10);
								client.BaseAddress = new Uri(uriSettings.CineworldBaseUri, UriKind.Absolute);
								client.DefaultRequestHeaders.Accept.Clear();
								client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));
							})
						.ConfigurePrimaryHttpMessageHandler(() =>
						{
							return new HttpClientHandler { AllowAutoRedirect = false, };
						});

					services
						.Configure<Configuration.Settings>(hostBuilderContext.Configuration)
						.Configure<Configuration.DbSettings>(hostBuilderContext.Configuration.GetSection(nameof(Configuration.DbSettings)))
						.Configure<Configuration.DockerSecrets>(hostBuilderContext.Configuration.GetSection(nameof(Configuration.DockerSecrets)))
						.Configure<Configuration.Uris>(hostBuilderContext.Configuration.GetSection(nameof(Configuration.Uris)));

					services
						.AddTransient<Clients.ICineworldClient, Clients.Concrete.CineworldClient>()
						.AddTransient<Services.ICineworldService, Services.Concrete.CineworldService>()
						.AddTransient<Repositories.ICineworldRepository>(serviceProvider =>
						{
							var dockerSecrets = hostBuilderContext.Configuration
								.GetSection(nameof(Configuration.DockerSecrets))
								.Get<Configuration.DockerSecrets>();

							var dbSettings = hostBuilderContext.Configuration
								.GetSection(nameof(Configuration.DbSettings))
								.Get<Configuration.DbSettings>();

							var builder = new MySql.Data.MySqlClient.MySqlConnectionStringBuilder
							{
								Server = dbSettings.Server,
								Port = (uint)dbSettings.Port,
								UserID = dbSettings.UserId,
								Password = dockerSecrets?.MySqlCineworldPassword ?? dbSettings.Password,
								Database = dbSettings.Database,
							};

							var logger = serviceProvider.GetRequiredService<ILogger<Repositories.Concrete.CineworldRepository>>();
							var tracer = serviceProvider.GetRequiredService<ITracer>();

							return new Repositories.Concrete.CineworldRepository(builder.ConnectionString, logger, tracer);
						});

					services
						.AddTransient<Steps.CopyValueStep>()
						.AddTransient<Steps.DoNotingStep>()
						.AddTransient<Steps.EchoStep>()
						.AddTransient<Steps.GetCinemasStep>()
						.AddTransient<Steps.GetListingsLastModifiedStep>()
						.AddTransient<Steps.SaveCinemasStep>()
						.AddTransient<Steps.SleepStep>()
						.AddTransient<Steps.TestDatabaseConnectivityStep>()
						.AddTransient<Workflows.StartJaegerTrace>()
						.AddTransient<Workflows.StopJaegerTrace>();

					services
						.AddWorkflow()
						.AddHostedService<HostedService>();
				});

			return builder.RunConsoleAsync();
		}
	}
}
