using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using GlobalStable.Domain.DTOs;
using GlobalStable.Domain.Events;
using GlobalStable.Domain.Helpers;
using GlobalStable.Domain.Interfaces.Messaging;

namespace GlobalStable.API.Middlewares;

public class AuditMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AuditMiddleware> _logger;
    private const string GlobalStableName = "GlobalStable";

    public AuditMiddleware(RequestDelegate next, ILogger<AuditMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IAuditEventPublisher auditEventPublisher)
    {

        if (context.Response.StatusCode > 300)
            return;

        context.Request.EnableBuffering();

        var originalBodyStream = context.Response.Body;
        using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        try
        {
            await _next(context);
        }
        finally
        {
            context.Response.Body.Seek(0, SeekOrigin.Begin);
            string responseBodyText = await new StreamReader(context.Response.Body).ReadToEndAsync();
            context.Response.Body.Seek(0, SeekOrigin.Begin);
            await responseBody.CopyToAsync(originalBodyStream);

            var request = context.Request;

            if (request.Path.StartsWithSegments("/check_server_status") ||
                request.Path.StartsWithSegments("/swagger") ||
                request.Method.Equals(HttpMethod.Get.ToString(), StringComparison.InvariantCultureIgnoreCase))
            {
                goto End;
            }

            var userAudit = GetUserAuditFromToken(context);
            long? entityId = ExtractEntityIdFromResponse(responseBodyText);

            var auditEvent = new AuditEvent
            {
                UserId = userAudit.UserId,
                CustomerId = ExtractCustomerIdFromPath(context),
                Operation = MapToAuditEvent.MapMethodToOperation(request.Method),
                EntityId = entityId,
                EntityTable = ExtractEntityTableFromPath(request.Path),
                OriginApplication = GlobalStableName,
                RequestPath = request.Path,
                ApplicationInstance = Environment.MachineName,
                Data = await ReadRequestBodyAsync(request),
                OperationTimestamp = DateTime.UtcNow,
            };

            try
            {
                await auditEventPublisher.PublishAuditEvent(auditEvent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish audit event.");
            }

        End:;
        }
    }

    private static UserAudit GetUserAuditFromToken(HttpContext context)
    {
        var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();

        var token = authHeader!.Split("Bearer ").LastOrDefault();

        var handler = new JwtSecurityTokenHandler();

        var jwtToken = handler.ReadJwtToken(token);
        var username = jwtToken.Claims.FirstOrDefault(c => c.Type == "username")?.Value;
        var eventId = jwtToken.Claims.FirstOrDefault(c => c.Type == "event_id")?.Value;
        var userAudit = new UserAudit(username, eventId);
        return userAudit;
    }

    private static long? ExtractCustomerIdFromPath(HttpContext context)
    {
        if (context.Request.RouteValues.TryGetValue("customerId", out var customerId) && customerId != null)
        {
            if (long.TryParse(customerId.ToString(), out var id))
                return id;
        }

        return null;
    }

    private static async Task<string?> ReadRequestBodyAsync(HttpRequest request)
    {
        if (request.ContentLength == 0) return null;

        request.Body.Position = 0;
        using var reader = new StreamReader(request.Body, leaveOpen: true);
        var body = await reader.ReadToEndAsync();
        request.Body.Position = 0;
        return body;
    }

    private static long? ExtractEntityIdFromResponse(string responseBodyText)
    {
        try
        {
            using var doc = JsonDocument.Parse(responseBodyText);

            // Tenta acessar result.id
            if (doc.RootElement.TryGetProperty("result", out var resultElement))
            {
                if (resultElement.TryGetProperty("id", out var idProp) && 
                    idProp.ValueKind == JsonValueKind.Number &&
                    idProp.TryGetInt64(out var id))
                {
                    return id;
                }
            }
        }
        catch
        {
            // Ignora erros de parsing
        }

        return null;
    }

    private static string? ExtractEntityTableFromPath(string path)
    {
        var normalizedPath = path.ToLowerInvariant();

        if (normalizedPath.Contains("/orders/deposit"))
            return "deposit_order";

        if (normalizedPath.Contains("/orders/withdrawal"))
            return "withdrawal_order";

        if (normalizedPath.Contains("/fee"))
            return "fee_config";

        var segments = normalizedPath.Split('/', StringSplitOptions.RemoveEmptyEntries);
        return segments.Length > 0 ? segments[^1] : null; // último segmento
    }

}
