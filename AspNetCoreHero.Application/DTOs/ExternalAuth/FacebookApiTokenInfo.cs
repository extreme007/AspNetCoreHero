using System;
using System.Collections.Generic;
using System.Text;

namespace AspNetCoreHero.Application.DTOs.ExternalAuth
{
    public class FacebookApiTokenInfo
    {
        public string id { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string name { get; set; }
        public string email { get; set; }
    }
}
