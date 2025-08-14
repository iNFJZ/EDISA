using AuthService.DTOs;
using AuthService.Services;
using AuthService.Extensions;
using Microsoft.AspNetCore.Authorization;               
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using AuthService.Exceptions;
using Shared.Services;

namespace AuthService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _auth;
        private readonly IGoogleAuthService _googleAuth;
        private readonly IConfiguration _config;
        private readonly ILoggingService _loggingService;

        public AuthController(IAuthService auth, IGoogleAuthService googleAuth, IConfiguration config, ILoggingService loggingService)
        {
            _auth = auth;
            _googleAuth = googleAuth;
            _config = config;
            _loggingService = loggingService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterWithAcceptDto dto)
        {
            try
            {
                _loggingService.Information("User registration attempt for email: {Email}", dto.Email);
                
                var (token, username, suggestedUsername, errorCode, message) = await _auth.RegisterAsync(dto, dto.AcceptSuggestedUsername);
                if (!string.IsNullOrEmpty(errorCode))
                {
                    _loggingService.Warning("User registration failed for email: {Email}, Error: {ErrorCode}", dto.Email, errorCode);
                    return BadRequest(new { errorCode, suggestedUsername, message });
                }
                
                _loggingService.Information("User registered successfully: {Username}, Email: {Email}", username, dto.Email);
                return Ok(new { token, username });
            }
            catch (Exception ex)
            {
                _loggingService.Error("Unexpected error during user registration for email: {Email}", ex, dto.Email);
                return StatusCode(500, new { message = "Internal server error during registration" });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    
                    _loggingService.Warning("Login validation failed for email: {Email}", dto.Email);
                    return BadRequest(new { success = false, message = "Validation failed", errors });
                }

                _loggingService.Information("Login attempt for email: {Email}", dto.Email);
                
                var result = await _auth.LoginAsync(dto);
                if (result.Require2FA)
                {
                    _loggingService.Information("2FA required for user: {Email}", dto.Email);
                    return Ok(new { success = false, require2FA = true, userId = result.UserId, message = result.Message });
                }
                
                _loggingService.Information("User logged in successfully: {Email}", dto.Email);
                return Ok(new { success = true, token = result.Token, message = result.Message, redirectUrl = $"{_config["Frontend:BaseUrl"]}/admin/index.html" });
            }
            catch (Exception ex)
            {
                _loggingService.Error("Login failed for email: {Email}", ex, dto.Email);
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPost("login/google")]
        public async Task<IActionResult> LoginWithGoogle([FromBody] GoogleLoginDto dto)
        {
            if (dto == null || string.IsNullOrEmpty(dto.Code) || string.IsNullOrEmpty(dto.RedirectUri))
            {
                _loggingService.Warning("Google login failed: Invalid request data");
                return BadRequest(new { message = "Code and RedirectUri are required" });
            }
            try
            {
                _loggingService.Information("Google login attempt with code: {Code}", dto.Code);
                
                var result = await _googleAuth.LoginWithGoogleAsync(dto);
                if (result.require2FA)
                {
                    _loggingService.Information("Google login requires 2FA for user: {UserId}", result.userId);
                    return Ok(new { success = false, require2FA = true, userId = result.userId, message = result.message });
                }
                
                _loggingService.Information("Google login successful for user: {UserId}", result.userId);
                return Ok(new { success = true, token = result.token, message = result.message, redirectUrl = $"{_config["Frontend:BaseUrl"]}/admin/index.html" });
            }
            catch (Exceptions.InvalidGoogleTokenException ex)
            {
                _loggingService.Warning("Google login failed: Invalid token - {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _loggingService.Error("Google login failed with unexpected error", ex);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            try
            {
                var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                var userId = userIdClaim?.Value ?? "Unknown";
                
                _loggingService.Information("Logout attempt for user: {UserId}", userId);
                
                var result = await _auth.LogoutAsync(token);
                
                if (result)
                {
                    _loggingService.Information("User logged out successfully: {UserId}", userId);
                    return Ok(new { message = "Logged out successfully" });
                }
                else
                {
                    _loggingService.Warning("Logout failed: Invalid token for user: {UserId}", userId);
                    return BadRequest(new { message = "Invalid token" });
                }
            }
            catch (Exception ex)
            {
                _loggingService.Error("Logout failed with unexpected error", ex);
                return StatusCode(500, new { message = "Internal server error during logout" });
            }
        }

        [HttpPost("validate")]
        public async Task<IActionResult> ValidateToken([FromBody] ValidateTokenRequest request)
        {
            try
            {
                if (request == null)
                {
                    _loggingService.Warning("Token validation failed: Request body is null");
                    return BadRequest(new { message = "Request body is required" });
                }
                if (string.IsNullOrEmpty(request.Token))
                {
                    _loggingService.Warning("Token validation failed: Token is empty");
                    return BadRequest(new { message = "Token is required" });
                }
                
                _loggingService.Debug("Token validation attempt for token: {Token}", request.Token);
                var isValid = await _auth.ValidateTokenAsync(request.Token);
                
                _loggingService.Debug("Token validation result: {IsValid}", isValid);
                return Ok(new { isValid = isValid });
            }
            catch (Exception ex)
            {
                _loggingService.Error("Token validation failed with unexpected error", ex);
                return StatusCode(500, new { message = "Internal server error during token validation" });
            }
        }

        [Authorize]
        [HttpGet("sessions")]
        public async Task<IActionResult> GetUserSessions()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out Guid userId))
                return Unauthorized();

            var sessions = await _auth.GetUserSessionsAsync(userId);
            return Ok(new { sessions });
        }

        [Authorize]
        [HttpDelete("sessions/{sessionId}")]
        public async Task<IActionResult> RemoveUserSession(string sessionId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out Guid userId))
                return Unauthorized();

            var result = await _auth.RemoveUserSessionAsync(userId, sessionId);
            if (result)
                return Ok(new { message = "Session removed successfully" });
            else
                return NotFound(new { message = "Session not found" });
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(new { success = false, message = "Validation failed", errors });
            }

            var clientIp = HttpContext.GetClientIpAddress();
            var result = await _auth.ForgotPasswordAsync(dto, clientIp);
            return Ok(new { 
                success = true, 
                message = "If the email exists, a password reset link has been sent to your email address.",
                redirectUrl = $"{_config["Frontend:BaseUrl"]}/auth/login.html?message=reset_link_sent"
            });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordDto dto)
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
                var result = await _auth.ResetPasswordAsync(dto);
                return Ok(new { 
                    success = true, 
                    message = "Password has been reset successfully.",
                    redirectUrl = $"{_config["Frontend:BaseUrl"]}/auth/login.html?message=password_reset_success"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword(ChangePasswordDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(new { message = "Validation failed", errors });
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out Guid userId))
                return Unauthorized();

            try
            {
                var result = await _auth.ChangePasswordAsync(userId, dto);
                return Ok(new { message = "Password has been changed successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("verify-email")]
        public async Task<IActionResult> VerifyEmail([FromQuery] string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return BadRequest(new { success = false, message = "Token is required" });
            }

            var result = await _auth.VerifyEmailAsync(token);
            if (result)
                return Ok(new { success = true, message = "Email verified successfully" });
            else
                return BadRequest(new { success = false, message = "Invalid or expired token" });
        }

        [HttpPost("resend-verification")]
        public async Task<IActionResult> ResendVerificationEmail([FromBody] ForgotPasswordDto dto)
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
                var result = await _auth.ResendVerificationEmailAsync(dto.Email, dto.Language ?? "en");
                return Ok(new { 
                    success = true, 
                    message = "Verification email has been resent successfully."
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("validate-reset-token")]
        public async Task<IActionResult> ValidateResetToken([FromQuery] string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return BadRequest(new { success = false, message = "Token is required" });
            }

            try
            {
                var email = await _auth.GetEmailFromResetTokenAsync(token);
                if (!string.IsNullOrEmpty(email))
                {
                    return Ok(new { success = true, email });
                }
                else
                {
                    return BadRequest(new { success = false, message = "Invalid or expired token" });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [Authorize]
        [HttpPost("enable-2fa-totp")]
        public async Task<IActionResult> EnableTwoFactor()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out Guid userId))
                    return Unauthorized();
                try
                {
                    var (qrCodeImage, secret) = await _auth.EnableTwoFactorAsync(userId);
                    return Ok(new { qrCodeImage, secret });
                }
                catch (Exception ex)
                {
                    if (ex is AuthService.Exceptions.AuthException authEx)
                    {
                        return BadRequest(new { success = false, errorCode = authEx.ErrorCode, message = authEx.Message });
                    }
                    return BadRequest(new { success = false, errorCode = "FAILED_TO_GENERATE_QR", message = "failedToGenerateQr" });
                }
            }
            catch (AuthException ex)
            {
                return BadRequest(new { success = false, errorCode = ex.ErrorCode, message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [Authorize]
        [HttpPost("verify-2fa-totp")]
        public async Task<IActionResult> VerifyTwoFactor([FromBody] VerifyOtpDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out Guid userId))
                return Unauthorized();
            var valid = await _auth.VerifyTwoFactorAsync(userId, dto.Code);
            if (valid) return Ok(new { success = true });
            return BadRequest(new { success = false, message = "Invalid OTP code" });
        }

        [Authorize]
        [HttpPost("disable-2fa-totp")]
        public async Task<IActionResult> DisableTwoFactor([FromBody] Disable2FADto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out Guid userId))
                return Unauthorized();
            try
            {
                var ok = await _auth.DisableTwoFactorAsync(userId, dto.Code, dto.Language);
                if (ok)
                    return Ok(new { success = true });
                return BadRequest(new { success = false });
            }
            catch (AuthException ex)
            {
                return BadRequest(new { success = false, errorCode = ex.ErrorCode, message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [Authorize]
        [HttpGet("two-factor-status")]
        public async Task<IActionResult> GetTwoFactorStatus()
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out Guid userId))
                return Unauthorized();
            var user = await _auth.GetUserByIdAsync(userId);
            if (user == null)
                return NotFound(new { success = false, message = "User not found" });
            return Ok(new { success = true, twoFactorEnabled = user.TwoFactorEnabled });
        }

        [HttpPost("verify-google-otp")]
        public async Task<IActionResult> VerifyGoogleOtp([FromBody] VerifyGoogleOtpDto dto)
        {
            if (dto == null || !dto.UserId.HasValue || string.IsNullOrEmpty(dto.OtpCode))
            {
                return BadRequest(new { message = "UserId and OtpCode are required" });
            }
            
            try
            {
                var result = await _googleAuth.VerifyGoogleOtpAsync(dto.UserId.Value, dto.OtpCode, dto.Language);
                
                if (result.success)
                {
                    return Ok(new { success = true, token = result.token, message = result.message, redirectUrl = $"{_config["Frontend:BaseUrl"]}/admin/index.html" });
                }
                else
                {
                    return Ok(new { success = false, require2FA = true, userId = dto.UserId, message = result.message });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("test-logging")]
        public IActionResult TestLogging()
        {
            try
            {
                _loggingService.Debug("Debug message test");
                _loggingService.Information("Information message test");
                _loggingService.Warning("Warning message test");
                _loggingService.Error("Error message test", new Exception("Test exception"));
                
                return Ok(new { 
                    message = "Logging test completed", 
                    timestamp = DateTime.UtcNow,
                    logs = new[] { "Debug", "Information", "Warning", "Error" }
                });
            }
            catch (Exception ex)
            {
                _loggingService.Fatal("Fatal error in test logging", ex);
                return StatusCode(500, new { message = "Logging test failed", error = ex.Message });
            }
        }

        [HttpGet("test-logging-public")]
        [AllowAnonymous]
        public IActionResult TestLoggingPublic()
        {
            try
            {
                _loggingService.Debug("Public Debug message test");
                _loggingService.Information("Public Information message test");
                _loggingService.Warning("Public Warning message test");
                _loggingService.Error("Public Error message test", new Exception("Public Test exception"));
                
                return Ok(new { 
                    message = "Public Logging test completed", 
                    timestamp = DateTime.UtcNow,
                    logs = new[] { "Debug", "Information", "Warning", "Error" }
                });
            }
            catch (Exception ex)
            {
                _loggingService.Fatal("Public Fatal error in test logging", ex);
                return StatusCode(500, new { message = "Public Logging test failed", error = ex.Message });
            }
        }


    }
}
