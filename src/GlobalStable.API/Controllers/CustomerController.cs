using GlobalStable.Application.ApiRequests;
using GlobalStable.Application.UseCases.CustomerUseCases;
using Microsoft.AspNetCore.Mvc;

namespace GlobalStable.API.Controllers;

[ApiController]
[Route("api/customers")]
public class CustomerController : ControllerBase
{
    private readonly CreateCustomerUseCase _createCustomerUseCase;
    // Adicione outros use cases do grupo conforme necessário

    public CustomerController(
        CreateCustomerUseCase createCustomerUseCase
        // Outros use cases do grupo, via DI
    )
    {
        _createCustomerUseCase = createCustomerUseCase;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCustomerRequest request)
    {
        var result = await _createCustomerUseCase.ExecuteAsync(request);

        if (result.IsFailed)
            return BadRequest(result.Errors);

        return Ok(result.Value);
    }

    // Adicione outros endpoints do grupo de customer aqui (Get, Update, Delete)
}