namespace ReactSpa_Backend.Models.Accounts;

using System.ComponentModel.DataAnnotations;
using ReactSpa_Backend.Entities;

public class CreateRequest
{
    [Required]
    public string Username { get; set; }

    [Required]
    [EnumDataType(typeof(Role))]
    public string Role { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; }

    [Required]
    [MinLength(6)]
    public string Password { get; set; }

    [Required]
    [Compare("Password")]
    public string ConfirmPassword { get; set; }
}