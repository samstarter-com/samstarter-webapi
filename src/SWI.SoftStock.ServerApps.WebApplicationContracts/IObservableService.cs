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

		GetAllResponse GetAll(GetAllRequest request);

		#endregion

		#region observable  caommand

		Task<Tuple<Guid?, ObservableCreationStatus>> Add(ObservableModelEx modelEx, Guid companyId, Guid createdByUserId);

        Task<Tuple<Guid?, ObservableAppendStatus>> Append(Guid observableId, Guid machineId);

		Task<ObservableRemoveStatus> Remove(Guid machineId, Guid observableId);

		Task<ObservableDeleteStatus> Delete(Guid observableId);

		#endregion
	}
}