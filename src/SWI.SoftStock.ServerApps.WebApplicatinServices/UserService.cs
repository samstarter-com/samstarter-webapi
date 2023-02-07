using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SWI.SoftStock.ServerApps.DataAccess2;
using SWI.SoftStock.ServerApps.DataModel2;
using SWI.SoftStock.ServerApps.DataModel2.Identity;
using SWI.SoftStock.ServerApps.DataModel2.Identity.Models;
using SWI.SoftStock.ServerApps.WebApplicationContracts.Statuses;
using SWI.SoftStock.ServerApps.WebApplicationContracts.UserService.GetByStructureUnitId;
using SWI.SoftStock.ServerApps.WebApplicationContracts.UserService.SetUsersRoles;
using SWI.SoftStock.ServerApps.WebApplicationContracts.UserService.UserUnLock;
using SWI.SoftStock.ServerApps.WebApplicationModel;
using SWI.SoftStock.ServerApps.WebApplicationModel.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using SWI.SoftStock.ServerApps.WebApplicationContracts;

namespace SWI.SoftStock.ServerApps.WebApplicationServices
{
    public class UserService : IUserService
    {
        private readonly ILogger<UserService> log;
        private readonly CustomRoleManager rolemanager;
        private readonly CustomUserManager customUserManager;
        private readonly MainDbContextFactory dbFactory;

        public UserService(ILogger<UserService> log, CustomRoleManager rolemanager, CustomUserManager customUserManager, MainDbContextFactory dbFactory)
        {
            this.log = log;
            this.rolemanager = rolemanager;
            this.customUserManager = customUserManager;
            this.dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
        }

        #region IUserService Members

        public async Task<IEnumerable<UserModel>> GetForAutocompleteAsync(Guid structureUnitId, string contains)
        {
            contains = contains.ToLower();
            var dbContext = dbFactory.Create();
            using (IUnitOfWork unitOfWork = new UnitOfWork(dbContext))
            {
                var user = await rolemanager.FindByNameAsync("User");
                var su = await unitOfWork.StructureUnitRepository.GetAll().SingleAsync(s => s.UniqueId == structureUnitId);
                var userRoleId = user.Id;
                var structureUnitIds = su.Ancestors(true, su => su.ParentStructureUnit).Select(su => su.Id);

                var query =
                    unitOfWork.StructureUnitUserRoleRepository
                        .Query(suur => suur.RoleRoleId == userRoleId && structureUnitIds.Contains(suur.StructureUnitId))
                        .Select(suur => suur.User)
                        .Where(s => s.EmailConfirmed && !(s.LockoutEnabled && s.LockoutEnd.HasValue && s.LockoutEnd > DateTime.UtcNow) && s.UserName.ToLower().Contains(contains));

                var users = query.OrderBy(u => u.UserName).ToArray();
                return users.Select(u => MapperFromModelToView.MapToUserModel(u, su));
            }
        }

