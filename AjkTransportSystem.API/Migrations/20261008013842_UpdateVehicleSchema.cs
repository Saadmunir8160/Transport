using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AjkTransportSystem.API.Migrations
{
    /// <inheritdoc />
    public partial class UpdateVehicleSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "FitnessExpiry",
                table: "Vehicles",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FuelType",
                table: "Vehicles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "InsuranceExpiry",
                table: "Vehicles",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Make",
                table: "Vehicles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Model",
                table: "Vehicles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "NextDocumentExpiry",
                table: "Vehicles",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PUCExpiry",
                table: "Vehicles",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PermitExpiry",
                table: "Vehicles",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SeatLayout",
                table: "Vehicles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Year",
                table: "Vehicles",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Documents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EntityType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    EntityId = table.Column<int>(type: "int", nullable: false),
                    DocumentType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FileUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReminderDays = table.Column<string>(type: "nvarchar(max)", nullable: false, defaultValue: "30,15,7"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Documents", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Complaints_Vehicles_VehicleId",
                table: "Complaints");

            migrationBuilder.DropTable(
                name: "Advisories");

            migrationBuilder.DropTable(
                name: "Documents");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Complaints_VehicleId",
                table: "Complaints");

            migrationBuilder.DropColumn(
                name: "FitnessExpiry",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "FuelType",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "InsuranceExpiry",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "Make",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "Model",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "NextDocumentExpiry",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "PUCExpiry",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "PermitExpiry",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "SeatLayout",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "Year",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "Address",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "AssignedVehicleId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "EmergencyContact",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IDNumber",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "JoiningDate",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LicenseIssueDate",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LicenseType",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PhotoUrl",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "BookedSeats",
                table: "Trips");

            migrationBuilder.DropColumn(
                name: "CancelReason",
                table: "Trips");

            migrationBuilder.DropColumn(
                name: "DelayInMinutes",
                table: "Trips");

            migrationBuilder.DropColumn(
                name: "LastStopReached",
                table: "Trips");

            migrationBuilder.DropColumn(
                name: "NextStop",
                table: "Trips");

            migrationBuilder.DropColumn(
                name: "PredictedETA",
                table: "Trips");

            migrationBuilder.DropColumn(
                name: "DefaultDriverId",
                table: "Timings");

            migrationBuilder.DropColumn(
                name: "ValidFrom",
                table: "Timings");

            migrationBuilder.DropColumn(
                name: "ValidTo",
                table: "Timings");

            migrationBuilder.DropColumn(
                name: "Address",
                table: "Organizations");

            migrationBuilder.DropColumn(
                name: "DefaultLanguage",
                table: "Organizations");

            migrationBuilder.DropColumn(
                name: "Severity",
                table: "Complaints");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Complaints");

            migrationBuilder.DropColumn(
                name: "VehicleId",
                table: "Complaints");

            migrationBuilder.DropColumn(
                name: "CancelReason",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "IsNoShow",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "PaymentStatus",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "TicketCode",
                table: "Bookings");
        }
    }
}
