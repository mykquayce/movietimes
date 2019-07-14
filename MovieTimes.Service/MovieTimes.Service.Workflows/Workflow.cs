using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace MovieTimes.Service.Workflows
{
	public class Workflow : IWorkflow<Models.PersistenceData>
	{
		public string Id => nameof(Workflow);
		public int Version => 1;

		public void Build(IWorkflowBuilder<Models.PersistenceData> builder)
		{
			builder
				.StartWith(_ => ExecutionResult.Next())
				.EndWorkflow();
		}
	}
}
