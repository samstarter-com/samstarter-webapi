using FluentDateTime;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SWI.SoftStock.ServerApps.DataAccess2;
using SWI.SoftStock.ServerApps.DataModel2;
using SWI.SoftStock.ServerApps.WebApplicationContracts;
using SWI.SoftStock.ServerApps.WebApplicationContracts.LicenseService.Add;
using SWI.SoftStock.ServerApps.WebApplicationContracts.LicenseService.GetAvailableLicensesBySoftwareId;
using SWI.SoftStock.ServerApps.WebApplicationContracts.LicenseService.GetByStructureUnitId;
using SWI.SoftStock.ServerApps.WebApplicationContracts.LicenseService.GetLicenseUsage;
using SWI.SoftStock.ServerApps.WebApplicationContracts.LicenseService.GetLicenseUsageList;
using SWI.SoftStock.ServerApps.WebApplicationContracts.LicenseService.GetStructureUnitId;
using SWI.SoftStock.ServerApps.WebApplicationContracts.Statuses;
using SWI.SoftStock.ServerApps.WebApplicationModel;
using SWI.SoftStock.ServerApps.WebApplicationModel.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using License = SWI.SoftStock.ServerApps.DataModel2.License;

namespace SWI.SoftStock.ServerApps.WebApplicationServices
{
    public class LicenseService : ILicenseService
    {
        private readonly ILogger<LicenseService> log;
        private readonly IDbContextFactory<MainDbContext> dbFactory;

        public LicenseService(ILogger<LicenseService> log, IDbContextFactory<MainDbContext> dbFactory)
        {
            this.log = log;
            this.dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
        }

        private static Expression<Func<License, object>> GetByStructureUnitIdOrderingSelecetor(string sort)
        {
            Expression<Func<License, object>> keySelector = m => m.Name;
            var sortModels = LicenseModel.GetSorting();
            if (!string.IsNullOrEmpty(sort) && sortModels != null && sortModels.Any())
            {
                var sortModel = LicenseModel.GetSortModel(sort);
                if (sortModel == null)
                {
                    return keySelector;
                }
                var orderedPropertyName = sortModel.PropertyName;
                if (orderedPropertyName == Nameof<LicenseModel>.Property(e => e.Name))
                {
                    keySelector = u => u.Name;
                }
                if (orderedPropertyName == Nameof<LicenseModel>.Property(e => e.LicenseTypeName))
                {
                    keySelector = u => u.LicenseType.Name;
                }
                if (orderedPropertyName == Nameof<LicenseModel>.Property(e => e.Count))
                {
                    keySelector = u => u.Count;
                }
                if (orderedPropertyName == Nameof<LicenseModel>.Property(e => e.AvailableCount))
                {
                    keySelector =
                        u =>
                            u.Count -
                            u.LicenseSoftwares.SelectMany(ls => ls.LicenseMachineSoftwares)
                                .Select(lms => lms.MachineSoftware.MachineId)
                                .Distinct()
                                .Count();
                }
                if (orderedPropertyName == Nameof<LicenseModel>.Property(e => e.BeginDate))
                {
                    keySelector = u => u.BeginDate;
                }
                if (orderedPropertyName == Nameof<LicenseModel>.Property(e => e.ExpirationDate))
                {
                    keySelector = u => u.ExpirationDate;
                }
                if (orderedPropertyName == Nameof<LicenseModel>.Property(e => e.StructureUnitName))
                {
                    keySelector = u => u.StructureUnit.ShortName;
                }
            }
            return keySelector;
        }

        private static string GetTickText(BarChartRangeType barChartRange, DateTime @from, DateTime to)
        {
            string result = barChartRange switch
            {
                BarChartRangeType.Day => from.ToShortDateString(),
                BarChartRangeType.Week => string.Format("{0}-{1}", from.ToShortDateString(), to.ToShortDateString()),
                BarChartRangeType.Month => string.Format("M{0} Y{1}", from.Month, from.Year),
                BarChartRangeType.Quarter => string.Format("Q{0} Y{1}", (from.Month - 1) / 3 + 1, from.Year),
                BarChartRangeType.Year => from.Year.ToString(),
                _ => string.Empty,
            };
            return result;
        }

