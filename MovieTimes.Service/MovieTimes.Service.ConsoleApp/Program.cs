using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace MovieTimes.Service.ConsoleApp
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
						hostBuilderContext.HostingEnvironment.ApplicationName = System.Reflection.Assembly.GetAssembly(typeof(Program))!.GetName().Name;
					}

					var environmentName = Environment.GetEnvironmentVariable("NETCORE_ENVIRONMENT") ?? Environments.Production;

					hostBuilderContext.HostingEnvironment.EnvironmentName = environmentName;

					configurationBuilder
						.SetBasePath(Environment.CurrentDirectory)
						.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
						.AddJsonFile($"appsettings.{environmentName}.json", optional: true, reloadOnChange: true);
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
					// http clients
					services
						.AddHttpClient(
							nameof(Clients.Concrete.ApiClient),
							(_, client) =>
							{
								var uriSettings = hostBuilderContext.Configuration
									.GetSection(nameof(Configuration.Uris))
									.Get<Configuration.Uris>();

								client.Timeout = TimeSpan.FromSeconds(1);
								client.BaseAddress = new Uri(uriSettings.ApiBaseUri!, UriKind.Absolute);
								client.DefaultRequestHeaders.Accept.Clear();
								client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
							})
						.ConfigurePrimaryHttpMessageHandler(() =>
						{
							return new HttpClientHandler { AllowAutoRedirect = false, };
						});

					services
						.AddHttpClient(
							nameof(Clients.Concrete.CineworldClient),
							(_, client) =>
							{
								var uriSettings = hostBuilderContext.Configuration
									.GetSection(nameof(Configuration.Uris))
									.Get<Configuration.Uris>();

								client.Timeout = TimeSpan.FromSeconds(10);
								client.BaseAddress = new Uri(uriSettings.CineworldBaseUri!, UriKind.Absolute);
								client.DefaultRequestHeaders.Accept.Clear();
								client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));
							})
						.ConfigurePrimaryHttpMessageHandler(() =>
						{
							return new HttpClientHandler { AllowAutoRedirect = false, };
						});

					// config
					services
						.Configure<Helpers.MySql.Models.DbSettings>(hostBuilderContext.Configuration.GetSection(nameof(Helpers.MySql.Models.DbSettings)))
						.Configure<Configuration.Uris>(hostBuilderContext.Configuration.GetSection(nameof(Configuration.Uris)));

					// clients
					services
						.AddTransient<Clients.IApiClient, Clients.Concrete.ApiClient>()
						.AddTransient<Clients.ICineworldClient, Clients.Concrete.CineworldClient>();

					// repos
					var dbSettings = hostBuilderContext.Configuration
						.GetSection(nameof(Helpers.MySql.Models.DbSettings))
						.Get<Helpers.MySql.Models.DbSettings>();

					services
						.AddTransient<Repositories.ICineworldRepository, Repositories.Concrete.CineworldRepository>()
						.AddTransient<Repositories.IMovieDetailsRepository, Repositories.Concrete.MovieDetailsRepository>();

					// services
					services
						.AddTransient<Services.IFixTitlesService, Services.Concrete.FixTitlesService>()
						.AddTransient<Repositories.IMovieDetailsRepository, Repositories.Concrete.MovieDetailsRepository>();

					// steps
					services
						.AddTransient<Steps.FixTitlesStep>()
						.AddTransient<Steps.GetListingsHeadersStep>()
						.AddTransient<Steps.GetListingsStep>()
						.AddTransient<Steps.GetLatestLogEntryStep>()
						.AddTransient<Steps.GetTitlesStep>()
						.AddTransient<Steps.SaveCinemasStep>()
						.AddTransient<Steps.SaveTitlesStep>();

					services
						.AddWorkflow()
						.AddHostedService<HostedService>();
				});

			return hostBuilder
				.RunConsoleAsync();
		}
	}
}
