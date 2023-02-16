using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SWI.SoftStock.ServerApps.DataModel2;
using SWI.SoftStock.ServerApps.WebApplicationContracts.Statuses;
using SWI.SoftStock.ServerApps.WebApplicationContracts.UserService.GetByStructureUnitId;
using SWI.SoftStock.ServerApps.WebApplicationContracts.UserService.SetUsersRoles;
using SWI.SoftStock.ServerApps.WebApplicationContracts.UserService.UserUnLock;
using SWI.SoftStock.ServerApps.WebApplicationModel;

namespace SWI.SoftStock.ServerApps.WebApplicationContracts
{
    public interface IUserService
	{
		#region user query

		UserModelEx GetById(Guid userId);

        Task<IEnumerable<UserRoleModel>> GetUserRoles(Guid structureUnitId, Guid userId);

		Task<IEnumerable<StructureUnitRoleSimpleModel>> GetUserStructureUnitRolesAsync(Guid userId);

        Task<IEnumerable<UserRoleSimpleModel>> GetStructureUnitUserRoles(Guid structureUnitId);

        Task<IEnumerable<UserModel>> GetForAutocompleteAsync(Guid value, string request);

		Guid GetCompanyId(Guid userId);

        Task<GetByStructureUnitIdResponse> GetByStructureUnitId(GetByStructureUnitIdRequest request);

		IEnumerable<RoleModel> GetRoles();

        #endregion

        #region user command
               
        Task<SetUsersRolesResponse> SetUsersRolesAsync(SetUsersRolesRequest request);

        Task<UserUpdateStatus> UpdateAsync(UserModelEx model);

        Task<UserDeleteStatus> DeleteById(Guid userId);

		UserUnLockResponse UnLock(UserUnLockRequest request);

        #endregion

        Task<bool> SetRefreshTokenAsync(User user, RefreshToken refreshToken);

        Task<RefreshToken> GetRefreshTokenAsync(Guid userId);

        Task<bool> IsUserInRole(Guid username, string roleName, Guid structureUnitUniqId);
    }
}