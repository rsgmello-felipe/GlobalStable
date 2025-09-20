using GlobalStable.Application.ApiResponses;
using GlobalStable.Domain.Interfaces.Repositories;
using FluentResults;

namespace GlobalStable.Application.UseCases.Fee;

public class GetFeeConfigByAccountUseCase(
    IFeeConfigRepository feeConfigRepository)
{
    public async Task<Result<IEnumerable<FeeConfigResponse>>> ExecuteAsync(long accountId)
    {
        var result = await feeConfigRepository.GetAllByAccountIdAsync(accountId);

        if (result == null || !result.Any())
        {
            return Result.Fail<IEnumerable<FeeConfigResponse>>("Fee configuration not found");
        }

        var response = result.Select(fee => new FeeConfigResponse(fee));
        return Result.Ok(response);
    }
}