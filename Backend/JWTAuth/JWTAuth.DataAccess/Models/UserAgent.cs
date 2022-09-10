using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JWTAuth.DataAccess.Models;

public class UserAgent
{
    [Required]
    public Guid Id { get; set; }

    [ForeignKey("Id")]
    public Token Token { get; set; }

    [Required]
    public string OS { get; set; }

    [Required]
    public string Browser { get; set; }
}