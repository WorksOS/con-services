
using System.Web.Http;
using VSS.Raptor.Service.WebApiModels.ProductionData.ResultHandling;
using VSS.Raptor.Service.Common.Contracts;


namespace VSS.Raptor.Service.WebApiModels.ProductionData.Contracts
{
        public interface IMachinesContract
        {
            MachineExecutionResult Get([FromUri] long projectId);
            ContractExecutionResult Get([FromUri]long projectId, [FromUri]long machineId);
        }
}
