using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TarvelAI.Migrations
{
    /// <inheritdoc />
    public partial class AddTripBookingLinksToHotelFlight : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TripBookingId",
                table: "HotelBookings",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TripBookingId",
                table: "FlightBookings",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_HotelBookings_TripBookingId",
                table: "HotelBookings",
                column: "TripBookingId");

            migrationBuilder.CreateIndex(
                name: "IX_FlightBookings_TripBookingId",
                table: "FlightBookings",
                column: "TripBookingId");

            migrationBuilder.AddForeignKey(
                name: "FK_HotelBookings_TripBookings_TripBookingId",
                table: "HotelBookings",
                column: "TripBookingId",
                principalTable: "TripBookings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FlightBookings_TripBookings_TripBookingId",
                table: "FlightBookings",
                column: "TripBookingId",
                principalTable: "TripBookings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FlightBookings_TripBookings_TripBookingId",
                table: "FlightBookings");

            migrationBuilder.DropForeignKey(
                name: "FK_HotelBookings_TripBookings_TripBookingId",
                table: "HotelBookings");

            migrationBuilder.DropIndex(
                name: "IX_FlightBookings_TripBookingId",
                table: "FlightBookings");

            migrationBuilder.DropIndex(
                name: "IX_HotelBookings_TripBookingId",
                table: "HotelBookings");

            migrationBuilder.DropColumn(
                name: "TripBookingId",
                table: "FlightBookings");

            migrationBuilder.DropColumn(
                name: "TripBookingId",
                table: "HotelBookings");
        }
    }
}
