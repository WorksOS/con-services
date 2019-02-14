using System.Net;
using System.Threading.Tasks;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Scheduler.Abstractions;
using VSS.Productivity3D.Scheduler.Jobs.DxfTileJob.Models;

namespace VSS.Productivity3D.Scheduler.Jobs.DxfTileJob
{
  /// <summary>
  /// Job to generate DXF tiles using Pegasus.
  /// </summary>
  public class DxfTileGenerationJob : IVSSJob
  {
    public Task Setup(object o)
    {
      return Task.FromResult(true);
    }

    public Task Run(object o)
    {
      var request = o as DxfTileGenerationRequest;
      if (request == null)
      {
        throw new ServiceException(HttpStatusCode.InternalServerError, 
          new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError,
          "Wrong parameters passed to DXF tile generation job"));
      }
      //Validate the parameters
      request.Validate();

      //TODO: for now dummy job - doesn't do anything. It will call the pegasus stuff.
      //TODO: Push service notification with zoom range

      return Task.FromResult(true);
    }

    public Task TearDown(object o)
    {
      return Task.FromResult(true);
    }
  }
}
