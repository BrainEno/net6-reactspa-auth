namespace ReactSpa_Backend.Services;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AutoMapper;
using BCrypt.Net;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using ReactSpa_Backend.Authorization;
using ReactSpa_Backend.Entities;
using ReactSpa_Backend.Helpers;
using ReactSpa_Backend.IServices;
using ReactSpa_Backend.Models.Accounts;

public class AccountService : IAccountService
{

    private DataContext _context;
    private IJwtUtils _jwtUtils;
    private readonly IMapper _mapper;
    private readonly AppSettings _appSettings;
    private readonly IEmailService _emailService;

    public AccountService(
        DataContext context,
        IJwtUtils jwtUtils,
        IMapper mapper,
        IOptions<AppSettings> appSettings,
        IEmailService emailService
        )
    {
        _context = context;
        _jwtUtils = jwtUtils;
        _mapper = mapper;
        _appSettings = appSettings.Value;
        _emailService = emailService;
    }


    public AuthenticateResponse Authenticate(AuthenticateRequest model, string ipAddress)
    {
        var user = _context.Users.SingleOrDefault(x => x.Email == model.Email);

        //Skip Email verify for now
        // if (user == null || !user.IsVerified || !BCrypt.Verify(model.Password, user.PasswordHash))
        // {
        //     throw new AppException("Email or password is incorrect");
        // }

        if (user == null || !BCrypt.Verify(model.Password, user.PasswordHash))
        {
            throw new AppException("Email or password is incorrect");
        }

        var jwtToken = _jwtUtils.GenerateJwtToken(user);
        var refreshToken = _jwtUtils.GenerateRefreshToken(ipAddress);
        user.RefreshTokens.Add(refreshToken);

        removeOldRefreshTokens(user);

        _context.Update(user);
        _context.SaveChanges();

        var response = _mapper.Map<AuthenticateResponse>(user);
        response.JwtToken = jwtToken;
        response.RefreshToken = refreshToken.Token;
        return response;
    }

    public AuthenticateResponse RefreshToken(string token, string ipAddress)
    {
        var user = getUserByRefreshToken(token);
        var refreshToken = user.RefreshTokens.Single(x => x.Token == token);

        if (refreshToken.IsRevoked)
        {
            revokeDescendantRefreshTokens(refreshToken, user, ipAddress, $"Attempted reuse of revoked refresh token: {token}");
            _context.Update(user);
            _context.SaveChanges();
        }

        if (!refreshToken.IsActive)
            throw new AppException("Invalid token");

        var newRefreshToken = rotateRefreshToken(refreshToken, ipAddress);
        user.RefreshTokens.Add(newRefreshToken);

        removeOldRefreshTokens(user);

        _context.Update(user);
        _context.SaveChanges();

        var jwtToken = _jwtUtils.GenerateJwtToken(user);

        var response = _mapper.Map<AuthenticateResponse>(user);
        response.JwtToken = jwtToken;
        response.RefreshToken = newRefreshToken.Token;
        return response;
    }

    public void RevokeToken(string token, string ipAddress)
    {
        var user = getUserByRefreshToken(token);
        var refreshToken = user.RefreshTokens.Single(x => x.Token == token);

        if (!refreshToken.IsActive)
        {
            throw new AppException("Invalid token");
        }

        revokeRefreshToken(refreshToken, ipAddress, "Revoked without replacement");
        _context.Update(user);
        _context.SaveChanges();
    }

    public void VerifyEmail(string token)
    {
        var account = _context.Users.SingleOrDefault(x => x.VerificationToken == token);

        if (account == null)
        {
            throw new AppException("Invalid token");
        }

        account.Verified = DateTime.UtcNow;
        account.VerificationToken = null;

        _context.Users.Update(account);
        _context.SaveChanges();
    }

    public void ForgotPassword(ForgotPasswordRequest model, string origin)
    {
        var account = _context.Users.SingleOrDefault(x => x.Email == model.Email);

        if (account == null)
            return;

        account.ResetToken = generateResetToken();
        account.ResetTokenExpires = DateTime.UtcNow.AddDays(1);

        _context.Users.Update(account);
        _context.SaveChanges();
        sendPasswordResetEmail(account, origin);
    }

