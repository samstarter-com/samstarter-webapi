using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SWI.SoftStock.ServerApps.DataAccess2;
using SWI.SoftStock.ServerApps.DataModel2;
using SWI.SoftStock.ServerApps.DataModel2.Identity;
using SWI.SoftStock.ServerApps.WebApplicationContracts;
using SWI.SoftStock.ServerApps.WebApplicationContracts.CompanyService.Add;
using SWI.SoftStock.ServerApps.WebApplicationContracts.SecurityService;
using SWI.SoftStock.ServerApps.WebApplicationContracts.SecurityService.ChangePassword;
using SWI.SoftStock.ServerApps.WebApplicationContracts.SecurityService.CreateUser;
using SWI.SoftStock.ServerApps.WebApplicationContracts.SecurityService.GetAccount;
using SWI.SoftStock.ServerApps.WebApplicationContracts.SecurityService.RegisterCompany;
using SWI.SoftStock.ServerApps.WebApplicationContracts.SecurityService.ValidateUser;
using SWI.SoftStock.ServerApps.WebApplicationContracts.UserService.SetUsersRoles;
using SWI.SoftStock.ServerApps.WebApplicationModel;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;

namespace SWI.SoftStock.ServerApps.WebApplicationServices
{
    public class SecurityService : ISecurityService
    {    
        private readonly IUserService userService;

        private readonly ILogger<SecurityService> log;

        private readonly IStructureUnitService structureUnitService;

        private readonly ICompanyService companyService;

        private readonly CustomUserManager customUserManager;

        private readonly CustomRoleManager customRoleManager;

        private readonly IDbContextFactory<MainDbContext> dbFactory;

        public SecurityService(
            ILogger<SecurityService> log, 
            IStructureUnitService structureUnitService,           
            IUserService userService,          
            ICompanyService companyService, 
            CustomUserManager customUserManager,
            CustomRoleManager customRoleManager,
            IDbContextFactory<MainDbContext> dbFactory)
        {
            this.log = log;           
            this.structureUnitService = structureUnitService;         
            this.structureUnitService = structureUnitService;
            this.userService = userService;          
            this.companyService = companyService;
            this.customUserManager = customUserManager;
            this.customRoleManager = customRoleManager;
            this.dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
        }

        #region ISecurityService Members

        public async Task<RegisterCompanyResponse> RegisterCompanyAsync(RegisterCompanyRequest request)
        {
            var model = request.RegisterModel;
            try
            {
                using var tranScope = new System.Transactions.TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
                var (user, status) = await CreateUserAsync(model.UserName,
                    model.Password,
                    model.Email,
                    model.CompanyName);

                if (status != RegisterCompanyStatus.Success)
                {
                    log.LogWarning("CodeFirstMembershipProvider.CreateUser return status:{0}", status);
                    return new RegisterCompanyResponse { Status = status };
                }

                var result = await structureUnitService.GetCompanyIdByName(model.CompanyName);
                var userId = user.Id;
                var setUsersRolesRequest = new SetUsersRolesRequest
                {
                    UserId = userId,
                    Roles = new[] {
                            new RoleData() { RoleId = customRoleManager.Roles.Single(r=>r.Name== "Admin").Id,  IsInRole = true },
                            new RoleData() { RoleId = customRoleManager.Roles.Single(r => r.Name == "User").Id, IsInRole = true } },
                    StructureUnitId = result.CompanyUniqueId
                };
                var setUsersRolesResponse = await userService.SetUsersRolesAsync(setUsersRolesRequest);

                if (setUsersRolesResponse.Status != UserRoleUpdateStatus.Success)
                {
                    log.LogWarning("UserService.SetUsersRoles return status:{0}", setUsersRolesResponse.Status);
                    return new RegisterCompanyResponse { Status = RegisterCompanyStatus.UnknownError };
                }
                var isSetCompanyAccount = await SetCompanyAccount(result.CompanyId, request.AccountName);
                if (!isSetCompanyAccount)
                {
                    log.LogWarning("Cannot set SetCompanyAccount");
                    return new RegisterCompanyResponse { Status = RegisterCompanyStatus.UnknownError };
                }
                tranScope.Complete();
                return new RegisterCompanyResponse { Status = status };
            }
            catch (Exception e)
            {
                log.LogError(0, e, e.Message);
                return new RegisterCompanyResponse { Status = RegisterCompanyStatus.UnknownError };
            }
        }
     
