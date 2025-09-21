using System.IdentityModel.Tokens.Jwt;
using Asp.Versioning;
using GlobalStable.Application.ApiRequests;
using GlobalStable.Application.ApiResponses;
using GlobalStable.Application.UseCases.QuoteOrderUseCases;
using GlobalStable.Domain.Constants;
using Microsoft.AspNetCore.Mvc;

namespace GlobalStable.API.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/customer/{customerId:long}/orders/quote")]
[ApiVersion("1.0")]
public class QuoteOrderController : ControllerBase
{
    [HttpPost]
    [ActionName(nameof(RequestQuoteOrder))]
    public async Task<IResult> RequestQuoteOrder(
        [FromServices] RequestQuoteOrderUseCase useCase,
        [FromBody] QuoteOrderRequest request,
        [FromRoute] long customerId)
    {
        var order = await useCase.ExecuteAsync(customerId, request);
        return Results.Json(order, statusCode: StatusCodes.Status201Created);
    }

    [HttpPost("{quoteOrderId:long}/accept")]
    [ActionName(nameof(AcceptQuoteOrder))]
    public async Task<IResult> AcceptQuoteOrder(
        [FromServices] AcceptQuoteOrderUseCase useCase,
        [FromRoute] long quoteOrderId)
    {
        var result = await useCase.ExecuteAsync(quoteOrderId);
        if(result.IsFailed)
        {
            return Results.BadRequest(result.Errors.FirstOrDefault()?.Message);
        }
        return Results.Json(result.Value, statusCode: StatusCodes.Status200OK);
    }
}