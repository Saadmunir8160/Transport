using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AjkTransportSystem.API.Migrations
{
    /// <inheritdoc />
    public partial class AddOrgWizardFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ContactEmail",
                table: "Organizations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContactPhone",
                table: "Organizations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LogoUrl",
                table: "Organizations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OfficeHours",
                table: "Organizations",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContactEmail",
                table: "Organizations");

            migrationBuilder.DropColumn(
                name: "ContactPhone",
                table: "Organizations");

            migrationBuilder.DropColumn(
                name: "LogoUrl",
                table: "Organizations");

            migrationBuilder.DropColumn(
                name: "OfficeHours",
                table: "Organizations");
        }
    }
}
