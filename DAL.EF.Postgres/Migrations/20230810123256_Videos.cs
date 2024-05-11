using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.EF.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class Videos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AuthorImages_Authors_AuthorId",
                table: "AuthorImages");

            migrationBuilder.DropForeignKey(
                name: "FK_AuthorImages_Images_ImageId",
                table: "AuthorImages");

            migrationBuilder.DropForeignKey(
                name: "FK_Authors_TextTranslationKeys_BioId",
                table: "Authors");

            migrationBuilder.DropForeignKey(
                name: "FK_AuthorStatisticSnapshots_Authors_AuthorId",
                table: "AuthorStatisticSnapshots");

            migrationBuilder.DropForeignKey(
                name: "FK_RefreshTokens_AspNetUsers_UserId",
                table: "RefreshTokens");

            migrationBuilder.DropForeignKey(
                name: "FK_TextTranslations_TextTranslationKeys_KeyId",
                table: "TextTranslations");

            migrationBuilder.DropIndex(
                name: "IX_Authors_Platform_IdOnPlatform",
                table: "Authors");

            migrationBuilder.AddColumn<DateTime>(
                name: "ValidAt",
                table: "AuthorStatisticSnapshots",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NameId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsPublic = table.Column<bool>(type: "boolean", nullable: false),
                    IsAssignable = table.Column<bool>(type: "boolean", nullable: false),
                    Platform = table.Column<string>(type: "text", nullable: false),
                    IdOnPlatform = table.Column<string>(type: "text", nullable: true),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Categories_Authors_CreatorId",
                        column: x => x.CreatorId,
                        principalTable: "Authors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Categories_TextTranslationKeys_NameId",
                        column: x => x.NameId,
                        principalTable: "TextTranslationKeys",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Videos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TitleId = table.Column<Guid>(type: "uuid", nullable: true),
                    DescriptionId = table.Column<Guid>(type: "uuid", nullable: true),
                    DefaultLanguage = table.Column<string>(type: "text", nullable: true),
                    DefaultAudioLanguage = table.Column<string>(type: "text", nullable: true),
                    Duration = table.Column<TimeSpan>(type: "interval", nullable: true),
                    LastCommentsFetch = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsLiveStreamRecording = table.Column<bool>(type: "boolean", nullable: true),
                    StreamId = table.Column<string>(type: "text", nullable: true),
                    LiveStreamStartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LiveStreamEndedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PublishedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RecordedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Platform = table.Column<string>(type: "text", nullable: false),
                    IdOnPlatform = table.Column<string>(type: "text", nullable: false),
                    PrivacyStatusOnPlatform = table.Column<string>(type: "text", nullable: true),
                    IsAvailable = table.Column<bool>(type: "boolean", nullable: false),
                    PrivacyStatus = table.Column<string>(type: "text", nullable: false),
                    LastFetchUnofficial = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastSuccessfulFetchUnofficial = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastFetchOfficial = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastSuccessfulFetchOfficial = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AddedToArchiveAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Videos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Videos_TextTranslationKeys_DescriptionId",
                        column: x => x.DescriptionId,
                        principalTable: "TextTranslationKeys",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Videos_TextTranslationKeys_TitleId",
                        column: x => x.TitleId,
                        principalTable: "TextTranslationKeys",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Captions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Platform = table.Column<string>(type: "text", nullable: false),
                    IdOnPlatform = table.Column<string>(type: "text", nullable: true),
                    Culture = table.Column<string>(type: "text", nullable: true),
                    Ext = table.Column<string>(type: "text", nullable: false),
                    FilePath = table.Column<string>(type: "text", nullable: true),
                    Url = table.Column<string>(type: "text", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: true),
                    Etag = table.Column<string>(type: "text", nullable: true),
                    ValidSince = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ValidUntil = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastFetched = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    VideoId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Captions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Captions_Videos_VideoId",
                        column: x => x.VideoId,
                        principalTable: "Videos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EntityAccessPermissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    VideoId = table.Column<Guid>(type: "uuid", nullable: true),
                    AuthorId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntityAccessPermissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EntityAccessPermissions_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EntityAccessPermissions_Authors_AuthorId",
                        column: x => x.AuthorId,
                        principalTable: "Authors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EntityAccessPermissions_Videos_VideoId",
                        column: x => x.VideoId,
                        principalTable: "Videos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StatusChangeEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PreviousAvailability = table.Column<bool>(type: "boolean", nullable: true),
                    NewAvailability = table.Column<bool>(type: "boolean", nullable: true),
                    PreviousPrivacyStatus = table.Column<string>(type: "text", nullable: true),
                    NewPrivacyStatus = table.Column<string>(type: "text", nullable: true),
                    OccurredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AuthorId = table.Column<Guid>(type: "uuid", nullable: true),
                    VideoId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StatusChangeEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StatusChangeEvents_Authors_AuthorId",
                        column: x => x.AuthorId,
                        principalTable: "Authors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StatusChangeEvents_Videos_VideoId",
                        column: x => x.VideoId,
                        principalTable: "Videos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "VideoAuthors",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VideoId = table.Column<Guid>(type: "uuid", nullable: false),
                    AuthorId = table.Column<Guid>(type: "uuid", nullable: false),
                    Role = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VideoAuthors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VideoAuthors_Authors_AuthorId",
                        column: x => x.AuthorId,
                        principalTable: "Authors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VideoAuthors_Videos_VideoId",
                        column: x => x.VideoId,
                        principalTable: "Videos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "VideoCategories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VideoId = table.Column<Guid>(type: "uuid", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssignedById = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VideoCategories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VideoCategories_Authors_AssignedById",
                        column: x => x.AssignedById,
                        principalTable: "Authors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VideoCategories_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VideoCategories_Videos_VideoId",
                        column: x => x.VideoId,
                        principalTable: "Videos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "VideoFiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Key = table.Column<string>(type: "text", nullable: true),
                    FilePath = table.Column<string>(type: "text", nullable: false),
                    Etag = table.Column<string>(type: "text", nullable: true),
                    Width = table.Column<int>(type: "integer", nullable: true),
                    Height = table.Column<int>(type: "integer", nullable: true),
                    BitrateBps = table.Column<int>(type: "integer", nullable: true),
                    ValidSince = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ValidUntil = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastFetched = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    VideoId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VideoFiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VideoFiles_Videos_VideoId",
                        column: x => x.VideoId,
                        principalTable: "Videos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "VideoImages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ImageType = table.Column<string>(type: "text", nullable: false),
                    ValidSince = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ValidUntil = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastFetched = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    VideoId = table.Column<Guid>(type: "uuid", nullable: false),
                    ImageId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VideoImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VideoImages_Images_ImageId",
                        column: x => x.ImageId,
                        principalTable: "Images",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VideoImages_Videos_VideoId",
                        column: x => x.VideoId,
                        principalTable: "Videos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "VideoStatisticSnapshots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ViewCount = table.Column<long>(type: "bigint", nullable: true),
                    LikeCount = table.Column<long>(type: "bigint", nullable: true),
                    DislikeCount = table.Column<long>(type: "bigint", nullable: true),
                    CommentCount = table.Column<long>(type: "bigint", nullable: true),
                    ValidAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    VideoId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VideoStatisticSnapshots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VideoStatisticSnapshots_Videos_VideoId",
                        column: x => x.VideoId,
                        principalTable: "Videos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "VideoTags",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Tag = table.Column<string>(type: "text", nullable: false),
                    ValidSince = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ValidUntil = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    VideoId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VideoTags", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VideoTags_Videos_VideoId",
                        column: x => x.VideoId,
                        principalTable: "Videos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Authors_Platform_IdOnPlatform",
                table: "Authors",
                columns: new[] { "Platform", "IdOnPlatform" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Captions_VideoId",
                table: "Captions",
                column: "VideoId");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_CreatorId",
                table: "Categories",
                column: "CreatorId");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_NameId",
                table: "Categories",
                column: "NameId");

            migrationBuilder.CreateIndex(
                name: "IX_EntityAccessPermissions_AuthorId",
                table: "EntityAccessPermissions",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_EntityAccessPermissions_UserId",
                table: "EntityAccessPermissions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_EntityAccessPermissions_VideoId",
                table: "EntityAccessPermissions",
                column: "VideoId");

            migrationBuilder.CreateIndex(
                name: "IX_StatusChangeEvents_AuthorId",
                table: "StatusChangeEvents",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_StatusChangeEvents_VideoId",
                table: "StatusChangeEvents",
                column: "VideoId");

            migrationBuilder.CreateIndex(
                name: "IX_VideoAuthors_AuthorId",
                table: "VideoAuthors",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_VideoAuthors_VideoId",
                table: "VideoAuthors",
                column: "VideoId");

            migrationBuilder.CreateIndex(
                name: "IX_VideoCategories_AssignedById",
                table: "VideoCategories",
                column: "AssignedById");

            migrationBuilder.CreateIndex(
                name: "IX_VideoCategories_CategoryId",
                table: "VideoCategories",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_VideoCategories_VideoId",
                table: "VideoCategories",
                column: "VideoId");

            migrationBuilder.CreateIndex(
                name: "IX_VideoFiles_VideoId",
                table: "VideoFiles",
                column: "VideoId");

            migrationBuilder.CreateIndex(
                name: "IX_VideoImages_ImageId",
                table: "VideoImages",
                column: "ImageId");

            migrationBuilder.CreateIndex(
                name: "IX_VideoImages_VideoId",
                table: "VideoImages",
                column: "VideoId");

            migrationBuilder.CreateIndex(
                name: "IX_Videos_DescriptionId",
                table: "Videos",
                column: "DescriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_Videos_TitleId",
                table: "Videos",
                column: "TitleId");

            migrationBuilder.CreateIndex(
                name: "IX_VideoStatisticSnapshots_VideoId",
                table: "VideoStatisticSnapshots",
                column: "VideoId");

            migrationBuilder.CreateIndex(
                name: "IX_VideoTags_VideoId",
                table: "VideoTags",
                column: "VideoId");

            migrationBuilder.AddForeignKey(
                name: "FK_AuthorImages_Authors_AuthorId",
                table: "AuthorImages",
                column: "AuthorId",
                principalTable: "Authors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AuthorImages_Images_ImageId",
                table: "AuthorImages",
                column: "ImageId",
                principalTable: "Images",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Authors_TextTranslationKeys_BioId",
                table: "Authors",
                column: "BioId",
                principalTable: "TextTranslationKeys",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AuthorStatisticSnapshots_Authors_AuthorId",
                table: "AuthorStatisticSnapshots",
                column: "AuthorId",
                principalTable: "Authors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RefreshTokens_AspNetUsers_UserId",
                table: "RefreshTokens",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TextTranslations_TextTranslationKeys_KeyId",
                table: "TextTranslations",
                column: "KeyId",
                principalTable: "TextTranslationKeys",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AuthorImages_Authors_AuthorId",
                table: "AuthorImages");

            migrationBuilder.DropForeignKey(
                name: "FK_AuthorImages_Images_ImageId",
                table: "AuthorImages");

            migrationBuilder.DropForeignKey(
                name: "FK_Authors_TextTranslationKeys_BioId",
                table: "Authors");

            migrationBuilder.DropForeignKey(
                name: "FK_AuthorStatisticSnapshots_Authors_AuthorId",
                table: "AuthorStatisticSnapshots");

            migrationBuilder.DropForeignKey(
                name: "FK_RefreshTokens_AspNetUsers_UserId",
                table: "RefreshTokens");

            migrationBuilder.DropForeignKey(
                name: "FK_TextTranslations_TextTranslationKeys_KeyId",
                table: "TextTranslations");

            migrationBuilder.DropTable(
                name: "Captions");

            migrationBuilder.DropTable(
                name: "EntityAccessPermissions");

            migrationBuilder.DropTable(
                name: "StatusChangeEvents");

            migrationBuilder.DropTable(
                name: "VideoAuthors");

            migrationBuilder.DropTable(
                name: "VideoCategories");

            migrationBuilder.DropTable(
                name: "VideoFiles");

            migrationBuilder.DropTable(
                name: "VideoImages");

            migrationBuilder.DropTable(
                name: "VideoStatisticSnapshots");

            migrationBuilder.DropTable(
                name: "VideoTags");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "Videos");

            migrationBuilder.DropIndex(
                name: "IX_Authors_Platform_IdOnPlatform",
                table: "Authors");

            migrationBuilder.DropColumn(
                name: "ValidAt",
                table: "AuthorStatisticSnapshots");

            migrationBuilder.CreateIndex(
                name: "IX_Authors_Platform_IdOnPlatform",
                table: "Authors",
                columns: new[] { "Platform", "IdOnPlatform" });

            migrationBuilder.AddForeignKey(
                name: "FK_AuthorImages_Authors_AuthorId",
                table: "AuthorImages",
                column: "AuthorId",
                principalTable: "Authors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AuthorImages_Images_ImageId",
                table: "AuthorImages",
                column: "ImageId",
                principalTable: "Images",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Authors_TextTranslationKeys_BioId",
                table: "Authors",
                column: "BioId",
                principalTable: "TextTranslationKeys",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AuthorStatisticSnapshots_Authors_AuthorId",
                table: "AuthorStatisticSnapshots",
                column: "AuthorId",
                principalTable: "Authors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RefreshTokens_AspNetUsers_UserId",
                table: "RefreshTokens",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TextTranslations_TextTranslationKeys_KeyId",
                table: "TextTranslations",
                column: "KeyId",
                principalTable: "TextTranslationKeys",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
