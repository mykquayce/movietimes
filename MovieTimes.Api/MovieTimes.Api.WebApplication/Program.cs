using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using System.Threading.Tasks;

namespace MovieTimes.Api.WebApplication
{
	public static class Program
	{
		public static Task Main(string[] args)
		{
			return WebHost.CreateDefaultBuilder(args)
				.UseStartup<Startup>()
				.Build()
				.RunAsync();
		}
	}
}
