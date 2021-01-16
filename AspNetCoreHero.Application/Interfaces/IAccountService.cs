using AspNetCoreHero.Application.DTOs.Account;
using AspNetCoreHero.Application.Wrappers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AspNetCoreHero.Application.Interfaces
{
    public interface IAccountService
    {
        Task<Response<AuthenticationResponse>> AuthenticateAsync(AuthenticationRequest request, string ipAddress ="");
        Task<Response<string>> RegisterAsync(RegisterRequest request, string origin="");
        Task<Response<string>> ConfirmEmailAsync(string userId, string code);
        Task ForgotPassword(ForgotPasswordRequest model, string origin="");
        Task<Response<string>> ResetPassword(ResetPasswordRequest model);

        Task<Response<AuthenticationResponse>> RefreshTokenAsync(string accessToken,string ipAddress="");

        Task<bool> RevokeToken(string accessToken, string ipAddress="");

        Task<Response<List<RefreshToken>>> GetRefreshTokenList(string id);
    }
}
