namespace SWI.SoftStock.ServerApps.WebApplicationModel.Collections
{
	public class LicenseMachineCollection : BaseCollection<LicenseMachineModel>
	{
		public LicenseMachineCollection(string order, string sort)
			: base(order, sort)
		{
		}

		public string LicenseName { get; set; }
	}
}