using System;
using System.ComponentModel;
using System.Linq;
using SWI.SoftStock.ServerApps.WebApplicationModel.Attributes;

namespace SWI.SoftStock.ServerApps.WebApplicationModel
{
	[Sortable]
	public class LicenseUsageMachineModel : BaseModel<LicenseUsageMachineModel>, IMachineModel
	{
		[ReadOnly(true)]
		[Sort("thLicenseUsed")]
		public bool LicenseUsed { get; set; }

		[ReadOnly(true)]
		[Sort("thFrom")]
		public DateTime From { get; set; }

		[ReadOnly(true)]
		[Sort("thTo")]
		public DateTime To { get; set; }

		public Guid? UserId { get; set; }

		public Guid MachineId { get; set; }

		public Guid? StructureUnitId { get; set; }

		[Sort("thOperationSystemName")]
		public string OperationSystemName { get; set; }

		[Sort("thStructureUnitName")]
		public string StructureUnitName { get; set; }

		[Sort("thName")]
		public string Name { get; set; }

		[Sort("thDomainUserName")]
		public string DomainUserName { get; set; }

		[Sort("thDomainUserDomainName")]
		public string DomainUserDomainName { get; set; }

		[Sort("thLinkedUserName")]
		public string LinkedUserName { get; set; }

		[Sort("thCreatedOn")]
		public DateTime CreatedOn { get; set; }

		[Sort("thLastActivity")]
		public DateTime LastActivity { get; set; }

		/// <summary>
		///     Количесвто ПО установленного на машине
		/// </summary>
		[Sort("thTotalSoftwareCount")]
		public int TotalSoftwareCount { get; set; }

		/// <summary>
		///     Количесвто лицензированного ПО установленного на машине
		/// </summary>
		[Sort("thLicensedSoftwareCount")]
		public int LicensedSoftwareCount { get; set; }

		/// <summary>
		///     Количесвто не лецинзированного ПО установленного на машине
		/// </summary>
		[Sort("thUnLicensedSoftwareCount")]
		public int UnLicensedSoftwareCount { get; set; }

        [Sort("thExpiredLicensedSoftwareCount")]
        public int ExpiredLicensedSoftwareCount { get; set; }

		[Sort("thEnabled")]
		public bool Enabled { get; set; }

		public static SortModel[] GetSorting()
		{
			return typeof (LicenseUsageMachineModel).GetProperties().Where(
				prop => Attribute.IsDefined(prop, typeof (SortAttribute)))
				.Select(prop => new SortModel
				{
					PropertyName = prop.Name,
					SortName =
						((SortAttribute[]) prop.GetCustomAttributes(typeof (SortAttribute), true)).
							Single().Name
				}).ToArray();
		}

		public static SortModel GetSortModel(string sort)
		{
			return GetSorting().SingleOrDefault(
				sm => string.Equals(sm.PropertyName, sort, StringComparison.CurrentCultureIgnoreCase));
		}
	}
}