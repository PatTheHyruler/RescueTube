using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RescueTube.DAL.EF.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class DataFetchOverhaul : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create DataFetchResults table
            migrationBuilder.CreateTable(
                name: "DataFetchResults",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DataFetchId = table.Column<Guid>(type: "uuid", nullable: false),
                    VideoId = table.Column<Guid>(type: "uuid", nullable: true),
                    AuthorId = table.Column<Guid>(type: "uuid", nullable: true),
                    PlaylistId = table.Column<Guid>(type: "uuid", nullable: true),
                    CommentId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataFetchResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DataFetchResults_Authors_AuthorId",
                        column: x => x.AuthorId,
                        principalTable: "Authors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DataFetchResults_Comments_CommentId",
                        column: x => x.CommentId,
                        principalTable: "Comments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DataFetchResults_DataFetches_DataFetchId",
                        column: x => x.DataFetchId,
                        principalTable: "DataFetches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DataFetchResults_Playlists_PlaylistId",
                        column: x => x.PlaylistId,
                        principalTable: "Playlists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DataFetchResults_Videos_VideoId",
                        column: x => x.VideoId,
                        principalTable: "Videos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            // Migrate DataFetches that should've been DataFetchResults

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:pgcrypto", ",,");

            // Add uuid_generate_v7 function
            migrationBuilder.Sql("""
                -- uuid_generate_v7 function copied from: https://github.com/Betterment/postgresql-uuid-generate-v7
                -- LICENSE:
                -- Copyright (c) 2023 Betterment Holdings Inc.
                -- Copyright (c) 2023 Kyle Hubert <kjmph@users.noreply.github.com> (https://github.com/kjmph)
                -- 
                -- Permission is hereby granted, free of charge, to any person obtaining a copy
                -- of this software and associated documentation files (the "Software"), to deal
                -- in the Software without restriction, including without limitation the rights
                -- to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
                -- copies of the Software, and to permit persons to whom the Software is
                -- furnished to do so, subject to the following conditions:
                -- 
                -- The above copyright notice and this permission notice shall be included in all
                -- copies or substantial portions of the Software.
                -- 
                -- THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
                -- IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
                -- FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
                -- AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
                -- LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
                -- OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
                -- SOFTWARE.

                CREATE OR REPLACE FUNCTION
                  uuid_generate_v7()
                RETURNS
                  uuid
                LANGUAGE
                  plpgsql
                PARALLEL SAFE
                AS $$
                  DECLARE
                    -- The current UNIX timestamp in milliseconds
                    unix_time_ms CONSTANT bytea NOT NULL DEFAULT substring(int8send((extract(epoch FROM clock_timestamp()) * 1000)::bigint) from 3);
                
                    -- The buffer used to create the UUID, starting with the UNIX timestamp and followed by random bytes
                    buffer                bytea NOT NULL DEFAULT unix_time_ms || gen_random_bytes(10);
                  BEGIN
                    -- Set most significant 4 bits of 7th byte to 7 (for UUID v7), keeping the last 4 bits unchanged
                    buffer = set_byte(buffer, 6, (b'0111' || get_byte(buffer, 6)::bit(4))::bit(8)::int);
                
                    -- Set most significant 2 bits of 9th byte to 2 (the UUID variant specified in RFC 4122), keeping the last 6 bits unchanged
                    buffer = set_byte(buffer, 8, (b'10'   || get_byte(buffer, 8)::bit(6))::bit(8)::int);
                
                    RETURN encode(buffer, 'hex');
                  END
                $$
                ;
                """);
            
            // Insert DataFetchResults for obvious direct matches
            migrationBuilder.Sql("""
                INSERT INTO "DataFetchResults" ("Id", "DataFetchId", "VideoId", "AuthorId", "PlaylistId")
                SELECT uuid_generate_v7(), df."Id", df."VideoId", df."AuthorId", df."PlaylistId"
                FROM "DataFetches" df
                WHERE df."Success" AND (
                    (df."VideoId" IS NOT NULL AND df."Type" IN ('videopage', 'videofiledownload')) OR
                    (df."PlaylistId" IS NOT NULL AND df."Type" = 'playlist') OR
                    (df."AuthorId" IS NOT NULL AND df."Type" IN ('channel', 'channelvideos'))
                )
                ;
                """);

            // Insert DataFetchResults for videos fetched by channel
            migrationBuilder.Sql("""
                insert into "DataFetchResults" ("Id", "DataFetchId", "VideoId")
                select uuid_generate_v7(), df."Id", v."Id"
                from "DataFetches" df
                join "DataFetches" sf
                	on df."Type" = sf."Type"
                	and df."Id" != sf."Id"
                	and sf."OccurredAt" between df."OccurredAt" and df."OccurredAt" + interval '2 minutes'
                join "Videos" v on v."Id" = sf."VideoId" and exists (select 1 from "VideoAuthors" va where va."AuthorId" = df."AuthorId" and va."VideoId" = v."Id")
                where df."Type" = 'channelvideos' and df."AuthorId" is not null
                group by df."Id", df."AuthorId", v."Id"
                order by MIN(sf."OccurredAt")
                ;
                """);

            // Insert DataFetchResults for channel data included in separate video fetches
            migrationBuilder.Sql("""
                insert into "DataFetchResults" ("Id", "DataFetchId", "AuthorId")
                select uuid_generate_v7(), df."Id", a."Id"
                from "DataFetches" df
                join "DataFetches" sf
                	on df."Type" = sf."Type"
                	and df."Id" != sf."Id"
                	and sf."OccurredAt" between df."OccurredAt" and df."OccurredAt" + interval '1 minutes'
                join "Authors" a on a."Id" = sf."AuthorId" and exists (select 1 from "VideoAuthors" va where va."AuthorId" = a."Id" and va."VideoId" = df."VideoId")
                where df."Type" = 'videopage' and df."VideoId" is not null
                group by df."Id", df."AuthorId", a."Id"
                order by MIN(sf."OccurredAt")
                ;
                """);

            // Insert DataFetchResults for video data added from a playlist fetch
            migrationBuilder.Sql("""
                insert into "DataFetchResults" ("Id", "DataFetchId", "VideoId")
                select uuid_generate_v7(), df."Id", v."Id"
                from "DataFetches" df
                join "DataFetches" sf
                	on df."Type" = sf."Type"
                	and df."Id" != sf."Id"
                	and sf."OccurredAt" between df."OccurredAt" and df."OccurredAt" + interval '10 minutes'
                join "Videos" v on v."Id" = sf."VideoId" and exists (select 1 from "PlaylistItems" pi where pi."VideoId" = v."Id" and pi."PlaylistId" = df."PlaylistId")
                where df."Type" = 'playlist' and df."PlaylistId" is not null
                group by df."Id", v."Id"
                order by MIN(sf."OccurredAt")
                ;
                """);

            // Insert DataFetchResults for author data added from a playlist fetch
            migrationBuilder.Sql("""
                insert into "DataFetchResults" ("Id", "DataFetchId", "AuthorId")
                select uuid_generate_v7(), df."Id", a."Id"
                from "DataFetches" df
                join "DataFetches" sf
                	on df."Type" = sf."Type"
                	and df."Id" != sf."Id"
                	and sf."OccurredAt" between df."OccurredAt" and df."OccurredAt" + interval '10 minutes'
                join "Authors" a on a."Id" = sf."AuthorId" and (
                	exists (
                		select 1
                		from "VideoAuthors" va
                		join "PlaylistItems" pi on pi."VideoId" = va."VideoId"
                		where va."AuthorId" = a."Id" and pi."PlaylistId" = df."PlaylistId"
                	)
                	or exists (
                		select 1 from "Playlists" p
                		where p."CreatorId" = a."Id"
                	)
                )
                where df."Type" = 'playlist' and df."PlaylistId" is not null
                group by df."Id", a."Id"
                order by MIN(sf."OccurredAt")
                ;
                """);

            // Insert DataFetchResults for comment data added from a video comments fetch
            migrationBuilder.Sql("""
                insert into "DataFetchResults" ("Id", "DataFetchId", "CommentId")
                select uuid_generate_v7(), df."Id", c."Id"
                from "DataFetches" df
                join "DataFetches" sf
                	on df."Type" = sf."Type"
                	and df."Id" != sf."Id"
                	and sf."OccurredAt" between df."OccurredAt" and df."OccurredAt" + interval '30 minutes'
                join "Comments" c on c."Id" = sf."CommentId" and c."VideoId" = df."VideoId"
                where df."Type" = 'comments' and df."VideoId" is not null
                group by df."Id", c."Id"
                order by MIN(sf."OccurredAt")
                ;
                """);

            // Skipping comment author results, something is wrong with their state, can't migrate

            // Also skipping general videoauthor results, no idea what those even were

            // Create DataFetchResults indexes
            migrationBuilder.CreateIndex(
                name: "IX_DataFetchResults_AuthorId",
                table: "DataFetchResults",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_DataFetchResults_CommentId",
                table: "DataFetchResults",
                column: "CommentId");

            migrationBuilder.CreateIndex(
                name: "IX_DataFetchResults_DataFetchId",
                table: "DataFetchResults",
                column: "DataFetchId");

            migrationBuilder.CreateIndex(
                name: "IX_DataFetchResults_PlaylistId",
                table: "DataFetchResults",
                column: "PlaylistId");

            migrationBuilder.CreateIndex(
                name: "IX_DataFetchResults_VideoId",
                table: "DataFetchResults",
                column: "VideoId");

            // -------------------------------------------------------------------

            // DataFetches updates

            // Replace Success with Status

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "DataFetches",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql("""
                                 UPDATE "DataFetches"
                                 SET "Status" =
                                     CASE
                                        WHEN "Success" THEN 'Succeeded'
                                        ELSE 'Failed'
                                     END
                                 ;
                                 """);

            // Remaining autogenerated operations

            migrationBuilder.DropForeignKey(
                name: "FK_DataFetches_Comments_CommentId",
                table: "DataFetches");

            migrationBuilder.DropIndex(
                name: "IX_DataFetches_CommentId",
                table: "DataFetches");

            migrationBuilder.DropColumn(
                name: "CommentId",
                table: "DataFetches");

            migrationBuilder.DropColumn(
                name: "ShouldAffectValidity",
                table: "DataFetches");

            migrationBuilder.DropColumn(
                name: "Success",
                table: "DataFetches");

            migrationBuilder.RenameColumn(
                name: "OccurredAt",
                table: "DataFetches",
                newName: "StartedAt");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastHeartbeatReceivedAt",
                table: "DataFetches",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Platform",
                table: "DataFetches",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "StatusUpdatedAt",
                table: "DataFetches",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            throw new NotImplementedException("Down migration for datafetch overhaul not implemented");

#pragma warning disable CS0162 // Unreachable code detected
            migrationBuilder.DropTable(
                name: "DataFetchResults");

            migrationBuilder.DropColumn(
                name: "LastHeartbeatReceivedAt",
                table: "DataFetches");

            migrationBuilder.DropColumn(
                name: "Platform",
                table: "DataFetches");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "DataFetches");

            migrationBuilder.DropColumn(
                name: "StatusUpdatedAt",
                table: "DataFetches");

            migrationBuilder.RenameColumn(
                name: "StartedAt",
                table: "DataFetches",
                newName: "OccurredAt");

            migrationBuilder.AlterDatabase()
                .OldAnnotation("Npgsql:PostgresExtension:pgcrypto", ",,");

            migrationBuilder.AddColumn<Guid>(
                name: "CommentId",
                table: "DataFetches",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ShouldAffectValidity",
                table: "DataFetches",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Success",
                table: "DataFetches",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_DataFetches_CommentId",
                table: "DataFetches",
                column: "CommentId");

            migrationBuilder.AddForeignKey(
                name: "FK_DataFetches_Comments_CommentId",
                table: "DataFetches",
                column: "CommentId",
                principalTable: "Comments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
#pragma warning restore CS0162 // Unreachable code detected
        }
    }
}
