using Dawn;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MovieTimes.CineworldService.Models.Generated;
using MovieTimes.CineworldService.Models.Helpers;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace MovieTimes.CineworldService.ConsoleApp
{
	public class HostedService : IHostedService
	{
		private readonly ILogger _logger;
		private readonly Clients.ICineworldClient _cineworldClient;
		private readonly Services.ICineworldService _cineworldService;
		private readonly Repositories.ICineworldRepository _cineworldRepository;
		private readonly System.Timers.Timer _timer;

		private DateTime _lastModified;

		public HostedService(
			ILogger<HostedService> logger,
			IOptions<Configuration.Settings> settingsOptions,
			Clients.ICineworldClient cineworldClient,
			Services.ICineworldService cineworldService,
			Repositories.ICineworldRepository cineworldRepository)
		{
			_logger = Guard.Argument(() => logger).NotNull().Value;

			Guard.Argument(() => settingsOptions).NotNull();
			Guard.Argument(() => settingsOptions.Value).NotNull();
			Guard.Argument(() => settingsOptions.Value.IntervalMS).InRange(1, 86_400_000);

			_cineworldClient = Guard.Argument(() => cineworldClient).NotNull().Value;
			_cineworldService = Guard.Argument(() => cineworldService).NotNull().Value;
			_cineworldRepository = Guard.Argument(() => cineworldRepository).NotNull().Value;

			_timer = new System.Timers.Timer
			{
				AutoReset = true,
				Enabled = true,
				Interval = settingsOptions.Value.IntervalMS,
			};

			_timer.Elapsed += OnTimedEventAsync;

			OnTimedEventAsync(this, default);
		}

		private async void OnTimedEventAsync(object sender, ElapsedEventArgs e)
		{
			// Get last modified
			var lastModified = await _cineworldClient.GetListingsLastModifiedAsync();

			_logger.LogInformation("{0}={1:O}, {2}={3:O}, {4}", nameof(_lastModified), _lastModified, nameof(lastModified), lastModified, lastModified <= _lastModified);

			// Newer than last time?
			if (lastModified <= _lastModified)
			{
				return;
			}

			// Get listings
			var cinemas = await _cineworldService.GetCinemasAsync();

			var (cinemaCount, filmCount, showCount) = cinemas.GetCounts();

			_logger.LogInformation("Downloaded {0:D} {1}(s), {2:D} {3}(s), and {4:D} {5}(s)", cinemaCount, nameof(cinema), filmCount, nameof(film), showCount, nameof(show));

			// Save listings
			_cineworldRepository.SaveCinemasAsync(cinemas);

			_logger.LogInformation("Saved");

			// Store last modified
			_lastModified = lastModified;
		}

		public Task StartAsync(CancellationToken cancellationToken)
		{
			_timer.Start();
			return Task.CompletedTask;
		}

		public Task StopAsync(CancellationToken cancellationToken)
		{
			_timer.Stop();
			return Task.CompletedTask;
		}
	}
}
