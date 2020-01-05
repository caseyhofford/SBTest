using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RestSharp;
using WeatherEF;
using System.Linq;

namespace SBTest
{
    public class UpdateDatabase
    {
        private readonly WeatherContext weatherContext;
        public UpdateDatabase(WeatherContext weatherContext)
        {
            Console.WriteLine("Context Set");
            this.weatherContext = weatherContext;
        }
        [FunctionName("UpdateDatabase")]
        public async Task Run([TimerTrigger("0 0/10 * * * *")]TimerInfo myTimer, ILogger log)
        {
            var allLocations = await weatherContext.Location.ToListAsync();
            // Get the connection string from app settings and use it to create a connection.
            foreach (Location current in allLocations)
            {
                Console.WriteLine(current.Zip);
                int zipcode = current.Zip;
                dynamic weatherData = GetWeather(Convert.ToString(zipcode));
                log.LogInformation($"Found Weather:\n{weatherData}");
                //Convert datetime to YYYY-MM-DD hh:mm:ss
                DateTime epoch = new System.DateTime(1970, 1, 1, 0, 0, 0, 0);
                DateTime readDT = epoch.AddSeconds(Convert.ToDouble(weatherData.dt));

                //Set Weather Type
                dynamic weatherType = weatherData.weather[0];
                AddWeatherType(weatherType);

                //Set Day Relationship
                int dayID = GetDayID(Convert.ToString(zipcode), Convert.ToDouble(weatherData.dt), Convert.ToDouble(weatherData.timezone), weatherData.sys);

                weatherContext.Reading.Add(
                    new Reading
                    {
                        LocationZipID = zipcode,
                        ReadingDateTime = readDT,
                        WeatherTypeID = weatherType.id,
                        WindSpeed = Convert.ToDecimal(weatherData.wind.speed),
                        WindDirection = Convert.ToInt16(weatherData.wind.deg ?? null),
                        WindGust = Convert.ToDecimal(weatherData.wind.gust ?? null),
                        Temperature = Convert.ToDecimal(weatherData.main.temp),
                        Clouds = Convert.ToInt16(weatherData.clouds.all),
                        DayID = dayID
                    }
                );
                Console.WriteLine("New reading created");

            }
            weatherContext.SaveChanges();
        }
        //Takes in a ZipCode as a string and returns weather data from the API as an object
        public static dynamic GetWeather(string zip)
        {
            string token = "8358673f01a549b46b006ef9858d324e";

            var client = new RestClient($"https://api.openweathermap.org/data/2.5/weather?appid={token}&units=imperial&zip={zip},us");
            client.Timeout = -1;
            var request = new RestRequest(Method.GET);
            IRestResponse response = client.Execute(request);
            return JsonConvert.DeserializeObject(response.Content);
        }

        //Takes in an object containing a weather type from the API response. Adds to database if it does not exist and returns true on success
        public bool AddWeatherType(dynamic weatherType)
        {
            bool exists = true ? weatherContext.WeatherType.Find(Convert.ToInt32(weatherType.id)) != null: false;
            if (!exists) 
            {
                weatherContext.Add(new WeatherType
                {
                    WeatherTypeID = Convert.ToInt32(weatherType.id),
                    Name = Convert.ToString(weatherType.main),
                    Description = Convert.ToString(weatherType.description)
                });
                return true;
            }
            return exists;
        }

        //Gets the DayID for a specified datetime and zipcode, adds it if not found. Returns the DayID
        //Requires a zipcode string, the date as a double of unix time, the zips timezone and the API response's sys field
        public int GetDayID(string zipcode, double unixDate, double timezone, dynamic sched)
        {
            //double unixDate = Convert.ToDouble(unixDateStr);
            //double timezone = Convert.ToDouble(timezoneStr);
            Console.WriteLine("Entered GetDayID");
            DateTime epoch = new System.DateTime(1970, 1, 1, 0, 0, 0, 0);
            int zipint = Convert.ToInt32(zipcode);
            //Get Current Day String for Local TZ
            double localtimeUnix = unixDate + timezone;
            DateTime localDate = epoch.AddSeconds(localtimeUnix);
            string dayStr = localDate.ToString("yyyy-MM-dd");
            //Originally extracted Time without date to store sunset and sunrise as time of day
            //May be better to store as UTC DateTime if DateTime type is required
            //Get sunrise and sunset time strings
            double sunrise = Convert.ToDouble(sched.sunrise);
            DateTime sunriseDT = epoch.AddSeconds(sunrise + timezone);
            double sunset = Convert.ToDouble(sched.sunset);
            string sunriseStr = sunriseDT.ToString("HH:mm:ss");
            DateTime sunsetDT = epoch.AddSeconds(sunset + timezone);
            string sunsetStr = sunsetDT.ToString("HH:mm:ss");
            bool exists = true ? weatherContext.Day.Where(d => d.LocationZipID == zipint && d.Date.Date == localDate.Date) != null : false;
            if (!exists)
            {
                weatherContext.Add(new Day
                {
                    LocationZipID = zipint,
                    Date = localDate.Date,
                    Sunrise = sunriseDT,
                    Sunset = sunsetDT
                });
            }
            Day day = weatherContext.Day.Where(d => d.LocationZipID == zipint && d.Date.Date == localDate.Date).FirstOrDefault();
            return day.DayID;
        }
    }
}
