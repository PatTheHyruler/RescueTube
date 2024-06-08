namespace RescueTube.Core.Utils;

public static class PathUtils
{
    public static string GetFilePathWithoutExtension(string filePath)
    {
        var filePathSpan = filePath.AsSpan();
        var length = filePathSpan.LastIndexOf('.');
        return length >= 0 ? filePathSpan[..length].ToString() : filePath;
    }
    
    public static string MakeRelativeFilePath(string filePath, string? directory = null)
    {
        var fileUri = new Uri(filePath);
        directory ??= Directory.GetCurrentDirectory();
        var referenceUri = new Uri(directory);
        var relativePath = Uri.UnescapeDataString(referenceUri.MakeRelativeUri(fileUri).ToString());

        var directoryName = Path.GetFileName(directory);
        if (relativePath.StartsWith(directoryName))
        {
            relativePath = relativePath[directoryName.Length..];
            while (relativePath.StartsWith('/'))
            {
                relativePath = relativePath[1..];
            }
        }

        return relativePath.Replace('/', Path.DirectorySeparatorChar);
    }
    
    public static string? GuessImageFileExtensionFromMediaType(string? mediaType) =>
        mediaType switch
        {
            "image/gif" => "gif",
            "image/tiff" => "tiff",
            "image/jpeg" => "jpg",
            "image/svg+xml" => "svg",
            "image/png" => "png",
            "image/x-icon" => "ico",
            _ => null,
        };
    
    public static string ToFileNameSanitized(this string str, int? maxLength = null)
    {
        char[] allowedPunctuationChars = ['.', '-', '_'];
        str = new string(
            str.ToCharArray()
                .Where(c => char.IsAsciiLetterOrDigit(c)
                            || allowedPunctuationChars.Contains(c))
                .ToArray()
        );

        if (maxLength == null || str.Length <= maxLength)
        {
            return str;
        }

        return str[..maxLength.Value];
    }
}