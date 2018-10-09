using Microsoft.AspNetCore.Mvc;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.WebApi.Models.ProductionData.Models;

namespace VSS.Productivity3D.WebApi.Models.ProductionData.Contracts
{
  public interface ICellPassesContract
  {
    ContractExecutionResult Post([FromBody] CellPassesRequest request);
  }
}