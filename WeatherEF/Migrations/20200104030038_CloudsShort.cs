using Microsoft.EntityFrameworkCore.Migrations;

namespace WeatherEF.Migrations
{
    public partial class CloudsShort : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<short>(
                name: "Clouds",
                table: "Reading",
                nullable: false,
                oldClrType: typeof(byte));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<byte>(
                name: "Clouds",
                table: "Reading",
                nullable: false,
                oldClrType: typeof(short));
        }
    }
}
