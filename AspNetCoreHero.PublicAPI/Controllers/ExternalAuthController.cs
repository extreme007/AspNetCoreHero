﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AspNetCoreHero.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AspNetCoreHero.PublicAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExternalAuthController : ControllerBase
    {
        private readonly IExternalAuthService _externalAuthService;
        public ExternalAuthController(IExternalAuthService externalAuthService)
        {
            _externalAuthService = externalAuthService;
        }

        [HttpGet("Google")]
        public async Task<IActionResult> GoogleAuthAsync()
        {
            string BearerToken = Request.Headers["Authorization"].ToString();
            if (string.IsNullOrEmpty(BearerToken))
                return Ok(false);
            var tokenArray = BearerToken.Split(" ").ToArray();
            var token = tokenArray[1];
            return Ok(await _externalAuthService.ExternalAuthenticateAsync(token, GenerateIPAddress()));
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
