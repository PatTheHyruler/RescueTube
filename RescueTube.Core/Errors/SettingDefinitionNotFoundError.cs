namespace RescueTube.Core.Errors;

public record SettingDefinitionNotFoundError(string Key, Type? RequestedType);