using Microsoft.AspNetCore.Mvc;
using VSS.Common.ResultsHandling;
using VSS.Productivity3D.Common.Contracts;
using VSS.Productivity3D.WebApiModels.ProductionData.Models;

namespace VSS.Productivity3D.WebApiModels.ProductionData.Contracts
{
  public interface ICellPassesContract
  {
    ContractExecutionResult Post([FromBody] CellPassesRequest request);
  }
}