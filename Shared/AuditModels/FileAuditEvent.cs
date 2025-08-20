namespace Shared.AuditModels;

public class FileAuditEvent : AuditEvent
{
    public FileAuditEvent()
    {
        ResourceType = "FILE";
        ServiceName = "FileService";
    }
}
