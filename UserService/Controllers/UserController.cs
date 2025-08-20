using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserService.DTOs;
using UserService.Services;
using UserService.Models;
using BCrypt.Net;
using Shared.Services;

namespace UserService.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILoggingService _loggingService;

    public UserController(IUserService userService, ILoggingService loggingService)
    {
        _userService = userService;
        _loggingService = loggingService;
    }

    [HttpGet]
    public async Task<IActionResult> GetUsers(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] string? status = null,
        [FromQuery] string? role = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortOrder = "asc",
        [FromQuery] bool includeDeleted = false)
    {
        try
        {
            _loggingService.Information("GetUsers called with page: {Page}, pageSize: {PageSize}, search: {Search}", page, pageSize, search ?? "none");
            
            var query = new UserQueryDto
            {
                Page = page,
                PageSize = pageSize,
                Search = search,
                Status = status,
                Role = role,
                SortBy = sortBy,
                SortOrder = sortOrder,
                IncludeDeleted = includeDeleted
            };

            var (users, totalCount, totalPages) = await _userService.GetUsersAsync(query);

            _loggingService.Information("GetUsers completed successfully. Found {Count} users, {Pages} pages", totalCount, totalPages);
            
            return Ok(new
            {
                success = true,
                data = users,
                pagination = new
                {
                    page,
                    pageSize,
                    totalCount,
                    totalPages,
                    hasNextPage = page < totalPages,
                    hasPreviousPage = page > 1
                }
            });
        }
        catch (Exception ex)
        {
            _loggingService.Error("GetUsers failed", ex);
            return StatusCode(500, new { success = false, message = "An error occurred while fetching users", error = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUser(Guid id)
    {
        try
        {
            _loggingService.Information("GetUser called for ID: {UserId}", id);
            
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                _loggingService.Warning("User not found for ID: {UserId}", id);
                return NotFound(new { success = false, message = "User not found" });
            }

            _loggingService.Information("GetUser completed successfully for ID: {UserId}", id);
            return Ok(new { success = true, data = user });
        }
        catch (Exception ex)
        {
            _loggingService.Error("GetUser failed for ID: {UserId}", ex, id);
            return StatusCode(500, new { success = false, message = "An error occurred while fetching user", error = ex.Message });
        }
    }



    [HttpGet("email/{email}")]
    public async Task<IActionResult> GetUserByEmail(string email)
    {
        try
        {
            var user = await _userService.GetUserByEmailAsync(email);
            if (user == null)
            {
                return NotFound(new { success = false, message = "User not found" });
            }

            return Ok(new { success = true, data = user });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "An error occurred while fetching user", error = ex.Message });
        }
    }

    [HttpGet("username/{username}")]
    public async Task<IActionResult> GetUserByUsername(string username)
    {
        try
        {
            var user = await _userService.GetUserByUsernameAsync(username);
            if (user == null)
            {
                return NotFound(new { success = false, message = "User not found" });
            }

            return Ok(new { success = true, data = user });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "An error occurred while fetching user", error = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUserDto dto)
    {   
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            return BadRequest(new { success = false, message = "Validation failed", errors });
        }

        try
        {
            var userLanguageClaim = User.FindFirst("language");
            var userLanguage = userLanguageClaim?.Value ?? "en";
            
            var result = await _userService.UpdateUserAsync(id, dto, userLanguage);
            if (result)
            {
                return Ok(new { success = true, message = "User updated successfully" });
            }
            else
            {
                return NotFound(new { success = false, message = "User not found" });
            }
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "An error occurred while updating user", error = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        try
        {
            var userLanguageClaim = User.FindFirst("language");
            var userLanguage = userLanguageClaim?.Value ?? "en";
            
            var result = await _userService.DeleteUserAsync(id, userLanguage);
            if (result)
            {
                return Ok(new { success = true, message = "User has been deactivated successfully" });
            }
            else
            {
                return NotFound(new { success = false, message = "User not found or already deactivated" });
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "An error occurred while deleting user", error = ex.Message });
        }
    }

    [HttpPatch("{id}/restore")]
    public async Task<IActionResult> RestoreUser(Guid id)
    {
        try
        {
            var userLanguageClaim = User.FindFirst("language");
            var userLanguage = userLanguageClaim?.Value ?? "en";
            
            var result = await _userService.RestoreUserAsync(id, userLanguage);
            if (result)
            {
                return Ok(new { success = true, message = "User has been restored successfully" });
            }
            else
            {
                return NotFound(new { success = false, message = "User not found or not deleted" });
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "An error occurred while restoring user", error = ex.Message });
        }
    }

    [HttpGet("statistics")]
    public async Task<IActionResult> GetUserStatistics()
    {
        try
        {
            var statistics = await _userService.GetStatisticsAsync();
            return Ok(new { success = true, data = statistics });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "An error occurred while fetching statistics", error = ex.Message });
        }
    }
    
    [HttpGet("deleted")]
    public async Task<IActionResult> GetDeletedUsers(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortOrder = "asc",
        [FromQuery] bool includeDeleted = false)

    {
        try
        {
            var query = new UserQueryDto
            {
                Page = page,
                PageSize = pageSize,
                Search = search,
                SortBy = sortBy,
                SortOrder = sortOrder,
                IncludeDeleted = true
            };

            var (users, totalCount, totalPages) = await _userService.GetDeletedUsersAsync(query);

            return Ok(new
            {
                success = true,
                data = users,
                pagination = new
                {
                    page,
                    pageSize,
                    totalCount,
                    totalPages,
                    hasNextPage = page < totalPages,
                    hasPreviousPage = page > 1
                }
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "An error occurred while fetching deleted users", error = ex.Message });
        }
    }
} 