using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SakuraHomeAPI.Data;
using SakuraHomeAPI.DTOs.Common;
using SakuraHomeAPI.DTOs.Users;
using SakuraHomeAPI.DTOs.Users.Requests;
using SakuraHomeAPI.DTOs.Users.Responses;
using SakuraHomeAPI.Models.Entities.Identity;
using SakuraHomeAPI.Models.Entities.UserWishlist;
using SakuraHomeAPI.Models.Entities;
using SakuraHomeAPI.Models.Enums;
using SakuraHomeAPI.Services.Interfaces;
using System.Net;

namespace SakuraHomeAPI.Services.Implementations
{
    /// <summary>
    /// Authentication service implementation
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly ITokenService _tokenService;
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<AuthService> _logger;
        private readonly IConfiguration _configuration;

        public AuthService(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            ITokenService tokenService,
            ApplicationDbContext context,
            IMapper mapper,
            ILogger<AuthService> logger,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
            _context = context;
            _mapper = mapper;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<ApiResponse<AuthResponseDto>> LoginAsync(LoginRequestDto request, string ipAddress)
        {
            try
            {
                _logger.LogInformation("Attempting login for email: {Email}", request.Email);

                // Find user by email
                var user = await _userManager.FindByEmailAsync(request.Email);
                if (user == null)
                {
                    _logger.LogWarning("Login attempt with non-existent email: {Email}", request.Email);
                    return ApiResponse.ErrorResult<AuthResponseDto>("Email hoặc mật khẩu không đúng");
                }

                // Check if user is deleted or inactive
                if (user.IsDeleted || !user.IsActive)
                {
                    _logger.LogWarning("Login attempt for inactive/deleted user: {UserId}", user.Id);
                    return ApiResponse.ErrorResult<AuthResponseDto>("Tài khoản đã bị vô hiệu hóa");
                }

                // Check if user is locked out
                if (user.IsLocked)
                {
                    _logger.LogWarning("Login attempt for locked user: {UserId}", user.Id);
                    return ApiResponse.ErrorResult<AuthResponseDto>("Tài khoản đã bị khóa. Vui lòng thử lại sau");
                }

                // Check password
                var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, true);
                if (!result.Succeeded)
                {
                    _logger.LogWarning("Failed login attempt for user: {UserId}", user.Id);

                    if (result.IsLockedOut)
                    {
                        return ApiResponse.ErrorResult<AuthResponseDto>("Tài khoản đã bị khóa do quá nhiều lần đăng nhập sai");
                    }

                    return ApiResponse.ErrorResult<AuthResponseDto>("Email hoặc mật khẩu không đúng");
                }

                // Update user login information
                user.LastLoginAt = DateTime.UtcNow;
                user.LastLoginIp = ipAddress;
                await _userManager.UpdateAsync(user);

                // Log user activity
                await LogUserActivity(user.Id, ActivityType.Login, ipAddress);

                // Generate tokens
                var accessToken = _tokenService.GenerateAccessToken(user);
                var refreshToken = _tokenService.GenerateRefreshToken();
                var expiresAt = _tokenService.GetTokenExpiration();

                // Save refresh token
                await SaveRefreshTokenAsync(user.Id, refreshToken, ipAddress);

                var userDto = _mapper.Map<UserDto>(user);
                var authResponse = new AuthResponseDto
                {
                    Token = accessToken,
                    RefreshToken = refreshToken,
                    ExpiresAt = expiresAt,
                    User = userDto
                };

                _logger.LogInformation("Successful login for user: {UserId}", user.Id);
                return ApiResponse.SuccessResult(authResponse, "đăng nhập thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for email: {Email}", request.Email);
                return ApiResponse.ErrorResult<AuthResponseDto>("Có lỗi xảy ra trong quá trình đăng nhập");
            }
        }

