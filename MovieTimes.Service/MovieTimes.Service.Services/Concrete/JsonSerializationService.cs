using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace MovieTimes.Service.Services.Concrete
{
	public class JsonSerializationService : ISerializationService
	{
		private static readonly JsonSerializerOptions _options = new JsonSerializerOptions
		{
			AllowTrailingCommas = true,
			IgnoreNullValues = false,
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
			PropertyNameCaseInsensitive = true,
			WriteIndented = false,
		};

		public Task<T> DeserializeAsync<T>(Stream stream)
			=> JsonSerializer.DeserializeAsync<T>(stream, _options).AsTask();

		public async Task<Stream> SerializeAsync<T>(T value)
		{
			var stream = new MemoryStream();

			await JsonSerializer.SerializeAsync(stream, value, _options);

			stream.Position = 0;

			return stream;
		}
	}
}
