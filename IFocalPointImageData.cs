namespace ImageResizer.Plugins.EPiFocalPoint
{
    using EPiServer.Core;
    using EPiServer.Data.Entity;

    using ImageResizer.Plugins.EPiFocalPoint.SpecializedProperties;

    public interface IFocalPointImageData : IContentImage, IReadOnly
    {
        FocalPoint FocalPoint { get; set; }

        int? OriginalWidth { get; set; }

        int? OriginalHeight { get; set; }
    }
}