        public async Task<ApiResponse<AuthResponseDto>> RegisterAsync(RegisterRequestDto request, string ipAddress)
        {
            try
            {
                _logger.LogInformation("Attempting registration for email: {Email}", request.Email);

                // Additional validation for AcceptTerms at service level
                if (!request.AcceptTerms)
                {
                    _logger.LogWarning("Registration attempt without accepting terms for email: {Email}", request.Email);
                    return ApiResponse.ErrorResult<AuthResponseDto>("Bạn phải chấp nhận điều khoản sử dụng để tiếp tục đăng ký");
                }

                // Check if registration is enabled
                var registrationEnabled = await GetSystemSetting("EnableRegistration", "true");
                if (registrationEnabled.ToLower() != "true")
                {
                    _logger.LogWarning("Registration attempt when registration is disabled for email: {Email}", request.Email);
                    return ApiResponse.ErrorResult<AuthResponseDto>("Đăng ký tài khoản hiện không được phép");
                }

                // Check if user already exists
                var existingUser = await _userManager.FindByEmailAsync(request.Email);
                if (existingUser != null)
                {
                    _logger.LogWarning("Registration attempt with existing email: {Email}", request.Email);
                    return ApiResponse.ErrorResult<AuthResponseDto>("Email này đã được sử dụng");
                }

                // Create new user với default values cho các field optional
                var user = new User
                {
                    Email = request.Email,
                    UserName = request.Email,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    PhoneNumber = request.PhoneNumber, // có thể null
                    DateOfBirth = request.DateOfBirth, // có thể null
                    Gender = request.Gender ?? Gender.Unknown, // default là Unknown
                    PreferredLanguage = !string.IsNullOrEmpty(request.PreferredLanguage) ? request.PreferredLanguage : "vi",
                    EmailNotifications = request.EmailNotifications, // default true
                    SmsNotifications = request.SmsNotifications, // default false
                    Status = AccountStatus.Active,
                    Role = UserRole.Customer,
                    Provider = LoginProvider.Local,
                    CreatedAt = DateTime.UtcNow,
                    LastLoginIp = ipAddress,
                    // Set default values for other optional fields
                    PreferredCurrency = "VND",
                    Points = 0,
                    TotalSpent = 0,
                    TotalOrders = 0,
                    Tier = UserTier.Bronze,
                    IsActive = true,
                    IsDeleted = false,
                    EmailVerified = false,
                    PhoneVerified = false,
                    PushNotifications = true
                };

                _logger.LogInformation("Creating user for email: {Email}", request.Email);

                // Create user with password
                var result = await _userManager.CreateAsync(user, request.Password);
                if (!result.Succeeded)
                {
                    var errors = result.Errors.Select(e => e.Description).ToList();
                    _logger.LogWarning("Registration failed for email: {Email}. Errors: {Errors}", 
                        request.Email, string.Join(", ", errors));
                    return ApiResponse.ErrorResult<AuthResponseDto>("Đăng ký không thành công", errors);
                }

                _logger.LogInformation("User created successfully for email: {Email}, UserId: {UserId}", request.Email, user.Id);

                // Generate email verification token if needed
                user.GenerateEmailVerificationToken();
                await _userManager.UpdateAsync(user);

                _logger.LogInformation("Creating wishlist for user: {UserId}", user.Id);

                // Create wishlist for new user
                var wishlist = new Wishlist
                {
                    UserId = user.Id,
                    Name = "Danh sách yêu thích của tôi"
                };
                _context.Wishlists.Add(wishlist);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Wishlist created for user: {UserId}", user.Id);

                // Log user activity
                await LogUserActivity(user.Id, ActivityType.Register, ipAddress);

                _logger.LogInformation("Generating tokens for user: {UserId}", user.Id);

                // Generate tokens
                var accessToken = _tokenService.GenerateAccessToken(user);
                var refreshToken = _tokenService.GenerateRefreshToken();
                var expiresAt = _tokenService.GetTokenExpiration();

                // Save refresh token
                await SaveRefreshTokenAsync(user.Id, refreshToken, ipAddress);

                var userDto = _mapper.Map<UserDto>(user);
                var authResponse = new AuthResponseDto
                {
                    Token = accessToken,
                    RefreshToken = refreshToken,
                    ExpiresAt = expiresAt,
                    User = userDto
                };

                _logger.LogInformation("Successful registration for user: {UserId}", user.Id);
                return ApiResponse.SuccessResult(authResponse, "Đăng ký thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration for email: {Email}", request.Email);
                return ApiResponse.ErrorResult<AuthResponseDto>("Có lỗi xảy ra trong quá trình đăng ký");
            }
        }