    public void ValidateResetToken(ValidateResetTokenRequest model)
    {
        getAccountByResetToken(model.Token);
    }

    public void ResetPassword(ResetPasswordRequest model)
    {
        var account = getAccountByResetToken(model.Token);

        account.PasswordHash = BCrypt.HashPassword(model.Password);
        account.PasswordReset = DateTime.UtcNow;
        account.ResetToken = null;
        account.ResetTokenExpires = null;

        _context.Users.Update(account);
        _context.SaveChanges();
    }

    public void Register(RegisterRequest model, string origin)
    {
        if (_context.Users.Any(x => x.Email == model.Email))
        {
            sendAlreadyRegisteredEmail(model.Email, origin);
            return;
        }

        var user = _mapper.Map<User>(model);
        //first registered account is an admin
        var isFirstAccount = _context.Users.Count() == 0;
        user.Role = isFirstAccount ? Role.Admin : Role.User;
        user.Created = DateTime.UtcNow;
        user.VerificationToken = generateVerificationToken();

        user.PasswordHash = BCrypt.HashPassword(model.Password);

        _context.Users.Add(user);
        _context.SaveChanges();

        // sendVerificationEmail(user, origin);
    }

    public AccountResponse Create(CreateRequest model)
    {
        if (_context.Users.Any(x => x.Email == model.Email))
            throw new AppException($"Email {model.Email} is already taken");

        var account = _mapper.Map<User>(model);
        account.Created = DateTime.UtcNow;
        account.Verified = DateTime.UtcNow;

        account.PasswordHash = BCrypt.HashPassword(model.Password);
        _context.Users.Add(account);
        _context.SaveChanges();

        return _mapper.Map<AccountResponse>(account);
    }
    public AccountResponse Update(int id, UpdateRequest model)
    {
        var user = getUser(id);

        if (model.Email != user.Username && _context.Users.Any(x => x.Username == model.Username))
            throw new AppException($"Username {model.Username} is already taken");

        if (model.Email != user.Email && _context.Users.Any(x => x.Email == model.Email))
            throw new AppException($"Email {model.Email} is already taken");

        if (!string.IsNullOrEmpty(model.Password))
            user.PasswordHash = BCrypt.HashPassword(model.Password);

        _mapper.Map(model, user);
        user.Updated = DateTime.UtcNow;

        _context.Users.Update(user);
        _context.SaveChanges();

        return _mapper.Map<AccountResponse>(user);
    }

    public void Delete(int id)
    {
        var user = getUser(id);
        _context.Users.Remove(user);
        _context.SaveChanges();
    }

    public IEnumerable<AccountResponse> GetAll()
    {
        var accounts = _context.Users;
        return _mapper.Map<IEnumerable<AccountResponse>>(accounts);
    }

    public AccountResponse GetById(int id)
    {

        var user = getUser(id);
        if (user == null) throw new KeyNotFoundException("User not found");
        return _mapper.Map<AccountResponse>(user);
    }

    public User GetUserByUsername(string username)
    {
        var user = _context.Users.SingleOrDefault(x => x.Username == username);
        if (user == null) throw new KeyNotFoundException("User not found");
        return user;
    }

    private User getUserByRefreshToken(string token)
    {
        var user = _context.Users.SingleOrDefault(x => x.RefreshTokens.Any(t => t.Token == token));

        if (user == null)
        {
            throw new AppException("Invalid refresh token");
        }

        return user;
    }

    private User getAccountByResetToken(string token)
    {
        var user = _context.Users.SingleOrDefault(x => x.ResetToken == token && x.ResetTokenExpires > DateTime.UtcNow);

        if (user == null)
        {
            throw new AppException("Invalid reset token");
        }

        return user;
    }

    private RefreshToken rotateRefreshToken(RefreshToken refreshToken, string ipAddress)
    {
        var newRefreshToken = _jwtUtils.GenerateRefreshToken(ipAddress);
        revokeRefreshToken(refreshToken, ipAddress, "Replaced by new token");
        return newRefreshToken;
    }

    private void removeOldRefreshTokens(User user)
    {
        user.RefreshTokens.RemoveAll(x =>
        !x.IsActive &&
         x.Created.AddDays(_appSettings.RefreshTokenTTL) <= DateTime.UtcNow);
    }

