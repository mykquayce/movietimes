using Dawn;
using Microsoft.Extensions.Logging;
using MovieTimes.CineworldService.Models.Generated;
using MovieTimes.CineworldService.Models.Helpers;
using OpenTracing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
			using (var scope = _tracer.BuildSpan($"{nameof(CineworldService)}.{nameof(GetCinemasAsync)}")
				.StartActive(finishSpanOnDispose: true))
			{
				cinemas cinemas;
				var xml = await _cineworldClient.GetListingsAsync();

				Log((nameof(xml), xml.Truncate()));

				using (var reader = new StringReader(xml))
				{
					try
					{
						cinemas = (cinemas)_xmlSerializer.Deserialize(reader);
					}
					catch (Exception ex)
					{
						Log((nameof(ex), ex), (nameof(xml), xml.Truncate()));
						_logger.LogCritical(ex, "{0}={1}", nameof(xml), xml.Truncate());
						return default;
					}
				}

				var (cinemaCount, filmCount, showCount) = cinemas.GetCounts();

				Log(
					(nameof(cinemaCount), cinemaCount),
					(nameof(filmCount), filmCount),
					(nameof(showCount), showCount)
					);

				return cinemas;
			}
		}

		public void Log(params (string name, object value)[] values) => Log(values.ToDictionary(t => t.name, t => t.value));

		public void Log(IDictionary<string, object> dictionary)
		{
			var message = string.Join(", ", dictionary.Select(kvp => $"{kvp.Key}={kvp.Value}"));

			_logger?.LogInformation(message);

			_tracer?.ActiveSpan?.Log(dictionary);
		}
	}
}
