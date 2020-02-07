using System.IO;
using System.Threading.Tasks;

namespace MovieTimes.Service.Models.Tests
{
	public static class Helpers
	{
		private static readonly Services.ISerializationService _serializationService = new Services.Concrete.JsonSerializationService();

		public async static Task<T> DeserializeFileAsync<T>(string path)
		{
			using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.None);

			return await _serializationService.DeserializeAsync<T>(stream);
		}
	}
}
