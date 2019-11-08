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
	public class CinemasController : ControllerBase
	{
		private readonly ITracer _tracer;
		private readonly ILogger _logger;
		private readonly ICineworldRepository _cineworldRepository;

		public CinemasController(
			ITracer tracer,
			ILogger<CinemasController> logger,
			ICineworldRepository cineworldRepository)
		{
			_tracer = Guard.Argument(() => tracer).NotNull().Value;
			_logger = Guard.Argument(() => logger).NotNull().Value;
			_cineworldRepository = Guard.Argument(() => cineworldRepository).NotNull().Value;
		}

		[HttpGet]
		[Route("")]
		public async Task<IActionResult> Get(
			[FromQuery(Name = "name")] ICollection<string>? searchTerms = default)
		{
			var searchTermsString = (searchTerms?.Any() ?? false) ? string.Join(", ", searchTerms!) : default;

			using var scope = _tracer.BuildDefaultSpan()
				.WithTag(nameof(searchTerms), searchTermsString)
				.StartActive(finishSpanOnDispose: true);

			_logger.LogInformation($"{nameof(searchTerms)}={searchTermsString}");

			if (searchTerms == default) searchTerms = new List<string> { string.Empty, };

			if (searchTerms.Count == 0) searchTerms.Add(string.Empty);

			var cinemas = new List<Models.Cinema>();

			foreach (var searchTerm in from n in searchTerms
									   where !string.IsNullOrWhiteSpace(n)
									   select n)
			{
				await foreach (var cinema in _cineworldRepository.GetCinemasAsync(searchTerm))
				{
					cinemas.Add(cinema);
				}
			}

			return Ok(from c in cinemas
					  group c by c.Id into gg
					  select new
					  {
						  id = gg.Key,
						  name = gg.First().Name,
					  });
		}
	}
}
