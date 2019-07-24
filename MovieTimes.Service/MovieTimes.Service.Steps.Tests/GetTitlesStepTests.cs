using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace MovieTimes.Service.Steps.Tests
{
	public class GetTitlesStepTests
	{
		private readonly Steps.GetTitlesStep _step;

		public GetTitlesStepTests()
		{
			_step = new Steps.GetTitlesStep();
		}

		[Fact]
		public async Task GetTitlesStepTests_BehavesPredictably()
		{
			// Arrange
			var step = new Steps.GetTitlesStep();

			step.Cinemas = new Models.Generated.cinemas
			{
				cinema = new List<Models.Generated.cinema>
				{
					new Models.Generated.cinema
					{
						listing = new List<Models.Generated.film>
						{
							new Models.Generated.film { edi = 1, title = "cinema0_film0", },
							new Models.Generated.film { edi = 2, title = "cinema0_film1", },
							new Models.Generated.film { edi = 3, title = "cinema0_film2", },
							new Models.Generated.film { edi = 4, title = "cinema0_film3", },
						},
					},
					new Models.Generated.cinema
					{
						listing = new List<Models.Generated.film>
						{
							new Models.Generated.film { edi = 5, title = "cinema1_film0", },
							new Models.Generated.film { edi = 6, title = "cinema1_film1", },
							new Models.Generated.film { edi = 7, title = "cinema1_film2", },
						},
					},
				},
			};

			var context = Mock.Of<WorkflowCore.Models.StepExecutionContext>();

			// Act
			await step.RunAsync(context);

			var actual = step.EdiTitleTuples?.ToList();

			// Assert
			Assert.NotNull(actual);
			Assert.NotEmpty(actual);
			Assert.Equal(7, actual!.Count);
			Assert.Equal(1, actual![0].edi);
			Assert.Equal(2, actual![1].edi);
			Assert.Equal(3, actual![2].edi);
			Assert.Equal(4, actual![3].edi);
			Assert.Equal(5, actual![4].edi);
			Assert.Equal(6, actual![5].edi);
			Assert.Equal(7, actual![6].edi);
			Assert.Equal("cinema0_film0", actual![0].title);
			Assert.Equal("cinema0_film1", actual![1].title);
			Assert.Equal("cinema0_film2", actual![2].title);
			Assert.Equal("cinema0_film3", actual![3].title);
			Assert.Equal("cinema1_film0", actual![4].title);
			Assert.Equal("cinema1_film1", actual![5].title);
			Assert.Equal("cinema1_film2", actual![6].title);
		}
	}
}
