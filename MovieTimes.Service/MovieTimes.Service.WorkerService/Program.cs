using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace MovieTimes.Service.WorkerService
{
	public class Program
	{
		public static Task Main(string[] args)
		{
			var hostBuilder = Host.CreateDefaultBuilder(args);

			hostBuilder
				.ConfigureServices((hostContext, services) =>
				{
					// http clients
					services
						.AddHttpClient(
							nameof(Clients.Concrete.ApiClient),
							(_, client) =>
							{
								var uriSettings = hostContext.Configuration
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

					// config
					services
						.Configure<Helpers.MySql.Models.DbSettings>(hostContext.Configuration.GetSection(nameof(Helpers.MySql.Models.DbSettings)))
						.Configure<Configuration.Uris>(hostContext.Configuration.GetSection(nameof(Configuration.Uris)));

					// clients
					services
						.AddTransient<Clients.IApiClient, Clients.Concrete.ApiClient>()
						.AddTransient<Helpers.Cineworld.ICineworldClient, Helpers.Cineworld.Concrete.CineworldClient>();

					// repos
					var dbSettings = hostContext.Configuration
						.GetSection(nameof(Helpers.MySql.Models.DbSettings))
						.Get<Helpers.MySql.Models.DbSettings>();

					services
						.AddTransient<Repositories.ICineworldRepository, Repositories.Concrete.CineworldRepository>()
						.AddTransient<Repositories.IQueriesRepository, Repositories.Concrete.QueriesRepository>();

					// steps
					services
						.AddTransient<Steps.GetLatestLogEntryStep>()
						.AddTransient<Steps.GetListingsHeadersStep>()
						.AddTransient<Steps.GetListingsStep>()
						.AddTransient<Steps.GetQueriesStep>()
						.AddTransient<Steps.RunQueryStep>()
						.AddTransient<Steps.SaveCinemasStep>()
						.AddTransient<Steps.SaveLogEntryStep>();

					services
						.AddWorkflow()
						.AddHostedService<Worker>();
				});

			return hostBuilder
				.RunConsoleAsync();
		}
	}
}
