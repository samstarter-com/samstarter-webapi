using System;

namespace SWI.SoftStock.ServerApps.WebApplicationModel
{
	public interface ILicenseModel
	{
		Guid LicenseId { get; set; }

		string Name { get; set; }

		string LicenseTypeName { get; set; }

		int Count { get; set; }

		int AvailableCount { get; set; }

		DateTime BeginDate { get; set; }

		DateTime ExpirationDate { get; set; }

		string Comments { get; set; }

		SoftwareModel[] LinkedSoftwares { get; set; }

		DocumentModel[] Documents { get; set; }

		AlertModel[] Alerts { get; set; }

		string StructureUnitName { get; set; }

		Guid StructureUnitId { get; set; }
	}
}