using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RescueTube.DAL.EF.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class InfoJsonAsJsonB : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "InfoJson",
                table: "Videos",
                type: "jsonb USING \"InfoJson\"::jsonb",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "InfoJson",
                table: "Videos",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "jsonb",
                oldNullable: true);
        }
    }
}
