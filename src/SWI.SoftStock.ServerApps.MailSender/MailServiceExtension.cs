using System;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SWI.SoftStock.ServerApps.DataModel2;
using SWI.SoftStock.ServerApps.DataModel2.Identity;
using SWI.SoftStock.ServerApps.WebApplicationContracts.SecurityService;

namespace SWI.SoftStock.ServerApps.MailSender
{
    public static class MailServiceExtension
    {
        public static IServiceCollection AddMailSender(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton(configuration.GetSection("MailServiceOptions").Get<MailServiceOptions>());

            services.AddHostedService(f => new MailService(
                f.GetService<ILogger<MailService>>(),
                f.GetService<MailServiceOptions>(),
                f.GetService<IVerificationService>(),
                new CustomUserManager(
                    new CustomUserStore(f.GetService<IDbContextFactory<MainDbContext>>().CreateDbContext()),
                    f.GetService<IOptions<IdentityOptions>>(),
                    new PasswordHasher<User>(),
                    null,
                    null,
                    null,
                    null,
                    f.GetService<IServiceProvider>(),
                    f.GetService<ILogger<UserManager<User>>>()
                )
            ));
            return services;
        }
    }
}