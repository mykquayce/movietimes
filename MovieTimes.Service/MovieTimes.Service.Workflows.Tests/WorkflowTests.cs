using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WorkflowCore.Interface;
using WorkflowCore.Models;
using WorkflowCore.Testing;
using Xunit;

namespace MovieTimes.Service.Workflows.Tests
{
	public class WorkflowTests : WorkflowTest<TestWorkflow, TestPersistenceData>
	{
		public WorkflowTests()
		{
			Setup();
		}

		[Fact]
		public void WorkflowTests_IterateOnADictionary()
		{
			// Arrange
			var persistenceData = new TestPersistenceData();
			var workflowId = StartWorkflow(persistenceData);

			// Assert
			Assert.Null(persistenceData.Dictionary);
			Assert.Empty(persistenceData.StringStack);

			// Act
			WaitForWorkflowToComplete(workflowId, TimeSpan.FromSeconds(30));

			// Assert
			Assert.NotNull(persistenceData.Dictionary);
			Assert.NotEmpty(persistenceData.Dictionary);

			Assert.NotNull(persistenceData.StringStack);
			Assert.NotEmpty(persistenceData.StringStack);

			foreach (var value in persistenceData.Dictionary!.Values)
			{
				Assert.Contains(value, persistenceData.StringStack);
			}
		}
	}

	public class TestWorkflow : IWorkflow<TestPersistenceData>
	{
		public string Id { get; } = "MyWorkflow";

		public int Version { get; } = 1;

		public void Build(IWorkflowBuilder<TestPersistenceData> builder)
		{
			builder
				.StartWith<PopulateDictionaryStep>()
					.Output(data => data.Dictionary, step => step.Dictionary)

				.ForEach(data => data.Dictionary)
					.Do(each => each
						.StartWith<GetValueStep<short, string>>()
							.Input(step => step.KeyValuePair, (_, context) => (KeyValuePair<short, string>)context.Item)
							.Output(data => data.Push, step => step.Value)
				)

				.EndWorkflow();
		}
	}

	public class TestPersistenceData
	{
		public IDictionary<short, string>? Dictionary { get; set; }
		public Stack<string> StringStack { get; } = new Stack<string>();
		public string? Push
		{
			get => throw new NotImplementedException();
			set => StringStack.Push(value!);
		}
	}

	public class PopulateDictionaryStep : IStepBody
	{
		public IDictionary<short, string>? Dictionary { get; set; }

		public Task<ExecutionResult> RunAsync(IStepExecutionContext context)
		{
			Dictionary = new Dictionary<short, string>(3)
			{
				[0] = "zero",
				[1] = "one",
				[2] = "two",
			};

			return Task.FromResult(ExecutionResult.Next());
		}
	}

	public class GetValueStep<TKey, TValue> : IStepBody
		where TValue : class
	{
		public KeyValuePair<TKey, TValue>? KeyValuePair { get; set; }
		public TValue? Value { get; set; }

		public Task<ExecutionResult> RunAsync(IStepExecutionContext context)
		{
			Value = KeyValuePair!.Value.Value;

			return Task.FromResult(ExecutionResult.Next());
		}
	}
}
