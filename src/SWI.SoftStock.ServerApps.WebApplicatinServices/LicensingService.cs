using Microsoft.EntityFrameworkCore;
using SWI.SoftStock.ServerApps.DataAccess2;
using SWI.SoftStock.ServerApps.DataModel2;
using SWI.SoftStock.ServerApps.WebApplicationContracts;
using SWI.SoftStock.ServerApps.WebApplicationContracts.LicensingService.GetAvailableLicensesByMachineId;
using SWI.SoftStock.ServerApps.WebApplicationContracts.LicensingService.GetLicensedMachine;
using SWI.SoftStock.ServerApps.WebApplicationContracts.LicensingService.LicenseLicense;
using SWI.SoftStock.ServerApps.WebApplicationContracts.LicensingService.LicenseMachine;
using SWI.SoftStock.ServerApps.WebApplicationContracts.LicensingService.LicenseMachines;
using SWI.SoftStock.ServerApps.WebApplicationContracts.LicensingService.LicenseSoftware;
using SWI.SoftStock.ServerApps.WebApplicationContracts.LicensingService.UnLicenseLicenses;
using SWI.SoftStock.ServerApps.WebApplicationContracts.LicensingService.UnLicenseMachine;
using SWI.SoftStock.ServerApps.WebApplicationContracts.LicensingService.UnLicenseMachines;
using SWI.SoftStock.ServerApps.WebApplicationContracts.LicensingService.UnLicenseSoftware;
using SWI.SoftStock.ServerApps.WebApplicationModel;
using SWI.SoftStock.ServerApps.WebApplicationModel.Collections;
using SWI.SoftStock.ServerApps.WebApplicationModel.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using PagingModel = SWI.SoftStock.ServerApps.WebApplicationModel.Common.PagingModel;

namespace SWI.SoftStock.ServerApps.WebApplicationServices
{
    public class LicensingService : ILicensingService
	{
        private readonly MainDbContextFactory dbFactory;

		public LicensingService(MainDbContextFactory dbFactory)
        {
            this.dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
        }

		#region ILicensingService Members

		public LicenseSoftwareResponse LicenseSoftware(LicenseSoftwareRequest softwareRequest)
		{
			var response = new LicenseSoftwareResponse();

			var dbContext = dbFactory.Create();
			using (IUnitOfWork unitOfWork = new UnitOfWork(dbContext))
			{
				var software =
					unitOfWork.SoftwareRepository.GetAll().SingleOrDefault(s => s.UniqueId == softwareRequest.SoftwareId);

				if (software == null)
				{
					response.Status = LicenseSoftwareStatus.SoftwareNotFound;
					return response;
				}

				var machine =
					unitOfWork.MachineRepository.GetAll().SingleOrDefault(m => m.UniqueId == softwareRequest.MachineId);

				if (machine == null)
				{
					response.Status = LicenseSoftwareStatus.MachineNotFound;
					return response;
				}

				var license =
					unitOfWork.LicenseRepository.GetAll().SingleOrDefault(m => m.UniqueId == softwareRequest.LicenseId);
				if (license == null)
				{
					response.Status = LicenseSoftwareStatus.LicenseNotFound;
					return response;
				}
				var licenseSoftware = license.LicenseSoftwares.SingleOrDefault(ls => ls.SoftwareId == software.Id);
				if (licenseSoftware == null)
				{
					response.Status = LicenseSoftwareStatus.LicenseNotForSoftware;
					return response;
				}

				var machineSoftwares =
					unitOfWork.MachineSoftwareRepository.GetAll()
						.Where(ms => ms.MachineId == machine.Id && ms.SoftwareId == software.Id);
				if (!machineSoftwares.Any())
				{
					response.Status = LicenseSoftwareStatus.SoftwareOnMachineNotFound;
					return response;
				}
				if (machineSoftwares.SelectMany(ms => ms.LicenseMachineSoftwares).Any(lms => !lms.IsDeleted))
				{
					response.Status = LicenseSoftwareStatus.SoftwareIsLinked;
					return response;
				}

				// license number check. is > 0
				var linkedLicenseCount = license.TotalUsedLicenseCount();
				if (linkedLicenseCount >= license.Count)
				{
					response.Status = LicenseSoftwareStatus.LicenseCountExceeded;
					return response;
				}

				var notLicensedMachineSoftwares =
					machineSoftwares.Where(
						ms => !ms.LicenseMachineSoftwares.Any() || ms.LicenseMachineSoftwares.All(lms => lms.IsDeleted))
						.ToArray();

				var lmsss = notLicensedMachineSoftwares.Select(
					machineSoftware => new LicenseMachineSoftware
					{
						MachineSoftwareId = machineSoftware.Id,
						LicenseSoftwareId = licenseSoftware.Id,
						CreatedOn = DateTime.UtcNow
					});
				foreach (var lms in lmsss)
				{
					unitOfWork.LicenseMachineSoftwareRepository.Add(lms);
				}
				unitOfWork.Save();
				response.Status = LicenseSoftwareStatus.Success;
				return response;
			}
		}

