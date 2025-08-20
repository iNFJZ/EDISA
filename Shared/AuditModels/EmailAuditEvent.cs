namespace Shared.AuditModels;

public class EmailAuditEvent : AuditEvent
{
    public EmailAuditEvent()
    {
        ResourceType = "EMAIL";
        ServiceName = "EmailService";
    }
}
