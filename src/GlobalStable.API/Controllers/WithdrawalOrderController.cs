using System.IdentityModel.Tokens.Jwt;
using Asp.Versioning;
using GlobalStable.Application.ApiRequests;
using GlobalStable.Application.ApiResponses;
using GlobalStable.Application.UseCases.Withdrawal;
using GlobalStable.Application.UseCases.WithdrawalOrderUseCases;
using GlobalStable.Domain.Constants;
using Microsoft.AspNetCore.Mvc;

namespace GlobalStable.API.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/customer/{customerId:long}/orders/withdrawal")]
[ApiVersion("1.0")]
public class WithdrawalOrderController() : ControllerBase
{
    [HttpPost("account/{accountId:long}")]
    [ActionName(nameof(CreateWithdrawalOrder))]
    public async Task<IResult> CreateWithdrawalOrder(
        [FromServices] CreateWithdrawalOrderUseCase useCase,
        [FromHeader(Name = "Authorization")] string authorization,
        [FromBody] CreateWithdrawalOrderRequest request,
        [FromRoute] long accountId)
    {
        var token = authorization.Substring("Bearer ".Length).Trim();

        var handler = new JwtSecurityTokenHandler();
        if (!handler.CanReadToken(token))
        {
            return Results.Unauthorized();
        }

        var jwtToken = handler.ReadJwtToken(token);
        var username = jwtToken.Claims.FirstOrDefault(c => UserIdentifiers.FullSet.Contains(c.Type))?.Value;
        var result = await useCase.ExecuteAsync(request, accountId, username!);

        return result.IsFailed
            ? Results.BadRequest(new BaseApiResponse<string>(
                null,
                400,
                result.Errors.FirstOrDefault()?.Message))
            : Results.Json(
                new BaseApiResponse<WithdrawalOrderResponse>(
                    result.Value,
                    201,
                    $"WithdrawalOrder successfully created for accountId: {result.Value.AccountId}."),
                statusCode: StatusCodes.Status201Created);
    }

    [HttpGet]
    [ActionName(nameof(GetWithdrawalOrders))]
    public async Task<IResult> GetWithdrawalOrders(
        [FromServices] GetWithdrawalOrdersUseCase useCase,
        [FromRoute] long customerId,
        [FromQuery] long? orderId,
        [FromQuery] long? accountId,
        [FromQuery] string? status,
        [FromQuery] string? name,
        [FromQuery] string? E2EId,
        [FromQuery] string? taxId,
        [FromQuery] DateTime? beginDate,
        [FromQuery] DateTime? endDate,
        [FromQuery] string? sortBy,
        [FromQuery] string? sortOrder,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await useCase.ExecuteAsync(
            customerId,
            orderId,
            accountId,
            status,
            name,
            E2EId,
            taxId,
            beginDate,
            endDate,
            sortBy,
            sortOrder,
            page,
            pageSize);

        if (result.IsFailed)
        {
            return Results.BadRequest(new BaseApiResponse<string>(
                null,
                400,
                result.Errors.FirstOrDefault()?.Message ?? "An error occurred"));
        }

        if (result.Value.Orders.Count == 0)
        {
            return Results.Json(
                new BaseApiResponse<ListWithdrawalOrdersResponse>(
                    result.Value,
                    204,
                    $"No WithdrawalOrders found."),
                statusCode: StatusCodes.Status204NoContent);
        }

        return Results.Ok(new BaseApiResponse<ListWithdrawalOrdersResponse>(
            result.Value,
            200,
            $"WithdrawalOrders successfully retrieved."));
    }

    [HttpPut("account/{accountId:long}/manual")]
    [ActionName(nameof(UpdateManualWithdrawalOrder))]
    public async Task<IResult> UpdateManualWithdrawalOrder(
        [FromServices] UpdateManualWithdrawalOrderUseCase useCase,
        [FromHeader(Name = "Authorization")] string authorization,
        [FromHeader(Name = "X-Customer-Id")] long requestingCustomerId,
        [FromRoute] long customerId,
        [FromRoute] long accountId,
        [FromBody] UpdateManualWithdrawalOrderRequest request)
    {
        var token = authorization.Substring("Bearer ".Length).Trim();

        var handler = new JwtSecurityTokenHandler();
        if (!handler.CanReadToken(token))
        {
            return Results.Unauthorized();
        }

        var jwtToken = handler.ReadJwtToken(token);
        var username = jwtToken.Claims.FirstOrDefault(c => UserIdentifiers.FullSet.Contains(c.Type))?.Value;

        request.OrderCustomerId = customerId;
        request.RequestingCustomerId = requestingCustomerId;
        request.AccountId = accountId;
        var result = await useCase.ExecuteAsync(request, username);

        return result.IsFailed
            ? Results.BadRequest(new BaseApiResponse<string>(
                null,
                400,
                result.Errors.FirstOrDefault()?.Message))
            : Results.Json(
                new BaseApiResponse<WithdrawalOrderResponse>(
                    result.Value,
                    201,
                    $"WithdrawalOrder ({request.OrderId}) successfully updated."),
                statusCode: StatusCodes.Status201Created);
    }
}
