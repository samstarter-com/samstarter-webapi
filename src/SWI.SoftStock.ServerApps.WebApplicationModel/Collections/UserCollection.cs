namespace SWI.SoftStock.ServerApps.WebApplicationModel.Collections
{
	public class UserCollection : BaseCollection<UserModelEx>
	{
		public UserCollection(string order, string sort)
			: base(order, sort)
		{
		}
	}
}