using System.Text.Json.Serialization;

namespace PowerplantChallenge.Models
{
    public class Response
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("p")]
        public decimal Power { get; set; }
    }
}
