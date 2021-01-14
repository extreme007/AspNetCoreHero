using AspNetCoreHero.Application.DTOs.Settings;
using AspNetCoreHero.Application.Extensions;
using AspNetCoreHero.Application.Interfaces.Shared;
using AspNetCoreHero.Infrastructure.Persistence.Extensions;
using AspNetCoreHero.Infrastructure.Shared.Extensions;
using AspNetCoreHero.PublicAPI.Extensions;
using AspNetCoreHero.PublicAPI.Filter;
using AspNetCoreHero.PublicAPI.Services;
using Hangfire;
using Hangfire.Dashboard;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System;
using System.IO;

namespace AspNetCoreHero.PublicAPI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IConfiguration _configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddApplicationLayer();
            services.AddSharedInfrastructure(_configuration);
            services.AddApiVersioningExtension();
            services.AddSwaggerService();
            services.AddControllers();
            services.AddPersistenceInfrastructureForApi(_configuration);
            services.AddRepositories();
            services.AddHttpContextAccessor();
            services.AddHealthChecks();
            //For In-Memory Caching
            services.AddMemoryCache();
            var AbsoluteExpiration = _configuration.GetValue<int>("MemoryCacheSettings:AbsoluteExpirationInHours");
            var SlidingExpirationInMinutes = _configuration.GetValue<int>("MemoryCacheSettings:SlidingExpirationInMinutes");
            services.AddDistributedMemoryCache(option => {
                new MemoryCacheEntryOptions
                {
                    AbsoluteExpiration = DateTime.Now.AddHours(AbsoluteExpiration),
                    Priority = CacheItemPriority.High,
                    SlidingExpiration = TimeSpan.FromMinutes(SlidingExpirationInMinutes),
                };
            });
            services.AddScoped<IAuthenticatedUserService, AuthenticatedUserService>();

            //Hangfire
            services.AddHangfire(_configuration);


            services.Configure<FormOptions>(o => {
                o.ValueLengthLimit = int.MaxValue;
                o.MultipartBodyLengthLimit = int.MaxValue;
                o.MemoryBufferThreshold = int.MaxValue;
            });

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseHttpsRedirection();
            app.UseRouting();
            //app.UseCookiePolicy();
            app.UseCors();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseSwaggerService();
            app.UseErrorHandlingMiddleware();
            app.UseHealthChecks("/health");
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            //app.UseStaticFiles();
            app.UseStaticFiles(new StaticFileOptions()
            {
                FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), @"Resources")),
                RequestPath = new PathString("/Resources")
            });

            ////Hangfire
            app.UseHangfireDashboard("/hangfire", new DashboardOptions
            {
                IsReadOnlyFunc = (DashboardContext context) => true,
                Authorization = new[] { new HangfireAuthorizationFilter("SuperAdmin") },
                //AppPath = null// "localhost.com:3000"
                AppPath = !string.IsNullOrEmpty(_configuration.GetSection("AllowedOrigins")?.Value) ? _configuration.GetSection("AllowedOrigins")?.Value.Split(";")[0] : null,
            });
        }
    }
}
