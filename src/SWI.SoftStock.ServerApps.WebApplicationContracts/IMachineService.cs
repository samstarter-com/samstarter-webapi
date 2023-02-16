using SWI.SoftStock.ServerApps.WebApplicationContracts.MachineService.GetByObservableId;
using SWI.SoftStock.ServerApps.WebApplicationContracts.MachineService.GetBySoftwareId;
using SWI.SoftStock.ServerApps.WebApplicationContracts.MachineService.GetByStructureUnitId;
using SWI.SoftStock.ServerApps.WebApplicationContracts.MachineService.GetStructureUnitId;
using SWI.SoftStock.ServerApps.WebApplicationContracts.Statuses;
using SWI.SoftStock.ServerApps.WebApplicationModel;
using System;
using System.Threading.Tasks;

namespace SWI.SoftStock.ServerApps.WebApplicationContracts
{
    public interface IMachineService
    {
        #region machine query

        Task<MachineModelEx> GetByIdAsync(Guid uniqueId);
        Task<GetByStructureUnitIdResponse> GetByStructureUnitIdAsync(GetByStructureUnitIdRequest request);
        Task<GetBySoftwareIdResponse> GetBySoftwareIdAsync(GetBySoftwareIdRequest request);
        Task<GetStructureUnitIdResponse> GetStructureUnitId(GetStructureUnitIdRequest request);
        Task<GetByObservableIdResponse> GetByObservableId(GetByObservableIdRequest request);

        #endregion

        #region machine command

        Task<MachineLinkToStructureUnitStatus> LinkToStructureUnitAsync(Guid machineId, Guid structureUnitId);
        Task<MachineLinkToUserStatus> LinkToUserAsync(Guid machineId, Guid userId);
        Task<MachineDeleteStatus> Delete(Guid machineId);
        Task<MachineDisableStatus> Disable(Guid machineId);
        Task<MachineEnableStatus> Enable(Guid machineId);

        #endregion
    }
}