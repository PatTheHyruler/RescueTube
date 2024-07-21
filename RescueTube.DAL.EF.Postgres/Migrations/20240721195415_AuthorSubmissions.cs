using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RescueTube.DAL.EF.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class AuthorSubmissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "AuthorId",
                table: "Submissions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IdType",
                table: "Submissions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Url",
                table: "Submissions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ArchivalSettingsId",
                table: "Authors",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AuthorArchivalSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Active = table.Column<bool>(type: "boolean", nullable: false),
                    ArchiveClips = table.Column<bool>(type: "boolean", nullable: false),
                    ArchivePlaylists = table.Column<bool>(type: "boolean", nullable: false),
                    ArchiveVideos = table.Column<bool>(type: "boolean", nullable: false),
                    ArchiveVideosFromDateTimeRanges = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthorArchivalSettings", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Submissions_AuthorId",
                table: "Submissions",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_Authors_ArchivalSettingsId",
                table: "Authors",
                column: "ArchivalSettingsId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Authors_AuthorArchivalSettings_ArchivalSettingsId",
                table: "Authors",
                column: "ArchivalSettingsId",
                principalTable: "AuthorArchivalSettings",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Submissions_Authors_AuthorId",
                table: "Submissions",
                column: "AuthorId",
                principalTable: "Authors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Authors_AuthorArchivalSettings_ArchivalSettingsId",
                table: "Authors");

            migrationBuilder.DropForeignKey(
                name: "FK_Submissions_Authors_AuthorId",
                table: "Submissions");

            migrationBuilder.DropTable(
                name: "AuthorArchivalSettings");

            migrationBuilder.DropIndex(
                name: "IX_Submissions_AuthorId",
                table: "Submissions");

            migrationBuilder.DropIndex(
                name: "IX_Authors_ArchivalSettingsId",
                table: "Authors");

            migrationBuilder.DropColumn(
                name: "AuthorId",
                table: "Submissions");

            migrationBuilder.DropColumn(
                name: "IdType",
                table: "Submissions");

            migrationBuilder.DropColumn(
                name: "Url",
                table: "Submissions");

            migrationBuilder.DropColumn(
                name: "ArchivalSettingsId",
                table: "Authors");
        }
    }
}
