using AspNetCoreHero.Application.DTOs.Settings;
using AspNetCoreHero.Application.Enums;
using AspNetCoreHero.Application.Interfaces.Shared;
using AspNetCoreHero.Infrastructure.Shared.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;

namespace AspNetCoreHero.Infrastructure.Shared.Extensions
{
    public static class ServiceCollectionExtension
    {
        public static void AddSharedInfrastructure(this IServiceCollection services, IConfiguration _config)
        {
            services.Configure<MailSettings>(_config.GetSection("MailSettings"));
            services.Configure<CacheSettings>(_config.GetSection("MemoryCacheSettings"));
            services.AddSingleton<IDateTimeService, DateTimeService>();
            services.AddSingleton<IMailService, MailService>();

            // AddCaching
            services.AddSingleton<MemoryCacheService>();
            services.AddSingleton<RedisCacheService>();
            services.AddSingleton<Func<CacheTech, ICacheService>>(serviceProvider => key =>
            {
                switch (key)
                {
                    case CacheTech.Memory:
                        return serviceProvider.GetService<MemoryCacheService>();
                    case CacheTech.Redis:
                        return serviceProvider.GetService<RedisCacheService>();
                    default:
                        return serviceProvider.GetService<MemoryCacheService>();
                }
            });
        }    
    }
}
