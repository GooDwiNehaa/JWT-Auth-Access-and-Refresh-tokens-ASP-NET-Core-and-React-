using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JWTAuth.DataAccess.Models;

public class Token
{
    [Required]
    public Guid Id { get; set; }

    [Required]
    public Guid UserID { get; set; }

    [ForeignKey("UserID")]
    public User User { get; set; }

    public UserAgent UserAgent { get; set; }

    [Required]
    public string RefreshToken { get; set; }

    [Required]
    public DateTime Created { get; set; } = DateTime.UtcNow;

    [Required]
    public DateTime Expired { get; set; }

    [Required]
    public int LifeTime { get; set; } = 2;

    public Token()
    {
        this.Expired = this.Created.AddMinutes(this.LifeTime);
    }
}