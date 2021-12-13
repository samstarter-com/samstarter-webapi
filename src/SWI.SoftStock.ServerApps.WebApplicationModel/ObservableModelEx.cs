using System;
using System.ComponentModel.DataAnnotations;
using SWI.SoftStock.ServerApps.WebApplicationModel.Attributes;

namespace SWI.SoftStock.ServerApps.WebApplicationModel
{
	[Sortable]
	public class ObservableModelEx : BaseModel<ObservableModelEx>
	{
		public Guid ObservableId { get; set; }

		[Display(Name = "Process name")]
		[Required(ErrorMessage = @"Process name is required")]
		[Sort("thProcessName")]
		public string ProcessName { get; set; }

		[Display(Name = "Software")]
		[Required(ErrorMessage = @"Observable process must be linked to a software")]
		public Guid? SoftwareId { get; set; }

		[Sort("thSoftwareName")]
		public string SoftwareName { get; set; }

		[Sort("thPublisherName")]
		public string PublisherName { get; set; }

		[Sort("thCreatedBy")]
		public string CreatedBy { get; set; }

		[Sort("thAppendedMachines")]
		public int AppendedMachines { get; set; }
	}
}