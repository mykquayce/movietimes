using Helpers.MySql;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MovieTimes.MovieDetailsService.Repositories.Concrete
{
	public class Repository : RepositoryBase, IRepository
	{
		public Repository(
			string connectionString,
			ILogger<RepositoryBase> logger = null)
			: base(connectionString, logger)
		{
			base.SafeCreateDatabaseAsync("movies").GetAwaiter().GetResult();
		}

		public async IAsyncEnumerable<(string title, short year)> GetMoviesMissingDetailsAsync()
		{
			base.QueryAsync<(string title, short year)>("")
		}
	}
}
