using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using MovieTimes.CineworldService.Clients.Concrete;
using OpenTracing;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace MovieTimes.CineworldService.Clients.Tests
{
	public class CineworldClientTests
	{
		private readonly ICineworldClient _cineworldClient;

		public CineworldClientTests()
		{
			var httpClient = new HttpClient { BaseAddress = new Uri("https://www.cineworld.co.uk/", UriKind.Absolute), };

			var logger = Mock.Of<ILogger<CineworldClient>>();

			var tracer = Mock.Of<ITracer>();

			var httpClientFactoryMock = new Mock<IHttpClientFactory>();

			httpClientFactoryMock
				.Setup(x => x.CreateClient(It.Is<string>(s => s == "CineworldClient")))
				.Returns(httpClient);

			var uris = new Configuration.Uris { ListingsUri = "/syndication/listings.xml", };

			var urisOptions = Mock.Of<IOptions<Configuration.Uris>>(o => o.Value == uris);

			_cineworldClient = new CineworldClient(logger, tracer, httpClientFactoryMock.Object, urisOptions);
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

		[Theory]
		[InlineData(1_024_576, 102_457_600)]
		public async Task CineworldClientTests_GetListingsAsync_ReturnsAJsonString_BetweenNAndNBytes(
			int minSize, int maxSize)
		{
			// Act
			var actual = await _cineworldClient.GetListingsAsync();

			// Assert
			Assert.NotNull(actual);
			Assert.NotEmpty(actual);
			Assert.StartsWith("<", actual);
			Assert.InRange(actual.Length, minSize, maxSize);
		}
	}
}
