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

namespace SBTest
{
    public static class GetDay
    {
        [FunctionName("GetDay")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");
            var str = Environment.GetEnvironmentVariable("sqldb_connection");
            string jsonString = "";
            string zip = req.Query["zip"];
            using (SqlConnection conn = new SqlConnection(str))
            {
                string jsoncmd = "SELECT * FROM Reading " +
                                    "WHERE Reading.ReadingDateTime > DATEADD(hh, -24, GETDATE()) ";
                if (zip != null)
                {
                    jsoncmd += $"AND  Reading.LocationZipID = {zip} ";
                }
                jsoncmd += "FOR JSON AUTO";
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(jsoncmd, conn))
                {
                    SqlDataReader zipsReader = await cmd.ExecuteReaderAsync();
                    while(zipsReader.Read())
                    {
                        jsonString += zipsReader.GetString(0);
                    }
                }
            }
            return (ActionResult)new OkObjectResult(jsonString);
        }
    }
}
