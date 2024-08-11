using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RescueTube.DAL.EF.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class VideoModelImprovements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LiveStatus",
                table: "Videos",
                type: "text",
                nullable: false,
                defaultValue: "None");

            migrationBuilder.Sql("UPDATE \"Videos\" SET \"LiveStatus\" = 'WasLive' WHERE \"IsLiveStreamRecording\"");

            migrationBuilder.DropColumn(
                name: "IsLiveStreamRecording",
                table: "Videos");

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "Videos",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Type",
                table: "Videos");

            migrationBuilder.AddColumn<bool>(
                name: "IsLiveStreamRecording",
                table: "Videos",
                type: "boolean",
                nullable: true);

            migrationBuilder.Sql(
                "UPDATE \"Videos\" SET \"IsLiveStreamRecording\" = true" +
                " WHERE \"LiveStatus\" IN ('IsLive', 'IsUpcoming', 'WasLive', 'PostLive');");

            migrationBuilder.Sql(
                "UPDATE \"Videos\" SET \"IsLiveStreamRecording\" = false WHERE \"LiveStatus\" IN ('NotLive');");

            migrationBuilder.DropColumn(
                name: "LiveStatus",
                table: "Videos");
        }
    }
}
