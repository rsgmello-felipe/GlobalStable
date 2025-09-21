using System.IdentityModel.Tokens.Jwt;
using Asp.Versioning;
using GlobalStable.Application.ApiRequests;
using GlobalStable.Application.ApiResponses;
using GlobalStable.Application.UseCases.Accounts;
using GlobalStable.Application.UseCases.AccountUseCases;
using GlobalStable.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GlobalStable.API.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/customer/{customerId:long}/accounts")]
[Authorize(Policy = "SameCustomer")]
[ApiVersion("1.0")]
public class AccountController : ControllerBase
{
    [HttpGet]
    [ActionName(nameof(GetAccountsByCustomer))]
    public async Task<IResult> GetAccountsByCustomer(
        [FromServices] GetAccountsByCustomerUseCase useCase,
        [FromRoute] long customerId)
    {
        var accounts = await useCase.ExecuteAsync(customerId);

        if (accounts == null || !accounts.Any())
        {
            return Results.Json(
                new BaseApiResponse<IEnumerable<object>>(accounts, 204, "No accounts found."),
                statusCode: StatusCodes.Status204NoContent);
        }

        return Results.Ok(new BaseApiResponse<IEnumerable<object>>(accounts, 200, $"Accounts successfully retrieved for customerId: {customerId}."));
    }

    [HttpPost]
    [ActionName(nameof(CreateAccount))]
    public async Task<IResult> CreateAccount(
        [FromServices] CreateAccountUseCase useCase,
        [FromRoute] long customerId,
        [FromBody] CreateAccountRequest request)
    {
        var result = await useCase.ExecuteAsync(customerId, request);

        if (result.IsFailed)
        {
            return Results.BadRequest(new BaseApiResponse<string>(
                null,
                400,
                result.Errors.FirstOrDefault()?.Message ?? "An error occurred during account creation."));
        }

        return Results.Json(
            new BaseApiResponse<object>(
                result.Value,
                201,
                "Account successfully created."),
            statusCode: StatusCodes.Status201Created);
    }

    [HttpPut("{accountId:long}")]
    [ActionName(nameof(UpdateAccount))]
    public async Task<IResult> UpdateAccount(
        [FromServices] UpdateAccountUseCase useCase,
        [FromRoute] long customerId,
        [FromBody] UpdateAccountRequest request)
    {
        var result = await useCase.ExecuteAsync(customerId, request);

        if (result.IsFailed)
        {
            return Results.BadRequest(new BaseApiResponse<string>(
                null,
                400,
                result.Errors.FirstOrDefault()?.Message ?? "An error occurred during account update."));
        }

        return Results.Ok(new BaseApiResponse<object>(
            result.Value,
            200,
            "Account successfully updated."));
    }
}