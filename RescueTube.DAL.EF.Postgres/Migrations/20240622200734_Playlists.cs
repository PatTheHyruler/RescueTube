using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RescueTube.DAL.EF.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class Playlists : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "PlaylistId",
                table: "Submissions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PlaylistId",
                table: "StatusChangeEvents",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Playlists",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TitleId = table.Column<Guid>(type: "uuid", nullable: true),
                    DescriptionId = table.Column<Guid>(type: "uuid", nullable: true),
                    Platform = table.Column<string>(type: "text", nullable: false),
                    IdOnPlatform = table.Column<string>(type: "text", nullable: false),
                    PrivacyStatusOnPlatform = table.Column<string>(type: "text", nullable: true),
                    IsAvailable = table.Column<bool>(type: "boolean", nullable: false),
                    PrivacyStatus = table.Column<string>(type: "text", nullable: false),
                    LastFetchUnofficial = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastSuccessfulFetchUnofficial = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastFetchOfficial = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastSuccessfulFetchOfficial = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    AddedToArchiveAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Playlists", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Playlists_Authors_CreatorId",
                        column: x => x.CreatorId,
                        principalTable: "Authors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Playlists_TextTranslationKeys_DescriptionId",
                        column: x => x.DescriptionId,
                        principalTable: "TextTranslationKeys",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Playlists_TextTranslationKeys_TitleId",
                        column: x => x.TitleId,
                        principalTable: "TextTranslationKeys",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PlaylistImages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ImageType = table.Column<string>(type: "text", nullable: false),
                    ValidSince = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ValidUntil = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastFetched = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    PlaylistId = table.Column<Guid>(type: "uuid", nullable: false),
                    ImageId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlaylistImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlaylistImages_Images_ImageId",
                        column: x => x.ImageId,
                        principalTable: "Images",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PlaylistImages_Playlists_PlaylistId",
                        column: x => x.PlaylistId,
                        principalTable: "Playlists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PlaylistItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PlaylistId = table.Column<Guid>(type: "uuid", nullable: false),
                    VideoId = table.Column<Guid>(type: "uuid", nullable: false),
                    Position = table.Column<long>(type: "bigint", nullable: false),
                    AddedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    RemovedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlaylistItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlaylistItems_Playlists_PlaylistId",
                        column: x => x.PlaylistId,
                        principalTable: "Playlists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PlaylistItems_Videos_VideoId",
                        column: x => x.VideoId,
                        principalTable: "Videos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PlaylistStatisticSnapshot",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ViewCount = table.Column<long>(type: "bigint", nullable: true),
                    LikeCount = table.Column<long>(type: "bigint", nullable: true),
                    DislikeCount = table.Column<long>(type: "bigint", nullable: true),
                    CommentCount = table.Column<long>(type: "bigint", nullable: true),
                    ValidAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    PlaylistId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlaylistStatisticSnapshot", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlaylistStatisticSnapshot_Playlists_PlaylistId",
                        column: x => x.PlaylistId,
                        principalTable: "Playlists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PlaylistItemPositionHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PlaylistItemId = table.Column<Guid>(type: "uuid", nullable: true),
                    Position = table.Column<long>(type: "bigint", nullable: false),
                    ValidSince = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ValidUntil = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlaylistItemPositionHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlaylistItemPositionHistories_PlaylistItems_PlaylistItemId",
                        column: x => x.PlaylistItemId,
                        principalTable: "PlaylistItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Submissions_PlaylistId",
                table: "Submissions",
                column: "PlaylistId");

            migrationBuilder.CreateIndex(
                name: "IX_StatusChangeEvents_PlaylistId",
                table: "StatusChangeEvents",
                column: "PlaylistId");

            migrationBuilder.CreateIndex(
                name: "IX_PlaylistImages_ImageId",
                table: "PlaylistImages",
                column: "ImageId");

            migrationBuilder.CreateIndex(
                name: "IX_PlaylistImages_PlaylistId",
                table: "PlaylistImages",
                column: "PlaylistId");

            migrationBuilder.CreateIndex(
                name: "IX_PlaylistItemPositionHistories_PlaylistItemId",
                table: "PlaylistItemPositionHistories",
                column: "PlaylistItemId");

            migrationBuilder.CreateIndex(
                name: "IX_PlaylistItems_PlaylistId",
                table: "PlaylistItems",
                column: "PlaylistId");

            migrationBuilder.CreateIndex(
                name: "IX_PlaylistItems_VideoId",
                table: "PlaylistItems",
                column: "VideoId");

            migrationBuilder.CreateIndex(
                name: "IX_Playlists_CreatorId",
                table: "Playlists",
                column: "CreatorId");

            migrationBuilder.CreateIndex(
                name: "IX_Playlists_DescriptionId",
                table: "Playlists",
                column: "DescriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_Playlists_TitleId",
                table: "Playlists",
                column: "TitleId");

            migrationBuilder.CreateIndex(
                name: "IX_PlaylistStatisticSnapshot_PlaylistId",
                table: "PlaylistStatisticSnapshot",
                column: "PlaylistId");

            migrationBuilder.AddForeignKey(
                name: "FK_StatusChangeEvents_Playlists_PlaylistId",
                table: "StatusChangeEvents",
                column: "PlaylistId",
                principalTable: "Playlists",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Submissions_Playlists_PlaylistId",
                table: "Submissions",
                column: "PlaylistId",
                principalTable: "Playlists",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StatusChangeEvents_Playlists_PlaylistId",
                table: "StatusChangeEvents");

            migrationBuilder.DropForeignKey(
                name: "FK_Submissions_Playlists_PlaylistId",
                table: "Submissions");

            migrationBuilder.DropTable(
                name: "PlaylistImages");

            migrationBuilder.DropTable(
                name: "PlaylistItemPositionHistories");

            migrationBuilder.DropTable(
                name: "PlaylistStatisticSnapshot");

            migrationBuilder.DropTable(
                name: "PlaylistItems");

            migrationBuilder.DropTable(
                name: "Playlists");

            migrationBuilder.DropIndex(
                name: "IX_Submissions_PlaylistId",
                table: "Submissions");

            migrationBuilder.DropIndex(
                name: "IX_StatusChangeEvents_PlaylistId",
                table: "StatusChangeEvents");

            migrationBuilder.DropColumn(
                name: "PlaylistId",
                table: "Submissions");

            migrationBuilder.DropColumn(
                name: "PlaylistId",
                table: "StatusChangeEvents");
        }
    }
}
