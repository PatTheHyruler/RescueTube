using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.EF.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class MinorFixes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NormalizedTag",
                table: "VideoTags",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Preference",
                table: "VideoImages",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Ext",
                table: "Images",
                type: "text",
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "BioId",
                table: "Authors",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NormalizedTag",
                table: "VideoTags");

            migrationBuilder.DropColumn(
                name: "Preference",
                table: "VideoImages");

            migrationBuilder.DropColumn(
                name: "Ext",
                table: "Images");

            migrationBuilder.AlterColumn<Guid>(
                name: "BioId",
                table: "Authors",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);
        }
    }
}