		public UnLicenseSoftwareResponse UnLicenseSoftware(UnLicenseSoftwareRequest request)
		{
			var response = new UnLicenseSoftwareResponse();
			var utcNow = DateTime.UtcNow;
			var dbContext = dbFactory.Create();
			using (IUnitOfWork unitOfWork = new UnitOfWork(dbContext))
			{
				var software =
					unitOfWork.SoftwareRepository.GetAll().SingleOrDefault(s => s.UniqueId == request.SoftwareId);

				if (software == null)
				{
					response.Status = UnLicenseSoftwareStatus.SoftwareNotFound;
					return response;
				}

				var machine =
					unitOfWork.MachineRepository.GetAll().SingleOrDefault(m => m.UniqueId == request.MachineId);

				if (machine == null)
				{
					response.Status = UnLicenseSoftwareStatus.MachineNotFound;
					return response;
				}

				var machineSoftwares =
					unitOfWork.MachineSoftwareRepository.GetAll().Where(
						ms => ms.MachineId == machine.Id && ms.SoftwareId == software.Id);
				if (!machineSoftwares.Any())
				{
					response.Status = UnLicenseSoftwareStatus.SoftwareOnMachineNotFound;
					return response;
				}
				if (
					machineSoftwares.All(
						ms => !ms.LicenseMachineSoftwares.Any() || ms.LicenseMachineSoftwares.All(lms => lms.IsDeleted)))
				{
					response.Status = UnLicenseSoftwareStatus.SoftwareIsNotLinked;
					return response;
				}

				var lmsss =
					machineSoftwares.SelectMany(mss => mss.LicenseMachineSoftwares).Where(lms => !lms.IsDeleted).ToArray();
				foreach (var licenseMachineSoftware in lmsss)
				{
					licenseMachineSoftware.IsDeleted = true;
					licenseMachineSoftware.DeletedOn = utcNow;
				}
				unitOfWork.LicenseMachineSoftwareRepository.SaveChanges();
				response.Status = UnLicenseSoftwareStatus.Success;
				return response;
			}
		}

