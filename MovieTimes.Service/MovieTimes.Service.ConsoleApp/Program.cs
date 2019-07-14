using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
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
						hostBuilderContext.HostingEnvironment.ApplicationName = System.Reflection.Assembly.GetAssembly(typeof(Program)).GetName().Name;
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
					services
						.AddWorkflow()
						.AddHostedService<HostedService>();
				});

			return hostBuilder
				.RunConsoleAsync();
		}
	}
}
