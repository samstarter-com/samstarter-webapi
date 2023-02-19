using SWI.SoftStock.ServerApps.WebApplicationContracts.ObservableService.Add;
using SWI.SoftStock.ServerApps.WebApplicationContracts.ObservableService.Append;
using SWI.SoftStock.ServerApps.WebApplicationContracts.ObservableService.GetAll;
using SWI.SoftStock.ServerApps.WebApplicationContracts.Statuses;
using SWI.SoftStock.ServerApps.WebApplicationModel;
using System;
using System.Threading.Tasks;

namespace SWI.SoftStock.ServerApps.WebApplicationContracts
{
	public interface IObservableService
	{
		#region observable  query

		Task<ObservableModelEx> GetObservableModelById(Guid observableId);

		Task<GetAllResponse> GetAll(GetAllRequest request);

		#endregion

		#region observable  caommand

		Task<AddResponse> Add(ObservableModelEx modelEx, Guid companyId, Guid createdByUserId);

        Task<AppendResponse> Append(Guid observableId, Guid machineId);

		Task<ObservableRemoveStatus> Remove(Guid machineId, Guid observableId);

		Task<ObservableDeleteStatus> Delete(Guid observableId);

		#endregion
	}
}