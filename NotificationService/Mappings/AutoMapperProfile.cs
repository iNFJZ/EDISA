using AutoMapper;
using NotificationService.DTOs;
using NotificationService.Models;

namespace NotificationService.Mappings
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<CreateNotificationDto, Notification>();
            CreateMap<Notification, NotificationDto>();
        }
    }
} 