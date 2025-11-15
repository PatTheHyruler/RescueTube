using Hangfire;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RescueTube.Core.Data;
using RescueTube.Core.Data.Pagination;
using RescueTube.Core.Identity;
using RescueTube.Core.JobOrchestration;
using RescueTube.Core.Jobs;
using RescueTube.Core.Utils;
using RescueTube.Core.Utils.Pagination;
using RescueTube.Domain.Enums;
using RescueTube.WebApi.ApiModels;
using RescueTube.WebApi.ApiModels.Mappers;

namespace RescueTube.WebApi.Endpoints;

public static class DataFetchEndpoints
{
    public static void MapDataFetchEndpoints(this IEndpointRouteBuilder app)
    {
        var dataFetchesGroup = app.MapGroup("data-fetches").WithTags("Data Fetches");

        dataFetchesGroup.MapGet("", GetDataFetchesAsync)
            .RequireAuthorization(p => p.RequireRole(RoleNames.AdminRoles))
            .HasApiVersion(1);

        dataFetchesGroup.MapPost("jobs/enqueue", EnqueueDataFetchJob)
            .RequireAuthorization(p => p.RequireRole(RoleNames.AdminRoles))
            .HasApiVersion(1);

        dataFetchesGroup.MapGet("jobs/definitions", GetDataFetchJobDefinitionsAsync)
            .RequireAuthorization(p => p.RequireRole(RoleNames.AdminRoles))
            .HasApiVersion(1);
    }

    private static async Task<Ok<DataFetchesResponseDtoV1>> GetDataFetchesAsync(
        [AsParameters] DataFetchQueryDtoV1 request,
        [FromServices] IDataUow dataUow,
        CancellationToken ct)
    {
        var query = dataUow.Ctx.DataFetches
            .Where(x => request.Source == null || x.Source == request.Source)
            .Where(x => request.Type == null || x.Type == request.Type)
            .Where(x => request.StartedAtFrom == null || x.StartedAt >= request.StartedAtFrom)
            .Where(x => request.StartedAtTo == null || x.StartedAt <= request.StartedAtTo)
            .Where(x => request.Statuses == null || request.Statuses.Length == 0 ||
                        request.Statuses.Contains(x.Status))
            .Where(x => request.Success == null || (request.Success.Value
                ? x.Status == DataFetchStatus.Succeeded
                : x.Status != DataFetchStatus.Succeeded));

        var orderedQuery = (request.OrderByDescending ?? true) switch
        {
            true => query.OrderByDescending(x => x.StartedAt),
            false => query.OrderBy(x => x.StartedAt),
        };
        var dataFetches = await orderedQuery
            .Paginate(request)
            .ToListAsync(ct);

        var paginationResult = request.ToPaginationResult(dataFetches.Count, await query.CountAsync(ct));
        var result = new DataFetchesResponseDtoV1
        {
            DataFetches = dataFetches.Select(ApiMapper.MapDataFetchDtoV1).ToArray(),
            Limit = paginationResult.Limit,
            Page = paginationResult.Page,
            TotalResults = paginationResult.TotalResults,
            AmountOnPage = paginationResult.AmountOnPage,
        };
        return TypedResults.Ok(result);
    }

    private static Results<Ok, BadRequest<ErrorResponseDto>> EnqueueDataFetchJob(
        [FromBody] EnqueueDataFetchJobRequestV1 request,
        [FromServices] IOptions<JobsConfiguration> config,
        [FromServices] IBackgroundJobClientV2 backgroundJobClient)
    {
        var jobName = request.JobName;

        var jobDefinition = config.Value.RegisteredJobs.FirstOrDefault(x => x.DataFetchDefinition is not null && x.JobId == jobName);
        if (jobDefinition is null)
        {
            return TypedResults.BadRequest(new ErrorResponseDto
            {
                ErrorType = EErrorType.GenericError,
                Message = $"Job definition with name '{jobName}' not found",
            });
        }

        backgroundJobClient.Enqueue<ManualDataFetchJob>(x => x.FetchEntityDataAsync(jobName, request.EntityId, CancellationToken.None));
        return TypedResults.Ok();
    }

    private static Ok<DataFetchJobDefinitionsResponseDtoV1> GetDataFetchJobDefinitionsAsync(IOptions<JobsConfiguration> dataFetchJobsConfig)
    {
        var result = new DataFetchJobDefinitionsResponseDtoV1(JobDefinitions: dataFetchJobsConfig.Value.RegisteredJobs
            .Select(x => new DataFetchJobDefinitionDtoV1(
                EntityType: x.DataFetchDefinition.AssertNotNull().EntityType,
                JobName: x.JobId)));
        return TypedResults.Ok(result);
    }
}