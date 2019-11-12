using Moq;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace MovieTimes.Service.Clients.Tests
{
	public sealed class ApiClientTests : IDisposable
	{
		private readonly IApiClient _apiClient;

		public ApiClientTests()
		{
			var httpClient = new HttpClient
			{
				BaseAddress = new Uri("https://www.bing.com/", UriKind.Absolute),
			};

			var httpClientFactory = Mock.Of<IHttpClientFactory>(factory => factory.CreateClient(It.IsAny<string>()) == httpClient);

			_apiClient = new Concrete.ApiClient(httpClientFactory);
		}

		public void Dispose()
		{
			_apiClient?.Dispose();
		}

		[Fact]
		public async Task ApiClientTests_RunQueryAsync_BehavesPredictably()
		{
			var relativeUri = new Uri("/", UriKind.Relative);

			var actual = await _apiClient.RunQueryAsync(relativeUri);

			Assert.NotNull(actual);
			Assert.NotEmpty(actual);
		}
	}
}
