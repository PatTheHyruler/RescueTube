namespace Public.DTO.v1;

/// <summary>
/// Possible values for an archive entity's platform of origin.
/// </summary>
public enum EPlatform
{
    /// <summary>
    /// Local to this archive.
    /// </summary>
    /// <remarks>
    /// Applies to videos uploaded directly to the archive,
    /// playlists created within the archive,
    /// authors belonging to archive users...
    /// </remarks>
    Local,
    /// <summary>
    /// Any platform that isn't explicitly supported by the archive,
    /// and isn't the archive itself.
    /// </summary>
    /// <remarks>
    /// This will be used rarely, if ever.
    /// Most/all entities will belong either to a supported external platform, or to the archive itself.
    /// </remarks>
    Other,
    /// <summary>
    /// The <a href="https://www.youtube.com">YouTube</a> online video platform.
    /// </summary>
    YouTube,
}