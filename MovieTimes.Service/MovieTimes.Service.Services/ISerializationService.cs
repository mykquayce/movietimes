using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace MovieTimes.Service.Services
{
	public interface ISerializationService
	{
		Task<T> DeserializeAsync<T>(Stream stream);

		Task<T> DeserializeAsync<T>(string s)
		{
			var bytes = Encoding.UTF8.GetBytes(s);

			using var stream = new MemoryStream(bytes);

			return DeserializeAsync<T>(stream);
		}

		Task<Stream> SerializeAsync<T>(T value);
	}
}