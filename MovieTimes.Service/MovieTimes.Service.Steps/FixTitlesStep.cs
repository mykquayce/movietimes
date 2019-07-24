using Dawn;
using System.Collections.Generic;
using System.Threading.Tasks;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace MovieTimes.Service.Steps
{
	public class FixTitlesStep : IStepBody
	{
		private readonly Services.IFixTitlesService _fixTitlesService;

		public FixTitlesStep(
			Services.IFixTitlesService fixTitlesService)
		{
			_fixTitlesService = Guard.Argument(() => fixTitlesService).NotNull().Value;
		}

		public IEnumerable<(int edi, string title)>? EdiTitleTuples { get; set; }
		public ICollection<(int edi, string title, Models.Formats format)> EdiTitleFormatTuples { get; } = new List<(int edi, string title, Models.Formats format)>();

		public Task<ExecutionResult> RunAsync(IStepExecutionContext context)
		{
			foreach (var (edi, title) in EdiTitleTuples!)
			{
				var (sanitized, format) = _fixTitlesService.Sanitize(title);

				EdiTitleFormatTuples.Add((edi, sanitized, format));
			}

			return Task.FromResult(ExecutionResult.Next());
		}
	}
}
