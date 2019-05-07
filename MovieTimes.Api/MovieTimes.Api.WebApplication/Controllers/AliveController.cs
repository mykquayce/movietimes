using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;

namespace MovieTimes.Api.WebApplication.Controllers
{
	[Route("")]
	[ApiController]
	public class AliveController : ControllerBase
	{
		private readonly IHostingEnvironment _hostingEnvironment;

		public AliveController(
			IHostingEnvironment hostingEnvironment)
		{
			_hostingEnvironment = hostingEnvironment;
		}

		[HttpGet]
		public IActionResult Get()
		{
			var thread = Thread.CurrentThread;

			return Ok(new
			{
				Environment.MachineName,
				_hostingEnvironment.ApplicationName,
				_hostingEnvironment.EnvironmentName,
				thread.CurrentCulture,
				thread.CurrentUICulture,
				TimeZone = TimeZoneInfo.Local.StandardName,
				ServerTime = DateTime.UtcNow,
			});
		}
	}
}
