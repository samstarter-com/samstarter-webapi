using SWI.SoftStock.ServerApps.WebApplicationModel.Common;

namespace SWI.SoftStock.ServerApps.WebApplicationModel.Collections
{
    public class PersonalMachineCollection : BaseCollection<PersonalMachineModel>
    {
        public PersonalMachineCollection(OrderModel order) : base(order.Order, order.Sort)
        {
        }
    }
}
