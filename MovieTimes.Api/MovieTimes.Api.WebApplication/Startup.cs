using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MovieTimes.Api.WebApplication
{
	public class Startup
	{
		private readonly IConfiguration _configuration;
		private readonly string _applicationName;

		public Startup(IConfiguration configuration, IWebHostEnvironment env)
		{
			_applicationName = env.ApplicationName;

			var isDevelopment = env.IsDevelopment();

			_configuration = configuration
				.AddDockerSecret("MySqlCineworldPassword", optional: isDevelopment, reloadOnChange: true)
				.AddDockerSecret("MySqlCineworldUser",     optional: isDevelopment, reloadOnChange: true);
		}

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			services
				.AddControllers(mvcOptions=>
				{
					//mvcOptions.OutputFormatters.Add(new PlainTextOutputFormatter());
				})
				.AddNewtonsoftJson();

			services
				.Configure<Configuration.DbSettings>(_configuration.GetSection(nameof(Configuration.DbSettings)))
				.Configure<Configuration.DockerSecrets>(_configuration.GetSection(nameof(Configuration.DockerSecrets)))
				.Configure<Configuration.JaegerSettings>(_configuration.GetSection(nameof(Configuration.JaegerSettings)));

			var jaegerSettings = _configuration.GetSection(nameof(Configuration.JaegerSettings)).Get<Configuration.JaegerSettings>();

			services
				.AddJaegerTracing(_applicationName, jaegerSettings.Host, jaegerSettings.Port);

			services
				.AddTransient<Repositories.ICineworldRepository, Repositories.Concrete.CineworldRepository>();

			services
				.AddLogging(builder =>
				{
					builder
						.AddConfiguration(_configuration.GetSection("Logging"))
						.AddConsole()
						.AddDebug()
						.AddEventSourceLogger();
				});
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}

			app.UseRouting();

			app.UseAuthorization();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllers();
			});
		}
	}
}
