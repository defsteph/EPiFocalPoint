using System.ComponentModel.DataAnnotations;

using EPiServer.Core;
using EPiServer.DataAnnotations;

using ImageResizer.Plugins.EPiFocalPoint.SpecializedProperties;

namespace ImageResizer.Plugins.EPiFocalPoint {
	public abstract class FocalPointImageData : ImageData, IFocalPointData {
		[BackingType(typeof(PropertyFocalPoint))]
		public virtual FocalPoint FocalPoint { get; set; }

		[ScaffoldColumn(false)]
		public virtual int? OriginalWidth { get; set; }

		[ScaffoldColumn(false)]
		public virtual int? OriginalHeight { get; set; }
	}
}