using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RestSharp;

namespace SBTest
{
    public static class UpdateDatabase
    {
        [FunctionName("UpdateDatabase")]
        public static async Task Run([TimerTrigger("0 0 * * * *")]TimerInfo myTimer, ILogger log)
        {
            // Get the connection string from app settings and use it to create a connection.
            var str = Environment.GetEnvironmentVariable("sqldb_connection");

            using (SqlConnection conn = new SqlConnection(str))
            {
                string addReadings = "INSERT INTO Reading " +
                "([LocationZipID],[ReadingDateTime],[WeatherTypeID],[WindSpeed],[WindDirection],[WindGust],[Temperature],[Clouds],[DayID]) " +
                "VALUES";
                //Get the locations table as an array
                string zipsQuery = "SELECT Zip FROM Location";

                conn.Open();
                using (SqlCommand cmd = new SqlCommand(zipsQuery, conn))
                {
                    SqlDataReader zipsReader = await cmd.ExecuteReaderAsync();
                    while (zipsReader.Read())
                    {
                        string zipcode = Convert.ToString(zipsReader.GetValue(0));
                        log.LogInformation($"Found Zipcode: {zipcode}");
                        dynamic weatherData = GetWeather(zipcode);
                        log.LogInformation($"Found Weather:\n{weatherData}");
                        //Convert datetime to YYYY-MM-DD hh:mm:ss
                        DateTime epoch = new System.DateTime(1970, 1, 1, 0, 0, 0, 0);
                        DateTime readDT = epoch.AddSeconds(Convert.ToDouble(weatherData.dt));
                        string formatDT = readDT.ToString("yyyy-MM-dd hh:mm:ss");

                        //Set Weather Type
                        dynamic weatherType = weatherData.weather[0];
                        AddWeatherType(weatherType);

                        //Set Day Relationship
                        string dayID = GetDayID(zipcode, Convert.ToDouble(weatherData.dt), Convert.ToDouble(weatherData.timezone), weatherData.sys);

                        string reading = $" ({zipcode},\'{formatDT}\',{weatherType.id},{weatherData.wind.speed},{weatherData.wind.deg ?? "NULL"},{weatherData.wind.gust ?? "NULL"},{weatherData.main.temp},{weatherData.clouds.all},{dayID}),";
                        addReadings += reading;
                    }
                    zipsReader.Close();
                }

                addReadings = addReadings.TrimEnd(',') + ";";
                log.LogInformation(addReadings);
                using (SqlCommand cmdInsert = new SqlCommand(addReadings, conn))
                {
                    var rows = await cmdInsert.ExecuteNonQueryAsync();
                    log.LogInformation($"{rows} rows were updated");
                }
            }
        }
        //Takes in a ZipCode as a string and returns weather data from the API as an object
        public static dynamic GetWeather(string zip)
        {
            string token = "8358673f01a549b46b006ef9858d324e";

            var client = new RestClient($"https://api.openweathermap.org/data/2.5/weather?appid={token}&zip={zip},us");
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
                                    $"(SELECT * FROM Day WHERE LocationZipID = {zipcode} AND Day = \'{dayStr}\') " +
                                "BEGIN " +
                                    "INSERT INTO Day ([LocationZipID],[Day],[Sunrise],[Sunset]) " +
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
                string getId = $"SELECT DayID FROM Day WHERE LocationZipID = {zipcode} AND Day = \'{dayStr}\';";
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
