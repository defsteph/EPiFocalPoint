using Newtonsoft.Json;

namespace ImageResizer.Plugins.EPiFocalPoint.SpecializedProperties {
	public class FocalPoint {
		[JsonProperty("x")]
		public double X { get; set; }
		[JsonProperty("y")]
		public double Y { get; set; }
		public override string ToString() {
			return JsonConvert.SerializeObject(this);
		}
	}
}