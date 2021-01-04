using System;
using System.Threading.Tasks;
using AspNetCoreHero.Application.DTOs.Account;
using AspNetCoreHero.Application.Interfaces;
using AspNetCoreHero.Application.Wrappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AspNetCoreHero.PublicAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;
        public AccountController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        [HttpPost("authenticate")]
        [AllowAnonymous]
        public async Task<IActionResult> AuthenticateAsync(AuthenticationRequest request)
        {
            var result = await _accountService.AuthenticateAsync(request, GenerateIPAddress());
            
            SetTokenInCookie(result?.Data?.RefreshToken, result?.Data?.AccessToken);
            return Ok(result);
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> RegisterAsync(RegisterRequest request)
        {
            var origin = Request.Headers["origin"];
            return Ok(await _accountService.RegisterAsync(request, origin));
        }

        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmailAsync([FromQuery] string userId, [FromQuery] string code)
        {
            //var origin = Request.Headers["origin"];
            return Ok(await _accountService.ConfirmEmailAsync(userId, code));
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordRequest model)
        {
            await _accountService.ForgotPassword(model, Request.Headers["origin"]);
            return Ok();
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordRequest model)
        {
            return Ok(await _accountService.ResetPassword(model));
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken()
        {
            var refreshToken = Request.Cookies["refreshToken"];
            var response = await _accountService.RefreshTokenAsync(refreshToken, GenerateIPAddress());
            if (!string.IsNullOrEmpty(response.Data.RefreshToken))
                SetTokenInCookie(response.Data.RefreshToken);
            return Ok(response);
        }

        [HttpPost("revoke-token")]
        public async Task<IActionResult> RevokeToken(RevokeTokenRequest model)
        {
            // accept token from request body or cookie
            var token = model.Token ?? Request.Cookies["refreshToken"];

            if (string.IsNullOrEmpty(token))
                return Ok(new Response<string>() { Succeeded = false, Message = "Token is required" });

            var response =await _accountService.RevokeToken(token);

            if (!response)
                return Ok(new Response<string>() { Succeeded = false, Message = "Token not found" });

            return Ok(new Response<string>() { Succeeded = true, Message = "Token revoked" });
        }

        [Authorize]
        [HttpPost("tokens/{id}")]
        public async Task<IActionResult> GetRefreshTokens(string id)
        {
            var refreshTokens = await _accountService.GetRefreshTokenList(id);
            return Ok(refreshTokens);
        }

        private void SetTokenInCookie(string refreshToken, string accessTokenHangfire = "")
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = DateTime.UtcNow.AddDays(7),
                //SameSite = SameSiteMode.None
            };
            Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
            Response.Cookies.Append("accessTokenHangfire", accessTokenHangfire, cookieOptions);
        }

        private string GenerateIPAddress()
        {
            if (Request.Headers.ContainsKey("X-Forwarded-For"))
                return Request.Headers["X-Forwarded-For"];
            else
                return HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
        }
    }
}
