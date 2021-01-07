﻿using Hangfire;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Globalization;


namespace AspNetCoreHero.Web.Extensions
{
    public static class ServiceCollectionExtension
    {
        public static void AddMultiLingualSupport(this IServiceCollection services)
        {
            services.Configure<RequestLocalizationOptions>(options =>
            {
                var cultures = new List<CultureInfo> {
        new CultureInfo("en"),
         new CultureInfo("ar"),
        new CultureInfo("fr")
                };
                options.DefaultRequestCulture = new Microsoft.AspNetCore.Localization.RequestCulture("en");
                options.SupportedCultures = cultures;
                options.SupportedUICultures = cultures;
            });
        }

        public static void AddHangfire(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddHangfire(x => x.UseSqlServerStorage(configuration.GetConnectionString("DefaultConnection")));
            services.AddHangfireServer();
        }
    }
}