		public LicenseMachineResponse LicenseMachine(LicenseMachineRequest request)
		{
			var response = new LicenseMachineResponse();

			var dbContext = dbFactory.Create();
			using (IUnitOfWork unitOfWork = new UnitOfWork(dbContext))
			{
				var machine =
					unitOfWork.MachineRepository.GetAll().SingleOrDefault(m => m.UniqueId == request.MachineId);

				if (machine == null)
				{
					response.Status = LicenseMachineStatus.MachineNotFound;
					return response;
				}

				var license =
					unitOfWork.LicenseRepository.GetAll().SingleOrDefault(m => m.UniqueId == request.LicenseId);
				if (license == null)
				{
					response.Status = LicenseMachineStatus.LicenseNotFound;
					return response;
				}
				var lsSoftwareIds = license.LicenseSoftwares.Select(ls => ls.SoftwareId);

				var machineSoftwares = unitOfWork.MachineSoftwareRepository.GetAll()
					.Where(ms => ms.MachineId == machine.Id)
					.Where(ms => lsSoftwareIds.Contains(ms.SoftwareId));


				if (!machineSoftwares.Any())
				{
					response.Status = LicenseMachineStatus.SoftwareOnMachineNotFound;
					return response;
				}
				if (machineSoftwares.All(ms => ms.LicenseMachineSoftwares.Any(lms => !lms.IsDeleted)))
				{
					response.Status = LicenseMachineStatus.SoftwareIsLinked;
					return response;
				}

                // license number check. is > 0
                var linkedLicenseCount = license.TotalUsedLicenseCount(new[] {machine.Id});

				if (linkedLicenseCount >= license.Count)
				{
					response.Status = LicenseMachineStatus.LicenseCountExceeded;
					return response;
				}

				var mss =
					machineSoftwares.Where(
						ms => !ms.LicenseMachineSoftwares.Any() || ms.LicenseMachineSoftwares.All(lms => lms.IsDeleted))
						.ToArray();

				var lmsss = mss.Select(
					machineSoftware => new LicenseMachineSoftware
					{
						MachineSoftware = machineSoftware,
						LicenseSoftwareId = license.LicenseSoftwares.Single(ls => ls.SoftwareId == machineSoftware.SoftwareId).Id,
						CreatedOn = DateTime.UtcNow
					}).ToArray();

				unitOfWork.LicenseMachineSoftwareRepository.AddRange(lmsss);

				unitOfWork.Save();
				response.Status = LicenseMachineStatus.Success;
				return response;
			}
		}

		public async Task<LicenseMachinesResponse> LicenseMachinesAsync(LicenseMachinesRequest request)
		{
			var licensedMachineRequest = new GetLicensedMachineRequest
			{
				Paging = new PagingModel
				{
					PageIndex = 0,
					PageSize = int.MaxValue
				},
				Ordering = new OrderModel(),
				LicenseId = request.LicenseId,
				SuIds = request.SuIds,
				LicensedMachineFilterType = LicensedMachineFilterType.None |
				                            LicensedMachineFilterType.PartialLicensed
			};
			var lmResponse = await GetLicensedMachineAsync(licensedMachineRequest);
			var licenseMachineUniqueIds = lmResponse.Model.Items.Select(m => m.MachineId);
			var response = new LicenseMachinesResponse();

			var dbContext = dbFactory.Create();
			using (IUnitOfWork unitOfWork = new UnitOfWork(dbContext))
			{
				var license = unitOfWork.LicenseRepository.GetAll().SingleOrDefault(m => m.UniqueId == request.LicenseId);
				var licenseMachineIds =
					unitOfWork.MachineRepository.GetAll()
						.Where(m => licenseMachineUniqueIds.Contains(m.UniqueId))
						.Select(m => m.Id)
						.ToArray();

                // license number check. is > 0
                var linkedLicenseCount = license.TotalUsedLicenseCount(licenseMachineIds);
				if (linkedLicenseCount + licenseMachineIds.Count() > license.Count)
				{
					response.Status = LicenseMachinesStatus.LicenseCountExceeded;
					return response;
				}

				var lsSoftwareIds = license.LicenseSoftwares.Select(ls => ls.SoftwareId);

				var dt = DateTime.UtcNow;
				var machineSoftwares1 = unitOfWork.MachineSoftwareRepository.GetAll()
					.Where(ms => licenseMachineIds.Contains(ms.MachineId))
					.Where(ms => lsSoftwareIds.Contains(ms.SoftwareId));

				var mss =
					machineSoftwares1.Where(
						ms => !ms.LicenseMachineSoftwares.Any() || ms.LicenseMachineSoftwares.All(lms => lms.IsDeleted)).ToArray();

				var lmsss = mss.Select(
					machineSoftware => new LicenseMachineSoftware
					{
						MachineSoftware = machineSoftware,
						LicenseSoftwareId = license.LicenseSoftwares.Single(ls => ls.SoftwareId == machineSoftware.SoftwareId).Id,
						CreatedOn = dt
					}).ToArray();

				unitOfWork.LicenseMachineSoftwareRepository.AddRange(lmsss);

				unitOfWork.Save();
				response.Status = LicenseMachinesStatus.Success;
				return response;
			}
		}

