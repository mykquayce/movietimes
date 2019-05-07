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
			[FromQuery(Name = "name")] IReadOnlyCollection<string> names = default)
		{
			var namesString = string.Join(", ", names);

			using (var scope = _tracer.BuildDefaultSpan()
				.WithTag(nameof(names), namesString)
				.StartActive(finishSpanOnDispose: true))
			{
				_logger.LogInformation($"{nameof(names)}={namesString}");

				if (names == default
					|| names.All(string.IsNullOrWhiteSpace))
				{
					return Ok(from c in await _cineworldRepository.GetCinemasAsync()
							  select new
							  {
								  c.id,
								  c.name,
							  });
				}

				var tasks = names.Where(s => !string.IsNullOrWhiteSpace(s)).Select(_cineworldRepository.GetCinemasAsync);

				var cinemases = await Task.WhenAll(tasks);

				return Ok(from cc in cinemases
						  from c in cc
						  group c by c.id into gg
						  orderby gg.Key
						  select new
						  {
							  id = gg.Key,
							  gg.First().name,
						  });
			}
		}
	}
}
