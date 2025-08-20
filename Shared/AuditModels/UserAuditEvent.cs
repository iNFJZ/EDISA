namespace Shared.AuditModels;

public class UserAuditEvent : AuditEvent
{
    public UserAuditEvent()
    {
        ResourceType = "USER";
        ServiceName = "UserService";
    }
}
