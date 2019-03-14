using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.ResultHandling;

namespace VSS.Productivity3D.WebApi.Models.ProductionData.Contracts
{
        public interface IMachinesContract
        {
            Task<MachineExecutionResult> GetMachinesOnProject([FromRoute] long projectId);
            Task<ContractExecutionResult> GetMachineOnProject([FromRoute]long projectId, [FromRoute]long machineId);
        }
}