		public async Task<UnLicenseMachinesResponse> UnLicenseMachinesAsync(UnLicenseMachinesRequest request)
		{
			var response = new UnLicenseMachinesResponse();
			var utcNow = DateTime.UtcNow;
			var licensedMachineRequest = new GetLicensedMachineRequest
			{
				Paging = new PagingModel
				{
					PageIndex = 0,
					PageSize = int.MaxValue
				},
				Ordering = new OrderModel(),
				LicenseId = request.LicenseId,
				SuIds = request.SuIds,
				LicensedMachineFilterType = LicensedMachineFilterType.Licensed |
				                            LicensedMachineFilterType.PartialLicensed
			};
			var lmResponse = await GetLicensedMachineAsync(licensedMachineRequest);
			var licenseMachineIds = lmResponse.Model.Items.Select(m => m.MachineId);
			var dbContext = dbFactory.Create();
			using (IUnitOfWork unitOfWork = new UnitOfWork(dbContext))
			{
				var license =
					unitOfWork.LicenseRepository.GetAll().SingleOrDefault(m => m.UniqueId == request.LicenseId);

				if (license == null)
				{
					response.Status = UnLicenseMachinesStatus.LicenseNotFound;
					return response;
				}
				var licenseId = license.Id;

				var licenseMachineSoftwares = unitOfWork.LicenseMachineSoftwareRepository.GetAll()
					.Where(lms => lms.LicenseSoftware.LicenseId == licenseId)
					.Where(lms => licenseMachineIds.Contains(lms.MachineSoftware.Machine.UniqueId))
					.Where(lms => !lms.IsDeleted)
					.ToArray();

				foreach (var licenseMachineSoftware in licenseMachineSoftwares)
				{
					licenseMachineSoftware.IsDeleted = true;
					licenseMachineSoftware.DeletedOn = utcNow;
				}

				unitOfWork.LicenseMachineSoftwareRepository.SaveChanges();
				response.Status = UnLicenseMachinesStatus.Success;
				return response;
			}
		}

		public async Task<LicenseLicenseResponse> LicenseLicenseAsync(LicenseLicenseRequest request)
		{
			var licensedMachineRequest = new GetAvailableLicensesByMachineIdRequest
			{
				Paging = new PagingModel
				{
					PageIndex = 0,
					PageSize = int.MaxValue
				},
				Ordering = new OrderModel(),
				MachineId = request.MachineId,
				SuIds = request.SuIds,
				LicensedMachineFilterType = LicensedMachineFilterType.None |
				                            LicensedMachineFilterType.PartialLicensed
			};
			var lmResponse = await GetAvailableLicensesByMachineIdAsync(licensedMachineRequest);
			var machineLicenseIds = lmResponse.Model.Items.Select(m => m.LicenseId);
			var response = new LicenseLicenseResponse();

			var dbContext = dbFactory.Create();
			using (IUnitOfWork unitOfWork = new UnitOfWork(dbContext))
			{
				var machine =
					unitOfWork.MachineRepository.GetAll().SingleOrDefault(m => m.UniqueId == request.MachineId);

				var licenses = unitOfWork.LicenseRepository.GetAll().Where(l => machineLicenseIds.Contains(l.UniqueId));
				foreach (var license in licenses)
				{
                    // license number check. is > 0
                    var linkedLicenseCount = license.TotalUsedLicenseCount(new[] {machine.Id});
					if (linkedLicenseCount >= license.Count)
					{
						continue;
					}

					var lsSoftwareIds = license.LicenseSoftwares.Select(ls => ls.SoftwareId);

					var mss =
						machine.MachineSoftwares.Where(ms => lsSoftwareIds.Contains(ms.SoftwareId))
							.Where(
								ms => !ms.LicenseMachineSoftwares.Any() || ms.LicenseMachineSoftwares.All(lms => lms.IsDeleted))
							.ToArray();

					var lmsss = mss.Select(
						machineSoftware => new LicenseMachineSoftware
						{
							MachineSoftware = machineSoftware,
							LicenseSoftwareId = license.LicenseSoftwares.Single(ls => ls.SoftwareId == machineSoftware.SoftwareId).Id,
							CreatedOn = DateTime.UtcNow
						}).ToArray();

					unitOfWork.LicenseMachineSoftwareRepository.AddRange(lmsss);
				}

				unitOfWork.Save();
				response.Status = LicenseLicenseStatus.Success;
				return response;
			}
		}

