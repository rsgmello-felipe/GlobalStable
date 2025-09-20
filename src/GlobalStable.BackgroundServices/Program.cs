using GlobalStable.Application.DependencyInjection;
using GlobalStable.BackgroundServices.Consumers;
using GlobalStable.Domain.Interfaces.Messaging;
using GlobalStable.Infrastructure.DependencyInjection;
using GlobalStable.Infrastructure.Messaging;
using GlobalStable.Infrastructure.Settings;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((hostingContext, config) =>
    {
        var env = hostingContext.HostingEnvironment;

        config.SetBasePath(Directory.GetCurrentDirectory());
        config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
        config.AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true);
        config.AddEnvironmentVariables();
    })
    .ConfigureServices((context, services) =>
    {
        var configuration = context.Configuration;

        services.AddHostedService<ConnectorDepositEventConsumer>();
        services.AddHostedService<DepositStatusUpdateConsumer>();
        services.AddHostedService<ConnectorWithdrawalEventConsumer>();
        services.AddHostedService<WithdrawalStatusUpdateConsumer>();

        services.Configure<CallbackSettings>(configuration.GetSection("CallbackSettings"));

        services.AddInfrastructureServices(configuration);

        services.AddScoped<IEventPublisher, RabbitMqPublisher>();
        services.AddScoped<IOrderEventPublisher, OrderEventPublisher>();

        services.AddUseCases();

        services.AddLogging(logging =>
        {
            logging.ClearProviders();
            logging.AddConsole();
        });
    })
    .Build();

await host.RunAsync();