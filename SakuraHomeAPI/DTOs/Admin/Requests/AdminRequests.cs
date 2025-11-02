using SakuraHomeAPI.Models.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace SakuraHomeAPI.DTOs.Admin.Requests
{
    // =============================================
    // USER MANAGEMENT DTOs
    // =============================================

    /// <summary>
    /// Request DTO để tạo user mới (Customer hoặc Staff/Admin)
    /// </summary>
    public class CreateUserRequest
    {
        // ========== BASIC INFORMATION (Required) ==========

        [Required(ErrorMessage = "Họ là bắt buộc")]
        [MaxLength(100, ErrorMessage = "Họ không được quá 100 ký tự")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tên là bắt buộc")]
        [MaxLength(100, ErrorMessage = "Tên không được quá 100 ký tự")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [MaxLength(255)]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
        [MinLength(3, ErrorMessage = "Mật khẩu phải có ít nhất 3 ký tự")]
        public string Password { get; set; } = string.Empty;

        // ========== OPTIONAL BASIC FIELDS ==========

        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string? PhoneNumber { get; set; }

        public DateTime? DateOfBirth { get; set; }

        public Gender? Gender { get; set; }

        [MaxLength(500)]
        public string? Avatar { get; set; }

        // ========== ROLE (Required) ==========

        [Required(ErrorMessage = "Role là bắt buộc")]
        public UserRole Role { get; set; }

        // ========== EMPLOYEE FIELDS (Required if Role = Staff or Admin) ==========

        [MaxLength(20, ErrorMessage = "CCCD không được quá 20 ký tự")]
        public string? NationalIdCard { get; set; }

        public DateTime? HireDate { get; set; }

        [Range(0, 100000000, ErrorMessage = "Lương phải từ 0 đến 100,000,000")]
        public decimal? BaseSalary { get; set; }

        // ========== CUSTOMER FIELDS ==========

        public UserTier? Tier { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Điểm phải >= 0")]
        public int? InitialPoints { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Chi tiêu phải >= 0")]
        public decimal? InitialSpent { get; set; }

        // ========== PREFERENCES (Optional) ==========

        [MaxLength(5)]
        public string? PreferredLanguage { get; set; } = "vi";

        [MaxLength(3)]
        public string? PreferredCurrency { get; set; } = "VND";

        public bool EmailNotifications { get; set; } = true;
        public bool SmsNotifications { get; set; } = false;
        public bool PushNotifications { get; set; } = true;

        // ========== STATUS (Optional) ==========

        public bool IsActive { get; set; } = true;
        public AccountStatus Status { get; set; } = AccountStatus.Active;
    }

    /// <summary>
    /// Request DTO để cập nhật user
    /// </summary>
    public class UpdateUserRequest
    {
        // Basic Information
        [MaxLength(100)]
        public string? FirstName { get; set; }

        [MaxLength(100)]
        public string? LastName { get; set; }

        [Phone]
        public string? PhoneNumber { get; set; }

        public DateTime? DateOfBirth { get; set; }

        public Gender? Gender { get; set; }

        [MaxLength(500)]
        public string? Avatar { get; set; }

        // Employee Fields
        [MaxLength(20)]
        public string? NationalIdCard { get; set; }

        public DateTime? HireDate { get; set; }

        [Range(0, 100000000)]
        public decimal? BaseSalary { get; set; }

        // Role & Status
        public UserRole? Role { get; set; }
        public AccountStatus? Status { get; set; }
        public bool? IsActive { get; set; }

        // Customer Fields
        public UserTier? Tier { get; set; }
    }

    /// <summary>
    /// Request DTO để filter/search users
    /// </summary>
    public class UserFilterRequest
    {
        public string? SearchTerm { get; set; }
        public UserRole? Role { get; set; }
        public AccountStatus? Status { get; set; }
        public bool? IsActive { get; set; }

        [Range(1, int.MaxValue)]
        public int PageNumber { get; set; } = 1;

        [Range(1, 100)]
        public int PageSize { get; set; } = 20;
    }

    public class AdminUserFilterRequestDto
    {
        public string? Keyword { get; set; }
        public string? Role { get; set; }
        public bool? IsActive { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class AdminCreateUserRequestDto
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;
        [Required, MinLength(3), MaxLength(50)]
        public string UserName { get; set; } = string.Empty;
        [Required, MinLength(8)]
        public string Password { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? Role { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class AdminUpdateUserRequestDto
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;
        [Required, MinLength(3), MaxLength(50)]
        public string UserName { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? Role { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class AdminChangeUserStatusRequestDto
    {
        public bool IsActive { get; set; }
    }
}