using Dawn;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace MovieTimes.Service.Steps
{
	public class SaveTitlesStep : IStepBody
	{
		private readonly Services.IFixTitlesService _fixTitlesService;

		public SaveTitlesStep(
			Services.IFixTitlesService fixTitlesService)
		{
			_fixTitlesService = Guard.Argument(() => fixTitlesService).NotNull().Value;
		}

		public IEnumerable<(int edi, string title, Models.Formats format)>? EdiTitleFormatTuples { get; set; }

		public Task<ExecutionResult> RunAsync(IStepExecutionContext context)
		{
			Guard.Argument(() => EdiTitleFormatTuples).NotNull().NotEmpty();



			throw new NotImplementedException();
		}
	}
}
