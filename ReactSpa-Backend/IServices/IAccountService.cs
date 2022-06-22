namespace ReactSpa_Backend.IServices;

using System.Collections.Generic;
using ReactSpa_Backend.Entities;
using ReactSpa_Backend.Models.Accounts;

public interface IAccountService
{
    AuthenticateResponse Authenticate(AuthenticateRequest model, string ipAddress);
    AuthenticateResponse RefreshToken(string token, string ipAddress);
    void RevokeToken(string token, string ipAddress);
    IEnumerable<AccountResponse> GetAll();
    AccountResponse GetById(int id);
    AccountResponse Create(CreateRequest model);
    AccountResponse Update(int id, UpdateRequest model);
    void Register(RegisterRequest model, string origin);
    void VerifyEmail(string token);
    void ForgotPassword(ForgotPasswordRequest model, string origin);
    void ResetPassword(ResetPasswordRequest model);
    void Delete(int id);
    void ValidateResetToken(ValidateResetTokenRequest model);
}