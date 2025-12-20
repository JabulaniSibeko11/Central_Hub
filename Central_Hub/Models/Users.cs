using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Central_Hub.Models
{
    public class Users 
    {
        public int Id { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        public string? FullName { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;



    }
}
