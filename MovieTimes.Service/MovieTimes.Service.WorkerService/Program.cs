using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace MovieTimes.Service.WorkerService
{
	public static class Program
	{
		private const string _userSecretsIdConfigKey = "UserSecrets:Id";

		public static Task Main(string[] args)
		{
			var hostBuilder = Host.CreateDefaultBuilder(args);

			hostBuilder
				.ConfigureAppConfiguration((context, builder) =>
				{
					var isDevelopment = context.HostingEnvironment.IsDevelopment();

					if (isDevelopment)
					{
						var config = builder.Build();
						var userSecretsId = config.GetValue<string>(_userSecretsIdConfigKey);

						builder
							.AddUserSecrets(userSecretsId);
					}

					builder
						.AddDockerSecret("DiscordWebhookId", "Discord:Webhook:Id", optional: isDevelopment)
						.AddDockerSecret("DiscordWebhookToken", "Discord:Webhook:Token", optional: isDevelopment)
						.AddDockerSecret("Password", "DbSettings:Password", optional: isDevelopment);
				});

			hostBuilder
				.ConfigureServices((hostContext, services) =>
				{
					var uriSettings = hostContext.Configuration
						.GetSection(nameof(Configuration.Uris))
						.Get<Configuration.Uris>();

					var jaegerSettings = hostContext.Configuration.GetSection(nameof(Helpers.Jaeger))
						.GetSection(nameof(Helpers.Jaeger.Models.Settings))
						.Get<Helpers.Jaeger.Models.Settings>();

					services
						.AddJaegerTracing(jaegerSettings);

					// http clients
					services
						.AddHttpClient(
							nameof(Helpers.Discord.Concrete.DiscordClient),
							(_, client) =>
							{
								client.BaseAddress = new Uri(uriSettings.DiscordUri!, UriKind.Absolute);
							})
						.ConfigurePrimaryHttpMessageHandler(() =>
						{
							return new HttpClientHandler { AllowAutoRedirect = false, };
						});

					// config
					services
						.Configure<Helpers.MySql.Models.DbSettings>(hostContext.Configuration.GetSection(nameof(Helpers.MySql.Models.DbSettings)))
						.Configure<Helpers.Discord.Models.Webhook>(hostContext.Configuration.GetSection(nameof(Helpers.Discord)).GetSection(nameof(Helpers.Discord.Models.Webhook)));

					// clients
					services
						.AddTransient<Helpers.Cineworld.ICineworldClient, Helpers.Cineworld.Concrete.CineworldClient>()
						.AddTransient<Helpers.Discord.IDiscordClient, Helpers.Discord.Concrete.DiscordClient>();

					// repos
					services
						.AddTransient<Repositories.ICineworldRepository, Repositories.Concrete.CineworldRepository>()
						.AddTransient<Repositories.IQueriesRepository, Repositories.Concrete.QueriesRepository>();

					// services
					services
						.AddTransient<Services.IQueriesService, Services.Concrete.QueriesService>()
						.AddTransient<Services.ISerializationService, Services.Concrete.JsonSerializationService>();

					// steps
					services
						.AddTransient<Steps.GetFilmsStep>()
						.AddTransient<Steps.GetLastTwoQueryResultsCollectionsStep>()
						.AddTransient<Steps.GetLatestLogEntryStep>()
						.AddTransient<Steps.GetHeadersStep>()
						.AddTransient<Steps.GetListingsStep>()
						.AddTransient<Steps.GetQueriesStep>()
						.AddTransient<Steps.RunQueryStep>()
						.AddTransient<Steps.SaveCinemasStep>()
						.AddTransient<Steps.SaveFilmLengthsStep>()
						.AddTransient<Steps.SaveLogEntryStep>()
						.AddTransient<Steps.SaveQueryResultsStep>()
						.AddTransient<Steps.SendMessageToDiscordStep>();

					services
						.AddWorkflow()
						.AddHostedService<Worker>();
				});

			return hostBuilder
				.RunConsoleAsync();
		}
	}
}
