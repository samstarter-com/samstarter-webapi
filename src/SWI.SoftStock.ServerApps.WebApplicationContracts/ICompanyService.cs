namespace SWI.SoftStock.ServerApps.WebApplicationContracts
{
    using CompanyService.Add;
    using SWI.SoftStock.ServerApps.WebApplicationModel;
    using System.Threading.Tasks;

    public interface ICompanyService
    {
        Task<CompanyAddResponse> Add(StructureUnitModel model);

        bool IsCompanyExists(string name);
    }
}