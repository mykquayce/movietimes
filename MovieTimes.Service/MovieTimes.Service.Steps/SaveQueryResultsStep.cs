using Dawn;
using Helpers.Common;
using Helpers.Tracing;
using MovieTimes.Service.Services;
using OpenTracing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace MovieTimes.Service.Steps
{
	public class SaveQueryResultsStep : IStepBody
	{
		private readonly Repositories.IQueriesRepository _queriesRepository;
		private readonly ISerializationService _serializationService;
		private readonly ITracer? _tracer;

		public SaveQueryResultsStep(
			Repositories.IQueriesRepository queriesRepository,
			Services.ISerializationService serializationService,
			ITracer? tracer)
		{
			_queriesRepository = Guard.Argument(() => queriesRepository).NotNull().Value;
			_serializationService = Guard.Argument(() => serializationService).NotNull().Value;
			_tracer = tracer;
		}

		public Models.QueryResults? QueryResults { get; set; }

		public async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
		{
			using var scope = _tracer?.BuildDefaultSpan()
				.WithTag("Count", QueryResults?.Count.ToString())
				.StartActive();

			Guard.Argument(() => QueryResults!).NotNull();

			if (string.IsNullOrWhiteSpace(QueryResults!.Json))
			{
				using var stream = await _serializationService.SerializeAsync(from r in QueryResults
																			  orderby r.CinemaId, r.ShowDateTime, r.FilmTitle
																			  select r);

				using var reader = new StreamReader(stream);

				QueryResults.Json = await reader.ReadToEndAsync();
			}

			if ((QueryResults.Checksum ?? 0) == 0)
			{
				QueryResults.Checksum = QueryResults.Json.GetDeterministicHashCode();
			}

			if (QueryResults!.Any())
			{
				await _queriesRepository.SaveQueryResultsAsync(QueryResults!);
			}

			return ExecutionResult.Next();
		}
	}
}
