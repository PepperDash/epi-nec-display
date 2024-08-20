using Newtonsoft.Json;

namespace PDT.NecDisplay.EPI
{
	public class NecDisplayConfigObject
	{
		/// <summary>
		/// ID of the display control.  Expressed as a byte escaped in a string
		/// </summary>
		/// <example>"\x2A"</example>
		[JsonProperty("id")]
		public string ID { get; set; }
	}
}