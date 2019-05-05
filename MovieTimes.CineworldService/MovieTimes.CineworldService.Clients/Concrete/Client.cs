using Dawn;
using Helpers.Tracing;
using Microsoft.Extensions.Logging;
using MovieTimes.CineworldService.Models.Helpers;
using OpenTracing;
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
		private readonly ITracer _tracer;
		private readonly HttpClient _httpClient;

		public Client(
			ILogger<Client> logger,
			ITracer tracer,
			IHttpClientFactory httpClientFactory)
		{
			_logger = Guard.Argument(() => logger).NotNull().Value;
			_tracer = Guard.Argument(() => tracer).NotNull().Value;

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

			using (var scope = _tracer.BuildSpan($"{this.GetType().Name}.{nameof(SendAsync)}")
				.WithTag(nameof(httpMethod), httpMethod.Method)
				.WithTag(nameof(relativeUri), relativeUri.OriginalString)
				.WithTag(nameof(body), body)
				.StartActive(finishSpanOnDispose: true))
			{
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
					scope.Span.Log(nameof(ex), ex.ToJsonString());

					_logger.LogCritical(ex, "{0}={1}, {2}={3}, {4}={5}", nameof(httpMethod), httpMethod.Method, nameof(relativeUri), relativeUri.OriginalString, nameof(body), body);
					return default;
				}

				var responseStatusCode = httpResponseMessage.StatusCode;
				var responseContent = await httpResponseMessage.Content?.ReadAsStringAsync();
				var responseHeaders = httpResponseMessage.Content?.Headers;

				scope.Span.Log(
					nameof(responseStatusCode), responseStatusCode,
					nameof(responseContent), responseContent,
					nameof(responseHeaders), responseHeaders?.ToJsonString());

				_logger.LogInformation("{0}={1}, {2}={3}, {4}={5}", nameof(responseStatusCode), responseStatusCode, nameof(responseContent), responseContent.Truncate(), nameof(responseHeaders), responseHeaders?.ToJsonString());

				return (responseStatusCode, responseContent, responseHeaders);
			}
		}
	}
}
