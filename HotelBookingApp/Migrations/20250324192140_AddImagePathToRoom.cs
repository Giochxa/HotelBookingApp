using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HotelBookingApp.Migrations
{
    /// <inheritdoc />
    public partial class AddImagePathToRoom : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImagePath",
                table: "Rooms",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImagePath",
                table: "Rooms");
        }
    }
}