        public async Task<ApiResponse> LogoutAsync(Guid userId, string? refreshToken = null)
        {
            try
            {
                _logger.LogInformation("Logging out user: {UserId}", userId);

                // Log user activity
                await LogUserActivity(userId, ActivityType.Logout);

                // Revoke refresh token if provided
                if (!string.IsNullOrEmpty(refreshToken))
                {
                    await RevokeRefreshTokenAsync(refreshToken, "Logout");
                }

                return ApiResponse.SuccessResult("Đăng xuất thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout for user: {UserId}", userId);
                return ApiResponse.ErrorResult("Có lỗi xảy ra trong quá trình đăng xuất");
            }
        }

        public async Task<ApiResponse<AuthResponseDto>> RefreshTokenAsync(RefreshTokenRequestDto request, string ipAddress)
        {
            try
            {
                // Validate and get user from access token
                var userId = _tokenService.GetUserIdFromToken(request.AccessToken);
                if (userId == null)
                {
                    return ApiResponse.ErrorResult<AuthResponseDto>("Token không hợp lệ");
                }

                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user == null || user.IsDeleted || !user.IsActive)
                {
                    return ApiResponse.ErrorResult<AuthResponseDto>("Người dùng không tồn tại hoặc đã bị vô hiệu hóa");
                }

                // Validate refresh token
                var storedRefreshToken = await _context.RefreshTokens
                    .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken && rt.UserId == userId);

                if (storedRefreshToken == null || !storedRefreshToken.IsActive)
                {
                    return ApiResponse.ErrorResult<AuthResponseDto>("Refresh token không hợp lệ hoặc đã hết hạn");
                }

                // Generate new tokens
                var accessToken = _tokenService.GenerateAccessToken(user);
                var newRefreshToken = _tokenService.GenerateRefreshToken();
                var expiresAt = _tokenService.GetTokenExpiration();

                // Revoke old refresh token and save new one
                storedRefreshToken.Revoke("Replaced by new token", ipAddress, newRefreshToken);
                await SaveRefreshTokenAsync(user.Id, newRefreshToken, ipAddress);
                await _context.SaveChangesAsync();

                var userDto = _mapper.Map<UserDto>(user);
                var authResponse = new AuthResponseDto
                {
                    Token = accessToken,
                    RefreshToken = newRefreshToken,
                    ExpiresAt = expiresAt,
                    User = userDto
                };

