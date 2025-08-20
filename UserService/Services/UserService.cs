using AutoMapper;
using UserService.DTOs;
using UserService.Models;
using UserService.Repositories;
using Shared.EmailModels;
using UserService.Services;
using System.Text.Json;
using Shared.LanguageService;
using Shared.Services;
using Shared.AuditModels;

namespace UserService.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    private readonly ISessionService _sessionService;
    private readonly IEmailMessageService _emailMessageService;
    private readonly IUserCacheService _userCacheService;
    private readonly INotificationService _notificationService;
    private readonly ILanguageService _languageService;
    private readonly IAuditHelper _auditHelper;

            public UserService(
            IUserRepository userRepository, 
            IMapper mapper, 
            ISessionService sessionService, 
            IEmailMessageService emailMessageService,
            IUserCacheService userCacheService,
            INotificationService notificationService,
            ILanguageService languageService,
            IAuditHelper auditHelper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
            _sessionService = sessionService;
            _emailMessageService = emailMessageService;
            _userCacheService = userCacheService;
            _notificationService = notificationService;
            _languageService = languageService;
            _auditHelper = auditHelper;
        }

    public async Task<(List<UserDto> Users, int TotalCount, int TotalPages)> GetUsersAsync(UserQueryDto query)
    {
        List<User> users;
        
        if (query.IncludeDeleted)
        {
            users = await _userRepository.GetAllAsync();
        }
        else
        {
            users = await _userRepository.GetAllActiveAsync();
        }

        if (query.IncludeDeleted && !string.IsNullOrEmpty(query.Status) && Enum.TryParse<UserStatus>(query.Status, true, out var userStatus) && userStatus == UserStatus.Banned)
        {
            users = users.Where(u => u.Status == UserStatus.Banned && u.DeletedAt != null).ToList();
        }
        else if (!string.IsNullOrEmpty(query.Status) && Enum.TryParse<UserStatus>(query.Status, true, out var userStatus2))
        {
            users = users.Where(u => u.Status == userStatus2).ToList();
        }

        if (!string.IsNullOrEmpty(query.Search))
        {
            users = users.Where(u => 
                u.Username.Contains(query.Search, StringComparison.OrdinalIgnoreCase) ||
                u.Email.Contains(query.Search, StringComparison.OrdinalIgnoreCase) ||
                (u.FullName != null && u.FullName.Contains(query.Search, StringComparison.OrdinalIgnoreCase))
            ).ToList();
        }

        if (!string.IsNullOrEmpty(query.SortBy))
        {
            users = query.SortBy.ToLower() switch
            {
                "username" => query.SortOrder?.ToLower() == "desc" 
                    ? users.OrderByDescending(u => u.Username).ToList()
                    : users.OrderBy(u => u.Username).ToList(),
                "email" => query.SortOrder?.ToLower() == "desc"
                    ? users.OrderByDescending(u => u.Email).ToList()
                    : users.OrderBy(u => u.Email).ToList(),
                "fullname" => query.SortOrder?.ToLower() == "desc"
                    ? users.OrderByDescending(u => u.FullName).ToList()
                    : users.OrderBy(u => u.FullName).ToList(),
                "createdat" => query.SortOrder?.ToLower() == "desc"
                    ? users.OrderByDescending(u => u.CreatedAt).ToList()
                    : users.OrderBy(u => u.CreatedAt).ToList(),
                "lastloginat" => query.SortOrder?.ToLower() == "desc"
                    ? users.OrderByDescending(u => u.LastLoginAt).ToList()
                    : users.OrderBy(u => u.LastLoginAt).ToList(),
                _ => users.OrderBy(u => u.CreatedAt).ToList()
            };
        }
        else
        {
            users = users.OrderByDescending(u => u.CreatedAt).ToList();
        }

        var totalCount = users.Count;
        var totalPages = (int)Math.Ceiling((double)totalCount / query.PageSize);
        var skip = (query.Page - 1) * query.PageSize;
        var pagedUsers = users.Skip(skip).Take(query.PageSize).ToList();

        var userDtos = _mapper.Map<List<UserDto>>(pagedUsers);

        return (userDtos, totalCount, totalPages);
    }

    public async Task<(List<UserDto> Users, int TotalCount, int TotalPages)> GetDeletedUsersAsync(UserQueryDto query)
    {
        var users = await _userRepository.GetAllDeletedAsync();

        if (!string.IsNullOrEmpty(query.Search))
        {
            users = users.Where(u => 
                u.Username.Contains(query.Search, StringComparison.OrdinalIgnoreCase) ||
                u.Email.Contains(query.Search, StringComparison.OrdinalIgnoreCase) ||
                (u.FullName != null && u.FullName.Contains(query.Search, StringComparison.OrdinalIgnoreCase))
            ).ToList();
        }

        if (!string.IsNullOrEmpty(query.SortBy))
        {
            users = query.SortBy.ToLower() switch
            {
                "username" => query.SortOrder?.ToLower() == "desc" 
                    ? users.OrderByDescending(u => u.Username).ToList()
                    : users.OrderBy(u => u.Username).ToList(),
                "email" => query.SortOrder?.ToLower() == "desc"
                    ? users.OrderByDescending(u => u.Email).ToList()
                    : users.OrderBy(u => u.Email).ToList(),
                "fullname" => query.SortOrder?.ToLower() == "desc"
                    ? users.OrderByDescending(u => u.FullName).ToList()
                    : users.OrderBy(u => u.FullName).ToList(),
                "deletedat" => query.SortOrder?.ToLower() == "desc"
                    ? users.OrderByDescending(u => u.DeletedAt).ToList()
                    : users.OrderBy(u => u.DeletedAt).ToList(),
                "createdat" => query.SortOrder?.ToLower() == "desc"
                    ? users.OrderByDescending(u => u.CreatedAt).ToList()
                    : users.OrderBy(u => u.CreatedAt).ToList(),
                _ => users.OrderByDescending(u => u.DeletedAt).ToList()
            };
        }
        else
        {
            users = users.OrderByDescending(u => u.DeletedAt).ToList();
        }

        var totalCount = users.Count;
        var totalPages = (int)Math.Ceiling((double)totalCount / query.PageSize);
        var skip = (query.Page - 1) * query.PageSize;
        var pagedUsers = users.Skip(skip).Take(query.PageSize).ToList();

        var userDtos = _mapper.Map<List<UserDto>>(pagedUsers);

        return (userDtos, totalCount, totalPages);
    }

    public async Task<UserDto?> GetUserByIdAsync(Guid id)
    {
        var cachedUser = await _userCacheService.GetUserByIdAsync(id);
        if (cachedUser != null)
        {
            return _mapper.Map<UserDto>(cachedUser);
        }

        var user = await _userRepository.GetByIdAsync(id);
        if (user != null)
        {
            await _userCacheService.SetUserAsync(user, TimeSpan.FromMinutes(30));
        }
        
        return _mapper.Map<UserDto>(user);
    }

    public async Task<UserDto?> GetUserByEmailAsync(string email)
    {
        var cachedUser = await _userCacheService.GetUserByEmailAsync(email);
        if (cachedUser != null)
        {
            return _mapper.Map<UserDto>(cachedUser);
        }

        var user = await _userRepository.GetByEmailAsync(email);
        if (user != null)
        {
            await _userCacheService.SetUserAsync(user, TimeSpan.FromMinutes(30));
        }
        
        return _mapper.Map<UserDto>(user);
    }

    public async Task<UserDto?> GetUserByUsernameAsync(string username)
    {
        var user = await _userRepository.GetByUsernameAsync(username);
        if (user != null)
        {
            await _userCacheService.SetUserAsync(user, TimeSpan.FromMinutes(30));
        }
        
        return _mapper.Map<UserDto>(user);
    }

    public async Task<bool> UpdateUserAsync(Guid id, UpdateUserDto dto, string language = "en")
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
            return false;

        var oldValues = new
        {
            Email = user.Email,
            Username = user.Username,
            FullName = user.FullName,
            PhoneNumber = user.PhoneNumber,
            Avatar = user.ProfilePicture,
            Status = user.Status.ToString()
        };

        var originalEmail = user.Email;
        var originalUsername = user.Username;

        _mapper.Map(dto, user);
        user.UpdatedAt = DateTime.UtcNow;

        var updatedUser = await _userRepository.UpdateAsync(user);
        if (updatedUser != null)
        {
            await _userCacheService.SetUserAsync(updatedUser, TimeSpan.FromMinutes(30));

            if (originalEmail != updatedUser.Email)
            {
                await _sessionService.RemoveAllUserSessionsAsync(updatedUser.Id);
                await _sessionService.RemoveAllActiveTokensForUserAsync(updatedUser.Id);
            }

            if (originalUsername != updatedUser.Username)
            {
                await _sessionService.RemoveAllUserSessionsAsync(updatedUser.Id);
                await _sessionService.RemoveAllActiveTokensForUserAsync(updatedUser.Id);
            }

            var notificationData = System.Text.Json.JsonSerializer.Serialize(new
            {
                userId = updatedUser.Id,
                email = updatedUser.Email,
                username = updatedUser.Username,
                fullName = updatedUser.FullName,
                updatedAt = updatedUser.UpdatedAt
            });

            var title = _languageService.GetText("profileUpdated", language);
            var message = _languageService.GetText("profileUpdatedMessage", language);
            
            await _notificationService.SendNotificationAsync(
                updatedUser.Id.ToString(),
                title,
                message,
                "info",
                notificationData
            );

            var newValues = new
            {
                Email = updatedUser.Email,
                Username = updatedUser.Username,
                FullName = updatedUser.FullName,
                PhoneNumber = updatedUser.PhoneNumber,
                Avatar = updatedUser.ProfilePicture,
                Status = updatedUser.Status.ToString()
            };

            var auditEvent = new UserAuditEvent
            {
                UserId = updatedUser.Id.ToString(),
                UserEmail = updatedUser.Email,
                Action = "UPDATE_USER",
                ResourceId = updatedUser.Id.ToString(),
                OldValues = oldValues,
                NewValues = newValues,
                Success = true,
                Metadata = new Dictionary<string, object>
                {
                    { "EmailChanged", originalEmail != updatedUser.Email },
                    { "UsernameChanged", originalUsername != updatedUser.Username },
                    { "Language", language }
                }
            };

            // Fire-and-forget audit logging
            _ = Task.Run(async () =>
            {
                try
                {
                    await _auditHelper.LogEventAsync(auditEvent);
                }
                catch (Exception ex)
                {
                    // Log warning but don't throw to avoid affecting business logic
                    Console.WriteLine($"Failed to log audit event for user update: {ex.Message}");
                }
            });
        }

        return updatedUser != null;
    }

    public async Task<bool> DeleteUserAsync(Guid userId, string language = "en")
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null || user.IsDeleted)
            return false;

        user.DeletedAt = DateTime.UtcNow;
        user.Status = UserStatus.Banned;
        user.IsVerified = false;
        await _userRepository.UpdateAsync(user);

        await _userCacheService.DeleteUserAsync(userId);

        await _sessionService.RemoveAllUserSessionsAsync(user.Id);
        await _sessionService.RemoveAllActiveTokensForUserAsync(user.Id);
        await _sessionService.SetUserLoginStatusAsync(user.Id, false);

        await _emailMessageService.PublishDeactivateAccountNotificationAsync(new DeactivateAccountEmailEvent
        {
            To = user.Email,
            Username = user.FullName ?? user.Username,
            DeactivatedAt = DateTime.UtcNow,
            Reason = "Account deactivated by administrator",
            Language = language
        });

        var auditEvent = new UserAuditEvent
        {
            UserId = user.Id.ToString(),
            UserEmail = user.Email,
            Action = "DELETE_USER",
            ResourceId = user.Id.ToString(),
            Success = true,
            Metadata = new Dictionary<string, object>
            {
                { "DeletedAt", DateTime.UtcNow },
                { "Reason", "Account deactivated by administrator" },
                { "Language", language }
            }
        };

        _ = Task.Run(async () =>
        {
            try
            {
                await _auditHelper.LogEventAsync(auditEvent);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to log audit event for user deletion: {ex.Message}");
            }
        });

        return true;
    }

    public async Task<bool> RestoreUserAsync(Guid id, string language = "en")
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null || !user.IsDeleted)
            return false;

        var result = await _userRepository.RestoreAsync(id);
        if (result)
        {
            await _userCacheService.SetUserAsync(user, TimeSpan.FromMinutes(30));

            await _emailMessageService.PublishRestoreAccountNotificationAsync(new RestoreAccountEmailEvent
            {
                To = user.Email,
                Username = user.FullName ?? user.Username,
                RestoredAt = DateTime.UtcNow,
                Reason = "Account restored by administrator",
                Language = language
            });
            
            var auditEvent = new UserAuditEvent
        {
            UserId = user.Id.ToString(),
            UserEmail = user.Email,
            Action = "RESTORE_USER",
            ResourceId = user.Id.ToString(),
            Success = true,
            Metadata = new Dictionary<string, object>
            {
                { "RestoredAt", DateTime.UtcNow },
                { "Reason", "Account restored by administrator" },
                { "Language", language }
            }
        };

        _ = Task.Run(async () =>
        {
            try
            {
                await _auditHelper.LogEventAsync(auditEvent);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to log audit event for user restoration: {ex.Message}");
            }
        });
        }
        return result;
    }



    public async Task<object> GetStatisticsAsync()
    {
        var users = await _userRepository.GetAllActiveAsync();
        
        var totalUsers = users.Count;
        var activeUsers = users.Count(u => u.Status == UserStatus.Active);
        var inactiveUsers = users.Count(u => u.Status == UserStatus.Inactive);
        var bannedUsers = users.Count(u => u.Status == UserStatus.Banned && u.DeletedAt != null);
        var verifiedUsers = users.Count(u => u.IsVerified);
        var unverifiedUsers = users.Count(u => !u.IsVerified);
        
        var usersByProvider = users.GroupBy(u => u.LoginProvider)
            .ToDictionary(g => g.Key, g => g.Count());
        
        var recentUsers = users.Where(u => u.CreatedAt >= DateTime.UtcNow.AddDays(7)).Count();
        
        return new
        {
            TotalUsers = totalUsers,
            ActiveUsers = activeUsers,
            InactiveUsers = inactiveUsers,
            BannedUsers = bannedUsers,
            VerifiedUsers = verifiedUsers,
            UnverifiedUsers = unverifiedUsers,
            UsersByProvider = usersByProvider,
            RecentUsers = recentUsers
        };
    }


} 