        private static T FillLicenseUsageRequest<T>(T request) where T : IGetLicenseUsageRequest
        {
            if (!request.Range.HasValue)
            {
                request.Range = (int)BarChartRangeType.Month;
            }
            var barChartRange = (BarChartRangeType)(request.Range);
            if (!request.ToDate.HasValue)
            {
                request.ToDate = DateTime.Now.Date.AddDays(1).AddTicks(-1);
            }

            if (!request.FromDate.HasValue)
            {
                switch (barChartRange)
                {
                    case BarChartRangeType.Day:
                        request.FromDate = request.ToDate.Value.AddDays(-7);
                        break;
                    case BarChartRangeType.Week:
                        request.FromDate = request.ToDate.Value.AddDays(-7 * 4);
                        break;
                    case BarChartRangeType.Month:
                        request.FromDate = request.ToDate.Value.AddMonths(-2);
                        break;
                    case BarChartRangeType.Quarter:
                        request.FromDate = request.ToDate.Value.AddMonths(-3 * 3);
                        break;
                    case BarChartRangeType.Year:
                        request.FromDate = request.ToDate.Value.AddYears(-1);
                        break;
                }
            }

            switch (barChartRange)
            {
                case BarChartRangeType.Day:
                    request.FromDate = request.FromDate.Value.Date;
                    break;
                case BarChartRangeType.Week:
                    request.FromDate = request.FromDate.Value.Date.FirstDayOfWeek();
                    break;
                case BarChartRangeType.Month:
                    request.FromDate = request.FromDate.Value.Date.FirstDayOfMonth();
                    break;
                case BarChartRangeType.Quarter:
                    request.FromDate = request.FromDate.Value.Date.FirstDayOfQuarter();
                    break;
                case BarChartRangeType.Year:
                    request.FromDate = request.FromDate.Value.Date.FirstDayOfYear();
                    break;
            }
            return request;
        }

        private static int TickCount(DateTime to, DateTime from, BarChartRangeType range)
        {
            var result = 0;

            switch (range)
            {
                case BarChartRangeType.Day:
                    result = (to - from).Days;
                    break;
                case BarChartRangeType.Week:
                    result = (int)Math.Ceiling((to - from).TotalDays / 7);
                    break;
                case BarChartRangeType.Month:
                    result = ((to.Year - from.Year) * 12) + (to.Month - from.Month) + 1;
                    break;
                case BarChartRangeType.Quarter:
                    result = (int)Math.Ceiling(((((to.Year - from.Year) * 12) + (to.Month - from.Month) + 1)) / 3.0);
                    break;
                case BarChartRangeType.Year:
                    result = (to.Year - from.Year) + 1;
                    break;
            }
            return result;
        }

        private static long GetTickLength(DateTime from, BarChartRangeType range)
        {
            long tickLength = 0;
            switch (range)
            {
                case BarChartRangeType.Day:
                    tickLength = TimeSpan.TicksPerDay;
                    break;
                case BarChartRangeType.Week:
                    tickLength = TimeSpan.TicksPerDay * 7;
                    break;
                case BarChartRangeType.Month:
                    tickLength = TimeSpan.TicksPerDay * DateTime.DaysInMonth(from.Year, from.Month);
                    break;
                case BarChartRangeType.Quarter:
                    tickLength = TimeSpan.TicksPerDay * ((from.AddMonths(3).LastDayOfMonth() - from).Days);
                    break;
                case BarChartRangeType.Year:
                    tickLength = TimeSpan.TicksPerDay * (DateTime.IsLeapYear(from.Year) ? 366 : 365);
                    break;
            }
            return tickLength;
        }

