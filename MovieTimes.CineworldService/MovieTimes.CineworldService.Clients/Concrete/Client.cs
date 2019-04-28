using Dawn;
using Microsoft.Extensions.Logging;
using MovieTimes.CineworldService.Models.Helpers;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace MovieTimes.CineworldService.Clients.Concrete
{
	public class Client : IClient
	{
		private readonly ILogger _logger;
		private readonly HttpClient _httpClient;

		public Client(
			ILogger<Client> logger,
			IHttpClientFactory httpClientFactory)
		{
			_logger = Guard.Argument(() => logger).NotNull().Value;

			Guard.Argument(() => httpClientFactory).NotNull();

			_httpClient = httpClientFactory.CreateClient(this.GetType().Name);

			Guard.Argument(() => _httpClient).NotNull();
			Guard.Argument(() => _httpClient.BaseAddress)
				.NotNull()
				.Require(u => !string.IsNullOrWhiteSpace(u.OriginalString), _ => nameof(httpClientFactory) + " has a blank base address");
		}

		public async Task<(HttpStatusCode, string, HttpContentHeaders)> SendAsync(HttpMethod httpMethod, Uri relativeUri, string body = null)
		{
			Guard.Argument(() => httpMethod).NotNull()
				.Require(m => !string.IsNullOrWhiteSpace(m.Method), _ => nameof(httpMethod) + " is blank");

			Guard.Argument(() => relativeUri)
				.NotNull()
				.Require(u => !u.IsAbsoluteUri, _ => nameof(relativeUri) + " must be a relative URI")
				.Require(u => !string.IsNullOrWhiteSpace(u.OriginalString), _ => nameof(relativeUri) + " is blank");

			_logger.LogInformation("{0}={1}, {2}={3}, {4}={5}", nameof(httpMethod), httpMethod.Method, nameof(relativeUri), relativeUri.OriginalString, nameof(body), body.Truncate());

			var httpRequestMessage = new HttpRequestMessage(httpMethod, relativeUri);

			if (!string.IsNullOrWhiteSpace(body))
			{
				var requestContent = new StringContent(body, Encoding.UTF8, "application/json");

				httpRequestMessage.Content = requestContent;
			}

			HttpResponseMessage httpResponseMessage;

			try
			{
				httpResponseMessage = await _httpClient.SendAsync(httpRequestMessage);
			}
			catch (Exception ex)
			{
				_logger.LogCritical(ex, "{0}={1}, {2}={3}, {4}={5}", nameof(httpMethod), httpMethod.Method, nameof(relativeUri), relativeUri.OriginalString, nameof(body), body);
				return default;
			}

			var responseStatusCode = httpResponseMessage.StatusCode;
			var responseContent = await httpResponseMessage.Content?.ReadAsStringAsync();
			var responseHeaders = httpResponseMessage.Content?.Headers;

			_logger.LogInformation("{0}={1}, {2}={3}, {4}={5}", nameof(responseStatusCode), responseStatusCode, nameof(responseContent), responseContent.Truncate(), nameof(responseHeaders), responseHeaders?.ToJsonString());

			return (responseStatusCode, responseContent, responseHeaders);
		}
	}
}
