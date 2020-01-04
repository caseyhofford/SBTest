using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RestSharp;
using WeatherEF;

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
                string dayID = GetDayID(Convert.ToString(zipcode), Convert.ToDouble(weatherData.dt), Convert.ToDouble(weatherData.timezone), weatherData.sys);

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
                        DayID = Convert.ToInt32(dayID)
                    }
                );

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
        public static bool AddWeatherType(dynamic weatherType)
        {
            var str = Environment.GetEnvironmentVariable("sqldb_connection");
            using (SqlConnection conn = new SqlConnection(str))
            {
                conn.Open();
                string cmd = "BEGIN " +
                                "IF NOT EXISTS " +
                                    $"(SELECT * FROM WeatherType WHERE WeatherTypeID = {weatherType.id})" +
                                "BEGIN " +
                                    "INSERT INTO WeatherType ([WeatherTypeID],[Name],[Description]) " +
                                    $"VALUES ({weatherType.id},\'{weatherType.main}\',\'{weatherType.description}\') " +
                                "END " +
                            "END";
                Console.WriteLine(cmd);
                using (SqlCommand cmdInsert = new SqlCommand(cmd, conn))
                {
                    var rows = cmdInsert.ExecuteNonQuery();
                    Console.WriteLine($"{rows} rows were updated");
                    return true;
                }
            }
        }

        //Gets the DayID for a specified datetime and zipcode, adds it if not found. Returns the DayID
        //Requires a zipcode string, the date as a double of unix time, the zips timezone and the API response's sys field
        public static string GetDayID(string zipcode, double unixDate, double timezone, dynamic sched)
        {
            //double unixDate = Convert.ToDouble(unixDateStr);
            //double timezone = Convert.ToDouble(timezoneStr);
            Console.WriteLine("Entered GetDayID");
            DateTime epoch = new System.DateTime(1970, 1, 1, 0, 0, 0, 0);
            //Get Current Day String for Locale
            double localtimeUnix = unixDate + timezone;
            DateTime localDate = epoch.AddSeconds(localtimeUnix);
            string dayStr = localDate.ToString("yyyy-MM-dd");

            //Get sunrise and sunset time strings
            double sunrise = Convert.ToDouble(sched.sunrise);
            DateTime sunriseDT = epoch.AddSeconds(sunrise + timezone);
            double sunset = Convert.ToDouble(sched.sunset);
            string sunriseStr = sunriseDT.ToString("HH:mm:ss");
            DateTime sunsetDT = epoch.AddSeconds(sunset + timezone);
            string sunsetStr = sunsetDT.ToString("HH:mm:ss");

            var str = Environment.GetEnvironmentVariable("sqldb_connection");
            using (SqlConnection conn = new SqlConnection(str))
            {

                conn.Open();
                //Query to add day if it doesn't exist
                string query = "BEGIN " +
                                "IF NOT EXISTS " +
                                    $"(SELECT * FROM Day WHERE LocationZipID = {zipcode} AND Date = \'{dayStr}\') " +
                                "BEGIN " +
                                    "INSERT INTO Day ([LocationZipID],[Date],[Sunrise],[Sunset]) " +
                                    $"VALUES ({zipcode},\'{dayStr}\',\'{sunriseStr}\',\'{sunsetStr}\') " +
                                "END " +
                                "END";

                Console.WriteLine(query);
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    int rows = cmd.ExecuteNonQuery();
                    Console.WriteLine($"{rows} rows updated");
                }
                //query to get DayID
                string getId = $"SELECT DayID FROM Day WHERE LocationZipID = {zipcode} AND Date = \'{dayStr}\';";
                Console.WriteLine(getId);
                using (SqlCommand cmd = new SqlCommand(getId, conn))
                {
                    SqlDataReader reader = cmd.ExecuteReader();
                    reader.Read();
                    return (Convert.ToString(reader.GetValue(0)));
                }
            }
        }
    }
}
