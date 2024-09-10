namespace RescueTube.Core.Utils;

public static class DataFormatUtils
{
    public static HumanReadableDataAmount GetHumanReadableFormat(double valueBytes)
    {
        if (valueBytes == 0) return new HumanReadableDataAmount(0, SizeUnit.B);

        var order = (int)Math.Log(valueBytes, 1024);
        var adjustedSize = valueBytes / Math.Pow(1024, order);

        // Cast the order directly to the SizeUnit enum
        return new HumanReadableDataAmount(adjustedSize, (SizeUnit)order);
    }

    public record HumanReadableDataAmount(double Value, SizeUnit Unit)
    {
        public string UnitString => Unit.ToString();

        public double Rounded(int decimalPlaces = 2) => Math.Round(Value, decimalPlaces);

        public override string ToString()
        {
            return $"{Rounded()} {UnitString}";
        }
    }

    public enum SizeUnit
    {
        B,
        KB,
        MB,
        GB,
        TB,
        PB,
        EB
    }
}