        private static Expression<Func<LicenseUsageMachineModel, object>> GetLicenseUsageListOrderingSelecetor(string sort)
        {
            Expression<Func<LicenseUsageMachineModel, object>> keySelector = m => m.Name;
            var sortModels = LicenseUsageMachineModel.GetSorting();
            if (!string.IsNullOrEmpty(sort) && sortModels != null && sortModels.Any())
            {
                var sortModel = LicenseUsageMachineModel.GetSortModel(sort);
                if (sortModel == null)
                {
                    return keySelector;
                }
                var orderedPropertyName = sortModel.PropertyName;
                if (orderedPropertyName == Nameof<LicenseUsageMachineModel>.Property(e => e.Name))
                {
                    keySelector = u => u.Name;
                }
                if (orderedPropertyName == Nameof<LicenseUsageMachineModel>.Property(e => e.CreatedOn))
                {
                    keySelector = u => u.CreatedOn;
                }
                if (orderedPropertyName == Nameof<LicenseUsageMachineModel>.Property(e => e.DomainUserName))
                {
                    keySelector = u => u.DomainUserName;
                }
                if (orderedPropertyName == Nameof<LicenseUsageMachineModel>.Property(e => e.DomainUserDomainName))
                {
                    keySelector = u => u.DomainUserDomainName;
                }
                if (orderedPropertyName == Nameof<LicenseUsageMachineModel>.Property(e => e.LinkedUserName))
                {
                    keySelector = u => u.LinkedUserName;
                }
                if (orderedPropertyName == Nameof<LicenseUsageMachineModel>.Property(e => e.StructureUnitName))
                {
                    keySelector = u => u.StructureUnitName;
                }
                if (orderedPropertyName == Nameof<LicenseUsageMachineModel>.Property(e => e.LicenseUsed))
                {
                    keySelector = u => u.LicenseUsed;
                }
                if (orderedPropertyName == Nameof<LicenseUsageMachineModel>.Property(e => e.From))
                {
                    keySelector = u => u.From;
                }
                if (orderedPropertyName == Nameof<LicenseUsageMachineModel>.Property(e => e.To))
                {
                    keySelector = u => u.To;
                }
            }
            return keySelector;
        }

        #region ILicenseService Members

        public async Task<GetStructureUnitIdResponse> GetStructureUnitId(GetStructureUnitIdRequest request)
        {
            GetStructureUnitIdResponse response = new GetStructureUnitIdResponse();
            var dbContext = dbFactory.CreateDbContext();
            using IUnitOfWork unitOfWork = new UnitOfWork(dbContext);
            License license =
                await unitOfWork.LicenseRepository.GetAll().SingleOrDefaultAsync(m => m.UniqueId == request.LicenseId);
            if (license == null)
            {
                response.Status = GetStructureUnitIdStatus.LicenseNotFound;
                return response;
            }
            response.StructureUnitId = license.StructureUnit.UniqueId;
            response.Status = GetStructureUnitIdStatus.Success;
            return response;
        }

        public async Task<LicenseModelEx> GetLicenseModelExById(Guid id)
        {
            var dbContext = dbFactory.CreateDbContext();
            using IUnitOfWork unitOfWork = new UnitOfWork(dbContext);
            var license = await unitOfWork.LicenseRepository.GetAll().SingleAsync(l => l.UniqueId == id);
            return license != null ? MapperFromModelToView.MapToLicenseModelEx(license) : null;
        }

        public async Task <LicenseModel> GetLicenseModelById(Guid id)
        {
            var dbContext = dbFactory.CreateDbContext();
            using IUnitOfWork unitOfWork = new UnitOfWork(dbContext);
            var license = await unitOfWork.LicenseRepository.GetAll().SingleAsync(l => l.UniqueId == id);
            return license != null ? MapperFromModelToView.MapToLicenseModel<LicenseModel>(license) : null;
        }

        public async Task<GetByStructureUnitIdResponse> GetByStructureUnitIdAsync(GetByStructureUnitIdRequest request)
        {
            var response = new GetByStructureUnitIdResponse
            {
                Model = new LicenseCollection(request.Ordering.Order, request.Ordering.Sort)
            };
            var dbContext = dbFactory.CreateDbContext();
            using IUnitOfWork unitOfWork = new UnitOfWork(dbContext);
            var structureUnit =
                await unitOfWork.StructureUnitRepository.GetAll().SingleAsync(s => s.UniqueId == request.StructureUnitId);
            IQueryable<License> query;
            if (!request.IncludeItemsOfSubUnits)
            {
                query = unitOfWork.LicenseRepository.Query(m => m.StructureUnitId == structureUnit.Id);
            }
            else
            {
                var structureUnitIds =
                    structureUnit.Descendants(sud => sud.ChildStructureUnits).Select(su => su.Id);
                query = unitOfWork.LicenseRepository.GetAll()
                    .Where(l => structureUnitIds.Contains(l.StructureUnitId));
            }

            var totalRecords = await query.CountAsync();

            var keySelector = GetByStructureUnitIdOrderingSelecetor(request.Ordering.Sort);

            var licenses =
                (string.IsNullOrEmpty(request.Ordering.Order) || request.Ordering.Order.ToLower() != "desc")
                    ? query.OrderBy(keySelector).Skip(request.Paging.PageIndex * request.Paging.PageSize)
                        .Take(request.Paging.PageSize)
                    : query.OrderByDescending(keySelector)
                        .Skip(request.Paging.PageIndex * request.Paging.PageSize)
                        .Take(request.Paging.PageSize);

            var items = await licenses
                .Include(l => l.LicenseType)
                .Include(l => l.LicenseSoftwares)                
                .ThenInclude(ls=>ls.Software)
                .ThenInclude(s => s.Publisher)
                .Include(l => l.LicenseSoftwares)
                .ThenInclude(ls => ls.LicenseMachineSoftwares)
                .ThenInclude(lms => lms.MachineSoftware)
                .Include(l => l.Documents)
                .Include(l => l.LicenseAlerts)
                .ThenInclude(la => la.Assignees)
                .ThenInclude(la => la.User)
                .ThenInclude(u => u.StructureUnitRoles)
                .ThenInclude(sur => sur.Role)
                .Include(l=>l.LicenseAlerts)
                .Include(l => l.StructureUnit)
                .AsSplitQuery()
                .ToArrayAsync();

            response.Model.Items = items.Select(MapperFromModelToView.MapToLicenseModel<LicenseModel>);
            response.Model.TotalRecords = totalRecords;
            response.Status = GetByStructureUnitIdStatus.Success;
            return response;
        }

