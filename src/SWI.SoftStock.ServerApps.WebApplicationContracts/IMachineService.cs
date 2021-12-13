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
        GetStructureUnitIdResponse GetStructureUnitId(GetStructureUnitIdRequest request);
        GetByObservableIdResponse GetByObservableId(GetByObservableIdRequest request);

        #endregion

        #region machine command

        Task<MachineLinkToStructureUnitStatus> LinkToStructureUnitAsync(Guid machineId, Guid structureUnitId);
        Task<MachineLinkToUserStatus> LinkToUserAsync(Guid machineId, Guid userId);
        MachineDeleteStatus Delete(Guid machineId);
        MachineDisableStatus Disable(Guid machineId);
        MachineEnableStatus Enable(Guid machineId);

        #endregion
    }
}