        public async Task<CreateUserResponse> CreateUserAsync(CreateUserRequest request)
        {           
            var model = request.UserModel;
            Guid userId;
            try
            {
                using var tranScope = new System.Transactions.TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
                var (user, registerCompanyStatus) = await CreateUserAsync(model.UserName,
                    model.Password,
                    model.Email,
                    request.StructureUnitId,
                    model.FirstName,
                    model.LastName);
                if (registerCompanyStatus != RegisterCompanyStatus.Success)
                {
                    log.LogWarning("CodeFirstMembershipProvider.CreateUser return status:{0}", registerCompanyStatus);
                    return new CreateUserResponse { Status = (CreateUserStatus)registerCompanyStatus };
                }

                userId = user.Id;

                var setUsersRolesRequest = new SetUsersRolesRequest
                {
                    UserId = userId,
                    Roles = new[] { new RoleData { RoleId = customRoleManager.Roles.Single(r => r.Name == "User").Id, IsInRole = true } },
                    StructureUnitId = request.StructureUnitId
                };
                var setUsersRolesResponse = await userService.SetUsersRolesAsync(setUsersRolesRequest);

                if (setUsersRolesResponse.Status != UserRoleUpdateStatus.Success)
                {
                    log.LogWarning("UserService.SetUsersRoles return status:{0}", setUsersRolesResponse.Status);
                    return new CreateUserResponse { Status = CreateUserStatus.UnknownError };
                }
                tranScope.Complete();
            }
            catch (Exception e)
            {
                log.LogError(0, e, e.Message);
                return new CreateUserResponse { Status = CreateUserStatus.UnknownError };
            }
            return new CreateUserResponse { Status = CreateUserStatus.Success, UserId = userId };
        }

        private async Task<Tuple<User, RegisterCompanyStatus>> CreateUserAsync(string username,
            string password,
            string email,
            Guid structureUnitId,
            string firstName,
            string lastName)
        {
            var status = CheckNewUser(username, password, email, out var hashedPassword);
            if (status != RegisterCompanyStatus.Success)
            {
                return Tuple.Create((User)null, status);
            }

            if (customUserManager.Users.Any(usr => usr.UserName == username))
            {
                status = RegisterCompanyStatus.DuplicateUserName;
                return Tuple.Create((User)null, status);
            }

            if (customUserManager.Users.Any(usr => usr.Email == email))
            {
                status = RegisterCompanyStatus.DuplicateEmail;
                return Tuple.Create((User)null, status);
            }

            var companyResult = structureUnitService.GetCompanyIdByStructureUnitId(structureUnitId);
                       
            var newUser = new User
            {
                Id = Guid.NewGuid(),
                UserName = username,
                PasswordHash = hashedPassword,
                Email = email,
                CreateDate = DateTime.UtcNow,
                LastPasswordChangedDate = DateTime.UtcNow,
                PasswordFailuresSinceLastSuccess = 0,
                LastLoginDate = DateTime.UtcNow,
                LastActivityDate = DateTime.UtcNow,
                LastLockoutDate = DateTime.UtcNow,
                LockoutEnabled = true,
                LastPasswordFailureDate = DateTime.UtcNow,
                SendStatus = SendStatus.None,
                SendCount = 0,
                CompanyId = companyResult.CompanyId,
                FirstName = firstName,
                LastName = lastName
            };

            var identityResult = await customUserManager.CreateAsync(newUser);
            if (!identityResult.Succeeded)
            {
                status = RegisterCompanyStatus.ProviderError;
                return Tuple.Create((User)null, status);
            }

            status = RegisterCompanyStatus.Success;
            return Tuple.Create(newUser, status);    
        }

        private async Task<bool> SetCompanyAccount(int companyId, string accountName)
        {
            var dbContext = dbFactory.CreateDbContext();
            using IUnitOfWork unitOfWork = new UnitOfWork(dbContext);
            var company = unitOfWork.StructureUnitRepository.GetById(companyId);
            if (company == null || company.UnitType != UnitType.Company)
            {
                return false;
            }
            var account = await
                unitOfWork.AccountRepository.GetAll().SingleOrDefaultAsync(a => a.Name == accountName && a.IsAvailable);
            if (account == null)
            {
                return false;
            }
            company.Account = account;
            unitOfWork.StructureUnitRepository.Update(company, company.Id);
            unitOfWork.StructureUnitRepository.SaveChanges();
            return true;
        }

        public async Task<ValidateUserResponse> ValidateUser(ValidateUserRequest request)
        {
            ValidateUserStatus status;
            try
            {
                var user = await customUserManager.FindByNameAsync(request.UserName);
                if (user == null)
                {
                    return new ValidateUserResponse { Status = ValidateUserStatus.Fail, User = null };
                }

                var isValid = await customUserManager.CheckPasswordAsync(user, request.Password);

                if (isValid && !user.EmailConfirmed)
                {
                    return new ValidateUserResponse { Status = ValidateUserStatus.NotApproved, User = user };
                }

                status = isValid ? ValidateUserStatus.Success : ValidateUserStatus.Fail;
                return new ValidateUserResponse { Status = status, User = user };
            }
            catch (Exception e)
            {
                status = ValidateUserStatus.UnknownError;
                log.LogError(0, e, e.Message);
                return new ValidateUserResponse {Status = status, User = null};
            }
        }