    private void revokeDescendantRefreshTokens(RefreshToken refreshToken, User user, string ipAddress, string reason)
    {
        var childToken = user.RefreshTokens.SingleOrDefault(x => x.Token == refreshToken.ReplacedByToken);
        if (childToken.IsActive)
            revokeRefreshToken(childToken, ipAddress, reason);
        else
            revokeDescendantRefreshTokens(childToken, user, ipAddress, reason);
    }

    private void revokeRefreshToken(RefreshToken token, string ipAddress, string reason = null, string replacedToken = null)
    {
        token.Revoked = DateTime.UtcNow;
        token.RevokedByIp = ipAddress;
        token.ReasonRevoked = reason;
        token.ReplacedByToken = replacedToken;
    }

    private User getUser(int id)
    {
        var user = _context.Users.Find(id);
        if (user == null) throw new KeyNotFoundException("User not found");
        return user;
    }
    private string generateJwtToken(User account)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[] { new Claim("id", account.Id.ToString()) }),
            Expires = DateTime.UtcNow.AddMinutes(15),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    private string generateResetToken()
    {
        // token is a cryptographically strong random sequence of values
        var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(64));

        // ensure token is unique by checking against db
        var tokenIsUnique = !_context.Users.Any(x => x.ResetToken == token);
        if (!tokenIsUnique)
            return generateResetToken();

        return token;
    }


    private string generateVerificationToken()
    {
        var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(64));

        var tokenIsUnique = !_context.Users.Any(x => x.VerificationToken == token);
        if (!tokenIsUnique)
            return generateVerificationToken();
        return token;
    }

    private void sendVerificationEmail(User account, string origin)
    {
        string message;
        if (!string.IsNullOrEmpty(origin))
        {
            // origin exists if request sent from browser single page app (e.g. Angular or React)
            // so send link to verify via single page app
            var verifyUrl = $"{origin}/account/verify-email?token={account.VerificationToken}";
            message = $@"<p>Please click the below link to verify your email address:</p>
                            <p><a href=""{verifyUrl}"">{verifyUrl}</a></p>";
        }
        else
        {
            // origin missing if request sent directly to api (e.g. from Postman)
            // so send instructions to verify directly with api
            message = $@"<p>Please use the below token to verify your email address with the <code>/accounts/verify-email</code> api route:</p>
                            <p><code>{account.VerificationToken}</code></p>";
        }

        _emailService.Send(
            to: account.Email,
            subject: "Sign-up Verification API - Verify Email",
            html: $@"<h4>Verify Email</h4>
                        <p>Thanks for registering!</p>
                        {message}"
        );
    }


    private void sendAlreadyRegisteredEmail(string email, string origin)
    {
        string message;
        if (!string.IsNullOrEmpty(origin))
            message = $@"<p>If you don't know your password please visit the <a href=""{origin}/account/forgot-password"">forgot password</a> page.</p>";
        else
            message = "<p>If you don't know your password you can reset it via the <code>/accounts/forgot-password</code> api route.</p>";

        _emailService.Send(
            to: email,
            subject: "Sign-up Verification API - Email Already Registered",
            html: $@"<h4>Email Already Registered</h4>
                        <p>Your email <strong>{email}</strong> is already registered.</p>
                        {message}"
        );
    }

    private void sendPasswordResetEmail(User account, string origin)
    {
        string message;
        if (!string.IsNullOrEmpty(origin))
        {
            var resetUrl = $"{origin}/account/reset-password?token={account.ResetToken}";
            message = $@"<p>Please click the below link to reset your password, the link will be valid for 1 day:</p>
                            <p><a href=""{resetUrl}"">{resetUrl}</a></p>";
        }
        else
        {
            message = $@"<p>Please use the below token to reset your password with the <code>/accounts/reset-password</code> api route:</p>
                            <p><code>{account.ResetToken}</code></p>";
        }

        _emailService.Send(
            to: account.Email,
            subject: "Sign-up Verification API - Reset Password",
            html: $@"<h4>Reset Password Email</h4>
                        {message}"
        );
    }
}