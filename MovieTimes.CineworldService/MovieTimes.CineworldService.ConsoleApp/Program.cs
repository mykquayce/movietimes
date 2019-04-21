using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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

					var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? EnvironmentName.Production;

					hostBuilderContext.HostingEnvironment.EnvironmentName = environmentName;

					configurationBuilder
						.SetBasePath(Environment.CurrentDirectory)
						.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
						.AddJsonFile($"appsettings.{environmentName}.json", optional: true, reloadOnChange: true)
						.AddDockerSecret("MySqlCineworldUser", optional: hostBuilderContext.HostingEnvironment.IsDevelopment(), reloadOnChange: true)
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
					services
						.AddHttpClient(
							nameof(Clients.Concrete.CineworldClient),
							(serviceProvider, client) =>
							{
								client.Timeout = TimeSpan.FromSeconds(10);
								var settingsOptions = serviceProvider.GetRequiredService<IOptions<Configuration.Uris>>();
								client.BaseAddress = new Uri(settingsOptions.Value.CineworldBaseUri, UriKind.Absolute);
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
						.AddTransient<Repositories.ICineworldRepository, Repositories.Concrete.CineworldRepository>();

					services
						.AddHostedService<HostedService>();
				});

			return builder.RunConsoleAsync();
		}
	}
}
