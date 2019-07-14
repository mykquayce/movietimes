using Dawn;
using Helpers.Tracing;
using Microsoft.Extensions.Logging;
using MovieTimes.CineworldService.Models.Generated;
using MovieTimes.CineworldService.Models.Helpers;
using OpenTracing;
using System.Threading.Tasks;

namespace MovieTimes.CineworldService.Services.Concrete
{
	public class CineworldService : ICineworldService
	{
		private readonly ILogger<CineworldService>? _logger;
		private readonly ITracer? _tracer;
		private readonly Clients.ICineworldClient _cineworldClient;

		public CineworldService(
			ILogger<CineworldService>? logger,
			ITracer? tracer,
			Clients.ICineworldClient cineworldClient)
		{
			_logger = logger;
			_tracer = tracer;

			_cineworldClient = Guard.Argument(() => cineworldClient).NotNull().Value;
		}

		public async Task<cinemas> GetCinemasAsync()
		{
			using var scope = _tracer?.BuildDefaultSpan()
				.StartActive(finishSpanOnDispose: true);

			var cinemas = await _cineworldClient.GetListingsAsync();

			var (cinemaCount, filmCount, showCount) = cinemas.GetCounts();

			scope?.Span.Log(
				nameof(cinemaCount), cinemaCount,
				nameof(filmCount), filmCount,
				nameof(showCount), showCount);

			_logger?.LogInformation($"{cinemaCount:D} cinema(s), {filmCount:D} film(s), {showCount:D} show(s)");

			return cinemas;
		}
	}
}
