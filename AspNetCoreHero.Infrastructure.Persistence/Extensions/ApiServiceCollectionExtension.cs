using AspNetCoreHero.Application.DTOs.Settings;
using AspNetCoreHero.Application.Interfaces;
using AspNetCoreHero.Application.Interfaces.Contexts;
using AspNetCoreHero.Application.Interfaces.Repositories;
using AspNetCoreHero.Application.Wrappers;
using AspNetCoreHero.Infrastructure.Persistence.Contexts;
using AspNetCoreHero.Infrastructure.Persistence.Identity;
using AspNetCoreHero.Infrastructure.Persistence.Repositories;
using AspNetCoreHero.Infrastructure.Persistence.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AspNetCoreHero.Infrastructure.Persistence.Extensions
{
    public static class ApiServiceCollectionExtension
    {
        public static void AddPersistenceInfrastructureForApi(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<IdentityContext>(options => options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
            services.AddIdentity<ApplicationUser, IdentityRole>(options =>{
                    // Default Lockout settings.
                    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                    options.Lockout.MaxFailedAccessAttempts = 5;
                    options.Lockout.AllowedForNewUsers = true;

                    // Default Password settings.
                    options.Password.RequireDigit = true;
                    options.Password.RequireLowercase = true;
                    options.Password.RequireNonAlphanumeric = true;
                    options.Password.RequireUppercase = true;
                    options.Password.RequiredLength = 6;
                    options.Password.RequiredUniqueChars = 1;

                    // Default SignIn settings.
                    options.SignIn.RequireConfirmedEmail = false;
                    options.SignIn.RequireConfirmedPhoneNumber = false;

                    // Default User settings.
                    options.User.AllowedUserNameCharacters =
                            "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
                    options.User.RequireUniqueEmail = false;
                }).AddEntityFrameworkStores<IdentityContext>().AddDefaultTokenProviders();
            services.AddDbContext<ApplicationContext>(options => options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"), b => b.MigrationsAssembly(typeof(ApplicationContext).Assembly.FullName)));
            #region Services
            services.AddTransient<IExternalAuthService, ExternalAuthService>();
            services.AddTransient<IAccountService, AccountService>();
            #endregion
            services.Configure<JWTSettings>(configuration.GetSection("JWTSettings"));
            services.Configure<MailSettings>(configuration.GetSection("MailSettings"));
            services.Configure<PaginationSettings>(configuration.GetSection("PaginationConfiguration"));
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(o =>
               {
                   o.RequireHttpsMetadata = false;
                   o.SaveToken = false;
                   o.TokenValidationParameters = new TokenValidationParameters
                   {
                       ValidateIssuerSigningKey = true,
                       ValidateIssuer = true,
                       ValidateAudience = true,
                       ValidateLifetime = true,
                       ClockSkew = TimeSpan.Zero,
                       ValidIssuer = configuration["JWTSettings:Issuer"],
                       ValidAudience = configuration["JWTSettings:Audience"],
                       IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWTSettings:Key"]))
                   };
                   o.Events = new JwtBearerEvents()
                   {
                       OnChallenge = context =>
                       {
                           context.HandleResponse();
                           context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                           context.Response.ContentType = "application/json";
                           var result = JsonConvert.SerializeObject(new Response<string>("You are not Authorized"));
                           return context.Response.WriteAsync(result);
                       },
                       OnForbidden = context =>
                       {
                           context.Response.StatusCode = 403;
                           context.Response.ContentType = "application/json";
                           var result = JsonConvert.SerializeObject(new Response<string>("You are not authorized to access this resource"));
                           return context.Response.WriteAsync(result);
                       },
                       //OnAuthenticationFailed = context =>
                       //{
                       //    //context.NoResult();
                       //    //context.Response.StatusCode = 500;
                       //    //context.Response.ContentType = "text/plain";
                       //    //return context.Response.WriteAsync(context.Exception.ToString());
                       //    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                       //    context.Response.ContentType = "application/json";
                       //    var result = JsonConvert.SerializeObject(new Response<string>(context.Exception.Message));
                       //    return context.Response.WriteAsync(result);
                       //},
                       //OnMessageReceived = context =>
                       //{
                       //    context.Token = context.Request.Cookies["your-cookie"];
                       //    return Task.CompletedTask;
                       //}
                   };
               });


            /// Cookie settings
            /// 

            services.ConfigureApplicationCookie(options =>
            {   
                //options.Cookie.SameSite = SameSiteMode.None;
                options.Cookie.HttpOnly = true;
                //options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
                options.SlidingExpiration = true;
            });

            /// Cors settings
            ///

            if (configuration.GetSection("AllowedOrigins").Exists())
            {
                var origins = configuration.GetSection("AllowedOrigins").Value.Split(";");
                if (origins.Length > 0)
                {
                    services.AddCors(options =>
                    {
                        options.AddDefaultPolicy(
                            builder =>
                            {
                                builder.WithOrigins(origins)
                                    .AllowAnyMethod()
                                    .AllowAnyHeader()
                                    .AllowCredentials();
                            });
                    });
                }
                else
                {
                    services.AddCors(options =>
                    {
                        options.AddDefaultPolicy(
                            builder =>
                            {
                                builder.AllowAnyOrigin()
                                    .AllowAnyMethod()
                                    .AllowAnyHeader()
                                    .AllowCredentials();
                            });
                    });
                }

            }

            services.AddScoped<IApplicationDbContext, ApplicationContext>();
        }
    }
}
