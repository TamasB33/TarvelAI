using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TarvelAI.Migrations
{
    /// <inheritdoc />
    public partial class AddTripAdminPlanningFlags : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AdminAirlineConfirmed",
                table: "Trips",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "AdminHotelConfirmed",
                table: "Trips",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdminAirlineConfirmed",
                table: "Trips");

            migrationBuilder.DropColumn(
                name: "AdminHotelConfirmed",
                table: "Trips");
        }
    }
}
