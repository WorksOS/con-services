using Microsoft.AspNetCore.Mvc;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.WebApi.Models.ProductionData.ResultHandling;

namespace VSS.Productivity3D.WebApi.Models.ProductionData.Contracts
{
        public interface IMachinesContract
        {
            MachineExecutionResult Get([FromRoute] long projectId);
            ContractExecutionResult Get([FromRoute]long projectId, [FromRoute]long machineId);
        }
}
