using System;

namespace ImageResizer.Plugins.EPiFocalPoint {
	internal class CropDimensions {
		public int X1 { get; set; }
		public int Y1 { get; set; }
		public int X2 { get; set; }
		public int Y2 { get; set; }
		public override string ToString() {
			return $"{X1},{Y1},{X2},{Y2}";
		}
        public static CropDimensions Parse(IFocalPointData focalPointData, ResizeSettings resizeSettings) {
            var sourceWidth = focalPointData.OriginalWidth ?? 1;
            var sourceHeight = focalPointData.OriginalHeight ?? 1;
            var focalPointY = (int)Math.Round(sourceHeight * (focalPointData.FocalPoint.Y / 100));
            var focalPointX = (int)Math.Round(sourceWidth * (focalPointData.FocalPoint.X / 100));
            var sourceAspectRatio = (double)sourceWidth / sourceHeight;
            double targetAspectRatio = 1.0f;
            if(resizeSettings != null) {
                //Calculate target aspect ratio from resizeSettings.
                if (resizeSettings.Width > 0 && resizeSettings.Height > 0) {
                    targetAspectRatio = (double) resizeSettings.Width / resizeSettings.Height;
                } else {
                    targetAspectRatio = sourceAspectRatio;
                }
            }
            var x1 = 0;
            var y1 = 0;
            int x2;
            int y2;
            if(targetAspectRatio.Equals(sourceAspectRatio)) {
                x2 = focalPointData.OriginalWidth ?? 0;
                y2 = focalPointData.OriginalHeight ?? 0;
            } else if(targetAspectRatio > sourceAspectRatio) {
                // the requested aspect ratio is wider than the source image
                var newHeight = (int)Math.Floor(sourceWidth / targetAspectRatio);
                x2 = sourceWidth;
                y1 = Math.Max(focalPointY - (int)Math.Round((double)newHeight / 2), 0);
                y2 = Math.Min(y1 + newHeight, sourceHeight);
                if(y2 == sourceHeight) {
                    y1 = y2 - newHeight;
                }
            } else {
                // the requested aspect ratio is narrower than the source image
                var newWidth = (int)Math.Round(sourceHeight * targetAspectRatio);
                x1 = Math.Max(focalPointX - (int)Math.Round((double)newWidth / 2), 0);
                x2 = Math.Min(x1 + newWidth, sourceWidth);
                y2 = sourceHeight;
                if(x2 == sourceWidth) {
                    x1 = x2 - newWidth;
                }
            }
            return new CropDimensions { X1 = x1, X2 = x2, Y1 = y1, Y2 = y2 };
        }
	}
}