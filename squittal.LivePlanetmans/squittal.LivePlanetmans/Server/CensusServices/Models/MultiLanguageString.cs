using Newtonsoft.Json;

namespace squittal.LivePlanetmans.Server.CensusServices.Models
{
    public class MultiLanguageString
    {
        [JsonProperty("en")]
        public string English { get; set; }
    }
}
