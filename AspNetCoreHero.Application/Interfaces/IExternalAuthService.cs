using AspNetCoreHero.Application.DTOs.Account;
using AspNetCoreHero.Application.DTOs.ExternalAuth;
using AspNetCoreHero.Application.Wrappers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AspNetCoreHero.Application.Interfaces
{
    public interface IExternalAuthService
    {
        Task<Response<AuthenticationResponse>> ExternalAuthenticateAsync(ExternalAuthRequest externalAuthRequest, string ipAddress);
    }
}
