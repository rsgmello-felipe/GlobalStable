using AutoFixture;
using AutoFixture.AutoFakeItEasy;
using AutoFixture.Xunit2;
using GlobalStable.Application.UseCases.Fee;
using GlobalStable.Domain.Entities;
using GlobalStable.Domain.Interfaces.Repositories;
using FakeItEasy;
using FluentAssertions;

namespace GlobalStable.Tests.UnitTests.Application.FeeConfigTests;

public class GetFeeConfigByAccountUseCaseTests
{
    public class CustomAutoDataAttribute() : AutoDataAttribute(() =>
        new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true }));

    [Theory]
    [CustomAutoData]
    public async Task ShouldSucceed_WhenFeeConfigsExist(
        long accountId,
        List<FeeConfig> feeConfigs,
        [Frozen] IFeeConfigRepository feeConfigRepository)
    {
        A.CallTo(() => feeConfigRepository.GetAllByAccountIdAsync(accountId))
            .Returns(feeConfigs);

        var sut = new GetFeeConfigByAccountUseCase(feeConfigRepository);

        var result = await sut.ExecuteAsync(accountId);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(feeConfigs.Count);
    }

    [Theory]
    [CustomAutoData]
    public async Task ShouldFail_WhenFeeConfigsDoNotExist(
        long accountId,
        [Frozen] IFeeConfigRepository feeConfigRepository)
    {
        A.CallTo(() => feeConfigRepository.GetAllByAccountIdAsync(accountId))
            .Returns(new List<FeeConfig>());

        var sut = new GetFeeConfigByAccountUseCase(feeConfigRepository);

        var result = await sut.ExecuteAsync(accountId);

        result.IsFailed.Should().BeTrue();
        result.Errors.First().Message.Should().Be("Fee configuration not found");
    }
}
