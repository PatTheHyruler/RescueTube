using System.Diagnostics.CodeAnalysis;

namespace RescueTube.Core.DataFetches;

public static class DataFetchExtensions
{
    public static void ThrowIfAlreadyFetching([NotNull] this DataFetchScope? dataFetchScope)
    {
        if (dataFetchScope is null)
        {
            throw new DataFetchAlreadyOngoingException("Data fetch already ongoing");
        }
    }
}