using Microsoft.EntityFrameworkCore;
using SWI.SoftStock.ServerApps.DataAccess2;
using SWI.SoftStock.ServerApps.DataModel2;
using SWI.SoftStock.ServerApps.WebApplicationContracts;
using SWI.SoftStock.ServerApps.WebApplicationContracts.SoftwareService.GetById;
using SWI.SoftStock.ServerApps.WebApplicationContracts.SoftwareService.GetByStructureUnitId;
using SWI.SoftStock.ServerApps.WebApplicationContracts.SoftwareService.GetSoftwaresByMachineId;
using SWI.SoftStock.ServerApps.WebApplicationModel;
using SWI.SoftStock.ServerApps.WebApplicationModel.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace SWI.SoftStock.ServerApps.WebApplicationServices
{
    public class SoftwareService : ISoftwareService
    {
        private readonly MainDbContextFactory dbFactory;

        public SoftwareService(MainDbContextFactory dbFactory)
        {
            this.dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
        }

        private Expression<Func<SoftwareCurrentLinkedStructureUnitReadOnlyGrouped, object>>
            GetByStructureUnitIdOrderingSelecetor(string sort)
        {
            Expression<Func<SoftwareCurrentLinkedStructureUnitReadOnlyGrouped, object>> keySelector =
                u => u.Software.Name;
            var sortModels = SoftwareModel.GetSorting();
            if (!string.IsNullOrEmpty(sort) && sortModels != null && sortModels.Any())
            {
                var sortModel = SoftwareModel.GetSortModel(sort);
                if (sortModel == null)
                {
                    return keySelector;
                }

                var orderedPropertyName = sortModel.PropertyName;
                if (orderedPropertyName == Nameof<SoftwareModel>.Property(e => e.Name))
                {
                    keySelector = u => u.Software.Name;
                }

                if (orderedPropertyName == Nameof<SoftwareModel>.Property(e => e.PublisherName))
                {
                    keySelector =
                        u =>
                            u.PublisherName ?? string.Empty;
                }

                if (orderedPropertyName == Nameof<SoftwareModel>.Property(e => e.Version))
                {
                    keySelector =
                        u =>
                            u.Software.Version;
                }

                if (orderedPropertyName == Nameof<SoftwareModel>.Property(e => e.SystemComponent))
                {
                    keySelector =
                        u =>
                            u.Software.SystemComponent;
                }

                if (orderedPropertyName == Nameof<SoftwareModel>.Property(e => e.WindowsInstaller))
                {
                    keySelector = u => u.Software.WindowsInstaller;
                }

                if (orderedPropertyName == Nameof<SoftwareModel>.Property(e => e.ReleaseType))
                {
                    keySelector = u => u.Software.ReleaseType;
                }

                if (orderedPropertyName == Nameof<SoftwareModel>.Property(e => e.TotalInstallationCount))
                {
                    keySelector = sms => sms.SoftwaresTotalCount;
                }

                if (orderedPropertyName == Nameof<SoftwareModel>.Property(e => e.LicensedInstallationCount))
                {
                    keySelector = sms => sms.SoftwaresIsActiveCount;
                }

                if (orderedPropertyName == Nameof<SoftwareModel>.Property(e => e.UnLicensedInstallationCount))
                {
                    keySelector = sms => sms.SoftwaresUnlicensedCount;
                }
            }

            return keySelector;
        }

        private Expression<Func<MachineSoftwareLicenseReadOnly, object>> GetByMachineIdOrderingSelecetor(string sort)
        {
            Expression<Func<MachineSoftwareLicenseReadOnly, object>> keySelector = u => u.Software.Name;
            var sortModels = InstalledSoftwareModel.GetSorting();
            if (!string.IsNullOrEmpty(sort) && sortModels != null && sortModels.Any())
            {
                var sortModel = InstalledSoftwareModel.GetSortModel(sort);
                if (sortModel == null)
                {
                    return keySelector;
                }

                var orderedPropertyName = sortModel.PropertyName;
                if (orderedPropertyName == Nameof<InstalledSoftwareModel>.Property(e => e.Name))
                {
                    keySelector = u => u.Software.Name;
                }

                if (orderedPropertyName == Nameof<InstalledSoftwareModel>.Property(e => e.PublisherName))
                {
                    keySelector =
                        u =>
                            u.Software.Publisher != null ? u.Software.Publisher.Name : string.Empty;
                }

                if (orderedPropertyName == Nameof<InstalledSoftwareModel>.Property(e => e.DiscoveryDate))
                {
                    keySelector =
                        u =>
                            u.MachineSoftware.CreatedOn;
                }

                if (orderedPropertyName == Nameof<InstalledSoftwareModel>.Property(e => e.InstallDate))
                {
                    keySelector =
                        u =>
                            u.MachineSoftware.InstallDate;
                }

                if (orderedPropertyName == Nameof<InstalledSoftwareModel>.Property(e => e.Version))
                {
                    keySelector =
                        u =>
                            u.Software.Version;
                }

                if (orderedPropertyName == Nameof<InstalledSoftwareModel>.Property(e => e.SystemComponent))
                {
                    keySelector =
                        u =>
                            u.Software.SystemComponent;
                }

                if (orderedPropertyName == Nameof<InstalledSoftwareModel>.Property(e => e.WindowsInstaller))
                {
                    keySelector = u => u.Software.WindowsInstaller;
                }

                if (orderedPropertyName == Nameof<InstalledSoftwareModel>.Property(e => e.ReleaseType))
                {
                    keySelector = u => u.Software.ReleaseType;
                }

                if (orderedPropertyName == Nameof<InstalledSoftwareModel>.Property(e => e.HasLicense))
                {
                    keySelector = u => u.LicenseId.HasValue;
                }
            }

            return keySelector;
        }

        #region ISoftwareService Members

        public async Task<GetByIdResponse> GetByIdAsync(GetByIdRequest request)
        {
            var response = new GetByIdResponse();
            var dbContext = dbFactory.Create();

            using (IUnitOfWork unitOfWork = new UnitOfWork(dbContext))
            {
                SoftwareModel detail;
                var software = await unitOfWork.SoftwareRepository.GetAll().SingleAsync(s => s.UniqueId == request.Id);
                if (request.StructureUnitId.HasValue)
                {
                    var structureUnit = await
                        unitOfWork.StructureUnitRepository.Query(s => s.UniqueId == request.StructureUnitId)
                            .SingleAsync();
                    IQueryable<Machine> queryMachine;

                    if (!request.IncludeItemsOfSubUnits)
                    {
                        queryMachine =
                            unitOfWork.MachineRepository.Query(
                                m => m.CurrentLinkedStructureUnitId == structureUnit.Id && !m.IsDisabled);
                    }
                    else
                    {
                        var structureUnitIds = structureUnit.Descendants(sud => sud.ChildStructureUnits)
                            .Select(su => su.Id);
                        queryMachine = unitOfWork.MachineRepository.Query(m => !m.IsDisabled).Join(structureUnitIds,
                            m => m.CurrentLinkedStructureUnitId,
                            su => su,
                            (m, su) => m);
                    }

                    var machineSoftwares = queryMachine.SelectMany(lm => lm.MachineSoftwares)
                        .Where(ms => ms.SoftwareId == software.Id).ToArray();
                    detail = MapperFromModelToView.MapToSoftwareModel<SoftwareModel>(software, machineSoftwares);
                }
                else
                {
                    detail = MapperFromModelToView.MapToSoftwareModel<SoftwareModel>(software);
                }

                var queryObservable =
                    unitOfWork.ObservableRepository.Query(o =>
                        o.Company.UniqueId == request.CompanyId && o.SoftwareId == software.Id);

                var allowedStructureUnitIds = request.UserStructureUnitIds
                    .Select(suId => unitOfWork.StructureUnitRepository.GetAll().Single(s => s.UniqueId == suId))
                    .SelectMany(su => su.Descendants(sud => sud.ChildStructureUnits)).Select(su => su.Id);

                var licenses =
                    unitOfWork.LicenseRepository.Query(l =>
                        allowedStructureUnitIds.Contains(l.StructureUnitId) &&
                        l.LicenseSoftwares.Select(ls => ls.SoftwareId).Contains(software.Id));

                detail.ObservableProcesses =
                    queryObservable.Select(MapperFromModelToView.MapToObservableModel).ToArray();
                detail.Licenses = licenses.Select(l => MapperFromModelToView.MapToLicenseModel<LicenseModel>(l))
                    .ToArray();
                response.Detail = detail;
                return response;
            }
        }

        public async Task<GetByStructureUnitIdResponse> GetByStructureUnitIdAsync(GetByStructureUnitIdRequest request)
        {
            var response = new GetByStructureUnitIdResponse();
            response.Model = new SoftwareCollection(request.Ordering.Order, request.Ordering.Sort);
            var dbContext = dbFactory.Create();
            using (IUnitOfWork unitOfWork = new UnitOfWork(dbContext))
            {
                var structureUnit = await
                    unitOfWork.StructureUnitRepository.Query(s => s.UniqueId == request.StructureUnitId).SingleAsync();
                IEnumerable<int> structureUnitIds;
                IQueryable<SoftwareCurrentLinkedStructureUnitReadOnlyGrouped> query;
                if (!request.IncludeItemsOfSubUnits)
                {
                    query = unitOfWork.SoftwareCurrentLinkedStructureUnitReadOnlyRepository.Query(
                            m => m.CurrentLinkedStructureUnitId == structureUnit.Id)
                        .GroupBy(ms => ms.SoftwareId)
                        .Select(ms => new SoftwareCurrentLinkedStructureUnitReadOnlyGrouped()
                        {
                            SoftwareId = ms.Key,
                            SoftwaresTotalCount = ms.Sum(mss => mss.SoftwaresTotalCount),
                            SoftwaresIsExpiredCount = ms.Sum(mss => mss.SoftwaresIsExpiredLicenseCount),
                            SoftwaresIsActiveCount = ms.Sum(mss => mss.SoftwaresIsActiveCount),
                            SoftwaresUnlicensedCount = ms.Sum(mss => mss.SoftwaresUnlicensedCount)
                        });
                }
                else
                {
                    structureUnitIds = structureUnit.Descendants(sud => sud.ChildStructureUnits).Select(su => su.Id);
                    query = unitOfWork.SoftwareCurrentLinkedStructureUnitReadOnlyRepository
                        .Query(m => structureUnitIds.Contains(m.CurrentLinkedStructureUnitId))
                        .GroupBy(ms => ms.SoftwareId).Select(ms =>
                            new SoftwareCurrentLinkedStructureUnitReadOnlyGrouped()
                            {
                                SoftwareId = ms.Key,
                                SoftwaresTotalCount = ms.Sum(mss => mss.SoftwaresTotalCount),
                                SoftwaresIsExpiredCount = ms.Sum(mss => mss.SoftwaresIsExpiredLicenseCount),
                                SoftwaresIsActiveCount = ms.Sum(mss => mss.SoftwaresIsActiveCount),
                                SoftwaresUnlicensedCount = ms.Sum(mss => mss.SoftwaresUnlicensedCount)
                            });
                }

                var filter = this.GetFilter(request);
                var filteredSoftwares = unitOfWork.SoftwareRepository.GetAll().Include(s => s.Publisher).Where(filter);

                query = filteredSoftwares.Join(
                    query,
                    o => o.Id,
                    i => i.SoftwareId,
                    (o, i) => new SoftwareCurrentLinkedStructureUnitReadOnlyGrouped()
                    {
                        SoftwareId = i.SoftwareId,
                        Software = o,
                        PublisherName= o.Publisher.Name,
                        SoftwaresTotalCount = i.SoftwaresTotalCount,
                        SoftwaresIsExpiredCount = i.SoftwaresIsExpiredCount,
                        SoftwaresIsActiveCount = i.SoftwaresIsActiveCount,
                        SoftwaresUnlicensedCount = i.SoftwaresUnlicensedCount
                    });

                var totalRecords = query.Count();

                var keySelector = this.GetByStructureUnitIdOrderingSelecetor(request.Ordering.Sort);
                var softwares =
                    (string.IsNullOrEmpty(request.Ordering.Order) || request.Ordering.Order.ToLower() != "desc")
                        ? query.OrderBy(keySelector).Skip(request.Paging.PageIndex * request.Paging.PageSize)
                            .Take(request.Paging.PageSize)
                        : query.OrderByDescending(keySelector).Skip(request.Paging.PageIndex * request.Paging.PageSize)
                            .Take(request.Paging.PageSize);

                var items =
                    softwares.Select(
                        sms =>
                            MapperFromModelToView.MapToSoftwareModel<SoftwareModel>(sms.Software, sms)).ToArray();
                response.Model.Items = items;
                response.Model.TotalRecords = totalRecords;
                response.Status = GetByStructureUnitIdStatus.Success;
                return response;
            }
        }

        private Expression<Func<Software, bool>> GetFilter(GetByStructureUnitIdRequest request)
        {
            if (request.FilterItems.ContainsKey("Name") && !string.IsNullOrEmpty(request.FilterItems["Name"]))
            {
                var contains = request.FilterItems["Name"];
                return s => s.Name != null && s.Name.Contains(contains);
            }

            if (request.FilterItems.ContainsKey("PublisherName") &&
                !string.IsNullOrEmpty(request.FilterItems["PublisherName"]))
            {
                var contains = request.FilterItems["PublisherName"];
                return s => (s.Publisher != null && s.Publisher.Name.Contains(contains));
            }

            if (request.FilterItems.ContainsKey("Version") && !string.IsNullOrEmpty(request.FilterItems["Version"]))
            {
                var contains = request.FilterItems["Version"];
                return s => s.Version != null && s.Version.Contains(contains);
            }

            return s => true;
        }

        public SoftwareCollection GetForAutocomplete(Guid structureUnitId,
            string contains,
            bool? includeSubUnits = false)
        {
            contains = contains.ToLower();
            var dbContext = dbFactory.Create();
            using (IUnitOfWork unitOfWork = new UnitOfWork(dbContext))
            {
                var structureUnit =
                    unitOfWork.StructureUnitRepository.Query(su => su.UniqueId == structureUnitId).Single();

                IQueryable<Machine> machines;

                if (includeSubUnits.HasValue && includeSubUnits.Value)
                {
                    var structureUnitIds =
                        structureUnit.Descendants(sud => sud.ChildStructureUnits).Select(su => su.Id);
                    machines = unitOfWork.MachineRepository.Query(m => !m.IsDisabled)
                        .Where(m => structureUnitIds.Contains(m.CurrentLinkedStructureUnitId));
                }
                else
                {
                    machines = unitOfWork.MachineRepository.Query(m => !m.IsDisabled)
                        .Where(m => m.CurrentLinkedStructureUnitId == structureUnit.Id);
                }

                var query = machines
                    .SelectMany(m => m.MachineSoftwares)
                    .Select(ms => ms.Software)
                    .Where(s => s.Name.ToLower().Contains(contains) || s.Version.ToLower().Contains(contains))
                    .Distinct();

                var softwares = query.OrderBy(u => u.Name);
                var softwareModels =
                    Enumerable.ToArray(softwares.Select(MapperFromModelToView.MapToSoftwareModel<SoftwareModel>));
                var result = new SoftwareCollection(string.Empty, string.Empty);
                result.Items = softwareModels;
                result.TotalRecords = softwareModels.Count();
                return result;
            }
        }

        public GetSoftwaresByMachineIdResponse GetByMachineId(GetSoftwaresByMachineIdRequest request)
        {
            var licenseFilterType = (LicenseFilterType) request.FilterType;
            var response = new GetSoftwaresByMachineIdResponse
            {
                Model = new MachineSoftwareCollection(request.Ordering)
            };
            var dbContext = dbFactory.Create();
            using (IUnitOfWork unitOfWork = new UnitOfWork(dbContext))
            {
                var machine =
                    unitOfWork.MachineRepository.GetAll()
                        .Include(m => m.CurrentLinkedStructureUnit)
                        .SingleOrDefault(m => m.UniqueId == request.MachineId);

                if (machine == null)
                {
                    response.Status = GetSoftwaresByMachineIdStatus.MachineNotFound;
                    return response;
                }

                if (machine.IsDisabled)
                {
                    response.Status = GetSoftwaresByMachineIdStatus.MachineIsDisabled;
                    return response;
                }

                response.Model.MachineName = machine.Name;
                response.Model.UserId = machine.CurrentUserId;

                Expression<Func<MachineSoftwareLicenseReadOnly, bool>> filter = ms => true;
                Expression<Func<MachineSoftwareLicenseReadOnly, bool>> filterLicensed = ms => ms.IsActive;
                Expression<Func<MachineSoftwareLicenseReadOnly, bool>> filterUnlicensed = ms => !ms.LicenseId.HasValue;
                Expression<Func<MachineSoftwareLicenseReadOnly, bool>> filterExpiredLicensed =
                    ms => ms.LicenseId.HasValue && !ms.IsActive;

                var expressions = new List<Expression<Func<MachineSoftwareLicenseReadOnly, bool>>>();

                if (licenseFilterType.HasFlag(LicenseFilterType.Licensed))
                {
                    expressions.Add(filterLicensed);
                }

                if (licenseFilterType.HasFlag(LicenseFilterType.Unlicensed))
                {
                    expressions.Add(filterUnlicensed);
                }

                if (licenseFilterType.HasFlag(LicenseFilterType.ExpiredLicensed))
                {
                    expressions.Add(filterExpiredLicensed);
                }

                if (expressions.Count > 0)
                {
                    filter = ExpressionExtension.BuildOr(expressions);
                }

                var query = unitOfWork.MachineSoftwareLicenseReadOnlyRepository.GetAll()
                    .Where(s => s.MachineId == machine.Id)
                    .Where(filter);

                if (request.FilterItems.ContainsKey("Name") && !string.IsNullOrEmpty(request.FilterItems["Name"]))
                {
                    var contains = request.FilterItems["Name"];
                    query = query.Where(ms => ms.Software.Name.Contains(contains));
                }

                if (request.FilterItems.ContainsKey("PublisherName") &&
                    !string.IsNullOrEmpty(request.FilterItems["PublisherName"]))
                {
                    var contains = request.FilterItems["PublisherName"];
                    query =
                        query.Where(
                            ms =>
                                (ms.Software.Publisher != null && ms.Software.Publisher.Name.Contains(contains)));
                }

                if (request.FilterItems.ContainsKey("Name") && !string.IsNullOrEmpty(request.FilterItems["Name"]))
                {
                    var contains = request.FilterItems["Name"];
                    query = query.Where(ms => ms.Software.Name.Contains(contains));
                }

                if (request.FilterItems.ContainsKey("Version") && !string.IsNullOrEmpty(request.FilterItems["Version"]))
                {
                    var contains = request.FilterItems["Version"];
                    query = query.Where(ms => ms.Software.Version.Contains(contains));
                }

                if (request.FilterItems.ContainsKey("LicenseName") &&
                    !string.IsNullOrEmpty(request.FilterItems["LicenseName"]))
                {
                    var contains = request.FilterItems["LicenseName"];
                    query =
                        query.Where(
                            ms => ms.License != null && ms.License.Name.Contains(contains));
                }

                var totalRecords = query.Count();
                query = query
                    .Include(ms => ms.Software)
                    .Include(ms => ms.Software.Publisher)
                    .Include(ms => ms.License)
                    .Include(ms => ms.MachineSoftware);

                var keySelector = GetByMachineIdOrderingSelecetor(request.Ordering.Sort);

                if (string.IsNullOrEmpty(request.Ordering.Order) || request.Ordering.Order.ToLower() != "desc")
                {
                    query =
                        query.OrderBy(keySelector)
                            .Skip(request.Paging.PageIndex * request.Paging.PageSize)
                            .Take(request.Paging.PageSize);
                }
                else
                {
                    query =
                        query.OrderByDescending(keySelector)
                            .Skip(request.Paging.PageIndex * request.Paging.PageSize)
                            .Take(request.Paging.PageSize);
                }

                var items = query.ToArray().Select(s =>
                {
                    var soft =
                        MapperFromModelToView.MapToSoftwareModel
                            <InstalledSoftwareModel>(
                                s.Software);
                    soft.HasLicense = s.LicenseId.HasValue;
                    soft.LicenseName = s.License != null ? s.License.Name : string.Empty;
                    soft.LicenseId = s.License?.UniqueId;
                    soft.InstallDate = s.MachineSoftware.InstallDate;
                    soft.DiscoveryDate = s.MachineSoftware.CreatedOn;
                    return soft;
                });

                response.Model.Items = items;
                response.Model.TotalRecords = totalRecords;
                response.Model.StructureUnitId = machine.CurrentLinkedStructureUnit.UniqueId;

                response.Status = GetSoftwaresByMachineIdStatus.Success;
                return response;
            }
        }

        public IEnumerable<LicenseFilterTypeModel> GetSoftwareTypes()
        {
            var result = new List<LicenseFilterTypeModel>
            {
                new LicenseFilterTypeModel {Id = (int) LicenseFilterType.Licensed},
                new LicenseFilterTypeModel {Id = (int) LicenseFilterType.Unlicensed}
            };
            return result;
        }

        #endregion
    }

    public class SoftwareCurrentLinkedStructureUnitReadOnlyGrouped
    {
        public int SoftwareId { get; set; }
        public int SoftwaresTotalCount { get; set; }
        public int SoftwaresIsActiveCount { get; set; }
        public int SoftwaresUnlicensedCount { get; set; }
        public int SoftwaresIsExpiredCount { get; set; }
        public Software Software { get; set; }
        public string PublisherName { get; set; }
    }
}