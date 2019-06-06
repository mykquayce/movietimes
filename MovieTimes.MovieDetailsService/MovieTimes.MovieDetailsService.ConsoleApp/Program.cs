using Dawn;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
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

					var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? Environments.Production;

					hostBuilderContext.HostingEnvironment.EnvironmentName = environmentName;

					configurationBuilder
						.SetBasePath(Environment.CurrentDirectory)
						.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
						.AddJsonFile($"appsettings.{environmentName}.json", optional: true, reloadOnChange: true)
						.AddDockerSecret("MySqlCineworldUser", optional: hostBuilderContext.HostingEnvironment.IsDevelopment(), reloadOnChange: true)
						.AddDockerSecret("MySqlCineworldPassword", optional: hostBuilderContext.HostingEnvironment.IsDevelopment(), reloadOnChange: true);
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
					var dbSettings = hostBuilderContext.Configuration
						.GetSection(nameof(Configuration.DbSettings))
						.Get<Configuration.DbSettings>();

					var connectionString = dbSettings.ToString();

					services
						.AddTransient<Repositories.IRepository>(serviceProvider =>
						{
							var logger = serviceProvider.GetRequiredService<ILogger<Helpers.MySql.RepositoryBase>>();

							return new Repositories.Concrete.Repository(connectionString, logger);
						});

					services
						.AddHostedService<HostedService>();
				});

			return hostBuilder.RunConsoleAsync();
		}
	}

	public class HostedService : IHostedService
	{
		private readonly Repositories.IRepository _repository;

		public HostedService(
			Repositories.IRepository repository)
		{
			Guard.Argument(() => repository).NotNull();
		}

		public async Task StartAsync(CancellationToken cancellationToken)
		{
			await foreach ((string title, short year) in _repository.GetMoviesMissingDetailsAsync())
			{

			}

			throw new NotImplementedException();
		}

		public Task StopAsync(CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}
	}
}
