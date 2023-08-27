using Base.Domain;
using Domain.Enums;

namespace Domain.Entities;

public class Image : AbstractIdDatabaseEntity
{
    public EPlatform Platform { get; set; }
    public string? IdOnPlatform { get; set; }

    public string? Key { get; set; }
    public string? Quality { get; set; }
    public string? Ext { get; set; }
    public string? Url { get; set; }
    public string? LocalFilePath { get; set; }
    public string? Etag { get; set; }
    public string? Hash { get; set; }
    public int? Width { get; set; }
    public int? Height { get; set; }
}