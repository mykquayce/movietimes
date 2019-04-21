using Dawn;
using Microsoft.Extensions.Logging;
using MovieTimes.CineworldService.Models.Generated;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace MovieTimes.CineworldService.Services.Concrete
{
	public class CineworldService : ICineworldService
	{
		private readonly ILogger<CineworldService> _logger;
		private readonly Clients.ICineworldClient _cineworldClient;
		private readonly XmlSerializer _xmlSerializer;

		public CineworldService(
			ILogger<CineworldService> logger,
			Clients.ICineworldClient cineworldClient)
		{
			_logger = Guard.Argument(() => logger).NotNull().Value;
			_cineworldClient = Guard.Argument(() => cineworldClient).NotNull().Value;

			_xmlSerializer = new XmlSerializer(typeof(cinemas));
		}

		public async Task<cinemas> GetCinemasAsync()
		{
			cinemas cinemas;
			var xml = await _cineworldClient.GetListingsAsync();

			_logger.LogDebug("{0}={1}", nameof(xml), xml?.Substring(0, 100));

			using (var reader = new StringReader(xml))
			{
				try
				{
					cinemas = (cinemas)_xmlSerializer.Deserialize(reader);
				}
				catch (Exception ex)
				{
					_logger.LogCritical(ex, "{0}={1}", nameof(xml), xml?.Substring(0, 100));
					return default;
				}
			}

			_logger.LogDebug("{0} {1}(s), {2} {3}(s)", cinemas.cinema?.Count ?? 0, nameof(cinema), cinemas.cinema?.Aggregate(0, (seed, c) => seed += c?.listing?.Count ?? 0) ?? 0, nameof(film));

			return cinemas;
		}
	}
}
