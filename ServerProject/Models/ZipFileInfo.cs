using Newtonsoft.Json;

namespace ServerProject.Models
{
    // incoming message JSON model
    public class ZipFileInfo
    {
        [JsonProperty(Required = Required.Always)]
        public string FileName { get; set; }

        [JsonProperty(Required = Required.Always)]
        public string ContentTree { get; set; }
    }
}
