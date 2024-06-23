using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RescueTube.DAL.EF.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class Remove_IsAvailable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsAvailable",
                table: "Videos");

            migrationBuilder.DropColumn(
                name: "NewAvailability",
                table: "StatusChangeEvents");

            migrationBuilder.DropColumn(
                name: "PreviousAvailability",
                table: "StatusChangeEvents");

            migrationBuilder.DropColumn(
                name: "IsAvailable",
                table: "Playlists");

            migrationBuilder.DropColumn(
                name: "IsAvailable",
                table: "Comments");

            migrationBuilder.DropColumn(
                name: "IsAvailable",
                table: "Authors");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsAvailable",
                table: "Videos",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "NewAvailability",
                table: "StatusChangeEvents",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "PreviousAvailability",
                table: "StatusChangeEvents",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsAvailable",
                table: "Playlists",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsAvailable",
                table: "Comments",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsAvailable",
                table: "Authors",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
