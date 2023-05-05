using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace TemperatureViewer.Migrations
{
    public partial class _2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "WasDeleted",
                table: "Sensors",
                newName: "WasDisabled");

            migrationBuilder.CreateTable(
                name: "SensorUser",
                columns: table => new
                {
                    SensorsId = table.Column<int>(type: "int", nullable: false),
                    UsersId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SensorUser", x => new { x.SensorsId, x.UsersId });
                    table.ForeignKey(
                        name: "FK_SensorUser_Sensors_SensorsId",
                        column: x => x.SensorsId,
                        principalTable: "Sensors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SensorUser_Users_UsersId",
                        column: x => x.UsersId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.RenameTable(
                name: "Measurements",
                newName: "Values");

            migrationBuilder.CreateIndex(
                name: "IX_SensorUser_UsersId",
                table: "SensorUser",
                column: "UsersId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SensorUser");

            migrationBuilder.RenameColumn(
                name: "WasDisabled",
                table: "Sensors",
                newName: "WasDeleted");

            migrationBuilder.RenameTable(
                name: "Values",
                newName: "Measurements");
        }
    }
}
