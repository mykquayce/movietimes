using Microsoft.Extensions.Options;
using MovieTimes.Service.Models;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MovieTimes.Service.Repositories.Concrete
{
	public class MovieDetailsRepository : Helpers.MySql.RepositoryBase, IMovieDetailsRepository
	{
		public MovieDetailsRepository(IOptions<Helpers.MySql.Models.DbSettings> options)
			: base(options.Value)
		{ }

		public async Task SaveSanitizedTitleAsync(int edi, string title, Formats format)
		{
			using var transaction = base.BeginTransaction();

			try
			{
				await base.ExecuteAsync(
					"INSERT IGNORE INTO `moviedetails`.`movie` (`title`) VALUES (@title);",
					new { title, },
					transaction: transaction);

				await base.ExecuteAsync(
					"INSERT IGNORE INTO `moviedetails`.`mapping` (`edi`, `id`, `format`) VALUES (@edi, LAST_INSERT_ID(), @format);",
					new { edi, format, },
					transaction: transaction);

				transaction.Commit();
			}
			catch (MySqlException)
			{
				transaction.Rollback();
				throw;
			}
		}

		public Task SaveSanitizedTitlesAsync(IEnumerable<(int edi, string title, Formats format)> ediTitleFormatTuples)
		{
			return Task.WhenAll(from t in ediTitleFormatTuples
								select SaveSanitizedTitleAsync(t.edi, t.title, t.format));
		}
	}
}
