using System.IdentityModel.Tokens.Jwt;
using Asp.Versioning;
using GlobalStable.Application.ApiRequests;
using GlobalStable.Application.ApiResponses;
using GlobalStable.Application.UseCases.CustomerUseCases;
using GlobalStable.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GlobalStable.API.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/customers")]
[ApiVersion("1.0")]
public class CustomerController : ControllerBase
{
    [HttpGet("{customerId:long}")]
    [Authorize(Policy = "SameCustomer")]
    [ActionName(nameof(GetCustomer))]
    public async Task<IResult> GetCustomer(
        [FromServices] GetCustomerUseCase useCase,
        [FromRoute] long customerId)
    {
        var customer = await useCase.ExecuteAsync(customerId);

        if (customer == null)
        {
            return Results.NotFound(new BaseApiResponse<object>(null, 404, "Customer not found."));
        }

        return Results.Ok(new BaseApiResponse<object>(customer, 200, $"Customer {customerId} successfully retrieved."));
    }

    [HttpPost]
    [ActionName(nameof(CreateCustomer))]
    public async Task<IResult> CreateCustomer(
        [FromServices] CreateCustomerUseCase useCase,
        [FromBody] CreateCustomerRequest request)
    {
        var result = await useCase.ExecuteAsync(request);

        if (result.IsFailed)
        {
            return Results.BadRequest(new BaseApiResponse<string>(
                null,
                400,
                result.Errors.FirstOrDefault()?.Message ?? "An error occurred during customer creation."));
        }

        return Results.Json(
            new BaseApiResponse<object>(
                result.Value,
                201,
                "Customer successfully created."),
            statusCode: StatusCodes.Status201Created);
    }

    [HttpPut("{customerId:long}")]
    [ActionName(nameof(UpdateCustomer))]
    public async Task<IResult> UpdateCustomer(
        [FromServices] UpdateCustomerUseCase useCase,
        [FromRoute] long customerId,
        [FromBody] UpdateCustomerRequest request)
    {
        var result = await useCase.ExecuteAsync(customerId, request);

        if (result.IsFailed)
        {
            return Results.BadRequest(new BaseApiResponse<string>(
                null,
                400,
                result.Errors.FirstOrDefault()?.Message ?? "An error occurred during customer update."));
        }

        return Results.Ok(new BaseApiResponse<object>(
            result.Value,
            200,
            "Customer successfully updated."));
    }
}