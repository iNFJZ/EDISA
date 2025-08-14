using Microsoft.AspNetCore.Mvc;
using EmailService.Services;
using EmailService.DTOs;
using EmailService.Models;
using AutoMapper;
using Shared.Services;

namespace EmailService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmailController : ControllerBase
    {
        private readonly IEmailTemplateService _emailTemplateService;
        private readonly IMapper _mapper;
        private readonly ILoggingService _loggingService;

        public EmailController(
            IEmailTemplateService emailTemplateService,
            IMapper mapper,
            ILoggingService loggingService)
        {
            _emailTemplateService = emailTemplateService;
            _mapper = mapper;
            _loggingService = loggingService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<EmailTemplateDto>>> GetAllTemplates()
        {
            try
            {
                _loggingService.Information("GetAllTemplates called");
                
                var templates = await _emailTemplateService.GetAllTemplatesAsync();
                var templateDtos = _mapper.Map<IEnumerable<EmailTemplateDto>>(templates);
                
                _loggingService.Information("GetAllTemplates completed successfully. Found {Count} templates", templates.Count());
                return Ok(templateDtos);
            }
            catch (Exception ex)
            {
                _loggingService.Error("GetAllTemplates failed", ex);
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<EmailTemplateDto>> GetTemplateById(int id)
        {
            try
            {
                _loggingService.Information("GetTemplateById called for ID: {Id}", id);
                
                var template = await _emailTemplateService.GetTemplateByIdAsync(id);
                if (template == null)
                {
                    _loggingService.Warning("Email template not found for ID: {Id}", id);
                    return NotFound($"Email template not found with ID: {id}");
                }

                var templateDto = _mapper.Map<EmailTemplateDto>(template);
                _loggingService.Information("GetTemplateById completed successfully for ID: {Id}", id);
                return Ok(templateDto);
            }
            catch (Exception ex)
            {
                _loggingService.Error("GetTemplateById failed for ID: {Id}", ex, id);
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("by-name/{name}/{language}")]
        public async Task<ActionResult<EmailTemplateDto>> GetTemplateByNameAndLanguage(string name, string language)
        {
            try
            {
                _loggingService.Information("GetTemplateByNameAndLanguage called for Name: {Name}, Language: {Language}", name, language);
                
                var template = await _emailTemplateService.GetTemplateByNameAndLanguageAsync(name, language);
                if (template == null)
                {
                    _loggingService.Warning("Email template not found for Name: {Name}, Language: {Language}", name, language);
                    return NotFound($"Email template not found with name: {name} and language: {language}");
                }

                var templateDto = _mapper.Map<EmailTemplateDto>(template);
                _loggingService.Information("GetTemplateByNameAndLanguage completed successfully for Name: {Name}, Language: {Language}", name, language);
                return Ok(templateDto);
            }
            catch (Exception ex)
            {
                _loggingService.Error("GetTemplateByNameAndLanguage failed for Name: {Name}, Language: {Language}", ex, name, language);
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost]
        public async Task<ActionResult<EmailTemplateDto>> CreateTemplate([FromBody] CreateEmailTemplateDto createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    _loggingService.Warning("CreateTemplate validation failed: {Errors}", string.Join(", ", errors));
                    return BadRequest(ModelState);
                }

                _loggingService.Information("CreateTemplate called for Name: {Name}, Language: {Language}", createDto.Name, createDto.Language);
                
                var template = _mapper.Map<EmailTemplate>(createDto);
                var createdTemplate = await _emailTemplateService.CreateTemplateAsync(template);
                var createdDto = _mapper.Map<EmailTemplateDto>(createdTemplate);

                _loggingService.Information("CreateTemplate completed successfully. Created ID: {Id}, Name: {Name}, Language: {Language}", 
                    createdDto.Id, createdDto.Name, createdDto.Language);
                
                return CreatedAtAction(nameof(GetTemplateById), new { id = createdDto.Id }, createdDto);
            }
            catch (Exception ex)
            {
                _loggingService.Error("CreateTemplate failed for Name: {Name}, Language: {Language}", ex, createDto.Name, createDto.Language);
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<EmailTemplateDto>> UpdateTemplate(int id, [FromBody] UpdateEmailTemplateDto updateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    _loggingService.Warning("UpdateTemplate validation failed for ID: {Id}, Errors: {Errors}", id, string.Join(", ", errors));
                    return BadRequest(ModelState);
                }

                _loggingService.Information("UpdateTemplate called for ID: {Id}", id);
                
                var existingTemplate = await _emailTemplateService.GetTemplateByIdAsync(id);
                if (existingTemplate == null)
                {
                    _loggingService.Warning("Email template not found for update ID: {Id}", id);
                    return NotFound($"Email template not found with ID: {id}");
                }

                _mapper.Map(updateDto, existingTemplate);
                var updatedTemplate = await _emailTemplateService.UpdateTemplateAsync(existingTemplate);
                var updatedDto = _mapper.Map<EmailTemplateDto>(updatedTemplate);

                _loggingService.Information("UpdateTemplate completed successfully for ID: {Id}, Name: {Name}, Language: {Language}", 
                    updatedDto.Id, updatedDto.Name, updatedDto.Language);
                
                return Ok(updatedDto);
            }
            catch (Exception ex)
            {
                _loggingService.Error("UpdateTemplate failed for ID: {Id}", ex, id);
                return StatusCode(500, ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteTemplate(int id)
        {
            try
            {
                _loggingService.Information("DeleteTemplate called for ID: {Id}", id);
                
                var template = await _emailTemplateService.GetTemplateByIdAsync(id);
                if (template == null)
                {
                    _loggingService.Warning("Email template not found for deletion ID: {Id}", id);
                    return NotFound($"Email template not found with ID: {id}");
                }

                await _emailTemplateService.DeleteTemplateAsync(id);
                _loggingService.Information("DeleteTemplate completed successfully for ID: {Id}, Name: {Name}", id, template.Name);
                return NoContent();
            }
            catch (Exception ex)
            {
                _loggingService.Error("DeleteTemplate failed for ID: {Id}", ex, id);
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("{id}/restore")]
        public async Task<ActionResult> RestoreTemplate(int id)
        {
            try
            {
                _loggingService.Information("RestoreTemplate called for ID: {Id}", id);
                
                var template = await _emailTemplateService.GetTemplateByIdAsync(id);
                if (template == null)
                {
                    _loggingService.Warning("Email template not found for restoration ID: {Id}", id);
                    return NotFound($"Email template not found with ID: {id}");
                }

                await _emailTemplateService.RestoreTemplateAsync(id);
                _loggingService.Information("RestoreTemplate completed successfully for ID: {Id}, Name: {Name}", id, template.Name);
                return NoContent();
            }
            catch (Exception ex)
            {
                _loggingService.Error("RestoreTemplate failed for ID: {Id}", ex, id);
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("by-language/{language}")]
        public async Task<ActionResult<IEnumerable<EmailTemplateDto>>> GetTemplatesByLanguage(string language)
        {
            try
            {
                _loggingService.Information("GetTemplatesByLanguage called for Language: {Language}", language);
                
                var templates = await _emailTemplateService.GetTemplatesByLanguageAsync(language);
                var templateDtos = _mapper.Map<IEnumerable<EmailTemplateDto>>(templates);
                
                _loggingService.Information("GetTemplatesByLanguage completed successfully for Language: {Language}. Found {Count} templates", 
                    language, templates.Count());
                
                return Ok(templateDtos);
            }
            catch (Exception ex)
            {
                _loggingService.Error("GetTemplatesByLanguage failed for Language: {Language}", ex, language);
                return StatusCode(500, ex.Message);
            }
        }
    }
}
