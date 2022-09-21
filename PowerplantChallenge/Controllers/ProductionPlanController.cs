using Microsoft.AspNetCore.Mvc;
using PowerplantChallenge.Models;
using System.Text.Json;

namespace PowerplantChallenge.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProductionPlanController : ControllerBase
    {
        private readonly ILogger<ProductionPlanController> _logger;
        private Payload payload = new();
         
        public ProductionPlanController(ILogger<ProductionPlanController> logger)
        {
            _logger = logger;
        }

        [HttpPost(Name = "productionplan")]
        public string Post(Payload input)
        {
            if (ModelState.IsValid)
            {
                _logger.LogInformation("Model validated.");
                payload = input;
                CalculateCosts();
                return Process();
            }
            else
            {
                _logger.LogError("Invalid model detected. Please try again.");
                return "Invalid model detected. Please try again.";
            }
        }

        private void CalculateCosts()
        {
            foreach (Powerplant p in payload.PowerPlants)
            {
                switch(p.Type)
                {
                    case "gasfired":
                        p.Cost = (payload.Fuels.Gas + (decimal)0.3 * payload.Fuels.CO2) / p.Efficiency;
                        break;
                    case "turbojet":
                        p.Cost = payload.Fuels.Kerosine / p.Efficiency; 
                        break;
                    case "windturbine":
                        p.Cost = 0;
                        break;
                }
            }
        }
        private string Process()
        {
            var lstResults = new List<Response>();
            decimal load = 0;
            decimal i = 0;

            foreach (Powerplant p in payload.PowerPlants.OrderByDescending(y => y.Pmax).OrderBy(x => x.Cost))
            {
                _logger.LogInformation(String.Format("Powerplan - Name:{0} Type:{1} Eff:{2} Pmin:{3} Pmax:{4} Cost:{5}", p.Name, p.Type, p.Efficiency, p.Pmin, p.Pmax, p.Cost));

                i = 0;
                
                if (load < payload.Load)
                {
                    var diff = payload.Load - load;
                    _logger.LogInformation(String.Format("diff {0} = payload.Load {1} - load {2}", diff, payload.Load, load));

                    if (p.Type == "gasfired")
                    {
                        if (p.Pmin <= diff)
                            i = p.Pmax <= diff ? p.Pmax : diff;
                        // backward compatibility -1 in case pmin is too high
                        else
                        {
                            lstResults.Last().Power -= p.Pmin;
                            load -= p.Pmin;
                            i = p.Pmax <= diff ? p.Pmax : p.Pmin + diff;
                        }
                    }
                    else
                    {
                        i = p.Pmax <= diff ? p.Pmax : diff;
                    }

                    if (p.Type == "windturbine" && p.Pmax <= diff)
                        i = (p.Pmax * payload.Fuels.Wind / 100);

                }
                else
                    i = 0;
                
                Math.Round(i, 1);
                load += i;

                _logger.LogInformation(String.Format("Powerplant added - Name:{0} Power:{1}", p.Name, i));
                lstResults.Add(new Response { Name = p.Name, Power = i });
            }

            _logger.LogInformation(String.Format("LOAD:{0}", load));

            if (load > payload.Load)
                _logger.LogCritical(String.Format("Load too high! Failure detected => load = {0}", load));

            return JsonSerializer.Serialize(lstResults, new JsonSerializerOptions { WriteIndented = true });
        }
    }
}
