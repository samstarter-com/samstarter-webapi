using SWI.SoftStock.ServerApps.WebApplicationContracts.Statuses;

namespace SWI.SoftStock.WebApi.Mapper
{
    static class StructureUnitDeleteStatusMessage
    {       
            public static string GetErrorMessage(StructureUnitDeleteStatus status)
            {
                string result;
                switch (status)
                {
                    case StructureUnitDeleteStatus.HasChildStructureUnit:
                        result = Resources.StructureUnitDeleteStatus.StructureUnitDeleteStatus_HasChildStructureUnit;
                        break;
                case StructureUnitDeleteStatus.HasMachine:
                    result = Resources.StructureUnitDeleteStatus.StructureUnitDeleteStatus_HasMachine;
                    break;
                case StructureUnitDeleteStatus.HasUser:
                    result = Resources.StructureUnitDeleteStatus.StructureUnitDeleteStatus_HasUser;
                    break;
                case StructureUnitDeleteStatus.UnknownError:
                    result = Resources.StructureUnitDeleteStatus.StructureUnitDeleteStatus_UnknownError;
                    break;
                default:
                    result = Resources.StructureUnitDeleteStatus.StructureUnitDeleteStatus_UnknownError;
                    break;
            }
                return result;
            }
        
    }
}
