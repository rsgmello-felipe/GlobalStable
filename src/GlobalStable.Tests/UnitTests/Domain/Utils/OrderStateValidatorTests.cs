using GlobalStable.Domain.Helpers;
using FluentAssertions;

namespace GlobalStable.Tests.UnitTests.Domain.Utils;

public class OrderStateValidatorTests
{
    public static IEnumerable<object[]> WithdrawalTransitionCases =>
        new List<object[]>
        {
            new object[] { 1, 3, true },    // CREATED → PENDING_APPROVAL
            new object[] { 3, 4, true },    // PENDING_APPROVAL → SENT_TO_CONNECTOR
            new object[] { 3, 9, true },    // PENDING_APPROVAL → CANCELLED
            new object[] { 4, 5, true },    // SENT_TO_CONNECTOR → PENDING_IN_BANK
            new object[] { 4, 13, true },   // SENT_TO_CONNECTOR → BLOCKED
            new object[] { 4, 8, true },    // SENT_TO_CONNECTOR → FAILED
            new object[] { 5, 6, true },    // PENDING_IN_BANK → PROCESSING
            new object[] { 5, 8, true },    // PENDING_IN_BANK → FAILED
            new object[] { 6, 7, true },    // PROCESSING → COMPLETED
            new object[] { 6, 8, true },    // PROCESSING → FAILED
            new object[] { 7, 11, true },   // COMPLETED → RETURNED

            // Casos inválidos
            new object[] { 1, 1, false },
            new object[] { 3, 5, false },
            new object[] { 4, 6, false },
            new object[] { 10, 13, false },
            new object[] { 11, 7, false },
        };

    public static IEnumerable<object[]> DepositTransitionCases =>
        new List<object[]>
        {
            new object[] { 2, 7, true },    // PENDING_DEPOSIT → COMPLETED
            new object[] { 2, 12, true },   // PENDING_DEPOSIT → EXPIRED
            new object[] { 2, 13, true },   // PENDING_DEPOSIT → BLOCKED
            new object[] { 7, 10, true },   // COMPLETED → PROCESSING_REFUND
            new object[] { 10, 11, true },  // PROCESSING_REFUND → RETURNED
            new object[] { 10, 7, true },   // PROCESSING_REFUND → COMPLETED

            // Casos inválidos
            new object[] { 2, 1, false },
            new object[] { 7, 13, false },
            new object[] { 11, 10, false },
            new object[] { 13, 6, false },
        };

    [Theory]
    [MemberData(nameof(WithdrawalTransitionCases))]
    public void CanTransitionWithdrawal_ShouldValidateCorrectly(long fromStatusId, long toStatusId, bool expected)
    {
        var result = OrderStateValidator.ValidAutomaticWithdrawalTransition(fromStatusId, toStatusId);
        result.Should().Be(expected, $"transition from {fromStatusId} to {toStatusId} should be {(expected ? "allowed" : "disallowed")}");
    }

    [Theory]
    [MemberData(nameof(DepositTransitionCases))]
    public void CanTransitionDeposit_ShouldValidateCorrectly(long fromStatusId, long toStatusId, bool expected)
    {
        var result = OrderStateValidator.ValidAutomaticDepositTransition(fromStatusId, toStatusId);
        result.Should().Be(expected, $"transition from {fromStatusId} to {toStatusId} should be {(expected ? "allowed" : "disallowed")}");
    }
}
