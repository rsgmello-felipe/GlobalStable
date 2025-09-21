using GlobalStable.Application.UseCases.AccountUseCases;
using Microsoft.AspNetCore.Mvc;

namespace GlobalStable.API.Controllers;

[ApiController]
[Route("api/accounts")]
public class AccountController : ControllerBase
{
    private readonly GetAccountsByCustomerUseCase _getAccountsByCustomerUseCase;
    // Adicione outros use cases de contas conforme necessário

    public AccountController(
        GetAccountsByCustomerUseCase getAccountsByCustomerUseCase
        // Outros use cases de account, via DI
    )
    {
        _getAccountsByCustomerUseCase = getAccountsByCustomerUseCase;
    }

    [HttpGet("by-customer/{customerId:long}")]
    public async Task<IActionResult> GetByCustomer(long customerId)
    {
        var accounts = await _getAccountsByCustomerUseCase.ExecuteAsync(customerId);
        return Ok(accounts);
    }

    // Adicione outros endpoints do grupo de account aqui (Create, Update, etc.)
}