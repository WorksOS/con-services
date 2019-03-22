using System;
using System.Net;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.TRex.DI;
using VSS.TRex.SiteModels.Interfaces;

namespace VSS.TRex.Gateway.Common.Helpers
{
  public class GatewayHelper
  {
    public static ISiteModel ValidateAndGetSiteModel(Guid projectUid, bool createIfNotExists = false)
    {
      if (projectUid == Guid.Empty)
        throw new ServiceException(HttpStatusCode.BadRequest, new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "siteModel ID format is invalid"));

      return DIContext.Obtain<ISiteModels>().GetSiteModel(projectUid, createIfNotExists);
    }

    public static ISiteModel ValidateAndGetSiteModel(string projectUid, bool createIfNotExists = false)
    {
      if (string.IsNullOrEmpty(projectUid))
        throw new ServiceException(HttpStatusCode.BadRequest, new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "siteModel ID format is invalid"));

      return ValidateAndGetSiteModel(Guid.Parse(projectUid), createIfNotExists);
    }
  }
}
