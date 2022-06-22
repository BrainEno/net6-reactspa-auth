namespace ReactSpa_Backend.Models.Accounts;

using System.ComponentModel.DataAnnotations;
using ReactSpa_Backend.Entities;
public class RegisterRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }

    [Required]
    public string Username { get; set; }

    [Required]
    public string Password { get; set; }

    [Required]
    [MinLength(6)]
    [Compare("Password")]
    public string ConfirmPassword { get; set; }

    [Range(typeof(bool), "true", "true")]
    public bool AcceptTerms { get; set; }
}