        public async Task<GetLicenseUsageListResponse> GetLicenseUsageList(GetLicenseUsageListRequest request)
        {
            //if (!request.ViewType.HasValue)
            //{
            //	request.ViewType = (int) UsageViewType.Chart;
            //}
            request = FillLicenseUsageRequest(request);

            var response = new GetLicenseUsageListResponse
            {
                Model = new LicenseMachineUsageItemCollection(request.Ordering.Order, request.Ordering.Sort)
                {
                    From = request.FromDate ?? DateTime.Now.AddDays(-3),
                    To = request.ToDate ?? DateTime.Now,
                    Range = request.Range.Value
                }
            };
            var barChartRange = (BarChartRangeType)(request.Range.Value);
            var tickCount = TickCount(response.Model.To, response.Model.From, barChartRange);
            var items = new List<LicenseUsageMachineModel>();
            var currentTickIndex = 0;
            var from = response.Model.From;

            var dbContext = dbFactory.CreateDbContext();
            using (IUnitOfWork unitOfWork = new UnitOfWork(dbContext))
            {
                var license =
                    await unitOfWork.LicenseRepository.GetAll().SingleOrDefaultAsync(l => l.UniqueId == request.LicenseId);
                if (license == null)
                {
                    response.Status = GetLicenseUsageMachineListStatus.NotExist;
                    return response;
                }
                response.LicenseId = license.UniqueId;
                response.LicenseName = license.Name;
                while (currentTickIndex < tickCount)
                {
                    var licenseUsage = new LicenseUsageMachineModel();
                    var tickLength = GetTickLength(from, barChartRange);
                    var to = from.AddTicks(tickLength);
                    if (to > request.ToDate.Value)
                    {
                        to = request.ToDate.Value;
                    }

                    var lmssTotal = license.LicenseSoftwares
                        .SelectMany(
                            ls =>
                                ls.LicenseMachineSoftwares.Where(
                                    lms => lms.CreatedOn < to && (!lms.DeletedOn.HasValue || lms.DeletedOn.Value > to)));

                    var totalMachines = lmssTotal.Select(lms => lms.MachineSoftware.Machine).Distinct();

                    var usageMachineIds =
                        totalMachines.SelectMany(m => m.MachineObservedProcesses)
                            .Join(lmssTotal,
                                mop => mop.Observable.SoftwareId,
                                lms => lms.LicenseSoftware.SoftwareId,
                                (mop, lms) => new { mop, lms }).
                            Where(
                                j =>
                                    j.mop.Processes.Any(
                                        p => p.DateTime >= from && p.DateTime < to && p.DateTime >= j.lms.CreatedOn)).Select(
                                            j => j.mop.Machine.Id).Distinct();

                    foreach (var totalMachine in totalMachines)
                    {
                        licenseUsage = MapperFromModelToView.MapToMachineModel<LicenseUsageMachineModel>(totalMachine);
                        licenseUsage.LicenseUsed = usageMachineIds.Contains(totalMachine.Id);
                        licenseUsage.From = from;
                        licenseUsage.To = to;

                        items.Add(licenseUsage);
                    }

                    from = to;
                    currentTickIndex = currentTickIndex + 1;
                }
            }

            var totalRecords = items.Count;

            var keySelector = GetLicenseUsageListOrderingSelecetor(request.Ordering.Sort);
            IEnumerable<LicenseUsageMachineModel> usedMachines;
            if (string.IsNullOrEmpty(request.Ordering.Order) || request.Ordering.Order.ToLower() != "desc")
            {
                usedMachines =
                    items.AsQueryable().OrderBy(keySelector).Skip(request.Paging.PageIndex * request.Paging.PageSize).Take(request.Paging.PageSize);
            }
            else
            {
                usedMachines =
                    items.AsQueryable().OrderByDescending(keySelector)
                        .Skip(request.Paging.PageIndex * request.Paging.PageSize)
                        .Take(request.Paging.PageSize);
            }

            response.Model.Items = usedMachines;
            response.Model.TotalRecords = totalRecords;
            response.Status = GetLicenseUsageMachineListStatus.Success;
            return response;
        }

