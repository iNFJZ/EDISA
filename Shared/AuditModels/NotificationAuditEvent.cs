namespace Shared.AuditModels;

public class NotificationAuditEvent : AuditEvent
{
    public NotificationAuditEvent()
    {
        ResourceType = "NOTIFICATION";
        ServiceName = "NotificationService";
    }
}
