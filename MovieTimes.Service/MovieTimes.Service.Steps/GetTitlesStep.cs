using Dawn;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace MovieTimes.Service.Steps
{
	public class GetTitlesStep : IStepBody
	{
		public Models.Generated.cinemas? Cinemas { get; set; }
		public IEnumerable<(int edi, string title)>? EdiTitleTuples { get; set; }

		public Task<ExecutionResult> RunAsync(IStepExecutionContext context)
		{
			Guard.Argument(() => Cinemas).NotNull();
			Guard.Argument(() => Cinemas!.cinema).NotNull().NotEmpty();

			EdiTitleTuples = from c in Cinemas!.cinema
							 from f in c.listing
							 group f by (f.edi, f.title) into gg
							 select (gg.Key.edi, gg.Key.title);

			return Task.FromResult(ExecutionResult.Next());
		}
	}
}
