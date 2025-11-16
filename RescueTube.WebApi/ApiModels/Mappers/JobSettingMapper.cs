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
            IsEnabled = src.JobSettings.IsEnabled,
            Cron = src.JobSettings.Cron,
            DataFetchJobSettings = src.JobSettings.DataFetchJobSettings?.MapToDataFetchJobSettingsDtoV1(),
        };

    private static DataFetchJobSettingsDtoV1 MapToDataFetchJobSettingsDtoV1(this DataFetchJobSettings src)
        => new()
        {
            SuccessCutoffOffset = src.SuccessCutoffOffset,
            FailureCutoffOffset = src.FailureCutoffOffset,
        };
}