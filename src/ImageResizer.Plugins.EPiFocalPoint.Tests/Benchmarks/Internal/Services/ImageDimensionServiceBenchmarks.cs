using System.Diagnostics;
using System.Drawing;
using System.Linq;

using ImageResizer.Plugins.EPiFocalPoint.Internal.Services;

using JetBrains.dotMemoryUnit;

using Xunit;
using Xunit.Abstractions;

namespace ImageResizer.Plugins.EPiFocalPoint.Tests.Benchmarks.Internal.Services {
	public class ImageDimensionServiceBenchmarks : IClassFixture<BenchmarkFixture> {
		private readonly BenchmarkFixture fixture;
		private readonly ITestOutputHelper testOutputHelper;

		public ImageDimensionServiceBenchmarks(BenchmarkFixture fixture, ITestOutputHelper testOutputHelper) {
			this.fixture = fixture;
			DotMemoryUnitTestOutput.SetOutputMethod(testOutputHelper.WriteLine);
			this.testOutputHelper = testOutputHelper;
		}

		[Fact]
		[DotMemoryUnit(FailIfRunWithoutSupport = false, CollectAllocations = true)]
		public void ImageDimensionService_GetDimensions() {
			var stopWatch = new Stopwatch();
			long memoryUsed = 0;
			stopWatch.Start();
			{
				var memoryCheckPoint1 = dotMemory.Check();
				foreach(var file in fixture.Files) {
					using(var stream = file.OpenRead()) {
						var size = ImageDimensionService.GetDimensions(stream);
					}
				}
				var memoryCheckPoint2 = dotMemory.Check(memory => {
					memoryUsed = memory.GetTrafficFrom(memoryCheckPoint1).AllocatedMemory.SizeInBytes;
				});
			}
			stopWatch.Stop();
			testOutputHelper.WriteLine($"Using {nameof(ImageDimensionService.GetDimensions)} to get size of {fixture.Files.Count()} images took {stopWatch.ElapsedMilliseconds} ms. Memory used was {memoryUsed} bytes.");
			Assert.True(true);
		}

		[Fact]
		[DotMemoryUnit(FailIfRunWithoutSupport = false, CollectAllocations = true)]
		public void ImageFromStream_WithValidation_GetDimensions() {
			var stopWatch = new Stopwatch();
			long memoryUsed = 0;
			stopWatch.Start();
			{
				var memoryCheckPoint1 = dotMemory.Check();
				foreach(var file in this.fixture.Files) {
					using(var stream = file.OpenRead()) {
						var size = Image.FromStream(stream, false);
					}
				}
				var memoryCheckPoint2 = dotMemory.Check(memory => {
					memoryUsed = memory.GetTrafficFrom(memoryCheckPoint1).AllocatedMemory.SizeInBytes;
				});
			}
			stopWatch.Stop();
			testOutputHelper.WriteLine($"Using {nameof(Image.FromStream)} with validation to get size of {fixture.Files.Count()} images took {stopWatch.ElapsedMilliseconds} ms. Memory used was {memoryUsed} bytes.");
			Assert.True(true);
		}

		[Fact]
		[DotMemoryUnit(FailIfRunWithoutSupport = false, CollectAllocations = true)]
		public void ImageFromStream_SkipValidation_GetDimensions() {
			var stopWatch = new Stopwatch();
			long memoryUsed = 0;
			stopWatch.Start();
			{
				var memoryCheckPoint1 = dotMemory.Check();
				foreach(var file in this.fixture.Files) {
					using(var stream = file.OpenRead()) {
						var size = Image.FromStream(stream, false, false);
					}
				}
				var memoryCheckPoint2 = dotMemory.Check(memory => {
					memoryUsed = memory.GetTrafficFrom(memoryCheckPoint1).AllocatedMemory.SizeInBytes;
				});
			}
			stopWatch.Stop();
			testOutputHelper.WriteLine($"Using {nameof(Image.FromStream)} without validation to get size of {fixture.Files.Count()} images took {stopWatch.ElapsedMilliseconds} ms. Memory used was {memoryUsed} bytes.");
			Assert.True(true);
		}
	}
}