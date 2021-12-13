using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using SWI.SoftStock.ServerApps.WebApplicationModel.Attributes;

namespace SWI.SoftStock.ServerApps.WebApplicationModel
{
	[Sortable]
	public class NewLicenseRequestModel : BaseModel<NewLicenseRequestModel>
	{
		[ReadOnly(true)]
		[Display(Name = "Machine")]
		[Sort("thMachineName")]
		public string MachineName { get; set; }

		public Guid MachineId { get; set; }

		[ReadOnly(true)]
		[Display(Name = "User")]
		[Sort("thUserName")]
		public string UserName { get; set; }

		public Guid UserId { get; set; }

		[Display(Name = "Users email")]
		[Sort("thUserEmail")]
		public string UserEmail { get; set; }

		[ReadOnly(true)]
		[Display(Name = "Software")]
		[Sort("thSoftwareName")]
		public string SoftwareName { get; set; }

		public Guid SoftwareId { get; set; }

		[ReadOnly(true)]
		[Display(Name = "Software publisher")]
		[Sort("thSoftwarePublisher")]
		public string SoftwarePublisher { get; set; }

		[DataType(DataType.MultilineText)]
		[Display(Name = "Request text")]
		[DisplayFormat(ConvertEmptyStringToNull = false)]
		[MaxLength(1000, ErrorMessage = "Text cannot be longer than 1000 characters")]
		[Sort("thText")]
		public string Text { get; set; }

		[ReadOnly(true)]
		[Display(Name = "Status")]
		[Sort("thStatus")]
		public string Status { get; set; }

	    public bool Sending { get; set; }
	}
}