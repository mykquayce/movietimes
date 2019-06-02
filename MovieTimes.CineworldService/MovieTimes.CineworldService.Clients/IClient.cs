using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace MovieTimes.CineworldService.Clients
{
	public interface IClient
	{
		Task<(HttpStatusCode, string, HttpContentHeaders)> SendAsync(
			HttpMethod httpMethod,
			Uri relativeUri,
			string? body = default,
			[CallerFilePath] string? filePath = default,
			[CallerMemberName] string? methodName = default);
	}
}
