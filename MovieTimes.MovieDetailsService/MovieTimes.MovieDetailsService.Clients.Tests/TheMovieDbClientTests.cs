using Microsoft.Extensions.Options;
using Moq;
using MovieTimes.MovieDetailsService.Clients;
using MovieTimes.MovieDetailsService.Clients.Concrete;
using MovieTimes.MovieDetailsService.Configuration;
using NUnit.Framework;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Tests
{
	public class TheMovieDbClientTests
	{
		private ITheMovieDbClient _client;

		[Test, Order(1)]
		public void TheMovieDbClientTests_Constructor()
		{
			var client = new HttpClient
			{
				BaseAddress = new Uri("https://api.themoviedb.org/", UriKind.Absolute),
			};

			var httpClientFactory = Mock.Of<IHttpClientFactory>(f => f.CreateClient(It.IsAny<string>()) == client);

			var settings = new TheMovieDbSettings { ApiKey = "40d01b496d7c0a0f2e9337669ac3fbbd", };

			var settingsOptions = Mock.Of<IOptions<TheMovieDbSettings>>(o => o.Value == settings);

			_client = new TheMovieDbClient(httpClientFactory, settingsOptions);

			Assert.IsNotNull(_client);
		}

		[Test]
		[TestCase("lego")]
		public async Task TheMovieDbClientTests_SearchAsync(string query)
		{
			var actual = await _client.SearchAsync(query);

			Assert.IsNotNull(actual);
			Assert.IsNotEmpty(actual);
			Assert.AreEqual('{', actual[0]);
		}
	}
}