        public async Task<IEnumerable<DropDownItemModel>> GetLicenseTypes()
        {
            var dbContext = dbFactory.CreateDbContext();
            using IUnitOfWork unitOfWork = new UnitOfWork(dbContext);
            var licenseTypes = await unitOfWork.LicenseTypeRepository.GetAll().OrderBy(l => l.Id).ToArrayAsync();
            return licenseTypes.Select(MapperFromModelToView.MapToLicenseTypeModel);
        }

        public async Task<GetLicenseUsageResponse> GetLicenseUsage(GetLicenseUsageRequest request)
        {
            //if (!request.ViewType.HasValue)
            //{
            //	request.ViewType = (int) UsageViewType.Chart;
            //}

            var response = new GetLicenseUsageResponse
            {
                Model = new LicenseUsageItemCollection()
            };
            request = FillLicenseUsageRequest(request);
            response.Model.From = request.FromDate.Value;
            response.Model.To = request.ToDate.Value;
            response.Model.Range = request.Range.Value;
            var barChartRange = (BarChartRangeType)(request.Range.Value);
            var tickCount = TickCount(request.ToDate.Value, request.FromDate.Value, barChartRange);

            var items = new List<LicenseUsageItemModel>();

            var currentTickIndex = 0;
            var from = request.FromDate.Value;
            var dbContext = dbFactory.CreateDbContext();
            using (IUnitOfWork unitOfWork = new UnitOfWork(dbContext))
            {
                var license =
                    await unitOfWork.LicenseRepository.GetAll().SingleOrDefaultAsync(l => l.UniqueId == request.LicenseId);
                if (license == null)
                {
                    response.Status = GetLicenseUsageStatus.NotExist;
                    return response;
                }
                response.LicenseId = license.UniqueId;
                response.LicenseName = license.Name;
                while (currentTickIndex < tickCount)
                {
                    var licenseUsage = new LicenseUsageItemModel();
                    var ticketLength = GetTickLength(from, barChartRange);
                    var to = from.AddTicks(ticketLength);
                    if (to > request.ToDate.Value)
                    {
                        to = request.ToDate.Value;
                    }

                    var lmssTotal = license.LicenseSoftwares
                        .SelectMany(
                            ls =>
                                ls.LicenseMachineSoftwares.Where(
                                    lms => lms.CreatedOn < to && (!lms.DeletedOn.HasValue || lms.DeletedOn.Value > to)));

                    var totalMachines = lmssTotal.Select(lms => lms.MachineSoftware.Machine).Distinct();

                    var usageMachine =
                        totalMachines.SelectMany(m => m.MachineObservedProcesses)
                            .Join(lmssTotal,
                                mop => mop.Observable.SoftwareId,
                                lms => lms.LicenseSoftware.SoftwareId,
                                (mop, lms) => new { mop, lms }).
                            Where(
                                j =>
                                    j.mop.Processes.Any(
                                        p => p.DateTime >= from && p.DateTime < to && p.DateTime >= j.lms.CreatedOn)).Select(
                                            j => j.mop.Machine).Distinct();

                    licenseUsage.Index = currentTickIndex;
                    licenseUsage.TickText = GetTickText(barChartRange, from, to);
                    licenseUsage.TotalCount = totalMachines.Count();
                    licenseUsage.UsageCount = usageMachine.Count();

                    items.Add(licenseUsage);
                    from = to;
                    currentTickIndex = currentTickIndex + 1;
                }
            }

            response.Status = GetLicenseUsageStatus.Success;
            response.Model.Items = items;

            response.Filter = new UsageFilterModel
            {
                //ViewType = request.ViewType.Value,
                Range = response.Model.Range,
                From = response.Model.From,
                To = response.Model.To
            };

            return response;
        }

