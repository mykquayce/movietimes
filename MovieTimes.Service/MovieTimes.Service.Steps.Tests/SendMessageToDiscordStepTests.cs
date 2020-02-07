using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace MovieTimes.Service.Steps.Tests
{
	public sealed class SendMessageToDiscordStepTests : IDisposable
	{
		private readonly HttpClient _httpClient;
		private readonly Helpers.Discord.IDiscordClient _discordClient;
		private readonly Steps.SendMessageToDiscordStep _step;

		public SendMessageToDiscordStepTests()
		{
			var config = new ConfigurationBuilder()
				.AddUserSecrets("8093bdd7-e455-40d8-a574-90ff004106b6")
				.Build();

			var webhook = config
				.GetSection("Discord")
				.GetSection(nameof(Helpers.Discord.Models.Webhook))
				.Get<Helpers.Discord.Models.Webhook>();

			var webhookOptions = Options.Create(webhook);

			var handler = new HttpClientHandler { AllowAutoRedirect = false, };

			_httpClient = new HttpClient(handler)
			{
				BaseAddress = new Uri("https://ptb.discordapp.com/", UriKind.Absolute),
			};

			var httpClientFactoryMock = new Mock<IHttpClientFactory>();

			httpClientFactoryMock
				.Setup(f => f.CreateClient(It.IsAny<string>()))
				.Returns(_httpClient);

			_discordClient = new Helpers.Discord.Concrete.DiscordClient(
				httpClientFactoryMock.Object,
				webhookOptions);

			_step = new SendMessageToDiscordStep(_discordClient);

		}

		[Theory]
		[InlineData("hello world")]
		public async Task RunAsync(string message)
		{
			_step.Message = message;

			await _step.RunAsync(Mock.Of<WorkflowCore.Interface.IStepExecutionContext>());
		}

		public void Dispose()
		{
			_discordClient?.Dispose();
			_httpClient?.Dispose();
		}
	}
}
