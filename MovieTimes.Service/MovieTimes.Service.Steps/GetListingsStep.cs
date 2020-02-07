using Dawn;
using Helpers.Cineworld.Models.Generated;
using System.Collections.Generic;
using System.Threading.Tasks;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace MovieTimes.Service.Steps
{
	public class GetListingsStep : IStepBody
	{
		private readonly Helpers.Cineworld.ICineworldClient _cineworldClient;

		public GetListingsStep(
			Helpers.Cineworld.ICineworldClient cineworldClient)
		{
			_cineworldClient = Guard.Argument(() => cineworldClient).NotNull().Value;
		}

		public ICollection<CinemaType> Cinemas { get; } = new List<CinemaType>();

		public async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
		{
			await foreach (var cinema in _cineworldClient.GetListingsAsync())
			{
				Cinemas.Add(cinema);
			}

			return ExecutionResult.Next();
		}
	}
}
