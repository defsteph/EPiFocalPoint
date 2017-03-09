using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Text;
using System.Web;

using EPiServer.Core;
using EPiServer.Framework.Cache;
using EPiServer.Logging;
using EPiServer.ServiceLocation;
using EPiServer.Web.Routing;

using ImageResizer.Configuration;
using ImageResizer.Configuration.Xml;

namespace ImageResizer.Plugins.EPiFocalPoint {
	public class EPiFocalPointPlugin : IPlugin {
		private readonly IContentCacheKeyCreator contentCacheKeyCreator;
		private readonly ISynchronizedObjectInstanceCache cache;
		private static readonly ILogger Logger = LogManager.GetLogger();
		private readonly Dictionary<string, ResizeSettings> defaults = new Dictionary<string, ResizeSettings>(StringComparer.OrdinalIgnoreCase);
		private readonly Dictionary<string, ResizeSettings> settings = new Dictionary<string, ResizeSettings>(StringComparer.OrdinalIgnoreCase);
		private bool onlyAllowPresets;
		public EPiFocalPointPlugin() : this(ServiceLocator.Current.GetInstance<IContentCacheKeyCreator>(), ServiceLocator.Current.GetInstance<ISynchronizedObjectInstanceCache>()) { }

		[Obsolete("The UrlResolver is no longer used.")]
		public EPiFocalPointPlugin(UrlResolver urlResolver, IContentCacheKeyCreator contentCacheKeyCreator, ISynchronizedObjectInstanceCache cache) {
			this.contentCacheKeyCreator = contentCacheKeyCreator;
			this.cache = cache;
		}
		public EPiFocalPointPlugin(IContentCacheKeyCreator contentCacheKeyCreator, ISynchronizedObjectInstanceCache cache) {
			this.contentCacheKeyCreator = contentCacheKeyCreator;
			this.cache = cache;
		}
		public IPlugin Install(Config c) {
			c.Plugins.add_plugin(this);
			ParseXml(c.getConfigXml().queryFirst("presets"));
			c.Pipeline.RewriteDefaults += PipelineRewriteDefaults;
			return this;
		}
		protected void ParseXml(Node presetConfigNode) {
			if(presetConfigNode?.Children == null) {
				return;
			}
			onlyAllowPresets = GetBoolFromString(presetConfigNode.Attrs["onlyAllowPresets"]);
			foreach(var presetNode in presetConfigNode.Children) {
				var name = presetNode.Attrs["name"];
				if(presetNode.Name.Equals("preset", StringComparison.OrdinalIgnoreCase)) {
					var presetDefaults = presetNode.Attrs["defaults"];
					if(!string.IsNullOrEmpty(presetDefaults)) {
						defaults[name] = new ResizeSettings(presetDefaults);
					}
					var presetSettings = presetNode.Attrs["settings"];
					if(!string.IsNullOrEmpty(presetSettings)) {
						settings[name] = new ResizeSettings(presetSettings);
					}
				}
			}
		}
		private static bool GetBoolFromString(string attributeValue) {
			return !string.IsNullOrWhiteSpace(attributeValue) && bool.Parse(attributeValue);
		}
		private void PipelineRewriteDefaults(IHttpModule sender, HttpContext context, IUrlEventArgs e) {
			ApplyFocalPointCropping(e);
		}
		private void ApplyFocalPointCropping(IUrlEventArgs urlEventArgs) {
#if DEBUG
			var stopWatch = new Stopwatch();
			stopWatch.Start();
#endif
			try {
				var resizeSettings = GetResizeSettingsFromQueryString(urlEventArgs.QueryString);
				if(resizeSettings == null) {
					return;
				}
				var cacheKey = GetCacheKeyForUrl(urlEventArgs, resizeSettings);
				var cropParameters = this.cache.Get(cacheKey) as string;
				if(cropParameters == null) {
					Logger.Debug($"Crop parameters not found in cache for '{urlEventArgs.VirtualPath}'.");
					var currentContent = ServiceLocator.Current.GetInstance<ContentRouteHelper>().Content;
					if(currentContent != null) {
						var evictionPolicy = GetEvictionPolicy(currentContent.ContentLink);
						var focalPointData = currentContent as IFocalPointData;
						if(focalPointData?.FocalPoint != null && ShouldApplyFocalPoint(focalPointData, resizeSettings)) {
							Logger.Debug($"Altering resize parameters for {focalPointData.Name} based on focal point.");
							cropParameters = GetCropDimensions(focalPointData, resizeSettings).ToString();
							this.cache.Insert(cacheKey, cropParameters, evictionPolicy);
						} else {
							Logger.Debug($"No focal point set for '{currentContent.Name}'.");
							this.cache.Insert(cacheKey, string.Empty, evictionPolicy);
						}
					}
				}
				if(!string.IsNullOrWhiteSpace(cropParameters)) {
					urlEventArgs.QueryString.Add("crop", cropParameters);
				}
			} catch(Exception ex) {
				Logger.Critical("A critical error occured when trying to get focal point data.", ex);
			}
#if DEBUG
			stopWatch.Stop();
			Logger.Debug($"{nameof(ApplyFocalPointCropping)} for {urlEventArgs.VirtualPath} took {stopWatch.ElapsedMilliseconds}ms.");
#endif
		}
		private ResizeSettings GetResizeSettingsFromQueryString(NameValueCollection queryString) {
			var preset = queryString["preset"];
			if(HasPreset(preset)) {
				return this.settings.ContainsKey(preset) && this.settings[preset] != null ? this.settings[preset] : this.defaults[preset];
			}
			return this.onlyAllowPresets ? null : new ResizeSettings(queryString);
		}
		private bool HasPreset(string preset) {
			return !string.IsNullOrWhiteSpace(preset) && (defaults.ContainsKey(preset) || settings.ContainsKey(preset));
		}
		private static string GetCacheKeyForUrl(IUrlEventArgs urlEventArgs, ResizeSettings resizeSettings) {
			var keyBuilder = new StringBuilder();
			keyBuilder.Append("focalpoint:");
			keyBuilder.Append(urlEventArgs.VirtualPath);
			keyBuilder.Append(":");
			foreach(var key in resizeSettings.AllKeys) {
				keyBuilder.AppendFormat("{0}:{1}", key, resizeSettings[key]);
			}
			return keyBuilder.ToString();
		}
		private CacheEvictionPolicy GetEvictionPolicy(ContentReference contentLink) {
			return new CacheEvictionPolicy(new[] { contentCacheKeyCreator.CreateCommonCacheKey(contentLink) });
		}
		private static bool ShouldApplyFocalPoint(IFocalPointData focalPointData, ResizeSettings resizeSettings) {
			if(resizeSettings == null || resizeSettings.Count <= 0) {
				return false;
			}
			if(resizeSettings.Mode == FitMode.Max) { // If using fitmode Max, the image won't be cropped, only resized, and focal point serves no purpose.
				return false;
			}
			var targetWidthIsLargerThanOriginal = resizeSettings.Width >= (focalPointData.OriginalWidth ?? 1);
			var targetHeightIsLargerThanOriginal = resizeSettings.Height >= (focalPointData.OriginalHeight ?? 1);
			if(targetWidthIsLargerThanOriginal && targetHeightIsLargerThanOriginal) { // If the target is bigger in both dimensions, it will result in a scaling operation, and the focal point serves no purpose.
				return false;
			}
			return true;
		}
		private static CropDimensions GetCropDimensions(IFocalPointData focalPointData, ResizeSettings resizeSettings) {
			var sourceWidth = focalPointData.OriginalWidth ?? 1;
			var sourceHeight = focalPointData.OriginalHeight ?? 1;
			var focalPointY = (int)Math.Round(sourceHeight * (focalPointData.FocalPoint.Y / 100));
			var focalPointX = (int)Math.Round(sourceWidth * (focalPointData.FocalPoint.X / 100));
			double targetAspectRatio = 1.0f;
			if(resizeSettings != null) {
				//Calculate target aspect ratio from resizeSettings.
				targetAspectRatio = (double)resizeSettings.Width / resizeSettings.Height;
			}
			var sourceAspectRatio = (double)sourceWidth / sourceHeight;
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
		public bool Uninstall(Config c) {
			c.Plugins.remove_plugin(this);
			c.Pipeline.RewriteDefaults -= PipelineRewriteDefaults;
			return true;
		}
	}
}