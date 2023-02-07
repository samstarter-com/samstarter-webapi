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
    using SWI.SoftStock.ServerApps.WebApplicationContracts.ObservableService.GetAll;

    public class ObservableService : IObservableService
    {
        private readonly MainDbContextFactory dbFactory;

        public ObservableService(MainDbContextFactory dbFactory)
        {
            this.dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
        }

        #region IObservableService Members

        public ObservableModelEx GetObservableModelById(Guid observableId)
        {
            var dbContext = dbFactory.Create();
            using IUnitOfWork unitOfWork = new UnitOfWork(dbContext);
            Observable observable =
                unitOfWork.ObservableRepository.GetAll().Single(m => m.UniqueId == observableId);
            return observable != null
                       ? MapperFromModelToView.MapToObservableModelEx(observable)
                       : null;
        }

        public GetAllResponse GetAll(GetAllRequest request)
        {
            GetAllResponse response = new GetAllResponse
            {
                Model = new ObservableExCollection(request.Ordering.Order, request.Ordering.Sort)
            };
            var dbContext = dbFactory.Create();
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
            int totalRecords = query.Count();
            var keySelector = GetAllOrderingSelecetor(request.Ordering.Sort);
            IEnumerable<Observable> observables;
            if (string.IsNullOrEmpty(request.Ordering.Order) || request.Ordering.Order.ToLower() != "desc")
            {
                observables = query.OrderBy(keySelector).Skip(request.Paging.PageIndex * request.Paging.PageSize).Take(request.Paging.PageSize);
            }
            else
            {
                observables =
                    query.OrderByDescending(keySelector).Skip(request.Paging.PageIndex * request.Paging.PageSize).Take(request.Paging.PageSize);
            }
            ObservableModelEx[] items = observables.Select(MapperFromModelToView.MapToObservableModelEx).ToArray();
            response.Model.Items = items;
            response.Model.TotalRecords = totalRecords;
            response.Status = GetAllStatus.Success;
            return response;
        }

        public Guid? Add(ObservableModelEx modelEx, Guid companyId, Guid createdByUserId, out ObservableCreationStatus status)
        {
            Observable observable;
            var dbContext = dbFactory.Create();
            using (IUnitOfWork unitOfWork = new UnitOfWork(dbContext))
            {
                var cId = unitOfWork.StructureUnitRepository.GetAll().Single(su => su.UniqueId == companyId).Id;
                Software software =
                    unitOfWork.SoftwareRepository.GetAll().SingleOrDefault(l => l.UniqueId == modelEx.SoftwareId);
                if (software == null)
                {
                    status = ObservableCreationStatus.SoftwareNotFound;
                    return null;
                }
                if (
                    unitOfWork.ObservableRepository.GetAll().Any(
                        o => o.CompanyId == cId && o.SoftwareId == software.Id))
                {
                    status = ObservableCreationStatus.ObservableSoftwareExist;
                    return null;
                }
                observable = MapperFromViewToModel.MapToObservable(modelEx, cId, software.Id, createdByUserId);
                unitOfWork.ObservableRepository.Add(observable);
                unitOfWork.Save();
            }
            status = ObservableCreationStatus.Success;
            return observable.UniqueId;
        }

        public Guid? Append(Guid observableId, Guid machineId, out ObservableAppendStatus status)
        {
            MachineObservedProcess machineObservedProcess;
            var dbContext = dbFactory.Create();
            using (IUnitOfWork unitOfWork = new UnitOfWork(dbContext))
            {
                Observable observable =
                    unitOfWork.ObservableRepository.GetAll().SingleOrDefault(l => l.UniqueId == observableId);
                if (observable == null)
                {
                    status = ObservableAppendStatus.ObservableNotFound;
                    return null;
                }
                Machine machine =
                    unitOfWork.MachineRepository.GetAll().SingleOrDefault(l => l.UniqueId == machineId);
                if (machine == null)
                {
                    status = ObservableAppendStatus.MachineNotFound;
                    return null;
                }

                if (!machine.MachineSoftwares.Any(ms => ms.SoftwareId == observable.SoftwareId))
                {
                    status = ObservableAppendStatus.SoftwareNotInstalledOnMachine;
                    return null;
                }
                if (
                    unitOfWork.MachineObservedProcessRepository.GetAll().Any(
                        mop => mop.ObservableId == observable.Id && mop.MachineId == machine.Id))
                {
                    status = ObservableAppendStatus.AlreadyAppended;
                    return null;
                }
                machineObservedProcess = new MachineObservedProcess
                {
                    ObservableId = observable.Id,
                    MachineId = machine.Id,
                    CreatedOn = DateTime.UtcNow
                };
                unitOfWork.MachineObservedProcessRepository.Add(machineObservedProcess);
                unitOfWork.Save();
            }
            status = ObservableAppendStatus.Success;
            return machineObservedProcess.UniqueId;
        }

        public ObservableRemoveStatus Remove(Guid machineId, Guid observableId)
        {
            {
                var dbContext = dbFactory.Create();
                using (IUnitOfWork unitOfWork = new UnitOfWork(dbContext))
                {
                    MachineObservedProcess machineObservedProcess =
                        unitOfWork.MachineObservedProcessRepository.GetAll().SingleOrDefault(
                            l => l.Machine.UniqueId == machineId && l.Observable.UniqueId == observableId);
                    if (machineObservedProcess == null)
                    {
                        return ObservableRemoveStatus.MachineObservableProcessNotFound;
                    }
                    unitOfWork.ProcessRepository.DeleteRange(machineObservedProcess.Processes.ToArray());
                    unitOfWork.MachineObservedProcessRepository.Delete(machineObservedProcess);
                    unitOfWork.Save();
                }
                return ObservableRemoveStatus.Success;
            }
        }

        public ObservableDeleteStatus Delete(Guid observableId)
        {
            {
                var dbContext = dbFactory.Create();
                using (IUnitOfWork unitOfWork = new UnitOfWork(dbContext))
                {
                    Observable observable =
                        unitOfWork.ObservableRepository.GetAll().SingleOrDefault(
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
                    unitOfWork.Save();
                }
                return ObservableDeleteStatus.Success;
            }
        }

        #endregion

        private Expression<Func<Observable, object>> GetAllOrderingSelecetor(string sort)
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