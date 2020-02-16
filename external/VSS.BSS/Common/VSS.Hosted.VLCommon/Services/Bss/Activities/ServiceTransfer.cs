using System;
using System.Collections.Generic;

using VSS.Hosted.VLCommon;

namespace VSS.Hosted.VLCommon.Bss
{
  public class ServiceTransfer : Activity
  {
    public const string COUNT_IS_ZERO = @"No Services Transferred to New IBKey: {0} from Old IBKey: {1}.";
    public const string SUCCESS_MESSAGE = @"{0} Servies Transferred.";
    public const string SERVICE_TRANSFER_INFO_TEMPLATE = @"Service Type: {0} Transferred to: {1}.";
    public const string FAILURE_MESSAGE = @"Failed to Transfer Services from Old IBKey: {0} to New IBkey: {1}.";

    public override ActivityResult Execute(Inputs inputs)
    {
      var context = inputs.Get<DeviceAssetContext>();
      List<Service> servicesTransferred;

      try
      {
        servicesTransferred = Services.ServiceViews().TransferServices
          (context.OldDeviceAsset.DeviceId,
          context.NewDeviceAsset.DeviceId,
          context.ActionUTC);

        if (servicesTransferred == null || servicesTransferred.Count == 0)
          return Warning(COUNT_IS_ZERO, context.NewIBKey, context.OldIBKey);
      }
      catch (Exception ex)
      {
        return Exception(ex, FAILURE_MESSAGE, context.OldIBKey, context.NewIBKey);
      }
      return Success(GetSuccessSummary(servicesTransferred, context.NewIBKey).ToNewLineString());
    }

    #region Private Methods

    private List<string> GetSuccessSummary(IList<Service> servicesTransferred, string newIBKey)
    {
      var summary = new List<string> { string.Format(SUCCESS_MESSAGE, servicesTransferred.Count) };

      foreach (var service in servicesTransferred)
      {
        summary.Add(string.Format(SERVICE_TRANSFER_INFO_TEMPLATE,
          (ServiceTypeEnum)service.fk_ServiceTypeID, newIBKey));
      }

      return summary;
    }
    #endregion

  }
}