        /// <summary>
        ///     Getting a list of users in the User role in a specific structural unit
        /// </summary>
        /// <returns></returns>
        public async Task<GetByStructureUnitIdResponse> GetByStructureUnitId(GetByStructureUnitIdRequest request)
        {
            var response = new GetByStructureUnitIdResponse();
            response.Model = new UserCollection(request.Ordering.Order, request.Ordering.Sort);
            var dbContext = dbFactory.Create();
            using (IUnitOfWork unitOfWork = new UnitOfWork(dbContext))
            {
                var userRole = await rolemanager.FindByNameAsync("User");
                Guid userRoleId = userRole.Id;
                StructureUnit structureUnit = unitOfWork.StructureUnitRepository.GetAll().Single(s => s.UniqueId == request.StructureUnitId);
                IQueryable<User> query;
                if (!request.IncludeItemsOfSubUnits)
                {
                    int suId = structureUnit.Id;
                    query =
                        unitOfWork.StructureUnitUserRoleRepository.GetAll().Where(
                            suur => suur.RoleRoleId == userRoleId && suur.StructureUnitId == suId).Select(
                                suur => suur.User);
                }
                else
                {
                    IEnumerable<int> structureUnitIds =
                        structureUnit.Descendants(sud => sud.ChildStructureUnits).Select(su => su.Id);
                    query =
                        unitOfWork.StructureUnitUserRoleRepository.Query(suur => suur.RoleRoleId == userRoleId).
                            Where(suur => structureUnitIds.Contains(suur.StructureUnitId)).Select(suur => suur.User);
                }

                int totalRecords = query.Count();
                var keySelector = GetByStructureUnitIdOrderingSelecetor(request.Ordering.Sort);
                IEnumerable<User> users;
                if (string.IsNullOrEmpty(request.Ordering.Order) || request.Ordering.Order.ToLower() != "desc")
                {
                    users = query.OrderBy(keySelector).Skip(request.Paging.PageIndex * request.Paging.PageSize).Take(request.Paging.PageSize);
                }
                else
                {
                    users =
                        query.OrderByDescending(keySelector).Skip(request.Paging.PageIndex * request.Paging.PageSize).Take(request.Paging.PageSize);
                }
                UserModelEx[] items = users.Select(MapperFromModelToView.MapToUserModelEx).ToArray();
                response.Model.Items = items;
                response.Model.TotalRecords = totalRecords;
                response.Status = GetByStructureUnitIdStatus.Success;
                return response;
            }
        }

        public IEnumerable<RoleModel> GetRoles()
        {
            CustomRole[] roles = rolemanager.Roles.Where(r => r.Name != "User").OrderBy(r => r.Name).ToArray();
            return roles.Select(MapperFromModelToView.MapToRoleModel);

        }

        public UserUnLockResponse UnLock(UserUnLockRequest request)
        {
            throw new NotImplementedException();
            //var response = new UserUnLockResponse();
            //var dbContext = dbFactory.Create();
            //using (IUnitOfWork unitOfWork = new UnitOfWork(dbContext))
            //{
            //    var user = customUserManager.Users.SingleOrDefault(u => u.Id == request.UserId);
            //    if (user == null)
            //    {
            //        response.Status = UserUnLockStatus.UserNotExist;
            //        return response;
            //    }
            //    try
            //    {
            //        user.LockoutEndDateUtc = null;
            //        user.PasswordFailuresSinceLastSuccess = 0;

            //        unitOfWork.UserRepository.Update(user, user.Id);
            //        unitOfWork.Save();
            //    }
            //    catch (Exception e)
            //    {
            //        log.Error(e);
            //        response.Status = UserUnLockStatus.UnknownError;
            //        return response;
            //    }

            //    response.Status = UserUnLockStatus.Success;
            //    return response;
            //}
        }

        public IEnumerable<UserRoleModel> GetUserRoles(Guid structureUnitId, Guid userId)
        {
            var result = new List<UserRoleModel>();
            var dbContext = dbFactory.Create();
            using (IUnitOfWork unitOfWork = new UnitOfWork(dbContext))
            {
                IOrderedEnumerable<CustomRole> roles =
                    rolemanager.Roles.Where(r => r.Name != "User").ToArray().
                        OrderBy(r => r.Name);
                Guid[] availableRoles =
                    unitOfWork.StructureUnitUserRoleRepository.GetAll().Where(
                        suur => suur.UserUserId == userId && suur.StructureUnit.UniqueId == structureUnitId).Select(
                            suur => suur.RoleRoleId).ToArray();

                foreach (CustomRole r in roles)
                {
                    var urm = new UserRoleModel();
                    urm.RoleId = r.Id;
                    urm.StructureUnitId = structureUnitId;
                    urm.IsInRole = availableRoles.Contains(r.Id);
                    urm.RoleName = r.Name;
                    result.Add(urm);
                }
            }
            return result;
        }

