using System.Collections.Generic;

namespace SWI.SoftStock.ServerApps.WebApplicationModel.Collections
{
	public abstract class BaseCollection<T> where T : BaseModel<T>
	{
		protected BaseCollection(string order, string sort)
		{
			SortedOrder = order;
			SortModels = BaseModel<T>.GetSorting();
			var sortModel = BaseModel<T>.GetSortModel(sort);
			SortedTableHeader = sortModel != null ? sortModel.SortName : string.Empty;

			Items = new T[0];
		}

		public int TotalRecords { get; set; }
		public IEnumerable<SortModel> SortModels { get; set; }
		public string SortedTableHeader { get; set; }
		public string SortedOrder { get; set; }
		public IEnumerable<T> Items { get; set; }
	}
}