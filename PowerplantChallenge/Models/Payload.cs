namespace PowerplantChallenge.Models
{
    public class Payload
    {
        public decimal Load { get; set; }
        public Fuel Fuels { get; set; }
        public List<Powerplant> PowerPlants { get; set; }
    }
}
