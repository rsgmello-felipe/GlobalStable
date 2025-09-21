using System.IdentityModel.Tokens.Jwt;
using Asp.Versioning;
using GlobalStable.Application.ApiRequests;
using GlobalStable.Application.ApiResponses;
using GlobalStable.Application.UseCases.DepositUseCases;
using GlobalStable.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GlobalStable.API.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/customer/{customerId:long}/orders/deposit")]
[Authorize(Policy = "SameCustomer")]
[ApiVersion("1.0")]
public class DepositOrderController() : ControllerBase
{
    [HttpPost("account/{accountId}")]
    [ActionName(nameof(CreateDepositOrder))]
    public async Task<IResult> CreateDepositOrder(
        [FromServices] CreateDepositOrderUseCase useCase,
        [FromBody] CreateDepositOrderRequest request,
        [FromRoute] long accountId)
    {
        var result = await useCase.ExecuteAsync(request, accountId);

        return result.IsFailed
            ? Results.BadRequest(new BaseApiResponse<string>(
                null,
                400,
                result.Errors.FirstOrDefault()?.Message))
            : Results.Json(
                new BaseApiResponse<DepositOrderResponse>(
                    result.Value,
                    201,
                    $"Deposit order successfully created for accountId: {result.Value.AccountId}."),
                statusCode: StatusCodes.Status201Created);
    }

    [HttpGet]
    [ActionName(nameof(GetDepositOrders))]
    public async Task<IResult> GetDepositOrders(
        [FromServices] GetDepositOrdersUseCase useCase,
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
                new BaseApiResponse<GetDepositOrdersResponse>(
                    result.Value,
                    204,
                    $"No DepositOrders found."),
                statusCode: StatusCodes.Status204NoContent);
        }

        return Results.Ok(new BaseApiResponse<GetDepositOrdersResponse>(
            result.Value,
            200,
            $"DepositOrders successfully retrieved: {accountId}."));
    }
}
