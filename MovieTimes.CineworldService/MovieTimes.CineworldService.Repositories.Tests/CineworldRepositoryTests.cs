using Microsoft.Extensions.Logging;
using Moq;
using OpenTracing;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace MovieTimes.CineworldService.Repositories.Tests
{
	public class CineworldRepositoryTests
	{
		private readonly ICineworldRepository _repository;

		public CineworldRepositoryTests()
		{
			var dbSettings = new Configuration.DbSettings
			{
				Server = "localhost",
				Port = 3_306,
				UserId = "movietimes",
				Password = "xiebeiyoothohYaidieroh8ahchohphi",
				Database = "cineworld",
			};

			var logger = Mock.Of<ILogger<Concrete.CineworldRepository>>();
			var tracer = BuildTracer();

			_repository = new Concrete.CineworldRepository(logger, tracer, "localhost", 3_306, "movietimes", "xiebeiyoothohYaidieroh8ahchohphi", "cineworld");
		}

		[Theory]
		[InlineData("2019-07-04T17:19:12Z")]
		public async Task CineworldRepositoryTests_Log(string lastModifiedString)
		{
			// Arrange
			var lastModified = DateTime.Parse(lastModifiedString).ToUniversalTime();

			// Act
			await _repository.LogAsync(lastModified);

			var actual = await _repository.GetLastModifiedFromLogAsync();

			// Assert
			Assert.NotNull(actual);
			Assert.Equal(DateTimeKind.Utc, actual.Value.Kind);
			Assert.Equal(lastModified, actual);
		}

		private static readonly ICollection<(string, object)> _log = new List<(string, object)>();

		private static ITracer BuildTracer()
		{
			_log.Clear();

			var spanMock = new Mock<ISpan>();

			spanMock
				.Setup(s => s.Log(It.IsAny<IEnumerable<KeyValuePair<string, object>>>()))
				.Callback<IEnumerable<KeyValuePair<string, object>>>(dictionary =>
				{
					foreach (var (key, value) in dictionary)
					{
						_log.Add((key, value));
					}
				});

			var scope = Mock.Of<IScope>(s => s.Span == spanMock.Object);
			var spanBuilder = Mock.Of<ISpanBuilder>(sb => sb.StartActive(It.IsAny<bool>()) == scope);
			var tracer = Mock.Of<ITracer>(t => t.BuildSpan(It.IsAny<string>()) == spanBuilder);

			return tracer;
		}
	}
}
