﻿using AspNetCoreHero.Application.DTOs.Account;
using AspNetCoreHero.Application.DTOs.Mail;
using AspNetCoreHero.Application.DTOs.Settings;
using AspNetCoreHero.Application.Enums;
using AspNetCoreHero.Application.Exceptions;
using AspNetCoreHero.Application.Interfaces;
using AspNetCoreHero.Application.Interfaces.Shared;
using AspNetCoreHero.Application.Wrappers;
using AspNetCoreHero.Infrastructure.Persistence.Helpers;
using AspNetCoreHero.Infrastructure.Persistence.Identity;
using Hangfire;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace AspNetCoreHero.Infrastructure.Persistence.Services
{
    public class AccountService : IAccountService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly JWTSettings _jwtSettings;
        private readonly IMailService _mailService;
        private readonly MailSettings _mailSettings;
        private readonly IDateTimeService _dateTimeService;
        public AccountService(UserManager<ApplicationUser> userManager,
            IOptions<JWTSettings> jwtSettings,
            SignInManager<ApplicationUser> signInManager,
            IMailService mailService,
            IOptions<MailSettings> mailSettings,IDateTimeService dateTimeService)
        {
            _userManager = userManager;
            _jwtSettings = jwtSettings.Value;
            _signInManager = signInManager;
            _mailService = mailService;
            _mailSettings = mailSettings.Value;
            _dateTimeService = dateTimeService;
        }

        public async Task<Response<AuthenticationResponse>> AuthenticateAsync(AuthenticationRequest request, string ipAddress)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                throw new ApiException($"No Accounts Registered with {request.Email}.");
            }
            var result = await _signInManager.PasswordSignInAsync(user.UserName, request.Password, false, lockoutOnFailure: false);
            if (!result.Succeeded)
            {
                throw new ApiException($"Invalid Credentials for '{request.Email}'.");
            }
            if (!user.IsActive)
            {
                throw new ApiException($"Account Not Active");
            }
            //if (!user.EmailConfirmed)
            //{
            //    throw new ApiException($"Account Not Confirmed for '{request.Email}'.");
            //}
            JwtSecurityToken jwtSecurityToken = await TokenHelper.GenerateJWToken(user,_userManager,_jwtSettings);
            AuthenticationResponse response = new AuthenticationResponse();
            response.Id = user.Id;
            response.AccessToken = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
            response.Email = user.Email;
            response.UserName = user.UserName;
            var rolesList = await _userManager.GetRolesAsync(user).ConfigureAwait(false);
            response.Roles = rolesList.ToList();
            response.IsVerified = user.EmailConfirmed;

            if (user.RefreshTokens.Any(a => a.IsActive))
            {
                var activeRefreshToken = user.RefreshTokens.Where(a => a.IsActive == true).FirstOrDefault();
                response.RefreshToken = activeRefreshToken.Token;
                response.RefreshTokenExpiration = activeRefreshToken.Expires;
            }
            else
            {
                var refreshTokenRandom = TokenHelper.GenerateRefreshToken(ipAddress);
                response.RefreshToken = refreshTokenRandom.Token;
                response.RefreshTokenExpiration = refreshTokenRandom.Expires;
                user.RefreshTokens.Add(refreshTokenRandom);
                await _userManager.UpdateAsync(user);
            }

            //var refreshToken = TokenHelper.GenerateRefreshToken(ipAddress);
            //response.RefreshToken = refreshToken.Token;
            return new Response<AuthenticationResponse>(response, $"Authenticated {user.UserName}");
        }

        public async Task<Response<string>> RegisterAsync(RegisterRequest request, string origin)
        {
            var userWithSameUserName = await _userManager.FindByNameAsync(request.UserName);
            if (userWithSameUserName != null)
            {
                throw new ApiException($"Username '{request.UserName}' is already taken.");
            }
            var user = new ApplicationUser
            {
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                UserName = request.UserName,
                IsActive = true,
                ProfilePicture = null,
            };
            var userWithSameEmail = await _userManager.FindByEmailAsync(request.Email);
            if (userWithSameEmail == null)
            {
                var result = await _userManager.CreateAsync(user, request.Password);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, Roles.Basic.ToString());
                    var verificationUri = await SendVerificationEmail(user, origin);
                    //TODO: Attach Email Service here and configure it via appsettings

                    var mailRequest = new MailRequest()
                    {
                        From = _mailSettings.From,
                        To = user.Email,
                        Body = $"Please confirm your account by visiting this URL {verificationUri}",
                        Subject = "Confirm Registration"
                    };
                    BackgroundJob.Enqueue(() => SendEmailBackgroundJob(mailRequest));
                    //await _mailService.SendAsync(mailRequest);
                    return new Response<string>(user.Id, message: $"User Registered. Please confirm your account by visiting this URL {verificationUri}");
                }
                else
                {
                    throw new ApiException($"{result.Errors}");
                }
            }
            else
            {
                throw new ApiException($"Email {request.Email } is already registered.");
            }
        }

        private async Task SendEmailBackgroundJob(MailRequest mailRequest)
        {
            await _mailService.SendAsync(mailRequest);
        }

        private async Task<string> SendVerificationEmail(ApplicationUser user, string origin)
        {
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var route = "api/account/confirm-email/";
            var _enpointUri = new Uri(string.Concat($"{origin}/", route));
            var verificationUri = QueryHelpers.AddQueryString(_enpointUri.ToString(), "userId", user.Id);
            verificationUri = QueryHelpers.AddQueryString(verificationUri, "code", code);
            return verificationUri;
        }

        private async Task<string> SendResetPasswordEmail(ApplicationUser user, string origin)
        {
            var code = await _userManager.GeneratePasswordResetTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var route = "api/account/reset-password";
            var _enpointUri = new Uri(string.Concat($"{origin}/", route));
            var resetPasswordUri = QueryHelpers.AddQueryString(_enpointUri.ToString(), "email", user.Email);
            resetPasswordUri = QueryHelpers.AddQueryString(resetPasswordUri, "token", code);
            return resetPasswordUri;
        }

        public async Task<Response<string>> ConfirmEmailAsync(string userId, string code)
        {
            var user = await _userManager.FindByIdAsync(userId);
            code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
            var result = await _userManager.ConfirmEmailAsync(user, code);
            if (result.Succeeded)
            {
                return new Response<string>(user.Id, message: $"Account Confirmed for {user.Email}. You can now use the /api/Account/authenticate endpoint.");
            }
            else
            {
                throw new ApiException($"An error occured while confirming {user.Email}.");
            }
        }

        public async Task ForgotPassword(ForgotPasswordRequest model, string origin)
        {
            var account = await _userManager.FindByEmailAsync(model.Email);
            if (account == null) return;

            //var code = await _userManager.GeneratePasswordResetTokenAsync(account);
            //var route = "api/account/reset-password/";
            //var _enpointUri = new Uri(string.Concat($"{origin}/", route));

            var resetPasswordUri = await SendResetPasswordEmail(account, origin);

            var emailRequest = new MailRequest()
            {
                Body = $"Please ResetPasswrod your account by visiting this URL - {resetPasswordUri}",
                To = model.Email,
                Subject = "Reset Password",
            };
            BackgroundJob.Enqueue(() => SendEmailBackgroundJob(emailRequest));
           // await _mailService.SendAsync(emailRequest);
        }

        public async Task<Response<string>> ResetPassword(ResetPasswordRequest model)
        {
            var account = await _userManager.FindByEmailAsync(model.Email);
            if (account == null) throw new ApiException($"No Accounts Registered with {model.Email}.");
            var result = await _userManager.ResetPasswordAsync(account, model.Token, model.Password);
            if (result.Succeeded)
            {
                return new Response<string>(model.Email, message: $"Password Resetted.");
            }
            else
            {
                throw new ApiException($"Error occured while reseting the password.");
            }
        }

        public async Task<Response<AuthenticationResponse>> RefreshTokenAsync(string accessToken, string ipAddress)
        {
            var response = new AuthenticationResponse();
            var user = _userManager.Users.SingleOrDefault(u => u.RefreshTokens.Any(t => t.Token == accessToken));
            if (user == null)
            {
                throw new ApiException($"Token did not match any users.");
            }

            var refreshToken = user.RefreshTokens.Single(x => x.Token == accessToken);

            if (!refreshToken.IsActive)
            {
                throw new ApiException($"Token Not Active.");
            }

            //Revoke Current Refresh Token
            //refreshToken.Revoked = DateTime.UtcNow;
            refreshToken.Revoked = _dateTimeService.NowUtc;


            //Generate new Refresh Token and save to Database
            var newRefreshToken = TokenHelper.GenerateRefreshToken(ipAddress);
            user.RefreshTokens.Add(newRefreshToken);
            await _userManager.UpdateAsync(user);

            //Generates new jwt

            JwtSecurityToken jwtSecurityToken = await TokenHelper.GenerateJWToken(user, _userManager, _jwtSettings);
            response.Id = user.Id;
            response.AccessToken = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
            response.Email = user.Email;
            response.UserName = user.UserName;
            var rolesList = await _userManager.GetRolesAsync(user).ConfigureAwait(false);
            response.Roles = rolesList.ToList();
            response.IsVerified = user.EmailConfirmed;
            response.RefreshToken = newRefreshToken.Token;
            response.RefreshTokenExpiration = newRefreshToken.Expires;
            return new Response<AuthenticationResponse>(response);         
        }

        public async Task<bool> RevokeToken(string accessToken, string ipAddress)
        {
            var user = _userManager.Users.SingleOrDefault(u => u.RefreshTokens.Any(t => t.Token == accessToken));

            // return false if no user found with token
            if (user == null) return false;

            var refreshToken = user.RefreshTokens.Single(x => x.Token == accessToken);

            // return false if token is not active
            if (!refreshToken.IsActive) return false;

            // revoke token and save
            //refreshToken.Revoked = DateTime.UtcNow;
            refreshToken.Revoked = _dateTimeService.NowUtc;
            refreshToken.RevokedByIp = ipAddress;            
            await _userManager.UpdateAsync(user);
            return true;
        }

        public async Task<Response<List<RefreshToken>>> GetRefreshTokenList(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            return new Response<List<RefreshToken>>(user?.RefreshTokens);
        }
    }
}
