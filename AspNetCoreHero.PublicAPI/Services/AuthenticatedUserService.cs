using AspNetCoreHero.Application.Interfaces.Shared;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AspNetCoreHero.PublicAPI.Services
{
    public class AuthenticatedUserService : IAuthenticatedUserService
    {
        public AuthenticatedUserService(IHttpContextAccessor httpContextAccessor)
        {
            UserId = httpContextAccessor.HttpContext?.User?.FindFirstValue("uid");
            Username = httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            Email = httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Email);       
            Roles = httpContextAccessor.HttpContext?.User?.FindAll(ClaimTypes.Role).Select(x=>x.Value).ToList();
        }

        public string UserId { get; }

        public string Username { get; }
        public string Email { get; }
        public List<string> Roles { get;}
    }
}
