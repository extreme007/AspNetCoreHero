﻿using System;
using System.Collections.Generic;
using System.Text;

namespace AspNetCoreHero.Application.DTOs.Settings
{
    public class MailSettings
    {
        public string From { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string DisplayName { get; set; }
    }
}
