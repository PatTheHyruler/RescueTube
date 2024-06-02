using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace RescueTube.DAL.EF.Converters;

public class DateTimeOffsetToUtcConverter : ValueConverter<DateTimeOffset, DateTimeOffset>
{
    public DateTimeOffsetToUtcConverter() : base(
        from => from.ToUniversalTime(),
        to => to
    )
    {
    }
}