using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SWI.SoftStock.ServerApps.DataAccess2;
using SWI.SoftStock.ServerApps.DataModel2;
using SWI.SoftStock.ServerApps.WebApplicationContracts;
using SWI.SoftStock.ServerApps.WebApplicationContracts.Statuses;
using SWI.SoftStock.ServerApps.WebApplicationContracts.StructureUnitService.CreateAndAdd;
using SWI.SoftStock.ServerApps.WebApplicationContracts.StructureUnitService.GetCompanyIdByName;
using SWI.SoftStock.ServerApps.WebApplicationContracts.StructureUnitService.GetCompanyIdByStructureUnitId;
using SWI.SoftStock.ServerApps.WebApplicationContracts.StructureUnitService.GetStructureUnitModels;
using SWI.SoftStock.ServerApps.WebApplicationModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SWI.SoftStock.ServerApps.WebApplicationServices
{
    public class StructureUnitService : IStructureUnitService
    {
        private readonly ILogger<StructureUnitService> log;
        private readonly IDbContextFactory<MainDbContext> dbFactory;

        public StructureUnitService(ILogger<StructureUnitService> log, IDbContextFactory<MainDbContext> dbFactory)
        {
            this.log = log;
            this.dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
        }

        #region IStructureUnitService Members

        public GetCompanyIdByStructureUnitIdResponse GetCompanyIdByStructureUnitId(Guid structureUnitId)
        {
            using var dbContext = dbFactory.CreateDbContext();
            var company = dbContext.StructureUnits.
                                Include(su => su.ParentStructureUnit)
                                .Single(c => c.UniqueId == structureUnitId)
                                .Ancestors(true, su => su.ParentStructureUnit)
                                .Single(psu => psu.UnitType == UnitType.Company);
            return new GetCompanyIdByStructureUnitIdResponse { CompanyId = company.Id, CompanyUniqueId = company.UniqueId };
        }

        public async Task<StructureUnitModel> GetByUniqueId(Guid uniqueId)
        {
            var dbContext = dbFactory.CreateDbContext();

            using IUnitOfWork unitOfWork = new UnitOfWork(dbContext);
            var structureUnit = await
                unitOfWork.StructureUnitRepository.GetAll().SingleOrDefaultAsync(su => su.UniqueId == uniqueId);

            return structureUnit != null
                ? MapperFromModelToView.MapToStructureModel(structureUnit)
                : null;
        }

        public async Task<StructureUnitDeleteStatus> DeleteByUniqueId(Guid uniqueId)
        {
            var dbContext = dbFactory.CreateDbContext();
            using IUnitOfWork unitOfWork = new UnitOfWork(dbContext);
            StructureUnit structureUnit = await
                unitOfWork.StructureUnitRepository.GetAll().SingleOrDefaultAsync(
                    su => su.UniqueId == uniqueId);

            StructureUnitDeleteStatus status = CheckBeforeDelete(structureUnit);
            if ((int)status > (int)StructureUnitDeleteStatus.None)
            {
                return status;
            }
            try
            {
                unitOfWork.StructureUnitRepository.Delete(structureUnit);
                await unitOfWork.SaveAsync();
            }
            catch (Exception e)
            {
                log.LogError(0, e, e.Message);
                return StructureUnitDeleteStatus.UnknownError;
            }

            return status;
        }

        public async Task<Guid?> GetParentUniqueId(Guid uniqueId)
        {
            Guid? result = null;
            var dbContext = dbFactory.CreateDbContext();
            using (IUnitOfWork unitOfWork = new UnitOfWork(dbContext))
            {
                StructureUnit structureUnit = await
                    unitOfWork.StructureUnitRepository.GetAll().Include(su => su.ParentStructureUnit).SingleOrDefaultAsync(
                        su => su.UniqueId == uniqueId);
                if (structureUnit != null && structureUnit.ParentStructureUnit != null)
                {
                    result = structureUnit.ParentStructureUnit.UniqueId;
                }
            }
            return result;
        }

        /// <summary>
        ///     Create a StructureUnit instance from a web model
        /// </summary>
        /// <param name="model"></param>
        /// <param name="parentId"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public async Task<CreateAndAddResponse> CreateAndAdd(StructureUnitModel model, Guid parentId)
        {
            StructureUnitCreationStatus status;
            try
            {
                var res = await Create(model, parentId);
                StructureUnit structureUnit = res.Item1;
                if (res.Item2 == StructureUnitCreationStatus.Success)
                {
                    await Add(structureUnit);
                    return new CreateAndAddResponse { StructureUnitId = structureUnit.UniqueId, Status = res.Item2 };
                }
                return new CreateAndAddResponse { Status = res.Item2 };
            }
            catch (Exception e)
            {
                log.LogError("Error in StructureUnitService.CreateAndAdd:{0}", e);
                status = StructureUnitCreationStatus.RunTime;
                return new CreateAndAddResponse { Status = status };
            }
        }

        public async Task<StructureUnitUpdateStatus> Update(StructureUnitModel newStructureUnit)
        {
            var dbContext = dbFactory.CreateDbContext();
            using IUnitOfWork unitOfWork = new UnitOfWork(dbContext);
            var existingStructureUnit = await
                unitOfWork.StructureUnitRepository.GetAll().SingleOrDefaultAsync(su => su.UniqueId == newStructureUnit.UniqueId);
            if (existingStructureUnit == null)
            {
                return StructureUnitUpdateStatus.NotExist;
            }
            if (existingStructureUnit.UniqueId == newStructureUnit.ParentUniqueId)
            {
                return StructureUnitUpdateStatus.ParentStructureUnitIsSame;
            }
            if (!newStructureUnit.Equals(existingStructureUnit))
            {
                var isMovedToChildNode = false;
                StructureUnit child = null;
                existingStructureUnit.Name = newStructureUnit.Name.Trim();
                existingStructureUnit.ShortName = newStructureUnit.ShortName.Trim();

                if (newStructureUnit.ParentUniqueId.HasValue)
                {
                    var newParentStructureUnit = await unitOfWork.StructureUnitRepository.GetAll()
                        .SingleAsync(su => su.UniqueId == newStructureUnit.ParentUniqueId);
                    if (existingStructureUnit.StructureUnitId.HasValue &&
                        existingStructureUnit.StructureUnitId.Value != newParentStructureUnit.Id)
                    {
                        // if moved to its own child node
                        var children = newParentStructureUnit.Ancestors(true, su => su.ParentStructureUnit);
                        child = children.SingleOrDefault(p => p.StructureUnitId == existingStructureUnit.Id);
                        if (child != null)
                        {
                            child.StructureUnitId = existingStructureUnit.StructureUnitId;
                            isMovedToChildNode = true;
                        }

                        existingStructureUnit.StructureUnitId = newParentStructureUnit.Id;
                    }
                }

                if (!await IsUnique(existingStructureUnit))
                {
                    return StructureUnitUpdateStatus.NonUnique;
                }

                unitOfWork.StructureUnitRepository.Update(existingStructureUnit, existingStructureUnit.Id);
                if (isMovedToChildNode)
                {
                    unitOfWork.StructureUnitRepository.Update(child, child.Id);
                }
                await unitOfWork.SaveAsync();
            }
            return StructureUnitUpdateStatus.Success;
        }

        public async Task<GetStructureUnitModelsResponse> GetStructureUnitModels(Guid userId,
            Guid? selectedId,
            string[] roles)
        {
            StructureUnitModel selectedStructureUnit;
            IList<StructureUnitTreeItemModel> result = new List<StructureUnitTreeItemModel>();
            var dbContext = dbFactory.CreateDbContext();
            selectedStructureUnit = null;
            using (IUnitOfWork unitOfWork = new UnitOfWork(dbContext))
            {
                var surs = await unitOfWork.StructureUnitUserRoleRepository.GetAll()
                    .Include(sur => sur.Role)
                    .Include(sur => sur.StructureUnit)
                    .Include(sur => sur.StructureUnit.ChildStructureUnits)
                    .Where(sur => sur.UserUserId == userId && roles.Contains(sur.Role.Name)).Select(
                        sur => sur.StructureUnit).ToArrayAsync();

                foreach (var structureUnit in surs)
                {
                    if (IsExistInTree(structureUnit.UniqueId, result))
                    {
                        continue;
                    }
                    var childStructureUnits =
                        structureUnit.Descendants(sud => sud.ChildStructureUnits).ToList();
                    var r = new StructureUnitTreeItemModel
                    {
                        UniqueId = structureUnit.UniqueId,
                        ShortName = structureUnit.ShortName,
                        State = new StructureUnitTreeItemState(null)
                        {
                            IsSelected = structureUnit.UniqueId == selectedId
                        }
                    };
                    if (r.State.IsSelected)
                    {
                        selectedStructureUnit = MapperFromModelToView.MapToStructureModel(structureUnit);
                    }

                    r.ChildUnits = GetChildUnits(childStructureUnits, selectedId, r, ref selectedStructureUnit);
                    result.Add(r);
                }
                var toRemove = result.Reverse().Where(r => IsExistInTree(r.UniqueId, result)).AsEnumerable();
                foreach (var tr in toRemove)
                {
                    result.Remove(tr);
                }
            }
            return new GetStructureUnitModelsResponse { StructureUnits = result, StructureUnit = selectedStructureUnit };
        }

        public async Task<GetCompanyIdByNameResponse> GetCompanyIdByName(string companyName)
        {
            var dbContext = dbFactory.CreateDbContext();

            using IUnitOfWork unitOfWork = new UnitOfWork(dbContext);
            StructureUnit company = await unitOfWork.StructureUnitRepository.GetAll()
                .SingleAsync(c => c.UnitType == UnitType.Company && c.Name == companyName);
            return new GetCompanyIdByNameResponse { CompanyId = company.Id, CompanyUniqueId = company.UniqueId };
        }

        public async Task<int> GetIdByUniqueId(Guid structureUnitId)
        {
            var dbContext = dbFactory.CreateDbContext();

            using IUnitOfWork unitOfWork = new UnitOfWork(dbContext);
            StructureUnit su = await unitOfWork.StructureUnitRepository.GetAll()
                .SingleAsync(c => c.UniqueId == structureUnitId);
            return su.Id;
        }

        public async Task<IEnumerable<Guid>> GetStructureUnitsGuid(Guid userId, string[] roles)
        {
            IList<Guid> result = new List<Guid>();
            var dbContext = dbFactory.CreateDbContext();
            using (IUnitOfWork unitOfWork = new UnitOfWork(dbContext))
            {
                {
                    var surs = await unitOfWork.StructureUnitUserRoleRepository.GetAll()
                        .Include(sur => sur.Role)
                        .Include(sur => sur.StructureUnit)
                        .Include(sur => sur.StructureUnit.ChildStructureUnits)
                        .Where(sur => sur.UserUserId == userId && roles.Contains(sur.Role.Name)).Select(
                            sur => sur.StructureUnit).ToArrayAsync();
                    foreach (StructureUnit structureUnit in surs)
                    {
                        List<StructureUnit> childStructureUnits = structureUnit.Descendants(sud => sud.ChildStructureUnits).ToList();
                        if (!result.Contains(structureUnit.UniqueId))
                            result.Add(structureUnit.UniqueId);
                        foreach (var childStructureUnit in childStructureUnits)
                        {
                            if (!result.Contains(childStructureUnit.UniqueId))
                                result.Add(childStructureUnit.UniqueId);
                        }

                    }
                }
            }
            return result;
        }

        #endregion

        private bool IsExistInTree(Guid uniqueId, IEnumerable<StructureUnitTreeItemModel> structureUnitTreeItemModels)
        {
            foreach (StructureUnitTreeItemModel su in structureUnitTreeItemModels.SelectMany(suvm => suvm.ChildUnits))
            {
                if (su.UniqueId == uniqueId)
                {
                    return true;
                }
                if (IsExistInTree(uniqueId, su.ChildUnits))
                {
                    return true;
                }
            }
            return false;
        }

        private async Task Add(StructureUnit structureUnit)
        {
            var dbContext = dbFactory.CreateDbContext();

            using IUnitOfWork unitOfWork = new UnitOfWork(dbContext);
            unitOfWork.StructureUnitRepository.Add(structureUnit);
            await unitOfWork.SaveAsync();
        }

        private async Task<Tuple<StructureUnit, StructureUnitCreationStatus>> Create(StructureUnitModel model, Guid parentId)
        {
            StructureUnitCreationStatus status;
            var structureUnit = new StructureUnit
            {
                Name = model.Name,
                ShortName = model.ShortName,
                UnitType = UnitType.StructureUnit,
                IsApproved = true
            };
            status = StructureUnitCreationStatus.Success;
            StructureUnit parent = null;

            var dbContext = dbFactory.CreateDbContext();
            using (IUnitOfWork unitOfWork = new UnitOfWork(dbContext))
            {
                parent = await
                    unitOfWork.StructureUnitRepository.GetAll().SingleOrDefaultAsync(su => su.UniqueId == parentId);
            }

            if (parent != null)
            {
                structureUnit.StructureUnitId = parent.Id;
            }
            else
            {
                status = StructureUnitCreationStatus.ParentNotFound;
            }
            if (!await IsUnique(structureUnit))
            {
                status = StructureUnitCreationStatus.NonUnique;
            }
            return new Tuple<StructureUnit, StructureUnitCreationStatus>(structureUnit, status);
        }

        private IEnumerable<StructureUnitTreeItemModel> GetChildUnits(IEnumerable<StructureUnit> structureUnits,
            Guid? selectedId,
            StructureUnitTreeItemModel parent,
            ref StructureUnitModel selectedStructureUnit)
        {
            IList<StructureUnitTreeItemModel> result = new List<StructureUnitTreeItemModel>();
            foreach (
                StructureUnit s in
                    structureUnits.Where(
                        s => s.ParentStructureUnit != null && s.ParentStructureUnit.UniqueId == parent.UniqueId))
            {
                var su = new StructureUnitTreeItemModel
                {
                    UniqueId = s.UniqueId,
                    Parent = parent,
                    ShortName = s.ShortName,
                    State = new StructureUnitTreeItemState(parent) { IsSelected = s.UniqueId == selectedId }
                };
                if (su.State.IsSelected)
                {
                    selectedStructureUnit = MapperFromModelToView.MapToStructureModel(s);
                }
                su.ChildUnits = GetChildUnits(structureUnits, selectedId, su, ref selectedStructureUnit);
                result.Add(su);
            }
            return result;
        }


        /// <summary>
        ///     Checking uniqueness within the company
        /// </summary>
        /// <param name="structureUnit"></param>
        /// <returns></returns>
        private async Task<bool> IsUnique(StructureUnit structureUnit)
        {
            var dbContext = dbFactory.CreateDbContext();
            using IUnitOfWork unitOfWork = new UnitOfWork(dbContext);
            StructureUnit existStructureUnit = await
                unitOfWork.StructureUnitRepository.GetAll().
                    Include(su => su.ParentStructureUnit).SingleOrDefaultAsync(
                        su => su.Name == structureUnit.Name && su.UniqueId != structureUnit.UniqueId);
            if (existStructureUnit == null)
            {
                return true;
            }
            Guid existCompanyUniqueId =
                existStructureUnit.Ancestors(false, su => su.ParentStructureUnit).Single(
                    psu => psu.UnitType == UnitType.Company).UniqueId;
            StructureUnit parentStructureUnit = await
                unitOfWork.StructureUnitRepository.GetAll().
                    Include(su => su.ParentStructureUnit).SingleAsync(su => su.Id == structureUnit.StructureUnitId);
            Guid parentCompanyUniqueId =
                parentStructureUnit.Ancestors(true, su => su.ParentStructureUnit).Single(
                    psu => psu.UnitType == UnitType.Company).UniqueId;
            return existCompanyUniqueId != parentCompanyUniqueId;
        }

        private static StructureUnitDeleteStatus CheckBeforeDelete(StructureUnit structureUnit)
        {
            var result = StructureUnitDeleteStatus.None;
            if (structureUnit.ChildStructureUnits != null && structureUnit.ChildStructureUnits.Any())
            {
                result = result | StructureUnitDeleteStatus.HasChildStructureUnit;
            }
            if (structureUnit.CurrentLinkedMachines != null && structureUnit.CurrentLinkedMachines.Any())
            {
                result = result | StructureUnitDeleteStatus.HasMachine;
            }
            if (structureUnit.StructureUnitRoles != null && structureUnit.StructureUnitRoles.Any(sur => sur.Role.Name == "User"))
            {
                result = result | StructureUnitDeleteStatus.HasUser;
            }
            return result;
        }
    }
}