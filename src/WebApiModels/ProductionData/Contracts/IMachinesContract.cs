using Microsoft.AspNetCore.Mvc;
using VSS.Raptor.Service.WebApiModels.ProductionData.ResultHandling;
using VSS.Raptor.Service.Common.Contracts;

namespace VSS.Raptor.Service.WebApiModels.ProductionData.Contracts
{
        public interface IMachinesContract
        {
            MachineExecutionResult Get([FromRoute] long projectId);
            ContractExecutionResult Get([FromRoute]long projectId, [FromRoute]long machineId);
        }
}
