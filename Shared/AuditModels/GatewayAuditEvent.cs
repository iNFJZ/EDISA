namespace Shared.AuditModels;

public class GatewayAuditEvent : AuditEvent
{
    public GatewayAuditEvent()
    {
        ResourceType = "API";
        ServiceName = "GatewayApi";
    }
}
