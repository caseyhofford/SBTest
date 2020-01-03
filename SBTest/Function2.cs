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
        [FunctionName("GetReadings")]
        public async Task<IActionResult> GetReadings(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            /*var query = (from c in weatherContext.Reading
                         where c.ReadingDateTime > DateTime.Now.AddHours(-24)
                         select c.Temperature);*/
            var reads = weatherContext.Reading;
            Reading firstReading = await reads.FindAsync(102);
            decimal readingtemp = firstReading.Temperature;
            /*foreach (Reading r in readings)
            {
                readingstring += r.Temperature;
            }*/
            log.LogInformation(readingtemp.ToString());
            return new OkObjectResult("OK: "+ readingtemp.ToString());
            /*string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            name = name ?? data?.name;

            return name != null
                ? (ActionResult)new OkObjectResult($"Hello, {name}")
                : new BadRequestObjectResult("Please pass a name on the query string or in the request body");*/
        }
    }
}
