namespace ImageResizer.Plugins.EPiFocalPoint
{
    internal static class FocalPointDataExtensions
    {
        public static bool ShouldApplyFocalPoint(this IFocalPointData focalPointData, ResizeSettings resizeSettings) {
            if(resizeSettings == null || resizeSettings.Count <= 0) {
                return false;
            }
            if(resizeSettings.Mode == FitMode.Max) { // If using fitmode Max, the image won't be cropped, only resized, and focal point serves no purpose.
                return false;
            }
            if(resizeSettings.Width < 0 && resizeSettings.Height < 0) {
                return false;
            }
            var targetWidthIsLargerThanOriginal = resizeSettings.Width >= (focalPointData.OriginalWidth ?? 1);
            var targetHeightIsLargerThanOriginal = resizeSettings.Height >= (focalPointData.OriginalHeight ?? 1);
            if(targetWidthIsLargerThanOriginal && targetHeightIsLargerThanOriginal) { // If the target is bigger in both dimensions, it will result in a scaling operation, and the focal point serves no purpose.
                return false;
            }
            return true;
        }
    }
}