        public async Task<IEnumerable<StructureUnitRoleSimpleModel>> GetUserStructureUnitRolesAsync(Guid userId)
        {
            var result = new List<StructureUnitRoleSimpleModel>();
            var dbContext = dbFactory.Create();
            using (IUnitOfWork unitOfWork = new UnitOfWork(dbContext))
            {
                var userRoleId = await rolemanager.Roles.Where(r => r.Name == "User").Select(r => r.Id).SingleAsync();

                var availableRoles =
                    unitOfWork.StructureUnitUserRoleRepository.GetAll()
                        .Where(suur => suur.UserUserId == userId
                                       //&& suur.RoleRoleId != userRoleId
                                       )
                        .ToArray();

                foreach (var ar in availableRoles)
                {
                    var urm = new StructureUnitRoleSimpleModel();
                    urm.RoleName = ar.Role.Name;
                    urm.StructureUnitName = ar.StructureUnit.ShortName;
                    urm.StructureUnitId = ar.StructureUnit.UniqueId;
                    result.Add(urm);
                }
            }
            return result;
        }

        public IEnumerable<UserRoleSimpleModel> GetStructureUnitUserRoles(Guid structureUnitId)
        {
            var result = new List<UserRoleSimpleModel>();
            var dbContext = dbFactory.Create();
            using (IUnitOfWork unitOfWork = new UnitOfWork(dbContext))
            {
                IEnumerable<Guid> roles =
                    rolemanager.Roles.Where(r => r.Name != "User").Select(r => r.Id).ToArray();

                var structureUnit = unitOfWork.StructureUnitRepository.GetAll().Single(su => su.UniqueId == structureUnitId);
                var structureUnitChains = structureUnit.Ancestors(true, su => su.ParentStructureUnit).Select(su => su.Id);

                var availableRoles =
                    unitOfWork.StructureUnitUserRoleRepository.GetAll()
                        .Where(suur => structureUnitChains.Contains(suur.StructureUnitId))
                        .Where(suur => roles.Contains(suur.RoleRoleId))
                        //.Join(structureUnitChains, suur => suur.StructureUnitId, r => r, (suur, r) => suur)
                        //.Join(roles, suur => suur.RoleRoleId, r => r, (suur, r) => new { Suur = suur })
                        .ToArray();

                result.AddRange(availableRoles.Select(ar => MapperFromModelToView.MapToUserRoleSimpleModel(ar, structureUnitId)));
            }
            return result.OrderByDescending(r => r.IsInherited).ThenBy(r => r.StructureUnitName);
        }

        public async Task<SetUsersRolesResponse> SetUsersRolesAsync(SetUsersRolesRequest request)
        {
            if (request == null)
            {
                return new SetUsersRolesResponse { Status = UserRoleUpdateStatus.Success };
            }

            if (request.Roles == null || request.Roles.Length == 0)
            {
                return new SetUsersRolesResponse { Status = UserRoleUpdateStatus.Success };
            }

            var userRoles =
                request.Roles.Select(
                    r =>
                        new UserRoleModel
                        {
                            IsInRole = r.IsInRole,
                            RoleId = r.RoleId
                        })
                        .Where(r => r.RoleId != Guid.Empty);

            var status = await SetUsersRolesAsync(request.UserId, request.StructureUnitId, userRoles);
            if (status == UserRoleUpdateStatus.Success)
            {
                foreach (var role in request.Roles.Where(r => r.IsInRole))
                {
                    // await customUserManager.AddToRoleAsync(request.UserId, rolemanager.Roles.Single(r => r.Id == role.RoleId).Name);                   
                }
                if (request.Roles.Where(r => !r.IsInRole).Any())
                {
                    var context = dbFactory.Create();
                    using (IUnitOfWork uow = new UnitOfWork(context))
                    {
                        foreach (var role in request.Roles.Where(r => !r.IsInRole))
                        {
                            StructureUnitUserRole[] existUserRoles = uow.StructureUnitUserRoleRepository.GetAll().Where(suur => suur.UserUserId == request.UserId).ToArray();
                            if (!existUserRoles.Any(ur => ur.Role.Name == rolemanager.Roles.Single(r => r.Id == role.RoleId).Name))
                            {

                                //  await customUserManager.RemoveFromRoleAsync(request.UserId, rolemanager.Roles.Single(r => r.Id == role.RoleId).Name);
                            }
                        }
                    }
                }
            }
            return new SetUsersRolesResponse { Status = status };
        }

