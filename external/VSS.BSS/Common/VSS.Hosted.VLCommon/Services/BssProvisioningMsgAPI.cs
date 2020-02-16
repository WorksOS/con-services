using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using log4net;
using VSS.Hosted.VLCommon;

namespace VSS.Hosted.VLCommon
{
  internal class BssProvisioningMsgAPI : IBssProvisioningMsgAPI
  {
    private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodInfo.GetCurrentMethod().DeclaringType);

    public BSSProvisioningMsg Create(INH_OP ctx, string messageType, long sequenceNumber, string senderIP, string xml, BSSStatusEnum status = BSSStatusEnum.Pending)
    {
      BSSProvisioningMsg message = new BSSProvisioningMsg();
      message.MessageXML = xml;
      message.InsertUTC = DateTime.UtcNow;
      message.UpdateUTC = DateTime.UtcNow;
      message.MachineName = Environment.MachineName;
      message.BSSMessageType = messageType;
      message.SequenceNumber = sequenceNumber;
      message.SenderIP = senderIP;
      message.FailedCount = 0;
      message.fk_BSSStatusID = (int)status;
      ctx.BSSProvisioningMsg.AddObject(message);
      ctx.SaveChanges();

      return message;
    }

    public bool Update(INH_OP ctx, BSSProvisioningMsg bssMessage, BSSStatusEnum status, DateTime? processedUTC = null, byte? failureCount = null, string machineName = null)
    {
      if (bssMessage != null)
      {
        bssMessage.fk_BSSStatusID = (int)status;
        bssMessage.UpdateUTC = DateTime.UtcNow;
        if (processedUTC.HasValue)
          bssMessage.ProcessedUTC = processedUTC;
        if (!string.IsNullOrEmpty(machineName))
          bssMessage.MachineName = machineName;
        if (failureCount.HasValue)
          bssMessage.FailedCount = failureCount.Value;
        return ctx.SaveChanges() > 0;
      }

      return false;
    }

    public BSSProvisioningMsg GetNextMessageToProcess(INH_OP ctx, DateTime currentTime, DateTime stuckMsgTimeout, Dictionary<int, double> retryDelays, int maxFailures)
    {      
      int pending = (int)BSSStatusEnum.Pending;
      int retry = (int)BSSStatusEnum.RetryPending;
      int inProgress = (int)BSSStatusEnum.InProgress;

      log.DebugFormat("Checking for any provisioning message currently in progress..");
      BSSProvisioningMsg msgInProgress = MessageCurrentlyInProgress(ctx, inProgress);

      //A message in progress?
      if (msgInProgress != null)
      {
        //Message not stuck? Then we do not need to process anything
        if (msgInProgress.UpdateUTC < stuckMsgTimeout)
        {
          log.DebugFormat("Provisioning Message with Sequence Number {0} is 'In Progress' but is not stuck.", msgInProgress.SequenceNumber);
          return null;
        }
        else   //Unstick it..
        {
          log.DebugFormat("Provisioning Message with Sequence Number {0} is 'In Progress'and is stuck. Unsticking it..", msgInProgress.SequenceNumber);
          //If processing has not failed, set it to 'pending', else set it to 'Retry Pending'
          if (msgInProgress.FailedCount == 0)
            msgInProgress.fk_BSSStatusID = pending;
          else
            msgInProgress.fk_BSSStatusID = retry;

          msgInProgress.UpdateUTC = DateTime.UtcNow;
          ctx.SaveChanges();
          return null;
        }        
      }

      log.DebugFormat("Checking for provisioning message to process..");
      //Get the first message that meets the following criteria
      //-Message is waiting to be Retried
      //-Message is Pending and has never been tried      
      BSSProvisioningMsg message = (from b in ctx.BSSProvisioningMsg                                   
                                    where b.InsertUTC <= currentTime                                    
                                    && ((b.FailedCount == 0 && b.fk_BSSStatusID == pending)
                                      || (b.fk_BSSStatusID == retry && b.fk_BSSStatusID < maxFailures))
                                    orderby b.SequenceNumber
                                    select b).FirstOrDefault();

      if (message != null)
      {        
        //Return the message if it is 'Pending'
        if (message.fk_BSSStatusID == pending)
        {
          log.DebugFormat("Found provisioning message with sequence number {0} in 'Pending' state", message.SequenceNumber);
          return message;
        }

        //If the message is in 'ReTry' state, check to see if it has hit the max wait time limit (based off the failed count)
        //before returning it.
        if ((message.ProcessedUTC.HasValue && message.ProcessedUTC.Value.AddSeconds(retryDelays[message.FailedCount]) <= DateTime.UtcNow)
              && message.fk_BSSStatusID == retry)
        {
          log.DebugFormat("Found provisioning message with sequence number {0} in 'Retry-Pending' state and Failed Count = {1}", message.SequenceNumber, message.FailedCount);
          return message;
        }
      }

      return null;
    }

    private BSSProvisioningMsg MessageCurrentlyInProgress(INH_OP ctx, int statusInProgress)
    {
      return (from b in ctx.BSSProvisioningMsg
              where b.fk_BSSStatusID == statusInProgress
              orderby b.SequenceNumber
              select b).FirstOrDefault();
    }

    # region VLSupportTool

    public BSSProvisioningMsg GetBssProvisioningMsg(long sequenceNo)
    {
      BSSProvisioningMsg result = new BSSProvisioningMsg();
      using (INH_OP opCtx = ObjectContextFactory.NewNHContext<INH_OP>(true))
      {
        result = (from proMsg in opCtx.BSSProvisioningMsgReadOnly
                  where proMsg.SequenceNumber == sequenceNo
                  select proMsg).FirstOrDefault();
      }

      return result;
    }

    # endregion

  }
}
