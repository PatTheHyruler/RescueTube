using System.Data.Common;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Options;
using RescueTube.Core;
using RescueTube.Core.Utils;
using RescueTube.DAL.EF.MigrationUtils;

namespace RescueTube.DAL.EF.Postgres.Migrations
{
    [DataMigration(typeof(ImageFilePathsDataMigration))]
    /// <inheritdoc />
    public partial class ImageFilePathsFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }

    public class ImageFilePathsDataMigration : IDataMigration
    {
        private readonly AppPaths _appPaths;

        public ImageFilePathsDataMigration(AppPaths appPaths)
        {
            _appPaths = appPaths;
        }

        private static string? GetStringOrNull(DbDataReader dataReader, int ordinal)
        {
            if (!dataReader.IsDBNull(ordinal))
            {
                return dataReader.GetString(ordinal);
            }

            return string.Empty;
        }

        public async Task MigrateAsync(DbConnection dbConnection)
        {
            List<(Guid Id, string LocalFilePath, string? Ext)> parameters = [];
            
            using (var queryCommand = dbConnection.CreateCommand())
            {
                const string query =
                    "SELECT i.\"Id\", i.\"LocalFilePath\", i.\"Ext\" FROM \"Images\" i WHERE i.\"LocalFilePath\" NOTNULL;";
                queryCommand.CommandText = query;

                using (var dataReader = await queryCommand.ExecuteReaderAsync())
                {
                    while (await dataReader.ReadAsync())
                    {
                        var id = dataReader.GetGuid(0);
                        var localFilePath = dataReader.GetString(1);
                        var ext = GetStringOrNull(dataReader, 2);

                        var newExt = ext?.ToFileNameSanitized();

                        var directory = Path.GetDirectoryName(localFilePath)
                                        ?? throw new Exception($"Failed to get directory for path '{localFilePath}'");
                        var fileName = Path.GetFileName(localFilePath);
                        var sanitizedFileName = fileName.ToFileNameSanitized();
                        if (string.IsNullOrWhiteSpace(sanitizedFileName))
                        {
                            sanitizedFileName = Guid.NewGuid().ToString().ToFileNameSanitized();
                            if (!string.IsNullOrWhiteSpace(newExt))
                            {
                                sanitizedFileName += $".{ext}";
                            }
                        }

                        var newLocalFilePath = Path.Combine(directory, sanitizedFileName);
                        parameters.Add((id, newLocalFilePath, newExt));

                        try
                        {
                            if (File.Exists(_appPaths.GetAbsolutePathFromDownloads(localFilePath)))
                            {
                                File.Move(_appPaths.GetAbsolutePathFromDownloads(localFilePath), 
                                    _appPaths.GetAbsolutePathFromDownloads(newLocalFilePath));
                            }
                            else
                            {
                                Console.WriteLine(
                                    $"{nameof(ImageFilePathsDataMigration)}: '{localFilePath}' not found, skipping rename");
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(
                                $"{nameof(ImageFilePathsDataMigration)}: Failed to rename '{localFilePath}' to '${newLocalFilePath}': ${e.Message}");
                        }
                    }
                }
            }

            using var updateCommand = dbConnection.CreateCommand();
            const string updateQuery =
                "UPDATE \"Images\" SET \"LocalFilePath\" = @LocalFilePath, \"Ext\" = @Ext WHERE \"Id\" = @Id;";
            updateCommand.CommandText = updateQuery;

            var idParameter = updateCommand.CreateParameter();
            idParameter.ParameterName = "@Id";
            var filePathParameter = updateCommand.CreateParameter();
            filePathParameter.ParameterName = "@LocalFilePath";
            var extParameter = updateCommand.CreateParameter();
            extParameter.ParameterName = "@Ext";

            updateCommand.Parameters.Add(idParameter);
            updateCommand.Parameters.Add(filePathParameter);
            updateCommand.Parameters.Add(extParameter);

            foreach (var (id, localFilePath, ext) in parameters)
            {
                idParameter.Value = id;
                filePathParameter.Value = localFilePath;
                extParameter.Value = ext;

                await updateCommand.ExecuteNonQueryAsync();
            }
        }
    }
}
