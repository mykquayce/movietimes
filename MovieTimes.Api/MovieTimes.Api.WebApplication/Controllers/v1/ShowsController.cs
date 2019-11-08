using Dawn;
using Helpers.Tracing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MovieTimes.Api.Models;
using MovieTimes.Api.Repositories;
using OpenTracing;
using System;
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
			[FromQuery(Name = "dayofweek")] ICollection<Models.Enums.DaysOfWeek> daysOfWeeks,
			[FromQuery(Name = "timeofday")] ICollection<Models.Enums.TimesOfDay> timesOfDays,
			[FromQuery(Name = "title")] ICollection<string> searchTerms,
			[FromQuery(Name = "weekCount")] int weekCount)
		{
			var daysOfWeek = daysOfWeeks.Aggregate(seed: Models.Enums.DaysOfWeek.None, (curr, sum) => sum |= curr);
			var timesOfDay = timesOfDays.Aggregate(seed: Models.Enums.TimesOfDay.None, (curr, sum) => sum |= curr);

			var cinemasString = string.Join(",", cinemas);
			var searchTermsString = string.Join(",", searchTerms);

			using var scope = _tracer?.BuildDefaultSpan()
				.WithTag(nameof(cinemas), cinemasString)
				.WithTag(nameof(daysOfWeek), daysOfWeek.ToString("F"))
				.WithTag(nameof(timesOfDay), timesOfDay.ToString("F"))
				.WithTag(nameof(searchTerms), searchTermsString)
				.StartActive(finishSpanOnDispose: true);

			_logger?.LogInformation($"{nameof(cinemas)}={cinemasString};{nameof(daysOfWeek)}={daysOfWeek:F};{nameof(timesOfDay)}={timesOfDay:F};{nameof(searchTerms)}={searchTermsString}");

			var cinemaIds = new List<short>();

			if (cinemas?.Count > 0)
			{
				await foreach (var cinema in _cineworldRepository.GetCinemasAsync(cinemas))
				{
					cinemaIds.Add(cinema.Id);
				}
			}

			var shows = new List<Show>();

			await foreach (var show in _cineworldRepository.GetShowsAsync(cinemaIds, daysOfWeek, timesOfDay, searchTerms ?? new string[0], weekCount))
			{
				shows.Add(show);
			}

			return Ok(from s in shows
					  orderby s.Cinema.Name
					  select new
					  {
						  cinema = s.Cinema.Name,
						  dateTime = s.DateTime,
						  movie = s.Movie.Title,
						  duration = s.Movie.Duration,
					  });
		}
	}
}
