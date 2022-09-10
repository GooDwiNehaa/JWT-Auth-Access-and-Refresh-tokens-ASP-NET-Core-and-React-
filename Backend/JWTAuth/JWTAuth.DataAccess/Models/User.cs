using System.ComponentModel.DataAnnotations;

namespace JWTAuth.DataAccess.Models;

public class User
{
    [Key]
    [Required]
    public Guid Id { get; set; }

    [EmailAddress]
    [Required]
    public string Email { get; set; }

    [Required]
    public string Password { get; set; }

    public List<Token> Tokens { get; set; }
}