                return ApiResponse.SuccessResult(authResponse, "Token đã được làm mới");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing token");
                return ApiResponse.ErrorResult<AuthResponseDto>("Có lỗi xảy ra khi làm mới token");
            }
        }

        public async Task<ApiResponse> ForgotPasswordAsync(ForgotPasswordRequestDto request)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(request.Email);
                if (user == null || user.IsDeleted || !user.IsActive)
                {
                    // Don't reveal that the user doesn't exist for security
                    return ApiResponse.SuccessResult("Nếu email tồn tại, link đặt lại mật khẩu đã được gửi");
                }

                // Generate password reset token
                user.GeneratePasswordResetToken(24); // 24 hours expiration
                await _userManager.UpdateAsync(user);

                // TODO: Send email with reset link
                // await _emailService.SendPasswordResetEmailAsync(user.Email, user.PasswordResetToken);

                _logger.LogInformation("Password reset requested for user: {UserId}", user.Id);
                return ApiResponse.SuccessResult("Link đặt lại mật khẩu đã được gửi đến email của bạn");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing forgot password request for email: {Email}", request.Email);
                return ApiResponse.ErrorResult("Có lỗi xảy ra khi xử lý yêu cầu đặt lại mật khẩu");
            }
        }

        public async Task<ApiResponse> ResetPasswordAsync(ResetPasswordRequestDto request)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(request.Email);
                if (user == null || user.IsDeleted || !user.IsActive)
                {
                    return ApiResponse.ErrorResult("Yêu cầu đặt lại mật khẩu không hợp lệ");
                }

                if (!user.CanResetPassword || user.PasswordResetToken != request.Token)
                {
                    return ApiResponse.ErrorResult("Token đặt lại mật khẩu không hợp lệ hoặc đã hết hạn");
                }

                // Reset password
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var result = await _userManager.ResetPasswordAsync(user, token, request.NewPassword);

                if (!result.Succeeded)
                {
                    var errors = result.Errors.Select(e => e.Description).ToList();
                    return ApiResponse.ErrorResult("Đặt lại mật khẩu không thành công", errors);
                }

                // Clear reset token
                user.PasswordResetToken = null;
                user.PasswordResetExpires = null;
                user.UnlockAccount(); // Unlock account if it was locked
                await _userManager.UpdateAsync(user);

                // Revoke all refresh tokens for security
                await RevokeAllUserTokensAsync(user.Id);

                // Log activity
                await LogUserActivity(user.Id, ActivityType.ResetPassword);

                _logger.LogInformation("Password reset successful for user: {UserId}", user.Id);
                return ApiResponse.SuccessResult("Mật khẩu đã được đặt lại thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting password for email: {Email}", request.Email);
                return ApiResponse.ErrorResult("Có lỗi xảy ra khi đặt lại mật khẩu");
            }
        }

        public async Task<ApiResponse> ChangePasswordAsync(Guid userId, ChangePasswordRequestDto request)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user == null || user.IsDeleted || !user.IsActive)
                {
                    return ApiResponse.ErrorResult("Người dùng không tồn tại");
                }

                var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
                if (!result.Succeeded)
                {
                    var errors = result.Errors.Select(e => e.Description).ToList();
                    return ApiResponse.ErrorResult("Đổi mật khẩu không thành công", errors);
                }

                // Log activity
                await LogUserActivity(userId, ActivityType.ChangePassword);

                _logger.LogInformation("Password changed successfully for user: {UserId}", userId);
                return ApiResponse.SuccessResult("Mật khẩu đã được thay đổi thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password for user: {UserId}", userId);
                return ApiResponse.ErrorResult("Có lỗi xảy ra khi đổi mật khẩu");
            }
        }

        public async Task<ApiResponse> VerifyEmailAsync(VerifyEmailRequestDto request)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(request.Email);
                if (user == null || user.IsDeleted || !user.IsActive)
                {
                    return ApiResponse.ErrorResult("Yêu cầu xác thực không hợp lệ");
                }

                if (user.EmailVerified)
                {
                    return ApiResponse.SuccessResult("Email đã được xác thực trước đó");
                }

                if (user.EmailVerificationToken != request.Token)
                {
                    return ApiResponse.ErrorResult("Token xác thực không hợp lệ");
                }

                // Verify email
                user.EmailVerified = true;
                user.EmailVerifiedAt = DateTime.UtcNow;
                user.EmailVerificationToken = null;
                user.Status = AccountStatus.Active;
                await _userManager.UpdateAsync(user);

                _logger.LogInformation("Email verified successfully for user: {UserId}", user.Id);
                return ApiResponse.SuccessResult("Email đã được xác thực thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying email for: {Email}", request.Email);
                return ApiResponse.ErrorResult("Có lỗi xảy ra khi xác thực email");
            }
        }

        public async Task<ApiResponse> ResendEmailVerificationAsync(string email)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null || user.IsDeleted || !user.IsActive)
                {
                    // Don't reveal that the user doesn't exist
                    return ApiResponse.SuccessResult("Nếu email tồn tại, email xác thực đã được gửi lại");
                }

                if (user.EmailVerified)
                {
                    return ApiResponse.ErrorResult("Email đã được xác thực");
                }

                // Generate new verification token
                user.GenerateEmailVerificationToken();
                await _userManager.UpdateAsync(user);

                // TODO: Send verification email
                // await _emailService.SendEmailVerificationAsync(user.Email, user.EmailVerificationToken);

                return ApiResponse.SuccessResult("Email xác thực đã được gửi lại");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resending email verification for: {Email}", email);
                return ApiResponse.ErrorResult("Có lỗi xảy ra khi gửi lại email xác thực");
            }
        }

        public async Task<ApiResponse<UserDto>> GetCurrentUserAsync(Guid userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user == null || user.IsDeleted || !user.IsActive)
                {
                    return ApiResponse.ErrorResult<UserDto>("Người dùng không tồn tại");
                }

                var userDto = _mapper.Map<UserDto>(user);
                return ApiResponse.SuccessResult(userDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current user: {UserId}", userId);
                return ApiResponse.ErrorResult<UserDto>("Có lỗi xảy ra khi lấy thông tin người dùng");
            }
        }

        public async Task<ApiResponse> RevokeTokenAsync(Guid userId, string token)
        {
            try
            {
                await RevokeRefreshTokenAsync(token, "Revoked by user");
                return ApiResponse.SuccessResult("Token đã được thu hồi");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking token for user: {UserId}", userId);
                return ApiResponse.ErrorResult("Có lỗi xảy ra khi thu hồi token");
            }
        }

        public async Task<ApiResponse> RevokeAllTokensAsync(Guid userId)
        {
            try
            {
                await RevokeAllUserTokensAsync(userId);
                return ApiResponse.SuccessResult("Tất cả token đã được thu hồi");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking all tokens for user: {UserId}", userId);
                return ApiResponse.ErrorResult("Có lỗi xảy ra khi thu hồi tất cả token");
            }
        }

        #region Private Helper Methods

        private async Task SaveRefreshTokenAsync(Guid userId, string token, string ipAddress)
        {
            var refreshToken = new RefreshToken
            {
                UserId = userId,
                Token = token,
                ExpiresAt = DateTime.UtcNow.AddDays(7), // 7 days
                CreatedByIp = ipAddress
            };

            _context.RefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync();
        }

        private async Task RevokeRefreshTokenAsync(string token, string? reason = null, string? revokedByIp = null)
        {
            var refreshToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Token == token);

            if (refreshToken != null && refreshToken.IsActive)
            {
                refreshToken.Revoke(reason, revokedByIp);
                await _context.SaveChangesAsync();
            }
        }

        private async Task RevokeAllUserTokensAsync(Guid userId)
        {
            var tokens = await _context.RefreshTokens
                .Where(rt => rt.UserId == userId && rt.IsActive)
                .ToListAsync();

            foreach (var token in tokens)
            {
                token.Revoke("All tokens revoked");
            }

            await _context.SaveChangesAsync();
        }

        private async Task LogUserActivity(Guid userId, ActivityType activityType, string? ipAddress = null, string? details = null)
        {
            try
            {
                var activity = new UserActivity
                {
                    UserId = userId,
                    ActivityType = activityType.ToString(),
                    Description = GetActivityDescription(activityType),
                    IpAddress = ipAddress ?? "Unknown",
                    UserAgent = "", // Can be populated from HttpContext
                    Details = details ?? "",
                    RelatedEntityType = "User", // Set default value for RelatedEntityType
                    RelatedEntityId = null, // This can be null since it's nullable
                    CreatedAt = DateTime.UtcNow
                };

                _context.UserActivities.Add(activity);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging user activity for user: {UserId}", userId);
                // Don't throw - logging shouldn't break the main operation
            }
        }

        private string GetActivityDescription(ActivityType activityType)
        {
            return activityType switch
            {
                ActivityType.Login => "Người dùng đăng nhập hệ thống",
                ActivityType.Logout => "Người dùng đăng xuất khỏi hệ thống", 
                ActivityType.Register => "Người dùng đăng ký tài khoản mới",
                ActivityType.ChangePassword => "Người dùng thay đổi mật khẩu",
                ActivityType.ResetPassword => "Người dùng đặt lại mật khẩu",
                _ => $"Hoạt động: {activityType}"
            };
        }

        private async Task<string> GetSystemSetting(string key, string defaultValue = "")
        {
            try
            {
                // Check if SystemSettings table exists and has data
                if (!_context.SystemSettings.Any())
                {
                    _logger.LogWarning("SystemSettings table is empty, using default value for key: {Key}", key);
                    return defaultValue;
                }

                var setting = await _context.SystemSettings
                    .FirstOrDefaultAsync(s => s.Key == key);
                return setting?.Value ?? defaultValue;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error accessing SystemSettings for key: {Key}, using default value", key);
                return defaultValue;
            }
        }

        #endregion
    }
}