using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RescueTube.DAL.EF.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class Remove_DataFetchesThatShouldHaveBeenResults : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DELETE FROM "DataFetches"
                WHERE "VideoIdOnPlatform" IS NULL AND "PlaylistIdOnPlatform" IS NULL AND "AuthorIdOnPlatform" IS NULL;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
