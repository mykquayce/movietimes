using Dawn;
using Helpers.Tracing;
using OpenTracing;
using System.Threading.Tasks;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace MovieTimes.Service.Steps
{
	public class SendMessageToDiscordStep : IStepBody
	{
		private readonly Helpers.Discord.IDiscordClient _discordClient;
		private readonly ITracer _tracer;

		public SendMessageToDiscordStep(
			Helpers.Discord.IDiscordClient discordClient,
			ITracer tracer)
		{
			_discordClient = Guard.Argument(() => discordClient).NotNull().Value;
			_tracer = tracer;
		}

		public string? Message { get; set; }

		public async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
		{
			_tracer?.BuildDefaultSpan()
				.WithTag(nameof(Message), Message)
				.StartActive();

			if (string.IsNullOrWhiteSpace(Message))
			{
				return ExecutionResult.Next();
			}

			await _discordClient.SendMessageAsync(Message!);

			return ExecutionResult.Next();
		}
	}
}
