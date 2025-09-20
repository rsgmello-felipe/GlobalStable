using System.Diagnostics.CodeAnalysis;
using GlobalStable.Application.UseCases.Deposit;
using GlobalStable.Application.UseCases.Fee;
using GlobalStable.Application.UseCases.Withdrawal;
using Microsoft.Extensions.DependencyInjection;

namespace GlobalStable.Application.DependencyInjection;

[ExcludeFromCodeCoverage]
public static class UseCasesInjection
{

    public static IServiceCollection AddUseCases(this IServiceCollection services)
    {
        services.AddScoped<CreateDepositOrderUseCase>();
        services.AddScoped<GetDepositOrdersUseCase>();
        services.AddScoped<HandleCompletedDepositStatusUseCase>();
        services.AddScoped<HandleDepositStatusUpdatedUseCase>();
        services.AddScoped<RefundDepositUseCase>();
        services.AddScoped<UpdateDepositStatusFromConnectorUseCase>();

        services.AddScoped<CreateFeeConfigUseCase>();

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
