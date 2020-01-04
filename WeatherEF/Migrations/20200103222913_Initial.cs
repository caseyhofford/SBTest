using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace WeatherEF.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Location",
                columns: table => new
                {
                    Zip = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    City = table.Column<string>(nullable: true),
                    Latitude = table.Column<decimal>(nullable: false),
                    Longitude = table.Column<decimal>(nullable: false),
                    TimeZone = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Location", x => x.Zip);
                });

            migrationBuilder.CreateTable(
                name: "WeatherType",
                columns: table => new
                {
                    WeatherTypeID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WeatherType", x => x.WeatherTypeID);
                });

            migrationBuilder.CreateTable(
                name: "Day",
                columns: table => new
                {
                    DayID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    LocationZipID = table.Column<int>(nullable: false),
                    Date = table.Column<DateTime>(nullable: false),
                    TimeZone = table.Column<string>(nullable: true),
                    LocationZip = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Day", x => x.DayID);
                    table.ForeignKey(
                        name: "FK_Day_Location_LocationZip",
                        column: x => x.LocationZip,
                        principalTable: "Location",
                        principalColumn: "Zip",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Reading",
                columns: table => new
                {
                    ReadingID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    LocationZipID = table.Column<int>(nullable: false),
                    ReadingDateTime = table.Column<DateTime>(nullable: false),
                    WeatherTypeID = table.Column<int>(nullable: false),
                    WindSpeed = table.Column<decimal>(nullable: false),
                    WindDirection = table.Column<short>(nullable: false),
                    WindGust = table.Column<decimal>(nullable: false),
                    Temperature = table.Column<decimal>(nullable: false),
                    Clouds = table.Column<byte>(nullable: false),
                    DayID = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reading", x => x.ReadingID);
                    table.ForeignKey(
                        name: "FK_Reading_Day_DayID",
                        column: x => x.DayID,
                        principalTable: "Day",
                        principalColumn: "DayID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Reading_Location_LocationZipID",
                        column: x => x.LocationZipID,
                        principalTable: "Location",
                        principalColumn: "Zip",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Reading_WeatherType_WeatherTypeID",
                        column: x => x.WeatherTypeID,
                        principalTable: "WeatherType",
                        principalColumn: "WeatherTypeID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Day_LocationZip",
                table: "Day",
                column: "LocationZip");

            migrationBuilder.CreateIndex(
                name: "IX_Reading_DayID",
                table: "Reading",
                column: "DayID");

            migrationBuilder.CreateIndex(
                name: "IX_Reading_LocationZipID",
                table: "Reading",
                column: "LocationZipID");

            migrationBuilder.CreateIndex(
                name: "IX_Reading_WeatherTypeID",
                table: "Reading",
                column: "WeatherTypeID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Reading");

            migrationBuilder.DropTable(
                name: "Day");

            migrationBuilder.DropTable(
                name: "WeatherType");

            migrationBuilder.DropTable(
                name: "Location");
        }
    }
}
