using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RescueTube.DAL.EF.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class ImageTypeDetection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ImageType",
                table: "AuthorImages",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<short>(
                name: "ImageTypeDetectionAttempts",
                table: "AuthorImages",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageTypeDetectionAttempts",
                table: "AuthorImages");

            migrationBuilder.AlterColumn<string>(
                name: "ImageType",
                table: "AuthorImages",
                type: "text",
                nullable: false,
                defaultValue: "ProfilePicture",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }
    }
}
