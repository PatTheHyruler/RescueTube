using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.EF.Migrations.Postgres
{
    /// <inheritdoc />
    public partial class VideoAndAuthorDownloadExtra : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FailedAuthorFetches",
                table: "Videos",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "FailedDownloadAttempts",
                table: "Videos",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "InfoJsonPath",
                table: "Videos",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FailedExtraDataFetchAttempts",
                table: "Authors",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FailedAuthorFetches",
                table: "Videos");

            migrationBuilder.DropColumn(
                name: "FailedDownloadAttempts",
                table: "Videos");

            migrationBuilder.DropColumn(
                name: "InfoJsonPath",
                table: "Videos");

            migrationBuilder.DropColumn(
                name: "FailedExtraDataFetchAttempts",
                table: "Authors");
        }
    }
}
