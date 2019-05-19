using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MovieTimes.MovieDetailsService.ConsoleApp
{
	public static class Program
	{
		public static Task Main()
		{
			Console.WriteLine("Hello World!");

			var hostBuilder = new HostBuilder();

			hostBuilder
				.ConfigureServices((hostBuilderContext, services) =>
				{
					services
						.AddHostedService<HostedService>();
				});

			return hostBuilder.RunConsoleAsync();
		}
	}

	public class HostedService : IHostedService
	{
		public Task StartAsync(CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}

		public Task StopAsync(CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}
	}
}
