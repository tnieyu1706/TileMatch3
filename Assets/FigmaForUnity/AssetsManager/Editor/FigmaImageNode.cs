using Newtonsoft.Json;

namespace FigmaForUnity.Editor
{
    public class FigmaImageNode
    {
        [JsonProperty("id")]
        public string NodeId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("imageRef")]
        public string ImageRef { get; set; }

        [JsonProperty("pageName")]
        public string PageName { get; set; }
    }
}
