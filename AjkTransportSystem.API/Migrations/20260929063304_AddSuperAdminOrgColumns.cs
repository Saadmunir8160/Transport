using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AjkTransportSystem.API.Migrations
{
    /// <inheritdoc />
    public partial class AddSuperAdminOrgColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AdminUserId",
                table: "Organizations",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsSoftDeleted",
                table: "Organizations",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastActivityDate",
                table: "Organizations",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Subdomain",
                table: "Organizations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Timings_RouteId",
                table: "Timings",
                column: "RouteId");

            migrationBuilder.CreateIndex(
                name: "IX_Timings_VehicleId",
                table: "Timings",
                column: "VehicleId");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_RouteId",
                table: "Bookings",
                column: "RouteId");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_TripId",
                table: "Bookings",
                column: "TripId");

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_Routes_RouteId",
                table: "Bookings",
                column: "RouteId",
                principalTable: "Routes",
                principalColumn: "RouteId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_Trips_TripId",
                table: "Bookings",
                column: "TripId",
                principalTable: "Trips",
                principalColumn: "TripId");

            migrationBuilder.AddForeignKey(
                name: "FK_Timings_Routes_RouteId",
                table: "Timings",
                column: "RouteId",
                principalTable: "Routes",
                principalColumn: "RouteId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Timings_Vehicles_VehicleId",
                table: "Timings",
                column: "VehicleId",
                principalTable: "Vehicles",
                principalColumn: "VehicleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_Routes_RouteId",
                table: "Bookings");

            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_Trips_TripId",
                table: "Bookings");

            migrationBuilder.DropForeignKey(
                name: "FK_Timings_Routes_RouteId",
                table: "Timings");

            migrationBuilder.DropForeignKey(
                name: "FK_Timings_Vehicles_VehicleId",
                table: "Timings");

            migrationBuilder.DropIndex(
                name: "IX_Timings_RouteId",
                table: "Timings");

            migrationBuilder.DropIndex(
                name: "IX_Timings_VehicleId",
                table: "Timings");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_RouteId",
                table: "Bookings");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_TripId",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "AdminUserId",
                table: "Organizations");

            migrationBuilder.DropColumn(
                name: "IsSoftDeleted",
                table: "Organizations");

            migrationBuilder.DropColumn(
                name: "LastActivityDate",
                table: "Organizations");

            migrationBuilder.DropColumn(
                name: "Subdomain",
                table: "Organizations");
        }
    }
}