        public async Task<LicenseAddResponse> AddAsync(LicenseModelEx model)
        {
            var result = new LicenseAddResponse();
            // todo extend LicenseCreationStatus: checks for the existence of LinkedSoftwares entities and others in model
            var dbContext = dbFactory.CreateDbContext();
            License license;
            using (IUnitOfWork unitOfWork = new UnitOfWork(dbContext))
            {
                var structureUnit =
                    await unitOfWork.StructureUnitRepository.GetAll().SingleAsync(su => su.UniqueId == model.StructureUnitId);
                var softwareIds = model.LinkedSoftwares.Select(ls => ls.SoftwareId);
                var softwares = await unitOfWork.SoftwareRepository.GetAll()
                        .Where(s => softwareIds.Contains(s.UniqueId)).ToArrayAsync();

                license = MapperFromViewToModel.MapToLicense(model, structureUnit, softwares);
                unitOfWork.LicenseRepository.Add(license);
                await unitOfWork.SaveAsync();
            }
            result.Status = LicenseCreationStatus.Success;
            result.LicenseUniqueId = license.UniqueId;
            return result;
        }

        public async Task<DocumentModelEx> GetDocumentById(Guid id)
        {
            var dbContext = dbFactory.CreateDbContext();
            using IUnitOfWork unitOfWork = new UnitOfWork(dbContext);
            var doc = await unitOfWork.DocumentRepository.GetAll().SingleAsync(d => d.UniqueId == id);
            return MapperFromModelToView.MapToDocumentModelEx(doc, true);
        }

