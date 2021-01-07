using AspNetCoreHero.Application.Extensions;
using AspNetCoreHero.Application.Interfaces.Shared;
using AspNetCoreHero.Infrastructure.Persistence.Extensions;
using AspNetCoreHero.Infrastructure.Shared.Extensions;
using AspNetCoreHero.Web.Extensions;
using AspNetCoreHero.Web.Filter;
using AspNetCoreHero.Web.Services;
using AutoMapper;
using FluentValidation.AspNetCore;
using Hangfire;
using Hangfire.Dashboard;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NToastNotify;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace AspNetCoreHero.Web
{
    public class Startup
    {
        public IConfiguration _configuration { get; }
        public Startup(IConfiguration configuration)
        {
            Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(configuration).CreateLogger();
            _configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddApplicationLayer();
            services.AddLocalization(options => options.ResourcesPath = "Resources");
            services.AddMvc()
                .AddViewLocalization(Microsoft.AspNetCore.Mvc.Razor.LanguageViewLocationExpanderFormat.Suffix)
                .AddDataAnnotationsLocalization(options => {
                    options.DataAnnotationLocalizerProvider = (type, factory) =>
                        factory.Create(typeof(SharedResource));
                }).AddNToastNotifyToastr(new ToastrOptions()
                {
                    ProgressBar = true,
                    PositionClass = ToastPositions.BottomRight
                });
            services.AddRouting(o => o.LowercaseUrls = true);
            services.AddSharedInfrastructure(_configuration);
            services.AddPersistenceInfrastructureForWeb(_configuration);
            services.AddAuthenticationSchemeForWeb(_configuration);
            services.AddHttpContextAccessor();
            services.AddMultiLingualSupport();
            services.AddRazorPages().AddRazorRuntimeCompilation().AddFluentValidation(fv =>
            {
                fv.RegisterValidatorsFromAssembly(Assembly.GetExecutingAssembly());
                fv.RunDefaultMvcValidationAfterFluentValidationExecutes = false;
            });
            services.AddAutoMapper(Assembly.GetExecutingAssembly());
            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddTransient<IActionContextAccessor, ActionContextAccessor>();
            services.AddScoped<IViewRenderService, ViewRenderService>();
            //For In-Memory Caching
            services.AddMemoryCache();
            services.AddScoped<IAuthenticatedUserService, AuthenticatedUserService>();

            //Hangfire
            services.AddHangfire(_configuration);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.UseSerilogLogging();
            //NOTE this line must be above .UseMvc() line.
            app.UseNToastNotify();
            app.UseMultiLingualFeature();
            app.UseStatusCodePagesWithRedirects("/Error?code={0}");
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapControllerRoute(
                  name: "default",
                  pattern: "{controller}/{action=Index}/{id?}");
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
