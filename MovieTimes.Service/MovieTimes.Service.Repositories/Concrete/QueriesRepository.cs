using Microsoft.Extensions.Options;
using System.Collections.Generic;

namespace MovieTimes.Service.Repositories.Concrete
{
	public class QueriesRepository : Helpers.MySql.RepositoryBase, IQueriesRepository
	{
		public QueriesRepository(IOptions<Helpers.MySql.Models.DbSettings> options)
			: base(options.Value)
		{ }

		public IAsyncEnumerable<(int id, string query)> GetQueriesAsync()
			=> base.QueryAsync<(int, string)>("SELECT * FROM queries.saved;");
	}
}
