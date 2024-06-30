using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RescueTube.DAL.EF.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class PlaylistAccessPermission : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "PlaylistId",
                table: "EntityAccessPermissions",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_EntityAccessPermissions_PlaylistId",
                table: "EntityAccessPermissions",
                column: "PlaylistId");

            migrationBuilder.AddForeignKey(
                name: "FK_EntityAccessPermissions_Playlists_PlaylistId",
                table: "EntityAccessPermissions",
                column: "PlaylistId",
                principalTable: "Playlists",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EntityAccessPermissions_Playlists_PlaylistId",
                table: "EntityAccessPermissions");

            migrationBuilder.DropIndex(
                name: "IX_EntityAccessPermissions_PlaylistId",
                table: "EntityAccessPermissions");

            migrationBuilder.DropColumn(
                name: "PlaylistId",
                table: "EntityAccessPermissions");
        }
    }
}
