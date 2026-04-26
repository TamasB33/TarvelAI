using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TarvelAI.Migrations
{
    /// <inheritdoc />
    public partial class FixTripUserFK : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Trips_AspNetUsers_UserId",
                table: "Trips");

            migrationBuilder.DropIndex(
                name: "IX_Trips_UserId",
                table: "Trips");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Trips");

            migrationBuilder.CreateIndex(
                name: "IX_Trips_CreatedBy",
                table: "Trips",
                column: "CreatedBy");

            migrationBuilder.AddForeignKey(
                name: "FK_Trips_AspNetUsers_CreatedBy",
                table: "Trips",
                column: "CreatedBy",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Trips_AspNetUsers_CreatedBy",
                table: "Trips");

            migrationBuilder.DropIndex(
                name: "IX_Trips_CreatedBy",
                table: "Trips");

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Trips",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Trips_UserId",
                table: "Trips",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Trips_AspNetUsers_UserId",
                table: "Trips",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
