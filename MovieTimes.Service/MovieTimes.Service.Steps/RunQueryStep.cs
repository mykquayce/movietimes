using Dawn;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace MovieTimes.Service.Steps
{
	public class RunQueryStep : IStepBody
	{
		private readonly Repositories.ICineworldRepository _cineworldRepository;

		public RunQueryStep(Repositories.ICineworldRepository cineworldRepository)
		{
			_cineworldRepository = Guard.Argument(() => cineworldRepository).NotNull().Value;
		}

		public KeyValuePair<short, Helpers.Cineworld.Models.Query>? KeyValuePair { get; set; }
		public Models.QueryResults? Results { get; set; }

		public async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
		{
			Guard.Argument(() => KeyValuePair).NotNull();
			Guard.Argument(() => KeyValuePair!.Value!.Key).InRange((short)1, short.MaxValue);
			Guard.Argument(() => KeyValuePair!.Value!.Value).NotNull();

			var id = KeyValuePair!.Value.Key;
			var query = KeyValuePair!.Value.Value;

			var queryResults = await _cineworldRepository.RunQueryAsync(query).ToListAsync();

			Results = new Models.QueryResults(queryResults)
			{
				QueryId = id,
			};

			return ExecutionResult.Next();
		}
	}
}
