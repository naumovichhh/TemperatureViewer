using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TemperatureViewer.Migrations
{
    public partial class json : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "JSON",
                table: "Sensors",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "JSON",
                table: "Sensors");
        }
    }
}
