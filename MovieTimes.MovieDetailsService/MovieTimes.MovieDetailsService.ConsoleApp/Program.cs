using Dawn;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace MovieTimes.MovieDetailsService.ConsoleApp
{
	public static class Program
	{
		public static Task Main()
		{
			var hostBuilder = new HostBuilder();

			hostBuilder
				.ConfigureAppConfiguration((hostBuilderContext, configurationBuilder) =>
				{
					if (string.IsNullOrWhiteSpace(hostBuilderContext.HostingEnvironment.ApplicationName))
					{
						hostBuilderContext.HostingEnvironment.ApplicationName = System.Reflection.Assembly.GetAssembly(typeof(Program)).GetName().Name;
					}

					var environmentName = Environment.GetEnvironmentVariable("NETCORE_ENVIRONMENT") ?? Environments.Production;

					hostBuilderContext.HostingEnvironment.EnvironmentName = environmentName;

					configurationBuilder
						.SetBasePath(Environment.CurrentDirectory)
						.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
						.AddJsonFile($"appsettings.{environmentName}.json", optional: true, reloadOnChange: true)
						.AddDockerSecret("MySqlCineworldPassword", optional: hostBuilderContext.HostingEnvironment.IsDevelopment(), reloadOnChange: true)
						.AddEnvironmentVariables();
				});

			hostBuilder
				.ConfigureLogging((hostBuilderContext, loggingBuilder) =>
				{
					loggingBuilder
						.AddConfiguration(hostBuilderContext.Configuration.GetSection("Logging"))
						.AddConsole()
						.AddDebug()
						.AddEventSourceLogger();
				});

			hostBuilder
				.ConfigureServices((hostBuilderContext, services) =>
				{
					services
						.AddHttpClient(
							nameof(Clients.Concrete.TheMovieDbClient),
							(_, client) =>
							{
								var uriSettings = hostBuilderContext.Configuration
									.GetSection(nameof(Configuration.Uris))
									.Get<Configuration.Uris>();

								client.Timeout = TimeSpan.FromSeconds(10);
								client.BaseAddress = new Uri(uriSettings.TheMovieDbApiUri, UriKind.Absolute);
								client.DefaultRequestHeaders.Accept.Clear();
								client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
							})
						.ConfigurePrimaryHttpMessageHandler(() =>
						{
							return new HttpClientHandler { AllowAutoRedirect = false, };
						});

					services
						.AddTransient<Clients.ITheMovieDbClient>(serviceProvider =>
						{
							var settings = hostBuilderContext.Configuration
								.GetSection(nameof(Configuration.TheMovieDbSettings))
								.Get<Configuration.TheMovieDbSettings>();

							Guard.Argument(() => settings).NotNull();
							Guard.Argument(() => settings.ApiKey).NotNull().NotEmpty().NotWhiteSpace();

							var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();

							return new Clients.Concrete.TheMovieDbClient(httpClientFactory, settings.ApiKey);
						});

					services
						.AddTransient<Services.ITheMovieDbService, Services.Concrete.TheMovieDbService>();

					services
						.AddTransient<Repositories.IRepository>(serviceProvider =>
						{
							var dbSettings = hostBuilderContext.Configuration
								.GetSection(nameof(Configuration.DbSettings))
								.Get<Configuration.DbSettings>();

							var connectionString = dbSettings.ToString();

							var logger = serviceProvider.GetRequiredService<ILogger<Helpers.MySql.RepositoryBase>>();

							return new Repositories.Concrete.Repository(connectionString, logger);
						});

					services
						.AddTransient<Steps.GetMoviesMissingMappingStep>()
						.AddTransient<Steps.GetRuntimeStep>()
						.AddTransient<Steps.SaveStep>()
						.AddTransient<Steps.SleepStep>()
						.AddTransient<Steps.TitleSanitizationStep>();

					services
						.AddWorkflow()
						.AddHostedService<HostedService>();
				});

			return hostBuilder.RunConsoleAsync();
		}
	}
}
