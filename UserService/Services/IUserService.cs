using UserService.DTOs;
using UserService.Models;

namespace UserService.Services;

public interface IUserService
{
    Task<(List<UserDto> Users, int TotalCount, int TotalPages)> GetUsersAsync(UserQueryDto query);
    Task<(List<UserDto> Users, int TotalCount, int TotalPages)> GetDeletedUsersAsync(UserQueryDto query);
    Task<UserDto?> GetUserByIdAsync(Guid id);
    Task<UserDto?> GetUserByEmailAsync(string email);
    Task<UserDto?> GetUserByUsernameAsync(string username);
    Task<bool> UpdateUserAsync(Guid id, UpdateUserDto dto, string language = "en");
    Task<bool> DeleteUserAsync(Guid userId, string language = "en");
    Task<bool> RestoreUserAsync(Guid id, string language = "en");
    Task<object> GetStatisticsAsync();
} 