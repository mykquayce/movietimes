using Dawn;
using Helpers.Tracing;
using Microsoft.Extensions.Logging;
using MovieTimes.CineworldService.Models.Generated;
using MovieTimes.CineworldService.Models.Helpers;
using OpenTracing;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace MovieTimes.CineworldService.Services.Concrete
{
	public class CineworldService : ICineworldService
	{
		private readonly ILogger<CineworldService> _logger;
		private readonly ITracer _tracer;
		private readonly Clients.ICineworldClient _cineworldClient;
		private readonly XmlSerializer _xmlSerializer;

		public CineworldService(
			ILogger<CineworldService> logger,
			ITracer tracer,
			Clients.ICineworldClient cineworldClient)
		{
			_logger = Guard.Argument(() => logger).NotNull().Value;
			_tracer = Guard.Argument(() => tracer).NotNull().Value;

			_cineworldClient = Guard.Argument(() => cineworldClient).NotNull().Value;

			_xmlSerializer = new XmlSerializer(typeof(cinemas));
		}

		public async Task<cinemas> GetCinemasAsync()
		{
			using (var scope = _tracer.BuildDefaultSpan()
				.StartActive(finishSpanOnDispose: true))
			{
				cinemas cinemas;
				var xml = await _cineworldClient.GetListingsAsync();

				scope.Span.Log(nameof(xml), xml.Truncate());
				_logger.LogInformation($"{nameof(xml)}={xml.Truncate()}");

				using (var reader = new StringReader(xml))
				{
					try
					{
						cinemas = (cinemas)_xmlSerializer.Deserialize(reader);
					}
					catch (Exception ex)
					{
						ex.Data.Add(nameof(xml), xml.Truncate());

						scope.Span.Log(
							nameof(ex), ex,
							nameof(xml), xml.Truncate());

						_logger.LogCritical(ex, $"{nameof(xml)}={xml.Truncate()}");

						throw;
					}
				}

				var (cinemaCount, filmCount, showCount) = cinemas.GetCounts();

				scope.Span.Log(
					nameof(cinemaCount), cinemaCount,
					nameof(filmCount), filmCount,
					nameof(showCount), showCount);

				_logger.LogInformation($"{cinemaCount:D} cinema(s), {filmCount:D} film(s), {showCount:D} show(s)");

				return cinemas;
			}
		}
	}
}
