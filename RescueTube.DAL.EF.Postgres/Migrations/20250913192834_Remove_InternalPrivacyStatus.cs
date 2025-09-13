using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RescueTube.DAL.EF.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class Remove_InternalPrivacyStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PrivacyStatus",
                table: "Videos");

            migrationBuilder.DropColumn(
                name: "PrivacyStatus",
                table: "Playlists");

            migrationBuilder.DropColumn(
                name: "PrivacyStatus",
                table: "Comments");

            migrationBuilder.DropColumn(
                name: "PrivacyStatus",
                table: "Authors");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PrivacyStatus",
                table: "Videos",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PrivacyStatus",
                table: "Playlists",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PrivacyStatus",
                table: "Comments",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PrivacyStatus",
                table: "Authors",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
