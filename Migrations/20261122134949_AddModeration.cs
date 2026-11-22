using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ZeeAcom.Migrations
{
    /// <inheritdoc />
    public partial class AddModeration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "HiddenByAdmin",
                table: "Property",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Owner",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                // Owners that already exist were public before moderation, so they start approved.
                // New ones get their status from the app (Pending for registrations).
                defaultValue: "Approved");

            migrationBuilder.CreateIndex(
                name: "IX_Owner_Status",
                table: "Owner",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Owner_Status",
                table: "Owner");

            migrationBuilder.DropColumn(
                name: "HiddenByAdmin",
                table: "Property");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Owner");
        }
    }
}
