using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using WeatherEF;

[assembly: FunctionsStartup(typeof(SBTest.Startup))]

namespace SBTest
{
    class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            string SqlConnection = Environment.GetEnvironmentVariable("sqldb_connection");
            builder.Services.AddDbContext<WeatherContext>(
                options => options.UseSqlServer(SqlConnection));
        }
    }
}
