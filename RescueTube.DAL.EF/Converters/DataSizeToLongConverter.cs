using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using RescueTube.Domain;

namespace RescueTube.DAL.EF.Converters;

public class DataSizeToLongConverter : ValueConverter<DataSize, long>
{
    public DataSizeToLongConverter() : base(
        dataSize => dataSize.TotalBytes,
        totalBytes => DataSize.FromBytes(totalBytes)
    )
    {
    }
}