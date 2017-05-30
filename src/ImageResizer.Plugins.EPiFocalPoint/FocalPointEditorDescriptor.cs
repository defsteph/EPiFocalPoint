using EPiServer.Shell.ObjectEditing.EditorDescriptors;

using ImageResizer.Plugins.EPiFocalPoint.SpecializedProperties;

namespace ImageResizer.Plugins.EPiFocalPoint {
	[EditorDescriptorRegistration(TargetType = typeof(FocalPoint))]
	public class FocalPointEditorDescriptor : EditorDescriptor {
		public FocalPointEditorDescriptor() {
			ClientEditingClass = "focal-point/editor";
		}
	}
}