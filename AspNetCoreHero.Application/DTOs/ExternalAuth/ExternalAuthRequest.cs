using System;
using System.Collections.Generic;
using System.Text;

namespace AspNetCoreHero.Application.DTOs.ExternalAuth
{
    public class ExternalAuthRequest
    {
        public string Type { get; set; }
        public string Token { get; set; }
        public string UserId { get; set; }
    }
}
