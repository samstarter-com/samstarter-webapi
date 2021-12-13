using SWI.SoftStock.ServerApps.WebApplicationContracts.PersonalMachineService.GetByUserId;
using System.Threading.Tasks;

namespace SWI.SoftStock.ServerApps.WebApplicationContracts
{
    public interface IPersonalMachineService
	{
		Task<GetByUserIdResponse> GetByUserIdAsync(GetByUserIdRequest request);
	}
}