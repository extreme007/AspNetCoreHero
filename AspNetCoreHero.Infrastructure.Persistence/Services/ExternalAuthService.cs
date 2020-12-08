using AspNetCoreHero.Application.Configurations;
using AspNetCoreHero.Application.DTOs.Account;
using AspNetCoreHero.Application.DTOs.ExternalAuth;
using AspNetCoreHero.Application.Enums.Identity;
using AspNetCoreHero.Application.Exceptions;
using AspNetCoreHero.Application.Interfaces;
using AspNetCoreHero.Application.Wrappers;
using AspNetCoreHero.Infrastructure.Persistence.Helpers;
using AspNetCoreHero.Infrastructure.Persistence.Identity;
using Microsoft.AspNetCore.Authorization;
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
        private readonly JWTConfiguration _jwtSettings;

        public ExternalAuthService(UserManager<ApplicationUser> userManager,
        IOptions<JWTConfiguration> jwtSettings)
        {
            _userManager = userManager;
            _jwtSettings = jwtSettings.Value;
        }
        public async Task<Response<AuthenticationResponse>> ExternalAuthenticateAsync(ExternalAuthRequest externalAuthRequest, string ipAddress)
        {
            ApplicationUser user = null;
            if(externalAuthRequest.Type == "google")
            {
                var GoogleApiTokenInfo = GetGoogleApiTokenInfo(externalAuthRequest);
                if (GoogleApiTokenInfo == null)
                    throw new ApiException($"Invalid Credentials");
                user = await _userManager.FindByEmailAsync(GoogleApiTokenInfo.email);
                if (user == null)
                {

                    user = new ApplicationUser
                    {
                        Email = GoogleApiTokenInfo.email,
                        FirstName = GoogleApiTokenInfo.given_name,
                        LastName = GoogleApiTokenInfo.family_name,
                        UserName = GoogleApiTokenInfo.sub,
                        EmailConfirmed = true,
                        ProfilePicture = null,
                        IsActive = true
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
            }
            else if(externalAuthRequest.Type == "facebook")
            {
                var FacebookApiTokenInfo = GetFacebookApiTokenInfo(externalAuthRequest);
                if (FacebookApiTokenInfo == null)
                    throw new ApiException($"Invalid Credentials");
                user = await _userManager.FindByEmailAsync(FacebookApiTokenInfo.email);
                if (user == null)
                {

                    user = new ApplicationUser
                    {
                        Email = FacebookApiTokenInfo.email,
                        FirstName = FacebookApiTokenInfo.first_name,
                        LastName = FacebookApiTokenInfo.last_name,
                        UserName = FacebookApiTokenInfo.id,
                        EmailConfirmed = true,
                        ProfilePicture = null,
                        IsActive = true
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
            }
            

            JwtSecurityToken jwtSecurityToken = await TokenHelper.GenerateJWToken(user, _userManager, _jwtSettings);
            AuthenticationResponse response = new AuthenticationResponse();
            response.Id = user.Id;
            response.AccessToken = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
            response.Email = user.Email;
            response.UserName = user.UserName;
            var rolesList = await _userManager.GetRolesAsync(user).ConfigureAwait(false);
            response.Roles = rolesList.ToList();
            response.IsVerified = user.EmailConfirmed;
            var refreshToken = TokenHelper.GenerateRefreshToken(ipAddress);
            response.RefreshToken = refreshToken.Token;
            return new Response<AuthenticationResponse>(response, $"Authenticated {user.UserName}");
        }

        private GoogleApiTokenInfo GetGoogleApiTokenInfo(ExternalAuthRequest externalAuthRequest)
        {
            const string url = "https://www.googleapis.com/oauth2/v3/tokeninfo?id_token={0}";
            var httpClient = new HttpClient();

            var requestUri = new Uri(string.Format(url, externalAuthRequest.Token));

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

            return googleApiTokenInfo;
        }

        private FacebookApiTokenInfo GetFacebookApiTokenInfo(ExternalAuthRequest externalAuthRequest)
        {
            const string url = "https://graph.facebook.com/{0}?access_token={1}&fields={2}";
            var httpClient = new HttpClient();

            var requestUri = new Uri(string.Format(url, externalAuthRequest.UserId, externalAuthRequest.Token, "first_name,last_name,locale,gender,name,email"));

            HttpResponseMessage httpResponseMessage;
            try
            {
                httpResponseMessage = httpClient.GetAsync(requestUri).Result;
            }
            catch (Exception ex)
            {
                throw new ApiException(ex.Message);
            }

            if (httpResponseMessage.StatusCode != HttpStatusCode.OK)
            {
                return null;
            }

            var response = httpResponseMessage.Content.ReadAsStringAsync().Result;
            var facebookApiTokenInfo = JsonConvert.DeserializeObject<FacebookApiTokenInfo>(response);

            return facebookApiTokenInfo;
        }
    }
}
