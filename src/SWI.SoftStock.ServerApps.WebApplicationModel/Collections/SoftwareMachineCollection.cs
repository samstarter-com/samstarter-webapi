namespace SWI.SoftStock.ServerApps.WebApplicationModel.Collections
{
	public class SoftwareMachineCollection : BaseCollection<InstalledSoftwareMachineModel>
	{
		public SoftwareMachineCollection(string order, string sort) : base(order, sort)
		{
		}

		public string SoftwareName { get; set; }
	}
}