        public async Task<LicenseUpdateStatus> UpdateAsync(LicenseModelEx model)
        {
            var now = DateTime.Now;
            var dbContext = dbFactory.CreateDbContext();
            using IUnitOfWork unitOfWork = new UnitOfWork(dbContext);
            var license =
                await unitOfWork.LicenseRepository.GetAll().SingleOrDefaultAsync(l => l.UniqueId == model.LicenseId);
            var changed = false;
            if (license == null)
            {
                return LicenseUpdateStatus.NotExist;
            }
            //todo when reducing the number of licenses, check that the number of already installed licenses is not exceeded
            if (!model.Equals(license))
            {
                license.Name = model.Name.Trim();
                license.LicenseTypeId = model.LicenseTypeId.Value;
                license.BeginDate = model.BeginDate;
                license.ExpirationDate = model.ExpirationDate;
                license.Count = model.Count;
                license.Comments = model.Comments != null ? model.Comments.Trim() : string.Empty;
                unitOfWork.LicenseRepository.Update(license, license.Id);
                changed = true;
            }
            var addedSoftwareUniqueIds =
                model.LinkedSoftwares.Where(
                    current =>
                        !license.LicenseSoftwares.Any(stored => current.SoftwareId == stored.Software.UniqueId)).Select(
                            sm => sm.SoftwareId).ToArray();
            var removedSoftwareModels =
                license.LicenseSoftwares.Where(
                    stored => !model.LinkedSoftwares.Any(current => current.SoftwareId == stored.Software.UniqueId)).ToArray();
            // todo check that the license is not tied to removedSoftwareModels. If it is linked, then do not change, but return the status of LinkedToRemovedSoftware

            var addedSoftwares =
                await unitOfWork.SoftwareRepository.GetAll().Where(s => addedSoftwareUniqueIds.Contains(s.UniqueId)).ToArrayAsync();

            if (addedSoftwares.Any())
            {
                unitOfWork.LicenseSoftwareRepository.AddRange(
                    addedSoftwares.Select(a => new LicenseSoftware { LicenseId = license.Id, SoftwareId = a.Id }).
                        ToArray());
                changed = true;
            }
            if (removedSoftwareModels.Any())
            {
                if (license.BeginDate < now && license.ExpirationDate > now &&
                    removedSoftwareModels.SelectMany(rsm => rsm.LicenseMachineSoftwares).Any(lms => !lms.IsDeleted))
                {
                    return LicenseUpdateStatus.LinkedToRemovedSoftware;
                }
                foreach (var removedSoftwareModel in removedSoftwareModels)
                {
                    unitOfWork.LicenseMachineSoftwareRepository.DeleteRange(removedSoftwareModel.LicenseMachineSoftwares.ToArray());
                }
                unitOfWork.LicenseSoftwareRepository.DeleteRange(removedSoftwareModels);
                changed = true;
            }

            var addedDocuments =
                model.Documents?.Where(current => license.Documents.All(stored => current.Id != stored.UniqueId)).ToArray();
            var removedDocuments =
                license.Documents.Where(stored => model.Documents.All(current => current.Id != stored.UniqueId)).ToArray();
            var changedDocuments = license.Documents.Except(removedDocuments)
                .Join(
                    model.Documents?.Select(MapperFromViewToModel.MapToDocument) ?? Array.Empty<Document>(),
                    o => o.UniqueId,
                    i => i.UniqueId,
                    (o, i) => new { Outer = o, Inner = i })
                .Where(j => j.Outer.HcLocation?.Trim() != j.Inner.HcLocation?.Trim())
                .Select(
                    j =>
                    {
                        j.Outer.HcLocation = j.Inner.HcLocation;
                        return j.Outer;
                    });

            if (addedDocuments?.Any() == true)
            {
                var uploadedDocuments = await unitOfWork.UploadedDocumentRepository.GetAll()
                    .Where(ud => addedDocuments.Select(ad => ad.UploadId).Contains(ud.Id)).ToArrayAsync();

                unitOfWork.DocumentRepository.AddRange(
                    addedDocuments.Select(
                        d =>
                        {
                            d.Content = uploadedDocuments.SingleOrDefault(ud => ud.Id == d.UploadId)?.Content;
                            var docModel = MapperFromViewToModel.MapToDocument(d);
                            docModel.LicenseId = license.Id;
                            return docModel;
                        }).ToArray());
                changed = true;
                unitOfWork.UploadedDocumentRepository.DeleteRange(uploadedDocuments);
            }

            if (removedDocuments.Any())
            {
                unitOfWork.DocumentRepository.DeleteRange(removedDocuments);
                changed = true;
            }

            if (changedDocuments.Any())
            {
                foreach (var changedDocument in changedDocuments)
                {
                    unitOfWork.DocumentRepository.Update(changedDocument, changedDocument.Id);
                }

                changed = true;
            }

            var addedAlerts =
                model.Alerts?.Where(current => license.LicenseAlerts.All(stored => current.Id != stored.UniqueId)).ToArray();
            var removedAlerts =
                license.LicenseAlerts.Where(stored => model.Alerts.All(current => current.Id != stored.UniqueId)).ToArray();
            var changedAlerts = license.LicenseAlerts.Except(removedAlerts)
                .Join(model.Alerts?.Select(MapperFromViewToModel.MapToAlert) ?? Array.Empty<LicenseAlert>(),
                    o => o.UniqueId,
                    i => i.UniqueId,
                    (o, i) => new { Outer = o, Inner = i })
                .Where(j => !j.Outer.Equals(j.Inner))
                .Select(j =>
                {
                    var deletedAlertUsers = j.Outer.Assignees.Where(
                        o => j.Inner.Assignees.All(i => i.UserUserId != o.UserUserId)).ToArray();
                    unitOfWork.LicenseAlertUserRepository.DeleteRange(deletedAlertUsers);

                    var addedAlertUsers = j.Inner.Assignees.
                        Where(
                            current => j.Outer.Assignees.All(stored => current.UserUserId != stored.UserUserId));
                    foreach (var added in addedAlertUsers)
                    {
                        j.Outer.Assignees.Add(added);
                    }

                    j.Outer.AlertDate = j.Inner.AlertDate;

                    j.Outer.Text = j.Inner.Text;
                    return j.Outer;
                }).ToArray();

            if (addedAlerts != null && addedAlerts.Any())
            {
                unitOfWork.LicenseAlertRepository.AddRange(
                    addedAlerts.Select(
                        a =>
                        {
                            var alertModel = MapperFromViewToModel.MapToAlert(a);
                            alertModel.LicenseId = license.Id;
                            return alertModel;
                        }).ToArray());
                changed = true;
            }

            if (removedAlerts.Any())
            {
                unitOfWork.LicenseAlertRepository.DeleteRange(removedAlerts);
                changed = true;
            }

            if (changedAlerts.Any())
            {
                foreach (var changedAlert in changedAlerts)
                {
                    unitOfWork.LicenseAlertRepository.Update(changedAlert, changedAlert.Id);
                }

                changed = true;
            }

            if (changed)
            {
                await unitOfWork.SaveAsync();
            }

            return LicenseUpdateStatus.Success;
        }

