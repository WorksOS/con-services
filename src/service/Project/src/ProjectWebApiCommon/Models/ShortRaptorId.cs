using System.Net;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.MasterData.Project.WebAPI.Common.Models
{
  public class ShortRaptorId
  {
    public int ShortRaptorAssetId { get; set; }

    private ShortRaptorId()
    { }

    public ShortRaptorId(int shortRaptorAssetId)
    {
      ShortRaptorAssetId = shortRaptorAssetId;
    }

    public void Validate()
    { 
      if ( ShortRaptorAssetId < 1)
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(9999, "Invalid shortRaptorAssetId.")); // todoMaverick find a number an d message
    }
  }
}
