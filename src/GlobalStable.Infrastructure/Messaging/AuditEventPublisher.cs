using GlobalStable.Domain.Constants;
using GlobalStable.Domain.Events;
using GlobalStable.Domain.Interfaces.Messaging;

namespace GlobalStable.Infrastructure.Messaging;

public class AuditEventPublisher(IEventPublisher eventPublisher) : IAuditEventPublisher
{
    public async Task PublishAuditEvent(AuditEvent auditEvent)
    {
        await eventPublisher.PublishEvent(auditEvent, MessagingKeys.Audit);
    }
}