using System;
using SWI.SoftStock.ServerApps.WebApplicationContracts.ObservableService.GetAll;
using SWI.SoftStock.ServerApps.WebApplicationContracts.Statuses;
using SWI.SoftStock.ServerApps.WebApplicationModel;

namespace SWI.SoftStock.ServerApps.WebApplicationContracts
{
	public interface IObservableService
	{
		#region observable  query

		ObservableModelEx GetObservableModelById(Guid observableId);

		GetAllResponse GetAll(GetAllRequest request);

		#endregion

		#region observable  caommand

		Guid? Add(ObservableModelEx modelEx, Guid companyId, Guid createdByUserId, out ObservableCreationStatus status);

		Guid? Append(Guid observableId, Guid machineId, out ObservableAppendStatus status);

		ObservableRemoveStatus Remove(Guid machineId, Guid observableId);

		ObservableDeleteStatus Delete(Guid observableId);

		#endregion
	}
}