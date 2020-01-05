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
            IQueryable<Day> days = weatherContext.Day;
            try
            {
                zip = Convert.ToInt32(req.Query["zip"]);
                Console.WriteLine("Zip parameter:: " + Convert.ToString(zip));
                if (zip != 0)
                {
                    readings = readings.Where(r => r.LocationZipID == zip);
                    days = days.Where(d => d.LocationZipID == zip);
                }
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
            //the -8 hours is a hacky solution because I haven't handled timezones correctly
            days = days.Where(r => r.Date.Date == DateTime.Now.AddHours(-8).Date);
            decimal Temperature = readings.First().Temperature;
            //var readQ = from read in readings


            var dayQ = from day in days
                       select new
                       {
                           Zip = day.LocationZipID,
                           day.Date,
                           Sunrise = day.Sunrise.TimeOfDay,
                           Sunset = day.Sunset.TimeOfDay,
                           Readings =
                           from read in readings.Where(r => r.ReadingDateTime > DateTime.UtcNow.AddHours(-24) && r.LocationZipID == day.LocationZipID)
                           select new
                           {
                               ZipCode = read.LocationZipID,
                               DateTime = read.ReadingDateTime,
                               Temperature = read.Temperature,
                               WindSpeed = read.WindSpeed
                           }
                       };
            Array dayArray = dayQ.ToArray();
            return readings != null ? (ActionResult)new JsonResult(dayArray) : new BadRequestObjectResult("No Zip Provided");
        }
    }
}
