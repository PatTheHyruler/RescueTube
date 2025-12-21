using RescueTube.Core.DTO;
using RescueTube.Domain.Entities;

namespace RescueTube.WebApi.ApiModels.Mappers;

public static class JobSettingMapper
{
    public static JobSettingsDtoV1 MapToJobSettingsDtoV1(this JobDefinitionWithSettings src)
        => new()
        {
            JobId = src.JobDefinition.JobId,
            IsArchivalJob = src.JobDefinition.IsArchivalJob,
            IsDefault = src.JobSettings.IsDefault,

            IsEnabled = src.JobSettings.IsEnabled,
            Cron = src.JobSettings.Cron,
            DataFetchJobSettings = src.JobSettings.DataFetchJobSettings?.MapToDataFetchJobSettingsDtoV1(),
            DefaultSettings = src.JobDefinition.DefaultSettings.MapToSimpleJobSettingsDtoV1(),
        };

    private static SimpleJobSettingsDtoV1 MapToSimpleJobSettingsDtoV1(this JobSettings src)
        => new()
        {
            JobId = src.JobId,
            IsDefault = true,
            IsEnabled = src.IsEnabled,
            Cron = src.Cron,
            DataFetchJobSettings = src.DataFetchJobSettings?.MapToDataFetchJobSettingsDtoV1(),
        };

    private static DataFetchJobSettingsDtoV1 MapToDataFetchJobSettingsDtoV1(this DataFetchJobSettings src)
        => new()
        {
            SuccessCutoffOffset = src.SuccessCutoffOffset,
            FailureCutoffOffset = src.FailureCutoffOffset,
        };
}