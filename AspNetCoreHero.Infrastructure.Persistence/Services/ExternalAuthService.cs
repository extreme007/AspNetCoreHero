using AspNetCoreHero.Application.Configurations;
using AspNetCoreHero.Application.DTOs.Account;
using AspNetCoreHero.Application.DTOs.ExternalAuth;
using AspNetCoreHero.Application.Enums.Identity;
using AspNetCoreHero.Application.Exceptions;
using AspNetCoreHero.Application.Interfaces;
using AspNetCoreHero.Application.Wrappers;
using AspNetCoreHero.Infrastructure.Persistence.Helpers;
using AspNetCoreHero.Infrastructure.Persistence.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace AspNetCoreHero.Infrastructure.Persistence.Services
{
    public class ExternalAuthService : IExternalAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly JWTConfiguration _jwtSettings;

        public ExternalAuthService(UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        SignInManager<ApplicationUser> signInManager,
        IOptions<JWTConfiguration> jwtSettings)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtSettings = jwtSettings.Value;
        }

        public async Task<Response<AuthenticationResponse>> ExternalAuthenticateAsync(string providerToken, string ipAddress)
        {
            var ProviderUserDetails = GetProviderUserDetails(providerToken);
            if (ProviderUserDetails == null)
                throw new ApiException($"Invalid Credentials");
            var user = await _userManager.FindByEmailAsync(ProviderUserDetails.Email);
            if (user == null)
            {
                user = new ApplicationUser
                {
                    Email = ProviderUserDetails.Email,
                    FirstName = ProviderUserDetails.FirstName,
                    LastName = ProviderUserDetails.LastName,
                    UserName = ProviderUserDetails.ProviderUserId,
                    EmailConfirmed = true
                };
                var result = await _userManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, Roles.Basic.ToString());
                }
                else
                {
                    throw new ApiException($"Invalid Credentials");
                }
            }

            JwtSecurityToken jwtSecurityToken = await TokenHelper.GenerateJWToken(user,_userManager,_jwtSettings);
            AuthenticationResponse response = new AuthenticationResponse();
            response.Id = user.Id;
            response.JWToken = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
            response.Email = user.Email;
            response.UserName = user.UserName;
            var rolesList = await _userManager.GetRolesAsync(user).ConfigureAwait(false);
            response.Roles = rolesList.ToList();
            response.IsVerified = user.EmailConfirmed;
            var refreshToken = TokenHelper.GenerateRefreshToken(ipAddress);
            response.RefreshToken = refreshToken.Token;
            return new Response<AuthenticationResponse>(response, $"Authenticated {user.UserName}");
        }

        public ProviderUserDetails GetProviderUserDetails(string providerToken)
        {
            var httpClient = new HttpClient();

            var requestUri = new Uri(string.Format("https://www.googleapis.com/oauth2/v3/tokeninfo?id_token={0}", providerToken));

            HttpResponseMessage httpResponseMessage;
            try
            {
                httpResponseMessage = httpClient.GetAsync(requestUri).Result;
            }
            catch (Exception ex)
            {
                return null;
            }

            if (httpResponseMessage.StatusCode != HttpStatusCode.OK)
            {
                return null;
            }

            var response = httpResponseMessage.Content.ReadAsStringAsync().Result;
            var googleApiTokenInfo = JsonConvert.DeserializeObject<GoogleApiTokenInfo>(response);

            var ProviderUserDetails = new ProviderUserDetails
            {
                Email = googleApiTokenInfo.email,
                FirstName = googleApiTokenInfo.given_name,
                LastName = googleApiTokenInfo.family_name,
                Locale = googleApiTokenInfo.locale,
                Name = googleApiTokenInfo.name,
                ProviderUserId = googleApiTokenInfo.sub
            };
            return ProviderUserDetails;
        }
    }
}
