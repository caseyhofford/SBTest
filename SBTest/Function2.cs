using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using WeatherEF;

namespace SBTest
{
    public class HttpTrigger
    {
        private readonly WeatherContext weatherContext;
        public HttpTrigger(WeatherContext weatherContext)
        {
            Console.WriteLine("Context Set");
            this.weatherContext = weatherContext;
        }

        [FunctionName("GetReading")]
        public async Task<IActionResult> GetReading(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            var reads = weatherContext.Reading;
            int id = Convert.ToInt32(req.Query["id"]);
            Reading reading = await reads.FindAsync(id);
            var readObj = new {
                    ZipCode = reading.LocationZipID,
                    DateTime = reading.ReadingDateTime,
                    Temperature = reading.Temperature,
                    WindSpeed = reading.WindSpeed
                };
            string jsonString = JsonConvert.SerializeObject(readObj);
            return reading != null ? (ActionResult)new OkObjectResult(jsonString) : new BadRequestObjectResult("No Valid ID Provided");
        }
    }
}