        public async Task<LicenseDeleteStatus> DeleteById(Guid licenseId)
        {
            var dbContext = dbFactory.CreateDbContext();
            using IUnitOfWork unitOfWork = new UnitOfWork(dbContext);
            var license =
                await unitOfWork.LicenseRepository.GetAll().SingleAsync(l => l.UniqueId == licenseId);

            if (license.LicenseSoftwares.Any() && license.LicenseSoftwares.SelectMany(ls => ls.LicenseMachineSoftwares).Any())
            {
                return LicenseDeleteStatus.LicenseAttachedToMachine;
            }

            try
            {
                unitOfWork.LicenseRepository.Delete(license);
                await unitOfWork.SaveAsync();
            }
            catch (Exception e)
            {
                log.LogError(0, e, e.Message);
                return LicenseDeleteStatus.UnknownError;
            }

            return LicenseDeleteStatus.Success;
        }

        public async Task<LicenseLinkToStructureUnitStatus> LinkToStructureUnitAsync(Guid licenseId, Guid structureUnitId)
        {
            var dbContext = dbFactory.CreateDbContext();
            using (IUnitOfWork unitOfWork = new UnitOfWork(dbContext))
            {
                var license = await unitOfWork.LicenseRepository.GetAll().SingleOrDefaultAsync(m => m.UniqueId == licenseId);
                if (license == null)
                {
                    return LicenseLinkToStructureUnitStatus.NotExist;
                }
                if (license.StructureUnit != null && license.StructureUnit.UniqueId == structureUnitId)
                {
                    // if the license is already attached to this structural unit, then do nothing
                    return LicenseLinkToStructureUnitStatus.Success;
                }

                var structureUnit = await unitOfWork.StructureUnitRepository.Query(su => su.UniqueId == structureUnitId).SingleOrDefaultAsync();
                if (structureUnit == null)
                {
                    return LicenseLinkToStructureUnitStatus.StructureUnitNotExist;
                }

                license.StructureUnit = structureUnit;
                unitOfWork.LicenseRepository.Update(license, license.Id);
                await unitOfWork.SaveAsync();
            }
            return LicenseLinkToStructureUnitStatus.Success;
        }

        public async Task<GetAvailableLicensesBySoftwareIdResponse> GetAvailableLicensesBySoftwareId(
            GetAvailableLicensesBySoftwareIdRequest request)
        {
            var response = new GetAvailableLicensesBySoftwareIdResponse();
            var now = DateTime.Now;
            response.Model = new ShortLicenseCollection(request.Ordering.Order, request.Ordering.Sort);
            var dbContext = dbFactory.CreateDbContext();
            using IUnitOfWork unitOfWork = new UnitOfWork(dbContext);
            var sus = unitOfWork.StructureUnitRepository.GetAll().Join(request.SuGuids, o => o.UniqueId, i => i, (o, i) => o);
            var software = await unitOfWork.SoftwareRepository.GetAll().SingleAsync(s => s.UniqueId == request.SoftwareId);
            var query = unitOfWork.LicenseRepository.GetAll().Join(sus, o => o.StructureUnitId, i => i.Id, (o, i) => o)
                .Where(l => l.LicenseSoftwares.Select(ls => ls.SoftwareId).Any(sids => sids == software.Id))
                .Where(l => l.BeginDate < now && now < l.ExpirationDate);

            var totalRecords = await query.CountAsync();

            var keySelector = GetByStructureUnitIdOrderingSelecetor(request.Ordering.Sort);
            IEnumerable<License> licenses;
            if (string.IsNullOrEmpty(request.Ordering.Order) || request.Ordering.Order.ToLower() != "desc")
            {
                licenses = await
                    query.OrderBy(keySelector).Skip(request.Paging.PageIndex * request.Paging.PageSize).Take(request.Paging.PageSize).ToArrayAsync(); ;
            }
            else
            {
                licenses =await
                    query.OrderByDescending(keySelector)
                        .Skip(request.Paging.PageIndex * request.Paging.PageSize)
                        .Take(request.Paging.PageSize).ToArrayAsync();
            }
            var items = licenses.Select(MapperFromModelToView.MapToShortLicenseModel);
            response.Model.Items = items;
            response.Model.TotalRecords = totalRecords;
            response.Status = GetAvailableLicensesBySoftwareIdStatus.Success;
            return response;
        }

        #endregion
    }
}