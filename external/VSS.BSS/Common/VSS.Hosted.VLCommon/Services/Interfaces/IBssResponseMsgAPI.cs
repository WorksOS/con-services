
using System;
using System.Collections.Generic;
using VSS.Hosted.VLCommon;

namespace VSS.Hosted.VLCommon
{
  public interface IBssResponseMsgAPI
  {
    BSSResponseMsg Create(INH_OP ctx, long bssSequenceNumber, string responseXML, long responseEndpointID, string machineName, BSSStatusEnum status = BSSStatusEnum.Pending, DateTime? responseSentUTC = null);
    bool Update(INH_OP ctx, BSSResponseMsg message, BSSStatusEnum status, DateTime sentUTC, byte? failedCount = null);
    BSSResponseMsg GetNextMessageToSend(INH_OP ctx, DateTime currentTime, DateTime stuckMsgTimeout, Dictionary<int, double> retryDelays, int maxFailures);
    BSSResponseMsg GetBssResponseMsg(long sequenceNo);
  }
}
