using Microsoft.EntityFrameworkCore;
using EmailService.Data;
using EmailService.Models;

namespace EmailService.Repositories
{
    public class EmailTemplateRepository : IEmailTemplateRepository
    {
        private readonly EmailServiceDbContext _context;

        public EmailTemplateRepository(EmailServiceDbContext context)
        {
            _context = context;
        }

        public async Task<EmailTemplate?> GetByIdAsync(int id)
        {
            return await _context.EmailTemplates.FirstOrDefaultAsync(et => et.Id == id);
        }

        public async Task<EmailTemplate?> GetByNameAndLanguageAsync(string name, string language)
        {
            return await _context.EmailTemplates
                .FirstOrDefaultAsync(et => et.Name == name && et.Language == language && et.IsActive && et.DeletedAt == null);
        }

        public async Task<IEnumerable<EmailTemplate>> GetAllAsync()
        {
            return await _context.EmailTemplates
                .OrderBy(et => et.Name)
                .ThenBy(et => et.Language)
                .ToListAsync();
        }

        public async Task<IEnumerable<EmailTemplate>> GetByLanguageAsync(string language)
        {
            return await _context.EmailTemplates
                .Where(et => et.Language == language)
                .OrderBy(et => et.Name)
                .ToListAsync();
        }

        public async Task<EmailTemplate> CreateAsync(EmailTemplate template)
        {
            template.CreatedAt = DateTime.UtcNow;
            _context.EmailTemplates.Add(template);
            await _context.SaveChangesAsync();
            return template;
        }

        public async Task<EmailTemplate> UpdateAsync(EmailTemplate template)
        {
            template.UpdatedAt = DateTime.UtcNow;
            _context.EmailTemplates.Update(template);
            await _context.SaveChangesAsync();
            return template;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var template = await _context.EmailTemplates.FindAsync(id);
            if (template == null)
                return false;

            template.IsActive = false;
            template.UpdatedAt = DateTime.UtcNow;
            template.DeletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RestoreAsync(int id)
        {
            var template = await _context.EmailTemplates.FindAsync(id);
            if (template == null)
                return false;

            template.IsActive = true;
            template.DeletedAt = null;
            template.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(string name, string language)
        {
            return await _context.EmailTemplates
                .AnyAsync(et => et.Name == name && et.Language == language && et.IsActive && et.DeletedAt == null);
        }
    }
}
