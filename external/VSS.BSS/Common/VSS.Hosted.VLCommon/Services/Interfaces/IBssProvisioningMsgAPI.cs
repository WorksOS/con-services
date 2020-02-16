using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using VSS.Hosted.VLCommon;

namespace VSS.Hosted.VLCommon
{
  public interface IBssProvisioningMsgAPI
  {
    BSSProvisioningMsg Create(INH_OP ctx, string messageType, long sequenceNumber, string senderIP, string xml, BSSStatusEnum status = BSSStatusEnum.Pending);
    bool Update(INH_OP ctx, BSSProvisioningMsg bssMessage, BSSStatusEnum status, DateTime? processedUTC, byte? failureCount = null, string machineName = null);
    BSSProvisioningMsg GetNextMessageToProcess(INH_OP ctx, DateTime currentTime, DateTime stuckMsgTimeout, Dictionary<int, double> retryDelays, int maxFailures);
    BSSProvisioningMsg GetBssProvisioningMsg(long sequenceNo);
  }
}
