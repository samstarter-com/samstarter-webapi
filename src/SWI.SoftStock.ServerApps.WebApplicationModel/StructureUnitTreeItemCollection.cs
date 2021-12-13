namespace SWI.SoftStock.ServerApps.WebApplicationModel
{
    using System.Collections.Generic;

    public class StructureUnitTreeItemCollection
    {
        public IEnumerable<StructureUnitTreeItemModel> Items { get; set; }

        public bool ShowIncludeSubItems { get; set; }

        public bool IncludeSubUnits { get; set; }
    }
}