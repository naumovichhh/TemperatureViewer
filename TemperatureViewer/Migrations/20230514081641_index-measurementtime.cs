using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TemperatureViewer.Migrations
{
    public partial class indexmeasurementtime : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Values_MeasurementTime",
                table: "Values",
                column: "MeasurementTime");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Values_MeasurementTime",
                table: "Values");
        }
    }
}
