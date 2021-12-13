using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using SWI.SoftStock.ServerApps.DataModel2;
using SWI.SoftStock.ServerApps.WebApplicationServices;
using SWI.SoftStock.WebApi.Authentication;
using SWI.SoftStock.WebApi.Common;
using System;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using SWI.SoftStock.ServerApps.WebApplicationContracts;

namespace SWI.SoftStock.WebApi
{
    public static class UseOAuthExtensions
    {
        public static IServiceCollection AddOAuth(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection")));

            services.AddIdentityCore<User>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireUppercase = true;
                options.Password.RequiredLength = 6;
                options.Password.RequiredUniqueChars = 0;
                options.SignIn.RequireConfirmedAccount = true;

                // Lockout settings.
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;
            }).AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();

            services.AddAuthentication(auth =>
            {
                auth.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                auth.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = configuration["Tokens:Issuer"],
                    ValidAudience = configuration["Tokens:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Tokens:Key"])),
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    RequireExpirationTime = true,
                    ValidateIssuerSigningKey = true
                };
                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                        {
                            context.Response.Headers.Add("Token-Expired", "true");
                        }
                        return Task.CompletedTask;
                    }
                };
            });

            services.AddSingleton(f => new ExpiredTokenValidationParameters()
            {
                ValidIssuer = configuration["Tokens:Issuer"],
                ValidAudience = configuration["Tokens:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Tokens:Key"])),
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = false,
                RequireExpirationTime = true,
                ValidateIssuerSigningKey = true
            });

            services.AddScoped<IUserService, UserService>();
            services.AddSingleton(f => new JwtTokenConfig()
            {
                AccessTokenExpiration = double.Parse(configuration["Tokens:AccessTokenExpiration"]),
                Audience = configuration["Tokens:Audience"],
                Issuer = configuration["Tokens:Issuer"],
                RefreshTokenExpiration = double.Parse(configuration["Tokens:RefreshTokenExpiration"]),
                Secret = configuration["Tokens:Key"]
            });

            services.AddScoped<IJwtAuthManager, JwtAuthManager>();

            services.AddAuthorization(options =>
                {
                    options.AddPolicy(Constants.PolicyAdministrator, policy => policy.RequireClaim(ClaimTypes.Role, "Admin"));
                    options.AddPolicy(Constants.PolicyManager, policy => policy.RequireClaim(ClaimTypes.Role, "Manager"));
                    options.AddPolicy(Constants.PolicyUser, policy => policy.RequireClaim(ClaimTypes.Role, "User"));
                });
            return services;
        }
    }
}
