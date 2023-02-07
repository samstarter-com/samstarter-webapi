using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using SWI.SoftStock.ServerApps.DataAccess2;
using SWI.SoftStock.ServerApps.DataModel2;
using SWI.SoftStock.ServerApps.WebApplicationContracts;
using SWI.SoftStock.ServerApps.WebApplicationContracts.Statuses;
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
        private readonly MainDbContextFactory dbFactory;

        public StructureUnitService(ILogger<StructureUnitService> log, MainDbContextFactory dbFactory)
        {
            this.log = log;
            this.dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
        }

        #region IStructureUnitService Members

        public Tuple<int, Guid> GetCompanyIdByStructureUnitId(Guid structureUnitId)
        {
            using var dbContext = dbFactory.Create();
            var company = dbContext.StructureUnits.
                                Include(su => su.ParentStructureUnit)
                                .Single(c => c.UniqueId == structureUnitId)
                                .Ancestors(true, su => su.ParentStructureUnit)
                                .Single(psu => psu.UnitType == UnitType.Company);
            return new Tuple<int, Guid>(company.Id, company.UniqueId);
        }

        public StructureUnitModel GetByUniqueId(Guid uniqueId)
        {
            var dbContext = dbFactory.Create();

            using IUnitOfWork unitOfWork = new UnitOfWork(dbContext);
            var structureUnit =
                unitOfWork.StructureUnitRepository.GetAll().SingleOrDefault(su => su.UniqueId == uniqueId);

            return structureUnit != null
                ? MapperFromModelToView.MapToStructureModel(structureUnit)
                : null;
        }

        public StructureUnitDeleteStatus DeleteByUniqueId(Guid uniqueId)
        {
            var dbContext = dbFactory.Create();
            using IUnitOfWork unitOfWork = new UnitOfWork(dbContext);
            StructureUnit structureUnit =
                unitOfWork.StructureUnitRepository.GetAll().SingleOrDefault(
                    su => su.UniqueId == uniqueId);

            StructureUnitDeleteStatus status = CheckBeforeDelete(structureUnit);
            if ((int)status > (int)StructureUnitDeleteStatus.None)
            {
                return status;
            }
            try
            {
                unitOfWork.StructureUnitRepository.Delete(structureUnit);
                unitOfWork.Save();
            }
            catch (Exception e)
            {
                log.LogError(0, e, e.Message);
                return StructureUnitDeleteStatus.UnknownError;
            }

            return status;
        }

        public Guid? GetParentUniqueId(Guid uniqueId)
        {
            Guid? result = null;
            var dbContext = dbFactory.Create();
            using (IUnitOfWork unitOfWork = new UnitOfWork(dbContext))
            {
                StructureUnit structureUnit =
                    unitOfWork.StructureUnitRepository.GetAll().Include(su => su.ParentStructureUnit).SingleOrDefault(
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
        public Guid? CreateAndAdd(StructureUnitModel model, Guid parentId, out StructureUnitCreationStatus status)
        {
            try
            {
                StructureUnit structureUnit = Create(model, parentId, out status);
                if (status == StructureUnitCreationStatus.Success)
                {
                    Add(structureUnit);
                    return structureUnit.UniqueId;
                }
                return null;
            }
            catch (Exception e)
            {
                log.LogError("Error in StructureUnitService.CreateAndAdd:{0}", e);
                status = StructureUnitCreationStatus.RunTime;
                return null;
            }
        }

        public async Task<StructureUnitUpdateStatus> Update(StructureUnitModel newStructureUnit)
        {
            var dbContext = dbFactory.Create();
            using IUnitOfWork unitOfWork = new UnitOfWork(dbContext);
            var existingStructureUnit =
                unitOfWork.StructureUnitRepository.GetAll().SingleOrDefault(su => su.UniqueId == newStructureUnit.UniqueId);
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
                    var newParentStructureUnit = unitOfWork.StructureUnitRepository.GetAll()
                        .Single(su => su.UniqueId == newStructureUnit.ParentUniqueId);
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

                if (!IsUnique(existingStructureUnit))
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

        public IEnumerable<StructureUnitTreeItemModel> GetStructureUnitModels(Guid userId,
            Guid? selectedId,
            string[] roles,
            out StructureUnitModel
                selectedStructureUnit)
        {
            IList<StructureUnitTreeItemModel> result = new List<StructureUnitTreeItemModel>();
            var dbContext = dbFactory.Create();
            selectedStructureUnit = null;
            using (IUnitOfWork unitOfWork = new UnitOfWork(dbContext))
            {
                var surs = unitOfWork.StructureUnitUserRoleRepository.GetAll()
                    .Include(sur => sur.Role)
                    .Include(sur => sur.StructureUnit)
                    .Include(sur => sur.StructureUnit.ChildStructureUnits)
                    .Where(sur => sur.UserUserId == userId && roles.Contains(sur.Role.Name)).Select(
                        sur => sur.StructureUnit);

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
            return result;
        }

        public Tuple<int, Guid> GetCompanyIdByName(string companyName)
        {
            var dbContext = dbFactory.Create();

            using IUnitOfWork unitOfWork = new UnitOfWork(dbContext);
            StructureUnit company = unitOfWork.StructureUnitRepository.GetAll()
                .Single(c => c.UnitType == UnitType.Company && c.Name == companyName);
            return new Tuple<int, Guid>(company.Id, company.UniqueId);
        }

        public int GetIdByUniqueId(Guid structureUnitId)
        {
            var dbContext = dbFactory.Create();

            using IUnitOfWork unitOfWork = new UnitOfWork(dbContext);
            StructureUnit su = unitOfWork.StructureUnitRepository.GetAll()
                .Single(c => c.UniqueId == structureUnitId);
            return su.Id;
        }

        public IEnumerable<Guid> GetStructureUnitsGuid(Guid userId, string[] roles)
        {
            IList<Guid> result = new List<Guid>();
            var dbContext = dbFactory.Create();
            using (IUnitOfWork unitOfWork = new UnitOfWork(dbContext))
            {
                {
                    IQueryable<StructureUnit> surs = unitOfWork.StructureUnitUserRoleRepository.GetAll()
                        .Include(sur => sur.Role)
                        .Include(sur => sur.StructureUnit)
                        .Include(sur => sur.StructureUnit.ChildStructureUnits)
                        .Where(sur => sur.UserUserId == userId && roles.Contains(sur.Role.Name)).Select(
                            sur => sur.StructureUnit);
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

        private void Add(StructureUnit structureUnit)
        {
            var dbContext = dbFactory.Create();

            using IUnitOfWork unitOfWork = new UnitOfWork(dbContext);
            unitOfWork.StructureUnitRepository.Add(structureUnit);
            unitOfWork.Save();
        }

        private StructureUnit Create(StructureUnitModel model, Guid parentId, out StructureUnitCreationStatus status)
        {
            var structureUnit = new StructureUnit
            {
                Name = model.Name,
                ShortName = model.ShortName,
                UnitType = UnitType.StructureUnit,
                IsApproved = true
            };
            status = StructureUnitCreationStatus.Success;
            StructureUnit parent = null;

            var dbContext = dbFactory.Create();
            using (IUnitOfWork unitOfWork = new UnitOfWork(dbContext))
            {
                parent =
                    unitOfWork.StructureUnitRepository.GetAll().SingleOrDefault(su => su.UniqueId == parentId);
            }

            if (parent != null)
            {
                structureUnit.StructureUnitId = parent.Id;
            }
            else
            {
                status = StructureUnitCreationStatus.ParentNotFound;
            }
            if (!IsUnique(structureUnit))
            {
                status = StructureUnitCreationStatus.NonUnique;
            }
            return structureUnit;
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
        private bool IsUnique(StructureUnit structureUnit)
        {
            var dbContext = dbFactory.Create();
            using IUnitOfWork unitOfWork = new UnitOfWork(dbContext);
            StructureUnit existStructureUnit =
                unitOfWork.StructureUnitRepository.GetAll().
                    Include(su => su.ParentStructureUnit).SingleOrDefault(
                        su => su.Name == structureUnit.Name && su.UniqueId != structureUnit.UniqueId);
            if (existStructureUnit == null)
            {
                return true;
            }
            Guid existCompanyUniqueId =
                existStructureUnit.Ancestors(false, su => su.ParentStructureUnit).Single(
                    psu => psu.UnitType == UnitType.Company).UniqueId;
            StructureUnit parentStructureUnit =
                unitOfWork.StructureUnitRepository.GetAll().
                    Include(su => su.ParentStructureUnit).Single(su => su.Id == structureUnit.StructureUnitId);
            Guid parentCompanyUniqueId =
                parentStructureUnit.Ancestors(true, su => su.ParentStructureUnit).Single(
                    psu => psu.UnitType == UnitType.Company).UniqueId;
            return existCompanyUniqueId != parentCompanyUniqueId;
        }

        private StructureUnitDeleteStatus CheckBeforeDelete(StructureUnit structureUnit)
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