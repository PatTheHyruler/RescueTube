using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RescueTube.DAL.EF.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class ClarifyDataFetchStartedSemantics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "OccurredAt",
                table: "DataFetches",
                newName: "StartedAt");

            migrationBuilder.Sql("""UPDATE "DataFetches" SET "Status" = 'Started' WHERE "Status" = 'Starting';""");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""UPDATE "DataFetches" SET "Status" = 'Starting' WHERE "Status" = 'Started';""");

            migrationBuilder.RenameColumn(
                name: "StartedAt",
                table: "DataFetches",
                newName: "OccurredAt");
        }
    }
}
