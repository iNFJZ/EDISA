using AutoMapper;
using EmailService.Models;
using EmailService.DTOs;

namespace EmailService.Mappings
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<EmailTemplate, EmailTemplateDto>();
            CreateMap<CreateEmailTemplateDto, EmailTemplate>();
            CreateMap<UpdateEmailTemplateDto, EmailTemplate>();
            
            CreateMap<UpdateEmailTemplateDto, EmailTemplate>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
        }
    }
}
