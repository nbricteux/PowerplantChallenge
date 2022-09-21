using System.Text.Json.Serialization;

namespace PowerplantChallenge.Models
{
    public class Powerplant
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public decimal Efficiency { get; set; }
        public decimal Pmin { get; set; }
        public decimal Pmax { get; set; }

        // Add field to know cost efficiency
        [JsonIgnore]
        public decimal Cost { get; set; }
    }
}