		public async Task<UnLicenseLicensesResponse> UnLicenseLicensesAsync(UnLicenseLicensesRequest request)
		{
			var response = new UnLicenseLicensesResponse();
			var utcNow = DateTime.UtcNow;
			var availableLicensesByMachineIdRequest = new GetAvailableLicensesByMachineIdRequest
			{
				Paging = new PagingModel
				{
					PageIndex = 0,
					PageSize = int.MaxValue
				},
				Ordering = new OrderModel(),
				MachineId = request.MachineId,
				SuIds = request.SuIds,
				LicensedMachineFilterType = LicensedMachineFilterType.Licensed |
				                            LicensedMachineFilterType.PartialLicensed
			};
			var lmResponse = await GetAvailableLicensesByMachineIdAsync(availableLicensesByMachineIdRequest);
			var licenseLicenseIds = lmResponse.Model.Items.Select(m => m.LicenseId);
			var dbContext = dbFactory.Create();
			using (IUnitOfWork unitOfWork = new UnitOfWork(dbContext))
			{
				var machine =
					unitOfWork.MachineRepository.GetAll().SingleOrDefault(m => m.UniqueId == request.MachineId);

				if (machine == null)
				{
					response.Status = UnLicenseLicensesStatus.MachineNotFound;
					return response;
				}
				var machineId = machine.Id;

				var licenseMachineSoftwares =
					unitOfWork.LicenseMachineSoftwareRepository.GetAll()
						.Where(lms => lms.MachineSoftware.MachineId == machineId)
						.Where(lms => licenseLicenseIds.Contains(lms.LicenseSoftware.License.UniqueId))
						.Where(lms => !lms.IsDeleted).ToArray();

				foreach (var licenseMachineSoftware in licenseMachineSoftwares)
				{
					licenseMachineSoftware.IsDeleted = true;
					licenseMachineSoftware.DeletedOn = utcNow;
				}
				unitOfWork.LicenseMachineSoftwareRepository.SaveChanges();
			}

			response.Status = UnLicenseLicensesStatus.Success;
			return response;
		}


