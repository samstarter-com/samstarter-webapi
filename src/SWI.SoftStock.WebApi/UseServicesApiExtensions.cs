using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SWI.SoftStock.ServerApps.DataModel2;
using SWI.SoftStock.ServerApps.DataModel2.Identity;
using SWI.SoftStock.ServerApps.DataModel2.Identity.Models;
using SWI.SoftStock.ServerApps.WebApplicationContracts;
using SWI.SoftStock.ServerApps.WebApplicationContracts.DocumentService;
using SWI.SoftStock.ServerApps.WebApplicationContracts.SecurityService;
using SWI.SoftStock.ServerApps.WebApplicationServices;

namespace SWI.SoftStock.WebApi
{
    public static class UseServicesApiExtensions
    {
        public static IServiceCollection AddServicesApi(this IServiceCollection services, IConfiguration configuration)
        {
            var dbOption = (new DbContextOptionsBuilder<MainDbContext>()).UseLazyLoadingProxies().UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
            services.AddSingleton(b => dbOption.Options);

            services.AddDbContext<MainDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection")));

            services.AddSingleton<MainDbContextFactory>();
            services.AddScoped<ISecurityService, SecurityService>();

            services.AddScoped<MainDbContext>();
            services.AddScoped<IUserStore<User>>(fac => new CustomUserStore(fac.GetService<MainDbContext>()));
            services.AddScoped<IRoleStore<CustomRole>>(fac => new CustomRoleStore(fac.GetService<MainDbContext>()));

            services.AddScoped<CustomUserManager>();
            services.AddScoped<CustomRoleManager>();

            services.AddScoped<ICompanyService, CompanyService>();
            services.AddScoped<IStructureUnitService, StructureUnitService>();
            services.AddScoped<IUserService, UserService>();


            services.AddScoped<IFeedbackService, FeedbackService>();
            services.AddScoped<IPersonalMachineService, PersonalMachineService>();
            services.AddScoped<ISoftwareService, SoftwareService>();

            services.AddScoped<IPersonalLicenseRequestService, PersonalLicenseRequestService>();

            services.AddScoped<IDocumentService, DocumentService>();

            services.AddScoped<ILicenseRequestService, LicenseRequestService>();
            services.AddScoped<IMachineService, MachineService>();
            services.AddScoped<ILicenseService, LicenseService>();
            services.AddScoped<ILicensingService, LicensingService>();

            services.AddScoped<IObservableService, ObservableService>();

            services.AddSingleton<IVerificationService, VerificationService>(provider =>
                new VerificationService(
                    new CustomUserStore(provider.GetService<MainDbContextFactory>().Create())));
            return services;
        }
    }
}
