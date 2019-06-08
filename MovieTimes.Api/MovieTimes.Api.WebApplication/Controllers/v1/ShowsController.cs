using Dawn;
using Helpers.Tracing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MovieTimes.Api.Repositories;
using OpenTracing;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MovieTimes.Api.WebApplication.Controllers.v1
{
	[Route("v1/[controller]")]
	[ApiController]
	public class ShowsController : ControllerBase
	{
		private readonly ITracer _tracer;
		private readonly ILogger _logger;
		private readonly ICineworldRepository _cineworldRepository;

		public ShowsController(
			ITracer tracer,
			ILogger<CinemasController> logger,
			ICineworldRepository cineworldRepository)
		{
			_tracer = tracer;
			_logger = logger;
			_cineworldRepository = Guard.Argument(() => cineworldRepository).NotNull().Value;
		}

		[HttpGet]
		[Route("")]
		public async Task<IActionResult> Get(
			[FromQuery(Name = "cinema")] ICollection<string> cinemas,
			[FromQuery(Name = "dayofweek")] ICollection<Models.DaysOfWeek> daysOfWeeks,
			[FromQuery(Name = "timeofday")] ICollection<Models.TimesOfDay> timesOfDays,
			[FromQuery(Name = "title")] ICollection<string> searchTerms)
		{
			var daysOfWeek = daysOfWeeks.Aggregate(seed: Models.DaysOfWeek.None, (curr, sum) => sum |= curr);
			var timesOfDay = timesOfDays.Aggregate(seed: Models.TimesOfDay.None, (curr, sum) => sum |= curr);

			var cinemasString = string.Join(",", cinemas);
			var searchTermsString = string.Join(",", searchTerms);

			using var scope = _tracer?.BuildDefaultSpan()
				.WithTag(nameof(cinemas), cinemasString)
				.WithTag(nameof(daysOfWeek), daysOfWeek.ToString("F"))
				.WithTag(nameof(timesOfDay), timesOfDay.ToString("F"))
				.WithTag(nameof(searchTerms), searchTermsString)
				.StartActive(finishSpanOnDispose: true);

			_logger?.LogInformation($"{nameof(cinemas)}={cinemasString};{nameof(daysOfWeek)}={daysOfWeek:F};{nameof(timesOfDay)}={timesOfDay:F};{nameof(searchTerms)}={searchTermsString}");

			ICollection<short> cinemaIds;

			if (cinemas?.Count > 0)
			{
				cinemaIds = (from tuple in await _cineworldRepository.GetCinemasAsync(cinemas)
							 select tuple.id
							).ToList();
			}
			else
			{
				cinemaIds = new short[0];
			}

			var shows = await _cineworldRepository.GetShowsAsync(cinemaIds, daysOfWeek, timesOfDay, searchTerms ?? new string[0]);

			return Ok(from s in shows
					  orderby s.cinemaName, s.dateTime, s.title
					  select new
					  {
						  s.cinemaName,
						  s.dateTime,
						  s.title,
					  });
		}
	}
}
