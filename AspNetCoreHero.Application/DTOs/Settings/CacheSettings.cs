﻿using System;
using System.Collections.Generic;
using System.Text;

namespace AspNetCoreHero.Application.DTOs.Settings
{
    public class CacheSettings
    {
        public int AbsoluteExpirationInHours { get; set; }
        public int SlidingExpirationInMinutes { get; set; }
    }
}
