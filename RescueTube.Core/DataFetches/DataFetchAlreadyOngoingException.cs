namespace RescueTube.Core.DataFetches;

public class DataFetchAlreadyOngoingException(string message) : ApplicationException(message);