using EmailService.Models;

namespace EmailService.Repositories
{
    public interface IEmailTemplateRepository
    {
        Task<EmailTemplate?> GetByIdAsync(int id);
        Task<EmailTemplate?> GetByNameAndLanguageAsync(string name, string language);
        Task<IEnumerable<EmailTemplate>> GetAllAsync();
        Task<IEnumerable<EmailTemplate>> GetByLanguageAsync(string language);
        Task<EmailTemplate> CreateAsync(EmailTemplate template);
        Task<EmailTemplate> UpdateAsync(EmailTemplate template);
        Task<bool> DeleteAsync(int id);
        Task<bool> RestoreAsync(int id);
        Task<bool> ExistsAsync(string name, string language);
    }
}
