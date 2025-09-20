using GlobalStable.Domain.Events;

namespace GlobalStable.Domain.Interfaces.Messaging;

public interface IAuditEventPublisher
{
    Task PublishAuditEvent(AuditEvent auditEvent);
}