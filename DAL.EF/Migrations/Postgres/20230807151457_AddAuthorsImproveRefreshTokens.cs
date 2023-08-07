using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.EF.Migrations.Postgres
{
    /// <inheritdoc />
    public partial class AddAuthorsImproveRefreshTokens : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RefreshTokens_UserId",
                table: "RefreshTokens");

            migrationBuilder.DropColumn(
                name: "PreviousExpiresAt",
                table: "RefreshTokens");

            migrationBuilder.DropColumn(
                name: "PreviousRefreshToken",
                table: "RefreshTokens");

            migrationBuilder.RenameColumn(
                name: "RefreshToken",
                table: "RefreshTokens",
                newName: "Token");

            migrationBuilder.AlterColumn<string>(
                table: "RefreshTokens",
                name: "Token",
                oldType: "character varying(64)",
                type: "text",
                oldMaxLength: 64,
                maxLength: null);

            migrationBuilder.AddColumn<string>(
                name: "JwtHash",
                table: "RefreshTokens",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "Images",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Platform = table.Column<string>(type: "text", nullable: false),
                    IdOnPlatform = table.Column<string>(type: "text", nullable: true),
                    Key = table.Column<string>(type: "text", nullable: true),
                    Quality = table.Column<string>(type: "text", nullable: true),
                    Url = table.Column<string>(type: "text", nullable: true),
                    LocalFilePath = table.Column<string>(type: "text", nullable: true),
                    Etag = table.Column<string>(type: "text", nullable: true),
                    Hash = table.Column<string>(type: "text", nullable: true),
                    Width = table.Column<int>(type: "integer", nullable: true),
                    Height = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Images", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TextTranslationKeys",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TextTranslationKeys", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Authors",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserName = table.Column<string>(type: "text", nullable: true),
                    DisplayName = table.Column<string>(type: "text", nullable: true),
                    BioId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_Authors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Authors_TextTranslationKeys_BioId",
                        column: x => x.BioId,
                        principalTable: "TextTranslationKeys",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TextTranslations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    Culture = table.Column<string>(type: "text", nullable: true),
                    ValidSince = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ValidUntil = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    KeyId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TextTranslations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TextTranslations_TextTranslationKeys_KeyId",
                        column: x => x.KeyId,
                        principalTable: "TextTranslationKeys",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AuthorImages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ImageType = table.Column<string>(type: "text", nullable: false),
                    ValidSince = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ValidUntil = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastFetched = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AuthorId = table.Column<Guid>(type: "uuid", nullable: false),
                    ImageId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthorImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuthorImages_Authors_AuthorId",
                        column: x => x.AuthorId,
                        principalTable: "Authors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AuthorImages_Images_ImageId",
                        column: x => x.ImageId,
                        principalTable: "Images",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AuthorStatisticSnapshots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FollowerCount = table.Column<long>(type: "bigint", nullable: true),
                    PaidFollowerCount = table.Column<long>(type: "bigint", nullable: true),
                    AuthorId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthorStatisticSnapshots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuthorStatisticSnapshots_Authors_AuthorId",
                        column: x => x.AuthorId,
                        principalTable: "Authors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_UserId_Token_JwtHash_ExpiresAt",
                table: "RefreshTokens",
                columns: new[] { "UserId", "Token", "JwtHash", "ExpiresAt" });

            migrationBuilder.CreateIndex(
                name: "IX_AuthorImages_AuthorId",
                table: "AuthorImages",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_AuthorImages_ImageId",
                table: "AuthorImages",
                column: "ImageId");

            migrationBuilder.CreateIndex(
                name: "IX_Authors_BioId",
                table: "Authors",
                column: "BioId");

            migrationBuilder.CreateIndex(
                name: "IX_Authors_Platform_IdOnPlatform",
                table: "Authors",
                columns: new[] { "Platform", "IdOnPlatform" });

            migrationBuilder.CreateIndex(
                name: "IX_AuthorStatisticSnapshots_AuthorId",
                table: "AuthorStatisticSnapshots",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_TextTranslations_KeyId_Culture_ValidUntil_ValidSince",
                table: "TextTranslations",
                columns: new[] { "KeyId", "Culture", "ValidUntil", "ValidSince" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuthorImages");

            migrationBuilder.DropTable(
                name: "AuthorStatisticSnapshots");

            migrationBuilder.DropTable(
                name: "TextTranslations");

            migrationBuilder.DropTable(
                name: "Images");

            migrationBuilder.DropTable(
                name: "Authors");

            migrationBuilder.DropTable(
                name: "TextTranslationKeys");

            migrationBuilder.DropIndex(
                name: "IX_RefreshTokens_UserId_Token_JwtHash_ExpiresAt",
                table: "RefreshTokens");

            migrationBuilder.DropColumn(
                name: "JwtHash",
                table: "RefreshTokens");
            
            migrationBuilder.RenameColumn(
                name: "Token",
                table: "RefreshTokens",
                newName: "RefreshToken");

            migrationBuilder.AlterColumn<string>(
                table: "RefreshTokens",
                name: "RefreshToken",
                type: "character varying(64)",
                oldType: "text",
                oldMaxLength: null,
                maxLength: 64);

            migrationBuilder.AddColumn<DateTime>(
                name: "PreviousExpiresAt",
                table: "RefreshTokens",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PreviousRefreshToken",
                table: "RefreshTokens",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RefreshToken",
                table: "RefreshTokens",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_UserId",
                table: "RefreshTokens",
                column: "UserId");
        }
    }
}
