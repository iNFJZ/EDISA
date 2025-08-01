using System.Threading.Tasks;

namespace UserService.Services
{
    public interface INotificationService
    {
        Task SendNotificationAsync(string userId, string title, string message, string type = "info", string? data = null);
    }
} 