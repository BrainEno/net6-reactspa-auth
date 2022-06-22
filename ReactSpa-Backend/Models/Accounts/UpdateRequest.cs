namespace ReactSpa_Backend.Models.Accounts;

using System.ComponentModel.DataAnnotations;
using ReactSpa_Backend.Entities;
public class UpdateRequest
{
    private string _password;
    private string _confirmPassword;
    private string _role;
    private string _email;
    public string Username { get; set; }

    [EnumDataType(typeof(Role))]
    public string Role
    {
        get => _role;
        set => _role = replaceEmptyWithNull(value);
    }

    public string Email
    {
        get => _email;
        set => _email = replaceEmptyWithNull(_email);
    }

    [MinLength(6)]
    public string Password
    {
        get => _password;
        set => _password = replaceEmptyWithNull(value);
    }

    [Compare("Password")]
    public string ConfirmPassword
    {
        get => _confirmPassword;
        set => _confirmPassword = replaceEmptyWithNull(value);
    }

    private string replaceEmptyWithNull(string value) => string.IsNullOrEmpty(value) ? null : value;
}