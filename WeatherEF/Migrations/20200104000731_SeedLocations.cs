using Microsoft.EntityFrameworkCore.Migrations;

namespace WeatherEF.Migrations
{
    public partial class SeedLocations : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Location",
                columns: new[] { "Zip", "City", "Latitude", "Longitude", "TimeZone" },
                values: new object[] { 98177, "Seattle", 47.75m, -122.37m, "Pacific Standard Time" });

            migrationBuilder.InsertData(
                table: "Location",
                columns: new[] { "Zip", "City", "Latitude", "Longitude", "TimeZone" },
                values: new object[] { 90004, "Los Angeles", 34.08m, -118.3m, "Pacific Standard Time" });

            migrationBuilder.InsertData(
                table: "Location",
                columns: new[] { "Zip", "City", "Latitude", "Longitude", "TimeZone" },
                values: new object[] { 80904, "Colorado Springs", 38.85m, -104.86m, "Mountain Standard Time" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Location",
                keyColumn: "Zip",
                keyValue: 80904);

            migrationBuilder.DeleteData(
                table: "Location",
                keyColumn: "Zip",
                keyValue: 90004);

            migrationBuilder.DeleteData(
                table: "Location",
                keyColumn: "Zip",
                keyValue: 98177);
        }
    }
}
