using System.Net;
using AutoFixture;
using AutoFixture.AutoFakeItEasy;
using AutoFixture.Xunit2;
using GlobalStable.Application.ApiRequests;
using GlobalStable.Application.UseCases.Withdrawal;
using GlobalStable.Domain.Common;
using GlobalStable.Domain.Constants;
using GlobalStable.Domain.Entities;
using GlobalStable.Domain.Enums;
using GlobalStable.Domain.Interfaces.Messaging;
using GlobalStable.Domain.Interfaces.Repositories;
using GlobalStable.Infrastructure.HttpClients;
using GlobalStable.Infrastructure.HttpClients.ApiRequests.TransactionService;
using GlobalStable.Infrastructure.HttpClients.ApiResponses;
using GlobalStable.Infrastructure.Persistence;
using FakeItEasy;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Refit;

namespace GlobalStable.Tests.UnitTests.Application.Withdrawal;

public class CreateWithdrawalOrderUseCaseTests
{
    public class CustomAutoDataAttribute : AutoDataAttribute
    {
        public CustomAutoDataAttribute()
            : base(() =>
                new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true }))
        {
        }
    }

    [Theory]
    [CustomAutoData]
    public async Task ShouldFail_WhenPendingStatusNotFound(
        CreateWithdrawalOrderRequest request,
        long accountId,
        [Frozen] IWithdrawalOrderRepository withdrawalOrderRepository,
        [Frozen] IOrderStatusRepository orderStatusRepository,
        [Frozen] IFeeConfigRepository feeConfigRepository,
        [Frozen] IAccountRepository accountRepository,
        [Frozen] IOrderEventPublisher orderEventPublisher,
        [Frozen] ITransactionServiceClient transactionServiceClient,
        [Frozen] ICustomerServiceClient customerServiceClient,
        [Frozen] ILogger<CreateWithdrawalOrderUseCase> logger)
    {
        // Arrange
        A.CallTo(() => orderStatusRepository.GetByNameAsync(OrderStatuses.Created))
            .Returns((OrderStatus?)null);

        var sut = new CreateWithdrawalOrderUseCase(
            withdrawalOrderRepository,
            orderStatusRepository,
            feeConfigRepository,
            accountRepository,
            orderEventPublisher,
            transactionServiceClient,
            customerServiceClient,
            logger,
            CreateInMemoryDb());

        // Act
        var result = await sut.ExecuteAsync(request, accountId, "test-user", null);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors[0].Message.Should().Be($"Status '{OrderStatuses.Created}' not found.");

        A.CallTo(() => withdrawalOrderRepository.AddAsync(A<WithdrawalOrder>._))
            .MustNotHaveHappened();

        A.CallTo(() => orderEventPublisher.PublishWithdrawalOrderEvent(A<WithdrawalOrder>._))
            .MustNotHaveHappened();
    }

    [Theory]
    [CustomAutoData]
    public async Task ShouldFail_WhenFeeConfigNotFound(
        CreateWithdrawalOrderRequest request,
        Account account,
        [Frozen] IWithdrawalOrderRepository withdrawalOrderRepository,
        [Frozen] IOrderStatusRepository orderStatusRepository,
        [Frozen] IFeeConfigRepository feeConfigRepository,
        [Frozen] IAccountRepository accountRepository,
        [Frozen] IOrderEventPublisher orderEventPublisher,
        [Frozen] ITransactionServiceClient transactionServiceClient,
        [Frozen] ICustomerServiceClient customerServiceClient,
        [Frozen] ILogger<CreateWithdrawalOrderUseCase> logger)
    {
        // Arrange
        var statusesDictionary = new Dictionary<long, string>
        {
            { 1, OrderStatuses.Created },
        };

        A.CallTo(() => orderStatusRepository.GetAllAsDictionaryAsync())
            .Returns(statusesDictionary);

        A.CallTo(() => accountRepository.GetByIdAsync(account.Id))
            .Returns(account);

        A.CallTo(() => feeConfigRepository.GetByAccountIdAsync(account.Id, TransactionOrderType.Withdrawal))
            .Returns((FeeConfig?)null);

        var getBalanceResponse = new GetBalanceResponse()
        {
            Balance = 1000m,
            AccountId = account.Id,
        };

        var balanceResponse = new ApiResponse<BaseApiResponse<GetBalanceResponse>>(
            new HttpResponseMessage(HttpStatusCode.OK),
            new BaseApiResponse<GetBalanceResponse>
            {
                Result = getBalanceResponse,
                Status = true,
            },
            new RefitSettings());

        A.CallTo(() => transactionServiceClient.GetBalanceAsync(account.CustomerId, account.Id))
            .Returns(balanceResponse);

        var sut = new CreateWithdrawalOrderUseCase(
            withdrawalOrderRepository,
            orderStatusRepository,
            feeConfigRepository,
            accountRepository,
            orderEventPublisher,
            transactionServiceClient,
            customerServiceClient,
            logger,
            CreateInMemoryDb());

        // Act
        var result = await sut.ExecuteAsync(request, account.Id, "test-user", null);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors[0].Message.Should().Contain("Fee config not found for AccountId");

        A.CallTo(() => withdrawalOrderRepository.AddAsync(A<WithdrawalOrder>._))
            .MustNotHaveHappened();

        A.CallTo(() => orderEventPublisher.PublishWithdrawalOrderEvent(A<WithdrawalOrder>._))
            .MustNotHaveHappened();
    }

    [Theory]
    [CustomAutoData]
    public async Task ShouldFail_WhenUnexpectedExceptionOccurs(
        CreateWithdrawalOrderRequest request,
        long accountId,
        [Frozen] IWithdrawalOrderRepository withdrawalOrderRepository,
        [Frozen] IOrderStatusRepository orderStatusRepository,
        [Frozen] IFeeConfigRepository feeConfigRepository,
        [Frozen] IAccountRepository accountRepository,
        [Frozen] IOrderEventPublisher orderEventPublisher,
        [Frozen] ITransactionServiceClient transactionServiceClient,
        [Frozen] ICustomerServiceClient customerServiceClient,
        [Frozen] ILogger<CreateWithdrawalOrderUseCase> logger)
    {
        // Arrange
        var exception = new InvalidOperationException("Database down");

        A.CallTo(() => orderStatusRepository.GetAllAsDictionaryAsync())
            .Throws(exception);

        var sut = new CreateWithdrawalOrderUseCase(
            withdrawalOrderRepository,
            orderStatusRepository,
            feeConfigRepository,
            accountRepository,
            orderEventPublisher,
            transactionServiceClient,
            customerServiceClient,
            logger,
            CreateInMemoryDb());

        // Act
        var result = await sut.ExecuteAsync(request, accountId, "test-user", null);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors[0].Message.Should().Contain("Error processing withdrawal order");
    }

    [Theory]
    [CustomAutoData]
    public async Task ShouldSucceed_WhenCryptoWithdrawalOrderIsCreated(
        CreateWithdrawalOrderRequest request,
        OrderStatus pendingStatus,
        FeeConfig feeConfig,
        Account account,
        [Frozen] IWithdrawalOrderRepository withdrawalOrderRepository,
        [Frozen] IOrderStatusRepository orderStatusRepository,
        [Frozen] IFeeConfigRepository feeConfigRepository,
        [Frozen] IAccountRepository accountRepository,
        [Frozen] IOrderEventPublisher orderEventPublisher,
        [Frozen] ITransactionServiceClient transactionServiceClient,
        [Frozen] ICustomerServiceClient customerServiceClient,
        [Frozen] ILogger<CreateWithdrawalOrderUseCase> logger)
    {
        // Arrange
        account.Currency = new Currency(3, "ETH", "Ethereum", 8, CurrencyType.Crypto);

        request.ReceiverBlockchain = "Ethereum";
        request.ReceiverWalletAddress = "0x1234abcd5678ef901234abcd5678ef901234abcd";

        pendingStatus = new OrderStatus(1, OrderStatuses.Created);

        feeConfig = new FeeConfig(account.Id, TransactionOrderType.Withdrawal, 0.01m, 5m, "Test");

        A.CallTo(() => accountRepository.GetByIdAsync(account.Id)).Returns(account);
        A.CallTo(() => orderStatusRepository.GetAllAsDictionaryAsync())
            .Returns(new Dictionary<long, string> { { pendingStatus.Id, pendingStatus.Name } });
        A.CallTo(() => feeConfigRepository.GetByAccountIdAsync(account.Id, TransactionOrderType.Withdrawal))
            .Returns(feeConfig);

        var getBalanceResponse = new GetBalanceResponse()
        {
            Balance = 1000m,
            AccountId = account.Id,
        };

        var balanceResponse = new ApiResponse<BaseApiResponse<GetBalanceResponse>>(
            new HttpResponseMessage(HttpStatusCode.OK),
            new BaseApiResponse<GetBalanceResponse>
            {
                Result = getBalanceResponse,
                Status = true,
            },
            new RefitSettings());

        A.CallTo(() => transactionServiceClient.GetBalanceAsync(account.CustomerId, account.Id))
            .Returns(balanceResponse);

        var txResponse = new ApiResponse<BaseApiResponse<CreatePendingTransactionResponse>>(
            new HttpResponseMessage(HttpStatusCode.OK),
            new BaseApiResponse<CreatePendingTransactionResponse>(),
            new RefitSettings());

        A.CallTo(() =>
                transactionServiceClient.CreatePendingTransactionAsync(
                    A<CreatePendingTransactionRequest>._,
                    A<long>._,
                    A<long>._))
            .Returns(txResponse);

        var blockchainResponse = new ApiResponse<BaseApiResponse<PagedResult<GetCurrencyResponse>>>(
            new HttpResponseMessage(HttpStatusCode.OK),
            new BaseApiResponse<PagedResult<GetCurrencyResponse>>
            {
                Result = new PagedResult<GetCurrencyResponse>(
                    new List<GetCurrencyResponse>
                    {
                        new GetCurrencyResponse
                        {
                            Name = request.ReceiverBlockchain,
                            CurrenciesBlockchains = new List<CurrencyBlockchain>
                            {
                                new CurrencyBlockchain
                                {
                                    Blockchain = new Blockchain
                                    {
                                        Code = "ETH",
                                        Enabled = true,
                                        Name = "Ethereum",
                                        Regex = "^0[xX][0-9a-fA-F]{40}$",
                                    },
                                },
                            },
                        },
                    },
                    new Pagination(1, 1, 10)),
            },
            new RefitSettings());

        A.CallTo(() => customerServiceClient.GetCurrencyAsync(account.Currency.Code, A<string>._))
            .Returns(blockchainResponse);

        var sut = new CreateWithdrawalOrderUseCase(
            withdrawalOrderRepository,
            orderStatusRepository,
            feeConfigRepository,
            accountRepository,
            orderEventPublisher,
            transactionServiceClient,
            customerServiceClient,
            logger,
            CreateInMemoryDb());

        // Act
        var result = await sut.ExecuteAsync(request, account.Id, "test-user", null);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.AccountId.Should().Be(account.Id);

        A.CallTo(() => withdrawalOrderRepository.AddAsync(A<WithdrawalOrder>._))
            .MustHaveHappenedOnceExactly();

        A.CallTo(() => orderEventPublisher.PublishWithdrawalOrderEvent(A<WithdrawalOrder>._))
            .MustHaveHappenedOnceExactly();
    }

    [Theory]
    [CustomAutoData]
    public async Task ShouldSucceed_WhenFiatWithdrawalOrderIsCreated(
        CreateWithdrawalOrderRequest request,
        OrderStatus pendingStatus,
        FeeConfig feeConfig,
        Account account,
        [Frozen] IWithdrawalOrderRepository withdrawalOrderRepository,
        [Frozen] IOrderStatusRepository orderStatusRepository,
        [Frozen] IFeeConfigRepository feeConfigRepository,
        [Frozen] IAccountRepository accountRepository,
        [Frozen] IOrderEventPublisher orderEventPublisher,
        [Frozen] ITransactionServiceClient transactionServiceClient,
        [Frozen] ICustomerServiceClient customerServiceClient,
        [Frozen] ILogger<CreateWithdrawalOrderUseCase> logger)
    {
        // Arrange
        account.Currency = new Currency(1, "BRL", "Brazilian Real", 2, CurrencyType.Fiat);

        request.ReceiverAccountKey = "123123";
        request.ReceiverTaxId = "18997933728";

        var autoExecuteProperty = typeof(Account).GetProperty("AutoExecuteWithdrawal");
        autoExecuteProperty?.SetValue(account, true);

        pendingStatus = new OrderStatus(1, OrderStatuses.Created);

        feeConfig = new FeeConfig(account.Id, TransactionOrderType.Withdrawal, 0.01m, 5m, "Test");

        A.CallTo(() => accountRepository.GetByIdAsync(account.Id)).Returns(account);
        A.CallTo(() => orderStatusRepository.GetAllAsDictionaryAsync())
            .Returns(new Dictionary<long, string> { { pendingStatus.Id, pendingStatus.Name } });
        A.CallTo(() => feeConfigRepository.GetByAccountIdAsync(account.Id, TransactionOrderType.Withdrawal))
            .Returns(feeConfig);

        var getBalanceResponse = new GetBalanceResponse()
        {
            Balance = 1000m,
            AccountId = account.Id,
        };

        var balanceResponse = new ApiResponse<BaseApiResponse<GetBalanceResponse>>(
            new HttpResponseMessage(HttpStatusCode.OK),
            new BaseApiResponse<GetBalanceResponse>
            {
                Result = getBalanceResponse,
                Status = true,
            },
            new RefitSettings());

        A.CallTo(() => transactionServiceClient.GetBalanceAsync(account.CustomerId, account.Id))
            .Returns(balanceResponse);

        var txResponse = new ApiResponse<BaseApiResponse<CreatePendingTransactionResponse>>(
            new HttpResponseMessage(HttpStatusCode.OK),
            new BaseApiResponse<CreatePendingTransactionResponse>(),
            new RefitSettings());

        A.CallTo(() =>
                transactionServiceClient.CreatePendingTransactionAsync(
                    A<CreatePendingTransactionRequest>._,
                    A<long>._,
                    A<long>._))
            .Returns(txResponse);

        var blockchainResponse = new ApiResponse<BaseApiResponse<PagedResult<GetCurrencyResponse>>>(
            new HttpResponseMessage(HttpStatusCode.OK),
            new BaseApiResponse<PagedResult<GetCurrencyResponse>>
            {
                Result = new PagedResult<GetCurrencyResponse>(
                    new List<GetCurrencyResponse>
                    {
                        new GetCurrencyResponse
                        {
                            Name = request.ReceiverBlockchain,
                            CurrenciesBlockchains = new List<CurrencyBlockchain>
                            {
                                new CurrencyBlockchain
                                {
                                    Blockchain = new Blockchain
                                    {
                                        Code = "ETH",
                                        Enabled = true,
                                        Name = "Ethereum",
                                        Regex = "^0[xX][0-9a-fA-F]{40}$",
                                    },
                                },
                            },
                        },
                    },
                    new Pagination(1, 1, 10)),
            },
            new RefitSettings());

        A.CallTo(() => customerServiceClient.GetCurrencyAsync(account.Currency.Code, A<string>._))
            .Returns(blockchainResponse);

        var sut = new CreateWithdrawalOrderUseCase(
            withdrawalOrderRepository,
            orderStatusRepository,
            feeConfigRepository,
            accountRepository,
            orderEventPublisher,
            transactionServiceClient,
            customerServiceClient,
            logger,
            CreateInMemoryDb());

        // Act
        var result = await sut.ExecuteAsync(request, account.Id, "test-user", null);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.AccountId.Should().Be(account.Id);

        A.CallTo(() => withdrawalOrderRepository.AddAsync(A<WithdrawalOrder>._))
            .MustHaveHappenedOnceExactly();

        A.CallTo(() => orderEventPublisher.PublishWithdrawalOrderEvent(A<WithdrawalOrder>._))
            .MustHaveHappenedOnceExactly();
    }

    [Theory]
    [CustomAutoData]
    public async Task ShouldFail_WhenPendingTransactionCreationFails(
        CreateWithdrawalOrderRequest request,
        OrderStatus pendingStatus,
        FeeConfig feeConfig,
        Account account,
        CreatePendingTransactionResponse createPendingTransactionResponse,
        [Frozen] IWithdrawalOrderRepository withdrawalOrderRepository,
        [Frozen] IOrderStatusRepository orderStatusRepository,
        [Frozen] IFeeConfigRepository feeConfigRepository,
        [Frozen] IAccountRepository accountRepository,
        [Frozen] IOrderEventPublisher orderEventPublisher,
        [Frozen] ITransactionServiceClient transactionServiceClient,
        [Frozen] ICustomerServiceClient customerServiceClient,
        [Frozen] ILogger<CreateWithdrawalOrderUseCase> logger)
    {
        // Arrange
        account.Currency = new Currency(1, "BRL", "Brazilian Real", 2, CurrencyType.Fiat);

        request.ReceiverAccountKey = "123123123";
        request.ReceiverTaxId = "18997933728";
        request.ReceiverBlockchain = null;
        request.ReceiverWalletAddress = null;

        pendingStatus = new OrderStatus(1, OrderStatuses.Created);

        feeConfig = new FeeConfig(account.Id, TransactionOrderType.Withdrawal, 0.01m, 5m, "Test");

        A.CallTo(() => accountRepository.GetByIdAsync(account.Id)).Returns(account);
        A.CallTo(() => orderStatusRepository.GetAllAsDictionaryAsync())
            .Returns(new Dictionary<long, string> { { pendingStatus.Id, pendingStatus.Name } });
        A.CallTo(() => feeConfigRepository.GetByAccountIdAsync(account.Id, TransactionOrderType.Withdrawal))
            .Returns(feeConfig);


        var baseApiResponse = new BaseApiResponse<CreatePendingTransactionResponse>(createPendingTransactionResponse);

        var failedHttpResponse = new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent("Erro na criação da transação"),
            RequestMessage = new HttpRequestMessage(HttpMethod.Post, "https://fake-url.com"),
        };

        var failedTransactionResponse = new ApiResponse<BaseApiResponse<CreatePendingTransactionResponse>>(
            failedHttpResponse,
            baseApiResponse,
            new RefitSettings());

        A.CallTo(() => transactionServiceClient.CreatePendingTransactionAsync(A<CreatePendingTransactionRequest>._, A<long>._, A<long>._))
            .Returns(failedTransactionResponse);

        var getBalanceResponse = new GetBalanceResponse()
        {
            Balance = 1000m,
            AccountId = account.Id,
        };

        var balanceResponse = new ApiResponse<BaseApiResponse<GetBalanceResponse>>(
            new HttpResponseMessage(HttpStatusCode.OK),
            new BaseApiResponse<GetBalanceResponse>
            {
                Result = getBalanceResponse,
                Status = true,
            },
            new RefitSettings());

        A.CallTo(() => transactionServiceClient.GetBalanceAsync(account.CustomerId, account.Id))
            .Returns(balanceResponse);


        var blockchainResponse = new ApiResponse<BaseApiResponse<PagedResult<GetCurrencyResponse>>>(
            new HttpResponseMessage(HttpStatusCode.OK),
            new BaseApiResponse<PagedResult<GetCurrencyResponse>>
            {
                Result = new PagedResult<GetCurrencyResponse>(
                    new List<GetCurrencyResponse>
                    {
                        new GetCurrencyResponse
                        {
                            Name = request.ReceiverBlockchain,
                            CurrenciesBlockchains = new List<CurrencyBlockchain>
                            {
                                new CurrencyBlockchain
                                {
                                    Blockchain = new Blockchain
                                    {
                                        Code = "ETH",
                                        Enabled = true,
                                        Name = "Ethereum",
                                        Regex = "^0[xX][0-9a-fA-F]{40}$",
                                    },
                                },
                            },
                        },
                    },
                    new Pagination(1, 1, 10)),
            },
            new RefitSettings());

        A.CallTo(() => customerServiceClient.GetCurrencyAsync(account.Currency.Code, A<string>._))
            .Returns(blockchainResponse);

        var sut = new CreateWithdrawalOrderUseCase(
            withdrawalOrderRepository,
            orderStatusRepository,
            feeConfigRepository,
            accountRepository,
            orderEventPublisher,
            transactionServiceClient,
            customerServiceClient,
            logger,
            CreateInMemoryDb());

        // Act
        var result = await sut.ExecuteAsync(request, account.Id, "test-user", null);

        // Assert
        result.IsSuccess.Should().BeFalse();

        A.CallTo(() => orderEventPublisher.PublishWithdrawalOrderEvent(A<WithdrawalOrder>._))
            .MustNotHaveHappened();
    }

    [Theory]
    [CustomAutoData]
    public async Task ShouldFail_WhenReceiverWalletAddressIsMissing_ForCrypto(
        CreateWithdrawalOrderRequest request,
        Account account,
        [Frozen] IWithdrawalOrderRepository withdrawalOrderRepository,
        [Frozen] IOrderStatusRepository orderStatusRepository,
        [Frozen] IFeeConfigRepository feeConfigRepository,
        [Frozen] IAccountRepository accountRepository,
        [Frozen] IOrderEventPublisher orderEventPublisher,
        [Frozen] ITransactionServiceClient transactionServiceClient,
        [Frozen] ICustomerServiceClient customerServiceClient,
        [Frozen] ILogger<CreateWithdrawalOrderUseCase> logger)
    {
        // Arrange
        account.Currency = new Currency(3, "BTC", "Bitcoin", 18, CurrencyType.Crypto);
        request.ReceiverWalletAddress = null;
        request.ReceiverBlockchain = "Bitcoin";

        A.CallTo(() => accountRepository.GetByIdAsync(account.Id)).Returns(account);

        var sut = new CreateWithdrawalOrderUseCase(
            withdrawalOrderRepository,
            orderStatusRepository,
            feeConfigRepository,
            accountRepository,
            orderEventPublisher,
            transactionServiceClient,
            customerServiceClient,
            logger,
            CreateInMemoryDb());

        // Act
        var result = await sut.ExecuteAsync(request, account.Id, "user", null);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.First().Message.Should().Contain("ReceiverWalletAddress");
    }

    [Theory]
    [CustomAutoData]
    public async Task ShouldFail_WhenBlockchainNotSupported(
        CreateWithdrawalOrderRequest request,
        Account account,
        OrderStatus createdStatus,
        [Frozen] IWithdrawalOrderRepository withdrawalOrderRepository,
        [Frozen] IOrderStatusRepository orderStatusRepository,
        [Frozen] IFeeConfigRepository feeConfigRepository,
        [Frozen] IAccountRepository accountRepository,
        [Frozen] IOrderEventPublisher orderEventPublisher,
        [Frozen] ITransactionServiceClient transactionServiceClient,
        [Frozen] ICustomerServiceClient customerServiceClient,
        [Frozen] ILogger<CreateWithdrawalOrderUseCase> logger)
    {
        // Arrange
        createdStatus = new OrderStatus(1, OrderStatuses.Created);
        A.CallTo(() => orderStatusRepository.GetAllAsDictionaryAsync())
            .Returns(new Dictionary<long, string> { { createdStatus.Id, createdStatus.Name } });

        account.Currency = new Currency(3, "BTC", "Bitcoin", 18, CurrencyType.Crypto);
        request.ReceiverWalletAddress = "wallet123";
        request.ReceiverBlockchain = "UnsupportedChain";

        A.CallTo(() => accountRepository.GetByIdAsync(account.Id)).Returns(account);

        var blockchainResponse = new ApiResponse<BaseApiResponse<PagedResult<GetCurrencyResponse>>>(
            new HttpResponseMessage(HttpStatusCode.OK),
            new BaseApiResponse<PagedResult<GetCurrencyResponse>>
            {
                Result = new PagedResult<GetCurrencyResponse>(
                    new List<GetCurrencyResponse>
                    {
                        new GetCurrencyResponse
                        {
                            Name = "SupportedChain",
                        },
                    },
                    new Pagination(1, 1, 10)),
            },
            new RefitSettings());


        A.CallTo(() => customerServiceClient.GetCurrencyAsync(account.Currency.Code, A<string>._))
            .Returns(blockchainResponse);

        var sut = new CreateWithdrawalOrderUseCase(
            withdrawalOrderRepository,
            orderStatusRepository,
            feeConfigRepository,
            accountRepository,
            orderEventPublisher,
            transactionServiceClient,
            customerServiceClient,
            logger,
            CreateInMemoryDb());

        // Act
        var result = await sut.ExecuteAsync(request, account.Id, "user", null);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.First().Message.Should().Contain("Blockchain not supported");
    }

    private static ServiceDbContext CreateInMemoryDb()
    {
        var options = new DbContextOptionsBuilder<ServiceDbContext>()
            .UseInMemoryDatabase("test_" + Guid.NewGuid())
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        return new ServiceDbContext(options);
    }
}