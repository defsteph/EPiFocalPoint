using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

using EPiServer;
using EPiServer.Core;
using EPiServer.ServiceLocation;
using EPiServer.Web.Routing;

using ImageResizer.Plugins.EPiServer;

namespace ImageResizer.Plugins.EPiFocalPoint
{
    public static class UrlBuilderExtensions
    {
        private const string Width = "w";
        private const string WidthAlt = "width";
        private const string Height = "h";
        private const string HeightAlt = "height";
        private const string Crop = "crop";

        private static bool ContainsKey(this UrlBuilder target, string name) {
            if (target == null) {
                throw new ArgumentNullException(nameof(target));
            }
            return !target.IsEmpty && target.QueryCollection.AllKeys.Contains(name);
        }

        private static UrlBuilder Remove(this UrlBuilder target, string name) {
            if (target.ContainsKey(name)) {
                target.QueryCollection.Remove(name);
            }
            return target;
        }

        private static IFocalPointData GetFocalPointData(this UrlBuilder target) {
            if (target == null || target.IsEmpty) {
                throw new ArgumentNullException(nameof(target));
            }

            var content = UrlResolver.Current.Route(new UrlBuilder(target.ToString()));
            if (content == null) {
                throw new ArgumentNullException(nameof(target));
            }

            if (ContentReference.IsNullOrEmpty(content.ContentLink)) { // should not be needed, added for extra security
                throw new ArgumentNullException(nameof(target));
            }

            ImageData imageData;
            try {
                imageData = ServiceLocator.Current.GetInstance<IContentLoader>().Get<ImageData>(content.ContentLink);
            }
            catch (TypeMismatchException) {
                throw new ArgumentNullException(nameof(target));
            }
            return imageData as IFocalPointData;
        }

        /// <summary>
        /// Generate image with focal point
        /// </summary>
        /// <param name="target">End of UrlBuilder</param>
        /// <example>&gt;img src="@Html.ResizeImage(Model.CurrentPage.Image).Width(500).Height(200).FitMode(FitMode.Crop).UseFocalPoint()" /&lt;</example>
        /// <returns></returns>
        public static string UseFocalPoint(this UrlBuilder target) {
            var focalPointData = target.GetFocalPointData();
            var resizeSettings = new ResizeSettings(target.QueryCollection);

            if (focalPointData?.FocalPoint != null && focalPointData.ShouldApplyFocalPoint(resizeSettings)) {
                target.Add(Crop, CropDimensions.Parse(focalPointData, resizeSettings).ToString());
            }
            return target.ToString();
        }

        /// <summary>
        /// Generate responsive image with focal point
        /// </summary>
        /// <param name="target">End of UrlBuilder</param>
        /// <param name="defaultWidth">Default image size</param>
        /// <param name="htmlAttributes">Html attributes: new {@class="class-name"}</param>
        /// <param name="widthSizes">Different dimensions for selecting closest size"</param>
        /// <example>@Html.ResizeImage(Model.CurrentPage.Image).FitMode(FitMode.Crop).UseFocalPoint(800, null, 1600, 1200, 1000, 800)</example>
        /// <returns>MvcHtmlString</returns>
        public static MvcHtmlString UseFocalPoint(this UrlBuilder target, int defaultWidth, object htmlAttributes, params int[] widthSizes) {
            // Idea borrowed from https://www.creuna.com/fi/blogit/responsive-images-with-episerver-and-imageresizer/
            target.Remove(Height).Remove(HeightAlt).Remove(Width).Remove(WidthAlt);

            var focalPointData = target.GetFocalPointData();
            var originalImageWidth = focalPointData?.OriginalWidth ?? 1;
            var originalImageHeight = focalPointData?.OriginalHeight ?? 1;

            var scaleFactor = defaultWidth / originalImageWidth;
            var imageHeight = (int) Math.Round((double) originalImageHeight * scaleFactor);

            var urlBuilder = new UrlBuilder(target)
                .Add(Width, $"{defaultWidth}")
                .Add(Height, $"{imageHeight}");
            var resizeSettings = new ResizeSettings(urlBuilder.QueryCollection);

            if (focalPointData?.FocalPoint != null && focalPointData.ShouldApplyFocalPoint(resizeSettings)) {
                urlBuilder.Add(Crop, CropDimensions.Parse(focalPointData, resizeSettings).ToString());
            }

            var tagBuilder = new TagBuilder("img");
            tagBuilder.MergeAttributes(HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes));
            tagBuilder.MergeAttribute("src", urlBuilder.ToString(), true);
            
            if (widthSizes != null) {
                var srcSet = new List<string>();
                var pushOriginal = true;

                foreach (var width in widthSizes) {
                    if (width > originalImageWidth) {
                        continue;
                    }

                    scaleFactor = width / originalImageWidth;
                    imageHeight = (int) Math.Round((double) originalImageHeight * scaleFactor);

                    urlBuilder = new UrlBuilder(target)
                        .Add(Width, $"{width}")
                        .Add(Height, $"{imageHeight}");
                    resizeSettings = new ResizeSettings(urlBuilder.QueryCollection);

                    if (focalPointData?.FocalPoint != null && focalPointData.ShouldApplyFocalPoint(resizeSettings)) {
                        urlBuilder.Add(Crop, CropDimensions.Parse(focalPointData, resizeSettings).ToString());
                    }
                    srcSet.Add($"{urlBuilder} {width}{Width}");

                    if (originalImageWidth == width) {
                        pushOriginal = false;
                    }
                }

                if (pushOriginal) {
                    urlBuilder = new UrlBuilder(target)
                        .Add(Width, $"{originalImageWidth}")
                        .Add(Height, $"{originalImageHeight}");

                    resizeSettings = new ResizeSettings(urlBuilder.QueryCollection);
                    if (focalPointData?.FocalPoint != null && focalPointData.ShouldApplyFocalPoint(resizeSettings)) {
                        urlBuilder.Add(Crop, CropDimensions.Parse(focalPointData, resizeSettings).ToString());
                    }
                    srcSet.Add($"{urlBuilder} {originalImageWidth}{Width}");
                }

                tagBuilder.MergeAttribute("srcset", string.Join(",\n", srcSet.ToArray()), true);
                tagBuilder.MergeAttribute("sizes", $"(min-width: {defaultWidth}px) {defaultWidth}px, 100vw", true);
            }
            
            return new MvcHtmlString(tagBuilder.ToString(TagRenderMode.SelfClosing));
        }
    }
}