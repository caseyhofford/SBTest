using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RestSharp;

namespace SBTest
{
    public static class Function1
    {
        [FunctionName("Function1")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");
            string token = "8358673f01a549b46b006ef9858d324e";
            //string name = req.Query["name"];
            string zip = req.Query["zip"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            //name = name ?? data?.name;
            zip = zip ?? data?.zip;

            var client = new RestClient($"https://api.openweathermap.org/data/2.5/weather?appid={token}&zip={zip},us");
            client.Timeout = -1;
            var request = new RestRequest(Method.GET);
            IRestResponse response = client.Execute(request);
            Console.WriteLine(response.Content);
            //dynamic weather = JsonConvert.DeserializeObject(response.Content);
            return zip != null
                ? (ActionResult)new OkObjectResult(response.Content)
                : new BadRequestObjectResult("Zip not specified");
        }
    }
}
