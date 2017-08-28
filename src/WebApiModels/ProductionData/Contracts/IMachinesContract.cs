using Microsoft.AspNetCore.Mvc;
using VSS.Common.ResultsHandling;
using VSS.Productivity3D.Common.Contracts;
using VSS.Productivity3D.WebApi.Models.ProductionData.ResultHandling;

namespace VSS.Productivity3D.WebApiModels.ProductionData.Contracts
{
        public interface IMachinesContract
        {
            MachineExecutionResult Get([FromRoute] long projectId);
            ContractExecutionResult Get([FromRoute]long projectId, [FromRoute]long machineId);
        }
}
