using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using MovieTimes.Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace MovieTimes.Api.WebApplication.Tests
{
	public class ControllersTests : IDisposable
	{
		private readonly TestServer _server;
		private readonly HttpClient _client;

		public ControllersTests()
		{
			var webHostBuilder = new WebHostBuilder()
				.UseStartup<Startup>()
				.ConfigureAppConfiguration(config =>
				{
					config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
				})
				.UseEnvironment(Environments.Development);

			_server = new TestServer(webHostBuilder);
			_client = _server.CreateClient();
		}

		public void Dispose()
		{
			_client?.Dispose();
			_server?.Dispose();
		}

		[Fact]
		public async Task ControllersTests_AliveController()
		{
			var json = await _client.GetStringAsync("/");

			Assert.NotNull(json);
			Assert.NotEmpty(json);
			Assert.StartsWith("{", json);
			Assert.Matches(
				@"^{""machineName"":"".+?"",""applicationName"":"".+?"",""environmentName"":""Development"",""currentCulture"":"".+?"",""currentUICulture"":"".+?"",""timeZone"":"".+?"",""serverTime"":"".+?""}$",
				json);
		}

		[Theory]
		[InlineData("ashton")]
		public async Task ControllersTests_CinemasController(params string[] names)
		{
			var json = await _client.GetStringAsync("/v1/cinemas/?" + string.Join("&", names.Select(n => "name=" + n)));

			Assert.NotNull(json);
			Assert.NotEmpty(json);
			Assert.StartsWith("[", json);
			Assert.Matches(@"{""id"":\d+,""name"":"".+?""}", json);
		}
	}
}
