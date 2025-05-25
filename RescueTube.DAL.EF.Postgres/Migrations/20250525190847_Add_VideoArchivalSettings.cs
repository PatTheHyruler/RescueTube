using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RescueTube.DAL.EF.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class Add_VideoArchivalSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ArchivalSettings_ShouldRegularlyFetchVideoData",
                table: "Videos",
                type: "boolean",
                nullable: false,
                defaultValue: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ArchivalSettings_ShouldRegularlyFetchVideoData",
                table: "Videos");
        }
    }
}
