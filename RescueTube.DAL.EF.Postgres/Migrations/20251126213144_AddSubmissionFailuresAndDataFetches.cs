using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RescueTube.DAL.EF.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class AddSubmissionFailuresAndDataFetches : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "SubmissionId",
                table: "DataFetches",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SubmissionHandlingFailures",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SubmissionId = table.Column<Guid>(type: "uuid", nullable: false),
                    OccurredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Reason = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubmissionHandlingFailures", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubmissionHandlingFailures_Submissions_SubmissionId",
                        column: x => x.SubmissionId,
                        principalTable: "Submissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DataFetches_SubmissionId",
                table: "DataFetches",
                column: "SubmissionId");

            migrationBuilder.CreateIndex(
                name: "IX_SubmissionHandlingFailures_SubmissionId",
                table: "SubmissionHandlingFailures",
                column: "SubmissionId");

            migrationBuilder.AddForeignKey(
                name: "FK_DataFetches_Submissions_SubmissionId",
                table: "DataFetches",
                column: "SubmissionId",
                principalTable: "Submissions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DataFetches_Submissions_SubmissionId",
                table: "DataFetches");

            migrationBuilder.DropTable(
                name: "SubmissionHandlingFailures");

            migrationBuilder.DropIndex(
                name: "IX_DataFetches_SubmissionId",
                table: "DataFetches");

            migrationBuilder.DropColumn(
                name: "SubmissionId",
                table: "DataFetches");
        }
    }
}
