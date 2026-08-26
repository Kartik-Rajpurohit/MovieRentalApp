using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MovieRental.Domain.Entities
{
    // Represents an application-level user for authentication and authorization
    [Table("user")]
    public class User
    {
        [Key]
        [Column("user_id")]
        public int UserId { get; set; }

        [Column("first_name")]
        public string FirstName { get; set; } = string.Empty;

        [Column("last_name")]
        public string LastName { get; set; } = string.Empty;

        [Column("email")]
        public string Email { get; set; } = string.Empty;

        // Stored as a hashed value — never plain text
        [Column("password_hash")]
        public string PasswordHash { get; set; } = string.Empty;

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }

        // FK → Role (defines what the user can access)
        [Column("role_id")]
        [ForeignKey("Role")]
        public int? RoleId { get; set; }
        public Role? Role { get; set; } = null!;

        // FK → Address (user's physical address — required)
        [Column("address_id")]
        [ForeignKey("Address")]
        public int? AddressId { get; set; }
        public Address? Address { get; set; } = null!;
        // Refresh token — used to generate new access token without re-login
        [Column("refresh_token")]
        public string? RefreshToken { get; set; }

        [Column("refresh_token_expiry")]
        public DateTime? RefreshTokenExpiry { get; set; }

        // Reverse navigation — if this user is a staff member (optional)
        public Staff? Staff { get; set; }

        // Reverse navigation — if this user is a rental customer (optional)
        public Customer? Customer { get; set; }
    }
}
