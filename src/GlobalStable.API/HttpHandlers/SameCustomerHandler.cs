using Microsoft.AspNetCore.Authorization;

namespace GlobalStable.Infrastructure.HttpHandlers;
public class SameCustomerRequirement : IAuthorizationRequirement {}

public class SameCustomerHandler : AuthorizationHandler<SameCustomerRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, SameCustomerRequirement requirement)
    {
        var customerClaim = context.User.FindFirst("customer_id")?.Value;
        if (string.IsNullOrEmpty(customerClaim))
            return Task.CompletedTask;

        if (context.Resource is Microsoft.AspNetCore.Mvc.Filters.AuthorizationFilterContext mvc)
        {
            if (mvc.RouteData.Values.TryGetValue("customerId", out var routeVal) &&
                long.TryParse(routeVal?.ToString(), out var routeCustomerId) &&
                long.TryParse(customerClaim, out var claimCustomerId) &&
                routeCustomerId == claimCustomerId)
            {
                context.Succeed(requirement);
            }
        }

        return Task.CompletedTask;
    }
}
