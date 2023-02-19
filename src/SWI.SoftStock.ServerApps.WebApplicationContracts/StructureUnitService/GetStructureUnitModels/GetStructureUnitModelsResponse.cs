using SWI.SoftStock.ServerApps.WebApplicationModel;
using System.Collections.Generic;

namespace SWI.SoftStock.ServerApps.WebApplicationContracts.StructureUnitService.GetStructureUnitModels
{
    public  class GetStructureUnitModelsResponse
    {
        public IEnumerable<StructureUnitTreeItemModel> StructureUnits { get; set; }
        public StructureUnitModel StructureUnit { get; set; }
    }
}
