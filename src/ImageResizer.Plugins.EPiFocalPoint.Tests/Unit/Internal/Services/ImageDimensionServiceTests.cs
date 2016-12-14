using System;
using System.Drawing;
using System.IO;
using ImageResizer.Plugins.EPiFocalPoint.Internal.Services;
using Shouldly;
using Xunit;

namespace ImageResizer.Plugins.EPiFocalPoint.Tests.Unit.Internal.Services {
	public class ImageDimensionServiceTests {

		[Fact]
		public void GetDimensions_StreamIsNull_ThrowsException() {
			Action action = () => ImageDimensionService.GetDimensions(null);
			action.ShouldThrow<ArgumentNullException>();
		}

		[Fact]
		public void GetDimensions_StreamCannotRead_ThrowsException() {
			Action action = () => ImageDimensionService.GetDimensions(Stream.Null);
			action.ShouldThrow<ArgumentNullException>();
		}

		[Fact]
		public void GetDimensions_StreamContainsNonImageFile_ReturnsSizeEmpty() {
			using(var stream = File.OpenRead("..\\..\\TestData\\nonimage.pdf")) {
				var size = ImageDimensionService.GetDimensions(stream);

				size.IsValid.ShouldBe(false);
			}
		}

		[Theory]
		[InlineData("bmp")]
		[InlineData("gif")]
		[InlineData("jpg")]
		[InlineData("png")]
		[InlineData("tif")]
		public void GetDimensions_StreamContainsFileOfType_ReturnsCorrectSize(string extension) {
			using(var stream = File.OpenRead($"..\\..\\TestData\\FileTypes\\TestImage.{extension}")) {
				var size = ImageDimensionService.GetDimensions(stream);

				size.IsValid.ShouldBe(true);
				size.Width.ShouldBe(500);
				size.Height.ShouldBe(500);
			}
		}

		[Theory]
		[InlineData("1920x1280")]
		[InlineData("2880x1920")]
		[InlineData("4928x3264")]
		[InlineData("5066x3266")]
		[InlineData("5456x3632")]
		[InlineData("5472x3648")]
		public void GetDimensions_StreamContainsJpeg_ReturnsCorrectSize(string fileNamePart) {
			var dimensions = fileNamePart.Split(new[] { 'x' }, StringSplitOptions.RemoveEmptyEntries);
			var expectedWidth = int.Parse(dimensions[0]);
			var expectedHeight = int.Parse(dimensions[1]);
			using(var stream = File.OpenRead($"..\\..\\TestData\\Dimensions\\{fileNamePart}.jpg")) {
				var size = ImageDimensionService.GetDimensions(stream);
				size.IsValid.ShouldBe(true);
				size.Width.ShouldBe(expectedWidth);
				size.Height.ShouldBe(expectedHeight);
			}
		}
	}
}