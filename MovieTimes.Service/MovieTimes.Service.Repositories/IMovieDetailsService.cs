﻿using System.Collections.Generic;
using System.Threading.Tasks;

namespace MovieTimes.Service.Repositories
{
	public interface IMovieDetailsRepository
	{
		Task SaveSanitizedTitleAsync(int edi, string title, Models.Formats format);
		Task SaveSanitizedTitlesAsync(IEnumerable<(int edi, string title, Models.Formats format)> ediTitleFormatTuples);
	}
}
