namespace RescueTube.Domain;

public readonly record struct DataSize : IComparable<DataSize>
{
    private const long BytesInKibibyte = 1024;
    private const long BytesInMebibyte = BytesInKibibyte * 1024;
    private const long BytesInGibibyte = BytesInMebibyte * 1024;
    private const long BytesInTebibyte = BytesInGibibyte * 1024;
    private const long BytesInPebibyte = BytesInTebibyte * 1024;

    private const long BytesInKilobyte = 1000;
    private const long BytesInMegabyte = BytesInKilobyte * 1000;
    private const long BytesInGigabyte = BytesInMegabyte * 1000;
    private const long BytesInTerabyte = BytesInGigabyte * 1000;
    private const long BytesInPetabyte = BytesInTerabyte * 1000;

    public long TotalBytes { get; }

    public DataSize(long totalBytes)
    {
        TotalBytes = totalBytes;
    }

    public static DataSize FromBytes(long bytes) => new(bytes);

    public static DataSize FromKibibytes(double kibibytes) => new((long)(kibibytes * BytesInKibibyte));
    public static DataSize FromMebibytes(double mebibytes) => new((long)(mebibytes * BytesInMebibyte));
    public static DataSize FromGibibytes(double gibibytes) => new((long)(gibibytes * BytesInGibibyte));
    public static DataSize FromTebibytes(double tebibytes) => new((long)(tebibytes * BytesInTebibyte));
    public static DataSize FromPebibytes(double pebibytes) => new((long)(pebibytes * BytesInPebibyte));

    public static DataSize FromKilobytes(double kilobytes) => new((long)(kilobytes * BytesInKilobyte));
    public static DataSize FromMegabytes(double megabytes) => new((long)(megabytes * BytesInMegabyte));
    public static DataSize FromGigabytes(double gigabytes) => new((long)(gigabytes * BytesInGigabyte));
    public static DataSize FromTerabytes(double terabytes) => new((long)(terabytes * BytesInTerabyte));
    public static DataSize FromPetabytes(double petabytes) => new((long)(petabytes * BytesInPetabyte));

    public double TotalKibibytes => (double)TotalBytes / BytesInKibibyte;
    public double TotalMebibytes => (double)TotalBytes / BytesInMebibyte;
    public double TotalGibibytes => (double)TotalBytes / BytesInGibibyte;
    public double TotalTebibytes => (double)TotalBytes / BytesInTebibyte;
    public double TotalPebibytes => (double)TotalBytes / BytesInPebibyte;

    public double TotalKilobytes => (double)TotalBytes / BytesInKilobyte;
    public double TotalMegabytes => (double)TotalBytes / BytesInMegabyte;
    public double TotalGigabytes => (double)TotalBytes / BytesInGigabyte;
    public double TotalTerabytes => (double)TotalBytes / BytesInTerabyte;
    public double TotalPetabytes => (double)TotalBytes / BytesInPetabyte;

    public static DataSize operator +(DataSize left, DataSize right) => new(left.TotalBytes + right.TotalBytes);
    public static DataSize operator -(DataSize left, DataSize right) => new(left.TotalBytes - right.TotalBytes);
    public static DataSize operator *(DataSize left, double multiplier) => new((long)(left.TotalBytes * multiplier));
    public static DataSize operator /(DataSize left, double divisor) => new((long)(left.TotalBytes / divisor));
    public static bool operator <(DataSize left, DataSize right) => left.TotalBytes < right.TotalBytes;
    public static bool operator >(DataSize left, DataSize right) => left.TotalBytes > right.TotalBytes;
    public static bool operator <=(DataSize left, DataSize right) => left.TotalBytes <= right.TotalBytes;
    public static bool operator >=(DataSize left, DataSize right) => left.TotalBytes >= right.TotalBytes;

    public static implicit operator long(DataSize size) => size.TotalBytes;
    public static implicit operator DataSize(long bytes) => new(bytes);

    public int CompareTo(DataSize other) => TotalBytes.CompareTo(other.TotalBytes);

    public string ToStringBinary()
    {
        return TotalBytes switch
        {
            < BytesInKibibyte => $"{TotalBytes} B",
            < BytesInMebibyte => $"{TotalKibibytes:F2} KiB",
            < BytesInGibibyte => $"{TotalMebibytes:F2} MiB",
            < BytesInTebibyte => $"{TotalGibibytes:F2} GiB",
            < BytesInPebibyte => $"{TotalTebibytes:F2} TiB",
            _ => $"{TotalPebibytes:F2} PiB",
        };
    }

    public string ToStringDecimal()
    {
        return TotalBytes switch
        {
            < BytesInKilobyte => $"{TotalBytes} B",
            < BytesInMegabyte => $"{TotalKilobytes:F2} kB",
            < BytesInGigabyte => $"{TotalMegabytes:F2} MB",
            < BytesInTerabyte => $"{TotalGigabytes:F2} GB",
            < BytesInPetabyte => $"{TotalTerabytes:F2} TB",
            _ => $"{TotalPetabytes:F2} PB",
        };
    }

    public override string ToString() => ToStringBinary();

    public string ToString(SizeUnit unit)
    {
        return unit switch
        {
            SizeUnit.Byte => $"{TotalBytes} B",

            SizeUnit.Kibibyte => $"{TotalKibibytes:F2} KiB",
            SizeUnit.Mebibyte => $"{TotalMebibytes:F2} MiB",
            SizeUnit.Gibibyte => $"{TotalGibibytes:F2} GiB",
            SizeUnit.Tebibyte => $"{TotalTebibytes:F2} TiB",
            SizeUnit.Pebibyte => $"{TotalPebibytes:F2} PiB",

            SizeUnit.Kilobyte => $"{TotalKilobytes:F2} kB",
            SizeUnit.Megabyte => $"{TotalMegabytes:F2} MB",
            SizeUnit.Gigabyte => $"{TotalGigabytes:F2} GB",
            SizeUnit.Terabyte => $"{TotalTerabytes:F2} TB",
            SizeUnit.Petabyte => $"{TotalPetabytes:F2} TB",

            _ => throw new ArgumentOutOfRangeException(nameof(unit), unit, "Unsupported unit"),
        };
    }

    public enum SizeUnit
    {
        Byte,

        Kilobyte,
        Megabyte,
        Gigabyte,
        Terabyte,
        Petabyte,

        Kibibyte,
        Mebibyte,
        Gibibyte,
        Tebibyte,
        Pebibyte,
    }
}