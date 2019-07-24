using Moq;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace MovieTimes.Service.Clients.Tests
{
	public class CineworldClient : IDisposable
	{
		private readonly Clients.ICineworldClient _client;
		private readonly HttpClient _httpClient;

		public CineworldClient()
		{
			var baseAddress = new Uri("https://www.cineworld.co.uk/", UriKind.Absolute);
			_httpClient = new HttpClient { BaseAddress = baseAddress, };
			var httpClientFactory = Mock.Of<IHttpClientFactory>(f => f.CreateClient(It.IsAny<string>()) == _httpClient);

			_client = new Clients.Concrete.CineworldClient(httpClientFactory);
		}

		public void Dispose()
		{
			_httpClient?.Dispose();
			_client?.Dispose();
		}

		[Fact]
		public async Task CineworldClient_GetHeadersAsync()
		{
			// Arrange
			var now = DateTime.UtcNow;

			// Act
			var headers = await _client.GetHeadersAsync();

			// Assert
			Assert.NotNull(headers);
			Assert.NotNull(headers.ContentLength);
			Assert.NotNull(headers.LastModified);
			Assert.InRange(headers.ContentLength!.Value, 1, long.MaxValue);
			Assert.Equal(DateTimeKind.Utc, headers.LastModified!.Value.Kind);
			Assert.InRange(headers.LastModified!.Value, now.Date.AddDays(-7), now);
		}
	}
}
