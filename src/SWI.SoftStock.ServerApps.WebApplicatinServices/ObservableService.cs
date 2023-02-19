using SWI.SoftStock.ServerApps.DataAccess2;
using SWI.SoftStock.ServerApps.DataModel2;
using SWI.SoftStock.ServerApps.WebApplicationContracts;
using SWI.SoftStock.ServerApps.WebApplicationContracts.Statuses;
using SWI.SoftStock.ServerApps.WebApplicationModel;
using SWI.SoftStock.ServerApps.WebApplicationModel.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace SWI.SoftStock.ServerApps.WebApplicationServices
{
    using Microsoft.EntityFrameworkCore;
    using SWI.SoftStock.ServerApps.WebApplicationContracts.ObservableService.GetAll;
    using System.Threading.Tasks;

    public class ObservableService : IObservableService
    {
        private readonly IDbContextFactory<MainDbContext> dbFactory;

        public ObservableService(IDbContextFactory<MainDbContext> dbFactory)
        {
            this.dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
        }

        #region IObservableService Members

        public async Task<ObservableModelEx> GetObservableModelById(Guid observableId)
        {
            var dbContext = dbFactory.CreateDbContext();
            using IUnitOfWork unitOfWork = new UnitOfWork(dbContext);
            Observable observable = await
                unitOfWork.ObservableRepository.GetAll().SingleAsync(m => m.UniqueId == observableId);
            return observable != null
                       ? MapperFromModelToView.MapToObservableModelEx(observable)
                       : null;
        }

        public async Task<GetAllResponse> GetAll(GetAllRequest request)
        {
            GetAllResponse response = new GetAllResponse
            {
                Model = new ObservableExCollection(request.Ordering.Order, request.Ordering.Sort)
            };
            var dbContext = dbFactory.CreateDbContext();
            using IUnitOfWork unitOfWork = new UnitOfWork(dbContext);

            IQueryable<Observable> query = unitOfWork.ObservableRepository.Query(o => o.Company.UniqueId == request.CompanyId);
            if (!string.IsNullOrEmpty(request.Prname))
            {
                query = query.Where(o => o.ProcessName.Contains(request.Prname));
            }
            if (request.FilterSoftwareId.HasValue)
            {
                query = query.Where(o => o.Software.UniqueId == request.FilterSoftwareId);
            }
            int totalRecords = await query.CountAsync();
            var keySelector = GetAllOrderingSelecetor(request.Ordering.Sort);
            IEnumerable<Observable> observables;
            if (string.IsNullOrEmpty(request.Ordering.Order) || request.Ordering.Order.ToLower() != "desc")
            {
                observables = await query.OrderBy(keySelector).Skip(request.Paging.PageIndex * request.Paging.PageSize).Take(request.Paging.PageSize).ToArrayAsync();
            }
            else
            {
                observables = await
                    query.OrderByDescending(keySelector).Skip(request.Paging.PageIndex * request.Paging.PageSize).Take(request.Paging.PageSize).ToArrayAsync();
            }          
            response.Model.Items = observables.Select(MapperFromModelToView.MapToObservableModelEx);
            response.Model.TotalRecords = totalRecords;
            response.Status = GetAllStatus.Success;
            return response;
        }

        public async Task<Tuple<Guid?, ObservableCreationStatus>> Add(ObservableModelEx modelEx, Guid companyId, Guid createdByUserId)
        {
            Observable observable;
            ObservableCreationStatus status;
            var dbContext = dbFactory.CreateDbContext();
            using (IUnitOfWork unitOfWork = new UnitOfWork(dbContext))
            {
                var cId = (await unitOfWork.StructureUnitRepository.GetAll().SingleAsync(su => su.UniqueId == companyId)).Id;
                Software software = await
                    unitOfWork.SoftwareRepository.GetAll().SingleOrDefaultAsync(l => l.UniqueId == modelEx.SoftwareId);
                if (software == null)
                {
                    status = ObservableCreationStatus.SoftwareNotFound;
                    return null;
                }
                if (
                    await unitOfWork.ObservableRepository.GetAll().AnyAsync(
                        o => o.CompanyId == cId && o.SoftwareId == software.Id))
                {
                    status = ObservableCreationStatus.ObservableSoftwareExist;
                    return null;
                }
                observable = MapperFromViewToModel.MapToObservable(modelEx, cId, software.Id, createdByUserId);
                unitOfWork.ObservableRepository.Add(observable);
                await unitOfWork.SaveAsync();
            }
            status = ObservableCreationStatus.Success;
            return new Tuple<Guid?, ObservableCreationStatus>(observable.UniqueId, status);
        }

        public async Task<Tuple<Guid?, ObservableAppendStatus>> Append(Guid observableId, Guid machineId)
        {
            MachineObservedProcess machineObservedProcess;
            ObservableAppendStatus status;
            var dbContext = dbFactory.CreateDbContext();
            using (IUnitOfWork unitOfWork = new UnitOfWork(dbContext))
            {
                Observable observable = await
                    unitOfWork.ObservableRepository.GetAll().SingleOrDefaultAsync(l => l.UniqueId == observableId);
                if (observable == null)
                {
                    status = ObservableAppendStatus.ObservableNotFound;
                    return new Tuple<Guid?, ObservableAppendStatus>(null, status);
                }
                Machine machine = await
                    unitOfWork.MachineRepository.GetAll().SingleOrDefaultAsync(l => l.UniqueId == machineId);
                if (machine == null)
                {
                    status = ObservableAppendStatus.MachineNotFound;
                    return new Tuple<Guid?, ObservableAppendStatus>(null, status);
                }

                if (!machine.MachineSoftwares.Any(ms => ms.SoftwareId == observable.SoftwareId))
                {
                    status = ObservableAppendStatus.SoftwareNotInstalledOnMachine;
                    return new Tuple<Guid?, ObservableAppendStatus>(null, status);
                }
                if (
                    await unitOfWork.MachineObservedProcessRepository.GetAll().AnyAsync(
                        mop => mop.ObservableId == observable.Id && mop.MachineId == machine.Id))
                {
                    status = ObservableAppendStatus.AlreadyAppended;
                    return new Tuple<Guid?, ObservableAppendStatus>(null, status);
                }
                machineObservedProcess = new MachineObservedProcess
                {
                    ObservableId = observable.Id,
                    MachineId = machine.Id,
                    CreatedOn = DateTime.UtcNow
                };
                unitOfWork.MachineObservedProcessRepository.Add(machineObservedProcess);
                await unitOfWork.SaveAsync();
            }
            status = ObservableAppendStatus.Success;
            return new Tuple<Guid?, ObservableAppendStatus>(machineObservedProcess.UniqueId, status);
        }

        public async Task<ObservableRemoveStatus> Remove(Guid machineId, Guid observableId)
        {
            {
                var dbContext = dbFactory.CreateDbContext();
                using (IUnitOfWork unitOfWork = new UnitOfWork(dbContext))
                {
                    MachineObservedProcess machineObservedProcess = await
                        unitOfWork.MachineObservedProcessRepository.GetAll().SingleOrDefaultAsync(
                            l => l.Machine.UniqueId == machineId && l.Observable.UniqueId == observableId);
                    if (machineObservedProcess == null)
                    {
                        return ObservableRemoveStatus.MachineObservableProcessNotFound;
                    }
                    unitOfWork.ProcessRepository.DeleteRange(machineObservedProcess.Processes.ToArray());
                    unitOfWork.MachineObservedProcessRepository.Delete(machineObservedProcess);
                    await unitOfWork.SaveAsync();
                }
                return ObservableRemoveStatus.Success;
            }
        }

        public async Task<ObservableDeleteStatus> Delete(Guid observableId)
        {
            
                var dbContext = dbFactory.CreateDbContext();
                using (IUnitOfWork unitOfWork = new UnitOfWork(dbContext))
                {
                    Observable observable = await
                        unitOfWork.ObservableRepository.GetAll().SingleOrDefaultAsync(
                            l => l.UniqueId == observableId);
                    if (observable == null)
                    {
                        return ObservableDeleteStatus.NotExist;
                    }
                    if (observable.MachineObservedProcesses.Any())
                    {
                        return ObservableDeleteStatus.AppendedToMachine;
                    }
                    unitOfWork.ObservableRepository.Delete(observable);
                    await unitOfWork.SaveAsync();
                }
                return ObservableDeleteStatus.Success;
            
        }

        #endregion

        private static Expression<Func<Observable, object>> GetAllOrderingSelecetor(string sort)
        {
            Expression<Func<Observable, object>> keySelector = (m) => m.ProcessName;
            SortModel[] sortModels = ObservableModelEx.GetSorting();
            if (!string.IsNullOrEmpty(sort) && sortModels != null && sortModels.Any())
            {
                SortModel sortModel = ObservableModelEx.GetSortModel(sort);
                if (sortModel == null)
                {
                    return keySelector;
                }
                string orderedPropertyName = sortModel.PropertyName;
                if (orderedPropertyName == Nameof<ObservableModelEx>.Property(e => e.ProcessName))
                {
                    keySelector = (u) => u.ProcessName;
                }
                if (orderedPropertyName == Nameof<ObservableModelEx>.Property(e => e.SoftwareName))
                {
                    keySelector = (u) => u.Software.Name;
                }
                if (orderedPropertyName == Nameof<ObservableModelEx>.Property(e => e.PublisherName))
                {
                    keySelector = (u) => u.Software.Publisher != null ? u.Software.Publisher.Name : string.Empty;
                }
                if (orderedPropertyName == Nameof<ObservableModelEx>.Property(e => e.CreatedBy))
                {
                    keySelector = (u) => u.CreatedByUser.UserName;
                }
                if (orderedPropertyName == Nameof<ObservableModelEx>.Property(e => e.AppendedMachines))
                {
                    keySelector = (u) => u.MachineObservedProcesses.Select(mop => mop.Machine).Count();
                }
            }
            return keySelector;
        }
    }
}