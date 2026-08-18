using Newtonsoft.Json;
using System.Collections.Generic;

namespace PDT.NecDisplay.EPI
{
	public class NecDisplayConfigObject
	{
		/// <summary>
		/// ID of the display control.  Expressed as a byte escaped in a string
		/// </summary>
		/// <example>"\x2A"</example>
		[JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
		public string Id { get; set; }

        [JsonProperty("warmupTime", NullValueHandling = NullValueHandling.Ignore)]
        public uint? WarmupTime { get; set; }

        [JsonProperty("cooldownTime", NullValueHandling = NullValueHandling.Ignore)]
        public uint? CooldownTime { get; set; }

        [JsonProperty("friendlyNames")]
        public List<FriendlyName> FriendlyNames { get; set; }

        public NecDisplayConfigObject()
            {
            FriendlyNames = new List<FriendlyName>();
            }
        }
    public class FriendlyName
        {
        [JsonProperty("inputKey")]
        public string InputKey { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("hideInput")]
        public bool HideInput { get; set; }
        }

}

