using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.Data.SqlClient;
using Newtonsoft.Json;
using RestSharp;
using WeatherEF;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace SBTest
{
    public class GetDay
    {
        private readonly WeatherContext weatherContext;
        public GetDay(WeatherContext weatherContext)
        {
            Console.WriteLine("Context Set");
            this.weatherContext = weatherContext;
        }
        [FunctionName("GetDay")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");
            int zip = 0;
            IQueryable<Reading> readings = weatherContext.Reading;
            try
            {
                zip = Convert.ToInt32(req.Query["zip"]);
                Console.WriteLine("Zip parameter:: "+Convert.ToString(zip));
                readings = readings.Where(r => r.LocationZipID == zip);
                //readings = readings.Where(r => r.LocationZipID == zip);
            }
            catch (FormatException)
            {
                return new BadRequestObjectResult("Check Parameters");
            }
            catch (Exception ex)
            {
                throw ex;
            }
            readings = readings.Where(r => r.ReadingDateTime > DateTime.UtcNow.AddHours(-24));
            decimal Temperature = readings.First().Temperature;
            var query = from read in readings
                        select new
                        {
                            ZipCode = read.LocationZipID,
                            DateTime = read.ReadingDateTime,
                            Temperature = read.Temperature,
                            WindSpeed = read.WindSpeed
                        };
            string jsonString = JsonConvert.SerializeObject(query.ToArray());
            return readings != null ? (ActionResult)new OkObjectResult(jsonString) : new BadRequestObjectResult("No Zip Provided");
        }
    }
}
