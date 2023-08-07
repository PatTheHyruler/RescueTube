using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace DAL.EF.Converters;

public class DateTimeConverter : ValueConverter<DateTime, DateTime>
{
    public DateTimeConverter() : base(
        dt => dt.ToUniversalTime(),
        dt => DateTime.SpecifyKind(dt, DateTimeKind.Utc))
    {
    }
}