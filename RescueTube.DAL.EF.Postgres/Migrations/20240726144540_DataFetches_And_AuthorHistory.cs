using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RescueTube.DAL.EF.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class DataFetches_And_AuthorHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AuthorHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CurrentId = table.Column<Guid>(type: "uuid", nullable: false),
                    LastOfficialValidAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastValidAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    FirstNotValidAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UserName = table.Column<string>(type: "text", nullable: true),
                    DisplayName = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthorHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuthorHistories_Authors_CurrentId",
                        column: x => x.CurrentId,
                        principalTable: "Authors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DataFetches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OccurredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Success = table.Column<bool>(type: "boolean", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    ShouldAffectValidity = table.Column<bool>(type: "boolean", nullable: false),
                    Source = table.Column<string>(type: "text", nullable: false),
                    VideoId = table.Column<Guid>(type: "uuid", nullable: true),
                    AuthorId = table.Column<Guid>(type: "uuid", nullable: true),
                    CommentId = table.Column<Guid>(type: "uuid", nullable: true),
                    PlaylistId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataFetches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DataFetches_Authors_AuthorId",
                        column: x => x.AuthorId,
                        principalTable: "Authors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DataFetches_Comments_CommentId",
                        column: x => x.CommentId,
                        principalTable: "Comments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DataFetches_Playlists_PlaylistId",
                        column: x => x.PlaylistId,
                        principalTable: "Playlists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DataFetches_Videos_VideoId",
                        column: x => x.VideoId,
                        principalTable: "Videos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuthorHistories_CurrentId",
                table: "AuthorHistories",
                column: "CurrentId");

            migrationBuilder.CreateIndex(
                name: "IX_DataFetches_AuthorId",
                table: "DataFetches",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_DataFetches_CommentId",
                table: "DataFetches",
                column: "CommentId");

            migrationBuilder.CreateIndex(
                name: "IX_DataFetches_PlaylistId",
                table: "DataFetches",
                column: "PlaylistId");

            migrationBuilder.CreateIndex(
                name: "IX_DataFetches_VideoId",
                table: "DataFetches",
                column: "VideoId");

            // migrationBuilder.Sql(File.ReadAllText("./20240726144540_DataFetches.sql"));
            migrationBuilder.Sql(
"""
INSERT INTO "DataFetches" ("Id", "OccurredAt", "Success", "Type", "ShouldAffectValidity", "Source", "VideoId")
SELECT gen_random_uuid(), "LastSuccessfulFetchUnofficial", true, 'videopage', true, 'yt-dlp', "Id"
FROM "Videos" WHERE "LastSuccessfulFetchUnofficial" is not null;
INSERT INTO "DataFetches" ("Id", "OccurredAt", "Success", "Type", "ShouldAffectValidity", "Source", "VideoId")
SELECT gen_random_uuid(), "LastFetchUnofficial", false, 'videopage', true, 'yt-dlp', "Id"
FROM "Videos" WHERE "LastFetchUnofficial" is not null AND "LastFetchUnofficial" > "LastSuccessfulFetchUnofficial";
-- Video DownloadAttempts
INSERT INTO "DataFetches" ("Id", "OccurredAt", "Success", "Type", "ShouldAffectValidity", "Source", "VideoId")
SELECT gen_random_uuid(), "AddedToArchiveAt", false, 'videofiledownload', false, 'yt-dlp', "Id"
FROM "Videos" WHERE "FailedDownloadAttempts" > 0;
INSERT INTO "DataFetches" ("Id", "OccurredAt", "Success", "Type", "ShouldAffectValidity", "Source", "VideoId")
SELECT gen_random_uuid(), vf."LastFetched", true, 'videofiledownload', false, 'yt-dlp', v."Id"
FROM "Videos" v INNER JOIN "VideoFiles" vf ON vf."VideoId" = v."Id";
-- Video AuthorFetches
INSERT INTO "DataFetches" ("Id", "OccurredAt", "Success", "Type", "ShouldAffectValidity", "Source", "VideoId")
SELECT gen_random_uuid(), "AddedToArchiveAt", false, 'videoauthor', false, 'general', "Id"
FROM "Videos" WHERE "FailedAuthorFetches" > 0;
INSERT INTO "DataFetches" ("Id", "OccurredAt", "Success", "Type", "ShouldAffectValidity", "Source", "VideoId")
SELECT gen_random_uuid(), "AddedToArchiveAt", true, 'videoauthor', false, 'general', "Id"
FROM "Videos" WHERE "FailedAuthorFetches" = 0;
-- Video LastCommentsFetch
INSERT INTO "DataFetches" ("Id", "OccurredAt", "Success", "Type", "ShouldAffectValidity", "Source", "VideoId")
SELECT gen_random_uuid(), "LastCommentsFetch", true, 'comments', false, 'yt-dlp', "Id"
FROM "Videos" WHERE "LastCommentsFetch" IS NOT NULL;

INSERT INTO "DataFetches" ("Id", "OccurredAt", "Success", "Type", "ShouldAffectValidity", "Source", "PlaylistId")
SELECT gen_random_uuid(), "LastSuccessfulFetchUnofficial", true, 'playlist', true, 'yt-dlp', "Id"
FROM "Playlists" WHERE "LastSuccessfulFetchUnofficial" is not null;
INSERT INTO "DataFetches" ("Id", "OccurredAt", "Success", "Type", "ShouldAffectValidity", "Source", "PlaylistId")
SELECT gen_random_uuid(), "LastFetchUnofficial", false, 'playlist', true, 'yt-dlp', "Id"
FROM "Playlists" WHERE "LastFetchUnofficial" is not null AND "LastFetchUnofficial" > "LastSuccessfulFetchUnofficial";

INSERT INTO "DataFetches" ("Id", "OccurredAt", "Success", "Type", "ShouldAffectValidity", "Source", "CommentId")
SELECT gen_random_uuid(), "LastSuccessfulFetchUnofficial", true, 'comments', true, 'yt-dlp', "Id"
FROM "Comments" WHERE "LastSuccessfulFetchUnofficial" is not null;
INSERT INTO "DataFetches" ("Id", "OccurredAt", "Success", "Type", "ShouldAffectValidity", "Source", "CommentId")
SELECT gen_random_uuid(), "LastFetchUnofficial", false, 'comments', true, 'yt-dlp', "Id"
FROM "Comments" WHERE "LastFetchUnofficial" is not null AND "LastFetchUnofficial" > "LastSuccessfulFetchUnofficial";

-- Authors added via comments
INSERT INTO "DataFetches" ("Id", "OccurredAt", "Success", "Type", "ShouldAffectValidity", "Source", "AuthorId")
SELECT gen_random_uuid(), "LastSuccessfulFetchUnofficial", true, 'comments', true, 'yt-dlp', "Id"
FROM "Authors" WHERE "DisplayName" is null AND "LastSuccessfulFetchUnofficial" is not null;
INSERT INTO "DataFetches" ("Id", "OccurredAt", "Success", "Type", "ShouldAffectValidity", "Source", "AuthorId")
SELECT gen_random_uuid(), "LastFetchUnofficial", false, 'comments', true, 'yt-dlp', "Id"
FROM "Authors" WHERE "DisplayName" is null AND "LastFetchUnofficial" is not null AND "LastFetchUnofficial" > "LastSuccessfulFetchUnofficial";
-- Authors added via video
INSERT INTO "DataFetches" ("Id", "OccurredAt", "Success", "Type", "ShouldAffectValidity", "Source", "AuthorId")
SELECT gen_random_uuid(), "AddedToArchiveAt", true, 'videopage', true, 'yt-dlp', "Id"
FROM "Authors" WHERE "DisplayName" is not null;
-- Extra author data fetched via YouTubeExplode
INSERT INTO "DataFetches" ("Id", "OccurredAt", "Success", "Type", "ShouldAffectValidity", "Source", "AuthorId")
SELECT gen_random_uuid(), "AddedToArchiveAt", true, 'channel', true, 'ytexplode', "Id"
FROM "Authors" WHERE "FailedExtraDataFetchAttempts" = 0;
INSERT INTO "DataFetches" ("Id", "OccurredAt", "Success", "Type", "ShouldAffectValidity", "Source", "AuthorId")
SELECT gen_random_uuid(), "AddedToArchiveAt", false, 'channel', true, 'ytexplode', "Id"
FROM "Authors" WHERE "FailedExtraDataFetchAttempts" > 0;
""");
            
            migrationBuilder.DropColumn(
                name: "FailedAuthorFetches",
                table: "Videos");

            migrationBuilder.DropColumn(
                name: "FailedDownloadAttempts",
                table: "Videos");

            migrationBuilder.DropColumn(
                name: "LastFetchOfficial",
                table: "Videos");

            migrationBuilder.DropColumn(
                name: "LastFetchUnofficial",
                table: "Videos");

            migrationBuilder.DropColumn(
                name: "LastSuccessfulFetchOfficial",
                table: "Videos");

            migrationBuilder.DropColumn(
                name: "LastSuccessfulFetchUnofficial",
                table: "Videos");

            migrationBuilder.DropColumn(
                name: "LastFetchOfficial",
                table: "Playlists");

            migrationBuilder.DropColumn(
                name: "LastFetchUnofficial",
                table: "Playlists");

            migrationBuilder.DropColumn(
                name: "LastSuccessfulFetchOfficial",
                table: "Playlists");

            migrationBuilder.DropColumn(
                name: "LastSuccessfulFetchUnofficial",
                table: "Playlists");

            migrationBuilder.DropColumn(
                name: "LastFetchOfficial",
                table: "Comments");

            migrationBuilder.DropColumn(
                name: "LastFetchUnofficial",
                table: "Comments");

            migrationBuilder.DropColumn(
                name: "LastSuccessfulFetchOfficial",
                table: "Comments");

            migrationBuilder.DropColumn(
                name: "LastSuccessfulFetchUnofficial",
                table: "Comments");

            migrationBuilder.DropColumn(
                name: "FailedExtraDataFetchAttempts",
                table: "Authors");

            migrationBuilder.DropColumn(
                name: "LastFetchOfficial",
                table: "Authors");

            migrationBuilder.DropColumn(
                name: "LastFetchUnofficial",
                table: "Authors");

            migrationBuilder.DropColumn(
                name: "LastSuccessfulFetchOfficial",
                table: "Authors");

            migrationBuilder.DropColumn(
                name: "LastSuccessfulFetchUnofficial",
                table: "Authors");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastFetchOfficial",
                table: "Videos",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastFetchUnofficial",
                table: "Videos",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastSuccessfulFetchOfficial",
                table: "Videos",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastSuccessfulFetchUnofficial",
                table: "Videos",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastFetchOfficial",
                table: "Playlists",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastFetchUnofficial",
                table: "Playlists",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastSuccessfulFetchOfficial",
                table: "Playlists",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastSuccessfulFetchUnofficial",
                table: "Playlists",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastFetchOfficial",
                table: "Comments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastFetchUnofficial",
                table: "Comments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastSuccessfulFetchOfficial",
                table: "Comments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastSuccessfulFetchUnofficial",
                table: "Comments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FailedExtraDataFetchAttempts",
                table: "Authors",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastFetchOfficial",
                table: "Authors",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastFetchUnofficial",
                table: "Authors",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastSuccessfulFetchOfficial",
                table: "Authors",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastSuccessfulFetchUnofficial",
                table: "Authors",
                type: "timestamp with time zone",
                nullable: true);
            
            // TODO: Set properties from DataFetches??? Skipped for now, likely unnecessary.
            
            migrationBuilder.DropTable(
                name: "AuthorHistories");

            migrationBuilder.DropTable(
                name: "DataFetches");
        }
    }
}