		public UnLicenseMachineResponse UnLicenseMachine(UnLicenseMachineRequest request)
		{
			var response = new UnLicenseMachineResponse();
			var utcNow = DateTime.UtcNow;
			var dbContext = dbFactory.Create();
			using (IUnitOfWork unitOfWork = new UnitOfWork(dbContext))
			{
				var machine =
					unitOfWork.MachineRepository.GetAll().SingleOrDefault(m => m.UniqueId == request.MachineId);

				if (machine == null)
				{
					response.Status = UnLicenseMachineStatus.MachineNotFound;
					return response;
				}

				var license =
					unitOfWork.LicenseRepository.GetAll().SingleOrDefault(m => m.UniqueId == request.LicenseId);

				if (license == null)
				{
					response.Status = UnLicenseMachineStatus.LicenseNotFound;
					return response;
				}
				var licenseId = license.Id;
				var machineId = machine.Id;

				var licenseMachineSoftwares = unitOfWork.LicenseMachineSoftwareRepository.GetAll()
				.Where(lms => lms.LicenseSoftware.LicenseId == licenseId)
				.Where(lms => lms.MachineSoftware.MachineId == machineId)
				.Where(lms => !lms.IsDeleted)
				.ToArray();

				if (licenseMachineSoftwares.Length==0)
				{
					response.Status = UnLicenseMachineStatus.SoftwareIsNotLinked;
					return response;
				}

				foreach (var licenseMachineSoftware in licenseMachineSoftwares)
				{
					licenseMachineSoftware.IsDeleted = true;
					licenseMachineSoftware.DeletedOn = utcNow;
				}
				unitOfWork.LicenseMachineSoftwareRepository.SaveChanges();
				response.Status = UnLicenseMachineStatus.Success;
				return response;
			}
		}

		public async Task<GetLicensedMachineResponse> GetLicensedMachineAsync(GetLicensedMachineRequest request)
		{
			var response = new GetLicensedMachineResponse();
			response.Model = new LicenseMachineCollection(request.Ordering.Order, request.Ordering.Sort);
			var dbContext = dbFactory.Create();
			using (IUnitOfWork unitOfWork = new UnitOfWork(dbContext))
			{
				var license = unitOfWork.LicenseRepository.GetAll().SingleOrDefault(s => s.UniqueId == request.LicenseId);
				if (license == null)
				{
					response.Status = GetLicensedMachineStatus.LicenseNotFound;
					return response;
				}
				response.Model.LicenseName = license.Name;

				var msQuery = unitOfWork.MachineSoftwareRepository.GetAll();
				var lmsQuery =
					unitOfWork.LicenseMachineSoftwareRepository.GetAll().Where(lms => !lms.IsDeleted);

				var join = unitOfWork.LicenseSoftwareRepository.GetAll()
					.Where(ls => ls.LicenseId == license.Id)
					.Join(msQuery, o => o.SoftwareId, i => i.SoftwareId, (o, i) => i);

                var leftJoinQuery = join.GroupJoin(lmsQuery, o => o.Id, i => i.MachineSoftware.Id,
					(o, i) => new {MachineSoftware = o, LicenseMachineSoftwares = i})
					.SelectMany(x => x.LicenseMachineSoftwares.DefaultIfEmpty(),
						(x, y) =>
							new MachineSoftwareLicenseMachineSoftware {MachineSoftware = x.MachineSoftware, LicenseMachineSoftware = y});

				// todo: optimize query : remove ToArrayAsync and use only IQueryable
				var leftJoinQueryArr = await leftJoinQuery.ToArrayAsync();
				var query = leftJoinQueryArr.GroupBy(a => a.MachineSoftware.Machine)
					.Where(g => !g.Key.IsDisabled).Where(g=> request.SuIds.Contains(g.Key.CurrentLinkedStructureUnit.UniqueId));
              
				Expression<Func<IGrouping<Machine, MachineSoftwareLicenseMachineSoftware>, bool>> where = (l => false);
				Expression<Func<IGrouping<Machine, MachineSoftwareLicenseMachineSoftware>, bool>> all =
					ms => ms.All(a => a.LicenseMachineSoftware != null && a.LicenseMachineSoftware.LicenseSoftware.LicenseId == license.Id);
				Expression<Func<IGrouping<Machine, MachineSoftwareLicenseMachineSoftware>, bool>> partial =
					ms => ms.Any(a => a.LicenseMachineSoftware != null && a.LicenseMachineSoftware.LicenseSoftware.LicenseId == license.Id) && ms.Any(a => a.LicenseMachineSoftware == null);
				Expression<Func<IGrouping<Machine, MachineSoftwareLicenseMachineSoftware>, bool>> none =
					ms => ms.All(a => a.LicenseMachineSoftware == null);

				var expressions = new List<Expression<Func<IGrouping<Machine, MachineSoftwareLicenseMachineSoftware>, bool>>>();
				if (request.LicensedMachineFilterType.HasFlag(LicensedMachineFilterType.None))
				{
					expressions.Add(none);
				}
				if (request.LicensedMachineFilterType.HasFlag(LicensedMachineFilterType.PartialLicensed))
				{
					expressions.Add(partial);
				}

				if (request.LicensedMachineFilterType.HasFlag(LicensedMachineFilterType.Licensed))
				{
					expressions.Add(all);
				}
				if (expressions.Count > 0)
				{
					where = ExpressionExtension.BuildOr(expressions);
				}

				var machinesQuery = query.AsQueryable().Where(where).Select(k => k.Key);

                var totalRecords = query.Count();
				IEnumerable<Machine> licenseMachines;
				if (string.IsNullOrEmpty(request.Ordering.Order) || request.Ordering.Order.ToLower() != "desc")
				{
					licenseMachines =
						machinesQuery.OrderBy(m => m.Name)
							.Skip(request.Paging.PageIndex*request.Paging.PageSize)
							.Take(request.Paging.PageSize);
				}
				else
				{
					licenseMachines =
						machinesQuery.OrderByDescending(m => m.Name)
							.Skip(request.Paging.PageIndex*request.Paging.PageSize)
							.Take(request.Paging.PageSize);
				}

				var licenseSoftwareIds = license.LicenseSoftwares.Select(ls => ls.SoftwareId);
				var items = licenseMachines.Select(
					m =>
					{
						var machine = MapperFromModelToView.MapToMachineModel<LicenseMachineModel>(m);

						var machineSoftwares = m.MachineSoftwares.Join(licenseSoftwareIds, o => o.SoftwareId, i => i, (o, i) => o);

						machine.LicensedMachineFilterType = machineSoftwares.GetLicensedMachineFilterType(license.Id);
						machine.Status = MapperFromModelToView.GetLicensedMachineFilterTypeEn(machine.LicensedMachineFilterType);
						return machine;
					}
					).ToArray();

				response.Model.Items = items;
				response.Model.TotalRecords = totalRecords;
				response.Status = GetLicensedMachineStatus.Success;
				return response;
			}
		}

