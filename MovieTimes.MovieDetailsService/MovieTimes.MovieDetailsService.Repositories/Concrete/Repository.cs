using Helpers.MySql;
using Microsoft.Extensions.Logging;
using MovieTimes.MovieDetailsService.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MovieTimes.MovieDetailsService.Repositories.Concrete
{
	public class Repository : RepositoryBase, IRepository
	{
		public Repository(
			string connectionString,
			ILogger<RepositoryBase> logger = null)
			: base(connectionString, logger)
		{ }

		public Task<IEnumerable<(int edi, string title)>> GetMoviesMissingMappingAsync()
		{
			var sql = @"SELECT f.edi, f.title
				FROM cineworld.film f
					LEFT OUTER JOIN moviedetails.mapping m ON f.edi=m.edi
				WHERE m.id IS NULL;";

			return QueryAsync<(int, string)>(sql);
		}

		public Task SaveAsync(ICollection<PersistenceData.Movie> movies)
		{
			return Task.WhenAll(
				ExecuteAsync(
					"INSERT `moviedetails`.`movie` (id, imdbId, title, runtime) VALUES (@Id, @ImdbId, @Title, @Runtime);",
					from movie in movies
					where movie.Runtime != default
					select new
					{
						movie.Id,
						movie.ImdbId,
						movie.Title,
						Runtime = (int)movie.Runtime.Value.TotalMinutes,
					}),
				ExecuteAsync(
					"INSERT `moviedetails`.`mapping` (id, edi, formats) VALUES (@Id, @Edi, @Formats);",
					from movie in movies
					group movie by (movie.Id, movie.Edi, movie.Formats) into gg
					select new
					{
						gg.Key.Id,
						gg.Key.Edi,
						Formats = (int)gg.Key.Formats,
					})
			);
		}
	}
}
