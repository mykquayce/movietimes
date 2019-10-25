using Helpers.Cineworld.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MovieTimes.Service.Repositories
{
	public interface IMovieDetailsRepository
	{
		Task SaveSanitizedTitleAsync(int edi, string title, Formats format);
		Task SaveSanitizedTitlesAsync(IEnumerable<(int edi, string title, Formats format)> ediTitleFormatTuples);
	}
}
