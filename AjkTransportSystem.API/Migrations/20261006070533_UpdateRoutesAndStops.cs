using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AjkTransportSystem.API.Migrations
{
    /// <inheritdoc />
    public partial class UpdateRoutesAndStops : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MinutesFromDeparture",
                table: "RouteStops",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "BaseFare",
                table: "Routes",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BaselineDuration",
                table: "Routes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsSeasonal",
                table: "Routes",
                type: "bit",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_RouteStops_RouteId",
                table: "RouteStops",
                column: "RouteId");

            migrationBuilder.AddForeignKey(
                name: "FK_RouteStops_Routes_RouteId",
                table: "RouteStops",
                column: "RouteId",
                principalTable: "Routes",
                principalColumn: "RouteId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RouteStops_Routes_RouteId",
                table: "RouteStops");

            migrationBuilder.DropIndex(
                name: "IX_RouteStops_RouteId",
                table: "RouteStops");

            migrationBuilder.DropColumn(
                name: "MinutesFromDeparture",
                table: "RouteStops");

            migrationBuilder.DropColumn(
                name: "BaseFare",
                table: "Routes");

            migrationBuilder.DropColumn(
                name: "BaselineDuration",
                table: "Routes");

            migrationBuilder.DropColumn(
                name: "IsSeasonal",
                table: "Routes");
        }
    }
}
