using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TarvelAI.Migrations
{
    /// <inheritdoc />
    public partial class AutoPendingMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ConfirmationNumber",
                table: "HotelBookings",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConfirmationNumber",
                table: "HotelBookings");
        }
    }
}
