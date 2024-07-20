using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RescueTube.DAL.EF.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class ImageResolutionUpdater : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Previously, resolution was set incorrectly for some images
            migrationBuilder.Sql("update \"Images\" set \"Width\" = null, \"Height\" = null");
            
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ResolutionParseAttemptedAt",
                table: "Images",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ResolutionParseAttemptedAt",
                table: "Images");
        }
    }
}
