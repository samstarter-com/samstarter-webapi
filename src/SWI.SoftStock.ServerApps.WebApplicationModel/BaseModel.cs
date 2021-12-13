using SWI.SoftStock.ServerApps.WebApplicationModel.Attributes;
using System;
using System.Linq;

namespace SWI.SoftStock.ServerApps.WebApplicationModel
{
    public abstract class BaseModel<T>
	{
		public static SortModel[] GetSorting()
		{
			return typeof (T).GetProperties().Where(
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