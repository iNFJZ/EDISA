using Grpc.Core;
using UserService.Services;
using UserService.DTOs;

namespace UserService.Services
{
    public class UserGrpcService : GrpcGreeter.UserService.UserServiceBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UserGrpcService> _logger;

        public UserGrpcService(IUserService userService, ILogger<UserGrpcService> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        public override async Task<global::GrpcGreeter.GetUserByIdResponse> GetUserById(global::GrpcGreeter.GetUserByIdRequest request, ServerCallContext context)
        {
            try
            {
                if (!Guid.TryParse(request.UserId, out Guid userId))
                {
                    return new global::GrpcGreeter.GetUserByIdResponse
                    {
                        Success = false,
                        Message = "Invalid user ID format"
                    };
                }

                var user = await _userService.GetUserByIdAsync(userId);
                if (user == null)
                {
                    return new global::GrpcGreeter.GetUserByIdResponse
                    {
                        Success = false,
                        Message = "User not found"
                    };
                }

                return new global::GrpcGreeter.GetUserByIdResponse
                {
                    Success = true,
                    Message = "User retrieved successfully",
                    User = new global::GrpcGreeter.UserInfo
                    {
                        Id = user.Id.ToString(),
                        Email = user.Email,
                        Username = user.Username,
                        FullName = user.FullName ?? "",
                        PhoneNumber = user.PhoneNumber ?? "",
                        Address = user.Address ?? "",
                        DateOfBirth = user.DateOfBirth?.ToString("yyyy-MM-dd") ?? "",
                        Gender = "",
                        Role = "",
                        IsActive = !user.IsDeleted,
                        CreatedAt = user.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ss"),
                        UpdatedAt = user.UpdatedAt?.ToString("yyyy-MM-ddTHH:mm:ss") ?? ""
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user by ID: {UserId}", request.UserId);
                return new global::GrpcGreeter.GetUserByIdResponse
                {
                    Success = false,
                    Message = "Internal server error"
                };
            }
        }

        public override async Task<global::GrpcGreeter.GetUserByEmailResponse> GetUserByEmail(global::GrpcGreeter.GetUserByEmailRequest request, ServerCallContext context)
        {
            try
            {
                var user = await _userService.GetUserByEmailAsync(request.Email);
                if (user == null)
                {
                    return new global::GrpcGreeter.GetUserByEmailResponse
                    {
                        Success = false,
                        Message = "User not found"
                    };
                }

                return new global::GrpcGreeter.GetUserByEmailResponse
                {
                    Success = true,
                    Message = "User retrieved successfully",
                    User = new global::GrpcGreeter.UserInfo
                    {
                        Id = user.Id.ToString(),
                        Email = user.Email,
                        Username = user.Username,
                        FullName = user.FullName ?? "",
                        PhoneNumber = user.PhoneNumber ?? "",
                        Address = user.Address ?? "",
                        DateOfBirth = user.DateOfBirth?.ToString("yyyy-MM-dd") ?? "",
                        Gender = "",
                        Role = "",
                        IsActive = !user.IsDeleted,
                        CreatedAt = user.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ss"),
                        UpdatedAt = user.UpdatedAt?.ToString("yyyy-MM-ddTHH:mm:ss") ?? ""
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user by email: {Email}", request.Email);
                return new global::GrpcGreeter.GetUserByEmailResponse
                {
                    Success = false,
                    Message = "Internal server error"
                };
            }
        }

        public override async Task<global::GrpcGreeter.UpdateUserResponse> UpdateUser(global::GrpcGreeter.UpdateUserRequest request, ServerCallContext context)
        {
            try
            {
                if (!Guid.TryParse(request.UserId, out Guid userId))
                {
                    return new global::GrpcGreeter.UpdateUserResponse
                    {
                        Success = false,
                        Message = "Invalid user ID format"
                    };
                }

                var updateDto = new UpdateUserDto
                {
                    FullName = request.FullName,
                    PhoneNumber = request.PhoneNumber,
                    Address = request.Address,
                    DateOfBirth = DateTime.TryParse(request.DateOfBirth, out var dob) ? dob : null
                };

                var success = await _userService.UpdateUserAsync(userId, updateDto);
                if (!success)
                {
                    return new global::GrpcGreeter.UpdateUserResponse
                    {
                        Success = false,
                        Message = "User not found or update failed"
                    };
                }

                // Get updated user
                var updatedUser = await _userService.GetUserByIdAsync(userId);
                if (updatedUser == null)
                {
                    return new global::GrpcGreeter.UpdateUserResponse
                    {
                        Success = false,
                        Message = "User not found after update"
                    };
                }

                return new global::GrpcGreeter.UpdateUserResponse
                {
                    Success = true,
                    Message = "User updated successfully",
                    User = new global::GrpcGreeter.UserInfo
                    {
                        Id = updatedUser.Id.ToString(),
                        Email = updatedUser.Email,
                        Username = updatedUser.Username,
                        FullName = updatedUser.FullName ?? "",
                        PhoneNumber = updatedUser.PhoneNumber ?? "",
                        Address = updatedUser.Address ?? "",
                        DateOfBirth = updatedUser.DateOfBirth?.ToString("yyyy-MM-dd") ?? "",
                        Gender = "",
                        Role = "",
                        IsActive = !updatedUser.IsDeleted,
                        CreatedAt = updatedUser.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ss"),
                        UpdatedAt = updatedUser.UpdatedAt?.ToString("yyyy-MM-ddTHH:mm:ss") ?? ""
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user: {UserId}", request.UserId);
                return new global::GrpcGreeter.UpdateUserResponse
                {
                    Success = false,
                    Message = "Internal server error"
                };
            }
        }

        public override async Task<global::GrpcGreeter.DeleteUserResponse> DeleteUser(global::GrpcGreeter.DeleteUserRequest request, ServerCallContext context)
        {
            try
            {
                if (!Guid.TryParse(request.UserId, out Guid userId))
                {
                    return new global::GrpcGreeter.DeleteUserResponse
                    {
                        Success = false,
                        Message = "Invalid user ID format"
                    };
                }

                var result = await _userService.DeleteUserAsync(userId);
                return new global::GrpcGreeter.DeleteUserResponse
                {
                    Success = result,
                    Message = result ? "User deleted successfully" : "User not found or delete failed"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user: {UserId}", request.UserId);
                return new global::GrpcGreeter.DeleteUserResponse
                {
                    Success = false,
                    Message = "Internal server error"
                };
            }
        }

        public override async Task<global::GrpcGreeter.GetUsersResponse> GetUsers(global::GrpcGreeter.GetUsersRequest request, ServerCallContext context)
        {
            try
            {
                var queryDto = new UserQueryDto
                {
                    Page = request.Page,
                    PageSize = request.PageSize,
                    SortBy = request.SortBy,
                    SortOrder = request.SortDesc ? "desc" : "asc"
                };

                var result = await _userService.GetUsersAsync(queryDto);
                var users = result.Users.Select(user => new global::GrpcGreeter.UserInfo
                {
                    Id = user.Id.ToString(),
                    Email = user.Email,
                    Username = user.Username,
                    FullName = user.FullName ?? "",
                    PhoneNumber = user.PhoneNumber ?? "",
                    Address = user.Address ?? "",
                    DateOfBirth = user.DateOfBirth?.ToString("yyyy-MM-dd") ?? "",
                    Gender = "",
                    Role = "",
                    IsActive = !user.IsDeleted,
                    CreatedAt = user.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ss"),
                    UpdatedAt = user.UpdatedAt?.ToString("yyyy-MM-ddTHH:mm:ss") ?? ""
                }).ToList();

                return new global::GrpcGreeter.GetUsersResponse
                {
                    Success = true,
                    Message = "Users retrieved successfully",
                    Users = { users },
                    TotalCount = result.TotalCount,
                    Page = queryDto.Page,
                    PageSize = queryDto.PageSize
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting users");
                return new global::GrpcGreeter.GetUsersResponse
                {
                    Success = false,
                    Message = "Internal server error"
                };
            }
        }

        public override async Task<global::GrpcGreeter.SearchUsersResponse> SearchUsers(global::GrpcGreeter.SearchUsersRequest request, ServerCallContext context)
        {
            try
            {
                var queryDto = new UserQueryDto
                {
                    Search = request.SearchTerm,
                    Page = request.Page,
                    PageSize = request.PageSize
                };

                var result = await _userService.GetUsersAsync(queryDto);
                var users = result.Users.Select(user => new global::GrpcGreeter.UserInfo
                {
                    Id = user.Id.ToString(),
                    Email = user.Email,
                    Username = user.Username,
                    FullName = user.FullName ?? "",
                    PhoneNumber = user.PhoneNumber ?? "",
                    Address = user.Address ?? "",
                    DateOfBirth = user.DateOfBirth?.ToString("yyyy-MM-dd") ?? "",
                    Gender = "",
                    Role = "",
                    IsActive = !user.IsDeleted,
                    CreatedAt = user.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ss"),
                    UpdatedAt = user.UpdatedAt?.ToString("yyyy-MM-ddTHH:mm:ss") ?? ""
                }).ToList();

                return new global::GrpcGreeter.SearchUsersResponse
                {
                    Success = true,
                    Message = "Users search completed successfully",
                    Users = { users },
                    TotalCount = result.TotalCount,
                    Page = queryDto.Page,
                    PageSize = queryDto.PageSize
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching users: {SearchTerm}", request.SearchTerm);
                return new global::GrpcGreeter.SearchUsersResponse
                {
                    Success = false,
                    Message = "Internal server error"
                };
            }
        }
    }
}
