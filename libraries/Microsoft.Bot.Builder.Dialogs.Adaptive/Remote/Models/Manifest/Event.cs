using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Remote.Models.Manifest
{
    public class Event
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }
    }
}