        public async Task<bool> IsUserInRole(Guid userId, string roleName, Guid structureUnitUniqId)
        {
            var context = dbFactory.Create();
            using (IUnitOfWork uow = new UnitOfWork(context))
            {
                CustomRole role = await rolemanager.FindByNameAsync(roleName);
                var user = customUserManager.Users.SingleOrDefault(u => u.Id == userId);
                //.Include(usr => usr.StructureUnitRoles)
                //.Include(usr => usr.StructureUnitRoles.Select(sur => sur.StructureUnit))

                var userStructureUnitIds = user.StructureUnitRoles.Where(sur => sur.RoleRoleId == role.Id).Select(sur => sur.StructureUnit.UniqueId);
                StructureUnit structureUnit = uow.StructureUnitRepository.GetAll().Include(usr => usr.ParentStructureUnit).Single(su => su.UniqueId == structureUnitUniqId);
                var structureUnitChains = structureUnit.Ancestors(true, su => su.ParentStructureUnit).Select(su => su.UniqueId);
                return structureUnitChains.Intersect(userStructureUnitIds).Any();
            }
        }

        private async Task<UserRoleUpdateStatus> SetUsersRolesAsync(Guid userId, Guid structureUnitId, IEnumerable<UserRoleModel> userRoles)
        {
            var lastAdminChecked = await CheckLastAdminAsync(userId, structureUnitId, userRoles);
            if (!lastAdminChecked)
            {
                return UserRoleUpdateStatus.IsLastAdmin;
            }
            var context = dbFactory.Create();
            using (IUnitOfWork uow = new UnitOfWork(context))
            {
                var user = customUserManager.Users.SingleOrDefault(u => u.Id == userId);

                if (user == null)
                {
                    return UserRoleUpdateStatus.UserNotExist;
                }
                var existUserRoles =
                    uow.StructureUnitUserRoleRepository.GetAll().Where(suur => suur.UserUserId == userId).ToArray();
                var todeleteList = new List<StructureUnitUserRole>();
                var toaddList = new List<StructureUnitUserRole>();
                foreach (var userRole in userRoles)
                {
                    if (!userRole.IsInRole)
                    {
                        var toDelete = existUserRoles.SingleOrDefault(
                            suur =>
                                suur.RoleRoleId == userRole.RoleId &&
                                suur.StructureUnit.UniqueId == structureUnitId);
                        if (toDelete != null)
                        {
                            todeleteList.Add(toDelete);
                        }
                    }
                    else
                    {
                        if (!existUserRoles.Any(
                            suur =>
                                suur.RoleRoleId == userRole.RoleId &&
                                suur.StructureUnit.UniqueId == structureUnitId))
                        {
                            var structureUnit =
                                uow.StructureUnitRepository.GetAll().SingleOrDefault(
                                    su => su.UniqueId == structureUnitId);
                            if (structureUnit == null)
                            {
                                return UserRoleUpdateStatus.StructureUnitNotExist;
                            }
                            toaddList.Add(new StructureUnitUserRole
                            {
                                StructureUnitId = structureUnit.Id,
                                RoleRoleId = userRole.RoleId,
                                UserUserId = userId
                            });
                        }
                    }
                }
                try
                {
                    uow.StructureUnitUserRoleRepository.DeleteRange(todeleteList.ToArray());
                    uow.StructureUnitUserRoleRepository.AddRange(toaddList.ToArray());
                    uow.Save();
                }
                catch (Exception e)
                {
                    log.LogError(0, e, e.Message);
                    return UserRoleUpdateStatus.RunTime;
                }
            }

            return UserRoleUpdateStatus.Success;
        }