		public async Task<GetAvailableLicensesByMachineIdResponse> GetAvailableLicensesByMachineIdAsync(
			GetAvailableLicensesByMachineIdRequest request)
		{
			var response = new GetAvailableLicensesByMachineIdResponse();
			response.Model = new MachineLicenseCollection(request.Ordering);
			var dbContext = dbFactory.Create();
			using (IUnitOfWork unitOfWork = new UnitOfWork(dbContext))
			{
				var sus = unitOfWork.StructureUnitRepository.GetAll().
					Where(o => request.SuIds.Contains(o.UniqueId));

                var susId = sus.Select(s => s.Id);
				var machine = unitOfWork.MachineRepository.GetAll().SingleOrDefault(m => m.UniqueId == request.MachineId);
				if (machine == null)
				{
					response.Status = GetAvailableLicensesByMachineIdStatus.NotFound;
					return response;
				}
				if (machine.IsDisabled)
				{
					response.Status = GetAvailableLicensesByMachineIdStatus.MachineIsDisabled;
					return response;
				}

				response.Model.MachineName = machine.Name;

				var softwareIdsOnMachine = machine.MachineSoftwares.Select(ms => ms.SoftwareId);

				var lmsQuery =
					unitOfWork.LicenseMachineSoftwareRepository.GetAll()
						.Where(lms => lms.MachineSoftware.MachineId == machine.Id && !lms.IsDeleted);

				var licensesSoftwares = unitOfWork.LicenseSoftwareRepository.GetAll()
					.Where(ls => susId.Contains(ls.License.StructureUnitId))
					.Where(ls => softwareIdsOnMachine.Contains(ls.SoftwareId));

				var leftJoinQuery = await licensesSoftwares.GroupJoin(lmsQuery, o => o.Id, i => i.LicenseSoftware.Id,
					(o, i) => new {LicenseSoftware = o, LicenseMachineSoftwares = i})
					.SelectMany(x => x.LicenseMachineSoftwares.DefaultIfEmpty(),
						(x, y) =>
							new LicenseSoftwareLicenseMachineSoftware {LicenseSoftware = x.LicenseSoftware, LicenseMachineSoftware = y}).ToArrayAsync();

				var query = leftJoinQuery.GroupBy(a => a.LicenseSoftware.License);

				Expression<Func<IGrouping<License, LicenseSoftwareLicenseMachineSoftware>, bool>> where = (l => false);
				Expression<Func<IGrouping<License, LicenseSoftwareLicenseMachineSoftware>, bool>> all =
					ms => ms.All(a => a.LicenseMachineSoftware != null);
				Expression<Func<IGrouping<License, LicenseSoftwareLicenseMachineSoftware>, bool>> partial =
					ms => ms.Any(a => a.LicenseMachineSoftware != null) && ms.Any(a => a.LicenseMachineSoftware == null);
				Expression<Func<IGrouping<License, LicenseSoftwareLicenseMachineSoftware>, bool>> none =
					ms => ms.All(a => a.LicenseMachineSoftware == null);

				var expressions = new List<Expression<Func<IGrouping<License, LicenseSoftwareLicenseMachineSoftware>, bool>>>();
				if (request.LicensedMachineFilterType.HasFlag(LicensedMachineFilterType.None))
				{
					expressions.Add(none);
				}
				if (request.LicensedMachineFilterType.HasFlag(LicensedMachineFilterType.PartialLicensed))
				{
					expressions.Add(partial);
				}

				if (request.LicensedMachineFilterType.HasFlag(LicensedMachineFilterType.Licensed))
				{
					expressions.Add(all);
				}
				if (expressions.Count > 0)
				{
					where = ExpressionExtension.BuildOr(expressions);
				}

				var licensesQuery = query.AsQueryable().Where(where).Select(k => k.Key);
				var totalRecords = query.Count();
				IEnumerable<License> machineLicenses;

				if (string.IsNullOrEmpty(request.Ordering.Order) || request.Ordering.Order.ToLower() != "desc")
				{
					machineLicenses =
						licensesQuery.OrderBy(m => m.Name)
							.Skip(request.Paging.PageIndex*request.Paging.PageSize)
							.Take(request.Paging.PageSize);
				}
				else
				{
					machineLicenses =
						licensesQuery.OrderByDescending(m => m.Name)
							.Skip(request.Paging.PageIndex*request.Paging.PageSize)
							.Take(request.Paging.PageSize);
				}

				var items = machineLicenses.Select(
					l =>
					{
						var license = MapperFromModelToView.MapToLicenseModel<MachineLicenseModel>(l);

						var licenseSoftwareIds = l.LicenseSoftwares.Select(ls => ls.SoftwareId);
						var machineSoftwares = machine.MachineSoftwares.Join(licenseSoftwareIds, o => o.SoftwareId, i => i, (o, i) => o);

						license.LicensedMachineFilterType = machineSoftwares.GetLicensedMachineFilterType(l.Id);

						license.Status = MapperFromModelToView.GetLicensedMachineFilterTypeEn(license.LicensedMachineFilterType);
						return license;
					}
					).ToArray();
				response.Model.Items = items;
				response.Model.TotalRecords = totalRecords;
				response.Status = GetAvailableLicensesByMachineIdStatus.Success;
				return response;
			}
		}

		#endregion
	}
}