using System;
using System.ComponentModel.DataAnnotations;

namespace SakuraHomeAPI.DTOs.Admin.Requests
{
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