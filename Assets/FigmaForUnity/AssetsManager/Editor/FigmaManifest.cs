using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace FigmaForUnity.Editor
{
    public class FigmaManifest
    {
        [JsonProperty("figmaFileKey")]
        public string FigmaFileKey { get; set; }

        [JsonProperty("localFolderPath")]
        public string LocalFolderPath { get; set; }

        [JsonProperty("lastSync")]
        public DateTime? LastSync { get; set; }

        [JsonProperty("assets")]
        public Dictionary<string, ManifestAsset> Assets { get; set; } = new Dictionary<string, ManifestAsset>();
    }

    public class ManifestAsset
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("imageRef")]
        public string ImageRef { get; set; }

        [JsonProperty("localPath")]
        public string LocalPath { get; set; }

        [JsonProperty("pageName")]
        public string PageName { get; set; }
    }
}
