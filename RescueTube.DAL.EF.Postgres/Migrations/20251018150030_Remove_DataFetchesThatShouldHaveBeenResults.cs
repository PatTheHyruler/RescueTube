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
                                 WHERE "VideoId" IS NOT NULL AND "Type" NOT IN ('videopage', 'videofiledownload', 'comments')
                                 """);

            migrationBuilder.Sql("""
                                 DELETE FROM "DataFetches"
                                 WHERE "PlaylistId" IS NOT NULL AND "Type" NOT IN ('playlist')
                                 """);

            migrationBuilder.Sql("""
                                 DELETE FROM "DataFetches"
                                 WHERE "AuthorId" IS NOT NULL AND "Type" NOT IN ('channel', 'channelvideos')
                                 """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