        public async Task<GetAccountResponse> GetAccount(GetAccountRequest request)
        {
            var result = new GetAccountResponse();
            var dbContext = dbFactory.CreateDbContext();
            using IUnitOfWork unitOfWork = new UnitOfWork(dbContext);
            var isAdmin = await unitOfWork.StructureUnitUserRoleRepository.GetAll()
                .AnyAsync(sur => sur.UserUserId == request.UserId && sur.Role.Name == "Admin");
            if (!isAdmin)
            {
                result.Status = GetAccountStatus.IsNotAdmin;
            }
            var user = customUserManager.Users.Single(u => u.Id == request.UserId);
            var account = user.Company.Account;
            var installedMachineCount = user.Company.CompanyMachines.Count;
            var model = MapperFromModelToView.MapToAccountModel(account, installedMachineCount);
            result.AccountModel = model;
            result.Status = GetAccountStatus.Success;
            return result;
        }

        #endregion

        private async Task<Tuple<User, RegisterCompanyStatus>> CreateUserAsync(string username,
            string password,
            string email,
            string company)
        {
            var status = CheckNewUser(username, password, email, out var hashedPassword);
            if (status != RegisterCompanyStatus.Success)
            {
                return Tuple.Create((User)null, status);
            }

            if (companyService.IsCompanyExists(company))
            {
                status = RegisterCompanyStatus.DuplicateCompany;
                return Tuple.Create((User)null, status);
            }


            if (customUserManager.Users.Any(usr => usr.UserName == username))
            {
                status = RegisterCompanyStatus.DuplicateUserName;
                return Tuple.Create((User)null, status);
            }

            if (customUserManager.Users.Any(usr => usr.Email == email))
            {
                status = RegisterCompanyStatus.DuplicateEmail;
                return Tuple.Create((User)null, status);
            }

         
            var companyResult = await companyService.Add(new StructureUnitModel { Name = company });
            if (companyResult.Status != CompanyCreationStatus.Success)
            {
                status = RegisterCompanyStatus.ProviderError;
                return Tuple.Create((User)null, status);
            }

            var newUser = new User
            {
                Id = Guid.NewGuid(),
                UserName = username,
                PasswordHash = hashedPassword,             
                Email = email,
                CreateDate = DateTime.UtcNow,
                LastPasswordChangedDate = DateTime.UtcNow,
                PasswordFailuresSinceLastSuccess = 0,
                LastLoginDate = DateTime.UtcNow,
                LastActivityDate = DateTime.UtcNow,
                LastLockoutDate = DateTime.UtcNow,
                LockoutEnabled = false,
                LastPasswordFailureDate = DateTime.UtcNow,
                SendStatus = SendStatus.None,
                SendCount = 0,
                CompanyId = companyResult.CompanyId              
            };
            //newUser.Claims.Add(new DataModel.Identity.Models.CustomIdentityUserClaim() { ClaimType = "BaseAddress", ClaimValue = baseAddress });          
            var identityResult = await customUserManager.CreateAsync(newUser);
            if (!identityResult.Succeeded)
            {
                status = RegisterCompanyStatus.ProviderError;
                return Tuple.Create((User)null, status);
            }           

            status = RegisterCompanyStatus.Success;

            return Tuple.Create(newUser, status);    
        }

        private RegisterCompanyStatus CheckNewUser(string username,
          string password,
          string email,
          out string hashedPassword)
        {
            var status = RegisterCompanyStatus.Success;
            if (string.IsNullOrEmpty(username))
            {
                status = RegisterCompanyStatus.InvalidUserName;
            }
            if (string.IsNullOrEmpty(password))
            {
                status = RegisterCompanyStatus.InvalidPassword;
            }
            if (string.IsNullOrEmpty(email))
            {
                status = RegisterCompanyStatus.InvalidEmail;
            }
            hashedPassword = this.customUserManager.PasswordHasher.HashPassword(new User(), password);
            if (hashedPassword.Length > 128)
            {
                status = RegisterCompanyStatus.InvalidPassword;
            }
            return status;
        }

        public async Task<User> GetUserAsync(string username)
        {
            return await customUserManager.FindByNameAsync(username);
        }

       

        public async Task<bool> VerifyAsync(string userId, string code)
        {
            IdentityResult result;
            try
            {
                var user = await customUserManager.Users.SingleOrDefaultAsync(u => u.Id == Guid.Parse(userId));
                result = await customUserManager.ConfirmEmailAsync(user, code);
            }
            catch(Exception e)
            {
                log.LogError(0, e, e.Message);
                return false;
            }

            if (!result.Succeeded)
            {
                log.LogWarning(
                    $"Email verification failed: userId:{userId} code: {code} errors: {string.Join(";", result.Errors.Select(er => $"{er.Code} {er.Description}"))}");
            }
            return (result.Succeeded);
        }

        public async Task<ChangePasswordResponse> ChangePassword(string userId, string currentPassword, string newPassword)
        {
            var user = await customUserManager.FindByIdAsync(userId);
            var identityResult = await customUserManager.ChangePasswordAsync(user, currentPassword, newPassword);
            return new ChangePasswordResponse()
                {Success = identityResult.Succeeded, Errors = identityResult.Errors.Select(e => e.Description)};
        }
    }
}