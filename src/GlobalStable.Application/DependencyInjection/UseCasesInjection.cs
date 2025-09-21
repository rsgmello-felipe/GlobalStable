using System.Diagnostics.CodeAnalysis;
using GlobalStable.Application.UseCases.Accounts;
using GlobalStable.Application.UseCases.AccountUseCases;
using GlobalStable.Application.UseCases.CustomerUseCases;
using GlobalStable.Application.UseCases.DepositUseCases;
using GlobalStable.Application.UseCases.QuoteOrderUseCases;
using GlobalStable.Application.UseCases.Withdrawal;
using GlobalStable.Application.UseCases.WithdrawalOrderUseCases;
using Microsoft.Extensions.DependencyInjection;

namespace GlobalStable.Application.DependencyInjection;

[ExcludeFromCodeCoverage]
public static class UseCasesInjection
{

    public static IServiceCollection AddUseCases(this IServiceCollection services)
    {
        services.AddScoped<CreateAccountUseCase>();
        services.AddScoped<DeleteAccountUseCase>();
        services.AddScoped<GetAccountsByCustomerUseCase>();
        services.AddScoped<UpdateAccountUseCase>();

        services.AddScoped<CreateCustomerUseCase>();
        services.AddScoped<DeleteCustomerUseCase>();
        services.AddScoped<UpdateCustomerUseCase>();
        
        services.AddScoped<CreateDepositOrderUseCase>();
        services.AddScoped<GetDepositOrdersUseCase>();
        services.AddScoped<HandleCompletedDepositStatusUseCase>();
        services.AddScoped<HandleDepositStatusUpdatedUseCase>();
        services.AddScoped<UpdateDepositStatusFromConnectorUseCase>();
        
        services.AddScoped<AcceptQuoteOrderUseCase>();
        services.AddScoped<RequestQuoteOrderUseCase>();

        services.AddScoped<CreateWithdrawalOrderUseCase>();
        services.AddScoped<UpdateManualWithdrawalOrderUseCase>();
        services.AddScoped<GetWithdrawalOrdersUseCase>();
        services.AddScoped<HandleCompletedWithdrawalStatusUseCase>();
        services.AddScoped<HandleCreatedWithdrawalUseCase>();
        services.AddScoped<HandleFailedWithdrawalUseCase>();
        services.AddScoped<HandleWithdrawalStatusUpdatedUseCase>();
        services.AddScoped<UpdateWithdrawalStatusFromConnectorUseCase>();

        return services;
    }
}
