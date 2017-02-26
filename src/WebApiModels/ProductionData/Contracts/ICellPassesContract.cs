using Microsoft.AspNetCore.Mvc;
using VSS.Raptor.Service.WebApiModels.ProductionData.Models;
using VSS.Raptor.Service.Common.Contracts;

namespace VSS.Raptor.Service.WebApiModels.ProductionData.Contracts
{
  public interface ICellPassesContract
  {
    ContractExecutionResult Post([FromBody] CellPassesRequest request);
  }
}