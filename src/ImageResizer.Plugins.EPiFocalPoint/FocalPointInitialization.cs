using System;

using EPiServer;
using EPiServer.Core;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.Framework.Localization;
using EPiServer.Framework.Localization.XmlResources;
using EPiServer.Logging;

using ImageResizer.Plugins.EPiFocalPoint.Internal.Services;

namespace ImageResizer.Plugins.EPiFocalPoint {
	[InitializableModule, ModuleDependency(typeof(ProviderBasedLocalizationService))]
	public class FocalPointInitialization : IInitializableModule {
		private const string LocalizationProviderName = "FocalPointLocalizations";
		private bool eventsAttached;
		private static readonly ILogger Logger = LogManager.GetLogger();
		private static LocalizationService LocalizationService;
		public void Initialize(InitializationEngine context) {
			LocalizationService = context.Locate.Advanced.GetInstance<LocalizationService>() as ProviderBasedLocalizationService;
			context.InitComplete += InitComplete;
			InitializeEventHooks(context);
		}
		private static void InitComplete(object sender, EventArgs e) {
			InitializeLocalizations();
		}
		private static void InitializeLocalizations() {
			var providerBasedLocalizationService = LocalizationService as ProviderBasedLocalizationService;
			if (providerBasedLocalizationService != null) {
				var localizationProviderInitializer = new EmbeddedXmlLocalizationProviderInitializer();
				var localizationProvider = localizationProviderInitializer.GetInitializedProvider(LocalizationProviderName, typeof(FocalPointInitialization).Assembly);
				providerBasedLocalizationService.InsertProvider(localizationProvider);
			}
		}
		private void InitializeEventHooks(InitializationEngine context) {
			if (!eventsAttached) {
				var contentEvents = context.Locate.Advanced.GetInstance<IContentEvents>();
				contentEvents.CreatingContent += SavingImage;
				contentEvents.SavingContent += SavingImage;
				eventsAttached = true;
			}
		}
		private static void SavingImage(object sender, ContentEventArgs e) {
			var focalPointData = e.Content as IFocalPointData;
			if (focalPointData != null) {
				SetDimensions(focalPointData);
			}
		}
		private static void SetDimensions(IFocalPointData focalPointData) {
			if (!focalPointData.IsReadOnly && focalPointData.BinaryData != null) {
				using (var stream = focalPointData.BinaryData.OpenRead()) {
					try {
						var size = ImageDimensionService.GetDimensions(stream);
						if (size.IsValid) {
							if (focalPointData.OriginalHeight != size.Height) {
								Logger.Information($"Setting height for {focalPointData.Name} to {size.Height}.");
								focalPointData.OriginalHeight = size.Height;
							}
							if (focalPointData.OriginalWidth != size.Width) {
								Logger.Information($"Setting width for {focalPointData.Name} to {size.Width}.");
								focalPointData.OriginalWidth = size.Width;
							}
						} else {
							Logger.Information($"Could not read size of {focalPointData.Name}.");
						}
					} catch (Exception ex) {
						Logger.Error($"Could not read size of {focalPointData.Name}, data might be corrupt.", ex);
					}
				}
			}
		}
		public void Uninitialize(InitializationEngine context) {
			UninitializeLocalizations(context);
			UninitializeEventHooks(context);
		}
		private static void UninitializeLocalizations(InitializationEngine context) {
			var localizationService = context.Locate.Advanced.GetInstance<LocalizationService>() as ProviderBasedLocalizationService;
			try {
				localizationService?.RemoveProvider(LocalizationProviderName);
			} catch (Exception ex) {
				Logger.Error("Error uninitializing localizations.", ex);
			}
		}
		private void UninitializeEventHooks(InitializationEngine context) {
			var contentEvents = context.Locate.Advanced.GetInstance<IContentEvents>();
			contentEvents.CreatingContent -= SavingImage;
			contentEvents.SavingContent -= SavingImage;
			eventsAttached = false;
		}
	}
}