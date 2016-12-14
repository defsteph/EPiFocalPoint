using System;

using EPiServer.Core;
using EPiServer.PlugIn;

using Newtonsoft.Json;

namespace ImageResizer.Plugins.EPiFocalPoint.SpecializedProperties {
	[PropertyDefinitionTypePlugIn(Description = "A property to edit the focal point of an image", DisplayName = "Focal Point")]
	public class PropertyFocalPoint : PropertyLongString {
		public override PropertyDataType Type => PropertyDataType.Json;
		public override Type PropertyValueType => typeof(FocalPoint);
		public override object Value {
			get {
				var value = base.Value as string;
				return value == null ? null : JsonConvert.DeserializeObject<FocalPoint>(value);
			}
			set {
				if(value is FocalPoint) {
					base.Value = JsonConvert.SerializeObject(value);
				} else {
					base.Value = value;
				}
			}
		}
		public override object SaveData(PropertyDataCollection properties) {
			return LongString;
		}
	}
}