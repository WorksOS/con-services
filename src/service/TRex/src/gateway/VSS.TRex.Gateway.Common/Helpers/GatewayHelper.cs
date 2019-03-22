using System;
using System.Net;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.TRex.DI;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.Storage.Interfaces;

namespace VSS.TRex.Gateway.Common.Helpers
{
  public class GatewayHelper
  {
    public static ISiteModel EnsureSiteModelExists(Guid projectUid)
    {
      var sm = DIContext.Obtain<ISiteModels>().GetSiteModel(projectUid, false);
      if (sm == null)
      {
        sm = DIContext.Obtain<ISiteModels>().GetSiteModel(projectUid, true);
        if (!sm.SaveMetadataToPersistentStore(DIContext.Obtain<IStorageProxyFactory>().MutableGridStorage()))
        {
          throw new ServiceException(HttpStatusCode.BadRequest, new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "Unable to local siteModel"));
        }
      }

      return sm;
    }

    public static ISiteModel EnsureSiteModelExists(string projectUid)
    {
      if (string.IsNullOrEmpty(projectUid) || Guid.Parse(projectUid) == Guid.Empty)
        throw new ServiceException(HttpStatusCode.BadRequest, new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "siteModel ID format is invalid"));

      var sm = EnsureSiteModelExists(Guid.Parse(projectUid));

      return sm;
    }
  }
}
