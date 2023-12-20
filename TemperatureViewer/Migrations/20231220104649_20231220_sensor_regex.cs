using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TemperatureViewer.Migrations
{
    public partial class _20231220_sensor_regex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Regex",
                table: "Sensors",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Regex",
                table: "Sensors");
        }
    }
}
