using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using NotificationService.Data;
using NotificationService.DTOs;
using NotificationService.Hubs;
using NotificationService.Models;
using AutoMapper;
using Shared.LanguageService;

namespace NotificationService.Services
{
    public class NotificationService : INotificationService
    {
        private readonly NotificationDbContext _context;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly IMapper _mapper;
        private readonly ILanguageService _languageService;

        public NotificationService(NotificationDbContext context, IHubContext<NotificationHub> hubContext, IMapper mapper, ILanguageService languageService)
        {
            _context = context;
            _hubContext = hubContext;
            _mapper = mapper;
            _languageService = languageService;
        }

        public async Task<NotificationDto> CreateNotificationAsync(CreateNotificationDto dto, string language = "en")
        {
            var notification = _mapper.Map<Notification>(dto);
            notification.CreatedAt = DateTime.UtcNow;
            
            // Translate notification content based on language using key from JSON
            if (_languageService.IsValidLanguage(language))
            {
                var translatedTitle = _languageService.GetText(notification.Title, language);
                if (translatedTitle != notification.Title)
                {
                    notification.Title = translatedTitle;
                }
                
                var translatedMessage = _languageService.GetText(notification.Message, language);
                if (translatedMessage != notification.Message)
                {
                    notification.Message = translatedMessage;
                }
            }
            
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
            
            var notificationDto = _mapper.Map<NotificationDto>(notification);
            notificationDto.TimeAgo = _languageService.GetTimeAgo(notification.CreatedAt, language);
            
            // Send real-time notification via SignalR
            await _hubContext.Clients.User(notification.UserId).SendAsync("ReceiveNotification", notificationDto);
            
            return notificationDto;
        }

        public async Task<List<NotificationDto>> GetUserNotificationsAsync(string userId, int page = 1, int pageSize = 20, string language = "en")
        {
            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var notificationDtos = _mapper.Map<List<NotificationDto>>(notifications);
            
            foreach (var dto in notificationDtos)
            {
                dto.TimeAgo = _languageService.GetTimeAgo(dto.CreatedAt, language);
            }
            
            return notificationDtos;
        }

        public async Task<NotificationDto?> GetNotificationByIdAsync(int id, string language = "en")
        {
            var notification = await _context.Notifications.FindAsync(id);
            if (notification == null) return null;
            
            var dto = _mapper.Map<NotificationDto>(notification);
            dto.TimeAgo = _languageService.GetTimeAgo(notification.CreatedAt, language);
            
            return dto;
        }

        public async Task<bool> MarkAsReadAsync(int id, string userId)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);
            
            if (notification == null) return false;
            
            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> MarkAllAsReadAsync(string userId)
        {
            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();
            
            foreach (var notification in notifications)
            {
                notification.IsRead = true;
                notification.ReadAt = DateTime.UtcNow;
            }
            
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> GetUnreadCountAsync(string userId)
        {
            return await _context.Notifications
                .CountAsync(n => n.UserId == userId && !n.IsRead);
        }

        public async Task<bool> DeleteNotificationAsync(int id, string userId)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);
            
            if (notification == null) return false;
            
            _context.Notifications.Remove(notification);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAllNotificationsAsync(string userId)
        {
            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId)
                .ToListAsync();
            
            if (!notifications.Any()) return false;
            
            _context.Notifications.RemoveRange(notifications);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task SendNotificationToUserAsync(string userId, CreateNotificationDto dto, string language = "en")
        {
            var notification = await CreateNotificationAsync(dto, language);
            await _hubContext.Clients.User(userId).SendAsync("ReceiveNotification", notification);
        }

        public async Task SendNotificationToAllAsync(CreateNotificationDto dto, string language = "en")
        {
            var notification = await CreateNotificationAsync(dto, language);
            await _hubContext.Clients.All.SendAsync("ReceiveNotification", notification);
        }

        public async Task SendNotificationToGroupAsync(List<string> userIds, CreateNotificationDto dto, string language = "en")
        {
            foreach (var userId in userIds)
            {
                await SendNotificationToUserAsync(userId, dto, language);
            }
        }
    }
} 