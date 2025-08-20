namespace Shared.AuditModels;

public class AuthAuditEvent : AuditEvent
{
    public AuthAuditEvent()
    {
        ResourceType = "AUTH";
        ServiceName = "AuthService";
    }
}
