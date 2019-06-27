using Dawn;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace MovieTimes.MovieDetailsService.Clients.Concrete
{
	public abstract class ClientBase
	{
		private readonly HttpClient _httpClient;

		public ClientBase(
			IHttpClientFactory httpClientFactory)
		{
			Guard.Argument(() => httpClientFactory).NotNull();

			_httpClient = httpClientFactory.CreateClient(this.GetType().Name);

			Guard.Argument(() => _httpClient).NotNull();
			Guard.Argument(() => _httpClient.BaseAddress).NotNull();
			Guard.Argument(() => _httpClient.BaseAddress.OriginalString).NotNull();
		}

		public async Task<(HttpStatusCode statusCode, HttpResponseHeaders headers, Stream content)> SendAsync(
			HttpMethod method,
			Uri relativeUri)
		{
			Guard.Argument(() => method).NotNull();
			Guard.Argument(() => method.Method).NotNull();
			Guard.Argument(() => relativeUri).NotNull();
			Guard.Argument(() => relativeUri.OriginalString).NotNull();

			try
			{
				var request = new HttpRequestMessage(method, relativeUri);

				var response = await _httpClient.SendAsync(request);

				return (
					response.StatusCode,
					response.Headers,
					await response.Content.ReadAsStreamAsync());
			}
			catch (Exception ex)
			{
				ex.Data.Add(nameof(method), method.Method);
				ex.Data.Add(nameof(Uri), _httpClient.BaseAddress.ToString() + relativeUri.ToString());
				throw;
			}
		}

		public Task<(HttpStatusCode statusCode, HttpResponseHeaders headers, Stream content)> SendAsync(
			HttpMethod method,
			string relativeUriString)
		{
			Guard.Argument(() => relativeUriString).NotNull();

			var relativeUri = new Uri(relativeUriString, UriKind.Relative);

			return SendAsync(method, relativeUri);
		}
	}
}
