using Moq;
using System;
using System.Globalization;
using System.Threading.Tasks;
using Xunit;

namespace MovieTimes.Service.Steps.Tests
{
	public class GetLatestLogEntryStepTests
	{
		[Theory]
		[InlineData("2019-07-21T12:33:51Z")]
		public async Task GetLatestLogEntryStepTests_BehavesPredictably(string lastModifiedString)
		{
			// Arrange
			var logEntry = new Models.LogEntry
			{
				LastModified = DateTime.Parse(lastModifiedString, styles: DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal),
			};

			var cineworldRepository = Mock.Of<Repositories.ICineworldRepository>(r => r.GetLatestLogEntryAsync() == Task.FromResult(logEntry));

			var context = Mock.Of<WorkflowCore.Models.StepExecutionContext>();

			var sut = new Steps.GetLatestLogEntryStep(cineworldRepository);

			// Act
			await sut.RunAsync(context);

			// Assert
			Assert.Equal(logEntry, sut.LogEntry);
		}
	}
}
