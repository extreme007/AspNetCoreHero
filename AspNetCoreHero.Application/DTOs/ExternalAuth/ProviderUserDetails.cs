using System;
using System.Collections.Generic;
using System.Text;

namespace AspNetCoreHero.Application.DTOs.ExternalAuth
{
    public class ProviderUserDetails
    {
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Locale { get; set; }
        public string Name { get; set; }
        public string ProviderUserId { get; set; }
    }
}
