using AutoFixture;
using AutoFixture.AutoFakeItEasy;
using AutoFixture.Xunit2;
using GlobalStable.Application.ApiRequests;
using GlobalStable.Application.UseCases.Fee;
using GlobalStable.Domain.Interfaces.Repositories;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging;

namespace GlobalStable.Tests.UnitTests.Application.FeeConfigTests;

public class CreateFeeConfigUseCaseTests
{
    public class CustomAutoDataAttribute() : AutoDataAttribute(() =>
        new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true }));

    [Theory]
    [CustomAutoData]
    public async Task ShouldSucceed_WhenFeeConfigIsCreatedSuccessfully(
        CreateFeeConfigRequest request,
        long accountId,
        [Frozen] IFeeConfigRepository feeConfigRepository,
        [Frozen] ILogger<CreateFeeConfigUseCase> logger)
    {
        // Arrange
        var sut = new CreateFeeConfigUseCase(feeConfigRepository, logger);

        A.CallTo(() => feeConfigRepository.AddAsync(A<GlobalStable.Domain.Entities.FeeConfig>.Ignored))
            .Returns(Task.CompletedTask);

        // Act
        var result = await sut.ExecuteAsync(request, accountId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.AccountId.Should().Be(accountId);
    }

    [Theory]
    [CustomAutoData]
    public async Task ShouldFail_WhenExceptionIsThrown(
        CreateFeeConfigRequest request,
        long accountId,
        [Frozen] IFeeConfigRepository feeConfigRepository,
        [Frozen] ILogger<CreateFeeConfigUseCase> logger)
    {
        // Arrange
        var sut = new CreateFeeConfigUseCase(feeConfigRepository, logger);

        A.CallTo(() => feeConfigRepository.AddAsync(A<GlobalStable.Domain.Entities.FeeConfig>.Ignored))
            .Throws(new Exception("Database failure"));

        // Act
        var result = await sut.ExecuteAsync(request, accountId);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors[0].Message.Should().Be("Fee configuration not found");
    }
}
