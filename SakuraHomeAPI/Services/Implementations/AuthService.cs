using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SakuraHomeAPI.Data;
using SakuraHomeAPI.Models.DTOs;
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
                    return ApiResponse.ErrorResult<AuthResponseDto>("Email ho?c m?t kh?u không ?úng");
                }

                // Check if user is deleted or inactive
                if (user.IsDeleted || !user.IsActive)
                {
                    _logger.LogWarning("Login attempt for inactive/deleted user: {UserId}", user.Id);
                    return ApiResponse.ErrorResult<AuthResponseDto>("Tài kho?n ?ã b? vô hi?u hóa");
                }

                // Check if user is locked out
                if (user.IsLocked)
                {
                    _logger.LogWarning("Login attempt for locked user: {UserId}", user.Id);
                    return ApiResponse.ErrorResult<AuthResponseDto>("Tài kho?n ?ã b? khóa. Vui lòng th? l?i sau");
                }

                // Check password
                var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, true);
                if (!result.Succeeded)
                {
                    _logger.LogWarning("Failed login attempt for user: {UserId}", user.Id);

                    if (result.IsLockedOut)
                    {
                        return ApiResponse.ErrorResult<AuthResponseDto>("Tài kho?n ?ã b? khóa do quá nhi?u l?n ??ng nh?p sai");
                    }

                    return ApiResponse.ErrorResult<AuthResponseDto>("Email ho?c m?t kh?u không ?úng");
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
                return ApiResponse.SuccessResult(authResponse, "??ng nh?p thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for email: {Email}", request.Email);
                return ApiResponse.ErrorResult<AuthResponseDto>("Có l?i x?y ra trong quá trình ??ng nh?p");
            }
        }

        public async Task<ApiResponse<AuthResponseDto>> RegisterAsync(RegisterRequestDto request, string ipAddress)
        {
            try
            {
                _logger.LogInformation("Attempting registration for email: {Email}", request.Email);

                // Check if registration is enabled
                var registrationEnabled = await GetSystemSetting("EnableRegistration", "true");
                if (registrationEnabled.ToLower() != "true")
                {
                    return ApiResponse.ErrorResult<AuthResponseDto>("??ng ký tài kho?n hi?n không ???c phép");
                }

                // Check if user already exists
                var existingUser = await _userManager.FindByEmailAsync(request.Email);
                if (existingUser != null)
                {
                    return ApiResponse.ErrorResult<AuthResponseDto>("Email này ?ã ???c s? d?ng");
                }

                // Create new user
                var user = new User
                {
                    Email = request.Email,
                    UserName = request.Email,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    PhoneNumber = request.PhoneNumber,
                    DateOfBirth = request.DateOfBirth,
                    Gender = request.Gender,
                    PreferredLanguage = request.PreferredLanguage,
                    EmailNotifications = request.EmailNotifications,
                    SmsNotifications = request.SmsNotifications,
                    Status = AccountStatus.Active, // Can be changed to Pending if email verification is required
                    Role = UserRole.Customer,
                    Provider = LoginProvider.Local,
                    CreatedAt = DateTime.UtcNow,
                    LastLoginIp = ipAddress
                };

                // Create user with password
                var result = await _userManager.CreateAsync(user, request.Password);
                if (!result.Succeeded)
                {
                    var errors = result.Errors.Select(e => e.Description).ToList();
                    _logger.LogWarning("Registration failed for email: {Email}. Errors: {Errors}", 
                        request.Email, string.Join(", ", errors));
                    return ApiResponse.ErrorResult<AuthResponseDto>("??ng ký không thành công", errors);
                }

                // Generate email verification token if needed
                user.GenerateEmailVerificationToken();
                await _userManager.UpdateAsync(user);

                // Create wishlist for new user
                var wishlist = new Wishlist
                {
                    UserId = user.Id,
                    Name = "Danh sách yêu thích c?a tôi"
                };
                _context.Wishlists.Add(wishlist);
                await _context.SaveChangesAsync();

                // Log user activity
                await LogUserActivity(user.Id, ActivityType.Register, ipAddress);

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
                return ApiResponse.SuccessResult(authResponse, "??ng ký thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration for email: {Email}", request.Email);
                return ApiResponse.ErrorResult<AuthResponseDto>("Có l?i x?y ra trong quá trình ??ng ký");
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

                return ApiResponse.SuccessResult("??ng xu?t thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout for user: {UserId}", userId);
                return ApiResponse.ErrorResult("Có l?i x?y ra trong quá trình ??ng xu?t");
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
                    return ApiResponse.ErrorResult<AuthResponseDto>("Token không h?p l?");
                }

                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user == null || user.IsDeleted || !user.IsActive)
                {
                    return ApiResponse.ErrorResult<AuthResponseDto>("Ng??i dùng không t?n t?i ho?c ?ã b? vô hi?u hóa");
                }

                // Validate refresh token
                var storedRefreshToken = await _context.RefreshTokens
                    .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken && rt.UserId == userId);

                if (storedRefreshToken == null || !storedRefreshToken.IsActive)
                {
                    return ApiResponse.ErrorResult<AuthResponseDto>("Refresh token không h?p l? ho?c ?ã h?t h?n");
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

                return ApiResponse.SuccessResult(authResponse, "Token ?ã ???c làm m?i");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing token");
                return ApiResponse.ErrorResult<AuthResponseDto>("Có l?i x?y ra khi làm m?i token");
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
                    return ApiResponse.SuccessResult("N?u email t?n t?i, link ??t l?i m?t kh?u ?ã ???c g?i");
                }

                // Generate password reset token
                user.GeneratePasswordResetToken(24); // 24 hours expiration
                await _userManager.UpdateAsync(user);

                // TODO: Send email with reset link
                // await _emailService.SendPasswordResetEmailAsync(user.Email, user.PasswordResetToken);

                _logger.LogInformation("Password reset requested for user: {UserId}", user.Id);
                return ApiResponse.SuccessResult("Link ??t l?i m?t kh?u ?ã ???c g?i ??n email c?a b?n");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing forgot password request for email: {Email}", request.Email);
                return ApiResponse.ErrorResult("Có l?i x?y ra khi x? lý yêu c?u ??t l?i m?t kh?u");
            }
        }

        public async Task<ApiResponse> ResetPasswordAsync(ResetPasswordRequestDto request)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(request.Email);
                if (user == null || user.IsDeleted || !user.IsActive)
                {
                    return ApiResponse.ErrorResult("Yêu c?u ??t l?i m?t kh?u không h?p l?");
                }

                if (!user.CanResetPassword || user.PasswordResetToken != request.Token)
                {
                    return ApiResponse.ErrorResult("Token ??t l?i m?t kh?u không h?p l? ho?c ?ã h?t h?n");
                }

                // Reset password
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var result = await _userManager.ResetPasswordAsync(user, token, request.NewPassword);

                if (!result.Succeeded)
                {
                    var errors = result.Errors.Select(e => e.Description).ToList();
                    return ApiResponse.ErrorResult("??t l?i m?t kh?u không thành công", errors);
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
                return ApiResponse.SuccessResult("M?t kh?u ?ã ???c ??t l?i thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting password for email: {Email}", request.Email);
                return ApiResponse.ErrorResult("Có l?i x?y ra khi ??t l?i m?t kh?u");
            }
        }

        public async Task<ApiResponse> ChangePasswordAsync(Guid userId, ChangePasswordRequestDto request)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user == null || user.IsDeleted || !user.IsActive)
                {
                    return ApiResponse.ErrorResult("Ng??i dùng không t?n t?i");
                }

                var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
                if (!result.Succeeded)
                {
                    var errors = result.Errors.Select(e => e.Description).ToList();
                    return ApiResponse.ErrorResult("??i m?t kh?u không thành công", errors);
                }

                // Log activity
                await LogUserActivity(userId, ActivityType.ChangePassword);

                _logger.LogInformation("Password changed successfully for user: {UserId}", userId);
                return ApiResponse.SuccessResult("M?t kh?u ?ã ???c thay ??i thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password for user: {UserId}", userId);
                return ApiResponse.ErrorResult("Có l?i x?y ra khi ??i m?t kh?u");
            }
        }

        public async Task<ApiResponse> VerifyEmailAsync(VerifyEmailRequestDto request)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(request.Email);
                if (user == null || user.IsDeleted || !user.IsActive)
                {
                    return ApiResponse.ErrorResult("Yêu c?u xác th?c không h?p l?");
                }

                if (user.EmailVerified)
                {
                    return ApiResponse.SuccessResult("Email ?ã ???c xác th?c tr??c ?ó");
                }

                if (user.EmailVerificationToken != request.Token)
                {
                    return ApiResponse.ErrorResult("Token xác th?c không h?p l?");
                }

                // Verify email
                user.EmailVerified = true;
                user.EmailVerifiedAt = DateTime.UtcNow;
                user.EmailVerificationToken = null;
                user.Status = AccountStatus.Active;
                await _userManager.UpdateAsync(user);

                _logger.LogInformation("Email verified successfully for user: {UserId}", user.Id);
                return ApiResponse.SuccessResult("Email ?ã ???c xác th?c thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying email for: {Email}", request.Email);
                return ApiResponse.ErrorResult("Có l?i x?y ra khi xác th?c email");
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
                    return ApiResponse.SuccessResult("N?u email t?n t?i, email xác th?c ?ã ???c g?i l?i");
                }

                if (user.EmailVerified)
                {
                    return ApiResponse.ErrorResult("Email ?ã ???c xác th?c");
                }

                // Generate new verification token
                user.GenerateEmailVerificationToken();
                await _userManager.UpdateAsync(user);

                // TODO: Send verification email
                // await _emailService.SendEmailVerificationAsync(user.Email, user.EmailVerificationToken);

                return ApiResponse.SuccessResult("Email xác th?c ?ã ???c g?i l?i");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resending email verification for: {Email}", email);
                return ApiResponse.ErrorResult("Có l?i x?y ra khi g?i l?i email xác th?c");
            }
        }

        public async Task<ApiResponse<UserDto>> GetCurrentUserAsync(Guid userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user == null || user.IsDeleted || !user.IsActive)
                {
                    return ApiResponse.ErrorResult<UserDto>("Ng??i dùng không t?n t?i");
                }

                var userDto = _mapper.Map<UserDto>(user);
                return ApiResponse.SuccessResult(userDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current user: {UserId}", userId);
                return ApiResponse.ErrorResult<UserDto>("Có l?i x?y ra khi l?y thông tin ng??i dùng");
            }
        }

        public async Task<ApiResponse> RevokeTokenAsync(Guid userId, string token)
        {
            try
            {
                await RevokeRefreshTokenAsync(token, "Revoked by user");
                return ApiResponse.SuccessResult("Token ?ã ???c thu h?i");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking token for user: {UserId}", userId);
                return ApiResponse.ErrorResult("Có l?i x?y ra khi thu h?i token");
            }
        }

        public async Task<ApiResponse> RevokeAllTokensAsync(Guid userId)
        {
            try
            {
                await RevokeAllUserTokensAsync(userId);
                return ApiResponse.SuccessResult("T?t c? token ?ã ???c thu h?i");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking all tokens for user: {UserId}", userId);
                return ApiResponse.ErrorResult("Có l?i x?y ra khi thu h?i t?t c? token");
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
                    IpAddress = ipAddress,
                    UserAgent = "", // Can be populated from HttpContext
                    Details = details,
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

        private async Task<string> GetSystemSetting(string key, string defaultValue = "")
        {
            try
            {
                var setting = await _context.SystemSettings
                    .FirstOrDefaultAsync(s => s.Key == key);
                return setting?.Value ?? defaultValue;
            }
            catch
            {
                return defaultValue;
            }
        }

        #endregion
    }
}