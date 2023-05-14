using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TemperatureViewer.Migrations
{
    public partial class observerid : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ObserverSensor_Observers_ObserversEmail",
                table: "ObserverSensor");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ObserverSensor",
                table: "ObserverSensor");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Observers",
                table: "Observers");

            migrationBuilder.DropColumn(
                name: "ObserversEmail",
                table: "ObserverSensor");

            migrationBuilder.AddColumn<int>(
                name: "ObserversId",
                table: "ObserverSensor",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Observers",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "Observers",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ObserverSensor",
                table: "ObserverSensor",
                columns: new[] { "ObserversId", "SensorsId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_Observers",
                table: "Observers",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ObserverSensor_Observers_ObserversId",
                table: "ObserverSensor",
                column: "ObserversId",
                principalTable: "Observers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ObserverSensor_Observers_ObserversId",
                table: "ObserverSensor");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ObserverSensor",
                table: "ObserverSensor");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Observers",
                table: "Observers");

            migrationBuilder.DropColumn(
                name: "ObserversId",
                table: "ObserverSensor");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "Observers");

            migrationBuilder.AddColumn<string>(
                name: "ObserversEmail",
                table: "ObserverSensor",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Observers",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ObserverSensor",
                table: "ObserverSensor",
                columns: new[] { "ObserversEmail", "SensorsId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_Observers",
                table: "Observers",
                column: "Email");

            migrationBuilder.AddForeignKey(
                name: "FK_ObserverSensor_Observers_ObserversEmail",
                table: "ObserverSensor",
                column: "ObserversEmail",
                principalTable: "Observers",
                principalColumn: "Email",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
