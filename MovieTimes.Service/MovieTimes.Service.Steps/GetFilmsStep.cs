using Dawn;
using Helpers.Cineworld.Models.Generated;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace MovieTimes.Service.Steps
{
	public class GetFilmsStep : IStepBody
	{
		private readonly Helpers.Cineworld.ICineworldClient _client;

		public GetFilmsStep(Helpers.Cineworld.ICineworldClient client)
		{
			_client = Guard.Argument(() => client).NotNull().Value;
		}

		public ICollection<FilmType>? Films { get; set; }

		public async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
		{
			Films = await _client.GetFilmsAsync().ToListAsync();

			return ExecutionResult.Next();
		}
	}
}
