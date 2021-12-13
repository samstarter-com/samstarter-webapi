using Microsoft.EntityFrameworkCore;
using SWI.SoftStock.ServerApps.DataAccess.Common2;
using SWI.SoftStock.ServerApps.DataModel2;
using System.Linq;

namespace SWI.SoftStock.ServerApps.DataAccess2
{
    public class StructureUnitRepository : DbContextRepository<StructureUnit>
    {
        public StructureUnitRepository(DbContext context)
            : base(context)
        {
        }

        public override void Delete(StructureUnit entity)
        {
            var todeleteUsereRoles = Context.Set<StructureUnitUserRole>().Where(
                suur =>
                suur.StructureUnitId == entity.Id && (suur.Role.Name == "Admin" || suur.Role.Name == "Manager"));
            Context.Set<StructureUnitUserRole>().RemoveRange(todeleteUsereRoles);

            base.Delete(entity);
        }
    }
}