        public UserModelEx GetById(Guid userId)
        {
            var dbContext = dbFactory.Create();
            using (IUnitOfWork unitOfWork = new UnitOfWork(dbContext))
            {
                var user = customUserManager.Users.SingleOrDefault(u => u.Id == userId);
                return user != null
                    ? MapperFromModelToView.MapToUserModelEx(user)
                    : null;
            }
        }

        public async Task<UserUpdateStatus> UpdateAsync(UserModelEx model)
        {
            // todo if the Username or Email has changed, then the user will be sent a notification to the new and old Email about who and what has changed
            var dbContext = dbFactory.Create();
            using (IUnitOfWork unitOfWork = new UnitOfWork(dbContext))
            {

                var user = customUserManager.Users.SingleOrDefault(u => u.Id == model.UserId);
                if (user == null)
                {
                    return UserUpdateStatus.NotExist;
                }
                if (!model.Equals(user))
                {
                    user.UserName = model.UserName.Trim();
                    user.LastName = model.LastName.Trim();
                    user.FirstName = model.FirstName.Trim();
                    user.Email = model.Email.Trim();
                    StructureUnitUserRole structureUnitRole = user.StructureUnitRoles.Single(sur => sur.Role.Name == "User");
                    if (structureUnitRole.StructureUnit.UniqueId != model.StructureUnitId)
                    {
                        structureUnitRole.StructureUnitId = unitOfWork.StructureUnitRepository.GetAll().Single(su => su.UniqueId == model.StructureUnitId).Id;
                        unitOfWork.StructureUnitUserRoleRepository.Update(structureUnitRole, structureUnitRole.Id);
                    }
                    if (customUserManager.Users.Any(usr => usr.UserName == user.UserName && usr.Id != user.Id))
                    //if (unitOfWork.UserRepository.GetAll().Any(usr => usr.UserName == user.UserName && usr.Id != user.Id))
                    {
                        return UserUpdateStatus.NonUnique;
                    }

                    if (customUserManager.Users.Any(usr => usr.Email == user.Email && usr.Id != user.Id))
                    {
                        return UserUpdateStatus.EmailNonUnique;
                    }

                    await customUserManager.UpdateAsync(user);
                    unitOfWork.Save();
                }
                return UserUpdateStatus.Success;
            }
        }

        public async Task<UserDeleteStatus> DeleteById(Guid userId)
        {
            var dbContext = dbFactory.Create();
            using (IUnitOfWork unitOfWork = new UnitOfWork(dbContext))
            {
                var user = customUserManager.Users.SingleOrDefault(u => u.Id == userId);

                UserDeleteStatus status = CheckBeforeDelete(user);
                if (status != UserDeleteStatus.None)
                {
                    return status;
                }
                try
                {
                    await customUserManager.DeleteAsync(user);
                    unitOfWork.Save();
                }
                catch (Exception e)
                {
                    log.LogError(0, e, e.Message);
                    return UserDeleteStatus.UnknownError;
                }

                return status;
            }
        }

        public Guid GetCompanyId(Guid userId)
        {
            var dbContext = dbFactory.Create();
            using (IUnitOfWork unitOfWork = new UnitOfWork(dbContext))
            {
                var user = customUserManager.Users.SingleOrDefault(u => u.Id == userId);
                // todo check that the user user exists, if not then return the status UserNotFound
                return user.Company.UniqueId;
            }
        }

        #endregion

