using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RescueTube.DAL.EF.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class Update_AuthorArchivalSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ArchiveVideosFromDateTimeRanges",
                table: "AuthorArchivalSettings");

            migrationBuilder.RenameColumn(
                name: "Active",
                table: "AuthorArchivalSettings",
                newName: "IsEnabledForArchival");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsEnabledForArchival",
                table: "AuthorArchivalSettings",
                newName: "Active");

            migrationBuilder.AddColumn<string>(
                name: "ArchiveVideosFromDateTimeRanges",
                table: "AuthorArchivalSettings",
                type: "jsonb",
                nullable: true);
        }
    }
}
