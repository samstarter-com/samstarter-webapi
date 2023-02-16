using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SWI.SoftStock.ServerApps.DataAccess2;
using SWI.SoftStock.ServerApps.DataModel2;
using SWI.SoftStock.ServerApps.DataModel2.Identity;
using SWI.SoftStock.ServerApps.WebApplicationContracts;
using SWI.SoftStock.ServerApps.WebApplicationContracts.MachineService.GetByObservableId;
using SWI.SoftStock.ServerApps.WebApplicationContracts.MachineService.GetBySoftwareId;
using SWI.SoftStock.ServerApps.WebApplicationContracts.MachineService.GetByStructureUnitId;
using SWI.SoftStock.ServerApps.WebApplicationContracts.MachineService.GetStructureUnitId;
using SWI.SoftStock.ServerApps.WebApplicationContracts.Statuses;
using SWI.SoftStock.ServerApps.WebApplicationModel;
using SWI.SoftStock.ServerApps.WebApplicationModel.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace SWI.SoftStock.ServerApps.WebApplicationServices
{
    public class MachineService : IMachineService
    {
        private readonly ILogger<MachineService> log;
        private readonly CustomUserManager customUserManager;
        private readonly IDbContextFactory<MainDbContext> dbFactory;

        public MachineService(ILogger<MachineService> log, CustomUserManager customUserManager, IDbContextFactory<MainDbContext> dbFactory)
        {
            this.log = log;
            this.customUserManager = customUserManager;
            this.dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
        }

        #region IMachineService Members

        public async Task<MachineModelEx> GetByIdAsync(Guid uniqueId)
        {
            var dbContext = dbFactory.CreateDbContext();
            using IUnitOfWork unitOfWork = new UnitOfWork(dbContext);
            Machine machine = await
                unitOfWork.MachineRepository.GetAll()
                    .Include(m => m.CurrentUser)
                    .Include(m => m.CurrentDomainUser)
                    .Include(m => m.CurrentLinkedStructureUnit)
                    .Include(m => m.MachineSoftwaresReadOnly)
                    .Include(m => m.NetworkAdapters)
                    .Include(m => m.MachineObservedProcesses)
                    .Include(m => m.Processor)
                    .Include(m => m.MachineOperationSystem)
                    .ThenInclude(m => m.OperationSystem)
                    .Include(m => m.MachineObservedProcesses)
                    .ThenInclude(mop => mop.Observable)
                    .ThenInclude(o => o.Software)
                    .SingleAsync(m => m.UniqueId == uniqueId);
            return machine != null
                       ? MapperFromModelToView.MapToMachineModelEx(machine)
                       : null;
        }

        /// <summary>
        /// Machine list. If structureUnitId is an company id then all linked and unlinked machines are in the 
        /// </summary>
        /// <returns></returns>
        public async Task<GetByStructureUnitIdResponse> GetByStructureUnitIdAsync(GetByStructureUnitIdRequest request)
        {
            var response = new GetByStructureUnitIdResponse
            {
                Model = new MachineCollection(request.Ordering.Order, request.Ordering.Sort)
            };
            var dbContext = dbFactory.CreateDbContext();
            using IUnitOfWork unitOfWork = new UnitOfWork(dbContext);
            var structureUnit =
                unitOfWork.StructureUnitRepository.Query(s => s.UniqueId == request.StructureUnitId).Single();
            IQueryable<Machine> query;
            Expression<Func<Machine, bool>> machineTypeWhere = (m => true);
            switch (request.MachineType)
            {
                case MachineFilterType.Disabled:
                    machineTypeWhere = (m => m.IsDisabled);
                    break;
                case MachineFilterType.Enabled:
                    machineTypeWhere = (m => !m.IsDisabled);
                    break;
                case MachineFilterType.Disabled | MachineFilterType.Enabled:
                    machineTypeWhere = (m => true);
                    break;
                default:
                    response.Status = GetByStructureUnitIdStatus.Success;
                    return response;
            }

            if (!request.IncludeItemsOfSubUnits)
            {
                query =
                    unitOfWork.MachineRepository.Query(machineTypeWhere).Where(
                        m => m.CurrentLinkedStructureUnitId == structureUnit.Id);
            }
            else
            {
                var structureUnitIds =
                    structureUnit.Descendants(sud => sud.ChildStructureUnits).Select(su => su.Id);
                query = unitOfWork.MachineRepository.Query(machineTypeWhere)
                    .Where(m => structureUnitIds.Contains(m.CurrentLinkedStructureUnitId));
            }

            var totalRecords = query.Count();
            query = query.Include(m => m.MachineOperationSystem)
                .Include(m => m.MachineOperationSystem.OperationSystem)
                .Include(m => m.MachineSoftwaresReadOnly)
                .Include(m => m.CurrentDomainUser)
                .Include(m => m.CurrentUser);
            var keySelector = this.GetByStructureUnitIdMachineOrderingSelector(request.Ordering.Sort);
            if (string.IsNullOrEmpty(request.Ordering.Order) || request.Ordering.Order.ToLower() != "desc")
            {
                query = query.OrderBy(keySelector);
            }
            else
            {
                query =
                    query.OrderByDescending(keySelector);
            }

            var machines = query.Skip(request.Paging.PageIndex * request.Paging.PageSize).Take(request.Paging.PageSize);
            var items = await machines.ToArrayAsync();

            response.Model.Items = items.Select(MapperFromModelToView.MapToMachineModel<MachineModel>);
            response.Model.TotalRecords = totalRecords;
            response.Status = GetByStructureUnitIdStatus.Success;
            return response;
        }

        public async Task<MachineLinkToStructureUnitStatus> LinkToStructureUnitAsync(Guid machineId, Guid structureUnitId)
        {
            var dbContext = dbFactory.CreateDbContext();
            using (IUnitOfWork unitOfWork = new UnitOfWork(dbContext))
            {
                var machine = await unitOfWork.MachineRepository.GetAll().SingleAsync(m => m.UniqueId == machineId);
                if (machine.CurrentLinkedStructureUnit != null &&
                    machine.CurrentLinkedStructureUnit.UniqueId == structureUnitId)
                {
                    // if the machine is already attached to this structural unit, then do nothing
                    return MachineLinkToStructureUnitStatus.Success;
                }
                machine.CurrentLinkedStructureUnit =
                    unitOfWork.StructureUnitRepository.Query(su => su.UniqueId == structureUnitId).Single();

                var machineStructureUnit = new MachineStructureUnit
                {
                    LinkDateTime = DateTime.UtcNow,
                    MachineId = machine.Id,
                    StructureUnitId = machine.CurrentLinkedStructureUnit.UniqueId
                };

                unitOfWork.MachineStructureUnitRepository.Add(machineStructureUnit);
                unitOfWork.MachineRepository.Update(machine, machine.Id);
                await unitOfWork.SaveAsync();
            }
            return MachineLinkToStructureUnitStatus.Success;
        }

        public async Task<MachineLinkToUserStatus> LinkToUserAsync(Guid machineId, Guid userId)
        {
            var dbContext = dbFactory.CreateDbContext();
            using (IUnitOfWork unitOfWork = new UnitOfWork(dbContext))
            {
                var machine = await unitOfWork.MachineRepository.GetAll().SingleAsync(m => m.UniqueId == machineId);
                if (machine.CurrentUser != null && machine.CurrentUser.Id == userId)
                {
                    // if machine is already linked to a user
                    return MachineLinkToUserStatus.Success;
                }
                
                machine.CurrentUserId = userId;
                var machineUser = new MachineUser
                {
                    LinkDateTime = DateTime.UtcNow,
                    MachineId = machine.Id,
                    UserUserId = userId
                };

                unitOfWork.MachineUserRepository.Add(machineUser);
                unitOfWork.MachineRepository.Update(machine, machine.Id);
                await unitOfWork.SaveAsync();
            }
            return MachineLinkToUserStatus.Success;
        }

        public async Task<GetBySoftwareIdResponse> GetBySoftwareIdAsync(GetBySoftwareIdRequest request)
        {
            GetBySoftwareIdResponse response = new GetBySoftwareIdResponse();

            var licenseFilterType = (LicenseFilterType)request.FilterType;
            response.Model = new SoftwareMachineCollection(request.Ordering.Order, request.Ordering.Sort);
            var dbContext = dbFactory.CreateDbContext();
            using IUnitOfWork unitOfWork = new UnitOfWork(dbContext);
            var software = await unitOfWork.SoftwareRepository.GetAll().SingleOrDefaultAsync(s => s.UniqueId == request.SoftwareId);
            if (software == null)
            {
                response.Status = GetBySoftwareIdStatus.SoftwareNotFound;
                return response;
            }
            response.Model.SoftwareName = !string.IsNullOrEmpty(software.Name) ? software.Name : "No name";

            Expression<Func<MachineSoftware, bool>> filter = ms => true;
            switch (licenseFilterType)
            {
                case (LicenseFilterType.Licensed | LicenseFilterType.Unlicensed):
                    filter = ms => true;
                    break;
                case LicenseFilterType.Licensed:
                    filter =
                        ms => ms.LicenseMachineSoftwares.Any(lms => !lms.IsDeleted);
                    break;
                case LicenseFilterType.Unlicensed:
                    filter =
                        ms =>
                            ms.LicenseMachineSoftwares.Count == 0 ||
                            ms.LicenseMachineSoftwares.All(lms => lms.IsDeleted);
                    break;
            }

            var query = unitOfWork.SoftwareRepository.GetAll()
                .Where(s => s.UniqueId == request.SoftwareId)
                .SelectMany(s => s.MachineSoftwares)
                .Where(filter)
                .Distinct()
                .Where(ms =>
                    !ms.Machine.IsDisabled &&
                    request.SuIds.Contains(ms.Machine.CurrentLinkedStructureUnit.UniqueId));

            int totalRecords = await query.CountAsync();
            var keySelector = this.GetBySoftwareIdOrderingSelecetor(request.Ordering.Sort);

            var machines = (string.IsNullOrEmpty(request.Ordering.Order) || request.Ordering.Order.ToLower() != "desc") ?
                 query.OrderBy(keySelector).Skip(request.Paging.PageIndex * request.Paging.PageSize).Take(request.Paging.PageSize) :
                 query.OrderByDescending(keySelector).Skip(request.Paging.PageIndex * request.Paging.PageSize).Take(request.Paging.PageSize);

            var items = await machines.Select(ms => MapperFromModelToView.MapToInstalledSoftwareMachineModel(ms)).ToArrayAsync();

            response.Model.Items = items;
            response.Model.TotalRecords = totalRecords;
            response.Status = GetBySoftwareIdStatus.Success;
            return response;
        }

        public async Task<GetByObservableIdResponse> GetByObservableId(GetByObservableIdRequest request)
        {
            GetByObservableIdResponse response = new GetByObservableIdResponse
            {
                Model = new MachineCollection(request.Ordering.Order, request.Ordering.Sort)
            };
            var dbContext = dbFactory.CreateDbContext();
            using IUnitOfWork unitOfWork = new UnitOfWork(dbContext);
            var observable = await
                unitOfWork.ObservableRepository.GetAll().SingleAsync(m => m.UniqueId == request.ObservableId);
            var query = unitOfWork.MachineObservedProcessRepository.GetAll().Where(mop => mop.ObservableId == observable.Id).
                Select(ms => ms.Machine);
            var totalRecords = await query.CountAsync();
            var keySelector = GetByStructureUnitIdMachineOrderingSelector(request.Ordering.Sort);
            IQueryable<Machine> machines;
            if (string.IsNullOrEmpty(request.Ordering.Order) || request.Ordering.Order.ToLower() != "desc")
            {
                machines = query.OrderBy(keySelector).Skip(request.Paging.PageIndex * request.Paging.PageSize).Take(request.Paging.PageSize);
            }
            else
            {
                machines =
                    query.OrderByDescending(keySelector).Skip(request.Paging.PageIndex * request.Paging.PageSize).Take(request.Paging.PageSize);
            }

            MachineModel[] items = (await machines.ToArrayAsync()).Select(MapperFromModelToView.MapToMachineModel<MachineModel>).ToArray();
            response.Model.Items = items;
            response.Model.TotalRecords = totalRecords;
            response.Status = GetByObservableIdStatus.Success;
            return response;
        }

        public async Task<MachineDeleteStatus> Delete(Guid machineId)
        {
            var dbContext = dbFactory.CreateDbContext();
            using (IUnitOfWork unitOfWork = new UnitOfWork(dbContext))
            {
                Machine machine = await unitOfWork.MachineRepository.GetAll().SingleAsync(m => m.UniqueId == machineId);
                if (machine == null)
                {
                    return MachineDeleteStatus.NotExist;
                }
                unitOfWork.MachineRepository.Delete(machine);
                var deletedMachine = new DeletedMachine() { UniqueId = machine.UniqueId, DeletedOn = DateTime.UtcNow };
                unitOfWork.DeletedMachineRepository.Add(deletedMachine);
                unitOfWork.Save();
            }
            return MachineDeleteStatus.Success;
        }

        public async Task<GetStructureUnitIdResponse> GetStructureUnitId(GetStructureUnitIdRequest request)
        {
            GetStructureUnitIdResponse response = new GetStructureUnitIdResponse();
            var dbContext = dbFactory.CreateDbContext();
            using IUnitOfWork unitOfWork = new UnitOfWork(dbContext);
            Machine machine = await
                unitOfWork.MachineRepository.GetAll().SingleOrDefaultAsync(m => m.UniqueId == request.MachineId);
            if (machine == null)
            {
                response.Status = GetStructureUnitIdStatus.MachineNotFound;
                return response;
            }
            if (machine.IsDisabled)
            {
                response.Status = GetStructureUnitIdStatus.MachineNotFound;
                return response;
            }
            response.StructureUnitId = machine.CurrentLinkedStructureUnit.UniqueId;
            response.Status = GetStructureUnitIdStatus.Success;
            return response;
        }

        public async Task<MachineDisableStatus> Disable(Guid machineId)
        {
            var dbContext = dbFactory.CreateDbContext();
            using (IUnitOfWork unitOfWork = new UnitOfWork(dbContext))
            {
                Machine machine = await unitOfWork.MachineRepository.GetAll().SingleAsync(m => m.UniqueId == machineId);
                if (machine == null)
                {
                    return MachineDisableStatus.NotExist;
                }
                machine.IsDisabled = true;
                unitOfWork.MachineRepository.Update(machine, machine.Id);
                unitOfWork.Save();
            }
            return MachineDisableStatus.Success;
        }

        public async Task<MachineEnableStatus> Enable(Guid machineId)
        {
            var dbContext = dbFactory.CreateDbContext();
            using (IUnitOfWork unitOfWork = new UnitOfWork(dbContext))
            {
                Machine machine = await unitOfWork.MachineRepository.GetAll().SingleAsync(m => m.UniqueId == machineId);
                if (machine == null)
                {
                    return MachineEnableStatus.NotExist;
                }
                machine.IsDisabled = false;
                unitOfWork.MachineRepository.Update(machine, machine.Id);
                unitOfWork.Save();
            }
            return MachineEnableStatus.Success;
        }

        #endregion

        private Expression<Func<MachineSoftware, object>> GetBySoftwareIdOrderingSelecetor(string sort)
        {
            Expression<Func<MachineSoftware, object>> keySelector = (m) => m.Machine.Name;
            SortModel[] sortModels = MachineModel.GetSorting();
            if (!string.IsNullOrEmpty(sort) && sortModels != null && sortModels.Any())
            {
                SortModel sortModel = MachineModel.GetSortModel(sort);
                if (sortModel == null)
                {
                    return keySelector;
                }
                string orderedPropertyName = sortModel.PropertyName;
                if (orderedPropertyName == Nameof<MachineModel>.Property(e => e.Name))
                {
                    keySelector = (u) => u.Machine.Name;
                }

                if (orderedPropertyName == Nameof<MachineModel>.Property(e => e.StructureUnitName))
                {
                    keySelector = (u) => u.Machine.CurrentLinkedStructureUnit.Name;
                }
                if (orderedPropertyName == Nameof<MachineModel>.Property(e => e.DomainUserName))
                {
                    keySelector = (u) => u.Machine.CurrentDomainUser != null ? u.Machine.CurrentDomainUser.Name : null;
                }
                if (orderedPropertyName == Nameof<MachineModel>.Property(e => e.DomainUserDomainName))
                {
                    keySelector = (u) => u.Machine.CurrentDomainUser != null ? u.Machine.CurrentDomainUser.DomainName : null;
                }
                if (orderedPropertyName == Nameof<MachineModel>.Property(e => e.LinkedUserName))
                {
                    keySelector = (u) => u.Machine.CurrentUser != null ? u.Machine.CurrentUser.UserName : null;
                }
            }
            return keySelector;
        }

        private Expression<Func<Machine, object>> GetByStructureUnitIdMachineOrderingSelector(string sort)
        {
            Expression<Func<Machine, object>> keySelector = (m) => m.Name;
            SortModel[] sortModels = MachineModel.GetSorting();
            if (!string.IsNullOrEmpty(sort) && sortModels != null && sortModels.Any())
            {
                SortModel sortModel = MachineModel.GetSortModel(sort);
                if (sortModel == null)
                {
                    return keySelector;
                }
                string orderedPropertyName = sortModel.PropertyName;
                if (orderedPropertyName == Nameof<MachineModel>.Property(e => e.Name))
                {
                    keySelector = (u) => u.Name;
                }
                if (orderedPropertyName == Nameof<MachineModel>.Property(e => e.Enabled))
                {
                    keySelector = (u) => u.IsDisabled;
                }
                if (orderedPropertyName == Nameof<MachineModel>.Property(e => e.CreatedOn))
                {
                    keySelector = (u) => u.CreatedOn;
                }
                if (orderedPropertyName == Nameof<MachineModel>.Property(e => e.StructureUnitName))
                {
                    keySelector = (u) => u.CurrentLinkedStructureUnit.Name;
                }
                if (orderedPropertyName == Nameof<MachineModel>.Property(e => e.DomainUserName))
                {
                    keySelector = (u) => u.CurrentDomainUser != null ? u.CurrentDomainUser.Name : null;
                }
                if (orderedPropertyName == Nameof<MachineModel>.Property(e => e.DomainUserDomainName))
                {
                    keySelector = (u) => u.CurrentDomainUser != null ? u.CurrentDomainUser.DomainName : null;
                }
                if (orderedPropertyName == Nameof<MachineModel>.Property(e => e.LinkedUserName))
                {
                    keySelector = (u) => u.CurrentUser != null ? u.CurrentUser.UserName : null;
                }
                if (orderedPropertyName == Nameof<MachineModel>.Property(e => e.LastActivity))
                {
                    keySelector = (u) => u.LastActivityDateTime;
                }
                if (orderedPropertyName == Nameof<MachineModel>.Property(e => e.TotalSoftwareCount))
                {
                    keySelector = (u) => u.MachineSoftwaresReadOnly.SoftwaresTotalCount;
                }
                if (orderedPropertyName == Nameof<MachineModel>.Property(e => e.OperationSystemName))
                {
                    keySelector =
                        (u) =>
                        (u.MachineOperationSystem != null && u.MachineOperationSystem.OperationSystem != null)
                            ? u.MachineOperationSystem.OperationSystem.Name
                            : null;
                }
                if (orderedPropertyName == Nameof<MachineModel>.Property(e => e.LicensedSoftwareCount))
                {
                    keySelector = (u) => u.MachineSoftwaresReadOnly.SoftwaresIsActiveCount;
                }
                if (orderedPropertyName == Nameof<MachineModel>.Property(e => e.UnLicensedSoftwareCount))
                {
                    keySelector = (u) => u.MachineSoftwaresReadOnly.SoftwaresUnlicensedCount;
                }
                if (orderedPropertyName == Nameof<MachineModel>.Property(e => e.ExpiredLicensedSoftwareCount))
                {
                    keySelector = (u) => u.MachineSoftwaresReadOnly.SoftwaresIsExpiredCount;
                }
            }
            return keySelector;
        }
    }
}