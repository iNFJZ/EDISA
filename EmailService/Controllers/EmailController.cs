using Microsoft.AspNetCore.Mvc;
using EmailService.Services;
using EmailService.DTOs;
using EmailService.Models;
using AutoMapper;

namespace EmailService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmailController : ControllerBase
    {
        private readonly IEmailTemplateService _emailTemplateService;
        private readonly IMapper _mapper;

        public EmailController(
            IEmailTemplateService emailTemplateService,
            IMapper mapper)
        {
            _emailTemplateService = emailTemplateService;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<EmailTemplateDto>>> GetAllTemplates()
        {
            try
            {
                var templates = await _emailTemplateService.GetAllTemplatesAsync();
                var templateDtos = _mapper.Map<IEnumerable<EmailTemplateDto>>(templates);
                return Ok(templateDtos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<EmailTemplateDto>> GetTemplateById(int id)
        {
            try
            {
                var template = await _emailTemplateService.GetTemplateByIdAsync(id);
                if (template == null)
                {
                    return NotFound($"Email template not found with ID: {id}");
                }

                var templateDto = _mapper.Map<EmailTemplateDto>(template);
                return Ok(templateDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("by-name/{name}/{language}")]
        public async Task<ActionResult<EmailTemplateDto>> GetTemplateByNameAndLanguage(string name, string language)
        {
            try
            {
                var template = await _emailTemplateService.GetTemplateByNameAndLanguageAsync(name, language);
                if (template == null)
                {
                    return NotFound($"Email template not found with name: {name} and language: {language}");
                }

                var templateDto = _mapper.Map<EmailTemplateDto>(template);
                return Ok(templateDto);
            }
            catch (Exception ex)
            {
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
                    return BadRequest(ModelState);
                }

                var template = _mapper.Map<EmailTemplate>(createDto);
                var createdTemplate = await _emailTemplateService.CreateTemplateAsync(template);
                var createdDto = _mapper.Map<EmailTemplateDto>(createdTemplate);

                return CreatedAtAction(nameof(GetTemplateById), new { id = createdDto.Id }, createdDto);
            }
            catch (Exception ex)
            {
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
                    return BadRequest(ModelState);
                }

                var existingTemplate = await _emailTemplateService.GetTemplateByIdAsync(id);
                if (existingTemplate == null)
                {
                    return NotFound($"Email template not found with ID: {id}");
                }

                _mapper.Map(updateDto, existingTemplate);
                var updatedTemplate = await _emailTemplateService.UpdateTemplateAsync(existingTemplate);
                var updatedDto = _mapper.Map<EmailTemplateDto>(updatedTemplate);

                return Ok(updatedDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteTemplate(int id)
        {
            try
            {
                var template = await _emailTemplateService.GetTemplateByIdAsync(id);
                if (template == null)
                {
                    return NotFound($"Email template not found with ID: {id}");
                }

                await _emailTemplateService.DeleteTemplateAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("{id}/restore")]
        public async Task<ActionResult> RestoreTemplate(int id)
        {
            try
            {
                var template = await _emailTemplateService.GetTemplateByIdAsync(id);
                if (template == null)
                {
                    return NotFound($"Email template not found with ID: {id}");
                }

                await _emailTemplateService.RestoreTemplateAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("by-language/{language}")]
        public async Task<ActionResult<IEnumerable<EmailTemplateDto>>> GetTemplatesByLanguage(string language)
        {
            try
            {
                var templates = await _emailTemplateService.GetTemplatesByLanguageAsync(language);
                var templateDtos = _mapper.Map<IEnumerable<EmailTemplateDto>>(templates);
                return Ok(templateDtos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
