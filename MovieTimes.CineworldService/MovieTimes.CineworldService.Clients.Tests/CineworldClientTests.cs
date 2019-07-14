using Microsoft.Extensions.Options;
using Moq;
using MovieTimes.CineworldService.Clients.Concrete;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace MovieTimes.CineworldService.Clients.Tests
{
	public class CineworldClientTests : IDisposable
	{
		private readonly ICineworldClient _cineworldClient;
		private readonly HttpClient _httpClient;

		public CineworldClientTests()
		{
			_httpClient = new HttpClient { BaseAddress = new Uri("https://www.cineworld.co.uk/", UriKind.Absolute), };

			var httpClientFactoryMock = new Mock<IHttpClientFactory>();

			httpClientFactoryMock
				.Setup(x => x.CreateClient(It.Is<string>(s => s == "CineworldClient")))
				.Returns(_httpClient);

			var uris = new Configuration.Uris { ListingsUri = "/syndication/listings.xml", };

			var urisOptions = Mock.Of<IOptions<Configuration.Uris>>(o => o.Value == uris);

			_cineworldClient = new CineworldClient(logger: default, tracer: default, httpClientFactoryMock.Object, urisOptions);
		}

		public void Dispose()
		{
			_httpClient?.Dispose();
		}

		[Theory]
		[InlineData(3)]
		public async Task CineworldClientTests_GetListingsLastModifiedAsync_ReturnsADateInTheLastNDays(
			int daysOld)
		{
			// Act
			var actual = await _cineworldClient.GetListingsLastModifiedAsync();

			// Assert
			Assert.InRange(
				actual,
				DateTime.UtcNow.Date.AddDays(-daysOld),
				DateTime.UtcNow);
		}

		[Fact]
		public async Task CineworldClientTests_GetListingsAsync_ReturnsCinemas()
		{
			// Act
			var cinemas = await _cineworldClient.GetListingsAsync();

			// Assert
			Assert.NotNull(cinemas);
			Assert.NotEmpty(cinemas.cinema);
		}
	}
}