        private Expression<Func<User, object>> GetByStructureUnitIdOrderingSelecetor(string sort)
        {
            Expression<Func<User, object>> keySelector = m => m.UserName;
            SortModel[] sortModels = UserModelEx.GetSorting();
            if (!string.IsNullOrEmpty(sort) && sortModels != null && sortModels.Any())
            {
                SortModel sortModel = MachineModel.GetSortModel(sort);
                if (sortModel == null)
                {
                    return keySelector;
                }
                string orderedPropertyName = sortModel.PropertyName;
                if (orderedPropertyName == Nameof<UserModelEx>.Property(e => e.UserName))
                {
                    keySelector = u => u.UserName;
                }
                if (orderedPropertyName == Nameof<UserModelEx>.Property(e => e.FirstName))
                {
                    keySelector = u => u.FirstName;
                }
                if (orderedPropertyName == Nameof<UserModelEx>.Property(e => e.LastName))
                {
                    keySelector = u => u.LastName;
                }
                if (orderedPropertyName == Nameof<UserModelEx>.Property(e => e.Email))
                {
                    keySelector = u => u.Email;
                }
                if (orderedPropertyName == Nameof<UserModelEx>.Property(e => e.IsApproved))
                {
                    keySelector = u => u.EmailConfirmed;
                }
                //if (orderedPropertyName == Nameof<UserModelEx>.Property(e => e.IsLocked))
                //{
                //    keySelector = u => u.LockoutEnabled && u.LockoutEndDate>DateTime.UtcNow;
                //}
                if (orderedPropertyName == Nameof<UserModelEx>.Property(e => e.StructureUnitName))
                {
                    keySelector = u => u.StructureUnitRoles.Single(sur => sur.Role.Name == "User").StructureUnit.ShortName;
                }
            }
            return keySelector;
        }

        private async Task<bool> CheckLastAdminAsync(Guid userId, Guid structureUnitId, IEnumerable<UserRoleModel> roles)
        {
            var dbContext = dbFactory.Create();
            using (IUnitOfWork unitOfWork = new UnitOfWork(dbContext))
            {
                var role = await rolemanager.FindByNameAsync("Admin");
                Guid adminRoleId = role.Id;
                StructureUnit company = customUserManager.Users.SingleOrDefault(u => u.Id == userId).Company;
                IEnumerable<UserRoleModel> removeAdmin =
                    roles.Where(r => r.RoleId == adminRoleId && !r.IsInRole && structureUnitId == company.UniqueId);
                if (removeAdmin.Any())
                {
                    StructureUnitUserRole[] companyAdmins = company.StructureUnitRoles.Where(sur => sur.RoleRoleId == adminRoleId).ToArray();
                    if (companyAdmins.Count() == 1 && companyAdmins.Single().UserUserId == userId)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private UserDeleteStatus CheckBeforeDelete(User user)
        {
            if (user.StructureUnitRoles.Any(sur => sur.Role.Name != "User"))
            {
                return UserDeleteStatus.IsInRole;
            }
            if (user.Machines.Any())
            {
                return UserDeleteStatus.HasLinkedMachine;
            }
            if (user.LicenseAlertUsers.Any())
            {
                return UserDeleteStatus.HasLicenseAlert;
            }
            return UserDeleteStatus.None;
        }

        public async Task<bool> SetRefreshTokenAsync(User user, RefreshToken refreshToken)
        {
            var dbContext = dbFactory.Create();
            using IUnitOfWork unitOfWork = new UnitOfWork(dbContext);
            unitOfWork.RefreshTokenRepository.DeleteRange(user.RefreshTokens.ToArray());
            unitOfWork.RefreshTokenRepository.Add(refreshToken);
            await unitOfWork.SaveAsync();
            return true;
        }

        public async Task<RefreshToken> GetRefreshTokenAsync(Guid userId)
        {
            var dbContext = dbFactory.Create();
            using IUnitOfWork unitOfWork = new UnitOfWork(dbContext);
            var refreshToken = await unitOfWork.RefreshTokenRepository.GetAll().FirstOrDefaultAsync(rt => rt.UserId == userId);
            return refreshToken;
        }
    }
}