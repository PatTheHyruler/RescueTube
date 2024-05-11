using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.EF.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class Comments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Comments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Platform = table.Column<string>(type: "text", nullable: false),
                    IdOnPlatform = table.Column<string>(type: "text", nullable: false),
                    PrivacyStatusOnPlatform = table.Column<string>(type: "text", nullable: true),
                    IsAvailable = table.Column<bool>(type: "boolean", nullable: false),
                    PrivacyStatus = table.Column<string>(type: "text", nullable: false),
                    LastFetchUnofficial = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastSuccessfulFetchUnofficial = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastFetchOfficial = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastSuccessfulFetchOfficial = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AddedToArchiveAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AuthorId = table.Column<Guid>(type: "uuid", nullable: false),
                    VideoId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReplyTargetId = table.Column<Guid>(type: "uuid", nullable: true),
                    ConversationRootId = table.Column<Guid>(type: "uuid", nullable: true),
                    Content = table.Column<string>(type: "text", nullable: true),
                    AuthorIsCreator = table.Column<bool>(type: "boolean", nullable: true),
                    CreatedAtVideoTimecode = table.Column<TimeSpan>(type: "interval", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    OrderIndex = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Comments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Comments_Authors_AuthorId",
                        column: x => x.AuthorId,
                        principalTable: "Authors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Comments_Comments_ConversationRootId",
                        column: x => x.ConversationRootId,
                        principalTable: "Comments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Comments_Comments_ReplyTargetId",
                        column: x => x.ReplyTargetId,
                        principalTable: "Comments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Comments_Videos_VideoId",
                        column: x => x.VideoId,
                        principalTable: "Videos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CommentHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CurrentId = table.Column<Guid>(type: "uuid", nullable: false),
                    LastOfficialValidAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastValidAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FirstNotValidAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Content = table.Column<string>(type: "text", nullable: true),
                    CreatedAtVideoTimecode = table.Column<TimeSpan>(type: "interval", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommentHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CommentHistories_Comments_CurrentId",
                        column: x => x.CurrentId,
                        principalTable: "Comments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CommentStatisticSnapshots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LikeCount = table.Column<long>(type: "bigint", nullable: true),
                    DislikeCount = table.Column<long>(type: "bigint", nullable: true),
                    ReplyCount = table.Column<long>(type: "bigint", nullable: true),
                    IsFavorited = table.Column<bool>(type: "boolean", nullable: true),
                    ValidAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CommentId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommentStatisticSnapshots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CommentStatisticSnapshots_Comments_CommentId",
                        column: x => x.CommentId,
                        principalTable: "Comments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CommentHistories_CurrentId",
                table: "CommentHistories",
                column: "CurrentId");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_AuthorId",
                table: "Comments",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_ConversationRootId",
                table: "Comments",
                column: "ConversationRootId");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_ReplyTargetId",
                table: "Comments",
                column: "ReplyTargetId");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_VideoId",
                table: "Comments",
                column: "VideoId");

            migrationBuilder.CreateIndex(
                name: "IX_CommentStatisticSnapshots_CommentId",
                table: "CommentStatisticSnapshots",
                column: "CommentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CommentHistories");

            migrationBuilder.DropTable(
                name: "CommentStatisticSnapshots");

            migrationBuilder.DropTable(
                name: "Comments");
        }
    }
}
