using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RescueTube.DAL.EF.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class AddDataFetchSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<TimeSpan>(
                name: "DataFetchJobSettings_FailureCutoffOffset",
                table: "JobSettings",
                type: "interval",
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "DataFetchJobSettings_SuccessCutoffOffset",
                table: "JobSettings",
                type: "interval",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DataFetchJobSettings_FailureCutoffOffset",
                table: "JobSettings");

            migrationBuilder.DropColumn(
                name: "DataFetchJobSettings_SuccessCutoffOffset",
                table: "JobSettings");
        }
    }
}
