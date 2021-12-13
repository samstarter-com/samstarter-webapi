namespace SWI.SoftStock.ServerApps.WebApplicationContracts
{
    using SWI.SoftStock.ServerApps.WebApplicationModel;
    using CompanyService.Add;

    public interface ICompanyService
    {
        CompanyAddResponse Add(StructureUnitModel model);

        bool IsCompanyExists(string name);
    }
}