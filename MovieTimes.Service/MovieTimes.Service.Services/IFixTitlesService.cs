using System.Collections.Generic;

namespace MovieTimes.Service.Services
{
	public interface IFixTitlesService
	{
		(string title, Models.Formats format) Sanitize(string title);
		(int edi, string title, Models.Formats format) Sanitize(Models.Generated.film film);
		IEnumerable<(int edi, string title, Models.Formats format)> Sanitize(IEnumerable<Models.Generated.film> films);
	}
}
