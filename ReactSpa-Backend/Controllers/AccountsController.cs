namespace ReactSpa_Backend.Controllers;

using Microsoft.AspNetCore.Mvc;
using ReactSpa_Backend.Authorization;
using ReactSpa_Backend.Entities;
using ReactSpa_Backend.IServices;
using ReactSpa_Backend.Models.Accounts;

[Authorize(Role.User)]
[ApiController]
[Route("[controller]")]
public class AccountsController : BaseController
{
    private readonly IAccountService _accountService;

    public AccountsController(IAccountService accountService)
    {
        _accountService = accountService;
    }

    [AllowAnonymous]
    [HttpPost("authenticate")]
    public IActionResult Authenticate([FromBody] AuthenticateRequest model)
    {
        var response = _accountService.Authenticate(model, ipAddress());

        setTokenCookie(response.RefreshToken);

        return Ok(response);
    }

    [AllowAnonymous]
    [HttpPost("register")]
    public IActionResult Register([FromBody] RegisterRequest model)
    {
        _accountService.Register(model, Request.Headers["origin"]);

        return Ok(new { message = "Registration successful,please check your email for verification instructions" });
    }

    [AllowAnonymous]
    [HttpPost("verify-email")]
    public IActionResult VerifyEmail(VerifyEmailRequest model)
    {
        _accountService.VerifyEmail(model.Token);

        return Ok(new { message = "Email verified successfully,you can now login" });
    }

    [AllowAnonymous]
    [HttpPost("forgot-password")]
    public IActionResult ForgotPassword(ForgotPasswordRequest model)
    {
        _accountService.ForgotPassword(model, Request.Headers["origin"]);

        return Ok(new { message = "Password reset instructions have been sent to your email" });
    }

    [AllowAnonymous]
    [HttpPost("valite-reset-token")]
    public IActionResult ValidateResetToken(ValidateResetTokenRequest model)
    {
        _accountService.ValidateResetToken(model);
        return Ok(new { message = "Token is valid" });
    }

    [AllowAnonymous]
    [HttpPost("reset-password")]
    public IActionResult ResetPassword(ResetPasswordRequest model)
    {
        _accountService.ResetPassword(model);
        return Ok(new { message = "Password reset successfully,you can now login" });
    }

    [AllowAnonymous]
    [HttpPost("refresh-token")]
    public IActionResult RefreshToken()
    {
        var refreshToken = Request.Cookies["refreshToken"];
        var response = _accountService.RefreshToken(refreshToken, ipAddress());
        setTokenCookie(response.RefreshToken);
        return Ok(response);
    }

    [Authorize(Role.Admin)]
    [HttpPost("revoke-token")]
    public IActionResult RevokeToken(RevokeTokenRequest model)
    {
        var token = model.Token ?? Request.Cookies["refreshToken"];

        if (string.IsNullOrEmpty(token))
        {
            return BadRequest(new { message = "Token is required" });
        }

        _accountService.RevokeToken(token, ipAddress());
        return Ok(new { message = "Token revoked" });
    }

    [HttpGet("{id:int}")]
    public IActionResult GetById(int id)
    {
        if (id != this.Account.Id && Account.Role != Role.Admin)
            return Unauthorized(new { message = "Unauthorized" });

        var user = _accountService.GetById(id);
        return Ok(user);
    }

    [Authorize(Role.Admin)]
    [HttpGet]
    public IActionResult GetAll()
    {
        var users = _accountService.GetAll();

        return Ok(users);
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        var refreshToken = Request.Cookies["refreshToken"];
        if (!string.IsNullOrEmpty(refreshToken))
        {
            _accountService.RevokeToken(refreshToken, ipAddress());
        }
        else
        {
            return BadRequest(new { message = "Token is required" });
        }

        return Ok(new { message = "Logout successful" });
    }

    [HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] UpdateRequest model)
    {
        if (id != Account.Id && Account.Role != Role.Admin)
            return Unauthorized(new { message = "Unauthorized" });

        if (Account.Role != Role.Admin)
            model.Role = null;

        _accountService.Update(id, model);
        return Ok(new { message = "User updated successfully" });
    }

    [HttpDelete("{id}")]
    public IActionResult Delete(int id, UpdateRequest model)
    {
        if (id != Account.Id && Account.Role != Role.Admin)
            return Unauthorized(new { message = "Unauthorized" });

        _accountService.Delete(id);
        return Ok(new { message = "User deleted successfully" });
    }

    private void setTokenCookie(string token)
    {
        var cookieOptions = new CookieOptions
        {
            Expires = DateTime.Now.AddDays(7),
            HttpOnly = true
        };
        Response.Cookies.Append("refreshToken", token, cookieOptions);
    }

    private string ipAddress()
    {
        if (Request.Headers.ContainsKey("X-Forwarded-For"))
        {
            return Request.Headers["X-Forwarded-For"];
        }
        else
        {
            return HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
        }
    }
}
