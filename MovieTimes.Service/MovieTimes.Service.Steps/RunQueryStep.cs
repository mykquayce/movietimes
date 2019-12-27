using Dawn;
using System;
using System.Collections.Generic;
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
		public ICollection<Helpers.Cineworld.Models.cinemaType> Cinemas { get; } = new List<Helpers.Cineworld.Models.cinemaType>();

		public short QueryId => KeyValuePair!.Value.Key;
		public Helpers.Cineworld.Models.Query Query => KeyValuePair!.Value.Value;

		public async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
		{
			Guard.Argument(() => KeyValuePair).NotNull();
			Guard.Argument(() => QueryId).Positive();
			Guard.Argument(() => Query).NotNull();

			await foreach(var cinema in _cineworldRepository.RunQueryAsync(Query))
			{
				Cinemas.Add(cinema);
			}

			return ExecutionResult.Next();
		}
	}
}
