using GlobalStable.Application.ApiRequests;
using GlobalStable.Application.ApiResponses;
using GlobalStable.Domain.Entities;
using GlobalStable.Domain.Interfaces.Repositories;
using FluentResults;
using Microsoft.Extensions.Logging;

namespace GlobalStable.Application.UseCases.Fee;

public class CreateFeeConfigUseCase(
    IFeeConfigRepository feeConfigRepository,
    ILogger<CreateFeeConfigUseCase> logger)
{
    public async Task<Result<FeeConfigResponse>> ExecuteAsync(
        CreateFeeConfigRequest request,
        long accountId)
    {
        try
        {
            var feeConfig = new FeeConfig(
                accountId,
                request.TransactionOrderType,
                request.FeePercentage,
                request.FlatFee,
                "API");

            await feeConfigRepository.AddAsync(feeConfig);

            var response = new FeeConfigResponse(feeConfig);
            return Result.Ok(response);
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Fee configuration could not be created.");
            return Result.Fail("Fee configuration not found");
        }
    }
}