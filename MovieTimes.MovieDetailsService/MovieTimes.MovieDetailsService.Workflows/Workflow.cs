using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace MovieTimes.MovieDetailsService.Workflows
{
	public class Workflow : IWorkflow<Models.PersistenceData>
	{
		public string Id => nameof(Workflow);
		public int Version => 1;

		public void Build(IWorkflowBuilder<Models.PersistenceData> builder)
		{
			builder
				.StartWith(_ => ExecutionResult.Next())

				// Begin infinite loop
				.While(_ => true)
					.Do(workflow => workflow

						// Get movies without format (2D, 3D, IMAX, etc.)
						.StartWith<Steps.GetMoviesMissingMappingStep>()
							.Output(data => data.Movies, step => step.Movies)

						// Get the format, and the sanitized title
						.ForEach(data => data.Movies)
							.Do(each => each
								.StartWith<Steps.TitleSanitizationStep>()
									.Input(step => step.Movie, (data, context) => (Models.PersistenceData.Movie)context.Item)
									.Output(data => data.MovieItem, step => step.Movie)
							)

						// Get the runtime
						.ForEach(data => data.Movies)
							.Do(each => each
								.StartWith<Steps.GetRuntimeStep>()
									.Input(step => step.Movie, (data, context) => (Models.PersistenceData.Movie)context.Item)
									.Output(data => data.MovieItem, step => step.Movie)
							)

						// Save
						.Then<Steps.SaveStep>()
							.Input(step => step.Movies, data => data.Movies)

						// Sleep
						.Then<Steps.SleepStep>()
							.Input(step => step.MillisecondsDelay, _ => 3_600_000)

					// Loop
					);
		}
	}
}
